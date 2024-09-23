using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Widget;
using G_Mobile_Android_WMS.Controls;
using G_Mobile_Android_WMS.ExtendedModel;
using Newtonsoft.Json;
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
    public class InnerSettingsActivity : BaseWMSActivity
    {
        CheckBox CheckCanCloseApp;
        CheckBox OnlyOncePalleteOnDocument;
        CheckBox DisableEditPallete;
        NumericUpDown RefreshFrequency;
        NumericUpDown Days_Back_Documents;
        CheckBox UseProdDate;
        NumericUpDown ProdDateOffset;
        CheckBox UseBestBefore;
        NumericUpDown BestBeforeOffset;
        CheckBox GetDataFromSSCC;
        CheckBox InventAutoClose;
        CheckBox CanBarcodeLogin;
        CheckBox AutoDetal;
        EditText SetRejestrForDetal;
        EditText SetContrahForDetal;

        CheckBox QuickMM;
        EditText SetRejestrForMM;
        EditText SetMagazineForMM;

        CheckBox Multipicking;
        CheckBox MultipConfirmOut;
        CheckBox MultipStatusCloseWZ;
        CheckBox MultipAutoKompletacjaAfterFinish;
        CheckBox MultipConfirmArt;
        CheckBox MultipConfirmIn;
        CheckBox MultipDocSelection;
        NumericUpDown MultipDelayBeforeClose;

        CheckBox DisableNavigationBar;
        CheckBox EnableCameraCaptureButton;
        CheckBox AllowLocationSourceChange;
        CheckBox DisableHandLocationsChange;
        CheckBox LocationPositionsSuggestedZL;

        CheckBox PositionConfirmOnlyByLocationRW;
        CheckBox PositionConfirmOnlyByLocationPW;
        CheckBox PositionConfirmOnlyByLocationZL;

        CheckBox SkipPositionConfirmPW;
        CheckBox SkipPositionConfirmRW;
        CheckBox SkipPositionConfirmPZ;
        CheckBox SkipPositionConfirmWZ;

        CheckBox BarcodeScannerOrderForce;

        NumericUpDown NumericSetForDocPW;
        NumericUpDown NumericSetForDocRW;
        NumericUpDown NumericSetForDocPZ;
        NumericUpDown NumericSetForDocWZ;
        NumericUpDown NumericSetForDocZL;
        NumericUpDown NumericSetForOrderDocPZ;
        NumericUpDown NumericSetForOrderDocRW;
        NumericUpDown NumericSetForOrderDocPW;
        NumericUpDown NumericSetForOrderDocWZ;
        NumericUpDown NumericSetForOrderDocZL;

        WMSSettings Edited;

        public enum ResultCodes
        {
            ModulesListResult = 10,
            ShowOnEditingDocumentsListResult = 20,
            ShowOnItemScreenResult = 30,
            RequiredDocItemsResult = 40,
            BarcodeSettingsResult = 50,
            RequiredDocItemItemsResult = 60,
            BarcodeOrderResult = 70,
            InstantScanListResult = 80
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_settings_inner);

            Edited = (WMSSettings)Helpers.ObjectCopy(Globalne.CurrentSettings, typeof(WMSSettings));
            GetAndSetControls();
            IsBusy = false;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(
                this,
                GetString(Resource.String.settings_inner_activity_name)
            );

            CheckCanCloseApp = FindViewById<CheckBox>(Resource.Id.settings_can_close_app_global);
            DisableEditPallete = FindViewById<CheckBox>(Resource.Id.disable_edit_sscc);
            OnlyOncePalleteOnDocument = FindViewById<CheckBox>(
                Resource.Id.only_one_SSCC_on_document
            );

            CanBarcodeLogin = FindViewById<CheckBox>(Resource.Id.settings_barcodelogin);
            InventAutoClose = FindViewById<CheckBox>(Resource.Id.settings_autoclose_one_invent);
            RefreshFrequency = FindViewById<NumericUpDown>(Resource.Id.settings_refresh_frequency);
            Days_Back_Documents = FindViewById<NumericUpDown>(
                Resource.Id.settings_days_back_to_display
            );
            UseProdDate = FindViewById<CheckBox>(Resource.Id.settings_proddate);
            ProdDateOffset = FindViewById<NumericUpDown>(Resource.Id.settings_proddate_days);
            UseBestBefore = FindViewById<CheckBox>(Resource.Id.settings_bestbeforedate);
            GetDataFromSSCC = FindViewById<CheckBox>(Resource.Id.settings_getfromsscc);
            BestBeforeOffset = FindViewById<NumericUpDown>(
                Resource.Id.settings_bestbeforedate_days
            );
            AutoDetal = FindViewById<CheckBox>(Resource.Id.settings_AutoDetal);
            SetRejestrForDetal = FindViewById<EditText>(Resource.Id.settings_SetRejestrForDetal);
            SetContrahForDetal = FindViewById<EditText>(Resource.Id.settings_SetContrahForDetal);

            BarcodeScannerOrderForce = FindViewById<CheckBox>(
                Resource.Id.settings_barcode_order_force
            );

            QuickMM = FindViewById<CheckBox>(Resource.Id.settings_QuickMM);
            SetRejestrForMM = FindViewById<EditText>(Resource.Id.settings_SetRejestrForMM);
            SetMagazineForMM = FindViewById<EditText>(Resource.Id.settings_SetMagazineForMM);

            DisableNavigationBar = FindViewById<CheckBox>(
                Resource.Id.settings_disable_navigationbar
            );
            EnableCameraCaptureButton = FindViewById<CheckBox>(
                Resource.Id.settings_enable_cameracapture
            );
            AllowLocationSourceChange = FindViewById<CheckBox>(Resource.Id.settings_locationChange);
            DisableHandLocationsChange = FindViewById<CheckBox>(
                Resource.Id.settings_disableHandLocationsChange
            );
            LocationPositionsSuggestedZL = FindViewById<CheckBox>(
                Resource.Id.settings_location_suggested_ZL
            );

            PositionConfirmOnlyByLocationPW = FindViewById<CheckBox>(Resource.Id.doc_pw);
            PositionConfirmOnlyByLocationRW = FindViewById<CheckBox>(Resource.Id.doc_rw);
            PositionConfirmOnlyByLocationZL = FindViewById<CheckBox>(Resource.Id.doc_zl);

            SkipPositionConfirmPW = FindViewById<CheckBox>(Resource.Id.tow_skip_pw);
            SkipPositionConfirmRW = FindViewById<CheckBox>(Resource.Id.tow_skip_rw);
            SkipPositionConfirmPZ = FindViewById<CheckBox>(Resource.Id.tow_skip_pz);
            SkipPositionConfirmWZ = FindViewById<CheckBox>(Resource.Id.tow_skip_wz);

            NumericSetForDocPW = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_doc_pw
            );
            NumericSetForDocRW = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_doc_rw
            );
            NumericSetForDocWZ = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_doc_wz
            );
            NumericSetForDocPZ = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_doc_pz
            );
            NumericSetForDocZL = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_doc_zl
            );

            NumericSetForOrderDocWZ = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_order_doc_wz
            );
            NumericSetForOrderDocPZ = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_order_doc_pz
            );
            NumericSetForOrderDocRW = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_order_doc_rw
            );
            NumericSetForOrderDocPW = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_order_doc_pw
            );
            NumericSetForOrderDocZL = FindViewById<NumericUpDown>(
                Resource.Id.settings_set_value_on_order_doc_zl
            );

            Multipicking = FindViewById<CheckBox>(Resource.Id.allow_multipicking);
            MultipConfirmOut = FindViewById<CheckBox>(Resource.Id.multipicking_confirmoutloc);
            MultipStatusCloseWZ = FindViewById<CheckBox>(
                Resource.Id.multipicking_set_status_close_after_finish
            );
            MultipAutoKompletacjaAfterFinish = FindViewById<CheckBox>(
                Resource.Id.multipicking_auto_kompletacja_after_finish
            );
            MultipConfirmArt = FindViewById<CheckBox>(Resource.Id.multipicking_confirmart);
            MultipConfirmIn = FindViewById<CheckBox>(Resource.Id.multipicking_confirminloc);
            MultipDelayBeforeClose = FindViewById<NumericUpDown>(
                Resource.Id.settings_multipicking_closedelay
            );
            MultipDocSelection = FindViewById<CheckBox>(
                Resource.Id.multipicking_document_selection
            );

            // Kompatybilność z 5.0
            Days_Back_Documents.Initialize();
            RefreshFrequency.Initialize();
            ProdDateOffset.Initialize();
            BestBeforeOffset.Initialize();
            MultipDelayBeforeClose.Initialize();
            NumericSetForDocPW.Initialize();
            NumericSetForDocRW.Initialize();
            NumericSetForDocPZ.Initialize();
            NumericSetForDocWZ.Initialize();
            NumericSetForDocZL.Initialize();
            NumericSetForOrderDocWZ.Initialize();
            NumericSetForOrderDocRW.Initialize();
            NumericSetForOrderDocPW.Initialize();
            NumericSetForOrderDocPZ.Initialize();
            NumericSetForOrderDocZL.Initialize();
            //  SetRejestrForDetal.Initialize();
            //  SetContrahForDetal.Initialize();

            FindViewById<Button>(Resource.Id.settings_modules).Click += Modules_Click;
            FindViewById<Button>(Resource.Id.settings_zlmode).Click += ZLMode_Click;
            FindViewById<Button>(Resource.Id.settings_mmmode).Click += MMMode_Click;
            FindViewById<Button>(Resource.Id.settings_lists).Click += ShowOnLists_Click;
            FindViewById<Button>(Resource.Id.settings_confirmscreen).Click += ShowOnConfirm_Click;
            FindViewById<Button>(Resource.Id.settings_requiredfields).Click += RequiredFields_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_exit).Click += Statuses_Exit_Click;
            FindViewById<Button>(Resource.Id.settings_codes).Click += CodeParsin_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_finish).Click +=
                Statuses_Finish_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_incomplete).Click +=
                Statuses_Incomplete_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_enter).Click += Statuses_Enter_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_pause).Click += Statuses_Pause_Click;
            FindViewById<Button>(Resource.Id.settings_statuses_done).Click += Statuses_Done_Click;
            FindViewById<Button>(Resource.Id.settings_items_requiredfields).Click +=
                RequiredFields_Items_Click;
            FindViewById<Button>(Resource.Id.settings_barcode_order).Click += BarcodeOrderClick;
            FindViewById<Button>(Resource.Id.settings_instantscan).Click +=
                InstantScanModules_Click;

            RefreshFrequency.DecimalSpaces = 0;
            Days_Back_Documents.DecimalSpaces = 0;
            Days_Back_Documents.Min = 0;
            // SetContrahForDetal =
            // SetRejestrForDetal =
            BestBeforeOffset.DecimalSpaces = 0;
            BestBeforeOffset.Min = Int16.MinValue;
            BestBeforeOffset.Max = Int16.MaxValue;
            ProdDateOffset.DecimalSpaces = 0;
            ProdDateOffset.Min = Int16.MinValue;
            ProdDateOffset.Max = Int16.MaxValue;
            MultipDelayBeforeClose.DecimalSpaces = 0;
            MultipDelayBeforeClose.Min = 0;
            MultipDelayBeforeClose.Max = 60000;

            NumericSetForDocPW.Max = 1;
            NumericSetForDocPW.Min = -1;
            NumericSetForDocPW.DecimalSpaces = 0;

            NumericSetForDocRW.Max = 1;
            NumericSetForDocRW.Min = -1;
            NumericSetForDocRW.DecimalSpaces = 0;

            NumericSetForDocWZ.Max = 1;
            NumericSetForDocWZ.Min = -1;
            NumericSetForDocWZ.DecimalSpaces = 0;

            NumericSetForDocPZ.Max = 1;
            NumericSetForDocPZ.Min = -1;
            NumericSetForDocPZ.DecimalSpaces = 0;

            NumericSetForDocZL.Max = 1;
            NumericSetForDocZL.Min = -1;
            NumericSetForDocZL.DecimalSpaces = 0;

            NumericSetForOrderDocWZ.Max = 1;
            NumericSetForOrderDocWZ.Min = -1;
            NumericSetForOrderDocWZ.DecimalSpaces = 0;

            NumericSetForOrderDocPW.Max = 1;
            NumericSetForOrderDocPW.Min = -1;
            NumericSetForOrderDocPW.DecimalSpaces = 0;

            NumericSetForOrderDocRW.Max = 1;
            NumericSetForOrderDocRW.Min = -1;
            NumericSetForOrderDocRW.DecimalSpaces = 0;

            NumericSetForOrderDocPZ.Max = 1;
            NumericSetForOrderDocPZ.Min = -1;
            NumericSetForOrderDocPZ.DecimalSpaces = 0;

            NumericSetForOrderDocZL.Max = 1;
            NumericSetForOrderDocZL.Min = -1;
            NumericSetForOrderDocZL.DecimalSpaces = 0;

            SetupBasedOnCurrentSettings();

            FindViewById<FloatingActionButton>(Resource.Id.SettingsInnerBtnPrev).Click +=
                SettingsBtnPrev_Click;
            FindViewById<FloatingActionButton>(Resource.Id.SettingsInnerBtnSave).Click +=
                SettingsBtnExport_Click;
            FindViewById<FloatingActionButton>(Resource.Id.SettingsInnerBtnDefault).Click +=
                SettingsBtnDefault_ClickAsync;
        }

        private async void BarcodeOrderClick(object sender, EventArgs e)
        {
            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (Enums.DocTypes Type in Edited.RequiredDocItemFields.Keys)
            {
                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                List<int> SettingDict = Edited.BarcodeScanningOrder[ResDict[Res]];

                if (IsSwitchingActivity)
                    return;

                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(BarcodeOrderActivity));
                i.PutExtra(BarcodeOrderActivity.Vars.Order, Helpers.SerializeJSON(SettingDict));
                i.PutExtra(BarcodeOrderActivity.Vars.DocType, (int)ResDict[Res]);

                StartActivityForResult(i, (int)ResultCodes.BarcodeOrderResult);
            }
        }

        private async void RequiredFields_Items_Click(object sender, EventArgs e)
        {
            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (Enums.DocTypes Type in Edited.RequiredDocItemFields.Keys)
            {
                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                Dictionary<string, bool> Dict = new Dictionary<string, bool>();

                Dictionary<Enums.DocumentItemFields, bool> SettingDict =
                    Edited.RequiredDocItemFields[ResDict[Res]];

                foreach (Enums.DocumentItemFields Key in SettingDict.Keys)
                    Dict[Helpers.GetEnumDescription(Key)] = SettingDict[Key];

                Helpers.OpenMultiListActivity(
                    this,
                    Res,
                    GetString(Resource.String.settings_requiredfields) + " " + Res,
                    Dict,
                    (int)ResultCodes.RequiredDocItemItemsResult
                );
                return;
            }
        }

        private void CodeParsin_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent(this, typeof(BarcodeSettingsActivity));
            i.PutExtra(
                BarcodeSettingsActivity.Vars.Settings,
                Helpers.SerializeJSON(Edited.CodeParsing)
            );

            StartActivityForResult(i, (int)ResultCodes.BarcodeSettingsResult);
        }

        private async void SettingsBtnDefault_ClickAsync(object sender, EventArgs e)
        {
            bool Resp = await Helpers.QuestionAlertAsync(
                this,
                Resource.String.settings_do_default,
                Resource.Raw.sound_message
            );

            if (Resp)
            {
                Edited = new WMSSettings();

                SetupBasedOnCurrentSettings();
            }
        }

        private async void Statuses_Pause_Click(object sender, EventArgs e)
        {
            await SelectStatus("Pause");
        }

        private async void Statuses_Done_Click(object sender, EventArgs e)
        {
            await SelectStatus("Done");
        }

        private async void Statuses_Enter_Click(object sender, EventArgs e)
        {
            await SelectStatus("Enter");
        }

        private async void Statuses_Incomplete_Click(object sender, EventArgs e)
        {
            await SelectStatus("FinishIncorrect");
        }

        private async void Statuses_Finish_Click(object sender, EventArgs e)
        {
            await SelectStatus("Finish");
        }

        private async void Statuses_Exit_Click(object sender, EventArgs e)
        {
            await SelectStatus("Leave");
        }

        private async void SettingsBtnExport_Click(object sender, EventArgs e)
        {
            bool Resp = await Helpers.QuestionAlertAsync(
                this,
                Resource.String.settings_do_export,
                Resource.Raw.sound_message
            );

            int Ret = 0;

            if (Resp)
            {
                await RunIsBusyTaskAsync(async () =>
                {
                    Helpers.ShowProgressDialog(GetString(Resource.String.global_wait));
                    Ret = await Task.Factory.StartNew(() => Export());
                    Helpers.HideProgressDialog();

                    if (Ret == 0)
                    {
                        Helpers.CenteredToast(
                            GetString(Resource.String.settings_exported),
                            ToastLength.Long
                        );
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(SettingsActivity));
                    }
                });
            }
        }

        private int Export()
        {
            try
            {
                Edited.CheckCanCloseApp = CheckCanCloseApp.Checked;
                Edited.OnlyOncePalleteSSCCOnDocument = OnlyOncePalleteOnDocument.Checked;
                Edited.DisableSSCCChange = DisableEditPallete.Checked;
                Edited.DocumentsDaysDisplayThreshhold = (int)Days_Back_Documents.Value;
                Edited.SetRejestrForDetal = (string)SetRejestrForDetal.Text;
                Edited.SetContrahForDetal = (string)SetContrahForDetal.Text;
                Edited.SetRejestrForMM = (string)SetRejestrForMM.Text;
                Edited.SetMagazineForMM = (string)SetMagazineForMM.Text;

                Edited.ModulesCheckRefreshRate = (int)RefreshFrequency.Value;
                Edited.InsertProdDate = UseProdDate.Checked;
                Edited.DaysToAddToProdDate = (int)ProdDateOffset.Value;
                Edited.InsertBestBeforeDate = UseBestBefore.Checked;
                Edited.DaysToAddToBestBeforeDate = (int)BestBeforeOffset.Value;
                Edited.GetDataFromFirstSSCCEntry = GetDataFromSSCC.Checked;
                Edited.AutoDetal = AutoDetal.Checked;
                Edited.QuickMM = QuickMM.Checked;
                Edited.CheckCanBarcodeLogin = CanBarcodeLogin.Checked;
                Edited.InventAutoClose = InventAutoClose.Checked;
                Edited.Multipicking = Multipicking.Checked;
                Edited.MultipickingSetStatusClose = MultipStatusCloseWZ.Checked;
                Edited.MultipickingAutoKompletacjaAfterFinish =
                    MultipAutoKompletacjaAfterFinish.Checked;
                Edited.AllowSourceLocationChange = AllowLocationSourceChange.Checked;
                Edited.DisableHandLocationsChange = DisableHandLocationsChange.Checked;
                Edited.LocationPositionsSuggestedZL = LocationPositionsSuggestedZL.Checked;

                Edited.BarcodeScanningOrderForce = BarcodeScannerOrderForce.Checked;

                Edited.PositionConfirmOnlyByLocationPW = PositionConfirmOnlyByLocationPW.Checked;
                Edited.PositionConfirmOnlyByLocationRW = PositionConfirmOnlyByLocationRW.Checked;
                Edited.PositionConfirmOnlyByLocationZL = PositionConfirmOnlyByLocationZL.Checked;

                Edited.SkipConfirmPositionPZ = SkipPositionConfirmPZ.Checked;
                Edited.SkipConfirmPositionWZ = SkipPositionConfirmWZ.Checked;
                Edited.SkipConfirmPositionRW = SkipPositionConfirmRW.Checked;
                Edited.SkipConfirmPositionPW = SkipPositionConfirmPW.Checked;

                Edited.DefaultValueOnDocRW = (int)NumericSetForDocRW.Value;
                Edited.DefaultValueOnDocWZ = (int)NumericSetForDocWZ.Value;
                Edited.DefaultValueOnDocPW = (int)NumericSetForDocPW.Value;
                Edited.DefaultValueOnDocPZ = (int)NumericSetForDocPZ.Value;
                Edited.DefaultValueOnDocZL = (int)NumericSetForDocZL.Value;

                Edited.DefaultValueOnOrderDocRW = (int)NumericSetForOrderDocRW.Value;
                Edited.DefaultValueOnOrderDocWZ = (int)NumericSetForOrderDocWZ.Value;
                Edited.DefaultValueOnOrderDocPW = (int)NumericSetForOrderDocPW.Value;
                Edited.DefaultValueOnOrderDocPZ = (int)NumericSetForOrderDocPZ.Value;
                Edited.DefaultValueOnOrderDocZL = (int)NumericSetForOrderDocZL.Value;

                Edited.DisableNavigationBar = DisableNavigationBar.Checked;
                Edited.EnableCameraCaptureButton = EnableCameraCaptureButton.Checked;
                Edited.MultipickingConfirmArticle = MultipConfirmArt.Checked;
                Edited.MultipickingConfirmInLocation = MultipConfirmIn.Checked;
                Edited.MultipickingConfirmOutLocation = MultipConfirmOut.Checked;
                Edited.MultipickingSelectDocuments = MultipDocSelection.Checked;
                Edited.MultipickingDelayBeforeClose = (int)MultipDelayBeforeClose.Value;

                Serwer.menuBL.ZapiszUstawienie(
                    (int)Enums.Ustawienia.KonfiguracjaAndroid,
                    Encryption.AESEncrypt(Helpers.SerializeJSON(Edited))
                );
                Globalne.CurrentSettings = Edited;

                return 0;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return -1;
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                if (requestCode == (int)ResultCodes.BarcodeSettingsResult)
                {
                    Dictionary<string, string> RDict =
                        (Dictionary<string, string>)
                            Helpers.DeserializePassedJSON(
                                data,
                                BarcodeSettingsActivity.Results.Settings,
                                typeof(Dictionary<string, string>)
                            );
                    Edited.CodeParsing = RDict;
                }
                else if (requestCode == (int)ResultCodes.BarcodeOrderResult)
                {
                    List<int> Dict =
                        (List<int>)
                            Helpers.DeserializePassedJSON(
                                data,
                                BarcodeOrderActivity.Results.Order,
                                typeof(List<int>)
                            );
                    Enums.DocTypes DocType = (Enums.DocTypes)
                        data.GetIntExtra(
                            BarcodeOrderActivity.Results.DocType,
                            (int)Enums.DocTypes.Error
                        );
                    Edited.BarcodeScanningOrder[DocType] = Dict;
                }
                else
                {
                    Dictionary<string, bool> Dict =
                        (Dictionary<string, bool>)
                            Helpers.DeserializePassedJSON(
                                data,
                                MultiSelectListActivity.Results.CheckedItems,
                                typeof(Dictionary<string, bool>)
                            );

                    Dictionary<string, Enums.DocTypes> DocTypesDict =
                        new Dictionary<string, Enums.DocTypes>();

                    foreach (Enums.DocTypes d in Enum.GetValues(typeof(Enums.DocTypes)))
                        DocTypesDict[Helpers.GetEnumDescription(d)] = d;

                    string Var = data.GetStringExtra(MultiSelectListActivity.Results.Variable);
                    Enums.DocTypes SelectedDocType = Enums.DocTypes.Error;

                    if (Var != null)
                    {
                        if (DocTypesDict.ContainsKey(Var))
                            SelectedDocType = DocTypesDict[Var];
                    }

                    switch (requestCode)
                    {
                        case (int)ResultCodes.InstantScanListResult:
                        {
                            Dictionary<string, Enums.DocTypes> ModulesDict =
                                new Dictionary<string, Enums.DocTypes>();

                            foreach (Enums.DocTypes Key in Edited.Modules.Keys)
                                ModulesDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (ModulesDict.ContainsKey(Key))
                                    Edited.InstantScanning[ModulesDict[Key]] = Dict[Key];
                            }

                            break;
                        }
                        case (int)ResultCodes.ModulesListResult:
                        {
                            Dictionary<string, Enums.Modules> ModulesDict =
                                new Dictionary<string, Enums.Modules>();

                            foreach (Enums.Modules Key in Edited.Modules.Keys)
                                ModulesDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (ModulesDict.ContainsKey(Key))
                                    Edited.Modules[ModulesDict[Key]] = Dict[Key];
                            }

                            break;
                        }
                        case (int)ResultCodes.RequiredDocItemsResult:
                        {
                            Dictionary<string, Enums.DocumentFields> RespDict =
                                new Dictionary<string, Enums.DocumentFields>();

                            foreach (
                                Enums.DocumentFields Key in Edited
                                    .CreatingDocumentsRequiredFields[SelectedDocType]
                                    .Keys
                            )
                                RespDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (RespDict.ContainsKey(Key))
                                    Edited.CreatingDocumentsRequiredFields[SelectedDocType][
                                        RespDict[Key]
                                    ] = Dict[Key];
                            }

                            break;
                        }
                        case (int)ResultCodes.RequiredDocItemItemsResult:
                        {
                            Dictionary<string, Enums.DocumentItemFields> RespDict =
                                new Dictionary<string, Enums.DocumentItemFields>();

                            foreach (
                                Enums.DocumentItemFields Key in Edited
                                    .RequiredDocItemFields[SelectedDocType]
                                    .Keys
                            )
                                RespDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (RespDict.ContainsKey(Key))
                                    Edited.RequiredDocItemFields[SelectedDocType][RespDict[Key]] =
                                        Dict[Key];
                            }

                            break;
                        }
                        case (int)ResultCodes.ShowOnEditingDocumentsListResult:
                        {
                            Dictionary<string, Enums.EditingDocumentsListDisplayElements> RespDict =
                                new Dictionary<string, Enums.EditingDocumentsListDisplayElements>();

                            foreach (
                                Enums.EditingDocumentsListDisplayElements Key in Edited
                                    .EditingDocumentsListDisplayElementsListsINNNR[SelectedDocType]
                                    .Keys
                            )
                                RespDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (RespDict.ContainsKey(Key))
                                    Edited.EditingDocumentsListDisplayElementsListsINNNR[
                                        SelectedDocType
                                    ][RespDict[Key]] = Dict[Key];
                            }

                            break;
                        }
                        case (int)ResultCodes.ShowOnItemScreenResult:
                        {
                            Dictionary<string, Enums.DocumentItemDisplayElements> RespDict =
                                new Dictionary<string, Enums.DocumentItemDisplayElements>();

                            foreach (
                                Enums.DocumentItemDisplayElements Key in Edited
                                    .EditingDocumentItemDisplayElementsListsKAT[SelectedDocType]
                                    .Keys
                            )
                                RespDict[Helpers.GetEnumDescription(Key)] = Key;

                            foreach (string Key in Dict.Keys)
                            {
                                if (RespDict.ContainsKey(Key))
                                    Edited.EditingDocumentItemDisplayElementsListsKAT[
                                        SelectedDocType
                                    ][RespDict[Key]] = Dict[Key];
                            }

                            break;
                        }
                    }
                }
            }
        }

        private async Task SelectStatus(string DictType)
        {
            Dictionary<Enums.DocTypes, int> DictToUse = null;

            switch (DictType)
            {
                case "Enter":
                    DictToUse = Edited.StatusesToSetOnDocumentEnter;
                    break;
                case "Leave":
                    DictToUse = Edited.StatusesToSetOnDocumentLeave;
                    break;
                case "Pause":
                    DictToUse = Edited.StatusesToSetOnDocumentPause;
                    break;
                case "Finish":
                    DictToUse = Edited.StatusesToSetOnDocumentPause;
                    break;
                case "Done":
                    DictToUse = Edited.StatusesToSetOnDocumentDone;
                    break;
                case "FinishIncorrect":
                    DictToUse = Edited.StatusesToSetOnDocumentFinishIncorrect;
                    break;
            }

            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (Enums.DocTypes Type in DictToUse.Keys)
            {
                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                try
                {
                    List<StatusDokumentuO> Statusy =
                        Serwer.dokumentBL.PobierzListęStatusówDokumentów(
                            ResDict[Res].ToString().Substring(0, 2)
                        );
                    Statusy.Add(
                        new StatusDokumentuO()
                        {
                            strNazwaStatusu = GetString(Resource.String.global_default),
                            ID = -1
                        }
                    );

                    StatusDokumentuO Obecny = Statusy.Find(x => x.ID == DictToUse[ResDict[Res]]);

                    if (Obecny != null)
                        Obecny.strNazwaStatusu = ">> " + Obecny.strNazwaStatusu + " <<";

                    Statusy = Statusy.OrderBy(x => x.ID).ThenBy(x => x.intPoziomStatusu).ToList();

                    string Res2 = await UserDialogs.Instance.ActionSheetAsync(
                        GetString(Resource.String.settings_select_doc_status),
                        GetString(Resource.String.global_cancel),
                        "",
                        null,
                        Statusy.Select(x => x.strNazwaStatusu).ToArray()
                    );

                    if (Res2 == GetString(Resource.String.global_cancel))
                        return;
                    else
                    {
                        StatusDokumentuO Status = Statusy.Find(x => x.strNazwaStatusu == Res2);
                        DictToUse[ResDict[Res]] = Status.ID;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            }
        }

        private async void RequiredFields_Click(object sender, EventArgs e)
        {
            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (Enums.DocTypes Type in Edited.CreatingDocumentsRequiredFields.Keys)
            {
                if (
                    Type.ToString().Contains("Gathering")
                    || Type.ToString().Contains("Distribution")
                )
                    continue;

                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                Dictionary<string, bool> Dict = new Dictionary<string, bool>();

                Dictionary<Enums.DocumentFields, bool> SettingDict =
                    Edited.CreatingDocumentsRequiredFields[ResDict[Res]];

                foreach (Enums.DocumentFields Key in SettingDict.Keys)
                    Dict[Helpers.GetEnumDescription(Key)] = SettingDict[Key];

                Helpers.OpenMultiListActivity(
                    this,
                    Res,
                    GetString(Resource.String.settings_requiredfields) + " " + Res,
                    Dict,
                    (int)ResultCodes.RequiredDocItemsResult
                );
                return;
            }
        }

        private async void ShowOnConfirm_Click(object sender, EventArgs e)
        {
            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (Enums.DocTypes Type in Edited.EditingDocumentItemDisplayElementsListsKAT.Keys)
            {
                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                Dictionary<string, bool> Dict = new Dictionary<string, bool>();
                Dictionary<Enums.DocumentItemDisplayElements, bool> SettingDict =
                    Edited.EditingDocumentItemDisplayElementsListsKAT[ResDict[Res]];

                foreach (Enums.DocumentItemDisplayElements Key in SettingDict.Keys)
                    Dict[Helpers.GetEnumDescription(Key)] = SettingDict[Key];

                Helpers.OpenMultiListActivity(
                    this,
                    Res,
                    GetString(Resource.String.settings_shown_elements) + " " + Res,
                    Dict,
                    (int)ResultCodes.ShowOnItemScreenResult
                );
                return;
            }
        }

        private async void ShowOnLists_Click(object sender, EventArgs e)
        {
            List<string> Types = new List<string>();
            Dictionary<string, Enums.DocTypes> ResDict = new Dictionary<string, Enums.DocTypes>();

            foreach (
                Enums.DocTypes Type in Edited.EditingDocumentsListDisplayElementsListsINNNR.Keys
            )
            {
                string Description = Helpers.GetEnumDescription(Type);

                Types.Add(Description);
                ResDict[Description] = Type;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_select_module),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Types.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                Dictionary<string, bool> Dict = new Dictionary<string, bool>();
                Dictionary<Enums.EditingDocumentsListDisplayElements, bool> SettingDict =
                    Edited.EditingDocumentsListDisplayElementsListsINNNR[ResDict[Res]];

                foreach (Enums.EditingDocumentsListDisplayElements Key in SettingDict.Keys)
                    Dict[Helpers.GetEnumDescription(Key)] = SettingDict[Key];

                Helpers.OpenMultiListActivity(
                    this,
                    Res,
                    GetString(Resource.String.settings_shown_elements) + " " + Res,
                    Dict,
                    (int)ResultCodes.ShowOnEditingDocumentsListResult
                );
                return;
            }
        }

        private async void ZLMode_Click(object sender, EventArgs e)
        {
            List<string> Options = new List<string>();
            Dictionary<string, Enums.ZLMMMode> ResDict = new Dictionary<string, Enums.ZLMMMode>();

            foreach (Enums.ZLMMMode m in Enum.GetValues(typeof(Enums.ZLMMMode)))
            {
                string Description = Helpers.GetEnumDescription(m);

                if (Edited.DefaultZLMode == m)
                    Description = ">> " + Description + "<< ";

                Options.Add(Description);
                ResDict[Description] = m;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_zlmode),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Options.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
                Edited.DefaultZLMode = ResDict[Res];
        }

        private async void MMMode_Click(object sender, EventArgs e)
        {
            List<string> Options = new List<string>();
            Dictionary<string, Enums.ZLMMMode> ResDict = new Dictionary<string, Enums.ZLMMMode>();

            foreach (Enums.ZLMMMode m in Enum.GetValues(typeof(Enums.ZLMMMode)))
            {
                string Description = Helpers.GetEnumDescription(m);

                if (Edited.DefaultZLMode == m)
                    Description = ">> " + Description + "<< ";

                Options.Add(Description);
                ResDict[Description] = m;
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.settings_mmmode),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Options.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
                Edited.DefaultMMMode = ResDict[Res];
        }

        private void Modules_Click(object sender, EventArgs e)
        {
            Dictionary<string, bool> Dict = new Dictionary<string, bool>();

            foreach (Enums.Modules Key in Edited.Modules.Keys)
                Dict[Helpers.GetEnumDescription(Key)] = Edited.Modules[Key];

            Helpers.OpenMultiListActivity(
                this,
                "",
                GetString(Resource.String.settings_modules),
                Dict,
                (int)ResultCodes.ModulesListResult
            );
        }

        private void InstantScanModules_Click(object sender, EventArgs e)
        {
            Dictionary<string, bool> Dict = new Dictionary<string, bool>();

            foreach (Enums.DocTypes Key in Edited.InstantScanning.Keys)
                Dict[Helpers.GetEnumDescription(Key)] = Edited.InstantScanning[Key];

            Helpers.OpenMultiListActivity(
                this,
                "",
                GetString(Resource.String.settings_instantscan),
                Dict,
                (int)ResultCodes.InstantScanListResult
            );
        }

        private void SetupBasedOnCurrentSettings()
        {
            CheckCanCloseApp.Checked = Edited.CheckCanCloseApp;
            DisableEditPallete.Checked = Edited.DisableSSCCChange;
            OnlyOncePalleteOnDocument.Checked = Edited.OnlyOncePalleteSSCCOnDocument;

            RefreshFrequency.Value = Edited.ModulesCheckRefreshRate;
            Days_Back_Documents.Value = Edited.DocumentsDaysDisplayThreshhold;
            UseProdDate.Checked = Edited.InsertProdDate;
            ProdDateOffset.Value = Edited.DaysToAddToProdDate;
            UseBestBefore.Checked = Edited.InsertBestBeforeDate;
            BestBeforeOffset.Value = Edited.DaysToAddToBestBeforeDate;
            GetDataFromSSCC.Checked = Edited.GetDataFromFirstSSCCEntry;
            AutoDetal.Checked = Edited.AutoDetal;
            SetRejestrForDetal.Text = Edited.SetRejestrForDetal;
            SetContrahForDetal.Text = Edited.SetContrahForDetal;

            BarcodeScannerOrderForce.Checked = Edited.BarcodeScanningOrderForce;

            QuickMM.Checked = Edited.QuickMM;
            SetRejestrForMM.Text = Edited.SetRejestrForMM;
            SetMagazineForMM.Text = Edited.SetMagazineForMM;

            PositionConfirmOnlyByLocationPW.Checked = Edited.PositionConfirmOnlyByLocationPW;
            PositionConfirmOnlyByLocationRW.Checked = Edited.PositionConfirmOnlyByLocationRW;
            PositionConfirmOnlyByLocationZL.Checked = Edited.PositionConfirmOnlyByLocationZL;

            NumericSetForOrderDocRW.Value = Edited.DefaultValueOnOrderDocRW;
            NumericSetForOrderDocPW.Value = Edited.DefaultValueOnOrderDocPW;
            NumericSetForOrderDocWZ.Value = Edited.DefaultValueOnOrderDocWZ;
            NumericSetForOrderDocPZ.Value = Edited.DefaultValueOnOrderDocPZ;
            NumericSetForOrderDocZL.Value = Edited.DefaultValueOnOrderDocZL;

            NumericSetForDocRW.Value = Edited.DefaultValueOnDocRW;
            NumericSetForDocPW.Value = Edited.DefaultValueOnDocPW;
            NumericSetForDocWZ.Value = Edited.DefaultValueOnDocWZ;
            NumericSetForDocPZ.Value = Edited.DefaultValueOnDocPZ;
            NumericSetForDocZL.Value = Edited.DefaultValueOnDocZL;

            DisableNavigationBar.Checked = Edited.DisableNavigationBar;
            EnableCameraCaptureButton.Checked = Edited.EnableCameraCaptureButton;
            AllowLocationSourceChange.Checked = Edited.AllowSourceLocationChange;
            DisableHandLocationsChange.Checked = Edited.DisableHandLocationsChange;
            LocationPositionsSuggestedZL.Checked = Edited.LocationPositionsSuggestedZL;

            InventAutoClose.Checked = Edited.InventAutoClose;
            CanBarcodeLogin.Checked = Edited.CheckCanBarcodeLogin;
            Multipicking.Checked = Edited.Multipicking;
            MultipAutoKompletacjaAfterFinish.Checked =
                Edited.MultipickingAutoKompletacjaAfterFinish;
            MultipConfirmArt.Checked = Edited.MultipickingConfirmArticle;
            MultipStatusCloseWZ.Checked = Edited.MultipickingSetStatusClose;
            MultipConfirmIn.Checked = Edited.MultipickingConfirmInLocation;
            MultipConfirmOut.Checked = Edited.MultipickingConfirmOutLocation;
            MultipDocSelection.Checked = Edited.MultipickingSelectDocuments;
            MultipDelayBeforeClose.Value = Edited.MultipickingDelayBeforeClose;

            //OneInsteadOfSet.Checked = Edited.OneInsteadOfSetAmount;
        }

        private void SettingsBtnPrev_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(
                () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(SettingsActivity))
            );
        }
    }
}
