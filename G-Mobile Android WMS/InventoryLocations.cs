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
    public class InventoryLocationsActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView LocationsList;
        FloatingActionButton BtnEdit;
        TextView ItemCount;

        DokumentVO Doc;
        List<LokalizacjaInwentaryzacji> Locs;

        DocumentStatusTypes Status = DocumentStatusTypes.Zamknięty;

        int LastSelectedItemPos = -1;
        int SelectedItemPos = -1;

        internal static class Vars
        {
            public const string InventoryDoc = "InventoryDoc";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_inventorylocations);

            Doc = (DokumentVO)
                Helpers.DeserializePassedJSON(Intent, Vars.InventoryDoc, typeof(DokumentVO));

            GetAndSetControls();

            Task.Run(() => InsertData());
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, Doc.strNazwa);

            Back = FindViewById<FloatingActionButton>(Resource.Id.locations_button_prev);
            BtnEdit = FindViewById<FloatingActionButton>(Resource.Id.locations_btn_edit);
            LocationsList = FindViewById<ListView>(Resource.Id.list_view_locations);
            ItemCount = FindViewById<TextView>(Resource.Id.locations_item_count);

            Back.Click += Back_Click;
            BtnEdit.Click += BtnEdit_Click;
            LocationsList.ItemClick += ListView_ItemClick;
            LocationsList.ItemLongClick += ListView_ItemLongClick;
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            SelectedItemPos = e.Position;
            BtnEdit_Click(this, null);
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedItemPos = e.Position;

            if (LastSelectedItemPos == SelectedItemPos)
            {
                LastSelectedItemPos = -1;
                BtnEdit_Click(this, null);
            }
            else
                LastSelectedItemPos = SelectedItemPos;
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() => Do_Edit());
        }

        private async void Do_Edit(int IDLoc = -1)
        {
            if (IDLoc < 0)
            {
                if (SelectedItemPos < 0)
                    return;

                LokalizacjaInwentaryzacji SelectedItem = (
                    LocationsList.Adapter as InventoryLocationsAdapter
                )[SelectedItemPos];

                if (SelectedItem == null)
                    return;

                IDLoc = SelectedItem.idLokalizacja;
            }

            Helpers.ShowProgressDialog(GetString(Resource.String.documents_opening));

            await Task.Delay(Globalne.TaskDelay);

            if (Status != DocumentStatusTypes.Zamknięty)
                Serwer.dokumentBL.UstawStatusLokInwentaryzacji(IDLoc, Doc.ID, false);

            BusinessLogicHelpers.Documents.EditDocuments(
                this,
                new List<int>() { Doc.ID },
                Enums.DocTypes.IN,
                Enums.ZLMMMode.None,
                IDLoc
            );

            Helpers.HideProgressDialog();
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

        private async Task ParseBarcode(string Data)
        {
            LokalizacjaVO lokalizacja = Serwer.lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(
                Data,
                Globalne.Magazyn.ID,
                true
            );

            // dodawanie nowej lokalizacji do dokumentu inwentaryzacjyjnego IN na uprawnienie
            if (
                lokalizacja.ID > 0
                && (Locs.Find(x => x.idLokalizacja == lokalizacja.ID) == null)
                && Globalne.Operator.dodawanieLokalizacjiDoDokumentuIN
            )
            {
                var res = Helpers
                    .AlertAsyncWithConfirm(
                        this,
                        $"Lokalizacja '{lokalizacja.strNazwa}' nie jest przypisana do dokumentu.\nCzy chcesz dodać lokalizację do dokumentu inwentaryzacyjnego?"
                    )
                    .Result;
                if (res.Value)
                {
                    Serwer.dokumentBL.WstawLokalizacjęDoDokumentuInwentaryzacji(
                        Doc.ID,
                        lokalizacja.ID
                    );
                    Do_Edit(lokalizacja.ID);
                }
                return;
            }

            if (lokalizacja.ID < 0 || (Locs.Find(x => x.idLokalizacja == lokalizacja.ID) == null))
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.inventorylocations_not_found,
                    Resource.Raw.sound_error,
                    Resource.String.global_alert
                );
                return;
            }
            else
            {
                Do_Edit(lokalizacja.ID);
            }
        }

        private void Settings_Click(object sender, EventArgs e)
        {
            UserSettings.ShowUnitSelectionDialog(this);
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedItemPos = e.Position;

            if (LastSelectedItemPos == SelectedItemPos)
            {
                LastSelectedItemPos = -1;
                BtnEdit_Click(this, null);
            }
            else
                LastSelectedItemPos = SelectedItemPos;
        }

        async Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.locations_loading));

                Locs = await Task.Factory.StartNew(() => GetData());

                Status = (DocumentStatusTypes)
                    Serwer.dokumentBL.PobierzTypStatusuDokumentu(Doc.ID, "", "", -1, -1, "");

                RunOnUiThread(() =>
                {
                    LocationsList.Adapter = new InventoryLocationsAdapter(this, Locs);
                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + LocationsList.Adapter.Count.ToString()
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

        private List<LokalizacjaInwentaryzacji> GetData()
        {
            List<LokalizacjaInwentaryzacji> Lokalizacje =
                Serwer.dokumentBL.PobierzListęLokalizacjiInwentaryzacji(Doc.ID);
            return Lokalizacje;
        }

        async void Back_Click(object sender, EventArgs e)
        {
            if (Status != DocumentStatusTypes.Zamknięty)
                await RunIsBusyTaskAsync(() => Do_Exit());
            else
                await RunIsBusyTaskAsync(() => Do_Exit_Without_Asking());
        }

        async Task Do_Exit()
        {
            try
            {
                if (IsSwitchingActivity)
                    return;

                if (
                    await BusinessLogicHelpers.Documents.ShowAndApplyDocumentExitOptions(
                        this,
                        new List<DokumentVO>() { Doc },
                        DocTypes.IN
                    )
                )
                {
                    IsSwitchingActivity = true;

                    Intent i = new Intent(this, typeof(DocumentsActivity));
                    i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocTypes.IN);
                    i.SetFlags(ActivityFlags.NewTask);

                    StartActivity(i);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        async Task Do_Exit_Without_Asking()
        {
            try
            {
                if (IsSwitchingActivity)
                    return;

                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(DocumentsActivity));
                i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocTypes.IN);
                i.SetFlags(ActivityFlags.NewTask);

                StartActivity(i);
                Finish();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }
    }

    internal class InventoryLocationsAdapter : BaseAdapter<LokalizacjaInwentaryzacji>
    {
        readonly List<LokalizacjaInwentaryzacji> Items;
        readonly Activity Ctx;
        readonly int ArticleID;

        public InventoryLocationsAdapter(Activity Ctx, List<LokalizacjaInwentaryzacji> Items)
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override LokalizacjaInwentaryzacji this[int position]
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
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_inventorylocs, null);

            view.FindViewById<TextView>(Resource.Id.locations_list_locationname).Text =
                Lokalizacja.NazwaLokalizacji;

            view.FindViewById<TextView>(Resource.Id.locations_list_status)
                .SetBackgroundColor(
                    Lokalizacja.bZakonczona
                        ? Android.Graphics.Color.DarkGreen
                        : Android.Graphics.Color.Yellow
                );

            return view;
        }
    }
}
