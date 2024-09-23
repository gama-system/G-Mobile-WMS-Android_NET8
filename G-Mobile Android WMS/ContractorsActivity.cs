using System;
using System.Collections.Generic;
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
using G_Mobile_Android_WMS.SQL;
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
    public class ContractorsActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView ContractorsList;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        TextView ItemCount;
        ListView DETALContractor;
        ListView QuickMM;

        bool AskOnStart = false;

        string FilterText = "";

        internal static class Vars
        {
            public const string AskOnStart = "AskOnStart";
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
            SetContentView(Resource.Layout.activity_contractors);

            AskOnStart = Intent.GetBooleanExtra(Vars.AskOnStart, false);

            GetAndSetControls();

            if (AskOnStart)
            {
                IsBusy = false;
                System.Threading.Tasks.Task.Run(() => ChangeFilter());
            }
            else
                System.Threading.Tasks.Task.Run(() => InsertData());
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.contractors_activity_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.contractors_button_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.contractors_btn_refresh);
            Search = FindViewById<FloatingActionButton>(Resource.Id.contractors_btn_search);
            ContractorsList = FindViewById<ListView>(Resource.Id.list_view_contractors);
            ItemCount = FindViewById<TextView>(Resource.Id.contractors_item_count);
            DETALContractor = FindViewById<ListView>(Resource.Id.creating_documents_btn_DETAL);
            QuickMM = FindViewById<ListView>(Resource.Id.creating_documents_btn_QuickMM);

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
            ContractorsList.ItemClick += List_ItemClick;
            Search.Click += Search_Click;
        }

        async void Search_Click(object sender, EventArgs e)
        {
            await System.Threading.Tasks.Task.Run(() => ChangeFilter());
        }

        public void SelectLocationAndCloseActivity(KontrahentVO Ktr)
        {
            Intent i = new Intent();
            i.PutExtra(Results.SelectedID, Ktr.ID);
            i.PutExtra(Results.SelectedJSON, Newtonsoft.Json.JsonConvert.SerializeObject(Ktr));

            SetResult(Result.Ok, i);
            Finish();
        }

        protected override async void OnScan(object sender, ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => ShowProgressAndDecodeBarcode(LastScanData[0]));
        }

        async System.Threading.Tasks.Task ShowProgressAndDecodeBarcode(string Data)
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

        private async System.Threading.Tasks.Task<int> ParseBarcode(string Data)
        {
            KontrahentVO Kod = Serwer.podmiotBL.PobierzKontrahentaWgKodu(Data);

            if (Kod.ID < 0)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.contractors_not_found,
                    Resource.Raw.sound_error,
                    Resource.String.global_alert
                );
                return -1;
            }
            else
            {
                SelectLocationAndCloseActivity(Kod);
                return -2;
            }
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            RunIsBusyAction(() =>
            {
                try
                {
                    object[] Selected = (ContractorsList.Adapter as ContractorsListAdapter)[
                        e.Position
                    ];
                    int IDKontrahenta = Convert.ToInt32(
                        Selected[(int)Contractors.Contractors_Results.idKontrahenta]
                    );

                    KontrahentVO Ktr = Serwer.podmiotBL.PobierzKontrahenta(IDKontrahenta);

                    if (Ktr.ID != -1 && Ktr.bAktywny && !Ktr.bZablokowany)
                        SelectLocationAndCloseActivity(Ktr);
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
                Resource.String.contractors_filter,
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

        async System.Threading.Tasks.Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.articles_loading));

                ZapytanieZTabeliO Kontrahenci = await System.Threading.Tasks.Task.Factory.StartNew(
                    () => GetData()
                );

                if (Kontrahenci.Poprawność != null && Kontrahenci.Poprawność != "")
                    throw new Exception(Kontrahenci.Poprawność);

                RunOnUiThread(() =>
                {
                    ContractorsList.Adapter = new ContractorsListAdapter(
                        this,
                        Kontrahenci.ListaWierszy
                    );
                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + ContractorsList.Adapter.Count.ToString()
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

        private ZapytanieZTabeliO GetData()
        {
            string Komenda = SQL.Contractors.GetContactors.Replace("<<FILTR>>", FilterText);

            ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)
                Helpers.HiveInvoke(
                    typeof(WMSServerAccess.Ogólne.OgólneBL),
                    "ZapytanieSQL",
                    Komenda
                );
            return Zap;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            this.Finish();
        }
    }

    internal class ContractorsListAdapter : BaseAdapter<object[]>
    {
        readonly List<object[]> Items;
        readonly Activity Ctx;

        public ContractorsListAdapter(Activity Ctx, List<object[]> Items)
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override object[] this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Pos = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_contractors, null);

            view.FindViewById<TextView>(Resource.Id.contractors_list_contractorname).Text = (string)
                Pos[(int)SQL.Contractors.Contractors_Results.strNazwa];
            view.FindViewById<TextView>(Resource.Id.contractors_list_contractorsymbol).Text =
                (string)Pos[(int)SQL.Contractors.Contractors_Results.strSymbol];

            int IDKontrahenta = Convert.ToInt32(
                Pos[(int)SQL.Contractors.Contractors_Results.idKontrahenta]
            );
            int IDKontrahentaNadrz = Convert.ToInt32(
                Pos[(int)SQL.Contractors.Contractors_Results.idKontrahentaNadrz]
            );

            view.FindViewById<LinearLayout>(Resource.Id.contractors_list_item)
                .SetBackgroundColor(
                    IDKontrahenta == IDKontrahentaNadrz
                        ? Android.Graphics.Color.Wheat
                        : Android.Graphics.Color.White
                );

            return view;
        }
    }
}
