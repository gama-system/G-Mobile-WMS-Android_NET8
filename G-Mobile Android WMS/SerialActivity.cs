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
using System.Linq;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, WindowSoftInputMode = Android.Views.SoftInput.AdjustPan | Android.Views.SoftInput.StateHidden)]
    public class SerialActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView serialList;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        FloatingActionButton BtnSettings;
        TextView ItemCount;
        TextView AmountField;


        int IDTowaru = -1;
        int IDLokalizacji = -1;
        bool AskOnStart = false;
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
            SetContentView(Resource.Layout.activity_serial);

            IDTowaru = Intent.GetIntExtra(Vars.IDTowaru, -1);
            IDLokalizacji = Intent.GetIntExtra(Vars.IDLokalizacji, -1);
            AskOnStart = Intent.GetBooleanExtra(Vars.AskOnStart, false);
            bool Bufor = Intent.GetBooleanExtra(Vars.Bufor, false);

   
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
            Helpers.SetActivityHeader(this, GetString(Resource.String.serial_name));

            Back = FindViewById<FloatingActionButton>(Resource.Id.serial_button_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.serial_btn_refresh);
            Search = FindViewById<FloatingActionButton>(Resource.Id.serial_btn_search);
            BtnSettings = FindViewById<FloatingActionButton>(Resource.Id.serial_btn_settings);
            serialList = FindViewById<ListView>(Resource.Id.list_view_serial);
            ItemCount = FindViewById<TextView>(Resource.Id.serial_item_count);

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;

            serialList.ItemClick += List_ItemClick;
            Search.Click += Search_Click;
            BtnSettings.Click += Settings_Click;
        }

        public void SelectSerialAndCloseActivity(NumerSeryjnyO Num)
        {
            // czy tylko widok/podglad
            if (Globalne.CurrentSettings.DisableSSCCChange)
                return;

            Intent i = new Intent();
            i.PutExtra(Results.SelectedID, Num.ID);
            i.PutExtra(Results.SelectedJSON, Newtonsoft.Json.JsonConvert.SerializeObject(Num));

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
            NumerSeryjnyO numerSeryjny = Globalne.numerSeryjnyBL.PobierzNumerSeryjny(-1, Data);
            if (numerSeryjny.ID < 0)
            {
                await Helpers.AlertAsyncWithConfirm(this, Resource.String.serial_not_found, Resource.Raw.sound_error, Resource.String.global_alert);
                return -1;
            }
            else
            {
                SelectSerialAndCloseActivity(numerSeryjny);
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
                    NumerSeryjnyO Selected = (serialList.Adapter as SerialListAdapter)[e.Position];

                    if (Selected.ID != -1)
                        SelectSerialAndCloseActivity(Selected);

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
            var Res = await Helpers.AlertAsyncWithPrompt(this, Resource.String.serial_filter, null, FilterText, InputType.Default);

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

                Helpers.ShowProgressDialog(GetString(Resource.String.serial_loading));

                List<NumerSeryjnyO> serial = await Task.Factory.StartNew(() => GetData());

                RunOnUiThread(() =>
                {
                    serialList.Adapter = new SerialListAdapter(this, serial, IDTowaru);
                    Helpers.SetTextOnTextView(this, ItemCount, GetString(Resource.String.global_liczba_pozycji) + " " + serialList.Adapter.Count.ToString());
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

        private List<NumerSeryjnyO> GetData()
        {
            List<NumerSeryjnyO> numery = Globalne.numerSeryjnyBL.PobierzListeNumerowSeryjychNaLokalizacjiDlaDanegoTowaru(IDTowaru,IDLokalizacji);

            return numery;
        }
        public IEnumerable<TSource> DistinctBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
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

    internal class SerialListAdapter : BaseAdapter<NumerSeryjnyO>
    {
        readonly List<NumerSeryjnyO> Items;
        readonly Activity Ctx;
        readonly int ArticleID;

        public SerialListAdapter(Activity Ctx, List<NumerSeryjnyO> Items, int ArticleID) : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
            this.ArticleID = ArticleID;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override NumerSeryjnyO this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
             var data= Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_serial, null);

            view.FindViewById<TextView>(Resource.Id.serial_list_name).Text =data.strKod ;

          
            return view;
        }

    }
}

