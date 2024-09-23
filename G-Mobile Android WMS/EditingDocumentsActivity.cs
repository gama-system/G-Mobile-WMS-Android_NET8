using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;
using G_Mobile_Android_WMS.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Enums;
using G_Mobile_Android_WMS.ExtendedModel;
using Java.Nio.Channels;
using Symbol.XamarinEMDK.Barcode;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = SoftInput.AdjustNothing | SoftInput.StateHidden
    )]
    public class EditingDocumentsActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton Add;
        FloatingActionButton Delete;
        FloatingActionButton Edit;
        FloatingActionButton EditAdd;
        FloatingActionButton ChangeLoc;
        FloatingActionButton Info;

        LinearLayout DocumentsListHeader;

        ListView ListView;
        TextView ItemCount;
        TextView ScanHint;
        TextView ItemSum;

        public DocTypes DocType = DocTypes.PW;
        public DocumentStatusTypes Status = DocumentStatusTypes.Otwarty;

        public List<DokumentVO> Documents = new List<DokumentVO>();
        public List<DocumentItemRow> Items = new List<DocumentItemRow>();
        public Operation CurrentOperation = Operation.In;

        public int InventoryLoc = -1;
        public int LastSelectedItemPos = -1;
        public int SelectedItemPos = -1;
        public static int SelectedDefaultLoc = -1;
        public string SelectedDefaultLocName = "";
        public bool ShowCheckboxes = false;
        public List<int> CheckedItems = new List<int>();

        readonly Dictionary<Enums.EditingDocumentsListDisplayElements, int> HeaderElementsDict =
            new Dictionary<Enums.EditingDocumentsListDisplayElements, int>()
            {
                [EditingDocumentsListDisplayElements.DoneAmount] = Resource
                    .Id
                    .editingdocuments_listheader_amount,
                [EditingDocumentsListDisplayElements.SetAmount] = Resource
                    .Id
                    .editingdocuments_listheader_setamount,
                [EditingDocumentsListDisplayElements.Location] = Resource
                    .Id
                    .editingdocuments_listheader_location,
                [EditingDocumentsListDisplayElements.BestBefore] = Resource
                    .Id
                    .editingdocuments_listheader_bestbefore,
                [EditingDocumentsListDisplayElements.ProductionDate] = Resource
                    .Id
                    .editingdocuments_listheader_proddate,
                [EditingDocumentsListDisplayElements.SerialNumber] = Resource
                    .Id
                    .editingdocuments_listheader_serialnumber,
                [EditingDocumentsListDisplayElements.Flog] = Resource
                    .Id
                    .editingdocuments_listheader_flog,
                [EditingDocumentsListDisplayElements.Partia] = Resource
                    .Id
                    .editingdocuments_listheader_partia,
                [EditingDocumentsListDisplayElements.Paleta] = Resource
                    .Id
                    .editingdocuments_listheader_paleta,
                [EditingDocumentsListDisplayElements.Lot] = Resource
                    .Id
                    .editingdocuments_listheader_lot,
                [EditingDocumentsListDisplayElements.ArticleName] = Resource
                    .Id
                    .editingdocuments_listheader_articlename,
                [EditingDocumentsListDisplayElements.KodEAN] = Resource
                    .Id
                    .editingdocuments_listheader_kodean,
                [EditingDocumentsListDisplayElements.NrKat] = Resource
                    .Id
                    .editingdocuments_listheader_NrKat,
                [EditingDocumentsListDisplayElements.Symbol] = Resource
                    .Id
                    .editingdocuments_listheader_symbol,
            };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_editingdocuments);

            InventoryLoc = Intent.GetIntExtra(
                EditingDocumentsActivity_Common.Vars.InventoryLoc,
                -1
            );
            DocType = (DocTypes)Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocType, 0);

            Status = (DocumentStatusTypes)
                Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocStatus, 5);
            Documents =
                (List<DokumentVO>)
                    Helpers.DeserializePassedJSON(
                        Intent,
                        EditingDocumentsActivity_Common.Vars.DocsJSON,
                        typeof(List<DokumentVO>)
                    );

            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[DocType];

            CurrentOperation =
                (
                    DocType == Enums.DocTypes.PW
                    || DocType == Enums.DocTypes.PZ
                    || DocType == Enums.DocTypes.IN
                )
                    ? Enums.Operation.In
                    : Enums.Operation.Out;

            GetAndSetControls();

            Task.Run(() => InsertData(false));
        }

        private void SetVisibilityOnHeaderItems()
        {
            Dictionary<EditingDocumentsListDisplayElements, bool> Set = Globalne
                .CurrentSettings
                .EditingDocumentsListDisplayElementsListsINNNR[DocType];

            foreach (EditingDocumentsListDisplayElements Item in HeaderElementsDict.Keys)
            {
                if (!HeaderElementsDict.ContainsKey(Item))
                    continue;

                TextView v = FindViewById<TextView>(HeaderElementsDict[Item]);

                if (v == null)
                    continue;

                bool VisibilityToSet;

                switch (Item)
                {
                    case EditingDocumentsListDisplayElements.SetAmount:
                        VisibilityToSet = (Set[Item] && Documents[0].bZlecenie);
                        break;
                    case EditingDocumentsListDisplayElements.Flog:
                        VisibilityToSet = (
                            Set[Item] && Globalne.CurrentSettings.FunkcjeLogistyczne
                        );
                        break;
                    case EditingDocumentsListDisplayElements.Paleta:
                        VisibilityToSet = (Set[Item] && Globalne.CurrentSettings.Palety);
                        break;
                    case EditingDocumentsListDisplayElements.Partia:
                        VisibilityToSet = (Set[Item] && Globalne.CurrentSettings.Partie);
                        break;
                    default:
                    {
                        if (Set.ContainsKey(Item))
                        {
                            VisibilityToSet = Set[Item];
                            break;
                        }
                        else
                            continue;
                    }
                }

                v.Visibility = VisibilityToSet ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        private void GetAndSetControls()
        {
            ScanHint = FindViewById<TextView>(Resource.Id.scanhint);

            if (Documents.Count == 1)
            {
                if (string.IsNullOrEmpty(Documents[0].strDokumentDostawcy))
                    Helpers.SetActivityHeader(this, Documents[0].strNazwa);
                else
                    Helpers.SetActivityHeader(
                        this,
                        Documents[0].strNazwa + "\r\n" + Documents[0].strDokumentDostawcy
                    );

                if (InventoryLoc >= 0)
                    SelectedDefaultLoc = InventoryLoc;
                else if (Documents[0].intLokalizacjaPozycji >= 0)
                    SelectedDefaultLoc = Documents[0].intLokalizacjaPozycji;

                if (SelectedDefaultLoc >= 0)
                {
#warning HiveInvoke
                    SelectedDefaultLocName = (string)
                        Helpers.HiveInvoke(
                            typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL),
                            "PobierzNazwęLokalizacji",
                            SelectedDefaultLoc
                        );

                    int ResId = 0;

                    if (CurrentOperation == Operation.In)
                        ResId = Resource.String.editingdocuments_activity_scanhint2_in;
                    else if (CurrentOperation == Operation.Out)
                        ResId = Resource.String.editingdocuments_activity_scanhint2_out;
                    else
                        ResId = Resource.String.editingdocuments_activity_scanhint2_unk;

                    Helpers.SetTextOnTextView(
                        this,
                        ScanHint,
                        GetString(ResId) + " " + SelectedDefaultLocName
                    );
                }
            }
            else
                Helpers.SetActivityHeader(
                    this,
                    GetString(Resource.String.editing_documents_activity_name)
                );

            Back = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_back);
            Add = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_add);
            Edit = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_edit);
            EditAdd = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_editadd);
            Delete = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_delete);
            ChangeLoc = FindViewById<FloatingActionButton>(
                Resource.Id.editingdocuments_btn_changeloc
            );
            Info = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_info);
            DocumentsListHeader = FindViewById<LinearLayout>(
                Resource.Id.editingdocuments_list_header
            );
            //ListView = FindViewById<ListView>(Resource.Id.list_view_documents);

            //     ListView.ItemClick += ListView_ItemClick;
            ListView = FindViewById<ListView>(Resource.Id.list_view_editingdocuments);
            ItemCount = FindViewById<TextView>(Resource.Id.editingdocuments_item_count);
            ItemSum = FindViewById<TextView>(Resource.Id.editingdocuments_item_sum);

            SetVisibilityOnHeaderItems();

            ListView.ItemLongClick += ListView_ItemLongClick;
            ListView.ItemClick += ListView_ItemClick;
            Back.Click += Back_Click;
            ChangeLoc.Click += ChangeLoc_Click;
            Info.Click += Info_Click;
            Add.Click += Add_Click;
            Delete.Click += Delete_Click;
            Edit.Click += Edit_Click;
            EditAdd.Click += EditAdd_Click;
            DocumentsListHeader.Click += DocumentsListHeader_Click;
            if (
                !Globalne.CurrentUserSettings.CanDeleteItems
                || (Documents[0].bZlecenie && !Globalne.CurrentUserSettings.CanDeleteItemsOnOrders)
            )
                Delete.Visibility = ViewStates.Gone;

            if (Documents[0].bZlecenie || Documents.Count != 1)
                Add.Visibility = ViewStates.Gone;

            if (Documents[0].bZlecenie)
                EditAdd.Visibility = ViewStates.Gone;

            // wylaczamy tymczasowo edycje pozycji z dodawaniem -  nie miescie sie na ekranie na mniejszych ekranach (np. MC33)
            EditAdd.Visibility = ViewStates.Gone;

            if (DocType == DocTypes.IN)
            {
                ChangeLoc.Visibility = ViewStates.Gone;
                Info.Visibility = ViewStates.Gone;
            }

            if (Status == DocumentStatusTypes.Zamknięty || Status == DocumentStatusTypes.Wstrzymany)
            {
                Add.Visibility = ViewStates.Gone;
                Edit.Visibility = ViewStates.Gone;
                EditAdd.Visibility = ViewStates.Gone;
                Delete.Visibility = ViewStates.Gone;
            }
            else
            {
                ListView.ItemClick += ListView_ItemClick;
                ListView.ItemLongClick += ListView_ItemLongClick;
            }
        }

        int howManyClicked = 0;

        private void Info_Click(object sender, EventArgs e)
        {
            if (SelectedItemPos < 0)
            {
                Intent i = new Intent(this, typeof(StocksActivity));
                i.PutExtra(ArticlesActivity.Vars.AskOnStart, true);

                RunOnUiThread(
                    () =>
                        StartActivityForResult(
                            i,
                            (int)StocksActivity.ResultCodes.LocationsActivityResult
                        )
                );
                EditingDocumentsActivity_Common.ShowLocDialog(this, -1);
            }
            else
            {
                DocumentItemRow SelectedItem = (ListView.Adapter as EditingDocumentsListAdapter)[
                    SelectedItemPos
                ];
                EditingDocumentsActivity_Common.ShowLocDialog(this, SelectedItem.Base.idTowaru);
            }
        }

        private void EditAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItemPos < 0)
                    return;

                if (
                    Status != DocumentStatusTypes.Zamknięty
                    || Status == DocumentStatusTypes.Wstrzymany
                )
                {
                    RunIsBusyAction(() =>
                    {
                        DocumentItemRow SelectedItem = (
                            ListView.Adapter as EditingDocumentsListAdapter
                        )[SelectedItemPos];

                        DocumentItems.EditItem(
                            this,
                            Documents,
                            SelectedItem,
                            DocType,
                            CurrentOperation,
                            SelectedDefaultLoc,
                            DefaultLocType.None,
                            SelectedDefaultLocName,
                            (int)
                                EditingDocumentsActivity_Common
                                    .ResultCodes
                                    .DocumentItemActivityResult,
                            true,
                            false
                        );
                    });
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private void ChangeLoc_Click(object sender, EventArgs e)
        {
            EditingDocumentsActivity_Common.ChangeDefaultLocDialog(this, Globalne.Magazyn.ID);
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItemPos < 0)
                    return;

                Thread.Sleep(Globalne.TaskDelay);

                if (
                    Status != Enums.DocumentStatusTypes.Zamknięty
                    && Status != DocumentStatusTypes.Wstrzymany
                )
                {
                    RunIsBusyAction(() =>
                    {
                        DocumentItemRow SelectedItem = (
                            ListView.Adapter as EditingDocumentsListAdapter
                        )[SelectedItemPos];

                        DocumentItems.EditItem(
                            this,
                            Documents,
                            SelectedItem,
                            DocType,
                            CurrentOperation,
                            SelectedDefaultLoc,
                            DefaultLocType.None,
                            SelectedDefaultLocName,
                            (int)
                                EditingDocumentsActivity_Common
                                    .ResultCodes
                                    .DocumentItemActivityResult,
                            false,
                            false
                        );
                    });
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                IsSwitchingActivity = false;
                return;
            }

            RunIsBusyAction(() => DoEdit());
        }

        private async void DoEdit(
            List<int> SelectedDocIDs = null,
            List<DokumentVO> Docs = null,
            bool Multipicking = false
        )
        {
            try
            {
                if (CheckedItems.Count == 0 && SelectedDocIDs == null && Docs == null)
                    return;

                Helpers.ShowProgressDialog(GetString(Resource.String.documents_opening));

                if (SelectedDocIDs == null && Docs == null)
                {
                    SelectedDocIDs = new List<int>();

                    foreach (int Pos in CheckedItems)
                    {
                        object[] Selected = (ListView.Adapter as DocumentsListAdapter)[Pos];
                        int IDDoc = Convert.ToInt32(
                            Selected[(int)SQL.Documents.Documents_Results.idDokumentu]
                        );
                        SelectedDocIDs.Add(IDDoc);
                    }
                }

                // await Task.Delay(100);

                //BusinessLogicHelpers.Documents.EditDocuments(this, SelectedDocIDs, DocType, ZLMMMode, -1, Docs, Multipicking);

                Helpers.HideProgressDialog();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                IsSwitchingActivity = false;
                return;
            }
        }

        async void Delete_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(async () =>
            {
                await DoDelete();
                await InsertData(true);
            });
        }

        async Task DoDelete()
        {
            try
            {
                if (SelectedItemPos < 0)
                    return;

                DocumentItemRow Item = (ListView.Adapter as EditingDocumentsListAdapter)[
                    SelectedItemPos
                ];
                await DocumentItems.DeleteDocumentItems(
                    this,
                    new List<int>() { Item.Base.ID },
                    DocType
                );
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            try
            {
                RunIsBusyAction(() =>
                {
                    DocumentItems.AddItem(
                        this,
                        Documents[0],
                        DocType,
                        CurrentOperation,
                        SelectedDefaultLoc,
                        SelectedDefaultLocName,
                        DefaultLocType.None,
                        (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult,
                        false
                    );
                });
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        async void Back_Click(object sender, EventArgs e)
        {
            if (Status == DocumentStatusTypes.Zamknięty || Status == DocumentStatusTypes.Wstrzymany)
                await RunIsBusyTaskAsync(() => Do_Exit_Without_Asking());
            else
                await RunIsBusyTaskAsync(() => Do_Exit());
        }

        private void DocumentsListHeader_Click(object sender, EventArgs e)
        {
            ShowCheckboxes = false;
            CheckedItems = new List<int>();

            for (int i = 0; i < ListView.Count; i++)
            {
                View Item = ListView.GetChildAt(i);

                if (Item != null)
                {
                    CheckBox Chb = (CheckBox)
                        Item.FindViewById(Resource.Id.documents_list_checkboxBOX);
                    Chb.Checked = false;
                }
            }

            if (ListView.Adapter != null)
                (ListView.Adapter as EditingDocumentsListAdapter).NotifyDataSetChanged();
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            ShowCheckboxes = true;
            CheckedItems = new List<int>();
            (ListView.Adapter as EditingDocumentsListAdapter).NotifyDataSetChanged();
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedItemPos = e.Position;

            // jezeli zmiana pozycji na liscie to wyzeruj licznik aby nie mozna bylo wejsc jednoklikiem w inna pozycje
            if (LastSelectedItemPos != SelectedItemPos)
            {
                LastSelectedItemPos = SelectedItemPos;
                howManyClicked = 0;
            }

            // sprawdzamy czy kliknieto 2 raz... do tego zdarzenia wchodzimy dwa razy jedno za drugim (nie anazlizowalem dlaczego wchodzi tu drugi raz jak kliknie sie raz).. dlatego wartosc 3
            if (howManyClicked >= 3)
            {
                if (ShowCheckboxes == false)
                    Edit_Click(this, null);
                howManyClicked = 0;
            }
            else
                howManyClicked++;
        }

        async Task Do_Exit()
        {
            try
            {
                if (IsSwitchingActivity)
                    return;

                if (DocType == DocTypes.IN)
                {
                    if (
                        await BusinessLogicHelpers.Documents.ShowAndApplyInventoryLocationExitOptions(
                            this,
                            Documents[0],
                            InventoryLoc
                        )
                    )
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent(this, typeof(InventoryLocationsActivity));
                        i.PutExtra(
                            InventoryLocationsActivity.Vars.InventoryDoc,
                            Helpers.SerializeJSON(Documents[0])
                        );
                        i.SetFlags(ActivityFlags.NewTask);

                        StartActivity(i);
                        Finish();
                    }
                }
                else
                {
                    if (
                        await BusinessLogicHelpers.Documents.ShowAndApplyDocumentExitOptions(
                            this,
                            Documents,
                            DocType
                        )
                    )
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent(this, typeof(DocumentsActivity));
                        i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocType);
                        i.SetFlags(ActivityFlags.NewTask);

                        StartActivity(i);
                        Finish();
                    }
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

                if (DocType == DocTypes.IN)
                {
                    await BusinessLogicHelpers.Documents.ShowAndApplyInventoryLocationExitOptions(
                        this,
                        Documents[0],
                        InventoryLoc,
                        true
                    );

                    Intent i = new Intent(this, typeof(InventoryLocationsActivity));
                    i.PutExtra(
                        InventoryLocationsActivity.Vars.InventoryDoc,
                        Helpers.SerializeJSON(Documents[0])
                    );
                    i.SetFlags(ActivityFlags.NewTask);

                    StartActivity(i);
                    Finish();
                }
                else
                {
                    foreach (DokumentVO Doc in Documents)
                        Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);

                    Intent i = new Intent(this, typeof(DocumentsActivity));
                    i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocType);
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

        async Task InsertData(bool ConfirmReady)
        {
            try
            {
                // ShowCheckboxes = false;
                // CheckedItems = new List<int>();
                LastSelectedItemPos = -1;
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.editing_documents_loading));

                List<DocumentItemRow> Items = await Task.Factory.StartNew(
                    () =>
                        EditingDocumentsActivity_Common.GetData(
                            Documents,
                            DocType,
                            ZLMMMode.None,
                            CurrentOperation,
                            SelectedDefaultLoc,
                            DefaultLocType.None,
                            InventoryLoc
                        )
                );

                RunOnUiThread(() =>
                {
                    if (ListView.Adapter == null)
                        ListView.Adapter = new EditingDocumentsListAdapter(this, Items);
                    else
                    {
                        (ListView.Adapter as EditingDocumentsListAdapter).Items = Items;
                        (ListView.Adapter as EditingDocumentsListAdapter).NotifyDataSetChanged();
                    }

                    if ((ListView.Adapter as EditingDocumentsListAdapter).Count != 0)
                        SelectedItemPos = 0;
                    else
                        SelectedItemPos = -1;

                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + ListView.Adapter.Count.ToString()
                    );

                    decimal? Sum = (ListView.Adapter as EditingDocumentsListAdapter).Sum;
                    Helpers.SetTextOnTextView(
                        this,
                        ItemSum,
                        GetString(Resource.String.global_suma_pozycji)
                            + " "
                            + (Sum == null ? "---" : Sum.ToString())
                    );
                });

                Helpers.HideProgressDialog();

                if (Globalne.CurrentSettings.InventAutoClose && ListView.Adapter.Count != 0)
                {
                    int Il = await Task.Factory.StartNew(
                        () => GetNumberOfLocsInInventoryDoc(Documents[0].ID)
                    );

                    if (Il > 1)
                    {
                        IsBusy = false;
                        await RunIsBusyTaskAsync(() => Do_Exit_Without_Asking());

                        //Helpers.PlaySound(this, Resource.Raw.sound_ok);

                        return;
                    }
                }

                if (LastScanData != null && LastScanData.Count != 0)
                {
                    IsBusy = false;
                    OnScan(this, null);
                }
                else if (ConfirmReady)
                {
                    //Helpers.PlaySound(this, Resource.Raw.sound_ok);
                }

                return;
            }
            catch (Exception ex)
            {
                Helpers.HideProgressDialog();
                Helpers.HandleError(this, ex);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static int GetNumberOfLocsInInventoryDoc(int DocID)
        {
            return Serwer.dokumentBL.PobierzListęLokalizacjiInwentaryzacji(DocID).Count();
        }

        private void SetDefaultLocationTo(LokalizacjaVO Loc)
        {
            if (Loc == null)
            {
                SelectedDefaultLoc = -1;
                SelectedDefaultLocName = "";
                Helpers.SetTextOnTextView(
                    this,
                    ScanHint,
                    GetString(Resource.String.editingdocuments_activity_scanhint)
                );
            }
            else
            {
                SelectedDefaultLoc = Loc.ID;
                SelectedDefaultLocName = Loc.strNazwa;

                // wylaczamy - powodowało ze
                // lista pozycji odswiezała sie tylko z ta lokalizacja, pozotalych nie widac
                // InventoryLoc = Loc.ID;

                int ResId = 0;

                if (CurrentOperation == Operation.In)
                    ResId = Resource.String.editingdocuments_activity_scanhint2_in;
                else if (CurrentOperation == Operation.Out)
                    ResId = Resource.String.editingdocuments_activity_scanhint2_out;
                else
                    ResId = Resource.String.editingdocuments_activity_scanhint2_unk;

                Helpers.SetTextOnTextView(
                    this,
                    ScanHint,
                    GetString(ResId) + " " + SelectedDefaultLocName
                );
            }
            if (ListView?.Adapter != null)
                RunOnUiThread(
                    () => (ListView.Adapter as EditingDocumentsListAdapter).NotifyDataSetChanged()
                );
        }

        protected override async void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case (int)EditingDocumentsActivity_Common.ResultCodes.LocationsActivityResult:
                {
                    if (resultCode == Result.Canceled)
                    {
                        SetDefaultLocationTo(null);
                    }
                    else if (resultCode == Result.Ok)
                    {
                        LokalizacjaVO Loc = (LokalizacjaVO)
                            Helpers.DeserializePassedJSON(
                                data,
                                LocationsActivity.Results.SelectedJSON,
                                typeof(LokalizacjaVO)
                            );
                        SetDefaultLocationTo(Loc);
                    }
                    break;
                }
                case (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult:
                {
                    if (resultCode == Result.Ok)
                    {
                        string[] Scanned = new string[0];

                        if (
                            data != null
                            && data.HasExtra(DocumentItemActivity_Common.Results.WereScanned)
                        )
                            Scanned = data.GetStringArrayExtra(
                                DocumentItemActivity_Common.Results.WereScanned
                            );

                        if (Scanned.Length != 0)
                            LastScanData = Scanned.ToList();

                        await RunIsBusyTaskAsync(
                            () => InsertData(Globalne.CurrentSettings.InstantScanning[DocType])
                        );
                    }
                    break;
                }
            }
        }

        #region Old
        /*
          
        private Enums.DocItemStatus GetStatusForItem(PozycjaRow R)
        {
            if (R.numIloscZlecona == R.numIloscZrealizowana)
                return Enums.DocItemStatus.Complete;
            else if (R.numIloscZrealizowana > R.numIloscZlecona)
                return Enums.DocItemStatus.Over;
            else
                return Enums.DocItemStatus.Incomplete;
        }

        private List<DocumentItemRow> GetData_Private()
        {
            List<DocumentItemRow> Items = new List<DocumentItemRow>();
            List<PozycjaŚcieżkiO> PathOrder = new List<PozycjaŚcieżkiO>();

            if (!Documents[0].bZlecenie)
                PathOrder = Serwer.lokalizacjaBL.PobierzŚcieżkęZbiórki(Globalne.Magazyn.ID);

            foreach (DokumentVO Doc in Documents)
            {
                if (Doc.bZlecenie)
                {
                    List<PozycjaRowZPodpowiedzią> Res = Serwer.dokumentBL.PobierzPozycjeIZaproponujLokalizacjeDlaDokumentu(Doc.ID,
                                                                                                                             CurrentOperation == Enums.Operation.In ? true : false,
                                                                                                                             CurrentOperation == Enums.Operation.Out ? true : false,
                                                                                                                             SelectedDefaultLoc);
                    foreach (PozycjaRowZPodpowiedzią Pzp in Res)
                    {
                        DocumentItemRow DocItem = new DocumentItemRow(Pzp.Pozycja);

                        DocItem.Status = GetStatusForItem(Pzp.Pozycja);

                        if (Pzp.PodpowiedźPrzychód != null)
                        {
                            DocItem.ExIDLokalizacjaP = Pzp.PodpowiedźPrzychód.ID;
                            DocItem.ExLokalizacjaP = Pzp.PodpowiedźPrzychód.strNazwa;
                            DocItem.KolejnośćNaŚcieżce = Pzp.PodpowiedźPrzychód.intPozycjaNaŚcieżce;

                        }
                        if (Pzp.PodpowiedźRozchód != null)
                        {
                            DocItem.ExIDLokalizacjaW = Pzp.PodpowiedźRozchód.ID;
                            DocItem.ExLokalizacjaW = Pzp.PodpowiedźRozchód.strNazwa;
                            DocItem.KolejnośćNaŚcieżce = Pzp.PodpowiedźRozchód.intPozycjaNaŚcieżce;
                        }

                        Items.Add(DocItem);
                    }
                }
                else
                {
                    List<PozycjaRow> Pozycje = Serwer.dokumentBL.PobierzListęPozycjiRow(Doc.ID);

                    foreach (PozycjaRow R in Pozycje)
                    {
                        DocumentItemRow DocItem = new DocumentItemRow(R);
                        DocItem.Status = GetStatusForItem(R);

                        if (CurrentOperation == Enums.Operation.In)
                        {
                            DocItem.ExIDLokalizacjaP = DocItem.Base.idLokalizacjaP;
                            DocItem.ExLokalizacjaP = DocItem.Base.strLokalizacjaP;
                        }
                        else
                        {
                            DocItem.ExIDLokalizacjaW = DocItem.Base.idLokalizacjaW;
                            DocItem.ExLokalizacjaW = DocItem.Base.strLokalizacjaW;
                        }

                        if (DocItem.KolejnośćNaŚcieżce < 0)
                        {
                            PozycjaŚcieżkiO Pos = PathOrder.Find(x => x.idLokalizacja == ((CurrentOperation == Enums.Operation.In) ? DocItem.ExIDLokalizacjaP : DocItem.ExIDLokalizacjaW));

                            if (Pos != null)
                                DocItem.KolejnośćNaŚcieżce = Pos.intPozycja;
                            else
                                DocItem.KolejnośćNaŚcieżce = Int32.MaxValue;
                        }

                        Items.Add(DocItem);
                    }
                }
            }

            return Items.OrderBy(x => x.Status).ThenBy(x => x.KolejnośćNaŚcieżce).ToList();
        }

        private List<DocumentItemRow> GetData_Old()
        {
            List<DocumentItemRow> Items = new List<DocumentItemRow>();
            List<PozycjaŚcieżkiO> PathOrder = Serwer.lokalizacjaBL.PobierzŚcieżkęZbiórki(Globalne.Magazyn.ID);


            foreach (DokumentVO Doc in Documents)
            {
                List<PozycjaRow> Pozycje = Serwer.dokumentBL.PobierzListęPozycjiRow(Doc.ID);

                string LocDefault = "";
                int OnPathDocLoc = -1;
                if (Doc.intLokalizacjaPozycji >= 0)
                {
                    LocDefault = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL), "PobierzNazwęLokalizacji", Doc.intLokalizacjaPozycji);
                    OnPathDocLoc = Serwer.lokalizacjaBL.PobierzNumerLokalizacjiNaŚcieżce(Doc.intLokalizacjaPozycji);
                }

                foreach (PozycjaRow R in Pozycje)
                {
                    DocumentItemRow DocItem = new DocumentItemRow(R);
                    DocItem.Status = GetStatusForItem(R);

                    if (Doc.bZlecenie)
                    {
                        if (CurrentOperation == Enums.Operation.In)
                        {
                            if (R.idLokalizacjaP < 0 && Doc.intLokalizacjaPozycji < 0)
                            {
                                PodpowiedźLokalizacjiO Pdp = Serwer.przychRozchBL.ZaproponujLokalizacjęDlaPrzychodu_Nazwa_PozycjaNaŚć(R.idTowaru, Doc.intMagazynP, new List<int>());

                                DocItem.ExIDLokalizacjaP = Pdp.ID;
                                DocItem.ExLokalizacjaP = Pdp.strNazwa;
                                DocItem.KolejnośćNaŚcieżce = Pdp.intPozycjaNaŚcieżce;
                            }
                            else if (Doc.intLokalizacjaPozycji >= 0)
                            {
                                DocItem.ExIDLokalizacjaP = Doc.intLokalizacjaPozycji;
                                DocItem.ExLokalizacjaP = LocDefault;
                                DocItem.KolejnośćNaŚcieżce = OnPathDocLoc;
                            }
                            else
                            {
                                DocItem.ExIDLokalizacjaP = R.idLokalizacjaP;
                                DocItem.ExLokalizacjaP = R.strLokalizacjaP;
                            }
                        }
                        else if (CurrentOperation == Enums.Operation.Out)
                        {
                            if (R.idLokalizacjaW < 0 && Doc.intLokalizacjaPozycji < 0)
                            {
                                PodpowiedźLokalizacjiO Pdp = Serwer.przychRozchBL.ZaproponujLokalizacjęDlaRozchodu_Nazwa_PozycjaNaŚć(R.idTowaru,
                                                                                                                                       Doc.intMagazynW,
                                                                                                                                       R.idPartia,
                                                                                                                                       "",
                                                                                                                                       R.idPaletaW,
                                                                                                                                       "",
                                                                                                                                       R.idFunkcjiLogistycznejW,
                                                                                                                                       new List<int>(),
                                                                                                                                       "");

                                DocItem.ExIDLokalizacjaW = Pdp.ID;
                                DocItem.ExLokalizacjaW = Pdp.strNazwa;
                                DocItem.KolejnośćNaŚcieżce = Pdp.intPozycjaNaŚcieżce;
                            }
                            else if (Doc.intLokalizacjaPozycji >= 0)
                            {
                                DocItem.ExIDLokalizacjaW = Doc.intLokalizacjaPozycji;
                                DocItem.ExLokalizacjaW = LocDefault;
                                DocItem.KolejnośćNaŚcieżce = OnPathDocLoc;
                            }
                            else
                            {
                                DocItem.ExIDLokalizacjaW = R.idLokalizacjaW;
                                DocItem.ExLokalizacjaW = R.strLokalizacjaW;
                            }
                        }
                    }
                    else
                    {
                        if (CurrentOperation == Enums.Operation.In)
                        {
                            DocItem.ExIDLokalizacjaP = DocItem.Base.idLokalizacjaP;
                            DocItem.ExLokalizacjaP = DocItem.Base.strLokalizacjaP;
                        }
                        else
                        {
                            DocItem.ExIDLokalizacjaW = DocItem.Base.idLokalizacjaW;
                            DocItem.ExLokalizacjaW = DocItem.Base.strLokalizacjaW;
                        }
                    }


                    if (DocItem.KolejnośćNaŚcieżce < 0)
                    {
                        PozycjaŚcieżkiO Pos = PathOrder.Find(x => x.idLokalizacja == ((CurrentOperation == Enums.Operation.In) ? DocItem.ExIDLokalizacjaP : DocItem.ExIDLokalizacjaW));

                        if (Pos != null)
                            DocItem.KolejnośćNaŚcieżce = Pos.intPozycja;
                        else
                            DocItem.KolejnośćNaŚcieżce = Int32.MaxValue;
                    }

                    Items.Add(DocItem);
                }

            }

            return Items.OrderBy(x => x.Status).ThenBy(x => x.KolejnośćNaŚcieżce).ToList();
        }

        */
        #endregion

        protected override async Task<bool> CheckBeforeAssumingScanningPath(List<string> BarcodesL)
        {
            await base.CheckBeforeAssumingScanningPath(BarcodesL);

            if (DocType == DocTypes.IN)
                return true;
            else
            {
                LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], true);

                if (Loc != null && Loc.ID >= 0)
                {
                    if (Loc.idMagazyn != Globalne.Magazyn.ID)
                    {
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.editingdocuments_locnoinwarehouse,
                            Resource.Raw.sound_miss
                        );
                        LastScanData = null;
                        return false;
                    }

                    SetDefaultLocationTo(Loc);
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        protected override async void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);

            if (Status == DocumentStatusTypes.Zamknięty || Status == DocumentStatusTypes.Wstrzymany)
                return;

            await RunIsBusyTaskAsync(() => ShowProgressAndDecideOperation(LastScanData));
        }

        async Task ShowProgressAndDecideOperation(List<string> Scanned)
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            await Task.Run(async () =>
            {
                try
                {
                    await FindPositionAndEnter(Scanned);
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            });

            Helpers.HideProgressDialog();
            return;
        }

        private async Task FindPositionAndEnter(List<string> Barcodes)
        {
            KodKreskowyZSzablonuO Kod = Helpers.ParseBarcodesAccordingToOrder(Barcodes, DocType);

            if (Globalne.CurrentSettings.GetDataFromFirstSSCCEntry && Kod.Paleta != "")
                DocumentItems.InsertSSCCData(ref Kod);

            if (Kod.TowaryJednostkiWBazie != null && Kod.TowaryJednostkiWBazie.Count == 0)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.articles_not_found,
                    Resource.Raw.sound_miss
                );
                LastScanData = null;
                return;
            }

            if (Kod.TowaryJednostkiWBazie != null && Kod.TowaryJednostkiWBazie.Count > 1)
            {
                TowarJednostkaO Towarjedn = await Indexes.SelectOneArticleFromMany(
                    this,
                    Kod.TowaryJednostkiWBazie
                );

                if (Towarjedn.IDTowaru >= 0)
                    Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>() { Towarjedn };
                else
                    return;
            }

            if (DocType == DocTypes.IN)
            {
                if (
                    !Serwer.dokumentBL.SprawdźCzyTowarMożeByćUżytyDlaPozycji(
                        Kod.TowaryJednostkiWBazie[0].IDTowaru,
                        "I",
                        Documents[0].ID
                    )
                )
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.articles_cannot_be_used,
                        Resource.Raw.sound_miss
                    );
                    LastScanData = null;
                    return;
                }
            }

            if (Documents[0].bZlecenie)
            {
                var result =
                    from x in (ListView.Adapter as EditingDocumentsListAdapter).Items
                    where
                        (x.Base.idTowaru == Kod.TowaryJednostkiWBazie[0].IDTowaru)
                        &&
                        // if () { }
                        (x.Base.idJednostkaMiary == Kod.TowaryJednostkiWBazie[0].IDJednostki)
                        && ((Kod.Partia == x.Base.strPartia) || (x.Base.strPartia == ""))
                        && (
                            (
                                CurrentOperation == Enums.Operation.In
                                    ? (Kod.Paleta == x.Base.strPaletaP)
                                    : (Kod.Paleta == x.Base.strPaletaW)
                            )
                            || (
                                CurrentOperation == Enums.Operation.In
                                    ? (x.Base.strPaletaP == "")
                                    : (x.Base.strPaletaW == "")
                            )
                        )
                        && (x.Base.numIloscZrealizowana != x.Base.numIloscZlecona)

                    select x;

                if (result.Count() != 0)
                {
                    DocumentItems.EditItem(
                        this,
                        Documents,
                        result.OrderBy(x => x.KolejnośćNaŚcieżce).First(),
                        DocType,
                        CurrentOperation,
                        SelectedDefaultLoc,
                        DefaultLocType.None,
                        SelectedDefaultLocName,
                        (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult,
                        false,
                        true,
                        Kod
                    );
                }
                else
                {
                    var resultFinished =
                        from x in (ListView.Adapter as EditingDocumentsListAdapter).Items
                        where
                            (x.Base.idTowaru == Kod.TowaryJednostkiWBazie[0].IDTowaru)
                            &&
                            // LIDER ZMIANA jednostki miary jednostka miary jednostkamiary/ rolka/

                            // (x.Base.idJednostkaMiary == Kod.TowaryJednostkiWBazie[0].IDJednostki) &&

                            ((Kod.Partia == x.Base.strPartia) || (x.Base.strPartia == ""))
                            // 30.11.2023 przy testach na paletach okazuje sie ze trzeba bylo zmienic zapis

                            && (
                                (
                                    CurrentOperation == Enums.Operation.In
                                        ? (Kod.Paleta == x.Base.strPaletaW)
                                        : (Kod.Paleta == x.Base.strPaletaP)
                                )
                                || (
                                    CurrentOperation == Enums.Operation.In
                                        ? (x.Base.strPaletaW == "")
                                        : (x.Base.strPaletaP == "")
                                )
                            )
                        //&& ((CurrentOperation == Enums.Operation.In ? (Kod.Paleta == x.Base.strPaletaP) : (Kod.Paleta == x.Base.strPaletaW))
                        //|| (CurrentOperation == Enums.Operation.In ? (x.Base.strPaletaP == "") : (x.Base.strPaletaW == "")))

                        select x;

                    if (resultFinished.Count() != 0)
                    {
                        DocumentItems.EditItem(
                            this,
                            Documents,
                            resultFinished.OrderBy(x => x.KolejnośćNaŚcieżce).First(),
                            DocType,
                            CurrentOperation,
                            SelectedDefaultLoc,
                            DefaultLocType.None,
                            SelectedDefaultLocName,
                            (int)
                                EditingDocumentsActivity_Common
                                    .ResultCodes
                                    .DocumentItemActivityResult,
                            false,
                            true
                        );
                    }
                    else
                    {
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.editingdocuments_cannot_find_item,
                            Resource.Raw.sound_miss
                        );
                        LastScanData = null;
                        return;
                    }
                }
            }
            else
            {
                // sprawdzenie czy nowo dodany towar i jego paleta jest juz w systemie, jezeli jest to pomin dodawanie

                bool czyJestPaleta = Serwer.paletaBL.CzyPaletaIstnieje(Kod.Paleta, -1);

                if (
                    Globalne.CurrentSettings.OnlyOncePalleteSSCCOnDocument
                    && czyJestPaleta
                    && (DocType == DocTypes.PZ || DocType == DocTypes.PW)
                )
                {
                    await Helpers.Alert(
                        this,
                        "Ustawienia wdrożeniowe nie pozwalają na dodanie drugiej takiej samej palety.\nPaleta (SSCC) o takim kodzie jest już w systemie.",
                        Title: "Uwaga!"
                    );
                    LastScanData = null;
                    return;
                }

                // dodawanie nowej pozycji

                DocumentItems.AddItem(
                    this,
                    Documents[0],
                    DocType,
                    CurrentOperation,
                    SelectedDefaultLoc,
                    SelectedDefaultLocName,
                    DefaultLocType.None,
                    (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult,
                    true,
                    Kod
                );
            }
        }
    }

    internal class EditingDocumentsListAdapter : BaseAdapter<DocumentItemRow>
    {
        public List<DocumentItemRow> Items;
        readonly EditingDocumentsActivity Ctx;

        public EditingDocumentsListAdapter(
            EditingDocumentsActivity Ctx,
            List<DocumentItemRow> Items
        )
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override DocumentItemRow this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }
        public decimal? Sum
        {
            get
            {
                if (Items.All(x => x.Base.idJednostkaMiary == Items[0].Base.idJednostkaMiary))
                {
                    return Items.Sum(x => x.Base.numIloscZrealizowana);
                }
                else
                    return null;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Dictionary<EditingDocumentsListDisplayElements, bool> Set = Globalne
                .CurrentSettings
                .EditingDocumentsListDisplayElementsListsINNNR[Ctx.DocType];

            var Pos = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_editingdocuments, null);

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_amount,
                Pos.Base.numIloscZrealizowana.ToString() + " " + Pos.Base.strNazwaJednostki,
                Set[EditingDocumentsListDisplayElements.DoneAmount]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_amount,
                Pos.Base.numIloscZrealizowana.ToString() + " " + Pos.Base.strNazwaJednostki,
                Set[EditingDocumentsListDisplayElements.DoneAmount]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_setamount,
                Pos.Base.numIloscZlecona.ToString() + " " + Pos.Base.strNazwaJednostki,
                Set[EditingDocumentsListDisplayElements.SetAmount] && Ctx.Documents[0].bZlecenie
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_location,
                Ctx.CurrentOperation == Operation.In ? Pos.ExLokalizacjaP : Pos.ExLokalizacjaW,
                Set[EditingDocumentsListDisplayElements.Location]
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_bestbefore,
                Pos.Base.dtDataPrzydatności.Year > 2900
                    ? "---"
                    : Pos.Base.dtDataPrzydatności.ToString(Globalne.CurrentSettings.DateFormat),
                Set[EditingDocumentsListDisplayElements.BestBefore]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_proddate,
                Pos.Base.dtDataProdukcji.Year > 2900
                    ? "---"
                    : Pos.Base.dtDataPrzydatności.ToString(Globalne.CurrentSettings.DateFormat),
                Set[EditingDocumentsListDisplayElements.ProductionDate]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_serialnumber,
                Pos.Base.strNumerySeryjne,
                Set[EditingDocumentsListDisplayElements.SerialNumber]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_lot,
                Pos.Base.strLoty,
                Set[EditingDocumentsListDisplayElements.Lot]
            );
            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_flog,
                Ctx.CurrentOperation == Operation.In
                    ? Pos.Base.strFunkcjiLogistycznejP
                    : Pos.Base.strFunkcjiLogistycznejW,
                Set[EditingDocumentsListDisplayElements.Flog]
                    && Globalne.CurrentSettings.FunkcjeLogistyczne
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_partia,
                Pos.Base.strPartia,
                Set[EditingDocumentsListDisplayElements.Partia] && Globalne.CurrentSettings.Partie
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_paleta,
                Ctx.CurrentOperation == Operation.In ? Pos.Base.strPaletaP : Pos.Base.strPaletaW,
                Set[EditingDocumentsListDisplayElements.Paleta] && Globalne.CurrentSettings.Palety
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_articlename,
                Pos.Base.strNazwaTowaru,
                Set[EditingDocumentsListDisplayElements.ArticleName]
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_symbol,
                Pos.Base.strSymbolTowaru,
                Set[EditingDocumentsListDisplayElements.Symbol]
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_list_kodean,
                Pos.Base.kodean,
                Set[EditingDocumentsListDisplayElements.KodEAN]
            );

            EditingDocumentsActivity_Common.SetView(
                view,
                Resource.Id.editingdocuments_listheader_NrKat,
                Pos.Base.NrKat,
                Set[EditingDocumentsListDisplayElements.NrKat]
            );

            view.FindViewById<TextView>(Resource.Id.editingdocuments_list_status)
                .SetBackgroundColor(Helpers.GetItemStatusColorForStatus(Pos.Status));
            // view.FindViewById<TextView>(Resource.Id.editingdocuments_list_status).SetBackgroundColor(Helpers.GetItemStatusColorForStatus(Pos.Status));
            CheckBox chb = view.FindViewById<CheckBox>(Resource.Id.documents_list_checkboxBOX);
            chb.Click -= Chb_Click;

            if (Ctx.CheckedItems.Contains(position))
                chb.Checked = true;
            else
                chb.Checked = false;

            chb.Tag = position;
            chb.Click += Chb_Click;

            if (Ctx.ShowCheckboxes)
                chb.Visibility = ViewStates.Visible;
            else
                chb.Visibility = ViewStates.Gone;

            return view;
        }

        private void Chb_Click(object sender, EventArgs e)
        {
            CheckBox Chb = (sender as CheckBox);

            if (Ctx.CheckedItems.Contains((int)Chb.Tag))
            {
                Ctx.CheckedItems.Remove((int)Chb.Tag);
                Chb.Checked = false;
            }
            else
            {
                Ctx.CheckedItems.Add((int)Chb.Tag);
                Chb.Checked = true;
            }
        }
    }
}
