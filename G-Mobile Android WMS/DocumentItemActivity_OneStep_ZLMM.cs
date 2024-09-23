using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Controls;
using G_Mobile_Android_WMS.Enums;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;
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
    public class DocumentItemActivityOneStep_ZLMM : ActivityWithScanner
    {
        FloatingActionButton EditArticle;
        FloatingActionButton EditFlogIn;
        FloatingActionButton EditFlogOut;
        FloatingActionButton EditOwner;
        FloatingActionButton EditLocationIn;
        FloatingActionButton EditLocationOut;
        FloatingActionButton EditUnit;
        FloatingActionButton EditPartia;
        FloatingActionButton EditPaletaOut;
        FloatingActionButton EditPaletaIn;
        FloatingActionButton EditSerial;

        TextView Article;
        EditText Partia;
        EditText PaletaIn;
        EditText PaletaOut;
        EditText SerialNumber;
        EditText Lot;
        EditText ProdDate;
        EditText BestBefore;
        TextView FlogIn;
        TextView FlogOut;
        TextView Owner;
        TextView LocationIn;
        TextView LocationOut;
        TextView OnDoc;
        TextView Ordered;
        TextView InWarehouse;
        TextView Unit;
        TextView CanBeAddedToLoc;
        TextView KodEAN;
        TextView NrKat;
        TextView Symbol;

        NumericUpDown NumAmount;

        DokumentVO Dokument;
        ExtendedModel.DocumentItemVO Item;

        ItemActivityMode Mode = ItemActivityMode.Create;
        Operation Operation = Operation.In;
        bool BufferSet = false;
        bool FromScanner = false;
        bool IsBlockedPosition
        {
            get
            {
                if (Item != null && Item.Base != null)
                    return (
                        Item.Base.dataModyfikacji != Item.Base.dataUtworzenia
                        && Item.Base.numIloscZebrana == Item.Base.numIloscZlecona
                    );
                return false;
            }
        }

        DefaultLocType BufferType = DefaultLocType.None;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_documentitem_onestep_zlmm);

            DocType = (DocTypes)
                Intent.GetIntExtra(DocumentItemActivity_Common.Vars.DocType, (int)DocTypes.Error);
            Dokument = (DokumentVO)
                Helpers.DeserializePassedJSON(
                    Intent,
                    DocumentItemActivity_Common.Vars.DocJSON,
                    typeof(DokumentVO)
                );
            Item = (ExtendedModel.DocumentItemVO)
                Helpers.DeserializePassedJSON(
                    Intent,
                    DocumentItemActivity_Common.Vars.ItemJSON,
                    typeof(ExtendedModel.DocumentItemVO)
                );
            Mode = (ItemActivityMode)
                Intent.GetIntExtra(
                    DocumentItemActivity_Common.Vars.Mode,
                    (int)ItemActivityMode.Create
                );
            BufferType = (DefaultLocType)
                Intent.GetIntExtra(
                    DocumentItemActivity_Common.Vars.BufferType,
                    (int)DefaultLocType.None
                );
            Operation = (Enums.Operation)
                Intent.GetIntExtra(DocumentItemActivity_Common.Vars.Operation, (int)Operation.In);
            BufferSet = Intent.GetBooleanExtra(DocumentItemActivity_Common.Vars.BufferSet, false);

            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[DocType];
            FromScanner = Intent.GetBooleanExtra(
                DocumentItemActivity_Common.Vars.FromScanner,
                false
            );

            GetAndSetControls();
            IsBusy = false;

            NumAmount.FocusField();

            if (
                Globalne.CurrentSettings.InstantScanning[DocType]
                && (FromScanner || Mode == ItemActivityMode.EditAdd)
            )
                OK_Click(this, null);

            if (IsBlockedPosition)
            {
                DisableControls();
            }
        }

        private void DisableControls()
        {
            Helpers.CenteredToast("Pozycja tylko do podglądu", ToastLength.Long);
            DisableButton(EditArticle);
            DisableButton(EditFlogIn);
            DisableButton(EditFlogOut);
            DisableButton(EditOwner);
            DisableButton(EditLocationIn);
            DisableButton(EditLocationOut);
            DisableButton(EditUnit);
            DisableButton(EditPartia);
            DisableButton(EditPaletaOut);
            DisableButton(EditPaletaIn);
            DisableButton(FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok));
            NumAmount.SetDisableControl();
        }

        private void SetupFields()
        {
            Symbol.Text = Item.ExSymbol;

            Article.Text = Item.ExArticle;
            Article.Tag = Item.ExIDArticle;
            EditArticle.Click += EditArticle_Click;

            Partia.Text = Item.ExPartia;
            Partia.Tag = Item.ExIDPartia;
            Partia.FocusChange += Partia_FocusChange;
            Partia.Click += OnClickTargettableTextView;
            Partia.FocusChange += OnFocusChangeTargettableTextView;
            EditPartia.Click += EditPartia_Click;

            PaletaIn.Text = Item.ExPaletaP;
            PaletaIn.Tag = Item.ExIDPaletaP;
            PaletaIn.FocusChange += Paleta_FocusChange;
            PaletaIn.Click += OnClickTargettableTextView;
            PaletaIn.FocusChange += OnFocusChangeTargettableTextView;
            EditPaletaIn.Click += EditPaletaIn_Click;

            PaletaOut.Text = Item.ExPaletaW;
            PaletaOut.Tag = Item.ExIDPaletaW;
            PaletaOut.FocusChange += Paleta_FocusChange;
            PaletaOut.TextChanged += PaletaOut_TextChanged;
            PaletaOut.Click += OnClickTargettableTextView;
            PaletaOut.FocusChange += OnFocusChangeTargettableTextView;
            EditPaletaOut.Click += EditPaletaOut_Click;

            EditSerial.Click += EditSerial_Click;

            SerialNumber.Text = Item.ExSerialNum;
            SerialNumber.Click += OnClickTargettableTextView;
            SerialNumber.FocusChange += OnFocusChangeTargettableTextView;

            KodEAN.Text = Item.ExKodEAN;
            //  KodEAN.Click += OnClickTargettableTextView;
            KodEAN.FocusChange += OnFocusChangeTargettableTextView;

            NrKat.Text = Item.ExNrKat;
            NrKat.FocusChange += OnFocusChangeTargettableTextView;

            Lot.Text = Item.ExLot;
            Lot.Click += OnClickTargettableTextView;
            Lot.FocusChange += OnFocusChangeTargettableTextView;

            FlogOut.Text = Item.ExFunkcjaLogistycznaW;
            FlogOut.Tag = Item.ExIDFunkcjaLogistycznaW;
            EditFlogOut.Click += EditFlogOut_Click;

            FlogIn.Text = Item.ExFunkcjaLogistycznaP;
            FlogIn.Tag = Item.ExIDFunkcjaLogistycznaP;
            EditFlogIn.Click += EditFlogIn_Click;

            LocationIn.Text = Item.ExLokalizacjaP;
            LocationIn.Tag = Item.ExIDLokalizacjaP;
            EditLocationIn.Click += EditLocationIn_Click;

            LocationOut.Text = Item.ExLokalizacjaW;
            LocationOut.Tag = Item.ExIDLokalizacjaW;
            EditLocationOut.Click += EditLocationOut_Click;

            Unit.Text = Item.ExUnit;
            Unit.Tag = Item.ExIDUnit;
            EditUnit.Click += EditUnit_Click;

            Owner.Text = Item.ExOwner;
            Owner.Tag = Item.ExIDOwner;
            EditOwner.Click += EditOwner_Click;

            Ordered.Text = Item.Base.numIloscZlecona.ToString("F3");

            ProdDate.Text = Item.ExProductionDate.ToString(Globalne.CurrentSettings.DateFormat);
            ProdDate.FocusChange += DateFields_FocusChange;

            BestBefore.Text = Item.ExBestBefore.ToString(Globalne.CurrentSettings.DateFormat);
            BestBefore.FocusChange += DateFields_FocusChange;

            OnDoc.Text = Math.Round(
                    Item.Base.numIloscZrealizowana,
                    Globalne.CurrentSettings.DecimalSpaces
                )
                .ToString("F3");

            NumAmount.Value = Item.DefaultAmount;

            if (Dokument.bZlecenie)
            {
                EditArticle.Visibility = ViewStates.Gone;
                EditUnit.Visibility = ViewStates.Gone;

                if (Item.Base.idPartia >= 0)
                {
                    EditPartia.Visibility = ViewStates.Gone;
                    Partia.Enabled = false;
                }

                if (Item.Base.idPaletaP >= 0)
                {
                    EditPaletaIn.Visibility = ViewStates.Gone;
                    PaletaIn.Enabled = false;
                }

                if (Item.Base.idPaletaW >= 0)
                {
                    EditPaletaOut.Visibility = ViewStates.Gone;
                    PaletaOut.Enabled = false;
                }

                if (Item.Base.strNumerySeryjne != "")
                    SerialNumber.Enabled = false;

                if (Item.Base.kodean != "")
                    KodEAN.Enabled = false;

                if (Item.Base.NrKat != "")
                    NrKat.Enabled = false;

                if (Item.Base.strLoty != "")
                    Lot.Enabled = false;

                if (Item.Base.idLokalizacjaP >= 0 || (!Globalne.CurrentSettings.Lokalizacje))
                    EditLocationIn.Visibility = ViewStates.Gone;

                if (
                    !Globalne.CurrentSettings.AllowSourceLocationChange
                        && Item.Base.idLokalizacjaW >= 0
                    || (!Globalne.CurrentSettings.Lokalizacje)
                )
                    EditLocationOut.Visibility = ViewStates.Gone;

                if (Item.Base.idFunkcjiLogistycznejP >= 0)
                    EditFlogIn.Visibility = ViewStates.Gone;

                if (Item.Base.idFunkcjiLogistycznejW >= 0)
                    EditFlogOut.Visibility = ViewStates.Gone;

                if (Operation == Operation.Out)
                    EditOwner.Visibility = ViewStates.Gone;
            }

            if (Mode == ItemActivityMode.EditAdd)
            {
                Partia.Enabled = false;
                PaletaIn.Enabled = false;
                PaletaOut.Enabled = false;
                EditPaletaIn.Visibility = ViewStates.Gone;
                EditPaletaOut.Visibility = ViewStates.Gone;
                EditPartia.Visibility = ViewStates.Gone;
                SerialNumber.Enabled = false;
                //KodEAN.Enabled = false;
                //todo usunac jak nie zadziala
                Lot.Enabled = false;
                EditLocationIn.Visibility = ViewStates.Gone;
                EditLocationOut.Visibility = ViewStates.Gone;
                EditFlogIn.Visibility = ViewStates.Gone;
                EditFlogOut.Visibility = ViewStates.Gone;
                EditUnit.Visibility = ViewStates.Gone;
                EditArticle.Visibility = ViewStates.Gone;
            }
            if (Globalne.CurrentSettings.DisableHandLocationsChange)
            {
                DisableButton(EditLocationIn);
                DisableButton(EditLocationOut);
            }
            if (SerialNumber.Text != "")
            {
                this.DisableButton(this.EditSerial);
                this.SerialNumber.Enabled = false;
            }

            this.SerialNumber.Enabled = false;
        }

        private void DisableButton(FloatingActionButton button)
        {
            button.SetColorFilter(Color.Gray);
            button.Background.Alpha = 50;
            button.Enabled = false;
        }

        private void EditPartia_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(PartieActivity));
                i.PutExtra(
                    PartieActivity.Vars.IDMagazynu,
                    Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW
                );

                if (Operation == Operation.Out)
                {
                    i.PutExtra(PartieActivity.Vars.IDDokumentu, Dokument.ID);
                    i.PutExtra(PartieActivity.Vars.IDTowaru, (int)Article.Tag);
                    i.PutExtra(PartieActivity.Vars.IDLokalizacji, (int)LocationOut.Tag);
                    i.PutExtra(PartieActivity.Vars.IDPalety, (int)PaletaOut.Tag);
                    i.PutExtra(PartieActivity.Vars.IDFunkcjiLogistycznej, (int)FlogOut.Tag);
                    i.PutExtra(PartieActivity.Vars.Rozchód, true);
                    i.PutExtra(PartieActivity.Vars.AskOnStart, false);
                }
                else
                    i.PutExtra(LocationsActivity.Vars.AskOnStart, true);

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
                i.PutExtra(
                    PaletyActivity.Vars.IDMagazynu,
                    Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW
                );

                if (Operation == Operation.Out)
                {
                    i.PutExtra(PaletyActivity.Vars.IDDokumentu, Dokument.ID);
                    i.PutExtra(PaletyActivity.Vars.IDTowaru, (int)Article.Tag);
                    i.PutExtra(PaletyActivity.Vars.IDLokalizacji, (int)this.LocationIn.Tag);

                    i.PutExtra(PaletyActivity.Vars.Rozchód, true);
                    i.PutExtra(PaletyActivity.Vars.AskOnStart, false);
                }
                else
                    i.PutExtra(LocationsActivity.Vars.AskOnStart, true);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.PaletaActivityResult
                );
            });
        }

        private void EditPaletaOut_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(PaletyActivity));
                i.PutExtra(PaletyActivity.Vars.IDMagazynu, Dokument.intMagazynW);
                i.PutExtra(PaletyActivity.Vars.IDDokumentu, Dokument.ID);
                i.PutExtra(PaletyActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(PaletyActivity.Vars.IDLokalizacji, (int)LocationOut.Tag);
                i.PutExtra(PaletyActivity.Vars.IDPartii, (int)Partia.Tag);
                i.PutExtra(PaletyActivity.Vars.IDFunkcjiLogistycznej, (int)FlogOut.Tag);
                i.PutExtra(PaletyActivity.Vars.Rozchód, true);
                i.PutExtra(PaletyActivity.Vars.AskOnStart, false);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.PaletaOutActivityResult
                );
            });
        }

        private void EditPaletaIn_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(PaletyActivity));
                i.PutExtra(PaletyActivity.Vars.IDMagazynu, Dokument.intMagazynP);
                i.PutExtra(PaletyActivity.Vars.Rozchód, false);
                i.PutExtra(PaletyActivity.Vars.AskOnStart, false);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.PaletaInActivityResult
                );
            });
        }

        private void EditSerial_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(SerialActivity));

                i.PutExtra(PaletyActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(PaletyActivity.Vars.IDLokalizacji, (int)LocationOut.Tag);
                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.SerialActivityResult
                );
            });
        }

        private void PaletaOut_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            Helpers.SetTextOnEditText(this, PaletaIn, PaletaOut.Text);
        }

        private void Partia_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!Partia.IsFocused)
            {
                Refresh();
            }
        }

        private void Paleta_FocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (!(sender as EditText).IsFocused)
            {
                Refresh();
            }
        }

        private void GetAndSetControls()
        {
            DocumentItemActivity_Common.SetHeaderBasedOnMode(this, Mode);

            TextView Scanhint = FindViewById<TextView>(Resource.Id.scanhint);

            if (BufferSet)
                Scanhint.Text = GetString(
                    Resource.String.documentitem_activity_scanhint_bufferset_zlmm_onestep
                );
            else
                Scanhint.Text = GetString(
                    Resource.String.documentitem_activity_scanhint_zlmm_onestep
                );

            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_cancel).Click +=
                Cancel_Click;
            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok).Click += OK_Click;

            EditArticle = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_article);
            EditFlogOut = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_flog_out
            );
            EditFlogIn = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_flog_in);
            EditOwner = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_owner);
            EditLocationOut = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_location_out
            );
            EditLocationIn = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_location_in
            );
            EditUnit = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_unit);
            EditPartia = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_partia);
            EditPaletaIn = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_paleta_in
            );
            EditPaletaOut = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_paleta_out
            );
            EditSerial = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_serial);

            Article = FindViewById<TextView>(Resource.Id.document_item_article);
            Partia = FindViewById<EditText>(Resource.Id.document_item_partia);
            PaletaOut = FindViewById<EditText>(Resource.Id.document_item_paleta_out);
            PaletaIn = FindViewById<EditText>(Resource.Id.document_item_paleta_in);
            SerialNumber = FindViewById<EditText>(Resource.Id.document_item_serial);
            Lot = FindViewById<EditText>(Resource.Id.document_item_lot);
            FlogOut = FindViewById<TextView>(Resource.Id.document_item_flog_out);
            FlogIn = FindViewById<TextView>(Resource.Id.document_item_flog_in);
            Owner = FindViewById<TextView>(Resource.Id.document_item_owner);
            LocationOut = FindViewById<TextView>(Resource.Id.document_item_location_out);
            LocationIn = FindViewById<TextView>(Resource.Id.document_item_location_in);
            OnDoc = FindViewById<TextView>(Resource.Id.document_item_ondoc);
            Ordered = FindViewById<TextView>(Resource.Id.document_item_ordered);
            InWarehouse = FindViewById<TextView>(Resource.Id.document_item_amountinwarehouse);
            ProdDate = FindViewById<EditText>(Resource.Id.document_item_proddate);
            BestBefore = FindViewById<EditText>(Resource.Id.document_item_bestbefore);
            Unit = FindViewById<TextView>(Resource.Id.document_item_unit);
            CanBeAddedToLoc = FindViewById<TextView>(
                Resource.Id.document_item_amountcanbeaddedtoloc
            );
            KodEAN = FindViewById<TextView>(Resource.Id.document_item_kodean);
            NrKat = FindViewById<TextView>(Resource.Id.document_item_NrKat);
            Symbol = FindViewById<TextView>(Resource.Id.document_item_symbol);

            NumAmount = FindViewById<NumericUpDown>(Resource.Id.document_item_amount);
            // Kompatybilność z 5.0
            NumAmount.Initialize();

            DocumentItemActivity_Common.SetVisibilityOfFields(
                this,
                DocType,
                Dokument.bZlecenie,
                Mode
            );
            SetupFields();

            switch (DocType)
            {
                case DocTypes.PW:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationPW)
                        DisableButton(
                            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok)
                        );
                    break;
                case DocTypes.RW:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationRW)
                        DisableButton(
                            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok)
                        );
                    break;
                case DocTypes.ZL:
                case DocTypes.ZLDistribution:
                case DocTypes.ZLGathering:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationZL)
                        DisableButton(
                            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok)
                        );
                    break;
                default:
                    break;
            }

            Do_Refresh();
        }

        private PozycjaVO GetDBObject()
        {
            PozycjaVO Poz = Item.Base;

            Poz.idDokumentu = Dokument.ID;

            Poz.idTowaru = (int)Article.Tag;

            if (Globalne.CurrentSettings.Partie)
            {
                Poz.strPartia = Partia.Text;
                Poz.idPartia = Serwer.partiaBL.PobierzIDPartii(Partia.Text);
            }
            else
            {
                Poz.idPartia = -1;
                Poz.strPartia = "";
            }

            if (Globalne.CurrentSettings.Palety)
            {
                Poz.strPaletaP = PaletaIn.Text;
                Poz.idPaletaP = Serwer.paletaBL.PobierzIDPalety(PaletaIn.Text);
                Poz.strPaletaW = PaletaOut.Text;
                Poz.idPaletaW = Serwer.paletaBL.PobierzIDPalety(PaletaOut.Text);
            }
            else
            {
                Poz.strPaletaP = "";
                Poz.strPaletaW = "";
                Poz.idPaletaP = -1;
                Poz.idPaletaW = -1;
            }

            if (Globalne.CurrentSettings.FunkcjeLogistyczne)
            {
                Poz.idFunkcjiLogistycznejP = (int)FlogIn.Tag;
                Poz.idFunkcjiLogistycznejW = (int)FlogOut.Tag;
            }
            else
            {
                Poz.idFunkcjiLogistycznejP = -1;
                Poz.idFunkcjiLogistycznejW = -1;
            }

            // Failsafe
            if ((int)LocationIn.Tag > 0 && LocationIn.Text != "")
            {
                LocationIn.Tag = Globalne
                    .lokalizacjaBL.PobierzLokalizacjęWgNazwy(LocationIn.Text, Dokument.intMagazynP)
                    .ID;
            }

            if ((int)LocationOut.Tag < 0 && LocationOut.Text != "")
            {
                LocationOut.Tag = Globalne
                    .lokalizacjaBL.PobierzLokalizacjęWgNazwy(LocationOut.Text, Dokument.intMagazynP)
                    .ID;
            }

            Poz.idLokalizacjaP = (int)LocationIn.Tag;
            Poz.idLokalizacjaW = (int)LocationOut.Tag;

            Poz.strLoty = Lot.Text;
            Poz.strNumerySeryjne = SerialNumber.Text;
            Poz.kodean = KodEAN.Text;
            Poz.NrKat = NrKat.Text;
            Poz.idKontrahent = (int)Owner.Tag;

            Poz.idJednostkaMiary = (int)Unit.Tag;

            try
            {
                Poz.dtDataProdukcji = DateTime.ParseExact(
                    ProdDate.Text,
                    Globalne.CurrentSettings.DateFormat,
                    System.Globalization.CultureInfo.InvariantCulture
                );
                Poz.dtDataPrzydatności = DateTime.ParseExact(
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

            if (Poz.intUtworzonyPrzez < 0)
                Poz.intUtworzonyPrzez = Globalne.Operator.ID;

            Poz.intZmodyfikowanyPrzez = Globalne.Operator.ID;

            return Poz;
        }

        private async Task<bool> CheckSanity()
        {
            if (NumAmount.Value <= 0)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.documentitem_cant_be_zero
                );
                return false;
            }

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
                    case DocumentItemFields.Symbol:
                        OK = (!Dict[Key] || Symbol.Text.Length >= 0);
                        break;
                    case DocumentItemFields.Article:
                        OK = (!Dict[Key] || (int)Article.Tag >= 0);
                        break;
                    case DocumentItemFields.FlogIn:
                        OK = (
                            !Dict[Key]
                            || !Globalne.CurrentSettings.FunkcjeLogistyczne
                            || (int)FlogIn.Tag >= 0
                        );
                        break;
                    case DocumentItemFields.FlogOut:
                        OK = (
                            !Dict[Key]
                            || !Globalne.CurrentSettings.FunkcjeLogistyczne
                            || (int)FlogOut.Tag >= 0
                        );
                        break;
                    case DocumentItemFields.LocationIn:
                        OK = (!Dict[Key] || (int)LocationIn.Tag >= 0);
                        break;
                    case DocumentItemFields.LocationOut:
                        OK = (!Dict[Key] || (int)LocationOut.Tag >= 0);
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
                    case DocumentItemFields.PaletaIn:
                        OK = (
                            !Dict[Key] || !Globalne.CurrentSettings.Palety || PaletaIn.Text != ""
                        );
                        break;
                    case DocumentItemFields.PaletaOut:
                        OK = (
                            !Dict[Key] || !Globalne.CurrentSettings.Palety || PaletaOut.Text != ""
                        );
                        break;
                    case DocumentItemFields.Partia:
                        OK = (!Dict[Key] || !Globalne.CurrentSettings.Partie || Partia.Text != "");
                        break;
                    case DocumentItemFields.Owner:
                        OK = (!Dict[Key] || (int)Owner.Tag >= 0);
                        break;
                    case DocumentItemFields.Lot:
                        OK = (!Dict[Key] || Lot.Text != "");
                        break;
                    case DocumentItemFields.SerialNumber:
                        OK = (!Dict[Key] || SerialNumber.Text != "");
                        break;
                    case DocumentItemFields.KodEAN:
                        OK = (!Dict[Key] || KodEAN.Text != "");
                        break;
                    case DocumentItemFields.NrKat:
                        OK = (!Dict[Key] || NrKat.Text != "");
                        break;
                    case DocumentItemFields.Unit:
                        OK = (!Dict[Key] || (int)Unit.Tag >= 0);
                        break;
                }

                View v = FindViewById<View>(DocumentItemActivity_Common.RequiredElementsDict[Key]);

                RunOnUiThread(() =>
                {
                    if (OK != true)
                    {
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

        private async void OK_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(async () =>
            {
                try
                {
                    PozycjaVO Poz = GetDBObject();

                    if (Poz == null)
                        return;

                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: Sanity Checking - START");
                    }

                    var sanityResult = await CheckSanity();

                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: Sanity Checking - END");
                    }

                    if (!sanityResult)
                    {
                        if (Globalne.debug_mode == 1)
                        {
                            Console.WriteLine("DEBUG_INFO: Sanity is false - RETURNING");
                        }

                        return;
                    }
                    if ((int)SerialNumber.Tag > 0)
                        Poz.idNumerSeryjny = (int)SerialNumber.Tag;
                    switch (Mode)
                    {
                        case ItemActivityMode.Create:
                        {
                            if (Operation == Operation.OutIn)
                            {
                                Poz.dataModyfikacji = DateTime.Now;
                                Poz.bezposrednieZL = true;
                            }

                            int IDPozIst =
                                Serwer.dokumentBL.PobierzIDPozycjiIdentycznejNaDokumencie(Poz);

                            int Res = -1;

                            if (IDPozIst != -1)
                            {
                                PozycjaVO PozIst = Serwer.dokumentBL.PobierzPozycję(IDPozIst);
                                PozIst.intZmodyfikowanyPrzez = Globalne.Operator.ID;
                                PozIst.numIloscZrealizowana += NumAmount.Value;
                                PozIst.numIloscZlecona += NumAmount.Value;
                                PozIst.numIloscZebrana += NumAmount.Value;

                                Res = Serwer.dokumentBL.EdytujPozycję(
                                    Helpers.StringDocType(DocType),
                                    PozIst,
                                    Dokument.bIgnorujBlokadePartii
                                );
                            }

                            if (IDPozIst < 0 || Res < 0)
                            {
                                var przychod = Serwer.przychRozchBL.PobierzPrzychód(
                                    Serwer.lokalizacjaBL.PobierIdLokalizacjaWgNazwy(
                                        LocationIn.Text,
                                        Globalne.Magazyn.ID
                                    ),
                                    Poz.idTowaru
                                );
                                if (przychod.ID > 0)
                                {
                                    //Poz.dataUtworzenia = przychod.dataPrzychodu;
                                    Poz.dtDataProdukcji = przychod.dataProdukcji;
                                    Poz.dtDataPrzydatności = przychod.dataPrzydatnosci;
                                }
                                Poz.numIloscZlecona = NumAmount.Value;
                                Poz.numIloscZrealizowana = NumAmount.Value;
                                Poz.numIloscZebrana = NumAmount.Value;
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Serwer.dokumentBL.ZróbPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
                            }

                            break;
                        }
                        case ItemActivityMode.Edit:
                        {
                            if (IsBlockedPosition && NumAmount.Value != Poz.numIloscZlecona)
                            {
                                await Helpers.Alert(this, "Pozycja nie może być już edytowana.");
                                break;
                            }

                            if (!Dokument.bZlecenie)
                                Poz.numIloscZlecona = NumAmount.Value;

                            Poz.numIloscZrealizowana = NumAmount.Value;
                            Poz.numIloscZebrana = NumAmount.Value;

                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZrealizowana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );

                            AutoException.ThrowIfNotNull(
                                this,
                                ErrorType.ItemCreationError,
                                Serwer.dokumentBL.EdytujPozycję(
                                    Helpers.StringDocType(DocType),
                                    Poz,
                                    Dokument.bIgnorujBlokadePartii
                                )
                            );
                            break;
                        }
                        case ItemActivityMode.EditAdd:
                        {
                            if (!Dokument.bZlecenie)
                                Poz.numIloscZlecona += NumAmount.Value;

                            Poz.numIloscZrealizowana += NumAmount.Value;
                            Poz.numIloscZebrana += NumAmount.Value;

                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZrealizowana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );

                            AutoException.ThrowIfNotNull(
                                this,
                                ErrorType.ItemCreationError,
                                Serwer.dokumentBL.EdytujPozycję(
                                    Helpers.StringDocType(DocType),
                                    Poz,
                                    Dokument.bIgnorujBlokadePartii
                                )
                            );
                            break;
                        }
                        case ItemActivityMode.Split:
                        {
                            PozycjaVO Original = Serwer.dokumentBL.PobierzPozycję(Poz.ID);

                            Poz.dataModyfikacji = DateTime.Now;

                            Poz.numIloscZrealizowana = NumAmount.Value;
                            Poz.numIloscZebrana = NumAmount.Value;
                            if (Dokument.strNazwa.StartsWith("ZL"))
                            {
                                if (ZLMMMode.TwoStep == Globalne.CurrentSettings.DefaultZLMode)
                                {
                                    Poz.bezposrednieZL = false;
                                }
                                else
                                {
                                    Poz.bezposrednieZL = true;
                                }
                            }
                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZrealizowana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );

                            if (Poz.numIloscZrealizowana >= Poz.numIloscZlecona)
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Serwer.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
                            }
                            else if (Poz.numIloscZrealizowana < Original.numIloscZrealizowana)
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Serwer.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
                                Original = (PozycjaVO)Helpers.ObjectCopy(Poz, typeof(PozycjaVO));
                                ;
                            }
                            else
                            {
                                Poz.numIloscZlecona = NumAmount.Value;

                                BusinessLogicHelpers.DocumentItems.EditSplitItem(
                                    this,
                                    Original,
                                    true,
                                    DocType,
                                    Operation,
                                    Poz.numIloscZrealizowana
                                );

                                try
                                {
                                    AutoException.ThrowIfNotNull(
                                        this,
                                        ErrorType.ItemCreationError,
                                        Serwer.dokumentBL.ZróbPozycję(
                                            Helpers.StringDocType(DocType),
                                            Poz,
                                            Dokument.bIgnorujBlokadePartii
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
                                        Operation
                                    );
                                    return;
                                }
                            }

                            break;
                        }
                    }

                    Intent i = new Intent();

                    if (LastScanData != null)
                    {
                        if (Globalne.debug_mode == 1)
                        {
                            Console.WriteLine(
                                "I commented PutExtra command to test regression test"
                            );
                        }

                        i.PutExtra(
                            DocumentItemActivity_Common.Results.WereScanned,
                            LastScanData.ToArray()
                        );
                    }

                    //if (sender == null)
                    //Helpers.PlaySound(this, Resource.Raw.sound_ok);

                    SetResult(Result.Ok, i);
                    this.Finish();
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            });
        }

        private void EditOwner_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(ContractorsActivity));
                i.PutExtra(ContractorsActivity.Vars.AskOnStart, true);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.ContractorsActivityResult
                );
            });
        }

        private void EditLocationOut_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(LocationsActivity));
                i.PutExtra(LocationsActivity.Vars.AskOnStart, false);
                i.PutExtra(LocationsActivity.Vars.IDMagazynu, Dokument.intMagazynW);
                i.PutExtra(LocationsActivity.Vars.IDDokumentu, Dokument.ID);
                i.PutExtra(LocationsActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(LocationsActivity.Vars.IDPartii, (int)Partia.Tag);
                i.PutExtra(LocationsActivity.Vars.IDPalety, (int)PaletaOut.Tag);
                i.PutExtra(LocationsActivity.Vars.IDFunkcjiLogistycznej, (int)FlogOut.Tag);
                i.PutExtra(LocationsActivity.Vars.Rozchód, true);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.LocationsActivityResultOut
                );
            });
        }

        private void EditLocationIn_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(LocationsActivity));
                i.PutExtra(LocationsActivity.Vars.AskOnStart, true);
                i.PutExtra(LocationsActivity.Vars.IDMagazynu, Dokument.intMagazynP);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.LocationsActivityResultIn
                );
            });
        }

        async void EditFlogOut_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                        this,
                        Dokument.intMagazynW,
                        FlogOut
                    )
            );
            RunIsBusyAction(() => Do_Refresh());
        }

        async void EditFlogIn_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                        this,
                        Dokument.intMagazynP,
                        FlogIn
                    )
            );
            RunIsBusyAction(() => Do_Refresh());
        }

        async void EditUnit_Click(object sender, EventArgs e)
        {
            if ((int)Article.Tag < 0)
                return;

            await RunIsBusyTaskAsync(
                () => BusinessLogicHelpers.Indexes.ShowUnitListAndSet(this, (int)Article.Tag, Unit)
            );

            Refresh();
        }

        private void EditArticle_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(ArticlesActivity));
                i.PutExtra(ArticlesActivity.Vars.AskOnStart, true);
                i.PutExtra(ArticlesActivity.Vars.IDMagazynu, Dokument.intMagazynW);
                i.PutExtra(ArticlesActivity.Vars.IDDokumentu, Dokument.ID);
                i.PutExtra(ArticlesActivity.Vars.IDLokalizacji, (int)LocationOut.Tag);
                i.PutExtra(ArticlesActivity.Vars.IDPartii, (int)Partia.Tag);
                i.PutExtra(ArticlesActivity.Vars.IDPalety, (int)PaletaOut.Tag);
                i.PutExtra(ArticlesActivity.Vars.IDFunkcjiLogistycznej, (int)FlogOut.Tag);
                i.PutExtra(ArticlesActivity.Vars.Rozchód, true);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.ArticlesActivityResult
                );
            });
        }

        private void Refresh()
        {
            RunIsBusyAction(() => Do_Refresh());
        }

        private void Paleta_Refresh()
        {
            RunIsBusyAction(() => Do_Paleta_Refresh());
        }

        private void Do_Paleta_Refresh()
        {
            if (
                PaletaOut.Text != ""
                && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry
                && Globalne.CurrentSettings.Palety
            )
                if (
                    BusinessLogicHelpers.DocumentItems.GetSSCCData(
                        PaletaOut.Text,
                        ref Item,
                        Operation
                    )
                )
                {
                    if (Item.ExPartia != "")
                    {
                        Partia.Text = Item.ExPartia;
                        Partia.Tag = Item.ExIDPartia;
                    }
                    if (Item.ExArticle != "")
                    {
                        Article.Text = Item.ExArticle;
                        Article.Tag = Item.ExIDArticle;
                    }
                    if (Item.ExUnit != "")
                    {
                        Unit.Text = Item.ExUnit;
                        Unit.Tag = Item.ExIDUnit;
                    }
                    if (Item.ExLot != "")
                        Lot.Text = Item.ExLot;
                    if (Item.ExSerialNum != "")
                        SerialNumber.Text = Item.ExSerialNum;
                    if (Item.ExKodEAN != "")
                        KodEAN.Text = Item.ExKodEAN;

                    if (Item.ExNrKat != "")
                        NrKat.Text = Item.ExNrKat;

                    ProdDate.Text = Item.ExProductionDate.ToString(
                        Globalne.CurrentSettings.DateFormat
                    );
                    BestBefore.Text = Item.ExBestBefore.ToString(
                        Globalne.CurrentSettings.DateFormat
                    );

                    if (Item.ExOwner != "")
                    {
                        Owner.Text = Item.ExOwner;
                        Owner.Tag = Item.ExIDOwner;
                    }
                }
        }

        private void Do_Refresh(bool InWarehouseOnly = false)
        {
            Dictionary<DocumentItemDisplayElements, bool> Set = Globalne
                .CurrentSettings
                .EditingDocumentItemDisplayElementsListsKAT[DocType];

            if (!InWarehouseOnly)
            {
                if (Set[DocumentItemDisplayElements.Partia] && Globalne.CurrentSettings.Partie)
                {
                    int ID = Serwer.partiaBL.PobierzIDPartii(Partia.Text);
                    Partia.Tag = ID;
                }

                if (Set[DocumentItemDisplayElements.Paleta] && Globalne.CurrentSettings.Palety)
                {
                    int ID = Serwer.paletaBL.PobierzIDPalety(PaletaIn.Text);
                    PaletaIn.Tag = ID;

                    ID = Serwer.paletaBL.PobierzIDPalety(PaletaOut.Text);

                    if ((int)PaletaOut.Tag != ID)
                        Paleta_Refresh();

                    PaletaOut.Tag = ID;
                }
                if (
                    Set[DocumentItemDisplayElements.SerialNumber]
                    && Globalne.CurrentSettings.SerialNumber
                )
                {
                    int ID = Globalne.numerSeryjnyBL.PobierzIDNumeruSeryjnego(SerialNumber.Text);
                    //todo: przeyjrze sie co tu siedzieje



                    if ((int)SerialNumber.Tag != ID)
                        Paleta_Refresh();
                    SerialNumber.Tag = ID;
                }
            }

            if (Set[DocumentItemDisplayElements.InWarehouse])
            {
                if ((int)Article.Tag >= 0)
                {
                    decimal Amount = Serwer.przychRozchBL.PobierzStanTowaru(
                        (int)Article.Tag,
                        Dokument.intMagazynW,
                        (int)LocationOut.Tag,
                        (int)Partia.Tag,
                        Partia.Text,
                        (int)PaletaOut.Tag,
                        PaletaOut.Text,
                        (int)FlogOut.Tag,
                        (int)SerialNumber.Tag,
                        true
                    );

                    if ((int)Unit.Tag >= 0)
                    {
                        JednostkaPrzeliczO Jm = Serwer.jednostkaMiaryBL.PobierzJednostkęPrzelicz(
                            (int)Unit.Tag,
                            (int)Article.Tag
                        );
                        Amount /= Jm.numIle;
                    }

                    Helpers.SetTextOnTextView(
                        this,
                        InWarehouse,
                        Math.Round(Amount, Globalne.CurrentSettings.DecimalSpaces).ToString("F3")
                    );
                }
                else
                    Helpers.SetTextOnTextView(this, InWarehouse, "---");
            }

            if (Set[DocumentItemDisplayElements.CanBeAddedToLoc])
            {
                if ((int)LocationIn.Tag >= 0 && (int)Article.Tag >= 0)
                {
                    int Value = Serwer.przychRozchBL.ObliczIleCałkTowaruZmieściSięWLokalizacji(
                        (int)Article.Tag,
                        (int)Unit.Tag,
                        (int)LocationIn.Tag
                    );
                    Helpers.SetTextOnTextView(
                        this,
                        CanBeAddedToLoc,
                        Value == -1 ? "∞" : Value.ToString("F3")
                    );
                }
                else
                    Helpers.SetTextOnTextView(this, CanBeAddedToLoc, "---");
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
                        case (int)DocumentItemActivity_Common.ResultCodes.ArticlesActivityResult:
                        {
                            TowarVO Art = (TowarVO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    ArticlesActivity.Results.SelectedJSON,
                                    typeof(TowarVO)
                                );

                            if (Art != null && Art.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(
                                    this,
                                    Article,
                                    Art.strSymbol + " - " + Art.strNazwa
                                );
                                Article.Tag = Art.ID;

                                JednostkaMiaryO Jedn =
                                    Serwer.towarBL.PobierzJednostkęDomyślnąTowaru(Art.ID);
                                Unit.Tag = Jedn.ID;
                                Unit.Text = Jedn.strNazwa;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.LocationsActivityResultIn:
                        {
                            LokalizacjaVO Lok = (LokalizacjaVO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(LokalizacjaVO)
                                );

                            if (Lok != null && Lok.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, LocationIn, Lok.strNazwa);
                                LocationIn.Tag = Lok.ID;

                                Refresh();
                            }

                            break;
                        }
                        case (int)
                            DocumentItemActivity_Common.ResultCodes.LocationsActivityResultOut:
                        {
                            LokalizacjaVO Lok = (LokalizacjaVO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(LokalizacjaVO)
                                );

                            if (Lok != null && Lok.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, LocationOut, Lok.strNazwa);
                                LocationOut.Tag = Lok.ID;

                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.PartiaActivityResult:
                        {
                            PartiaO Prt = (PartiaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(PartiaO)
                                );

                            if (Prt != null && Prt.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, Partia, Prt.strKod);
                                Partia.Tag = Prt.ID;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.PaletaInActivityResult:
                        {
                            PaletaO Pal = (PaletaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(PaletaO)
                                );

                            if (Pal != null && Pal.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, PaletaIn, Pal.strOznaczenie);
                                PaletaIn.Tag = Pal.ID;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.PaletaOutActivityResult:
                        {
                            PaletaO Pal = (PaletaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(PaletaO)
                                );

                            if (Pal != null && Pal.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, PaletaOut, Pal.strOznaczenie);
                                PaletaOut.Tag = Pal.ID;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.ContractorsActivityResult:
                        {
                            KontrahentVO Contr = (KontrahentVO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    ContractorsActivity.Results.SelectedJSON,
                                    typeof(KontrahentVO)
                                );

                            if (Contr != null && Contr.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, Owner, Contr.strNazwa);
                                Owner.Tag = Contr.ID;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.SerialActivityResult:
                        {
                            NumerSeryjnyO Ser = (NumerSeryjnyO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    SerialActivity.Results.SelectedJSON,
                                    typeof(NumerSeryjnyO)
                                );

                            if (Ser != null && Ser.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, SerialNumber, Ser.strKod);
                                SerialNumber.Tag = Ser.ID;
                                Refresh();
                            }

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

        private void Cancel_Click(object sender, EventArgs e)
        {
            Finish();
        }

        protected override async void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            await RunIsBusyTaskAsync(() => DoBarcode());
        }

        protected override async Task<bool> CheckBeforeAssumingScanningPath(List<string> BarcodesL)
        {
            await base.CheckBeforeAssumingScanningPath(BarcodesL);

            LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], true);
            int Lokalizacja =
                BufferType == DefaultLocType.In
                    ? Item.Base.idLokalizacjaP
                    : Item.Base.idLokalizacjaW;

            if (
                Loc.ID > -1
                && Globalne.CurrentSettings.AllowSourceLocationChange
                && (
                    (!Globalne.CurrentSettings.Lokalizacje)
                    || (Lokalizacja >= 0) && (Lokalizacja != Loc.ID)
                )
            )
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.documentitem_location_already_set,
                    Resource.Raw.sound_error
                );
                LastScanData = null;
                return false;
            }

            if (Loc != null && Loc.ID >= 0)
            {
                RunOnUiThread(() =>
                {
                    if (BufferType == DefaultLocType.In)
                    {
                        LocationOut.Tag = Loc.ID;
                        LocationOut.Text = Loc.strNazwa;
                    }
                    else
                    {
                        LocationIn.Tag = Loc.ID;
                        LocationIn.Text = Loc.strNazwa;
                    }
                });

                LastScanData = null;
                IsBusy = false;
                OK_Click(null, null);
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task DoBarcode()
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            await Task.Run(() =>
            {
                try
                {
                    if (LastScanData != null && !string.IsNullOrEmpty(LastScanData[0]))
                    {
                        var czyLokalizacja =
                            Globalne
                                .lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(
                                    LastScanData[0],
                                    Globalne.Magazyn.ID,
                                    true
                                )
                                ?.ID != -1;
                        var skanowanyTowarNieIstnieje =
                            Serwer.kodykreskoweBL.WyszukajKodKreskowy(LastScanData[0])?.Towar == "";
                        if (skanowanyTowarNieIstnieje && !czyLokalizacja)
                        {
                            AutoException.ThrowIfNotNull(this, Resource.String.articles_not_found);
                        }
                        else if (!czyLokalizacja)
                        {
                            var skanowanyTowar = Globalne
                                .towarBL.PobierzTowarWgKoduKreskowego(LastScanData[0])
                                .FirstOrDefault();
                            // sprawdzamy czy towar jest na liscie pozycji w danym dokumencie
                            if (
                                Dokument.bZlecenie
                                && Globalne
                                    .dokumentBL.PobierzListęIDPozycji(Dokument.ID)
                                    .Where(x => x == skanowanyTowar.ID)
                                    .Count() == 0
                            )
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.articles_cannot_be_used
                                );
                            }
                        }
                    }
                    IsBusy = false;
                    OK_Click(this, null);
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            });

            Helpers.HideProgressDialog();
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Down)
            {
                OK_Click(null, null);
                return true;
            }
            else
                return base.DispatchKeyEvent(e);
        }
    }
}
