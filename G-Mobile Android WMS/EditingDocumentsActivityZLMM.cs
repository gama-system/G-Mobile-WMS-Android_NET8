using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Enums;
using G_Mobile_Android_WMS.ExtendedModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WMSServerAccess.Model;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, WindowSoftInputMode = Android.Views.SoftInput.AdjustNothing | Android.Views.SoftInput.StateHidden)]
    public class EditingDocumentsActivityZLMM : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton Add;
        FloatingActionButton Delete;
        FloatingActionButton Edit;
        FloatingActionButton EditAdd;
        FloatingActionButton ChangeLoc;
        FloatingActionButton Info;
        Switch ModeSwitch;

        ListView ListView;
        TextView ItemCount;
        TextView ScanHint;
        TextView ItemSum;
        
        TextView Zbieranie;
        TextView Roznoszenie;


        public Enums.DocumentStatusTypes Status = Enums.DocumentStatusTypes.Otwarty;

        public List<DokumentVO> Documents = new List<DokumentVO>();
        public List<DocumentItemRow> Items = new List<DocumentItemRow>();
        public Enums.Operation CurrentOperation = Enums.Operation.Out;
        public Enums.ZLMMMode Mode = Enums.ZLMMMode.TwoStep;

        public int LastSelectedItemPos = -1;
        public int SelectedItemPos = -1;
        public int SelectedDefaultLoc = -1;
        static int SelectedDefaultLocSet = -1;
        public string SelectedDefaultLocName = "";
        public static string LastScanDataFromActivity = "";
        public Enums.DefaultLocType SelectedDefaultLocType = Enums.DefaultLocType.Out;

        internal static class Vars
        {
            public const string Mode = "Mode";
        }

        readonly Dictionary<Enums.EditingDocumentsListDisplayElements, int> HeaderElementsDict = new Dictionary<Enums.EditingDocumentsListDisplayElements, int>()
        {
            [Enums.EditingDocumentsListDisplayElements.DoneAmount] = Resource.Id.editingdocuments_listheader_amount,
            [Enums.EditingDocumentsListDisplayElements.SetAmount] = Resource.Id.editingdocuments_listheader_setamount,
            [Enums.EditingDocumentsListDisplayElements.GotAmount] = Resource.Id.editingdocuments_listheader_gotamount,
            [Enums.EditingDocumentsListDisplayElements.LocationIn] = Resource.Id.editingdocuments_listheader_location_in,
            [Enums.EditingDocumentsListDisplayElements.LocationOut] = Resource.Id.editingdocuments_listheader_location_out,
            [Enums.EditingDocumentsListDisplayElements.BestBefore] = Resource.Id.editingdocuments_listheader_bestbefore,
            [Enums.EditingDocumentsListDisplayElements.ProductionDate] = Resource.Id.editingdocuments_listheader_proddate,
            [Enums.EditingDocumentsListDisplayElements.SerialNumber] = Resource.Id.editingdocuments_listheader_serialnumber,
            [Enums.EditingDocumentsListDisplayElements.FlogIn] = Resource.Id.editingdocuments_listheader_flog_in,
            [Enums.EditingDocumentsListDisplayElements.FlogOut] = Resource.Id.editingdocuments_listheader_flog_out,
            [Enums.EditingDocumentsListDisplayElements.Partia] = Resource.Id.editingdocuments_listheader_partia,
            [Enums.EditingDocumentsListDisplayElements.PaletaIn] = Resource.Id.editingdocuments_listheader_paleta_in,
            [Enums.EditingDocumentsListDisplayElements.PaletaOut] = Resource.Id.editingdocuments_listheader_paleta_out,
            [Enums.EditingDocumentsListDisplayElements.Lot] = Resource.Id.editingdocuments_listheader_lot,
            [Enums.EditingDocumentsListDisplayElements.ArticleName] = Resource.Id.editingdocuments_listheader_articlename,
            [Enums.EditingDocumentsListDisplayElements.KodEAN] = Resource.Id.editingdocuments_listheader_kodean,
            [Enums.EditingDocumentsListDisplayElements.NrKat] = Resource.Id.editingdocuments_listheader_NrKat,
            [Enums.EditingDocumentsListDisplayElements.Symbol] = Resource.Id.editingdocuments_listheader_symbol,
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_editingdocuments_zlmm);
            DocType = (Enums.DocTypes)Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocType, 0);
            Status = (Enums.DocumentStatusTypes)Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocStatus, 5);
            Documents = (List<DokumentVO>)Helpers.DeserializePassedJSON(Intent, EditingDocumentsActivity_Common.Vars.DocsJSON, typeof(List<DokumentVO>));
            Mode = (Enums.ZLMMMode)Intent.GetIntExtra(Vars.Mode, 0);

            if (Mode == Enums.ZLMMMode.TwoStep)
            {
                if (DocType == Enums.DocTypes.ZL)
                {
                    DocType = Enums.DocTypes.ZLGathering;
                    Globalne.DocumentMode = DocType;
                }
                else if (DocType == Enums.DocTypes.MM)
                {
                    DocType = Enums.DocTypes.MMGathering;
                    Globalne.DocumentMode = DocType;
                }
            }

            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[DocType];

            CurrentOperation = Mode == Enums.ZLMMMode.TwoStep ? Enums.Operation.Out : Enums.Operation.OutIn;

            GetAndSetControls();

            SetDefaultLocationTo(SelectedDefaultLocSet);

            Task.Run(() => InsertData(false));
        }

        private void SetVisibilityOnHeaderItems()
        {
            Dictionary<Enums.EditingDocumentsListDisplayElements, bool> Set = Globalne.CurrentSettings.EditingDocumentsListDisplayElementsListsINNNR[DocType];

            foreach (Enums.EditingDocumentsListDisplayElements Item in HeaderElementsDict.Keys)
            {
                if (!HeaderElementsDict.ContainsKey(Item))
                    continue;

                TextView v = FindViewById<TextView>(HeaderElementsDict[Item]);
                bool VisibilityToSet;

                if (v == null)
                    continue;

                switch (Item)
                {
                    case Enums.EditingDocumentsListDisplayElements.DoneAmount: VisibilityToSet = (Set[Item]); break;
                    case Enums.EditingDocumentsListDisplayElements.SetAmount: VisibilityToSet = (Set[Item] && Documents[0].bZlecenie); break;
                    case Enums.EditingDocumentsListDisplayElements.GotAmount: VisibilityToSet = (Set[Item] && !Documents[0].bZlecenie); break;
                    case Enums.EditingDocumentsListDisplayElements.FlogIn:
                    case Enums.EditingDocumentsListDisplayElements.FlogOut:
                        VisibilityToSet = (Set[Item] && Globalne.CurrentSettings.FunkcjeLogistyczne); break;
                    case Enums.EditingDocumentsListDisplayElements.PaletaIn:
                    case Enums.EditingDocumentsListDisplayElements.PaletaOut:
                        VisibilityToSet = (Set[Item] && Globalne.CurrentSettings.Palety); break;
                    case Enums.EditingDocumentsListDisplayElements.Partia: VisibilityToSet = (Set[Item] && Globalne.CurrentSettings.Partie); break;
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
                Helpers.SetActivityHeader(this, Documents[0].strNazwa);
            }
            else
                Helpers.SetActivityHeader(this, GetString(Resource.String.editing_documents_activity_name));


            Back = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_back);
            Add = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_add);
            Edit = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_edit);
            EditAdd = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_editadd);
            Delete = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_delete);
            ChangeLoc = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_changeloc);
            Info = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_info);

            Roznoszenie = FindViewById<TextView>(Resource.Id.editingdocuments_zl_distrib);
            Zbieranie= FindViewById<TextView>(Resource.Id.editingdocuments_zl_gathering);

            ListView = FindViewById<ListView>(Resource.Id.list_view_editingdocuments);
            ItemCount = FindViewById<TextView>(Resource.Id.editingdocuments_item_count);
            ItemSum = FindViewById<TextView>(Resource.Id.editingdocuments_item_sum);

            ModeSwitch = FindViewById<Switch>(Resource.Id.editingdocuments_zlmm_mode);

            if (Mode != Enums.ZLMMMode.TwoStep)
                FindViewById<LinearLayout>(Resource.Id.editingdocuments_modeswitch).Visibility = ViewStates.Gone;

            SetVisibilityOnHeaderItems();

            Back.Click += Back_Click;
            ChangeLoc.Click += ChangeLoc_Click;
            Add.Click += Add_Click;
            Delete.Click += Delete_Click;
            Edit.Click += Edit_Click;
            EditAdd.Click += EditAdd_Click;
            Info.Click += Info_Click;

           

            ModeSwitch.CheckedChange += ModeSwitch_CheckedChange;

            if (DocType == DocTypes.ZLDistribution)
            {
                ModeSwitch.Checked = true;
                ModeSwitch_CheckedChange(null, null);
            }

            SetTextZLSwitchColor();
            SetButtonVisibilityAndFunctionAvailability();
            
        }
        private void SetDefaultLocationTo(int lokalizacjaId)
        {
            var Loc = Globalne.lokalizacjaBL.PobierzLokalizację(lokalizacjaId);

            if (Loc == null)
            {
                SelectedDefaultLoc = -1;
                SelectedDefaultLocName = "";
                Helpers.SetTextOnTextView(this, ScanHint, GetString(Resource.String.editingdocuments_activity_scanhint));
            }
            else
            {
                SelectedDefaultLoc = Loc.ID;
                SelectedDefaultLocName = Loc.strNazwa;
                SelectedDefaultLocSet = Loc.ID;

                int ResId = 0;

                if (CurrentOperation == Operation.In)
                    ResId = Resource.String.editingdocuments_activity_scanhint2_in;
                else if (CurrentOperation == Operation.Out)
                    ResId = Resource.String.editingdocuments_activity_scanhint2_out;
                else
                    ResId = Resource.String.editingdocuments_activity_scanhint2_unk;


                Helpers.SetTextOnTextView(this, ScanHint, GetString(ResId) + " " + SelectedDefaultLocName);
            }
            if (ListView?.Adapter != null)
                RunOnUiThread(() => (ListView.Adapter as EditingDocumentsListAdapter).NotifyDataSetChanged());
        }
        private void Info_Click(object sender, EventArgs e)
        {
            if (SelectedItemPos < 0)
            {
                EditingDocumentsActivity_Common.ShowLocDialog(this, -1);
            }
            else
            {
                DocumentItemRow SelectedItem = (ListView.Adapter as EditingDocumentsListAdapterZLMM)[SelectedItemPos];
                EditingDocumentsActivity_Common.ShowLocDialog(this, SelectedItem.Base.idTowaru);
            }
        }

        private void SetButtonVisibilityAndFunctionAvailability()
        {

            if (!Globalne.CurrentUserSettings.CanDeleteItems || (Documents[0].bZlecenie && !Globalne.CurrentUserSettings.CanDeleteItemsOnOrders))
                Delete.SetVisibility(ViewStates.Gone);
            else
                Delete.SetVisibility(ViewStates.Visible);

            if (Documents[0].bZlecenie || Documents.Count != 1 || (Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In))
                Add.SetVisibility(ViewStates.Gone);
            else
                Add.SetVisibility(ViewStates.Visible);

            if (Documents[0].bZlecenie || (Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In))
                EditAdd.SetVisibility(ViewStates.Gone);
            else
                EditAdd.SetVisibility(ViewStates.Visible);

            // wylaczamy tymczasowo edycje pozycji z dodawaniem -  nie miescie sie na ekranie na mniejszych ekranach (np. MC33)
            EditAdd.SetVisibility(ViewStates.Gone);


            if (Status == Enums.DocumentStatusTypes.Zamknięty)
            {
                Add.SetVisibility(ViewStates.Gone);
                Edit.SetVisibility(ViewStates.Gone);
                EditAdd.SetVisibility(ViewStates.Gone);
                Delete.Visibility = ViewStates.Gone;
            }
            else
            {
                ListView.ItemClick -= ListView_ItemClick;
                ListView.ItemClick += ListView_ItemClick;
            }
        }

        private async void ModeSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            switch (DocType)
            {
                case Enums.DocTypes.MMGathering:
                    {
                        CurrentOperation = Enums.Operation.In;
                        DocType = Enums.DocTypes.MMDistribution;
                        break;
                    }
                case Enums.DocTypes.MMDistribution:
                    {
                        CurrentOperation = Enums.Operation.Out;
                        DocType = Enums.DocTypes.MMGathering;
                        break;
                    }
                case Enums.DocTypes.ZLGathering:
                    {
                        CurrentOperation = Enums.Operation.In;
                        DocType = Enums.DocTypes.ZLDistribution;
                        break;
                    }
                case Enums.DocTypes.ZLDistribution:
                    {
                        CurrentOperation = Enums.Operation.Out;
                        DocType = Enums.DocTypes.ZLGathering;
                        break;
                    }
                case Enums.DocTypes.ZL:
                    {
                        CurrentOperation = Enums.Operation.OutIn;
                        DocType = Enums.DocTypes.ZL;
                        break;
                    }
            }
            SelectedDefaultLoc = -1;
            SelectedDefaultLocName = null;

            SetDefaultLocation(ScanHint, null, DocType, ref SelectedDefaultLoc, ref SelectedDefaultLocName, ref SelectedDefaultLocType);
            SetVisibilityOnHeaderItems();
            SetButtonVisibilityAndFunctionAvailability();
            SetTextZLSwitchColor();
            
            await RunIsBusyTaskAsync(() => InsertData(false));
        }

        private void SetTextZLSwitchColor()
        {
            switch (DocType)    
            {
                case DocTypes.ZL:
                case DocTypes.ZLGathering:
                    Roznoszenie.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Gray));
                    Roznoszenie.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    Zbieranie.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Yellow));
                    Zbieranie.SetBackgroundColor(Android.Graphics.Color.MediumOrchid);
                    break;
                case DocTypes.ZLDistribution:
                    Zbieranie.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Gray));
                    Zbieranie.SetBackgroundColor(Android.Graphics.Color.Transparent);
                    Roznoszenie.SetTextColor(Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.GreenYellow));
                    Roznoszenie.SetBackgroundColor(Android.Graphics.Color.SlateBlue);
                    break;
                default:
                    break;
            }
        }

        private void EditAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItemPos < 0)
                    return;


                if (Status != Enums.DocumentStatusTypes.Zamknięty)
                {
                    DocumentItemRow SelectedItem = (ListView.Adapter as EditingDocumentsListAdapterZLMM)[SelectedItemPos];

                    RunIsBusyAction(() =>
                    {
                        DocumentItems.EditItem(this, Documents, SelectedItem, DocType, CurrentOperation,
                                               SelectedDefaultLoc, SelectedDefaultLocType, SelectedDefaultLocName,
                                               (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, true, false);
                    });
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void ChangeLoc_Click(object sender, EventArgs e)
        {
            DefaultLocation d = await DetermineDefaultLocation(new LokalizacjaVO(), DocType, SelectedDefaultLoc);

            int magDoc = -1;

            if (d.Type == DefaultLocType.In)
                magDoc = Documents[0].intMagazynP;
            else
                magDoc = Documents[0].intMagazynW;

            EditingDocumentsActivity_Common.ChangeDefaultLocDialog(this, magDoc);

        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            SelectedItemPos = e.Position;
            Edit_Click(this, null);
        }
        int howManyClicked = 0;
        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            SelectedItemPos = e.Position;

            // jezeli zmiana pozycji na liscie to wyzeruj licznik aby nie mozna bylo wejsc jednoklikiem w inna pozycje
            if (LastSelectedItemPos != SelectedItemPos)
            {
                LastSelectedItemPos = SelectedItemPos;
                howManyClicked = 0;
            }

            if (howManyClicked >= 1)
            {
                Edit_Click(this, null);
                howManyClicked = 0;
            }
            else
                howManyClicked++;

        }

        private void Edit_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedItemPos < 0)
                    return;

                if (Status != Enums.DocumentStatusTypes.Zamknięty)
                {
                    DocumentItemRow SelectedItem = (ListView.Adapter as EditingDocumentsListAdapterZLMM)[SelectedItemPos];

                    RunIsBusyAction(() =>
                    {
                        DocumentItems.EditItem(this, Documents, SelectedItem, DocType, CurrentOperation,
                                               SelectedDefaultLoc, SelectedDefaultLocType, SelectedDefaultLocName,
                                               (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, false, false);
                    });
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
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
                
                DocumentItemRow Item = (ListView.Adapter as EditingDocumentsListAdapterZLMM)[SelectedItemPos];
                await DocumentItems.DeleteDocumentItems(this, new List<int>() { Item.Base.ID }, DocType);

            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private void Add_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                DocumentItems.AddItem(this, Documents[0], DocType, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, SelectedDefaultLocType,
                                      (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, false);
            });
        }

        async void Back_Click(object sender, EventArgs e)
        {
            if (Status == Enums.DocumentStatusTypes.Zamknięty)
                await RunIsBusyTaskAsync(() => Do_Exit_Without_Asking());
            else
                await RunIsBusyTaskAsync(() => Do_Exit());
        }

        async Task Do_Exit()
        {
            try
            {
                if (await BusinessLogicHelpers.Documents.ShowAndApplyDocumentExitOptions(this, Documents, DocType))
                {
                    IsSwitchingActivity = true;

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

        async Task Do_Exit_Without_Asking()
        {
            try
            {
                IsSwitchingActivity = true;

                foreach (DokumentVO Doc in Documents)
                    Globalne.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);

                Intent i = new Intent(this, typeof(DocumentsActivity));
                i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocType);
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
        bool isInsertBusy = false;
        async Task InsertData(bool ConfirmReady)
        {
            try
            {
                if (isInsertBusy)
                    return;

                isInsertBusy = true;
                if (LastScanDataFromActivity != string.Empty && (LastScanData == null || LastScanData.Count == 0) && (CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn))
                {
                    await ShowProgressAndDecideOperation(new List<string>() { LastScanDataFromActivity });
                    LastScanDataFromActivity = string.Empty;
                }
                    
                if (LastScanData != null && LastScanData.Count != 0)
                {
                    IsBusy = true;
                    OnScan(this, null);
                    return;
                    
                }
                else if (ConfirmReady)
                {
                    //Helpers.PlaySound(this, Resource.Raw.sound_ok);
                }

                LastSelectedItemPos = -1;
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.editing_documents_loading));

                List<DocumentItemRow> Items = await Task.Factory.StartNew(() => EditingDocumentsActivity_Common.GetData(Documents, DocType, Mode, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocType));

                RunOnUiThread(() =>
                {
                    ListView.Adapter = new EditingDocumentsListAdapterZLMM(this, Items);

                    if ((ListView.Adapter as EditingDocumentsListAdapterZLMM).Count != 0)
                        SelectedItemPos = 0;
                    else
                        SelectedItemPos = -1;

                    Helpers.SetTextOnTextView(this, ItemCount, GetString(Resource.String.global_liczba_pozycji) + " " + ListView.Adapter.Count.ToString());

                    decimal? Sum = (ListView.Adapter as EditingDocumentsListAdapterZLMM).Sum;
                    Helpers.SetTextOnTextView(this, ItemSum, GetString(Resource.String.global_suma_pozycji) + " " + (Sum == null ? "---" : Sum.ToString()));
                });

                Helpers.HideProgressDialog();



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
                isInsertBusy = false;
                IsBusy = false;
            }
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            switch (requestCode)
            {
                case (int)EditingDocumentsActivity_Common.ResultCodes.LocationsActivityResult:
                    {
                        if (resultCode == Result.Canceled)
                        {
                            SelectedDefaultLoc = -1;
                            SelectedDefaultLocName = "";

                            SetDefaultLocation(ScanHint, null, DocType, ref SelectedDefaultLoc, ref SelectedDefaultLocName, ref SelectedDefaultLocType);
                            break;
                        }

                        else if (resultCode == Result.Ok)
                        {
                            LokalizacjaVO Loc = (LokalizacjaVO)Helpers.DeserializePassedJSON(data, LocationsActivity.Results.SelectedJSON, typeof(LokalizacjaVO));

                            if (Loc == null || (Loc != null && Loc.ID == -1))
                            {
                                SetDefaultLocation(ScanHint, null, DocType, ref SelectedDefaultLoc, ref SelectedDefaultLocName, ref SelectedDefaultLocType);
                            }
                            if (Loc != null && Loc.ID != -1)
                            {
                                DefaultLocation d;

                                d = await DetermineDefaultLocation(Loc, DocType, SelectedDefaultLoc);

                                SetDefaultLocation(ScanHint, d, DocType, ref SelectedDefaultLoc, ref SelectedDefaultLocName, ref SelectedDefaultLocType);
                            }
                        }

                        break;
                    }
                case (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult:
                    {
                        if (resultCode == Result.Ok)
                        {
                            string[] Scanned = new string[0];

                            if (data != null && data.HasExtra(DocumentItemActivity_Common.Results.WereScanned))
                                Scanned = data.GetStringArrayExtra(DocumentItemActivity_Common.Results.WereScanned);

                            if (Scanned.Length != 0)
                                LastScanData = Scanned.ToList();

                            await RunIsBusyTaskAsync(() => InsertData(Globalne.CurrentSettings.InstantScanning[DocType]));
                        }

                        break;
                    }
            }

        }

        #region Old


        /*
        private Enums.DocItemStatus GetStatusForItem(PozycjaRow R)
        {
            if (Mode == Enums.ZLMMMode.OneStep)
            {
                if (R.numIloscZlecona == R.numIloscZrealizowana)
                    return Enums.DocItemStatus.Complete;
                else if (R.numIloscZrealizowana > R.numIloscZlecona)
                    return Enums.DocItemStatus.Over;
                else
                    return Enums.DocItemStatus.Incomplete;
            }
            else
            {
                if (CurrentOperation == Enums.Operation.In)
                {
                    if (R.numIloscZebrana == R.numIloscZrealizowana)
                        return Enums.DocItemStatus.Complete;
                    else if (R.numIloscZrealizowana > R.numIloscZebrana)
                        return Enums.DocItemStatus.Over;
                    else
                        return Enums.DocItemStatus.Incomplete;
                }
                else
                {
                    if (R.numIloscZebrana == R.numIloscZlecona)
                        return Enums.DocItemStatus.Complete;
                    else if (R.numIloscZebrana > R.numIloscZlecona)
                        return Enums.DocItemStatus.Over;
                    else
                        return Enums.DocItemStatus.Incomplete;
                }
            }
        }

        private List<DocumentItemRow> GetData_Private()
        {
            List<DocumentItemRow> Items = new List<DocumentItemRow>();
            List<PozycjaŚcieżkiO> PathOrder = new List<PozycjaŚcieżkiO>();

            if (!Documents[0].bZlecenie && !(Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In) && Mode != Enums.ZLMMMode.OneStep)
                PathOrder = Globalne.lokalizacjaBL.PobierzŚcieżkęZbiórki(Globalne.Magazyn.ID);

            foreach (DokumentVO Doc in Documents)
            {
                if (Doc.bZlecenie || (Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In) || Mode == Enums.ZLMMMode.OneStep)
                {
                    List<PozycjaRowZPodpowiedzią> Res = Globalne.
                                                        dokumentBL.
                                                        PobierzPozycjeIZaproponujLokalizacjeDlaDokumentu
                                                                                        (
                                                                                            Doc.ID,
                                                                                            (CurrentOperation == Enums.Operation.In || CurrentOperation == Enums.Operation.OutIn) ? true : false,
                                                                                            (CurrentOperation == Enums.Operation.Out || CurrentOperation == Enums.Operation.OutIn) ? true : false,
                                                                                            SelectedDefaultLoc
                                                                                        );
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
                    List<PozycjaRow> Pozycje = Globalne.dokumentBL.PobierzListęPozycjiRow(Doc.ID);

                    foreach (PozycjaRow R in Pozycje)
                    {
                        DocumentItemRow DocItem = new DocumentItemRow(R);
                        DocItem.Status = GetStatusForItem(R);

                        if (CurrentOperation == Enums.Operation.In || CurrentOperation == Enums.Operation.OutIn)
                        {
                            DocItem.ExIDLokalizacjaP = DocItem.Base.idLokalizacjaP;
                            DocItem.ExLokalizacjaP = DocItem.Base.strLokalizacjaP;
                        }
                        if (CurrentOperation == Enums.Operation.Out || CurrentOperation == Enums.Operation.OutIn)
                        {
                            DocItem.ExIDLokalizacjaW = DocItem.Base.idLokalizacjaW;
                            DocItem.ExLokalizacjaW = DocItem.Base.strLokalizacjaW;
                        }

                        int LocToFind = -1;

                        if (Mode != Enums.ZLMMMode.OneStep)
                            LocToFind = DocItem.ExIDLokalizacjaW;
                        else
                            LocToFind = ((CurrentOperation == Enums.Operation.In) ? DocItem.ExIDLokalizacjaP : DocItem.ExIDLokalizacjaW);

                        PozycjaŚcieżkiO Pos = PathOrder.Find(x => x.idLokalizacja == LocToFind);

                        if (Pos != null)
                            DocItem.KolejnośćNaŚcieżce = Pos.intPozycja;
                        else
                            DocItem.KolejnośćNaŚcieżce = Int32.MaxValue;

                        Items.Add(DocItem);
                    }
                }
            }

            return Items.OrderBy(x => x.Status).ThenBy(x => x.KolejnośćNaŚcieżce).ToList();
        }

        private List<DocumentItemRow> GetData_Old()
        {
            List<DocumentItemRow> Items = new List<DocumentItemRow>();
            List<PozycjaŚcieżkiO> PathOrder = Globalne.lokalizacjaBL.PobierzŚcieżkęZbiórki(Globalne.Magazyn.ID);

            foreach (DokumentVO Doc in Documents)
            {
                List<PozycjaRow> Pozycje = Globalne.dokumentBL.PobierzListęPozycjiRow(Doc.ID);

                string LocDefault = "";
                if (Doc.intLokalizacjaPozycji >= 0)
                    LocDefault = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL), "PobierzNazwęLokalizacji", Doc.intLokalizacjaPozycji);

                foreach (PozycjaRow R in Pozycje)
                {
                    DocumentItemRow DocItem = new DocumentItemRow(R);
                    DocItem.Status = GetStatusForItem(R);

                    if (Doc.bZlecenie || (Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In) || Mode == Enums.ZLMMMode.OneStep)
                    {

                        if (R.idLokalizacjaW < 0 && Doc.intLokalizacjaPozycji < 0)
                        {
                            PodpowiedźLokalizacjiO Pdp = Globalne.przychrozchBL.ZaproponujLokalizacjęDlaRozchodu_Nazwa_PozycjaNaŚć(R.idTowaru,
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
                        }
                        else if (Doc.intLokalizacjaPozycji >= 0)
                        {
                            DocItem.ExIDLokalizacjaW = Doc.intLokalizacjaPozycji;
                            DocItem.ExLokalizacjaW = LocDefault;
                        }
                        else
                        {
                            DocItem.ExIDLokalizacjaW = R.idLokalizacjaW;
                            DocItem.ExLokalizacjaW = R.strLokalizacjaW;
                        }


                        if (R.idLokalizacjaP < 0 && Doc.intLokalizacjaPozycji < 0)
                        {
                            PodpowiedźLokalizacjiO Pdp = Globalne.przychrozchBL.ZaproponujLokalizacjęDlaPrzychodu_Nazwa_PozycjaNaŚć(R.idTowaru, Doc.intMagazynP,
                                                                                                                                    new List<int>() { DocItem.ExIDLokalizacjaW });


                            DocItem.ExIDLokalizacjaP = Pdp.ID;
                            DocItem.ExLokalizacjaP = Pdp.strNazwa;
                        }
                        else if (Doc.intLokalizacjaPozycji >= 0)
                        {
                            DocItem.ExIDLokalizacjaP = Doc.intLokalizacjaPozycji;
                            DocItem.ExLokalizacjaP = LocDefault;
                        }
                        else
                        {
                            DocItem.ExIDLokalizacjaP = R.idLokalizacjaP;
                            DocItem.ExLokalizacjaP = R.strLokalizacjaP;
                        }

                    }
                    else
                    {
                        if (CurrentOperation == Enums.Operation.In || CurrentOperation == Enums.Operation.OutIn)
                        {
                            DocItem.ExIDLokalizacjaP = DocItem.Base.idLokalizacjaP;
                            DocItem.ExLokalizacjaP = DocItem.Base.strLokalizacjaP;
                        }
                        if (CurrentOperation == Enums.Operation.Out || CurrentOperation == Enums.Operation.OutIn)
                        {
                            DocItem.ExIDLokalizacjaW = DocItem.Base.idLokalizacjaW;
                            DocItem.ExLokalizacjaW = DocItem.Base.strLokalizacjaW;
                        }
                    }

                    int LocToFind = -1;

                    if (Mode != Enums.ZLMMMode.OneStep)
                        LocToFind = DocItem.ExIDLokalizacjaW;
                    else
                        LocToFind = ((CurrentOperation == Enums.Operation.In) ? DocItem.ExIDLokalizacjaP : DocItem.ExIDLokalizacjaW);

                    PozycjaŚcieżkiO Pos = PathOrder.Find(x => x.idLokalizacja == LocToFind);

                    if (Pos != null)
                        DocItem.KolejnośćNaŚcieżce = Pos.intPozycja;
                    else
                        DocItem.KolejnośćNaŚcieżce = Int32.MaxValue;

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
            
            LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], true);

            if (Loc != null && Loc.ID >= 0)
            {
                DefaultLocation d;

                d = await DetermineDefaultLocation(Loc, DocType, SelectedDefaultLoc);

                int LocDoc;

                if (d.Type == DefaultLocType.In)
                    LocDoc = Documents[0].intMagazynP;
                else
                    LocDoc = Documents[0].intMagazynW;

                if (LocDoc != Loc.idMagazyn)
                {
                    await Helpers.AlertAsyncWithConfirm(this, Resource.String.editingdocuments_locnoinwarehouse, Resource.Raw.sound_miss);
                    LastScanData = null;
                    return false;
                }

                SetDefaultLocation(ScanHint, d, DocType, ref SelectedDefaultLoc, ref SelectedDefaultLocName, ref SelectedDefaultLocType);

                return false;
            }
            else
                return true;
        }

        async protected override void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);

            if (Status == Enums.DocumentStatusTypes.Zamknięty)
                return;

            await RunIsBusyTaskAsync(() => ShowProgressAndDecideOperation(LastScanData));
        }

        private async Task<DefaultLocType> AskDefaultLocType(DocTypes Type)
        {
            if (DocumentItems.IsZLMM(Type))
            {
                if (DocumentItems.IsDistributionMode(Type, CurrentOperation))
                    return DefaultLocType.In;
                else if (DocumentItems.IsGatheringMode(Type, CurrentOperation))
                    return DefaultLocType.Out;
                else
                {

                    string[] Options = {
                                        GetString(Resource.String.editingdocuments_buffertype_in),
                                        GetString(Resource.String.editingdocuments_buffertype_out)
                                   };

                    string Res = await UserDialogs.Instance.ActionSheetAsync(GetString(Resource.String.editingdocuments_buffertype),
                                                                             GetString(Resource.String.global_cancel),
                                                                             "",
                                                                             null,
                                                                             Options);

                    if (Res == GetString(Resource.String.editingdocuments_buffertype_in))
                        return DefaultLocType.In;
                    else if (Res == GetString(Resource.String.editingdocuments_buffertype_out))
                        return DefaultLocType.Out;
                    else
                        return DefaultLocType.None;
                }
            }

            return DefaultLocType.None;
        }

        private async Task<DefaultLocation> DetermineDefaultLocation(LokalizacjaVO Loc, DocTypes Type, int SelectedDefaultLoc)
        {
            DefaultLocType ZLMMT = DefaultLocType.Out; //await AskDefaultLocType(Type);

            if (DocumentItems.IsZLMM(Type))
            {
                if (CurrentOperation == Operation.In)
                    ZLMMT = DefaultLocType.In;
                else if (CurrentOperation == Operation.Out)
                    ZLMMT = DefaultLocType.Out;
                else if (CurrentOperation == Operation.OutIn)
                    ZLMMT = DefaultLocType.Out;
            }

            return new DefaultLocation() { IDLoc = Loc.ID, Type = ZLMMT, LocName = Loc.strNazwa };
        }

        private void SetDefaultLocation(TextView ScanHint, DefaultLocation Loc, Enums.DocTypes Type, ref int SelectedDefaultLoc,
                                        ref string SelectedDefaultLocName, ref DefaultLocType SelectedDefaultLocType)
        {
            if (Loc == null)
            {
                SelectedDefaultLoc = -1;
                SelectedDefaultLocName = "";
                SelectedDefaultLocType = DefaultLocType.None;
                SelectedDefaultLocSet = -1;
            }
            else
            {
                SelectedDefaultLoc = Loc.IDLoc;
                SelectedDefaultLocName = Loc.LocName;
                SelectedDefaultLocType = Loc.Type;
                SelectedDefaultLocSet = Loc.IDLoc;

                if (SelectedDefaultLoc < 0)
                    Helpers.SetTextOnTextView(this, ScanHint, GetString(Resource.String.documents_activity_scanhintZLMM));
                else
                {
                    int ResId = 0;

                    if (SelectedDefaultLocType == DefaultLocType.In)
                    {
                        ResId = Resource.String.editingdocuments_activity_scanhint2_in;
                    }
                    else if (SelectedDefaultLocType == DefaultLocType.Out)
                    {
                        ResId = Resource.String.editingdocuments_activity_scanhint2_out;
                    }
                    else
                    {
                        ResId = Resource.String.editingdocuments_activity_scanhint2_unk;
                        //   Helpers.CenteredToast("Skanowanie fajna rzecz", ToastLength.Short);
                    }


                    Helpers.SetTextOnTextView(this, ScanHint, GetString(ResId) + " " + SelectedDefaultLocName);
                }

                if (ListView?.Adapter != null)
                    RunOnUiThread(() => (ListView.Adapter as EditingDocumentsListAdapterZLMM).NotifyDataSetChanged());
            }
        }

        async Task ShowProgressAndDecideOperation(List<string> Scanned)
        {
            if (Scanned?.Count == 0)
                return;

            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            await Task.Run(async () =>
            {
                try
                {
                    await Task.Run(async () => await FindPositionAndEnter(Scanned));
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
                await Helpers.AlertAsyncWithConfirm(this, Resource.String.articles_not_found, Resource.Raw.sound_miss);
                LastScanData = null;
                return;
            }

            if (Kod.TowaryJednostkiWBazie != null && Kod.TowaryJednostkiWBazie.Count > 1)
            {
                TowarJednostkaO Towarjedn = await Indexes.SelectOneArticleFromMany(this, Kod.TowaryJednostkiWBazie);

                if (Towarjedn.IDTowaru >= 0)
                    Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>() { Towarjedn };
                else
                    return;
            }

            if (Documents[0].bZlecenie || (Mode == Enums.ZLMMMode.TwoStep && CurrentOperation == Enums.Operation.In))
            {
                var result = from x in (ListView.Adapter as EditingDocumentsListAdapterZLMM).Items
                             where

                             (x.Base.idTowaru == Kod.TowaryJednostkiWBazie[0].IDTowaru)

                             &&
                             // jednostka zdaje sie byc zbedna przy edycji pozycji
                             //(x.Base.idJednostkaMiary == Kod.TowaryJednostkiWBazie[0].IDJednostki)

                             //&&

                             ((Kod.Partia == x.Base.strPartia) || (x.Base.strPartia == ""))

                             &&

                             ((CurrentOperation == Enums.Operation.In ? (Kod.Paleta == x.Base.strPaletaP) : (Kod.Paleta == x.Base.strPaletaW))
                             || (CurrentOperation == Enums.Operation.In ? (x.Base.strPaletaP == "") : (x.Base.strPaletaW == "")))

                             &&

                             (Mode == Enums.ZLMMMode.TwoStep ?
                                CurrentOperation == Enums.Operation.In ?
                                    (x.Base.numIloscZrealizowana != x.Base.numIloscZebrana) :
                                    (x.Base.numIloscZebrana != x.Base.numIloscZlecona)
                                :
                                (x.Base.numIloscZrealizowana != x.Base.numIloscZlecona))


                             select x;


                if (result.Count() != 0)
                {
                    DocumentItems.EditItem(this, Documents, result.OrderBy(x => x.KolejnośćNaŚcieżce).First(), DocType, CurrentOperation,
                                           SelectedDefaultLoc, SelectedDefaultLocType, SelectedDefaultLocName,
                                           (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, false, true, Kod);
                }
                else
                {
                    IEnumerable<DocumentItemRow> resultFinished = new List<DocumentItemRow>();

                    if ((ListView.Adapter as EditingDocumentsListAdapterZLMM)?.Items != null)
                    {

                        resultFinished = from x in (ListView.Adapter as EditingDocumentsListAdapterZLMM)?.Items
                                             where

                                             (x.Base.idTowaru == Kod.TowaryJednostkiWBazie[0].IDTowaru) &&

                                             //  do edycji jednostka wydaje sie byc zbedna                                           
                                             //(x.Base.idJednostkaMiary == Kod.TowaryJednostkiWBazie[0].IDJednostki) &&

                                             ((Kod.Partia == x.Base.strPartia) || (x.Base.strPartia == "")) &&


                                             ((CurrentOperation == Enums.Operation.In ? (Kod.Paleta == x.Base.strPaletaP) : (Kod.Paleta == x.Base.strPaletaW))
                                             || (CurrentOperation == Enums.Operation.In ? (x.Base.strPaletaP == "") : (x.Base.strPaletaW == "")))

                                             select x;
                    }
                    if (resultFinished.Count() != 0)
                    {
                        DocumentItems.EditItem(this, Documents, resultFinished.OrderBy(x => x.KolejnośćNaŚcieżce).First(), DocType, CurrentOperation,
                                               SelectedDefaultLoc, SelectedDefaultLocType, SelectedDefaultLocName,
                                               (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, false, true);
                    }
                    else
                    {
                        await Helpers.AlertAsyncWithConfirm(this, Resource.String.editingdocuments_cannot_find_item, Resource.Raw.sound_miss);
                        LastScanData = null;
                        return;
                    }
                }
            }
            else
            {
                DocumentItems.AddItem(this, Documents[0], DocType, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, SelectedDefaultLocType,
                                      (int)EditingDocumentsActivity_Common.ResultCodes.DocumentItemActivityResult, true, Kod);
            }
        }
    }

    internal class EditingDocumentsListAdapterZLMM : BaseAdapter<ExtendedModel.DocumentItemRow>
    {
        public List<DocumentItemRow> Items;
        readonly EditingDocumentsActivityZLMM Ctx;

        public EditingDocumentsListAdapterZLMM(EditingDocumentsActivityZLMM Ctx, List<ExtendedModel.DocumentItemRow> Items) : base()
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
                    if ((Ctx.Mode == Enums.ZLMMMode.OneStep) || (Ctx.CurrentOperation == Enums.Operation.In))
                        return Items.Sum(x => x.Base.numIloscZrealizowana);
                    else
                        return Items.Sum(x => x.Base.numIloscZebrana);
                }
                else
                    return null;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            Dictionary<Enums.EditingDocumentsListDisplayElements, bool> Set = Globalne.CurrentSettings.EditingDocumentsListDisplayElementsListsINNNR[ActivityWithScanner.DocType];

            var Pos = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_editingdocuments_zlmm, null);


            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_amount, Pos.Base.numIloscZrealizowana.ToString() + " " + Pos.Base.strNazwaJednostki,
                                                    Set[Enums.EditingDocumentsListDisplayElements.DoneAmount]);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_gotamount, Pos.Base.numIloscZebrana.ToString() + " " + Pos.Base.strNazwaJednostki,
                                                    Set[Enums.EditingDocumentsListDisplayElements.GotAmount] && !Ctx.Documents[0].bZlecenie);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_setamount, Pos.Base.numIloscZlecona.ToString() + " " + Pos.Base.strNazwaJednostki,
                                                    Set[Enums.EditingDocumentsListDisplayElements.SetAmount] && Ctx.Documents[0].bZlecenie); 
            
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_location_in, Pos.ExLokalizacjaP,
                                                    Set[Enums.EditingDocumentsListDisplayElements.LocationIn]);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_location_out, Pos.ExLokalizacjaW,
                                                    Set[Enums.EditingDocumentsListDisplayElements.LocationOut]);

            if (Ctx.SelectedDefaultLoc >= 0)
            {
                if (Ctx.SelectedDefaultLocType == DefaultLocType.In && Pos.Base.idLokalizacjaP < 0)
                {
                    TextView View = view.FindViewById<TextView>(Resource.Id.editingdocuments_list_location_in);
                    View.Text = Ctx.SelectedDefaultLocName;
                }

                if (Ctx.SelectedDefaultLocType == DefaultLocType.Out && Pos.Base.idLokalizacjaW < 0)
                {
                    TextView View2 = view.FindViewById<TextView>(Resource.Id.editingdocuments_list_location_out);
                    View2.Text = Ctx.SelectedDefaultLocName;
                }
            }

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_bestbefore,
                                                    Pos.Base.dtDataPrzydatności.Year > 2900 ? "---" : Pos.Base.dtDataPrzydatności.ToString(Globalne.CurrentSettings.DateFormat),
                                                    Set[Enums.EditingDocumentsListDisplayElements.BestBefore]);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_proddate,
                                                    Pos.Base.dtDataProdukcji.Year > 2900 ? "---" : Pos.Base.dtDataPrzydatności.ToString(Globalne.CurrentSettings.DateFormat),
                                                    Set[Enums.EditingDocumentsListDisplayElements.ProductionDate]);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_serialnumber, Pos.Base.strNumerySeryjne,
                                                    Set[Enums.EditingDocumentsListDisplayElements.SerialNumber]);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_lot, Pos.Base.strLoty,
                                                    Set[Enums.EditingDocumentsListDisplayElements.Lot]);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_flog_in, Pos.Base.strFunkcjiLogistycznejP,
                                                    Set[Enums.EditingDocumentsListDisplayElements.FlogIn] && Globalne.CurrentSettings.FunkcjeLogistyczne);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_flog_out, Pos.Base.strFunkcjiLogistycznejW,
                                                    Set[Enums.EditingDocumentsListDisplayElements.FlogOut] && Globalne.CurrentSettings.FunkcjeLogistyczne);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_partia, Pos.Base.strPartia,
                                                    Set[Enums.EditingDocumentsListDisplayElements.Partia] && Globalne.CurrentSettings.Partie);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_paleta_in, Pos.Base.strPaletaP,
                                                    Set[Enums.EditingDocumentsListDisplayElements.PaletaIn] && Globalne.CurrentSettings.Palety);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_paleta_out, Pos.Base.strPaletaW,
                                                    Set[Enums.EditingDocumentsListDisplayElements.PaletaOut] && Globalne.CurrentSettings.Palety);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_symbol, Pos.Base.strSymbolTowaru,
                                                    Set[Enums.EditingDocumentsListDisplayElements.Symbol]);
            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_articlename, Pos.Base.strNazwaTowaru,
                                                    Set[Enums.EditingDocumentsListDisplayElements.ArticleName]);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_list_kodean, Pos.Base.kodean,
                                                    Set[Enums.EditingDocumentsListDisplayElements.KodEAN]);

            EditingDocumentsActivity_Common.SetView(view, Resource.Id.editingdocuments_listheader_NrKat, Pos.Base.NrKat,
                                        Set[Enums.EditingDocumentsListDisplayElements.NrKat]);
            view.FindViewById<TextView>(Resource.Id.editingdocuments_list_status).SetBackgroundColor(Helpers.GetItemStatusColorForStatus(Pos.Status));

            return view;
        }
    }
}