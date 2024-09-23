using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Util;
using System.Collections.Generic;
using System.Threading;
using Android.Media;
using Android.Views.InputMethods;

using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;

using Acr.UserDialogs;

using WMSServerAccess.Model;
using Android.Content;
using System.Timers;
using System.Threading.Tasks;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, WindowSoftInputMode = Android.Views.SoftInput.AdjustPan | Android.Views.SoftInput.StateHidden)]
    public class LocationsActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView LocationsList;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        FloatingActionButton BtnSettings;
        TextView ItemCount;
        TextView AmountField;

        int IDMagazynu = -1;
        int IDKontrahenta = -1;
        int IDTowaru = -1;
        int IDFunkcjiLogistycznej = -1;
        int IDPartii = -1;
        int IDPalety = -1;
        int IDDokumentu = -1;
        bool AskOnStart = false;
        bool Rozchód = false;
        string FilterText = "";

        internal static class Vars
        {
            public const string IDMagazynu = "IDMagazynu";
            public const string IDTowaru = "IDTowaru";
            public const string IDFunkcjiLogistycznej = "IDFunkcjiLogistycznej";
            public const string IDPartii = "IDPartii";
            public const string IDPalety = "IDPalety";
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
            SetContentView(Resource.Layout.activity_locations);

            //IDMagazynu = Intent.GetIntExtra(Vars.IDMagazynu, -1);
            IDMagazynu = Globalne.Magazyn.ID;
            IDTowaru = Intent.GetIntExtra(Vars.IDTowaru, -1);
            IDFunkcjiLogistycznej = Intent.GetIntExtra(Vars.IDFunkcjiLogistycznej, -1);
            IDPartii = Intent.GetIntExtra(Vars.IDPartii, -1);
            IDPalety = Intent.GetIntExtra(Vars.IDPalety, -1);
            IDKontrahenta = Intent.GetIntExtra(Vars.IDKontrahenta, -1);
            IDDokumentu = Intent.GetIntExtra(Vars.IDDokumentu, -1);
            AskOnStart = Intent.GetBooleanExtra(Vars.AskOnStart, false);
            bool Bufor = Intent.GetBooleanExtra(Vars.Bufor, false);

            Rozchód = Intent.GetBooleanExtra(Vars.Rozchód, false);

            GetAndSetControls();

            if (Bufor)
            {
                Task.Run(() => InsertData(true));
            }
            else
            {
                if (AskOnStart)
                {
                    IsBusy = false;
                    Task.Run(() => ChangeFilter());
                }
                else
                    Task.Run(() => InsertData(false));
            }

        }


        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.locations_activity_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.locations_button_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.locations_btn_refresh);
            Search = FindViewById<FloatingActionButton>(Resource.Id.locations_btn_search);
            BtnSettings = FindViewById<FloatingActionButton>(Resource.Id.locations_btn_settings);
            LocationsList = FindViewById<ListView>(Resource.Id.list_view_locations);
            ItemCount = FindViewById<TextView>(Resource.Id.locations_item_count);
            AmountField = FindViewById<TextView>(Resource.Id.locations_list_header_amount);

            if (IDTowaru < 0)
            {
                FindViewById<TextView>(Resource.Id.locations_list_header_amount).Visibility = ViewStates.Gone;
                BtnSettings.Visibility = ViewStates.Gone;
                AmountField.Visibility = ViewStates.Gone;
            }


            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;

            LocationsList.ItemClick += List_ItemClick;
            Search.Click += Search_Click;
            BtnSettings.Click += Settings_Click;
        }

        public void SelectLocationAndCloseActivity(LokalizacjaVO Loc)
        {
            Intent i = new Intent();
            i.PutExtra(Results.SelectedID, Loc.ID);
            i.PutExtra(Results.SelectedJSON, Newtonsoft.Json.JsonConvert.SerializeObject(Loc));

            SetResult(Result.Ok, i);
            Finish();
        }

        async protected override void OnScan(object sender, ElapsedEventArgs e)
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
            LokalizacjaVO Kod = Globalne.lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(Data, Globalne.Magazyn.ID, true);

            if (Kod.ID < 0)
            {
                await Helpers.AlertAsyncWithConfirm(this, Resource.String.locations_not_found, Resource.Raw.sound_error, Resource.String.global_alert);
                return -1;
            }
            else
            {
                SelectLocationAndCloseActivity(Kod);
                return -2;
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            UserSettings.ShowUnitSelectionDialog(this);
        }

        public override void OnSettingsChangedAsync()
        {
            Task.Run(() => InsertData(false));
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
                    LokalizacjaRowTerminal Selected = (LocationsList.Adapter as LocationsListAdapter)[e.Position];
                    LokalizacjaVO Lok = Globalne.lokalizacjaBL.PobierzLokalizację(Selected.ID);

                    if (Lok.ID != -1)
                        SelectLocationAndCloseActivity(Lok);
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
            var Res = await Helpers.AlertAsyncWithPrompt(this, Resource.String.locations_filter, null, FilterText, InputType.Default);

            if (Res.Ok)
            {
                FilterText = Res.Text;
                await RunIsBusyTaskAsync(() => InsertData(false));
            }
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData(false));
        }

        async Task<bool> InsertData(bool Bufor)
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.locations_loading));

                List<LokalizacjaRowTerminal> Lokalizacje = await Task.Factory.StartNew(() => GetData(Bufor));

                RunOnUiThread(() =>
                {
                    LocationsList.Adapter = new LocationsListAdapter(this, Lokalizacje, IDTowaru);
                    Helpers.SetTextOnTextView(this, ItemCount, GetString(Resource.String.global_liczba_pozycji) + " " + LocationsList.Adapter.Count.ToString());
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

        private List<LokalizacjaRowTerminal> GetData(bool Bufor)
        {
            List<LokalizacjaRowTerminal> Lokalizacje = Globalne.lokalizacjaBL.
                PobierzListęDostępnychLokalizacjiZeStanemWedług(IDDokumentu,
                                                                IDMagazynu,
                                                                IDTowaru,
                                                                IDFunkcjiLogistycznej,
                                                                IDPartii,
                                                                IDPalety,
                                                                Globalne.CurrentUserSettings.DisplayUnit,
                                                                IDKontrahenta,
                                                                FilterText,
                                                                Bufor,
                                                                Rozchód,
                                                                true);


            return Lokalizacje;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            SetResult(Result.Canceled);
            this.Finish();
        }
    }

    internal class LocationsListAdapter : BaseAdapter<LokalizacjaRowTerminal>
    {
        readonly List<LokalizacjaRowTerminal> Items;
        readonly Activity Ctx;
        readonly int ArticleID;

        public LocationsListAdapter(Activity Ctx, List<LokalizacjaRowTerminal> Items, int ArticleID) : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
            this.ArticleID = ArticleID;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override LokalizacjaRowTerminal this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Lokalizacja = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_locations, null);

            view.FindViewById<TextView>(Resource.Id.locations_list_locationname).Text = Lokalizacja.strNazwa;
            view.FindViewById<TextView>(Resource.Id.locations_list_warehouse).Text = Lokalizacja.strNazwaMagazynu;

            if (ArticleID < 0)
                view.FindViewById<TextView>(Resource.Id.locations_list_amount).Visibility = ViewStates.Gone;
            else
                view.FindViewById<TextView>(Resource.Id.locations_list_amount).Text = Lokalizacja.Zapas.ToString() + "(" + Lokalizacja.strNazwaJednostki.ToString() + ")";

            view.FindViewById<LinearLayout>(Resource.Id.locations_list_item).SetBackgroundColor(Lokalizacja.Bufor ? Android.Graphics.Color.Aquamarine : Android.Graphics.Color.White);
            view.FindViewById<TextView>(Resource.Id.locations_list_status).SetBackgroundColor(Lokalizacja.Pełna ? Android.Graphics.Color.Yellow : Android.Graphics.Color.DarkGreen);

            return view;
        }

    }
}

