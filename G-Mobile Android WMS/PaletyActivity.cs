﻿using System;
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
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustPan
            | Android.Views.SoftInput.StateHidden
    )]
    public class PaletyActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView PaletyList;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        FloatingActionButton BtnSettings;
        TextView ItemCount;
        TextView AmountField;

        int IDMagazynu = -1;
        int IDKontrahenta = -1;
        int IDTowaru = -1;
        int IDFunkcjiLogistycznej = -1;
        int IDLokalizacji = -1;
        int IDPartii = -1;
        int IDDokumentu = -1;
        bool AskOnStart = false;
        bool Rozchód = false;
        string FilterText = "";

        internal static class Vars
        {
            public const string IDMagazynu = "IDMagazynu";
            public const string IDTowaru = "IDTowaru";
            public const string IDFunkcjiLogistycznej = "IDFunkcjiLogistycznej";
            public const string IDLokalizacji = "IDLokalizacji";
            public const string IDPartii = "IDPartii";
            public const string IDKontrahenta = "IDKontrahenta";
            public const string IDDokumentu = "IDDokumentu";
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
            SetContentView(Resource.Layout.activity_palety);

            IDMagazynu = Intent.GetIntExtra(Vars.IDMagazynu, -1);
            IDTowaru = Intent.GetIntExtra(Vars.IDTowaru, -1);
            IDFunkcjiLogistycznej = Intent.GetIntExtra(Vars.IDFunkcjiLogistycznej, -1);
            IDLokalizacji = Intent.GetIntExtra(Vars.IDLokalizacji, -1);
            IDPartii = Intent.GetIntExtra(Vars.IDPartii, -1);
            IDKontrahenta = Intent.GetIntExtra(Vars.IDKontrahenta, -1);
            IDDokumentu = Intent.GetIntExtra(Vars.IDDokumentu, -1);
            AskOnStart = Intent.GetBooleanExtra(Vars.AskOnStart, false);
            bool Bufor = Intent.GetBooleanExtra(Vars.Bufor, false);

            Rozchód = Intent.GetBooleanExtra(Vars.Rozchód, false);

            GetAndSetControls();

            if (Bufor)
            {
                Task.Run(() => InsertData());
            }
            else
            {
                if (AskOnStart)
                {
                    IsBusy = false;
                    Task.Run(() => ChangeFilter());
                }
                else
                    Task.Run(() => InsertData());
            }
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.palety_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.palety_button_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.palety_btn_refresh);
            Search = FindViewById<FloatingActionButton>(Resource.Id.palety_btn_search);
            BtnSettings = FindViewById<FloatingActionButton>(Resource.Id.palety_btn_settings);
            PaletyList = FindViewById<ListView>(Resource.Id.list_view_palety);
            ItemCount = FindViewById<TextView>(Resource.Id.palety_item_count);
            AmountField = FindViewById<TextView>(Resource.Id.palety_list_header_amount);

            if (!Rozchód)
                AmountField.Visibility = ViewStates.Invisible;

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;

            PaletyList.ItemClick += List_ItemClick;
            Search.Click += Search_Click;
            BtnSettings.Click += Settings_Click;
        }

        public void SelectPaletaAndCloseActivity(PaletaO Pal)
        {
            // czy tylko widok/podglad
            if (Globalne.CurrentSettings.DisableSSCCChange)
                return;

            Intent i = new Intent();
            i.PutExtra(Results.SelectedID, Pal.ID);
            i.PutExtra(Results.SelectedJSON, Newtonsoft.Json.JsonConvert.SerializeObject(Pal));

            SetResult(Result.Ok, i);
            Finish();
        }

        protected override async void OnScan(object sender, ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => ShowProgressAndDecodeBarcode(LastScanData[0]));
        }

        async Task ShowProgressAndDecodeBarcode(string Data)
        {
            try
            {
                Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

                await Task.Run(() => ParseBarcode(Data));

                Helpers.HideProgressDialog();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async Task<int> ParseBarcode(string Data)
        {
            PaletaO Kod = Serwer.paletaBL.PobierzPaletę(-1, Data);

            if (Kod.ID < 0)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.palety_not_found,
                    Resource.Raw.sound_error,
                    Resource.String.global_alert
                );
                return -1;
            }
            else
            {
                SelectPaletaAndCloseActivity(Kod);
                return -2;
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            UserSettings.ShowUnitSelectionDialog(this);
        }

        public override void OnSettingsChangedAsync()
        {
            Task.Run(() => InsertData());
        }

        async void Search_Click(object sender, EventArgs e)
        {
            await Task.Run(() => ChangeFilter());
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            RunIsBusyAction(() =>
            {
                try
                {
                    PaletaRowTerminal Selected = (PaletyList.Adapter as PaletyListAdapter)[
                        e.Position
                    ];
                    PaletaO Pal = Serwer.paletaBL.PobierzPaletę(Selected.ID, "");

                    if (Pal.ID != -1)
                        SelectPaletaAndCloseActivity(Pal);
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
                Resource.String.palety_filter,
                null,
                FilterText,
                InputType.Default
            );

            if (Res.Ok)
            {
                FilterText = Res.Text;
                await RunIsBusyTaskAsync(() => InsertData());
            }
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        async Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.palety_loading));

                List<PaletaRowTerminal> Palety = await Task.Factory.StartNew(() => GetData());

                RunOnUiThread(() =>
                {
                    PaletyList.Adapter = new PaletyListAdapter(this, Palety, IDTowaru);
                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + PaletyList.Adapter.Count.ToString()
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

        private List<PaletaRowTerminal> GetData()
        {
            List<PaletaRowTerminal> Lokalizacje =
                Serwer.paletaBL.PobierzListęDostępnychPaletNaTerminalZeStanemWedług(
                    IDMagazynu,
                    IDTowaru,
                    IDFunkcjiLogistycznej,
                    IDLokalizacji,
                    IDPartii,
                    Globalne.CurrentUserSettings.DisplayUnit,
                    IDKontrahenta,
                    IDDokumentu,
                    FilterText,
                    Rozchód,
                    ""
                );
            // usuniecie zduplikowanych wpisow z numerami SSCC
            return DistinctBy(Lokalizacje, x => x.strNazwa).ToList();

            //return Lokalizacje;
        }

        public IEnumerable<TSource> DistinctBy<TSource, TKey>(
            IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector
        )
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            this.Finish();
        }
    }

    internal class PaletyListAdapter : BaseAdapter<PaletaRowTerminal>
    {
        readonly List<PaletaRowTerminal> Items;
        readonly Activity Ctx;
        readonly int ArticleID;

        public PaletyListAdapter(Activity Ctx, List<PaletaRowTerminal> Items, int ArticleID)
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
            this.ArticleID = ArticleID;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override PaletaRowTerminal this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Partia = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_palety, null);

            view.FindViewById<TextView>(Resource.Id.palety_list_name).Text = Partia.strNazwa;

            if (ArticleID < 0)
                view.FindViewById<TextView>(Resource.Id.palety_list_amount).Visibility =
                    ViewStates.Invisible;
            else
                view.FindViewById<TextView>(Resource.Id.palety_list_amount).Text =
                    Partia.Zapas.ToString() + "(" + Partia.strNazwaJednostki.ToString() + ")";

            return view;
        }
    }
}
