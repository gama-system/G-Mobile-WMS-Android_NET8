using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Controls;
using G_Mobile_Android_WMS.Enums;
using G_Mobile_Android_WMS.ExtendedModel;
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
    public class MultipickingActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton OK;
        FloatingActionButton Pause;

        FloatingActionButton EditFlog;
        FloatingActionButton EditPaleta;
        FloatingActionButton EditPartia;

        ScrollView MultipickingView;

        LinearLayout PreviousLocLay;
        TextView PreviousLoc;
        TextView Article;
        TextView Amount;
        TextView From;
        TextView To;
        TextView Description;
        NumericUpDown NumAmount;
        EditText Partia;
        EditText Paleta;
        EditText SerialNumber;
        TextView kodean;
        EditText Lot;
        EditText ProdDate;
        EditText BestBefore;
        TextView Flog;
        TextView ScanHint;
        TextView NrKat;

        System.Timers.Timer ErrorTimer = new System.Timers.Timer() { Interval = 200 };
        int TimerErrorMessage = -1;

        public DocTypes DocType = DocTypes.WZ;
        public DocumentStatusTypes Status = DocumentStatusTypes.Otwarty;

        public List<DokumentVO> Documents = new List<DokumentVO>();
        public Dictionary<int, string> DocLocNames = new Dictionary<int, string>();
        public List<DocumentItemRow> Items = new List<DocumentItemRow>();
        public DocumentItemRow LastItem = null;
        public DocumentItemRow CurrentItem = null;
        public DokumentVO CurrentDoc = null;

        public List<int> IDItemsAlreadyDone = new List<int>();
        public List<int> LocationsAlreadyBeenTo = new List<int>();
        public int LowestPathID = -1;
        public ScanningSteps ScanningStep = ScanningSteps.NONE;

        public enum ScanningSteps
        {
            OUT,
            ART,
            IN,
            NONE,
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_multipicking);

            DocType = (DocTypes)Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocType, 0);
            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[DocType];
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

            GetControls();

            Task.Run(() => Next(true));
        }

        private void SetScanHintBasedOnCurrentScanStep()
        {
            switch (ScanningStep)
            {
                case ScanningSteps.OUT:
                    Helpers.SetTextOnTextView(
                        this,
                        ScanHint,
                        GetString(Resource.String.multip_scanhint1)
                    );
                    From.SetBackgroundColor(Color.Green);
                    To.SetBackgroundColor(Color.LightGray);

                    break;
                case ScanningSteps.ART:
                    Helpers.SetTextOnTextView(
                        this,
                        ScanHint,
                        GetString(Resource.String.multip_scanhint2)
                    );
                    From.SetBackgroundColor(Color.LightGray);
                    Article.SetBackgroundColor(Color.Green);
                    break;

                case ScanningSteps.IN:
                    Helpers.SetTextOnTextView(
                        this,
                        ScanHint,
                        GetString(Resource.String.multip_scanhint3)
                    );
                    Article.SetBackgroundColor(Color.LightGray);
                    To.SetBackgroundColor(Color.Green);
                    break;
                default:
                    Helpers.SetTextOnTextView(this, ScanHint, "");
                    break;
            }
        }

        private void SetFirstScanAction()
        {
            if (Globalne.CurrentSettings.MultipickingConfirmOutLocation)
                ScanningStep = ScanningSteps.OUT;
            else if (Globalne.CurrentSettings.MultipickingConfirmArticle)
                ScanningStep = ScanningSteps.ART;
            else if (Globalne.CurrentSettings.MultipickingConfirmOutLocation)
                ScanningStep = ScanningSteps.IN;
            else
                ScanningStep = ScanningSteps.NONE;

            SetScanHintBasedOnCurrentScanStep();
        }

        private void GetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.multipicking_activity_name));

            // Back = FindViewById<FloatingActionButton>(Resource.Id.multipicking_back);
            OK = FindViewById<FloatingActionButton>(Resource.Id.multipicking_ok);
            Pause = FindViewById<FloatingActionButton>(Resource.Id.multipicking_pause);
            PreviousLocLay = FindViewById<LinearLayout>(Resource.Id.document_item_layout_prevloc);
            PreviousLoc = FindViewById<TextView>(Resource.Id.document_item_prevloc);
            Article = FindViewById<TextView>(Resource.Id.document_item_article);
            Amount = FindViewById<TextView>(Resource.Id.document_item_multipsetamountX);
            From = FindViewById<TextView>(Resource.Id.MultipZabierzZ);
            To = FindViewById<TextView>(Resource.Id.MultipWłóżDo);
            Description = FindViewById<TextView>(Resource.Id.MultipOpis);
            Partia = FindViewById<EditText>(Resource.Id.document_item_partia);
            Paleta = FindViewById<EditText>(Resource.Id.document_item_paleta);
            SerialNumber = FindViewById<EditText>(Resource.Id.document_item_serial);
            kodean = FindViewById<TextView>(Resource.Id.document_item_kodean);
            NrKat = FindViewById<TextView>(Resource.Id.document_item_NrKat);
            Lot = FindViewById<EditText>(Resource.Id.document_item_lot);
            Flog = FindViewById<TextView>(Resource.Id.document_item_flog);
            ProdDate = FindViewById<EditText>(Resource.Id.document_item_proddate);
            BestBefore = FindViewById<EditText>(Resource.Id.document_item_bestbefore);
            ScanHint = FindViewById<TextView>(Resource.Id.scanhint);
            MultipickingView = FindViewById<ScrollView>(Resource.Id.activity_multip_scrollcontent);

            SetFirstScanAction();

            EditFlog = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_flog);
            EditPaleta = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_paleta);
            EditPartia = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_partia);

            NumAmount = FindViewById<NumericUpDown>(Resource.Id.MultipAmount);

            // Kompatybilność z 5.0
            NumAmount.Initialize();

            //  Back.Click += Back_Click;
            // nie wiem dlaczego ale przycisk jest wylaczony bo sie nie podobal
            OK.Click += OK_Click;
            EditFlog.Click += EditFlog_Click;
            EditPaleta.Click += EditPaleta_Click;
            EditPartia.Click += EditPartia_Click;
            Partia.Click += OnClickTargettableTextView;
            Partia.FocusChange += OnFocusChangeTargettableTextView;
            Paleta.Click += OnClickTargettableTextView;
            Paleta.FocusChange += OnFocusChangeTargettableTextView;
            SerialNumber.Click += OnClickTargettableTextView;
            SerialNumber.FocusChange += OnFocusChangeTargettableTextView;
            Lot.Click += OnClickTargettableTextView;
            Lot.FocusChange += OnFocusChangeTargettableTextView;
            ProdDate.FocusChange += DateFields_FocusChange;
            BestBefore.FocusChange += DateFields_FocusChange;
            Pause.Click += Pause_Click;

            ErrorTimer.Elapsed += Timer_Elapsed;

            DocumentItemActivity_Common.SetVisibilityOfFields(
                this,
                DocType,
                true,
                ItemActivityMode.Split
            );
        }

        private void EditPartia_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(PartieActivity));
                i.PutExtra(PartieActivity.Vars.IDMagazynu, CurrentDoc.intMagazynW);

                i.PutExtra(PartieActivity.Vars.IDDokumentu, CurrentDoc.ID);
                i.PutExtra(PartieActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(PartieActivity.Vars.IDLokalizacji, CurrentItem.ExIDLokalizacjaW);
                i.PutExtra(PartieActivity.Vars.IDPalety, (int)Paleta.Tag);
                i.PutExtra(PartieActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
                i.PutExtra(PartieActivity.Vars.Rozchód, true);
                i.PutExtra(PartieActivity.Vars.AskOnStart, false);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.PartiaActivityResult
                );
            });
        }

        private void EditPaleta_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(PaletyActivity));
                i.PutExtra(PaletyActivity.Vars.IDMagazynu, CurrentDoc.intMagazynW);

                i.PutExtra(PaletyActivity.Vars.IDDokumentu, CurrentDoc.ID);
                i.PutExtra(PaletyActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(PaletyActivity.Vars.IDLokalizacji, CurrentItem.ExIDLokalizacjaW);
                i.PutExtra(PaletyActivity.Vars.IDPartii, (int)Partia.Tag);
                i.PutExtra(PaletyActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
                i.PutExtra(PaletyActivity.Vars.Rozchód, true);
                i.PutExtra(PaletyActivity.Vars.AskOnStart, false);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.PaletaActivityResult
                );
            });
        }

        async void EditFlog_Click(object sender, EventArgs e)
        {
            FunkcjaLogistycznaO SFlog = null;

            await RunIsBusyTaskAsync(() =>
            {
                var Ret = BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                    this,
                    CurrentDoc.intMagazynW,
                    Flog
                );
                SFlog = Ret.Result;
                return Ret;
            });

            if (SFlog != null)
                Flog.Tag = SFlog.ID;
        }

        private void DateFields_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if ((sender as EditText).IsFocused)
            {
                if (
                    !DateTime.TryParseExact(
                        (sender as EditText).Text,
                        Globalne.CurrentSettings.DateFormat,
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime Current
                    )
                )
                    Current = DateTime.Now;

                RunIsBusyAction(() => Helpers.OpenDateEditor(this, (EditText)sender, Current));
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            try
            {
                if (resultCode == Result.Ok)
                {
                    switch (requestCode)
                    {
                        case (int)DocumentItemActivity_Common.ResultCodes.PartiaActivityResult:
                        {
                            PartiaO Prt = (PartiaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    PartieActivity.Results.SelectedJSON,
                                    typeof(PartiaO)
                                );

                            if (Prt != null && Prt.ID >= 0)
                                Helpers.SetTextOnTextView(this, Partia, Prt.strKod);

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.PaletaActivityResult:
                        {
                            PaletaO Pal = (PaletaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    PartieActivity.Results.SelectedJSON,
                                    typeof(PaletaO)
                                );

                            if (Pal != null && Pal.ID >= 0)
                                Helpers.SetTextOnTextView(this, Paleta, Pal.strOznaczenie);

                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private PozycjaVO GetDBObject(PozycjaVO Unedited)
        {
            PozycjaVO Edited = (PozycjaVO)Helpers.ObjectCopy(Unedited, typeof(PozycjaVO));

            if (Globalne.CurrentSettings.Partie)
            {
                Edited.idPartia = Serwer.partiaBL.PobierzIDPartii(Partia.Text);
                Edited.strPartia = Partia.Text;
            }
            else
            {
                Edited.idPartia = -1;
                Edited.strPartia = "";
            }

            if (Globalne.CurrentSettings.Palety)
            {
                Edited.idPaletaW = Serwer.paletaBL.PobierzIDPalety(Paleta.Text);
                Edited.strPaletaW = Paleta.Text;
            }
            else
            {
                Edited.strPaletaW = "";
                Edited.idPaletaW = -1;
            }

            if (Globalne.CurrentSettings.FunkcjeLogistyczne)
                Edited.idFunkcjiLogistycznejW = (int)Flog.Tag;
            else
                Edited.idFunkcjiLogistycznejW = -1;

            Edited.idLokalizacjaW = CurrentItem.ExIDLokalizacjaW;
            Edited.strLoty = Lot.Text;
            Edited.strNumerySeryjne = SerialNumber.Text;
            Edited.kodean = kodean.Text;

            if (NrKat != null)
            {
                Edited.NrKat = NrKat.Text;
            }

            try
            {
                Edited.dtDataProdukcji = DateTime.ParseExact(
                    ProdDate.Text,
                    Globalne.CurrentSettings.DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture
                );
                Edited.dtDataPrzydatności = DateTime.ParseExact(
                    BestBefore.Text,
                    Globalne.CurrentSettings.DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture
                );
            }
            catch (Exception)
            {
                AutoException.ThrowIfNotNull(this, Resource.String.global_dateerror);
                return null;
            }

            Edited.intZmodyfikowanyPrzez = Globalne.Operator.ID;
            Edited.numIloscZrealizowana = NumAmount.Value;

            return Edited;
        }

        private async Task<bool> CheckSanity(PozycjaVO Edited)
        {
            Color RequiredTextField = new Color(
                ContextCompat.GetColor(this, Resource.Color.required_text_field)
            );
            Color OkTextField = new Color(
                ContextCompat.GetColor(this, Resource.Color.ok_text_field)
            );

            Dictionary<DocumentItemFields, bool> Dict = Globalne
                .CurrentSettings
                .RequiredDocItemFields[DocType];

            bool OK = true;

            foreach (DocumentItemFields Key in Dict.Keys)
            {
                switch (Key)
                {
                    case DocumentItemFields.Article:
                        OK = (!Dict[Key] || Edited.idTowaru >= 0);
                        break;
                    case DocumentItemFields.Flog:
                        OK = (
                            !Dict[Key]
                            || !Globalne.CurrentSettings.FunkcjeLogistyczne
                            || Edited.idFunkcjiLogistycznejW >= 0
                        );
                        break;
                    case DocumentItemFields.Location:
                        OK = (!Dict[Key] || Edited.idLokalizacjaW >= 0);
                        break;
                    case DocumentItemFields.DataProdukcji:
                    {
                        DateTime ProdDateVal = DateTime.ParseExact(
                            ProdDate.Text,
                            Globalne.CurrentSettings.DateFormat,
                            System.Globalization.CultureInfo.InvariantCulture
                        );
                        OK = (!Dict[Key] || ProdDateVal.Year < 2900);
                        break;
                    }
                    case DocumentItemFields.DataPrzydatności:
                    {
                        DateTime BestBeforeVal = DateTime.ParseExact(
                            BestBefore.Text,
                            Globalne.CurrentSettings.DateFormat,
                            System.Globalization.CultureInfo.InvariantCulture
                        );
                        OK = (!Dict[Key] || BestBeforeVal.Year < 2900);
                        break;
                    }
                    case DocumentItemFields.Paleta:
                        OK = (!Dict[Key] || !Globalne.CurrentSettings.Palety || Paleta.Text != "");
                        break;
                    case DocumentItemFields.Partia:
                        OK = (!Dict[Key] || !Globalne.CurrentSettings.Partie || Partia.Text != "");
                        break;
                    case DocumentItemFields.Lot:
                        OK = (!Dict[Key] || Lot.Text != "");
                        break;
                    case DocumentItemFields.SerialNumber:
                        OK = (!Dict[Key] || SerialNumber.Text != "");
                        break;
                    case DocumentItemFields.Unit:
                        OK = (!Dict[Key] || Edited.idJednostkaMiary >= 0);
                        break;
                    case DocumentItemFields.KodEAN:
                        OK = (!Dict[Key] || Edited.kodean != "");
                        break;
                    case DocumentItemFields.NrKat:
                        OK = (!Dict[Key] || Edited.NrKat != "");
                        break;
                }

                View v = FindViewById<View>(DocumentItemActivity_Common.RequiredElementsDict[Key]);

                RunOnUiThread(() =>
                {
                    if (OK != true)
                    {
                        if (v != null)
                            v.SetBackgroundColor(RequiredTextField);
                    }
                });

                if (OK != true)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.documentitem_not_filled
                    );
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> DoSaveCurrentItem()
        {
            try
            {
                if (NumAmount.Value <= 0)
                    return true;

                PozycjaVO Original = Serwer.dokumentBL.PobierzPozycję(CurrentItem.Base.ID);
                PozycjaVO Edited = GetDBObject(Original);

                if (!await CheckSanity(Edited))
                    return false;

                if (
                    CurrentDoc.bZlecenie
                    && Edited.numIloscZrealizowana > Edited.numIloscZlecona
                    && !CurrentDoc.bDopuszczalnePrzekroczenieIlosci
                )
                    AutoException.ThrowIfNotNull(
                        this,
                        Resource.String.documentitem_creationedition_toomuch
                    );

                if (Edited.numIloscZrealizowana >= Edited.numIloscZlecona)
                {
                    AutoException.ThrowIfNotNull(
                        this,
                        ErrorType.ItemCreationError,
                        Serwer.dokumentBL.EdytujPozycję(
                            Helpers.StringDocType(DocType),
                            Edited,
                            CurrentDoc.bIgnorujBlokadePartii
                        )
                    );
                }
                else
                {
                    Edited.numIloscZlecona = NumAmount.Value;

                    BusinessLogicHelpers.DocumentItems.EditSplitItem(
                        this,
                        Original,
                        true,
                        DocType,
                        Operation.Out,
                        Edited.numIloscZrealizowana
                    );

                    try
                    {
                        AutoException.ThrowIfNotNull(
                            this,
                            ErrorType.ItemCreationError,
                            Serwer.dokumentBL.ZróbPozycję(
                                Helpers.StringDocType(DocType),
                                Edited,
                                CurrentDoc.bIgnorujBlokadePartii
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        Helpers.HandleError(this, ex);
                        BusinessLogicHelpers.DocumentItems.EditSplitItem(
                            this,
                            Original,
                            false,
                            DocType,
                            Operation.Out
                        );
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return false;
            }
        }

        private async void OK_Click(object sender, EventArgs e)
        {
            bool Res = false;

            await RunIsBusyTaskAsync(async () =>
            {
                if (!LocationsAlreadyBeenTo.Contains(CurrentItem.ExIDLokalizacjaW))
                    LocationsAlreadyBeenTo.Add(CurrentItem.ExIDLokalizacjaW);

                if (!IDItemsAlreadyDone.Contains(CurrentItem.Base.ID))
                    IDItemsAlreadyDone.Add(CurrentItem.Base.ID);

                // Sprawdzamy, czy są jeszcze pozycje na liście które mają zaproponowaną tą samą lokalizację, która była proponowana dla obecnej pozycji, o innym towarze.
                // Jeśli nie, lokalizacja dodawana jest do pomijanych.
                DocumentItemRow ItemsNotYetDoneWithSameLocation = Items.Find(x =>
                    !IDItemsAlreadyDone.Contains(x.Base.ID)
                    && x.ExIDLokalizacjaW == CurrentItem.ExIDLokalizacjaW
                    && (
                        x.Base.ID != CurrentItem.Base.ID
                        || x.Base.idTowaru != CurrentItem.Base.idTowaru
                        || x.Base.idPaletaW != CurrentItem.Base.idPaletaW
                        || x.Base.idPartia != CurrentItem.Base.idPartia
                        || x.Base.idFunkcjiLogistycznejW != CurrentItem.Base.idFunkcjiLogistycznejW
                    )
                );

                if (ItemsNotYetDoneWithSameLocation == null)
                    LowestPathID = Serwer.lokalizacjaBL.PobierzNumerLokalizacjiNaŚcieżce(
                        CurrentItem.ExIDLokalizacjaW
                    );

                Res = await DoSaveCurrentItem();
            });

            if (Res)
            {
                SetFirstScanAction();
                Task.Run(() => Next());
            }
        }

        private async void Pause_Click(object sender, EventArgs e)
        {
            bool choice = await BusinessLogicHelpers.Documents.AskYesOrNo(
                this,
                GetString(Resource.String.editingdocuments_rwzm_pause_multipick)
            );

            RunIsBusyAction(() =>
            {
                if (CallingActivity == null && choice)
                {
                    Helpers.SwitchAndFinishCurrentActivity(this, typeof(ModulesActivity));
                }
            });
        }

        private async Task<bool> End()
        {
            if (IsSwitchingActivity)
                return false;

            await Task.Delay(Globalne.TaskDelay);

            Helpers.ShowProgressDialog(
                GetString(Resource.String.multipicking_activity_closing) + To.Text
            );

            await Task.Delay(Globalne.CurrentSettings.MultipickingDelayBeforeClose);

            if (
                await BusinessLogicHelpers.Documents.ShowAndApplyDocumentExitOptions(
                    this,
                    Documents,
                    DocType,
                    DocumentLeaveAction.Close,
                    false,
                    true
                )
            )
            {
                Helpers.HideProgressDialog();
                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(DocumentsActivity));
                i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocType);
                i.SetFlags(ActivityFlags.NewTask);

                StartActivity(i);
                Finish();
                return true;
            }
            else
                return false;
        }

        private async Task<DocumentItemRow> GetCurrentItemOrEnd()
        {
            int IndexOf = 0;
            CurrentItem = Items[IndexOf];

            if (CurrentItem.Base.numIloscZlecona == CurrentItem.Base.numIloscZrealizowana)
                return null;

            IndexOf = 1;

            while (
                (
                    IDItemsAlreadyDone.Contains(CurrentItem.Base.ID)
                    && LocationsAlreadyBeenTo.Contains(CurrentItem.ExIDLokalizacjaW)
                )
                || CurrentItem.ExIDLokalizacjaW < 0
            )
            {
                if (Items.Count == IndexOf)
                    return null;
                else
                    CurrentItem = Items[IndexOf];

                if (CurrentItem.Base.numIloscZlecona == CurrentItem.Base.numIloscZrealizowana)
                    return null;

                IndexOf++;
            }

            return CurrentItem;
        }

        private async void SetControls()
        {
            if (LastItem != null)
            {
                PreviousLocLay.Visibility = ViewStates.Visible;
                PreviousLoc.Text = DocLocNames[LastItem.Base.idDokumentu];
            }
            else
                PreviousLocLay.Visibility = ViewStates.Gone;

            CurrentItem = await GetCurrentItemOrEnd();

            if (CurrentItem == null)
            {
                await Task.Run(() => End());
                return;
            }

            CurrentDoc = Documents.Find(x => x.ID == CurrentItem.Base.idDokumentu);

            if (CurrentDoc.strOpis != "")
            {
                Description.Visibility = ViewStates.Visible;
                Description.Text = CurrentDoc.strOpis;
            }
            else
                Description.Visibility = ViewStates.Gone;

            decimal InWarehouse = Serwer.przychRozchBL.PobierzStanTowaruWJednostce(
                CurrentItem.Base.idTowaru,
                CurrentDoc.intMagazynW,
                CurrentItem.ExIDLokalizacjaW,
                CurrentItem.Base.idPartia,
                "",
                CurrentItem.Base.idPaletaW,
                "",
                CurrentItem.Base.idFunkcjiLogistycznejW,
                CurrentItem.Base.idJednostkaMiary,
                CurrentItem.Base.idNumerSeryjny
            );

            Amount.Text =
                $"{Math.Round(Math.Min(InWarehouse, CurrentItem.Base.numIloscZlecona - CurrentItem.Base.numIloscZrealizowana), Globalne.CurrentSettings.DecimalSpaces).ToString("F3")} {CurrentItem.Base.strNazwaJednostki}";
            Article.Text =
                $"{CurrentItem.Base.strSymbolTowaru} - {CurrentItem.Base.strNazwaTowaru}";
            To.Text = DocLocNames[CurrentItem.Base.idDokumentu];
            From.Text = CurrentItem.ExLokalizacjaW;
            Partia.Text = CurrentItem.ExPartia;
            Paleta.Text = CurrentItem.ExPaletaW;
            SerialNumber.Text = CurrentItem.Base.strNumerySeryjne;
            kodean.Text = CurrentItem.Base.kodean;

            if (NrKat != null)
                NrKat.Text = CurrentItem.Base.NrKat;

            Lot.Text = CurrentItem.Base.strLoty;
            Flog.Text = CurrentItem.ExFunkcjaLogistycznaW;
            ProdDate.Text = CurrentItem.Base.dtDataProdukcji.ToString(
                Globalne.CurrentSettings.DateFormat
            );
            BestBefore.Text = CurrentItem.Base.dtDataPrzydatności.ToString(
                Globalne.CurrentSettings.DateFormat
            );

            Color TextField = new Color(ContextCompat.GetColor(this, Resource.Color.text_field));
            Color NextLocLast = new Color(
                ContextCompat.GetColor(this, Resource.Color.next_loc_last)
            );

            int CurrIndex = Items.IndexOf(CurrentItem);

            if (Items.Count > CurrIndex + 1)
            {
                Color NextLocSame = new Color(
                    ContextCompat.GetColor(this, Resource.Color.next_loc_same)
                );
                DocumentItemRow NextItem = Items[CurrIndex + 1];

                // Koloryzujemy pola na zielono jeśli następna lokalizacja wydania bądź kuweta są takie same jak obecne, i na żółto jeśli to ostatnia pozycja.
                if (NextItem.Base.numIloscZlecona == NextItem.Base.numIloscZrealizowana)
                    //  From.SetBackgroundColor(NextLocLast)
                    ;
                else if (NextItem.ExIDLokalizacjaW == NextItem.ExIDLokalizacjaW)
                    //  From.SetBackgroundColor(NextLocSame)
                    ;
                else
                    //  From.SetBackgroundColor(TextField)
                    ;

                if (NextItem.Base.numIloscZlecona == NextItem.Base.numIloscZrealizowana)
                    To.SetBackgroundColor(NextLocLast);
                else if (
                    DocLocNames[NextItem.Base.idDokumentu]
                    == DocLocNames[CurrentItem.Base.idDokumentu]
                )
                    //       To.SetBackgroundColor(NextLocSame)
                    ;
                else
                    To.SetBackgroundColor(TextField);
            }
            else
            {
                //  From.SetBackgroundColor(NextLocLast);
                To.SetBackgroundColor(NextLocLast);
            }

            // Wyłączamy pola które były uzupełnione w desktopie
            if (true)
            {
                if (CurrentItem.Base.idPartia >= 0)
                {
                    EditPartia.Visibility = ViewStates.Gone;
                    Partia.Enabled = false;
                }
                else
                {
                    EditPartia.Visibility = ViewStates.Visible;
                    Partia.Enabled = true;
                }

                if (CurrentItem.Base.idPaletaW >= 0)
                {
                    EditPaleta.Visibility = ViewStates.Gone;
                    Paleta.Enabled = false;
                }
                else
                {
                    EditPaleta.Visibility = ViewStates.Visible;
                    Paleta.Enabled = true;
                }

                if (CurrentItem.Base.strNumerySeryjne != "")
                    SerialNumber.Enabled = false;
                else
                    SerialNumber.Enabled = true;

                if (CurrentItem.Base.strLoty != "")
                    Lot.Enabled = false;
                else
                    Lot.Enabled = true;

                if (CurrentItem.Base.kodean != "")
                    kodean.Enabled = false;
                else
                    kodean.Enabled = true;

                if (NrKat != null && CurrentItem.Base.NrKat != "")
                    NrKat.Enabled = false;
                else
                    kodean.Enabled = true;

                if (CurrentItem.Base.idFunkcjiLogistycznejW >= 0)
                    EditFlog.Visibility = ViewStates.Gone;
                else
                    EditFlog.Visibility = ViewStates.Visible;
            }

            decimal New = Math.Min(
                InWarehouse,
                CurrentItem.Base.numIloscZlecona - CurrentItem.Base.numIloscZrealizowana
            );
            NumAmount.Max = New;
            NumAmount.Value = New;
        }

        //       async void Back_Click(object sender, EventArgs e)
        //       {
        //           await RunIsBusyTaskAsync(() => Do_Exit());
        //       }
        //
        async Task Do_Exit()
        {
            try
            {
                if (IsSwitchingActivity)
                    return;

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

                foreach (DokumentVO Doc in Documents)
                    Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);

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

        async Task Next(bool First = false)
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.editing_documents_loading));
                LastItem = Items.Count == 0 ? null : (CurrentItem ?? Items.First());

                if (First)
                {
                    foreach (DokumentVO Doc in Documents)
                        DocLocNames[Doc.ID] = (string)
                            Helpers.HiveInvoke(
                                typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL),
                                "PobierzNazwęLokalizacji",
                                Doc.intLokalizacja
                            );

                    Items = await Task.Factory.StartNew(
                        () =>
                            EditingDocumentsActivity_Common.GetData(
                                Documents,
                                DocType,
                                ZLMMMode.None,
                                Operation.Out,
                                -1,
                                DefaultLocType.None,
                                -1,
                                "Ścieżka",
                                new List<int>(),
                                LowestPathID
                            )
                    );
                    if (
                        Items.Find(x =>
                            (x.Base.numIloscZlecona > x.Base.numIloscZrealizowana)
                            && x.ExIDLokalizacjaW >= 0
                        ) == null
                    )
                    {
                        Helpers.HideProgressDialog();
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.multip_docs_done,
                            Resource.Raw.sound_message
                        );
                        await Do_Exit_Without_Asking();
                        return;
                    }
                }
                else if (Items != null)
                {
                    foreach (var item in Items)
                    {
                        item.Status = EditingDocumentsActivity_Common.GetStatusForItem(
                            item.Base,
                            DocType,
                            ZLMMMode.None,
                            Operation.Out
                        );
                    }
                    Items = Items
                        .OrderBy(x => x.Status)
                        .ThenBy(x => x.KolejnośćNaŚcieżce)
                        .ThenBy(x => x.EXIDLokalizacjaDokumentu)
                        .ToList();
                }

                RunOnUiThread(() =>
                {
                    SetControls();
                    MultipickingView.Visibility = ViewStates.Visible;
                    MultipickingView.Enabled = true;
                });

                Helpers.HideProgressDialog();
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

        private void NextScanStepAction()
        {
            Helpers.PlaySound(this, Resource.Raw.sound_ok);

            switch (ScanningStep)
            {
                case ScanningSteps.OUT:
                {
                    if (Globalne.CurrentSettings.MultipickingConfirmArticle)
                    {
                        ScanningStep = ScanningSteps.ART;
                        SetScanHintBasedOnCurrentScanStep();
                    }
                    else if (
                        !Globalne.CurrentSettings.MultipickingConfirmArticle
                        && Globalne.CurrentSettings.MultipickingConfirmInLocation
                    )
                    {
                        ScanningStep = ScanningSteps.IN;
                        SetScanHintBasedOnCurrentScanStep();
                    }
                    else
                    {
                        OK_Click(null, null);
                    }

                    break;
                }
                case ScanningSteps.ART:
                {
                    if (Globalne.CurrentSettings.MultipickingConfirmInLocation)
                    {
                        ScanningStep = ScanningSteps.IN;
                        SetScanHintBasedOnCurrentScanStep();
                    }
                    else
                    {
                        OK_Click(null, null);
                    }
                    break;
                }
                case ScanningSteps.IN:
                {
                    OK_Click(null, null);
                    break;
                }
            }
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ErrorTimer.Stop();
            await Helpers.AlertAsyncWithConfirm(this, TimerErrorMessage, Resource.Raw.sound_error);
        }

        protected override async Task<bool> CheckBeforeAssumingScanningPath(List<string> BarcodesL)
        {
            await base.CheckBeforeAssumingScanningPath(BarcodesL);

            switch (ScanningStep)
            {
                case ScanningSteps.OUT:
                {
                    LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], true);

                    if (CurrentItem.ExIDLokalizacjaW != Loc.ID)
                    {
                        TimerErrorMessage = Resource.String.multip_wrong_loc;
                        ErrorTimer.Start();

                        LastScanData = null;
                        return false;
                    }
                    else
                    {
                        LastScanData = null;
                        NextScanStepAction();
                        return false;
                    }
                }
                case ScanningSteps.ART:
                {
                    return true;
                }
                case ScanningSteps.IN:
                {
                    LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], false);

                    if (CurrentDoc.intLokalizacja != Loc.ID)
                    {
                        TimerErrorMessage = Resource.String.multip_wrong_loc;
                        ErrorTimer.Start();

                        LastScanData = null;
                        return false;
                    }
                    else
                    {
                        LastScanData = null;
                        NextScanStepAction();
                        return false;
                    }
                }
                default:
                    return false;
            }
        }

        protected override async void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => DoBarcode());
        }

        private async Task DoBarcode()
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            try
            {
                KodKreskowyZSzablonuO Kod = Helpers.ParseBarcodesAccordingToOrder(
                    LastScanData,
                    DocType
                );

                // Sprawdzamy czy zeskanowano ten sam artykuł
                if (
                    Kod.TowaryJednostkiWBazie.Count == 0
                    || (Kod.TowaryJednostkiWBazie[0].IDTowaru != CurrentItem.Base.idTowaru)
                )
                {
                    Helpers.HideProgressDialog();

                    TimerErrorMessage = Resource.String.multip_wrong_art;
                    ErrorTimer.Start();

                    return;
                }

                // Sprawdzamy czy zeskanowano partię zadaną przez Desktop
                if (Kod.Partia != "" && Globalne.CurrentSettings.Partie)
                {
                    if (CurrentItem.Base.idPartia >= 0)
                    {
                        if (
                            Serwer.partiaBL.PobierzIDPartii(Kod.Partia) != CurrentItem.Base.idPartia
                        )
                        {
                            Helpers.HideProgressDialog();

                            TimerErrorMessage = Resource.String.multip_wrong_partia;
                            ErrorTimer.Start();

                            return;
                        }
                    }
                }

                // Sprawdzamy czy zeskanowano paletę zadaną przez Desktop
                if (Kod.Paleta != "" && Globalne.CurrentSettings.Palety)
                {
                    if (CurrentItem.Base.idPaletaW >= 0)
                    {
                        if (
                            Serwer.paletaBL.PobierzIDPalety(Kod.Paleta) != CurrentItem.Base.idPartia
                        )
                        {
                            Helpers.HideProgressDialog();

                            TimerErrorMessage = Resource.String.multip_wrong_paleta;
                            ErrorTimer.Start();

                            return;
                        }
                    }
                }
                if (Kod.NrSeryjny != "" && Globalne.CurrentSettings.SerialNumber)
                {
                    if (CurrentItem.Base.idNumerSeryjny >= 0)
                    {
                        if (
                            Globalne.numerSeryjnyBL.PobierzIDNumeruSeryjnego(Kod.NrSeryjny)
                            != CurrentItem.Base.idNumerSeryjny
                        )
                        {
                            Helpers.HideProgressDialog();

                            TimerErrorMessage = Resource.String.multip_wrong_serialnum;
                            ErrorTimer.Start();

                            return;
                        }
                    }
                }
                //todo: rzyjrzec sie czy dziala poprawnie
                // Sprawdzamy, czy towar który zeskanowano występuje faktycznie w takiej ilości w lokalizacji
                decimal Stan = Serwer.przychRozchBL.PobierzStanTowaru(
                    CurrentItem.Base.idTowaru,
                    CurrentDoc.intMagazynW,
                    CurrentItem.ExIDLokalizacjaW,
                    -1,
                    Kod.Partia,
                    -1,
                    Kod.Paleta,
                    (int)Flog.Tag,
                    CurrentItem.Base.idNumerSeryjny,
                    true
                );

                if (Stan > NumAmount.Value)
                {
                    Helpers.HideProgressDialog();

                    TimerErrorMessage = Resource.String.multip_art_not_in_loc;
                    ErrorTimer.Start();

                    return;
                }
                else
                {
                    if (Globalne.CurrentSettings.Partie)
                        Helpers.SetTextOnEditText(this, Partia, Kod.Partia);

                    if (Globalne.CurrentSettings.Palety)
                        Helpers.SetTextOnEditText(this, Paleta, Kod.Paleta);

                    Helpers.SetTextOnEditText(this, SerialNumber, Kod.NrSeryjny);
                    Helpers.SetTextOnEditText(this, Lot, Kod.Lot);

                    DateTime EmptyDate = new DateTime(1900, 01, 01);

                    RunOnUiThread(() =>
                    {
                        if (Kod.DataProdukcji != EmptyDate)
                            ProdDate.Text = Kod.DataProdukcji.ToString(
                                Globalne.CurrentSettings.DateFormat
                            );

                        if (Kod.DataPrzydatności != EmptyDate)
                            BestBefore.Text = Kod.DataPrzydatności.ToString(
                                Globalne.CurrentSettings.DateFormat
                            );
                    });

                    Helpers.HideProgressDialog();
                    LastScanData = null;
                    NextScanStepAction();
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }
    }
}
