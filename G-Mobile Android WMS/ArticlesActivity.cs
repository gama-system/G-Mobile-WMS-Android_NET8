using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using G_Mobile_Android_WMS.Enums;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustPan
            | Android.Views.SoftInput.StateHidden
    )]
    public class ArticlesActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView ArticlesList;

        //TextView kodean;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        FloatingActionButton BtnSettings;
        TextView ItemCount;

        int IDMagazynu = -1;
        int IDKontrahenta = -1;
        int IDLokalizacji = -1;
        int IDFunkcjiLogistycznej = -1;
        int IDPartii = -1;
        int IDPalety = -1;
        int IDDokumentu = -1;
        int IDTowaru = -1;
        bool AskOnStart = false;
        bool Rozchód = false;

        string FilterText = "";

        internal static class Vars
        {
            public const string IDMagazynu = "IDMagazynu";
            public const string IDLokalizacji = "IDLokalizacji";
            public const string IDFunkcjiLogistycznej = "IDFunkcjiLogistycznej";
            public const string IDPartii = "IDPartii";
            public const string IDPalety = "IDPalety";
            public const string IDKontrahenta = "IDKontrahenta";
            public const string IDDokumentu = "IDDokumentu";
            public const string IDTowaru = "IDTowaru";
            public const string AskOnStart = "AskOnStart";
            public const string Bufor = "Bufor";
            public const string Rozchód = "Rozchód";
        }

        internal static class Results
        {
            public const string SelectedID = "SelectedID";
            public const string SelectedJSON = "SelectedJSON";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_article);

            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[Enums.DocTypes.RW];

            IDMagazynu = Intent.GetIntExtra(Vars.IDMagazynu, -1);
            IDLokalizacji = Intent.GetIntExtra(Vars.IDLokalizacji, -1);
            IDFunkcjiLogistycznej = Intent.GetIntExtra(Vars.IDFunkcjiLogistycznej, -1);
            IDPartii = Intent.GetIntExtra(Vars.IDPartii, -1);
            IDPalety = Intent.GetIntExtra(Vars.IDPalety, -1);
            IDKontrahenta = Intent.GetIntExtra(Vars.IDKontrahenta, -1);
            IDDokumentu = Intent.GetIntExtra(Vars.IDDokumentu, -1);
            IDTowaru = Intent.GetIntExtra(Vars.IDTowaru, -1);
            AskOnStart = Intent.GetBooleanExtra(Vars.AskOnStart, false);
            Rozchód = Intent.GetBooleanExtra(Vars.Rozchód, false);

            GetAndSetControls();

            if (IDTowaru > -1)
            {
                if (Serwer.towarBL.PobierzTowar(IDTowaru).strSymbol.Length > 0)
                    FilterText = Serwer.towarBL.PobierzTowar(IDTowaru).strSymbol;
            }

            if (AskOnStart)
            {
                IsBusy = false;
                System.Threading.Tasks.Task.Run(() => ChangeFilter());
            }
            else
                System.Threading.Tasks.Task.Run(() => InsertData(FilterText));
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.article_activity_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.articles_button_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.articles_btn_refresh);
            Search = FindViewById<FloatingActionButton>(Resource.Id.articles_btn_search);
            BtnSettings = FindViewById<FloatingActionButton>(Resource.Id.articles_btn_settings);
            ArticlesList = FindViewById<ListView>(Resource.Id.list_view_articles);
            ItemCount = FindViewById<TextView>(Resource.Id.articles_item_count);

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
            ArticlesList.ItemClick += List_ItemClick;

            Search.Click += Search_Click;
            BtnSettings.Click += Settings_Click;
        }

        public void SelectArticleAndCloseActivity(TowarVO t)
        {
            Intent i = new Intent();
            i.PutExtra(Results.SelectedID.ToString(), t.ID);
            i.PutExtra(
                Results.SelectedJSON.ToString(),
                Newtonsoft.Json.JsonConvert.SerializeObject(t)
            );

            SetResult(Result.Ok, i);
            Finish();
        }

        protected override async void OnScan(object sender, ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => ShowProgressAndDecodeBarcode(LastScanData));
        }

        async System.Threading.Tasks.Task ShowProgressAndDecodeBarcode(List<string> Data)
        {
            try
            {
                Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

                await System.Threading.Tasks.Task.Run(() => ParseBarcode(Data));

                Helpers.HideProgressDialog();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async System.Threading.Tasks.Task<int> ParseBarcode(List<string> Barcodes)
        {
            KodKreskowyZSzablonuO Kod = Helpers.ParseBarcodesAccordingToOrder(
                Barcodes,
                Enums.DocTypes.RW
            );

            if (Kod.TowaryJednostkiWBazie.Count == 0)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.articles_not_found,
                    Resource.Raw.sound_error,
                    Resource.String.global_alert
                );
                return -1;
            }
            else if (Kod.TowaryJednostkiWBazie.Count != 1)
            {
                TowarJednostkaO TwJedn =
                    await BusinessLogicHelpers.Indexes.SelectOneArticleFromMany(
                        this,
                        Kod.TowaryJednostkiWBazie
                    );

                if (TwJedn.IDTowaru < 0)
                    return -1;
                else
                {
                    TowarVO T = Serwer.towarBL.PobierzTowar(TwJedn.IDTowaru);

                    if (T.ID != -1)
                        SelectArticleAndCloseActivity(T);

                    return -2;
                }
            }
            else
            {
                TowarVO T = Serwer.towarBL.PobierzTowar(Kod.TowaryJednostkiWBazie[0].IDTowaru);

                if (T.ID != -1)
                    SelectArticleAndCloseActivity(T);

                SelectArticleAndCloseActivity(T);
                return -3;
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            UserSettings.ShowUnitSelectionDialog(this);
        }

        public override void OnSettingsChangedAsync()
        {
            System.Threading.Tasks.Task.Run(() => InsertData(FilterText));
        }

        async void Search_Click(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => ChangeFilter());
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            RunIsBusyAction(() =>
            {
                try
                {
                    TowarRowTerminal Selected = (ArticlesList.Adapter as ArticlesListAdapter)[
                        e.Position
                    ];
                    TowarVO T = (TowarVO)Serwer.towarBL.PobierzTowar(Selected.ID);

                    if (T.ID != -1)
                        SelectArticleAndCloseActivity(T);
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            });
        }

        async void ChangeFilter()
        {
            var Res = await Helpers.AlertAsyncWithPrompt(
                this,
                Resource.String.documents_filter,
                null,
                FilterText,
                InputType.Default
            );

            if (Res.Ok)
            {
                FilterText = Res.Text;
                await RunIsBusyTaskAsync(() => InsertData(FilterText));
            }
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => InsertData(FilterText));
        }

        async System.Threading.Tasks.Task<bool> InsertData(string Filtr)
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.articles_loading));

                List<TowarRowTerminal> Towary = await System.Threading.Tasks.Task.Factory.StartNew(
                    () => GetData(Filtr)
                );

                RunOnUiThread(() =>
                {
                    ArticlesList.Adapter = new ArticlesListAdapter(this, Towary);
                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + ArticlesList.Adapter.Count.ToString()
                    );
                });

                Helpers.HideProgressDialog();

                return true;
            }
            catch (Exception ex)
            {
                Helpers.HideProgressDialog();
                Helpers.HandleError(this, ex);

                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<TowarRowTerminal> GetData(string Filtr)
        {
            List<TowarRowTerminal> Towary =
                Serwer.towarBL.PobierzListęDostępnychTowarówZeStanemWedług(
                    IDDokumentu,
                    IDMagazynu,
                    IDLokalizacji,
                    IDFunkcjiLogistycznej,
                    IDPartii,
                    IDPalety,
                    Globalne.CurrentUserSettings.DisplayUnit,
                    IDKontrahenta,
                    Filtr,
                    Rozchód
                );

            return Towary;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            this.Finish();
        }
    }

    internal class ArticlesListAdapter : BaseAdapter<TowarRowTerminal>
    {
        readonly List<TowarRowTerminal> Items;
        readonly Activity Ctx;

        public ArticlesListAdapter(Activity Ctx, List<TowarRowTerminal> Items)
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override TowarRowTerminal this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Towar = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_articles, null);

            view.FindViewById<TextView>(Resource.Id.articles_list_name).Text =
                String.Concat(Towar.strSymbol) + " - " + String.Concat(Towar.strNazwa);
            view.FindViewById<TextView>(Resource.Id.articles_list_amount).Text = String.Concat(
                Towar.Zapas >= 0 ? Towar.Zapas.ToString() : "---",
                " (",
                Towar.strJednostkaMiary,
                " )"
            );

            return view;
        }
    }
}
