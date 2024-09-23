using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using WMSServerAccess.Model;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, MainLauncher = false)]

    public class StocksActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton Search;
        FloatingActionButton Settings;
        ListView StocksList;
        FloatingActionButton Refresh;
        TextView ItemCount;
        TextView ItemSum;

        KodKreskowyZSzablonuO Kod = null;
        int IDLokalizacji = -1;
        int IDTowaru;
        static bool blokadaTowaru = false;
        public static bool ExitToModules = false;

        public enum ResultCodes
        {
            ArticlesActivityResult = 10,
            LocationsActivityResult = 20,
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_stock);
            GetAndSetControls();

            BarcodeOrder = new List<int>() { Enums.BarcodeOrder.Template };

            IDLokalizacji = Intent.GetIntExtra("IDLokalizacji", -1);
            IDTowaru = Intent.GetIntExtra("IDTowaru", -1);
            
            if (IDTowaru != -1)
            {
                Kod = new KodKreskowyZSzablonuO();

                Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>();

                Kod.TowaryJednostkiWBazie.Add(new TowarJednostkaO() { IDTowaru = IDTowaru, IDJednostki = 0 });

            }

            // robimy odczyt z bazy wtedy gdy został wybrany towar i nie nastapila 'blokadaTowaru'
            // słuzy do zaczytywania wybranego towaru na liscie - np pozycja dokumentu
            if (IDTowaru != -1 && !blokadaTowaru)
            {
                System.Threading.Tasks.Task.Run(() => InsertData());
                blokadaTowaru = true;
            }
            IsBusy = false;
        }
       
        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.stocks_activity_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.stocks_button_prev);
            Settings = FindViewById<FloatingActionButton>(Resource.Id.stocks_button_settings);
            Search = FindViewById<FloatingActionButton>(Resource.Id.stocks_button_search);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.stocks_button_refresh);
            StocksList = FindViewById<ListView>(Resource.Id.list_view_stocks);
            ItemSum = FindViewById<TextView>(Resource.Id.stocks_sum);
            ItemCount = FindViewById<TextView>(Resource.Id.stocks_item_count);

            Back.Click += Back_Click;
            Settings.Click += Settings_Click;
            Search.Click += Search_Click;
            Refresh.Click += Refresh_Click;
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        private void Search_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                ActionSheetConfig Conf = new ActionSheetConfig()
                                       .SetCancel(GetString(Resource.String.global_cancel))
                                       .SetTitle(GetString(Resource.String.stocks_search_by_what));

                Conf.Add(GetString(Resource.String.stocks_search_by_article), () => SelectArticle());
                Conf.Add(GetString(Resource.String.stocks_search_by_location), () => SelectLocation());

                UserDialogs.Instance.ActionSheet(Conf);
            });
        }

        private void SelectArticle()
        {
            if (IsSwitchingActivity)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent(this, typeof(ArticlesActivity));
            i.PutExtra(ArticlesActivity.Vars.AskOnStart, true);

            RunOnUiThread(() => StartActivityForResult(i, (int)ResultCodes.ArticlesActivityResult));
        }

        private void SelectLocation()
        {
            if (IsSwitchingActivity)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent(this, typeof(LocationsActivity));
            i.PutExtra(LocationsActivity.Vars.AskOnStart, true);

            RunOnUiThread(() => StartActivityForResult(i, (int)ResultCodes.LocationsActivityResult));
        }

        int IDWybraneArticlesActivityResult = -1;
        async protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ResultCodes.ArticlesActivityResult && resultCode == Result.Ok)
            {
                IDWybraneArticlesActivityResult = data.GetIntExtra(ArticlesActivity.Results.SelectedID.ToString(), -1);

                if (IDWybraneArticlesActivityResult != -1)
                {
                    Kod = new KodKreskowyZSzablonuO();

                    Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>();

                    Kod.TowaryJednostkiWBazie.Add(new TowarJednostkaO() { IDTowaru = IDWybraneArticlesActivityResult, IDJednostki = 0 });

                    IDLokalizacji = -1;
                    if (IDWybraneArticlesActivityResult > -1)
                        // istnieje takze w OnCreate
                        await System.Threading.Tasks.Task.Run(() => InsertData());
                    

                }
            }
            else if (requestCode == (int)ResultCodes.LocationsActivityResult && resultCode == Result.Ok)
            {
                int IDWybrane = data.GetIntExtra(LocationsActivity.Results.SelectedID, -1);

                if (IDWybrane != -1)
                {
                    Kod = null;
                    IDLokalizacji = IDWybrane;
                    if (IDWybrane > -1)
                        // istnieje takze w OnCreate
                        await System.Threading.Tasks.Task.Run(() => InsertData());
                }
            }
        }

        public override async void OnSettingsChangedAsync()
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() => UserSettings.ShowUnitSelectionDialog(this));
        }

        private void Back_Click(object sender, EventArgs e)
        {
            if (ExitToModules)
            {
                RunIsBusyAction(() =>
                {
                    if (CallingActivity == null)
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(ModulesActivity));
                });
                ExitToModules = false;
            }
            else
            {
                blokadaTowaru = false;
                SetResult(Result.Canceled);
                this.Finish();
            }
        }

        async protected override void OnScan(object sender, ElapsedEventArgs e)
        { 
            //IDTowaru = -1;
            //IDLokalizacji = -1;
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => ShowProgressAndDecodeBarcode(LastScanData));
        }

        async System.Threading.Tasks.Task ShowProgressAndDecodeBarcode(List<string> Data)
        {
            try
            {
                Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

                int Ret = await System.Threading.Tasks.Task.Run(() => ParseBarcode(Data));

                Helpers.HideProgressDialog();

                if (Ret == 0)
                {
                    await Helpers.AlertAsyncWithConfirm(this, Resource.String.stocks_article_or_location_not_found, Resource.Raw.sound_error);
                    return;
                }
                
                await System.Threading.Tasks.Task.Run(() => InsertData());
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async System.Threading.Tasks.Task<int> ParseBarcode(List<string> Data)
        {
            LokalizacjaVO KodL = Globalne.lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(Data[0], Globalne.Magazyn.ID, true);

            if (KodL.ID >= 0)
            {
                Kod = null;
                IDLokalizacji = KodL.ID;
                return 0;
            }
            else
            {
                KodKreskowyZSzablonuO KodT = Helpers.ParseBarcodesAccordingToOrder(Data, Enums.DocTypes.RW);

                if (Globalne.CurrentSettings.GetDataFromFirstSSCCEntry && KodT.Paleta != "")
                    BusinessLogicHelpers.DocumentItems.InsertSSCCData(ref KodT);

                int idTowaru = -1;

                if (KodT.TowaryJednostkiWBazie != null && KodT.TowaryJednostkiWBazie.Count > 1)
                {
                    TowarJednostkaO TowarJedn = await BusinessLogicHelpers.Indexes.SelectOneArticleFromMany(this, KodT.TowaryJednostkiWBazie);

                    if (TowarJedn.IDTowaru < 0)
                        return 1;
                    else
                    {
                        idTowaru = TowarJedn.IDTowaru;
                        Kod = new KodKreskowyZSzablonuO();
                        Kod.Towar = KodT.Towar;
                        Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>() { TowarJedn };
                    }
                }
                if (idTowaru < 0)
                    Kod = KodT;

                if (Kod.TowaryJednostkiWBazie.Count == 0)
                    return 0;

                IDLokalizacji = -1;

                return 1;
            }
        }
        async System.Threading.Tasks.Task<bool> InsertData()
        {
            try
            {
                //await Task.Delay(Globalne.TaskDelay);
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.stocks_loading));
                
                ZapytanieZTabeliO PozStan = await Task.Factory.StartNew(() => GetData());
                await Task.Factory.StartNew(() => GetDataBufor(PozStan));

                if (PozStan.Poprawność != null && PozStan.Poprawność != "")
                    throw new Exception(PozStan.Poprawność);

                

                RunOnUiThread(() =>
                {
                    StocksList.Adapter = new StocksListAdapter(this, PozStan.ListaWierszy);
                    Helpers.SetTextOnTextView(this, ItemCount, GetString(Resource.String.global_liczba_pozycji) + " " + StocksList.Adapter.Count.ToString());

                    Helpers.SetTextOnTextView(this, ItemSum, GetString(Resource.String.global_suma_pozycji) + " " + Math.Round((StocksList.Adapter as StocksListAdapter).Sum, Globalne.CurrentSettings.DecimalSpaces).ToString("F3"));
                    ItemSum.Visibility = ViewStates.Visible;

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
            string Komenda;

            string Pal = "";
            string Par = "";

            var idTow = Kod?.TowaryJednostkiWBazie[0]?.IDTowaru == null ? "-1" : Kod?.TowaryJednostkiWBazie[0]?.IDTowaru.ToString();


            if (Kod != null && Kod.Paleta != null && Kod.Paleta != "")
                Pal = Globalne.paletaBL.PobierzIDPalety(Kod.Paleta).ToString();

            if (Kod != null && Kod.Partia != null && Kod.Partia != "")
                Par = Globalne.partiaBL.PobierzIDPartii(Kod.Partia).ToString();


            string Sum =
                SQL.Stocks.GetStocks_Where_Mag.Replace("<<ID_MAG>>", Globalne.Magazyn.ID.ToString()) + 

                (
                
                (Kod != null && Kod.TowaryJednostkiWBazie != null && Kod.TowaryJednostkiWBazie.Count != 0) 
                    ? 
                        SQL.Stocks.GetStocks_Where_Article.Replace("<<IDTOWARU>>", idTow) 
                    : 
                    
                        ""
                ) +

                ((IDLokalizacji >= 0) ? SQL.Stocks.GetStocks_Where_Location.Replace("<<IDLOKALIZACJA>>", IDLokalizacji.ToString()) : "") +
                ((Kod != null && Kod.Paleta != null && Kod.Paleta != "") ? SQL.Stocks.GetStocks_Where_Pal.Replace("<<ID_PAL>>", Pal) : "") +
                ((Kod != null && Kod.Partia != null && Kod.Partia != "") ? SQL.Stocks.GetStocks_Where_Part.Replace("<<ID_PART>>", Par) : "");

            string Where =
                SQL.Stocks.GetStocks_Where_Mag_Where.Replace("<<ID_MAG>>", Globalne.Magazyn.ID.ToString()) +

                (
                
                (Kod != null && Kod.TowaryJednostkiWBazie != null && Kod.TowaryJednostkiWBazie.Count != 0) 
                    ? 
                        SQL.Stocks.GetStocks_Where_Article_Where.Replace("<<IDTOWARU>>", idTow) 
                    : 
                        ""
                ) +

                ((IDLokalizacji >= 0) ? SQL.Stocks.GetStocks_Where_Location_Where.Replace("<<IDLOKALIZACJA>>", IDLokalizacji.ToString()) : "") +
                ((Kod != null && Kod.Paleta != null && Kod.Paleta != "") ? SQL.Stocks.GetStocks_Where_Pal_Where.Replace("<<ID_PAL>>", Pal) : "") +
                ((Kod != null && Kod.Partia != null && Kod.Partia != "") ? SQL.Stocks.GetStocks_Where_Part_Where.Replace("<<ID_PART>>", Par) : "");

            Komenda =
                SQL.Stocks.GetStocks.Replace("<<IDJEDNOSTKI>>", Globalne.CurrentUserSettings.DisplayUnit.ToString()).
                                    Replace("<<SUM>>", Sum).
                                    Replace("<<WHERE>>", Where).
                                    Replace("<<ROZNOSZENIE>>", "0");
                                    //Replace("<<ROZNOSZENIE>>", SQL.Stocks.W_Roznoszeniu.Replace("<<IDTOWARU>>", idTow).Replace("<<IDJEDNOSTKI>>", Globalne.CurrentUserSettings.DisplayUnit.ToString()));

            try
            {
                #warning HiveInvoke
                ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)Helpers.HiveInvoke(typeof(WMSServerAccess.Ogólne.OgólneBL), "ZapytanieSQL", Komenda);
                // usuniecie pozycji gdzie ilosc rowna sie zeru.. nalezaloby zmienic SQL GetStocks, rozwiazanie tymczasowe i zarazem docelowe
                Zap.ListaWierszy?.RemoveAll(x => x[3]?.ToString() == "0");
                return Zap;
            }
            catch (Exception ex)
            {

                return new ZapytanieZTabeliO() { Poprawność = ex.Message };
            }
            

        }
        private void GetDataBufor(ZapytanieZTabeliO zap)
        {
            if (zap == null)
                return;

            var idTow = Kod?.TowaryJednostkiWBazie[0]?.IDTowaru == null ? "-1" : Kod?.TowaryJednostkiWBazie[0]?.IDTowaru.ToString();

            var sqlRoznoszoneZBufora = SQL.Stocks.GetStocks_RoznoszoneZBufora.Replace("<<IDTOWARU>>", idTow).Replace("<<IDJEDNOSTKI>>", Globalne.CurrentUserSettings.DisplayUnit.ToString());
            try
            {

#warning HiveInvoke
                var roznoszoneZBufora = (ZapytanieZTabeliO)Helpers.HiveInvoke(typeof(WMSServerAccess.Ogólne.OgólneBL), "ZapytanieSQL", sqlRoznoszoneZBufora);

                // usuniecie pozycji gdzie ilosc rowna sie zeru.. nalezaloby zmienic SQL GetStocks, rozwiazanie tymczasowe i zarazem docelowe
                zap.ListaWierszy.AddRange(roznoszoneZBufora.ListaWierszy);

            }
            catch (Exception ex)
            {
                zap.Poprawność = ex.Message;
            }
        }
    }

    internal class StocksListAdapter : BaseAdapter<object[]>
    {
        readonly List<object[]> Items;
        readonly Activity Ctx;

        public StocksListAdapter(Activity Ctx, List<object[]> Items) : base()
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
        public decimal Sum
        {
            get
            {
                try
                {
                    decimal Sum = 0;

                    foreach (object[] Pos in this.Items)
                        if (Convert.ToDecimal(Pos[3]) != -1)
                            Sum += Convert.ToDecimal(Pos[3]);

                    return Sum;
                }
                catch (Exception)
                {
                    return -1;
                }

            }

        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Pos = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_stocks, null);

            view.FindViewById<TextView>(Resource.Id.stocks_list_articlename).Text = (string)Pos[(int)SQL.Stocks.Stocks_Results.strNazwaTowaru];
            view.FindViewById<TextView>(Resource.Id.stocks_list_locationsname).Text = (string)Pos[(int)SQL.Stocks.Stocks_Results.strNazwaLok];
            view.FindViewById<TextView>(Resource.Id.stocks_list_amount).Text = (Math.Round(Convert.ToDecimal(Pos[(int)SQL.Stocks.Stocks_Results.numStan]), Globalne.CurrentSettings.DecimalSpaces)).ToString() +
                                                                               " (" + (string)Pos[(int)SQL.Stocks.Stocks_Results.strNazwaJednostki] + ") ";

            return view;
        }
    }
}

