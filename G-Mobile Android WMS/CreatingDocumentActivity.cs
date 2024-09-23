using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false
    )]
    public class CreatingDocumentActivity : ActivityWithScanner
    {
        FloatingActionButton OKButton = null;
        FloatingActionButton CancelButton = null;
        FloatingActionButton DETALButton = null;
        FloatingActionButton QuickMM = null;

        TextView Registry;
        TextView TargetWarehouse;
        TextView Contractor;
        TextView SourceLogisticFunction;
        TextView TargetLogisticFunction;

        FloatingActionButton TargetWarehouseButton;
        FloatingActionButton ContractorButton;
        FloatingActionButton SourceLogisticFunctionButton;
        FloatingActionButton TargetLogisticFunctionButton;
        FloatingActionButton RegistryButton;
        FloatingActionButton DodajTekstOpis;

        LinearLayout RegistryView;
        LinearLayout TargetWarehouseView;
        LinearLayout ContractorView;
        LinearLayout SourceFlogView;
        LinearLayout TargetFlogView;
        LinearLayout DescriptionView;
        LinearLayout RelatedDocumentView;

        EditText Description;
        EditText RelatedDocument;

        Enums.DocTypes DocType;

        internal static class Vars
        {
            public const string DocType = "DocType";
        }

        public enum ResultCodes
        {
            WarehouseActivityResult = 10,
            ContractorsActivityResult = 20,
        }

        internal static class Results
        {
            public const string CreatedDocumentID = "CreatedDocumentID";
            public const string ZLMMMode = "ZLMMMode";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_creatingdocument);

            DocType = (Enums.DocTypes)Intent.GetIntExtra(Vars.DocType, (int)Enums.DocTypes.Error);

            GetAndSetControls();
            IsBusy = false;
        }

        private void GetAndSetControls()
        {
            try
            {
                Helpers.SetActivityHeader(
                    this,
                    GetString(Resource.String.creating_documents_activity_name)
                );

                Registry = FindViewById<TextView>(Resource.Id.creating_documents_registry);
                TargetWarehouse = FindViewById<TextView>(Resource.Id.creating_documents_warehouse);
                Contractor = FindViewById<TextView>(Resource.Id.creating_documents_contractor);
                SourceLogisticFunction = FindViewById<TextView>(
                    Resource.Id.creating_documents_funclog_src
                );
                TargetLogisticFunction = FindViewById<TextView>(
                    Resource.Id.creating_documents_funclog_tar
                );

                TargetWarehouseButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_warehouse
                );
                ContractorButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_contractor
                );
                SourceLogisticFunctionButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_funclog_src
                );
                TargetLogisticFunctionButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_funclog_tar
                );

                DodajTekstOpis = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_dodajtekst
                );

                CancelButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_cancel
                );
                OKButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_ok
                );
                RegistryButton = FindViewById<FloatingActionButton>(
                    Resource.Id.creating_documents_btn_registry
                );

                if (Globalne.CurrentSettings.AutoDetal is true)
                {
                    DETALButton = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_DETAL
                    );
                    DETALButton.Hide();
                }
                if (Globalne.CurrentSettings.AutoDetal is false)
                {
                    DETALButton = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_DETAL
                    );
                    DETALButton.SetBackgroundColor(Android.Graphics.Color.Gray);
                    DETALButton.Hide();
                }
                if (Globalne.CurrentSettings.AutoDetal is true && DocType == Enums.DocTypes.WZ)
                {
                    DETALButton = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_DETAL
                    );
                    DETALButton.Show();
                }
                //
                if (Globalne.CurrentSettings.QuickMM is true)
                {
                    QuickMM = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_QuickMM
                    );
                    QuickMM.Hide();
                }
                if (Globalne.CurrentSettings.QuickMM is false)
                {
                    QuickMM = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_QuickMM
                    );
                    QuickMM.SetBackgroundColor(Android.Graphics.Color.Gray);
                    QuickMM.Hide();
                }
                if (Globalne.CurrentSettings.QuickMM is true && DocType == Enums.DocTypes.MM)
                {
                    QuickMM = FindViewById<FloatingActionButton>(
                        Resource.Id.creating_documents_btn_QuickMM
                    );
                    QuickMM.Show();
                }

                Description = FindViewById<EditText>(Resource.Id.creating_documents_description);
                RelatedDocument = FindViewById<EditText>(Resource.Id.creating_documents_docrelated);

                RegistryView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_registry
                );
                TargetWarehouseView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_warehouse
                );
                ContractorView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_contractor
                );
                SourceFlogView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_funclog_src
                );
                TargetFlogView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_funclog_tar
                );
                DescriptionView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_description
                );
                RelatedDocumentView = FindViewById<LinearLayout>(
                    Resource.Id.creating_documents_layout_docrelated
                );

                CancelButton.Click += CancelButton_Click;
                TargetWarehouseButton.Click += TargetWarehouseButton_Click;
                ContractorButton.Click += ContractorButton_Click;
                SourceLogisticFunctionButton.Click += SourceLogisticFunction_Click;
                TargetLogisticFunctionButton.Click += TargetLogisticFunction_Click;
                RegistryButton.Click += RegistryButton_Click;
                OKButton.Click += OKButton_Click;

                DodajTekstOpis.Click += DodajTekstOpis_Click;

                DETALButton.Click += DETALButton_Click;
                QuickMM.Click += QuickMM_Click;
                Description.Click += OnClickTargettableTextView;
                Description.FocusChange += OnFocusChangeTargettableTextView;

                RelatedDocument.Click += OnClickTargettableTextView;
                RelatedDocument.FocusChange += OnFocusChangeTargettableTextView;

                TargetWarehouse.Tag = -1;
                Contractor.Tag = -1;
                SourceLogisticFunction.Tag = -1;
                TargetLogisticFunction.Tag = -1;
                Registry.Tag = -1;

                SetViewBasedOnDocTypeSettings();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        async void OKButton_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(async () =>
            {
                try
                {
                    if (!CheckIfFieldsFilled())
                    {
                        Helpers.PlaySound(this, Resource.Raw.sound_error);
                        Helpers.CenteredToast(
                            GetString(Resource.String.creating_documents_not_complete),
                            ToastLength.Long
                        );
                        return;
                    }
                    else
                    {
                        Enums.ZLMMMode ZLMMMode = Globalne.CurrentSettings.DefaultZLMode;

                        if (
                            (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.ZL)
                            && ZLMMMode == Enums.ZLMMMode.None
                        )
                        {
                            ZLMMMode = await BusinessLogicHelpers.Documents.AskZLMMMode(
                                this,
                                DocType
                            );
                            if (ZLMMMode == Enums.ZLMMMode.None)
                                return;
                        }

                        Helpers.ShowProgressDialog(
                            GetString(Resource.String.creating_documents_progress)
                        );

                        int Added = await Task.Run(() =>
                        {
                            int Added = BusinessLogicHelpers.Documents.CreateDocument(
                                this,
                                DocType,
                                (int)Registry.Tag,
                                (int)Contractor.Tag,
                                (int)SourceLogisticFunction.Tag,
                                (int)TargetLogisticFunction.Tag,
                                (int)TargetWarehouse.Tag,
                                -1,
                                -1,
                                RelatedDocument.Text,
                                Description.Text
                            );

                            return Added;
                        });

                        Helpers.HideProgressDialog();

                        BusinessLogicHelpers.Documents.EditDocuments(
                            this,
                            new List<int> { Added },
                            DocType,
                            ZLMMMode
                        );
                    }
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            });
        }

        private bool CheckIfFieldsFilled()
        {
            foreach (
                KeyValuePair<Enums.DocumentFields, bool> Field in Globalne
                    .CurrentSettings.CreatingDocumentsRequiredFields[DocType]
                    .Where(x => x.Value == true)
                    .ToList()
            )
            {
                switch (Field.Key)
                {
                    case Enums.DocumentFields.Contractor:
                    {
                        if ((int)Contractor.Tag == -1)
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.TargetWarehouse:
                    {
                        if ((int)TargetWarehouse.Tag == -1)
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.Registry:
                    {
                        if ((int)Registry.Tag == -1)
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.SourceFlog:
                    {
                        if ((int)SourceLogisticFunction.Tag == -1)
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.TargetFlog:
                    {
                        if ((int)TargetLogisticFunction.Tag == -1)
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.RelatedDoc:
                    {
                        if (RelatedDocument.Text == "")
                            return false;
                        else
                            continue;
                    }
                    case Enums.DocumentFields.Description:
                    {
                        if (Description.Text == "")
                            return false;
                        else
                            continue;
                    }
                }
            }

            return true;
        }

        private void SetViewBasedOnDocTypeSettings()
        {
            Dictionary<Enums.DocumentFields, bool> Settings = Globalne
                .CurrentSettings
                .CreatingDocumentsRequiredFields[DocType];

            Android.Graphics.Color RequiredTextField = new Android.Graphics.Color(
                ContextCompat.GetColor(this, Resource.Color.required_text_field)
            );

            if (Settings.ContainsKey(Enums.DocumentFields.Registry))
            {
                if (Settings[Enums.DocumentFields.Registry] == true)
                    Registry.SetBackgroundColor(RequiredTextField);

                try
                {
                    List<RejestrRow> Rejestry =
                        Serwer.rejestrBL.PobierzListęRejestrówDostępnychDlaOperatoraNaTerminalu(
                            Helpers.StringDocType(DocType),
                            Globalne.Magazyn.ID,
                            Globalne.Operator.ID
                        );

                    if (Rejestry.Count == 1)
                    {
                        Helpers.SetTextOnTextView(this, Registry, Rejestry[0].strNazwaRej);
                        Registry.Tag = Rejestry[0].ID;
                    }
                }
                catch (Exception) { }
            }
            else
                RegistryView.Visibility = ViewStates.Gone;

            if (Settings.ContainsKey(Enums.DocumentFields.TargetWarehouse))
            {
                if (Settings[Enums.DocumentFields.TargetWarehouse] == true)
                    TargetWarehouse.SetBackgroundColor(RequiredTextField);

                try
                {
                    List<MagazynO> Magazyny = Serwer.magazynBL.PobierzListęDostępnychDlaOperatora(
                        Globalne.Operator.ID
                    );

                    MagazynO Skipped = Magazyny.Find(x => x.ID == Globalne.Magazyn.ID);

                    if (Skipped != null)
                        Magazyny.Remove(Skipped);

                    if (Magazyny.Count == 1)
                    {
                        Helpers.SetTextOnTextView(this, TargetWarehouse, Magazyny[0].Nazwa);
                        TargetWarehouse.Tag = Magazyny[0].ID;
                    }
                }
                catch (Exception) { }
            }
            else
                TargetWarehouseView.Visibility = ViewStates.Gone;

            if (Settings.ContainsKey(Enums.DocumentFields.Contractor))
            {
                if (Settings[Enums.DocumentFields.Contractor] == true)
                    Contractor.SetBackgroundColor(RequiredTextField);
            }
            else
                ContractorView.Visibility = ViewStates.Gone;

            if (
                Settings.ContainsKey(Enums.DocumentFields.SourceFlog)
                && Globalne.CurrentSettings.FunkcjeLogistyczne
            )
            {
                if (Settings[Enums.DocumentFields.SourceFlog] == true)
                    SourceLogisticFunction.SetBackgroundColor(RequiredTextField);
            }
            else
                SourceFlogView.Visibility = ViewStates.Gone;

            if (
                Settings.ContainsKey(Enums.DocumentFields.TargetFlog)
                && Globalne.CurrentSettings.FunkcjeLogistyczne
            )
            {
                if (Settings[Enums.DocumentFields.TargetFlog] == true)
                    TargetLogisticFunction.SetBackgroundColor(RequiredTextField);
            }
            else
                TargetFlogView.Visibility = ViewStates.Gone;

            if (Settings.ContainsKey(Enums.DocumentFields.RelatedDoc))
            {
                if (Settings[Enums.DocumentFields.RelatedDoc] == true)
                    RelatedDocument.SetBackgroundColor(RequiredTextField);
            }
            else
                RelatedDocumentView.Visibility = ViewStates.Gone;

            if (Settings.ContainsKey(Enums.DocumentFields.Description))
            {
                if (Settings[Enums.DocumentFields.Description] == true)
                    Description.SetBackgroundColor(RequiredTextField);
            }
            else
                DescriptionView.Visibility = ViewStates.Gone;
        }

        async void RegistryButton_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowRegistryListAndSet(
                        this,
                        DocType,
                        Globalne.Magazyn.ID,
                        Registry
                    )
            );
        }

        async void TargetLogisticFunction_Click(object sender, EventArgs e)
        {
            int WarehouseID = Globalne.Magazyn.ID;

            if (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.MMGathering)
            {
                WarehouseID = Convert.ToInt32(TargetWarehouse.Tag);

                if (WarehouseID < 0)
                {
                    Helpers.CenteredToast(
                        GetString(Resource.String.creating_documents_target_warehouse_not_set),
                        ToastLength.Long
                    );
                    return;
                }
            }

            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                        this,
                        WarehouseID,
                        TargetLogisticFunction
                    )
            );
        }

        async void SourceLogisticFunction_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(
                () =>
                    BusinessLogicHelpers.Indexes.ShowLogisticFunctionsListAndSet(
                        this,
                        Globalne.Magazyn.ID,
                        SourceLogisticFunction
                    )
            );
        }

        async void DodajTekstOpis_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                ActionSheetConfig Conf1 = new ActionSheetConfig()
                    .SetCancel(GetString(Resource.String.global_cancel))
                    .SetTitle(GetString(Resource.String.stocks_search_by_what));

                Conf1.Add(
                    GetString(Resource.String.Dopotwierdzenia),
                    () => Helpers.SetTextOnTextView(this, Description, "Do Potwierdzenia")
                );
                Conf1.Add(
                    GetString(Resource.String.Uszkodzenie),
                    () => Helpers.SetTextOnTextView(this, Description, "Uszkodzenie")
                );
                Conf1.Add(
                    GetString(Resource.String.Brakfaktury),
                    () => Helpers.SetTextOnTextView(this, Description, "Brak Faktury")
                );
                Conf1.Add(
                    GetString(Resource.String.Brakitowarowe),
                    () => Helpers.SetTextOnTextView(this, Description, "Braki Towarowe")
                );
                Conf1.Add(
                    GetString(Resource.String.Dostawaniekompletna),
                    () => Helpers.SetTextOnTextView(this, Description, "Dostawa niekompletna")
                );

                UserDialogs.Instance.ActionSheet(Conf1);
            });
        }

        private void ContractorButton_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(ContractorsActivity));
                i.PutExtra(ContractorsActivity.Vars.AskOnStart, true);

                StartActivityForResult(i, (int)ResultCodes.ContractorsActivityResult);
            });
        }

        private async void QuickMM_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(async () =>
            {
                if (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.MM)
                {
                    try
                    {
                        string strERP = Globalne.CurrentSettings.SetRejestrForMM;
                        RejestrVO rejestr = Serwer.rejestrBL.PobierzRejestrWgSymbolu(strERP);

                        Helpers.SetTextOnTextView(this, Registry, rejestr.strNazwaRej);
                        Registry.Tag = rejestr.ID;
                    }
                    catch (Exception ex) { }

                    try
                    {
                        string NazwaMagazynu = Globalne.CurrentSettings.SetMagazineForMM;
                        MagazynO mag = Serwer.magazynBL.PobierzMagazyn(NazwaMagazynu);
                        if (Globalne.Magazyn.Nazwa != NazwaMagazynu)
                        {
                            Helpers.SetTextOnTextView(this, TargetWarehouse, mag.Nazwa);

                            TargetWarehouse.Tag = mag.ID;
                        }
                    }
                    catch (Exception ex) { }
                }
            });
            if (!CheckIfFieldsFilled())
            {
                Helpers.PlaySound(this, Resource.Raw.sound_error);
                Helpers.CenteredToast(
                    GetString(Resource.String.creating_documents_not_complete),
                    ToastLength.Long
                );
                return;
            }
            else
            {
                await RunIsBusyTaskAsync(async () =>
                {
                    try
                    {
                        if (!CheckIfFieldsFilled())
                        {
                            Helpers.PlaySound(this, Resource.Raw.sound_error);
                            Helpers.CenteredToast(
                                GetString(Resource.String.creating_documents_not_complete),
                                ToastLength.Long
                            );
                            return;
                        }
                        else
                        {
                            Enums.ZLMMMode ZLMMMode = Enums.ZLMMMode.OneStep;
                            if (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.ZL) { }
                            Helpers.ShowProgressDialog(
                                GetString(Resource.String.creating_documents_progress)
                            );
                            int Added = await Task.Run(() =>
                            {
                                int Added = BusinessLogicHelpers.Documents.CreateDocument(
                                    this,
                                    DocType,
                                    (int)Registry.Tag,
                                    (int)Contractor.Tag,
                                    (int)SourceLogisticFunction.Tag,
                                    (int)TargetLogisticFunction.Tag,
                                    (int)TargetWarehouse.Tag,
                                    -1,
                                    -1,
                                    RelatedDocument.Text,
                                    Description.Text
                                );

                                return Added;
                            });
                            Helpers.HideProgressDialog();
                            BusinessLogicHelpers.Documents.EditDocuments(
                                this,
                                new List<int> { Added },
                                DocType,
                                ZLMMMode
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        Helpers.HandleError(this, ex);
                        return;
                    }
                });
            }
        }

        private void DETALButton_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                if (DocType == Enums.DocTypes.WZ || DocType == Enums.DocTypes.RW)
                {
                    try
                    {
                        //      List<RejestrRow> Rejestry = Serwer.rejestrBL.PobierzListęRejestrówDostępnychDlaOperatoraNaTerminalu(Helpers.StringDocType(DocType), Globalne.Magazyn.ID, Globalne.Operator.ID);
                        string strERP = Globalne.CurrentSettings.SetRejestrForDetal;
                        RejestrVO rejestr = Serwer.rejestrBL.PobierzRejestrWgSymbolu(strERP);

                        Helpers.SetTextOnTextView(this, Registry, rejestr.strNazwaRej);
                        //  Helpers.SetTextOnTextView(this, Registry, Rejestry[0].strNazwaRej);
                        Registry.Tag = rejestr.ID;
                    }
                    catch (Exception ex) { }

                    try
                    {
                        string NazwaKontrahenta = Globalne.CurrentSettings.SetContrahForDetal;
                        KontrahentVO Ktr = Serwer.podmiotBL.PobierzKontrahentaWgNazwy(
                            NazwaKontrahenta
                        );

                        if (Ktr.bAktywny && !Ktr.bZablokowany)
                        {
                            Helpers.SetTextOnTextView(this, Contractor, Ktr.strNazwa);

                            Contractor.Tag = Ktr.ID;
                        }
                    }
                    catch (Exception ex) { }
                }
            });
        }

        private void TargetWarehouseButton_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(WarehousesActivity));
                i.PutExtra(WarehousesActivity.Vars.Mode, (int)WarehousesActivity.Modes.Target);

                if (
                    DocType == Enums.DocTypes.MM
                    || DocType == Enums.DocTypes.MMDistribution
                    || DocType == Enums.DocTypes.MMGathering
                )
                    i.PutExtra(WarehousesActivity.Vars.SkipWarehouse, Globalne.Magazyn.ID);

                StartActivityForResult(i, (int)ResultCodes.WarehouseActivityResult);
            });
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
                switch (requestCode)
                {
                    case (int)ResultCodes.WarehouseActivityResult:
                    {
                        MagazynO Selected = (MagazynO)
                            Helpers.DeserializePassedJSON(
                                data,
                                WarehousesActivity.Results.SelectedWarehouseJson,
                                typeof(MagazynO)
                            );
                        Helpers.SetTextOnTextView(this, TargetWarehouse, Selected.Nazwa);
                        TargetWarehouse.Tag = Selected.ID;
                        break;
                    }
                    case (int)ResultCodes.ContractorsActivityResult:
                    {
                        KontrahentVO Selected = (KontrahentVO)
                            Helpers.DeserializePassedJSON(
                                data,
                                ContractorsActivity.Results.SelectedJSON,
                                typeof(KontrahentVO)
                            );
                        Helpers.SetTextOnTextView(this, Contractor, Selected.strNazwa);
                        Contractor.Tag = Selected.ID;
                        break;
                    }
                }
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity)
                return;

            RunIsBusyAction(() =>
            {
                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(DocumentsActivity));
                i.PutExtra(DocumentsActivity.Vars.DocType, (int)DocType);
                i.SetFlags(ActivityFlags.NewTask);

                StartActivity(i);
                Finish();
            });
        }
    }
}
