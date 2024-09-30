using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Enums;
using G_Mobile_Android_WMS.ExtendedModel;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API.Konfiguracje;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    public class TerminalSettings : IKonfiguracjaKontroler
    {
        public static string SettingsBundleName = "GMobileWMS";
        public string IP { get; set; }
        public string Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public ScreenOrientation Orientation { get; set; }
        Konfiguracja IKonfiguracjaKontroler.Konfig
        {
            get { return new Konfiguracja(this.IP, this.Port, this.User, this.Password); }
            set
            {
                this.IP = value.IP;
                this.Port = value.Port;
                this.User = value.User;
                this.Password = value.Password;
            }
        }

        public TerminalSettings()
        {
            IP = "";
            Port = "";
            User = "";
            Password = "";
            Orientation = ScreenOrientation.Portrait;
        }

        public static TerminalSettings GetSettings()
        {
            var SettingsBundle = Application.Context.GetSharedPreferences(
                SettingsBundleName,
                FileCreationMode.Private
            );

            if (SettingsBundle == null)
                return null;

            string Json = SettingsBundle.GetString("Json", null);

            if (Json == null)
                return null;
            else
            {
                TerminalSettings Set;

                try
                {
                    Set = Newtonsoft.Json.JsonConvert.DeserializeObject<TerminalSettings>(Json);
                }
                catch (Exception)
                {
                    Set = new TerminalSettings();
                }

                Set.Password = Encryption.AESDecrypt(Set.Password);

                return Set;
            }
        }

        public static void SaveSettings(TerminalSettings Set)
        {
            var SettingsBundle = Application.Context.GetSharedPreferences(
                SettingsBundleName,
                FileCreationMode.Private
            );
            var SettingsEditor = SettingsBundle.Edit();

            // Encrypt the password, then return it to its unencrypted state for further operations
            string UEPass = Set.Password;

            Set.Password = Encryption.AESEncrypt(Set.Password);
            string Json = Newtonsoft.Json.JsonConvert.SerializeObject(Set);

            Set.Password = UEPass;

            SettingsEditor.PutString("Json", Json);
            SettingsEditor.Apply();
        }

        void IKonfiguracjaKontroler.ZaładujKonfiguracje()
        {
            TerminalSettings terminal = GetSettings();
            this.IP = terminal.IP;
            this.Orientation = terminal.Orientation;
            this.Password = terminal.Password;
            this.Port = terminal.Port;
            this.User = terminal.User;
        }
    }

    public class WMSSettings
    {
        public bool CheckCanCloseApp { get; set; }

        public bool CheckCanBarcodeLogin { get; set; }

        public bool InventAutoClose { get; set; }

        public Dictionary<Modules, bool> Modules { get; set; }
        public Dictionary<DocTypes, bool> InstantScanning { get; set; }
        public int ModulesCheckRefreshRate { get; set; }

        public int DocumentsDaysDisplayThreshhold { get; set; }

        public string SetRejestrForDetal { get; set; }

        public string SetContrahForDetal { get; set; }

        public string SetRejestrForMM { get; set; }

        public string SetMagazineForMM { get; set; }

        public Dictionary<
            DocTypes,
            Dictionary<DocumentFields, bool>
        > CreatingDocumentsRequiredFields { get; set; }

        public int DecimalSpaces { get; set; }
        public bool FunkcjeLogistyczne { get; set; }

        public bool Palety { get; set; }
        public bool SerialNumber { get; set; } = true;
        public bool Partie { get; set; }
        public bool bkodean { get; set; }

        public bool bNrKat { get; set; }
        public bool Lokalizacje { get; set; }

        public string DateFormat { get; set; }

        public bool InsertProdDate { get; set; }

        public int DaysToAddToProdDate { get; set; }

        public bool InsertBestBeforeDate { get; set; }

        public int DaysToAddToBestBeforeDate { get; set; }

        public bool Multipicking { get; set; }

        public bool MultipickingConfirmOutLocation { get; set; }

        public bool MultipickingSelectDocuments { get; set; }

        public bool MultipickingConfirmArticle { get; set; }

        public bool MultipickingConfirmInLocation { get; set; }

        public int MultipickingDelayBeforeClose { get; set; }

        public bool MultipickingSetStatusClose { get; set; }
        public bool MultipickingAutoKompletacjaAfterFinish { get; set; }

        public bool AllowSourceLocationChange { get; set; }
        public bool DisableHandLocationsChange { get; set; }

        public bool LocationPositionsSuggestedZL { get; set; }

        public bool OnlyOncePalleteSSCCOnDocument { get; set; }
        public bool DisableSSCCChange { get; set; }

        public bool PositionConfirmOnlyByLocationRW { get; set; }
        public bool PositionConfirmOnlyByLocationPW { get; set; }
        public bool PositionConfirmOnlyByLocationZL { get; set; }

        public bool SkipConfirmPositionPZ { get; set; }
        public bool SkipConfirmPositionWZ { get; set; }
        public bool SkipConfirmPositionPW { get; set; }
        public bool SkipConfirmPositionRW { get; set; }

        public int DefaultValueOnOrderDocRW { get; set; }
        public int DefaultValueOnOrderDocPW { get; set; }
        public int DefaultValueOnOrderDocWZ { get; set; }
        public int DefaultValueOnOrderDocPZ { get; set; }
        public int DefaultValueOnOrderDocZL { get; set; }

        public int DefaultValueOnDocRW { get; set; }
        public int DefaultValueOnDocPW { get; set; }
        public int DefaultValueOnDocWZ { get; set; }
        public int DefaultValueOnDocPZ { get; set; }
        public int DefaultValueOnDocZL { get; set; }

        public bool DisableNavigationBar { get; set; }

        public bool EnableCameraCaptureButton { get; set; }

        public bool BarcodeScanningOrderForce { get; set; }
        public bool BarcodeScanningRemoveSpecialCharacters { get; set; }
        public Dictionary<DocTypes, List<int>> BarcodeScanningOrder { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentLeave { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentPause { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentEnter { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentDone { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentFinish { get; set; }
        public Dictionary<DocTypes, int> StatusesToSetOnDocumentFinishIncorrect { get; set; }
        public Dictionary<
            DocTypes,
            Dictionary<EditingDocumentsListDisplayElements, bool>
        > EditingDocumentsListDisplayElementsListsINNNR { get; set; }
        public Dictionary<
            DocTypes,
            Dictionary<DocumentItemDisplayElements, bool>
        > EditingDocumentItemDisplayElementsListsKAT { get; set; }
        public Dictionary<
            DocTypes,
            Dictionary<DocumentItemFields, bool>
        > RequiredDocItemFields { get; set; }
        public Dictionary<string, string> CodeParsing { get; set; }
        public bool GetDataFromFirstSSCCEntry { get; set; }
        public bool AutoDetal { get; set; }
        public bool QuickMM { get; set; }
        public ZLMMMode DefaultZLMode { get; set; }
        public ZLMMMode DefaultMMMode { get; set; }

        public WMSSettings()
        {
            CheckCanCloseApp = false;
            InventAutoClose = false;
            CheckCanBarcodeLogin = true;
            ModulesCheckRefreshRate = 10000;
            DocumentsDaysDisplayThreshhold = 2;
            SetRejestrForDetal = "";
            SetContrahForDetal = "";
            SetRejestrForMM = "";
            SetMagazineForMM = "";
            DateFormat = "dd.MM.yyyy";
            InsertProdDate = false;
            InsertBestBeforeDate = false;
            DaysToAddToBestBeforeDate = 0;
            DaysToAddToProdDate = 0;
            GetDataFromFirstSSCCEntry = false;
            AutoDetal = false;
            QuickMM = false;
            DisableNavigationBar = false;
            DisableHandLocationsChange = false;
            LocationPositionsSuggestedZL = true;
            EnableCameraCaptureButton = false;

            DisableSSCCChange = false;
            OnlyOncePalleteSSCCOnDocument = false;

            DecimalSpaces = 3;
            FunkcjeLogistyczne = false;
            Palety = false;
            Partie = false;
            Lokalizacje = false;
            bkodean = false;
            bNrKat = false;

            Multipicking = true;
            MultipickingConfirmArticle = true;
            MultipickingSetStatusClose = false;
            MultipickingConfirmInLocation = true;
            MultipickingConfirmOutLocation = true;
            MultipickingSelectDocuments = false;
            MultipickingDelayBeforeClose = 5000;

            PositionConfirmOnlyByLocationPW = false;
            PositionConfirmOnlyByLocationRW = false;
            PositionConfirmOnlyByLocationZL = false;

            DefaultZLMode = ZLMMMode.None;
            DefaultMMMode = ZLMMMode.None;

            Modules = new Dictionary<Enums.Modules, bool>()
            {
                [Enums.Modules.PW] = true,
                [Enums.Modules.PZ] = true,
                [Enums.Modules.RW] = true,
                [Enums.Modules.WZ] = true,
                [Enums.Modules.ZL] = true,
                [Enums.Modules.MM] = true,
                [Enums.Modules.STAN] = true,
                [Enums.Modules.IN] = true,
                [Enums.Modules.KOMPLETACJA] = true,
            };

            InstantScanning = new Dictionary<Enums.DocTypes, bool>()
            {
                [Enums.DocTypes.PW] = false,
                [Enums.DocTypes.PZ] = false,
                [Enums.DocTypes.RW] = false,
                [Enums.DocTypes.WZ] = false,
                [Enums.DocTypes.ZL] = false,
                [Enums.DocTypes.MM] = false,
                [Enums.DocTypes.ZLGathering] = false,
                [Enums.DocTypes.MMGathering] = false,
                [Enums.DocTypes.ZLDistribution] = false,
                [Enums.DocTypes.MMDistribution] = false,
                [Enums.DocTypes.IN] = false,
            };

            CodeParsing = new Dictionary<string, string>()
            {
                [nameof(KodKreskowyZSzablonuO.DataProdukcji)] = nameof(
                    DocumentItemVO.ExProductionDate
                ), // Should be static
                [nameof(KodKreskowyZSzablonuO.DataPrzydatności)] = nameof(
                    DocumentItemVO.ExBestBefore
                ), // Should be static
                [nameof(KodKreskowyZSzablonuO.Partia)] = nameof(DocumentItemVO.ExPartia),
                [nameof(KodKreskowyZSzablonuO.Paleta)] = nameof(DocumentItemVO.ExPaletaP),
                [nameof(KodKreskowyZSzablonuO.Ilość)] = nameof(
                    DocumentItemVO.Base.numIloscZrealizowana
                ),
                [nameof(KodKreskowyZSzablonuO.KrajPochodzenia)] = "",
                [nameof(KodKreskowyZSzablonuO.NrSeryjny)] = nameof(DocumentItemVO.ExSerialNum),
                [nameof(KodKreskowyZSzablonuO.NumerZamówienia)] = "",
                [nameof(KodKreskowyZSzablonuO.Lot)] = nameof(DocumentItemVO.ExLot),
                [nameof(KodKreskowyZSzablonuO.Producent)] = nameof(DocumentItemVO.ExOwner) // Should be static
            };

            StatusesToSetOnDocumentEnter = new Dictionary<Enums.DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            StatusesToSetOnDocumentLeave = new Dictionary<Enums.DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            StatusesToSetOnDocumentDone = new Dictionary<Enums.DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            StatusesToSetOnDocumentFinish = new Dictionary<Enums.DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            StatusesToSetOnDocumentPause = new Dictionary<Enums.DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            StatusesToSetOnDocumentFinishIncorrect = new Dictionary<DocTypes, int>()
            {
                [Enums.DocTypes.PW] = -1,
                [Enums.DocTypes.RW] = -1,
                [Enums.DocTypes.PZ] = -1,
                [Enums.DocTypes.WZ] = -1,
                [Enums.DocTypes.MM] = -1,
                [Enums.DocTypes.MMGathering] = -1,
                [Enums.DocTypes.MMDistribution] = -1,
                [Enums.DocTypes.ZL] = -1,
                [Enums.DocTypes.ZLGathering] = -1,
                [Enums.DocTypes.ZLDistribution] = -1,
                [Enums.DocTypes.IN] = -1,
            };

            BarcodeScanningRemoveSpecialCharacters = true;

            BarcodeScanningOrderForce = false;
            BarcodeScanningOrder = new Dictionary<DocTypes, List<int>>()
            {
                [Enums.DocTypes.PW] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.RW] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.PZ] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.WZ] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.MM] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.MMGathering] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.MMDistribution] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.ZL] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.ZLGathering] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.ZLDistribution] = new List<int>() { BarcodeOrder.Template },

                [Enums.DocTypes.IN] = new List<int>() { BarcodeOrder.Template },
            };

            RequiredDocItemFields = new Dictionary<
                Enums.DocTypes,
                Dictionary<Enums.DocumentItemFields, bool>
            >()
            {
                [Enums.DocTypes.PW] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.RW] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.PZ] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.WZ] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.ZL] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.PaletaIn] = true,
                    [DocumentItemFields.PaletaOut] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.FlogIn] = true,
                    [DocumentItemFields.FlogOut] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.LocationIn] = true,
                    [DocumentItemFields.LocationOut] = true,
                },

                [Enums.DocTypes.ZLGathering] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.ZLDistribution] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.MM] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.PaletaIn] = true,
                    [DocumentItemFields.PaletaOut] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.FlogIn] = true,
                    [DocumentItemFields.FlogOut] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.LocationIn] = true,
                    [DocumentItemFields.LocationOut] = true,
                },

                [Enums.DocTypes.MMGathering] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.MMDistribution] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },

                [Enums.DocTypes.IN] = new Dictionary<Enums.DocumentItemFields, bool>()
                {
                    [DocumentItemFields.Symbol] = true,
                    [DocumentItemFields.Article] = true,
                    [DocumentItemFields.Paleta] = true,
                    [DocumentItemFields.Partia] = true,
                    [DocumentItemFields.Flog] = true,
                    [DocumentItemFields.SerialNumber] = false,
                    [DocumentItemFields.Unit] = true,
                    [DocumentItemFields.Lot] = false,
                    [DocumentItemFields.Owner] = false,
                    [DocumentItemFields.DataProdukcji] = false,
                    [DocumentItemFields.DataPrzydatności] = false,
                    [DocumentItemFields.Location] = true,
                },
            };

            EditingDocumentsListDisplayElementsListsINNNR = new Dictionary<
                Enums.DocTypes,
                Dictionary<Enums.EditingDocumentsListDisplayElements, bool>
            >()
            {
                [Enums.DocTypes.PW] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.Paleta] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.Flog] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.Location] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = true,
                },

                [Enums.DocTypes.RW] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.Paleta] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.Flog] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.Location] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.PZ] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.Paleta] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.Flog] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.Location] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.WZ] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.Paleta] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.Flog] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.Location] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZL] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZLGathering] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZLDistribution] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MM] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MMGathering] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = false,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MMDistribution] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.PaletaOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.FlogOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.GotAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = false,
                    [Enums.EditingDocumentsListDisplayElements.LocationIn] = true,
                    [Enums.EditingDocumentsListDisplayElements.LocationOut] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.IN] = new Dictionary<
                    Enums.EditingDocumentsListDisplayElements,
                    bool
                >()
                {
                    [Enums.EditingDocumentsListDisplayElements.Symbol] = true,
                    [Enums.EditingDocumentsListDisplayElements.ArticleName] = true,
                    [Enums.EditingDocumentsListDisplayElements.Paleta] = true,
                    [Enums.EditingDocumentsListDisplayElements.Partia] = true,
                    [Enums.EditingDocumentsListDisplayElements.Flog] = true,
                    [Enums.EditingDocumentsListDisplayElements.SerialNumber] = false,
                    [Enums.EditingDocumentsListDisplayElements.ProductionDate] = false,
                    [Enums.EditingDocumentsListDisplayElements.BestBefore] = false,
                    [Enums.EditingDocumentsListDisplayElements.DoneAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.SetAmount] = true,
                    [Enums.EditingDocumentsListDisplayElements.Location] = true,
                    [Enums.EditingDocumentsListDisplayElements.Lot] = false,
                    [Enums.EditingDocumentsListDisplayElements.KodEAN] = true,
                    [Enums.EditingDocumentsListDisplayElements.NrKat] = false,
                },
            };

            EditingDocumentItemDisplayElementsListsKAT = new Dictionary<
                Enums.DocTypes,
                Dictionary<Enums.DocumentItemDisplayElements, bool>
            >()
            {
                [Enums.DocTypes.PW] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = false,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.RW] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = false,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.PZ] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = false,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.WZ] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = false,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZLGathering] = new Dictionary<
                    Enums.DocumentItemDisplayElements,
                    bool
                >()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = false,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZLDistribution] = new Dictionary<
                    Enums.DocumentItemDisplayElements,
                    bool
                >()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MMGathering] = new Dictionary<
                    Enums.DocumentItemDisplayElements,
                    bool
                >()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = false,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MMDistribution] = new Dictionary<
                    Enums.DocumentItemDisplayElements,
                    bool
                >()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.ZL] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.MM] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = true,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = true,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = true,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },

                [Enums.DocTypes.IN] = new Dictionary<Enums.DocumentItemDisplayElements, bool>()
                {
                    [Enums.DocumentItemDisplayElements.Symbol] = true,
                    [Enums.DocumentItemDisplayElements.ArticleName] = true,
                    [Enums.DocumentItemDisplayElements.Paleta] = true,
                    [Enums.DocumentItemDisplayElements.Partia] = true,
                    [Enums.DocumentItemDisplayElements.Flog] = true,
                    [Enums.DocumentItemDisplayElements.SerialNumber] = false,
                    [Enums.DocumentItemDisplayElements.ProductionDate] = false,
                    [Enums.DocumentItemDisplayElements.BestBefore] = false,
                    [Enums.DocumentItemDisplayElements.OrderedAmount] = true,
                    [Enums.DocumentItemDisplayElements.Amount] = true,
                    [Enums.DocumentItemDisplayElements.Unit] = true,
                    [Enums.DocumentItemDisplayElements.Location] = false,
                    [Enums.DocumentItemDisplayElements.Lot] = false,
                    [Enums.DocumentItemDisplayElements.Owner] = false,
                    [Enums.DocumentItemDisplayElements.InWarehouse] = false,
                    [Enums.DocumentItemDisplayElements.OnDoc] = true,
                    [Enums.DocumentItemDisplayElements.CanBeAddedToLoc] = false,
                    [Enums.DocumentItemDisplayElements.KodEAN] = true,
                    [Enums.DocumentItemDisplayElements.NrKat] = false,
                },
            };

            CreatingDocumentsRequiredFields = new Dictionary<
                Enums.DocTypes,
                Dictionary<Enums.DocumentFields, bool>
            >()
            {
                [Enums.DocTypes.PW] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.Contractor] = false,
                    [Enums.DocumentFields.TargetFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },

                [Enums.DocTypes.PZ] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.Contractor] = true,
                    [Enums.DocumentFields.TargetFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },

                [Enums.DocTypes.RW] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.Contractor] = false,
                    [Enums.DocumentFields.SourceFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },

                [Enums.DocTypes.WZ] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.Contractor] = true,
                    [Enums.DocumentFields.SourceFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },

                [Enums.DocTypes.ZL] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.SourceFlog] = false,
                    [Enums.DocumentFields.TargetFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },

                [Enums.DocTypes.MM] = new Dictionary<Enums.DocumentFields, bool>()
                {
                    [Enums.DocumentFields.Registry] = true,
                    [Enums.DocumentFields.TargetWarehouse] = true,
                    [Enums.DocumentFields.SourceFlog] = false,
                    [Enums.DocumentFields.TargetFlog] = false,
                    [Enums.DocumentFields.Description] = false,
                    [Enums.DocumentFields.RelatedDoc] = false
                },
            };
        }
    }

    public class UserSettings
    {
        public bool CanCloseApp { get; set; }
        public bool Sounds { get; set; }
        public Dictionary<Modules, bool> Modules { get; set; }
        public bool CanDeleteClosedDocuments { get; set; }
        public bool CanDeleteOwnDocuments { get; set; }
        public bool CanDeleteAllDocuments { get; set; }
        public bool CanDeleteItems { get; set; }

        //TODO
        public bool CanDeleteItemsZL { get; set; }
        public bool CanDeleteItemsOnOrders { get; set; }
        public int DisplayUnit { get; set; }
        public bool ShowDifferenceColorOnDocumentsWhenAnyPositionIsComplete { get; set; }
        public bool ShowHidenDocumentsEditingByOthers { get; set; }
        public string ColorForEditedPositionsOnDocument { get; set; }

        public UserSettings()
        {
            CanCloseApp = true;
            Sounds = true;

            Modules = new Dictionary<Enums.Modules, bool>()
            {
                [Enums.Modules.PW] = true,
                [Enums.Modules.PZ] = true,
                [Enums.Modules.RW] = true,
                [Enums.Modules.WZ] = true,
                [Enums.Modules.ZL] = true,
                [Enums.Modules.MM] = true,
                [Enums.Modules.STAN] = true,
                [Enums.Modules.IN] = true,
                [Enums.Modules.KOMPLETACJA] = true,
            };

            CanDeleteClosedDocuments = false;
            CanDeleteOwnDocuments = true;
            CanDeleteAllDocuments = false;
            CanDeleteItems = true;
            CanDeleteItemsOnOrders = false;
            ShowDifferenceColorOnDocumentsWhenAnyPositionIsComplete = false;
            ShowHidenDocumentsEditingByOthers = false;
            ColorForEditedPositionsOnDocument = "7FFFD4"; // kolor zgnila zielen ;)
            DisplayUnit = -1; // -1 = Default unit, >= 0 specified unit
        }

        public static void ShowUnitSelectionDialog(Context ctx)
        {
            try
            {
                List<JednostkaMiaryO> Units = new List<JednostkaMiaryO>();

                try
                {
                    Units = Serwer.jednostkaMiaryBL.PobierzListę();
                }
                catch (Exception ex)
                {
                    Helpers.HandleError((Activity)ctx, ex);
                    return;
                }

                Units.Add(
                    new JednostkaMiaryO()
                    {
                        ID = -1,
                        strNazwa = ctx.GetString(Resource.String.stocks_default_unit)
                    }
                );
                Units.Add(
                    new JednostkaMiaryO()
                    {
                        ID = -2,
                        strNazwa = ctx.GetString(Resource.String.stocks_base_unit)
                    }
                );

                Units = Units.OrderBy(x => x.ID).ToList();

                ActionSheetConfig Conf = new ActionSheetConfig()
                    .SetCancel(ctx.GetString(Resource.String.global_cancel))
                    .SetTitle(ctx.GetString(Resource.String.stocks_unit_mode));

                foreach (JednostkaMiaryO JM in Units)
                    Conf.Add(JM.strNazwa, () => SetUnitMode(ctx, JM.ID));

                UserDialogs.Instance.ActionSheet(Conf);
            }
            catch (Exception ex)
            {
                Helpers.HandleError((Activity)ctx, ex);
                return;
            }
        }

        private static void SetUnitMode(Context ctx, int ID)
        {
            Globalne.CurrentUserSettings.DisplayUnit = ID;

            if (ctx is BaseWMSActivity)
                (ctx as BaseWMSActivity).OnSettingsChangedAsync();
        }
    }
}
