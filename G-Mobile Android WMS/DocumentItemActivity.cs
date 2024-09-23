using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Text;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Controls;
using G_Mobile_Android_WMS.Enums;
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
    public class DocumentItemActivity : ActivityWithScanner
    {
        FloatingActionButton EditArticle;
        FloatingActionButton EditFlog;
        FloatingActionButton EditOwner;
        FloatingActionButton EditLocation;
        FloatingActionButton EditUnit;
        FloatingActionButton EditPaleta;
        FloatingActionButton Edit;
        FloatingActionButton EditPartia;
        FloatingActionButton EditSerial;

        TextView Symbol;
        TextView Article;
        EditText Partia;
        EditText Paleta;
        EditText SerialNumber;
        EditText Lot;
        EditText ProdDate;
        EditText BestBefore;
        TextView Flog;
        TextView Owner;
        TextView Location;
        TextView OnDoc;
        TextView Ordered;
        TextView InWarehouse;
        TextView Unit;
        TextView CanBeAddedToLoc;
        TextView KodEAN;
        TextView NrKat;

        NumericUpDown NumAmount;

        DokumentVO Dokument;
        ExtendedModel.DocumentItemVO Item;
        DocTypes DocType = DocTypes.PW;
        ItemActivityMode Mode = ItemActivityMode.Create;
        Operation Operation = Operation.In;
        bool BufferSet = false;
        bool FromScanner = false;
        bool IsBlockedPosition
        {
            get
            {
                if (Item != null && Item.Base != null)
                    return Item.Base.dataModyfikacji != Item.Base.dataUtworzenia
                        && Item.Base.numIloscZrealizowana == Item.Base.numIloscZlecona;
                return false;
            }
            set { return; }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_documentitem);

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
            Mode = (ItemActivityMode)Intent.GetIntExtra(DocumentItemActivity_Common.Vars.Mode, 0);
            Operation = (Operation)
                Intent.GetIntExtra(DocumentItemActivity_Common.Vars.Operation, 0);
            BufferSet = Intent.GetBooleanExtra(DocumentItemActivity_Common.Vars.BufferSet, false);
            FromScanner = Intent.GetBooleanExtra(
                DocumentItemActivity_Common.Vars.FromScanner,
                false
            );

            BarcodeOrder = Globalne.CurrentSettings.BarcodeScanningOrder[DocType];
            GetAndSetControls();
            IsBusy = false;

            NumAmount.FocusField();

            if (
                Globalne.CurrentSettings.InstantScanning[DocType]
                && (FromScanner || Mode == ItemActivityMode.EditAdd)
            )
                OK_Click(this, null);
            Do_Refresh(false);
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

            Paleta.Text = Operation == Operation.In ? Item.ExPaletaP : Item.ExPaletaW;
            Paleta.Tag = Operation == Operation.In ? Item.ExIDPaletaP : Item.ExIDPaletaW;
            Paleta.FocusChange += Paleta_FocusChange;
            Paleta.Click += OnClickTargettableTextView;
            Paleta.FocusChange += OnFocusChangeTargettableTextView;

            EditPaleta.Click += EditPaleta_Click;
            EditSerial.Click += EditSerial_Click;

            SerialNumber.Text = Item.ExSerialNum;
            SerialNumber.Click += OnClickTargettableTextView;
            SerialNumber.FocusChange += OnFocusChangeTargettableTextView;

            KodEAN.Text = Item.ExKodEAN;
            KodEAN.Tag = Item.ExKodEAN;
            //      KodEAN.Click += OnClickTargettableTextView;
            KodEAN.FocusChange += OnFocusChangeTargettableTextView;

            NrKat.Text = Item.ExNrKat;
            NrKat.Tag = Item.ExNrKat;
            NrKat.FocusChange += OnFocusChangeTargettableTextView;

            Lot.Text = Item.ExLot;
            Lot.Click += OnClickTargettableTextView;
            Lot.FocusChange += OnFocusChangeTargettableTextView;

            Flog.Text =
                Operation == Operation.In ? Item.ExFunkcjaLogistycznaP : Item.ExFunkcjaLogistycznaW;
            Flog.Tag =
                Operation == Operation.In
                    ? Item.ExIDFunkcjaLogistycznaP
                    : Item.ExIDFunkcjaLogistycznaW;
            EditFlog.Click += EditFlog_Click;

            Location.Text = Operation == Operation.In ? Item.ExLokalizacjaP : Item.ExLokalizacjaW;
            Location.Tag =
                Operation == Operation.In ? Item.ExIDLokalizacjaP : Item.ExIDLokalizacjaW;
            Location.TextChanged += Location_TextChanged;

            EditLocation.Click += EditLocation_Click;

            Unit.Text = Item.ExUnit;
            Unit.Tag = Item.ExIDUnit;
            EditUnit.Click += EditUnit_Click;

            Owner.Text = Item.ExOwner;
            Owner.Tag = Item.ExIDOwner;
            EditOwner.Click += EditOwner_Click;

            Ordered.Text = Item.Base.numIloscZlecona.ToString("F3");

            var przychod = Globalne.przychrozchBL.PobierzPrzychód(
                Operation == Operation.In ? Item.ExIDLokalizacjaP : Item.ExIDLokalizacjaW,
                Item.Base.idTowaru
            );
            if (przychod.ID > -1)
            {
                Item.ExBestBefore = przychod.dataPrzydatnosci;
                Item.ExProductionDate = przychod.dataProdukcji;
            }

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

                if (
                    Operation == Operation.In
                        ? (Item.Base.idPaletaP >= 0)
                        : (Item.Base.idPaletaW >= 0)
                )
                {
                    EditPaleta.Visibility = ViewStates.Gone;
                    Paleta.Enabled = false;
                }

                if (Item.Base.strNumerySeryjne != "")
                    SerialNumber.Enabled = false;

                if (Item.Base.strLoty != "")
                    Lot.Enabled = false;

                if (Item.Base.kodean != "")
                    KodEAN.Enabled = false;

                if (Item.Base.NrKat != "")
                    NrKat.Enabled = false;

                if (
                    !Globalne.CurrentSettings.AllowSourceLocationChange
                    && (
                        (
                            Operation == Operation.In
                                ? (Item.Base.idLokalizacjaP >= 0)
                                : (Item.Base.idLokalizacjaW >= 0)
                        ) || (!Globalne.CurrentSettings.Lokalizacje)
                    )
                )
                    EditLocation.Visibility = ViewStates.Gone;

                if (
                    Operation == Operation.In
                        ? (Item.Base.idFunkcjiLogistycznejP >= 0)
                        : (Item.Base.idFunkcjiLogistycznejW >= 0)
                )
                    EditFlog.Visibility = ViewStates.Gone;

                if (Operation == Operation.Out)
                    EditOwner.Visibility = ViewStates.Gone;
            }

            if (BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation))
            {
                EditArticle.Visibility = ViewStates.Gone;
                EditUnit.Visibility = ViewStates.Gone;
                SerialNumber.Enabled = false;
                EditPartia.Visibility = ViewStates.Gone;
                Partia.Enabled = false;
                Lot.Enabled = false;
                EditOwner.Visibility = ViewStates.Gone;

                //TODO: MP: to chyba powinno byc wlaczone aby pojawilo sie na liscie
                KodEAN.Enabled = false;
                NrKat.Enabled = false;
            }

            if (Mode == ItemActivityMode.EditAdd)
            {
                Partia.Enabled = false;
                Paleta.Enabled = false;
                KodEAN.Enabled = false;
                NrKat.Enabled = false;
                SerialNumber.Enabled = false;
                Lot.Enabled = false;
                EditLocation.Visibility = ViewStates.Gone;
                EditFlog.Visibility = ViewStates.Gone;
                EditUnit.Visibility = ViewStates.Gone;
                EditArticle.Visibility = ViewStates.Gone;
                EditPartia.Visibility = ViewStates.Gone;
                EditPaleta.Visibility = ViewStates.Gone;
            }

            if (Globalne.CurrentSettings.DisableHandLocationsChange)
            {
                DisableButton(EditLocation);
                //DisableButton(EditLocationOut);
            }
            if (Globalne.CurrentSettings.DisableSSCCChange)
            {
                Paleta.Enabled = false;
            }

            if (this.DocType == DocTypes.PZ || this.DocType == DocTypes.PW)
            {
                this.DisableButton(this.EditSerial);
            }
            else
            {
                this.SerialNumber.Enabled = false;
            }

            if (SerialNumber.Text != "")
            {
                this.DisableButton(this.EditSerial);
                this.SerialNumber.Enabled = false;
            }
        }

        private void Location_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (InWarehouse.Visibility == ViewStates.Visible)
            //{
            //    if ((int)Article.Tag >= 0)
            //    {
            //        decimal Amount = Globalne.przychrozchBL.PobierzStanTowaru((int)Article.Tag,
            //                                                                 Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW,
            //                                                                 (int)Location.Tag,
            //                                                                 (int)Partia.Tag,
            //                                                                 Partia.Text,
            //                                                                 (int)Paleta.Tag,
            //                                                                 Paleta.Text,
            //                                                                 (int)Flog.Tag);

            //        if ((int)Unit.Tag >= 0)
            //        {
            //            JednostkaPrzeliczO Jm = Globalne.jednostkaMiaryBL.PobierzJednostkęPrzelicz((int)Unit.Tag, (int)Article.Tag);
            //            Amount /= Jm.numIle;
            //        }

            //        Helpers.SetTextOnTextView(this, InWarehouse, Math.Round(Amount, Globalne.CurrentSettings.DecimalSpaces).ToString("F3"));
            //    }
            //    else
            //        Helpers.SetTextOnTextView(this, InWarehouse, "---");
            //}

            //Location.Tag = Globalne.lokalizacjaBL.PobierzLokalizacjęWgNazwy(e.Text.ToString(), Globalne.Magazyn.ID).ID;
            //Helpers.SetTextOnTextView(this, Location, Math.Round(Amount, Globalne.CurrentSettings.DecimalSpaces).ToString("F3"));


            //Do_Refresh(false);
            //Console.WriteLine(e.Text);
            //Console.WriteLine(e.Start);
            //Console.WriteLine(Location.Text);
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
                    i.PutExtra(PartieActivity.Vars.IDLokalizacji, (int)Location.Tag);
                    i.PutExtra(PartieActivity.Vars.IDPalety, (int)Paleta.Tag);
                    i.PutExtra(PartieActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
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
                    i.PutExtra(PaletyActivity.Vars.IDLokalizacji, (int)Location.Tag);
                    i.PutExtra(PaletyActivity.Vars.IDPartii, (int)Partia.Tag);
                    i.PutExtra(PaletyActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
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

        private void EditSerial_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(SerialActivity));

                i.PutExtra(PaletyActivity.Vars.IDTowaru, (int)Article.Tag);
                i.PutExtra(PaletyActivity.Vars.IDLokalizacji, (int)Location.Tag);
                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.SerialActivityResult
                );
            });
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
            if (!Paleta.IsFocused)
            {
                Refresh();
            }
        }

        private void GetAndSetControls()
        {
            DocumentItemActivity_Common.SetHeaderBasedOnMode(this, Mode);

            TextView Scanhint = FindViewById<TextView>(Resource.Id.scanhint);

            switch (Operation)
            {
                case Operation.In:
                    Scanhint.Text = GetString(
                        Resource.String.documentitem_activity_scanhint_loc_add
                    );
                    break;
                case Operation.Out:
                    Scanhint.Text = GetString(
                        Resource.String.documentitem_activity_scanhint_loc_take
                    );
                    break;
            }

            FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_cancel).Click +=
                Cancel_Click;
            FloatingActionButton OKButton = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_ok
            );

            if (
                BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation)
                || BusinessLogicHelpers.DocumentItems.IsGatheringMode(DocType, Operation)
            )
                OKButton.Click += OK_Click_ZLMM;
            else
                OKButton.Click += OK_Click;

            switch (DocType)
            {
                case DocTypes.PW:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationPW)
                        DisableButton(OKButton);
                    break;
                case DocTypes.RW:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationRW)
                        DisableButton(OKButton);
                    break;
                case DocTypes.ZL:
                    if (Globalne.CurrentSettings.PositionConfirmOnlyByLocationZL)
                        DisableButton(OKButton);
                    break;
                case DocTypes.ZLGathering:
                    break;
                case DocTypes.ZLDistribution:
                    break;
                default:
                    break;
            }

            EditArticle = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_article);
            EditFlog = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_flog);
            EditOwner = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_owner);
            EditLocation = FindViewById<FloatingActionButton>(
                Resource.Id.document_item_btn_location
            );
            EditUnit = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_unit);
            EditPaleta = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_paleta);
            EditPartia = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_partia);
            EditSerial = FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_serial);

            Symbol = FindViewById<TextView>(Resource.Id.document_item_symbol);
            Article = FindViewById<TextView>(Resource.Id.document_item_article);
            Partia = FindViewById<EditText>(Resource.Id.document_item_partia);
            Paleta = FindViewById<EditText>(Resource.Id.document_item_paleta);
            SerialNumber = FindViewById<EditText>(Resource.Id.document_item_serial);
            Lot = FindViewById<EditText>(Resource.Id.document_item_lot);
            Flog = FindViewById<TextView>(Resource.Id.document_item_flog);
            Owner = FindViewById<TextView>(Resource.Id.document_item_owner);
            Location = FindViewById<TextView>(Resource.Id.document_item_location);
            OnDoc = FindViewById<TextView>(Resource.Id.document_item_ondoc);
            Ordered = FindViewById<TextView>(Resource.Id.document_item_ordered);
            InWarehouse = FindViewById<TextView>(Resource.Id.document_item_amountinwarehouse);
            ProdDate = FindViewById<EditText>(Resource.Id.document_item_proddate);
            BestBefore = FindViewById<EditText>(Resource.Id.document_item_bestbefore);
            Unit = FindViewById<TextView>(Resource.Id.document_item_unit);
            CanBeAddedToLoc = FindViewById<TextView>(
                Resource.Id.document_item_amountcanbeaddedtoloc
            );
            NumAmount = FindViewById<NumericUpDown>(Resource.Id.document_item_amount);
            KodEAN = FindViewById<TextView>(Resource.Id.document_item_kodean);
            NrKat = FindViewById<TextView>(Resource.Id.document_item_NrKat);

            // Kompatybilność z 5.0
            NumAmount.Initialize();

            DocumentItemActivity_Common.SetVisibilityOfFields(
                this,
                DocType,
                Dokument.bZlecenie,
                Mode
            );
            SetupFields();

            if (IsBlockedPosition)
                DisableControls();

            Do_Refresh(true);
        }

        private void DisableControls()
        {
            Helpers.CenteredToast("Pozycja tylko do podglądu", ToastLength.Long);
            DisableButton(EditArticle);
            DisableButton(EditFlog);
            DisableButton(EditOwner);
            DisableButton(EditLocation);
            DisableButton(EditUnit);
            DisableButton(EditPaleta);
            DisableButton(EditPartia);
            DisableButton(FindViewById<FloatingActionButton>(Resource.Id.document_item_btn_ok));
            NumAmount.SetDisableControl();
        }

        private void DisableButton(FloatingActionButton button)
        {
            button.SetColorFilter(Color.Gray);
            button.Background.Alpha = 50;
            button.Enabled = false;
        }

        private PozycjaVO GetDBObject()
        {
            PozycjaVO Poz = Item.Base;
            string przedTag1 = "";
            string przedText1 = "";
            int przedLocId1 = -2;

            Poz.idDokumentu = Dokument.ID;

            Poz.idTowaru = (int)Article.Tag;

            if (Globalne.CurrentSettings.Partie)
            {
                Poz.idPartia = Globalne.partiaBL.PobierzIDPartii(Partia.Text);
                Poz.strPartia = Partia.Text;
            }
            else
            {
                Poz.idPartia = -1;
                Poz.strPartia = "";
            }

            if (Globalne.CurrentSettings.Palety)
            {
                if (Operation == Operation.In)
                {
                    Poz.strPaletaP = Paleta.Text;
                    Poz.idPaletaP = Globalne.paletaBL.PobierzIDPalety(Paleta.Text);
                }
                else
                {
                    Poz.strPaletaW = Paleta.Text;
                    Poz.idPaletaW = Globalne.paletaBL.PobierzIDPalety(Paleta.Text);
                }
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
                if (Operation == Operation.In)
                {
                    Poz.idFunkcjiLogistycznejP = (int)Flog.Tag;
                }
                else
                {
                    Poz.idFunkcjiLogistycznejW = (int)Flog.Tag;
                }
            }
            else
            {
                Poz.idFunkcjiLogistycznejP = -1;
                Poz.idFunkcjiLogistycznejW = -1;
            }

            // Failsafe
            if ((int)Location.Tag > 0 && Location.Text != "")
            {
                if (Globalne.debug_mode == 1)
                {
                    Console.WriteLine("[ARCHUNG!] - fail safe in progress... ! ");
                }

                if (Location.Text != null || Location.Text != "")
                {
                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: Lokalizacja.Text:" + Location.Text);
                    }
                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: Lokalizacja.Tag:" + Location.Tag);
                    }
                }

                przedTag1 = (string)Location.Tag;
                przedText1 = (string)Location.Text;
                przedLocId1 = Globalne
                    .lokalizacjaBL.PobierzLokalizacjęWgNazwy(Location.Text, Globalne.Magazyn.ID)
                    .ID;

                Console.WriteLine(string.Format("Lok ID: {0}, nazwa: {1}", przedTag1, przedText1));
                Console.WriteLine("Lokalizacja ID: " + przedLocId1);

                // na Android 11 pojawiaja sie bledy zwiazane z zapisem poprawnej lokalizacji na dokmentach
                // linijka wyzej naprawia ten problem >> "przedLocId1 = Globalne.lokalizacjaBL.PobierzLokalizacjęWgNazwy(Location.Text, Globalne.Magazyn.ID).ID;"
                // wystarczy ze uzyjemy raz w/w metody i jezeli w pierwszej przepisze zlą lokalizacje to kolejne polecenie juz wskaze poprawna/ odczytana skanerem lokalizacje - SZOK!
                // natomiast Task.Delay(50) wstawiane jest jako dodatkowe zabezpieczenie - o ile to tak mozna nazwac

                Task.Delay(50);
                Location.Tag = Globalne
                    .lokalizacjaBL.PobierzLokalizacjęWgNazwy(Location.Text, Globalne.Magazyn.ID)
                    .ID;
                Task.Delay(50);
            }

            if (Operation == Operation.In)
            {
                if (Globalne.debug_mode == 1)
                {
                    Console.WriteLine("DEBUG_INFO: Operation" + Operation.ToString());
                }

                Poz.idLokalizacjaP = (int)Location.Tag;
            }
            else
            {
                Poz.idLokalizacjaW = (int)Location.Tag;
            }

            Poz.strLoty = Lot.Text;
            Poz.strNumerySeryjne = SerialNumber.Text;
            Poz.idKontrahent = (int)Owner.Tag;
            Poz.kodean = KodEAN.Text;
            Poz.NrKat = NrKat.Text;

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

            Poz.idJednostkaMiary = (int)Unit.Tag;

            if (Poz.intUtworzonyPrzez < 0)
            {
                Poz.intUtworzonyPrzez = Globalne.Operator.ID;
            }

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
                        OK = (!Dict[Key] || (int)Symbol.Text.Length > 0);
                        break;
                    case DocumentItemFields.Article:
                        OK = (!Dict[Key] || (int)Article.Tag >= 0);
                        break;
                    case DocumentItemFields.Flog:
                        OK = (
                            !Dict[Key]
                            || !Globalne.CurrentSettings.FunkcjeLogistyczne
                            || (int)Flog.Tag >= 0
                        );
                        break;
                    case DocumentItemFields.Location:
                        OK = (!Dict[Key] || (int)Location.Tag >= 0);
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
                    case DocumentItemFields.Owner:
                        OK = (!Dict[Key] || (int)Owner.Tag >= 0);
                        break;
                    case DocumentItemFields.Lot:
                        OK = (!Dict[Key] || Lot.Text != "");
                        break;
                    case DocumentItemFields.SerialNumber:
                        OK = (!Dict[Key] || SerialNumber.Text != "");
                        break;
                    case DocumentItemFields.Unit:
                        OK = (!Dict[Key] || (int)Unit.Tag >= 0);
                        break;
                    case DocumentItemFields.KodEAN:
                        OK = (!Dict[Key] || KodEAN.Text != "");
                        break;
                    case DocumentItemFields.NrKat:
                        OK = (!Dict[Key] || NrKat.Text != "");
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

                    if (!await CheckSanity())
                        return;

                    if (Operation == Operation.In)
                    {
                        Poz.dataModyfikacji = DateTime.Now;
                    }
                    if ((int)SerialNumber.Tag > 0)
                        Poz.idNumerSeryjny = (int)SerialNumber.Tag;
                    switch (Mode)
                    {
                        case ItemActivityMode.Create:
                        {
                            // sprawdzenie czy nowo dodany towar i jego paleta jest juz w systemie, jezeli jest to pomin dodawanie

                            if (
                                Globalne.CurrentSettings.OnlyOncePalleteSSCCOnDocument
                                && (DocType == DocTypes.PZ || DocType == DocTypes.PW)
                                && Globalne.paletaBL.CzyPaletaIstnieje(Poz.strPaletaP, -1)
                            )
                            {
                                await Helpers.Alert(
                                    this,
                                    "Ustawienia wdrożeniowe nie pozwalają na dodanie drugiej takiej samej palety.\nPaleta (SSCC) o takim kodzie jest już w systemie.",
                                    Title: "Uwaga!"
                                );
                                return;
                            }

                            // bylo 500
                            Thread.Sleep(Globalne.TaskDelay);

                            // todo fancy  mode
                            int IDPozIst =
                                Globalne.dokumentBL.PobierzIDPozycjiIdentycznejNaDokumencie(Poz);

                            int Res = -1;

                            if (IDPozIst != -1)
                            {
                                PozycjaVO PozIst = Globalne.dokumentBL.PobierzPozycję(IDPozIst);
                                PozIst.intZmodyfikowanyPrzez = Globalne.Operator.ID;
                                PozIst.numIloscZrealizowana += NumAmount.Value;
                                PozIst.numIloscZlecona += NumAmount.Value;

                                Res = Globalne.dokumentBL.EdytujPozycję(
                                    Helpers.StringDocType(DocType),
                                    PozIst,
                                    Dokument.bIgnorujBlokadePartii
                                );
                            }

                            if (IDPozIst < 0 || Res < 0)
                            {
                                Poz.numIloscZlecona = NumAmount.Value;
                                Poz.numIloscZrealizowana = NumAmount.Value;
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Globalne.dokumentBL.ZróbPozycję(
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
                            if (!Dokument.bZlecenie)
                                Poz.numIloscZlecona = NumAmount.Value;

                            Poz.numIloscZrealizowana = NumAmount.Value;

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
                                Globalne.dokumentBL.EdytujPozycję(
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
                                Globalne.dokumentBL.EdytujPozycję(
                                    Helpers.StringDocType(DocType),
                                    Poz,
                                    Dokument.bIgnorujBlokadePartii
                                )
                            );
                            break;
                        }
                        case ItemActivityMode.Split:
                        {
                            PozycjaVO Original = Globalne.dokumentBL.PobierzPozycję(Poz.ID);
                            Poz.numIloscZrealizowana = NumAmount.Value;

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
                                    Globalne.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
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
                                        Globalne.dokumentBL.ZróbPozycję(
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
                        i.PutExtra(
                            DocumentItemActivity_Common.Results.WereScanned,
                            LastScanData.ToArray()
                        );

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

        private async void OK_Click_ZLMM(object sender, EventArgs e)
        {
            try
            {
                if (!await CheckSanity())
                    return;

                if (Globalne.debug_mode == 1)
                {
                    Console.WriteLine("DEBUG_INFO: Getting DBObject");
                }

                PozycjaVO Poz = GetDBObject();

                if (Globalne.debug_mode == 1)
                {
                    Console.WriteLine("DEBUG_INFO: DBObject Received");
                }

                var przychod = Globalne.przychrozchBL.PobierzPrzychód(
                    Globalne.lokalizacjaBL.PobierIdLokalizacjaWgNazwy(
                        Location.Text,
                        Globalne.Magazyn.ID
                    ),
                    Poz.idTowaru
                );
                if (
                    przychod.ID > 0
                    && (
                        DocType == DocTypes.ZL
                        || DocType == DocTypes.ZLGathering
                        || DocType == DocTypes.ZLDistribution
                    )
                )
                {
                    //Poz.dataUtworzenia = przychod.dataPrzychodu;
                    Poz.dtDataProdukcji = przychod.dataProdukcji;
                    Poz.dtDataPrzydatności = przychod.dataPrzydatnosci;
                }

                if ((int)SerialNumber.Tag > 0)
                    Poz.idNumerSeryjny = (int)SerialNumber.Tag;

                switch (Mode)
                {
                    case ItemActivityMode.Create:
                    {
                        if (Globalne.debug_mode == 1)
                        {
                            Console.WriteLine("DEBUG_INFO: Checking the same position on document");
                        }

                        int IDPozIst = Globalne.dokumentBL.PobierzIDPozycjiIdentycznejNaDokumencie(
                            Poz
                        );

                        if (Poz != null && IDPozIst != 0 && Poz.ID != -1)
                        {
                            Console.WriteLine(Poz.ID.ToString());
                        }

                        if (IDPozIst == -1)
                        {
                            if (Globalne.debug_mode == 1)
                            {
                                Console.WriteLine(
                                    "DEBUG_INFO: Getting the same position once again"
                                );
                            }

                            IDPozIst = Globalne.dokumentBL.PobierzIDPozycjiIdentycznejNaDokumencie(
                                Poz
                            );
                        }

                        if (Globalne.debug_mode == 1)
                        {
                            Console.WriteLine(
                                "DEBUG_INFO: Received document position: " + IDPozIst.ToString()
                            );
                        }

                        int Res = -1;

                        if (IDPozIst != -1)
                        {
                            PozycjaVO PozIst = Globalne.dokumentBL.PobierzPozycję(IDPozIst);
                            PozIst.intZmodyfikowanyPrzez = Globalne.Operator.ID;
                            PozIst.numIloscZebrana += NumAmount.Value;
                            PozIst.numIloscZlecona += NumAmount.Value;

                            if (Globalne.debug_mode == 1)
                            {
                                Console.WriteLine("DEBUG_INFO: Editing the same position");
                            }

                            Res = Globalne.dokumentBL.EdytujPozycję(
                                Helpers.StringDocType(DocType),
                                PozIst,
                                Dokument.bIgnorujBlokadePartii
                            );
                        }

                        if (IDPozIst < 0 || Res < 0)
                        {
                            Poz.numIloscZlecona = NumAmount.Value;
                            Poz.numIloscZebrana = NumAmount.Value;

                            if (Globalne.debug_mode == 1)
                            {
                                Console.WriteLine(
                                    "DEBUG_INFO: The same position not found - Creating new position"
                                );
                            }

                            AutoException.ThrowIfNotNull(
                                this,
                                ErrorType.ItemCreationError,
                                Globalne.dokumentBL.ZróbPozycję(
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
                        if (Globalne.debug_mode == 1)
                        {
                            Console.WriteLine("DEBUG_INFO: Editing position");
                        }

                        if (
                            (DocType == DocTypes.ZLDistribution || DocType == DocTypes.ZL)
                            && IsBlockedPosition
                        )
                        {
                            await Helpers.Alert(
                                this,
                                "Pozycja tylko w trybie do odczytu. Zamiany beda odrzucone."
                            );
                            break;
                        }

                        if (Operation == Operation.Out)
                        {
                            if (!Dokument.bZlecenie)
                                Poz.numIloscZlecona = NumAmount.Value;

                            Poz.numIloscZebrana = NumAmount.Value;

                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZebrana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );
                        }
                        else if (Operation == Operation.In)
                            Poz.numIloscZrealizowana = NumAmount.Value;

                        if (Poz.numIloscZrealizowana > Poz.numIloscZebrana)
                            AutoException.ThrowIfNotNull(
                                this,
                                Resource.String.documentitem_edition_zlmm_distrib_more_than_gather
                            );

                        AutoException.ThrowIfNotNull(
                            this,
                            ErrorType.ItemCreationError,
                            Globalne.dokumentBL.EdytujPozycję(
                                Helpers.StringDocType(DocType),
                                Poz,
                                Dokument.bIgnorujBlokadePartii
                            )
                        );
                        break;
                    }
                    case ItemActivityMode.EditAdd:
                    {
                        if (Operation == Operation.Out)
                        {
                            if (!Dokument.bZlecenie)
                                Poz.numIloscZlecona += NumAmount.Value;

                            Poz.numIloscZebrana += NumAmount.Value;

                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZebrana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );
                        }
                        else if (Operation == Operation.In)
                            Poz.numIloscZrealizowana += NumAmount.Value;

                        if (Poz.numIloscZrealizowana > Poz.numIloscZebrana)
                            AutoException.ThrowIfNotNull(
                                this,
                                Resource.String.documentitem_edition_zlmm_distrib_more_than_gather
                            );

                        AutoException.ThrowIfNotNull(
                            this,
                            ErrorType.ItemCreationError,
                            Globalne.dokumentBL.EdytujPozycję(
                                Helpers.StringDocType(DocType),
                                Poz,
                                Dokument.bIgnorujBlokadePartii
                            )
                        );
                        break;
                    }
                    case ItemActivityMode.Split:
                    {
                        PozycjaVO Original = Globalne.dokumentBL.PobierzPozycję(Poz.ID);

                        if (Operation == Operation.Out)
                        {
                            Poz.numIloscZebrana = NumAmount.Value;

                            if (
                                Dokument.bZlecenie
                                && Poz.numIloscZebrana > Poz.numIloscZlecona
                                && !Dokument.bDopuszczalnePrzekroczenieIlosci
                            )
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource.String.documentitem_creationedition_toomuch
                                );

                            if (Poz.numIloscZebrana >= Poz.numIloscZlecona)
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Globalne.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
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
                                    Poz.numIloscZebrana
                                );

                                try
                                {
                                    AutoException.ThrowIfNotNull(
                                        this,
                                        ErrorType.ItemCreationError,
                                        Globalne.dokumentBL.ZróbPozycję(
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
                                }
                            }
                        }
                        else if (Operation == Operation.In)
                        {
                            Poz.numIloscZrealizowana = NumAmount.Value;

                            if (Poz.numIloscZrealizowana > Poz.numIloscZebrana)
                                AutoException.ThrowIfNotNull(
                                    this,
                                    Resource
                                        .String
                                        .documentitem_edition_zlmm_distrib_more_than_gather
                                );

                            if (Poz.numIloscZrealizowana < Original.numIloscZrealizowana)
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Globalne.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
                                Original = (PozycjaVO)Helpers.ObjectCopy(Poz, typeof(PozycjaVO));
                                ;
                            }

                            if (Poz.numIloscZrealizowana >= Poz.numIloscZebrana)
                            {
                                AutoException.ThrowIfNotNull(
                                    this,
                                    ErrorType.ItemCreationError,
                                    Globalne.dokumentBL.EdytujPozycję(
                                        Helpers.StringDocType(DocType),
                                        Poz,
                                        Dokument.bIgnorujBlokadePartii
                                    )
                                );
                            }
                            else
                            {
                                Poz.numIloscZebrana = NumAmount.Value;
                                Poz.numIloscZlecona = NumAmount.Value;

                                BusinessLogicHelpers.DocumentItems.EditSplitItem(
                                    this,
                                    Original,
                                    true,
                                    DocType,
                                    Operation,
                                    Poz.numIloscZrealizowana
                                );

                                if (DocType == DocTypes.ZLDistribution)
                                    Poz.bezposrednieZL = true;
                                //var przychod = Globalne.przychrozchBL.PobierzPrzychód(Globalne.lokalizacjaBL.PobierIdLokalizacjaWgNazwy(Location.Text, Globalne.Magazyn.ID), Poz.idTowaru);
                                //if (przychod.ID > 0 && (DocType == DocTypes.ZL || DocType == DocTypes.ZLGathering || DocType == DocTypes.ZLDistribution))
                                //{
                                //    Poz.dataUtworzenia = przychod.dataPrzychodu;
                                //    Poz.dtDataProdukcji = przychod.dataProdukcji;
                                //    Poz.dtDataPrzydatności = przychod.dataPrzydatnosci;
                                //}

                                try
                                {
                                    AutoException.ThrowIfNotNull(
                                        this,
                                        ErrorType.ItemCreationError,
                                        Globalne.dokumentBL.ZróbPozycję(
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
                                        true,
                                        DocType,
                                        Operation
                                    );
                                }
                            }
                        }

                        break;
                    }
                }

                Intent i = new Intent();

                if (LastScanData != null)
                    i.PutExtra(
                        DocumentItemActivity_Common.Results.WereScanned,
                        LastScanData.ToArray()
                    );

                //if (sender == null)
                //Helpers.PlaySound(this, Resource.Raw.sound_ok);

                SetResult(Result.Ok, i);
                this.Finish();

                //Console.WriteLine("Garbydż Kolekt będzie zamiatał");
                //GC.Collect();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
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

        private void EditLocation_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(LocationsActivity));
                i.PutExtra(
                    LocationsActivity.Vars.IDMagazynu,
                    Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW
                );

                if (Operation == Operation.Out)
                {
                    i.PutExtra(LocationsActivity.Vars.IDDokumentu, Dokument.ID);
                    i.PutExtra(LocationsActivity.Vars.IDTowaru, (int)Article.Tag);
                    i.PutExtra(LocationsActivity.Vars.IDPartii, (int)Partia.Tag);
                    i.PutExtra(LocationsActivity.Vars.IDPalety, (int)Paleta.Tag);
                    i.PutExtra(LocationsActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
                    i.PutExtra(LocationsActivity.Vars.Rozchód, true);
                    i.PutExtra(LocationsActivity.Vars.AskOnStart, false);
                }
                else
                    i.PutExtra(LocationsActivity.Vars.AskOnStart, true);

                StartActivityForResult(
                    i,
                    (int)DocumentItemActivity_Common.ResultCodes.LocationsActivityResult
                );
            });
        }

        async void EditFlog_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                        this,
                        Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW,
                        Flog
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
                i.PutExtra(
                    ArticlesActivity.Vars.IDMagazynu,
                    Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW
                );

                if (Operation == Operation.Out)
                {
                    i.PutExtra(ArticlesActivity.Vars.IDDokumentu, Dokument.ID);
                    i.PutExtra(ArticlesActivity.Vars.IDLokalizacji, (int)Location.Tag);
                    i.PutExtra(ArticlesActivity.Vars.IDPartii, (int)Partia.Tag);
                    i.PutExtra(ArticlesActivity.Vars.IDPalety, (int)Paleta.Tag);
                    i.PutExtra(ArticlesActivity.Vars.IDFunkcjiLogistycznej, (int)Flog.Tag);
                    i.PutExtra(ArticlesActivity.Vars.Rozchód, true);
                }

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
                Paleta.Text != ""
                && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry
                && Globalne.CurrentSettings.Palety
            )
                if (
                    BusinessLogicHelpers.DocumentItems.GetSSCCData(Paleta.Text, ref Item, Operation)
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
                    //if (Item.ExSymbol != "")
                    //{
                    //    Symbol.Text = Item.ExSymbol;
                    //}

                    if (Item.ExUnit != "")
                    {
                        Unit.Text = Item.ExUnit;
                        Unit.Tag = Item.ExIDUnit;
                    }

                    if (Item.ExLot != "")
                    {
                        Lot.Text = Item.ExLot;
                    }

                    if (Item.ExSerialNum != "")
                    {
                        SerialNumber.Text = Item.ExSerialNum;
                    }

                    if (Item.ExKodEAN != "")
                    {
                        KodEAN.Text = Item.ExKodEAN;
                    }

                    if (Item.ExNrKat != "")
                    {
                        NrKat.Text = Item.ExNrKat;
                    }

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
                    int ID = Globalne.partiaBL.PobierzIDPartii(Partia.Text);
                    Partia.Tag = ID;
                }

                if (Set[DocumentItemDisplayElements.Paleta] && Globalne.CurrentSettings.Palety)
                {
                    int ID = Globalne.paletaBL.PobierzIDPalety(Paleta.Text);

                    if (ID != (int)Paleta.Tag)
                    {
                        Paleta_Refresh();
                    }

                    Paleta.Tag = ID;
                }

                if (
                    Set[DocumentItemDisplayElements.SerialNumber]
                    && Globalne.CurrentSettings.SerialNumber
                )
                {
                    int ID = Globalne.numerSeryjnyBL.PobierzIDNumeruSeryjnego(SerialNumber.Text);

                    if (ID != (int)SerialNumber.Tag)
                    {
                        Paleta_Refresh();
                    }

                    SerialNumber.Tag = ID;
                }
            }

            if (Set[DocumentItemDisplayElements.InWarehouse])
            {
                if ((int)Article.Tag >= 0)
                {
                    decimal Amount = Globalne.przychrozchBL.PobierzStanTowaru(
                        (int)Article.Tag,
                        Operation == Operation.In ? Dokument.intMagazynP : Dokument.intMagazynW,
                        (int)Location.Tag,
                        (int)Partia.Tag,
                        Partia.Text,
                        (int)Paleta.Tag,
                        Paleta.Text,
                        (int)Flog.Tag,
                        (int)SerialNumber.Tag,
                        true
                    );

                    if ((int)Unit.Tag >= 0)
                    {
                        JednostkaPrzeliczO Jm = Globalne.jednostkaMiaryBL.PobierzJednostkęPrzelicz(
                            (int)Unit.Tag,
                            (int)Article.Tag
                        );
                        if (Jm.ID > -1)
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
                if ((int)Location.Tag >= 0 && (int)Article.Tag >= 0)
                {
                    int Value = Globalne.przychrozchBL.ObliczIleCałkTowaruZmieściSięWLokalizacji(
                        (int)Article.Tag,
                        (int)Unit.Tag,
                        (int)Location.Tag
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
                                Helpers.SetTextOnTextView(this, Symbol, Art.strSymbol);
                                Helpers.SetTextOnTextView(this, Article, Art.strNazwa);
                                Article.Tag = Art.ID;

                                JednostkaMiaryO Jedn =
                                    Globalne.towarBL.PobierzJednostkęDomyślnąTowaru(Art.ID);
                                Unit.Tag = Jedn.ID;
                                Unit.Text = Jedn.strNazwa;

                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.LocationsActivityResult:
                        {
                            LokalizacjaVO Lok = (LokalizacjaVO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    LocationsActivity.Results.SelectedJSON,
                                    typeof(LokalizacjaVO)
                                );

                            if (Lok != null && Lok.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, Location, Lok.strNazwa);
                                Location.Tag = Lok.ID;
                                Refresh();
                            }

                            break;
                        }
                        case (int)DocumentItemActivity_Common.ResultCodes.PartiaActivityResult:
                        {
                            PartiaO Prt = (PartiaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    PartieActivity.Results.SelectedJSON,
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
                        case (int)DocumentItemActivity_Common.ResultCodes.PaletaActivityResult:
                        {
                            PaletaO Pal = (PaletaO)
                                Helpers.DeserializePassedJSON(
                                    data,
                                    PartieActivity.Results.SelectedJSON,
                                    typeof(PaletaO)
                                );

                            if (Pal != null && Pal.ID >= 0)
                            {
                                Helpers.SetTextOnTextView(this, Paleta, Pal.strOznaczenie);
                                Paleta.Tag = Pal.ID;
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

        protected override void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            RunIsBusyAction(() => DoBarcode());
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.KeyCode == Keycode.Enter && e.Action == KeyEventActions.Down)
            {
                if (
                    BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation)
                    || BusinessLogicHelpers.DocumentItems.IsGatheringMode(DocType, Operation)
                )
                {
                    OK_Click_ZLMM(null, null);
                }
                else
                {
                    OK_Click(null, null);
                }

                return true;
            }
            else
                return base.DispatchKeyEvent(e);
        }

        protected override async Task<bool> CheckBeforeAssumingScanningPath(List<string> BarcodesL)
        {
            await base.CheckBeforeAssumingScanningPath(BarcodesL);

            LokalizacjaVO Loc = Barcodes.GetLocationFromBarcode(BarcodesL[0], true);
            int Lokalizacja =
                Operation == Operation.In ? Item.Base.idLokalizacjaP : Item.Base.idLokalizacjaW;

            Console.WriteLine("---------- SKAN LOKALIZACJI -----------");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(Loc.ID + " >> " + Loc.strNazwa);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("-------------- KONIEC -----------------");

            if (
                Loc.ID > -1
                && !Globalne.CurrentSettings.AllowSourceLocationChange
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
                    Location.Tag = Loc.ID;
                    Location.Text = Loc.strNazwa;
                });

                LastScanData = null;
                IsBusy = false;

                if (
                    BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation)
                    || BusinessLogicHelpers.DocumentItems.IsGatheringMode(DocType, Operation)
                )
                {
                    OK_Click_ZLMM(null, null);
                }
                else
                {
                    OK_Click(null, null);
                }

                return false;
            }
            else
            {
                return true;
            }
        }

        private void DoBarcode()
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            try
            {
                IsBusy = false;

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
                        Globalne.kodykreskoweBL.WyszukajKodKreskowy(LastScanData[0])?.Towar == "";
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
                                .Select(x => x == skanowanyTowar.ID)
                                .Count() == 0
                        )
                        {
                            AutoException.ThrowIfNotNull(
                                this,
                                Resource.String.articles_cannot_be_used
                            );
                        }
                        // jezeli zaznaczona jest opcja w ustawieniach - pomin potwierdzanie pozycji kolejnym towarem
                        if (
                            (
                                DocType.Equals(DocTypes.RW)
                                && Globalne.CurrentSettings.SkipConfirmPositionRW
                            )
                            || (
                                DocType.Equals(DocTypes.PW)
                                && Globalne.CurrentSettings.SkipConfirmPositionPW
                            )
                            || (
                                DocType.Equals(DocTypes.PZ)
                                && Globalne.CurrentSettings.SkipConfirmPositionPZ
                            )
                            || (
                                DocType.Equals(DocTypes.WZ)
                                && Globalne.CurrentSettings.SkipConfirmPositionWZ
                            )
                        )
                        {
                            LastScanData = null;
                            return;
                        }
                    }
                }

                // OK_Click(null, null);

                if (
                    BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation)
                    || BusinessLogicHelpers.DocumentItems.IsGatheringMode(DocType, Operation)
                )
                {
                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: OK_Click_ZLMM entered");
                    }
                    if (Operation != Operation.In)
                        EditingDocumentsActivityZLMM.LastScanDataFromActivity = LastScanData?[0];
                    OK_Click_ZLMM(null, null);
                }
                else
                {
                    if (Globalne.debug_mode == 1)
                    {
                        Console.WriteLine("DEBUG_INFO: OK_Click entered");
                    }
                    OK_Click(null, null);
                }
            }
            catch (Exception ex)
            {
                LastScanData = null;
                Helpers.HandleError(this, ex);
                return;
            }
            finally
            {
                Helpers.HideProgressDialog();
            }
        }
    }
}
