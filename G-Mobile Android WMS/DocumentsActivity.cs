using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Drm;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Systems;
using Android.Views;
using Android.Widget;
using Java.Nio.Channels;
using Java.Util;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;
using Xamarin.Essentials;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false
    )]
    public class DocumentsActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton Add;
        FloatingActionButton Delete;
        FloatingActionButton Edit;
        FloatingActionButton Refresh;
        FloatingActionButton Search;
        FloatingActionButton Confirm;

        LinearLayout DocumentsListHeader;
        ListView ListView;
        TextView ItemCount;
        TextView ScanHint;

        string FilterText = "";

        Enums.ZLMMMode ZLMMMode = Enums.ZLMMMode.None;

        public bool ShowCheckboxes = false;
        public List<int> CheckedItems = new List<int>();
        List<LokalizacjaRow> LokGru; // list of location for multipicking
        GrupaLokalizacjiO Gru = null; //scanned group location

        List<int> _StatsusFilters = null;
        public List<int> StatsusFilters
        {
            get { return _StatsusFilters; }
            set
            {
                _StatsusFilters = value;
                (ListView.Adapter as DocumentsListAdapter).StatsusFilters = _StatsusFilters;
            }
        }

        internal static class Vars
        {
            public const string DocType = "DocType";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_documents);

            DocType = (Enums.DocTypes)Intent.GetIntExtra(Vars.DocType, (int)Enums.DocTypes.Error);

            if (DocType == Enums.DocTypes.MMDistribution || DocType == Enums.DocTypes.MMGathering)
                DocType = Enums.DocTypes.MM;

            if (DocType == Enums.DocTypes.ZLDistribution || DocType == Enums.DocTypes.ZLGathering)
                DocType = Enums.DocTypes.ZL;

            GetAndSetControls();

            Task.Run(() => InsertData());
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(
                this,
                GetString(Resource.String.documents_activity_name)
                    + " "
                    + Helpers.StringDocType(DocType)
            );

            Back = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_back);
            Add = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_add);
            Edit = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_edit);
            Delete = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_delete);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_refresh);
            DocumentsListHeader = FindViewById<LinearLayout>(Resource.Id.documents_list_header);
            Search = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_search);
            Confirm = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_confirm);

            ListView = FindViewById<ListView>(Resource.Id.list_view_documents);
            ItemCount = FindViewById<TextView>(Resource.Id.documents_item_count);
            ScanHint = FindViewById<TextView>(Resource.Id.scanhint);
            ListView.ItemClick += ListView_ItemClick;

            if (DocType != Enums.DocTypes.IN)
                ListView.ItemLongClick += ListView_ItemLongClick;

            DocumentsListHeader.Click += DocumentsListHeader_Click;

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
            Add.Click += Add_Click;
            Add.LongClick += Add_LongClick;
            Delete.Click += Delete_Click;
            Edit.Click += Edit_Click;
            Search.Click += Search_Click;

            if (DocType == Enums.DocTypes.IN)
            {
                Add.Visibility = ViewStates.Gone;
                Delete.Visibility = ViewStates.Gone;
            }

            if (
                !Globalne.CurrentUserSettings.CanDeleteOwnDocuments
                && !Globalne.CurrentUserSettings.CanDeleteAllDocuments
            )
                Delete.Visibility = ViewStates.Gone;
        }

        private void Add_LongClick(object sender, View.LongClickEventArgs e)
        {
            if (DocType != Enums.DocTypes.ZL)
                return;

            Add.PlaySoundEffect(SoundEffects.Click);

            RunIsBusyAction(() =>
            {
                ActionSheetConfig Conf = new ActionSheetConfig()
                    .SetCancel(GetString(Resource.String.global_cancel))
                    .SetTitle("Utwórz dokument ZL z bufora:");

                foreach (
                    var item in Serwer.lokalizacjaBL.PobierzListęDostępnychLokalizacjiBuforowych(
                        Globalne.Magazyn.ID,
                        true
                    )
                )
                {
                    var przychod = Serwer
                        .przychRozchBL.PobierzListęPrzychodów(
                            -1,
                            Globalne.Magazyn.ID,
                            item.ID,
                            -1,
                            "",
                            -1,
                            "",
                            -1,
                            -1,
                            -1,
                            true
                        )
                        .ToList();

                    foreach (var przych in przychod.GroupBy(x => x.idDokumentP))
                    {
                        var dokument = Serwer.dokumentBL.PobierzDokument(
                            przych.Key,
                            "",
                            "",
                            -1,
                            item.ID,
                            ""
                        );
                        var pozycjeCount = Serwer
                            .dokumentBL.PobierzListęPozycji(dokument.ID)
                            .Distinct()
                            .Count();
                        // dla dokumentow PW ze statusem "Zamknięty"
                        if (dokument.idStatusDokumentu == 1039)
                            Conf.Add(
                                item.strNazwa.ToUpper()
                                    + $" [pozycje: {pozycjeCount}, {dokument.strNazwa}]",
                                () => CreateDocumentFromBuffor(item.ID)
                            );
                    }
                }

                if (Conf.Options.Count > 0)
                {
                    UserDialogs.Instance.ActionSheet(Conf);
                }
                else
                {
                    Conf.SetTitle("Brak pozycji buforowych");
                    UserDialogs.Instance.ActionSheet(Conf);
                }
            });
        }

        void CreateDocumentFromBuffor(int idBuffor)
        {
            var Rejestr = Serwer.rejestrBL.PobierzPierwszyRejestrDlaDokumentu(
                "ZL",
                Globalne.Magazyn.ID
            );

            DokumentVO Dokument = Serwer.dokumentBL.PustyDokumentVO();
            Dokument.intLokalizacjaPozycji = idBuffor;
            Dokument.intEdytowany = Globalne.Operator.ID;
            Dokument.intUtworzonyPrzez = Globalne.Operator.ID;
            Dokument.intZmodyfikowanyPrzez = Globalne.Operator.ID;
            Dokument.idRejestr = Rejestr.ID;
            Dokument.intPriorytet = Rejestr.intPriorytet;
            Dokument.dataDokumentu = Serwer.ogólneBL.GetSQLDate();
            Dokument.bTworzenieNaTerminalu = true;
            Dokument.bDokumentMobilny = true;
            Dokument.bZlecenie = true;
            Dokument.idFunkcjiLogistycznejP = -1;
            Dokument.idFunkcjiLogistycznejW = -1;
            Dokument.strDokumentDostawcy = "";
            Dokument.idKontrahent = -1;

            StatusDokumentuO Status = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                "ZL",
                1
            );
            Dokument.idStatusDokumentu = Status.ID;

            Dokument.intMagazynP = Globalne.Magazyn.ID;
            Dokument.intMagazynW = Globalne.Magazyn.ID;

            int DodanyDok = Serwer.dokumentBL.ZróbDokument(Dokument);
            if (DodanyDok == -1)
            {
                Helpers.Alert(this, "BłądDodawaniaDokumentu").Wait();
            }
            else if (idBuffor != -1)
            {
                bool ZabieranieCałegoBufora = true;
                if (ZabieranieCałegoBufora)
                {
                    Serwer.dokumentBL.WypełnijTowaramiZBufora(
                        DodanyDok,
                        idBuffor,
                        "ZL",
                        true,
                        Globalne.Operator.ID
                    );
                }
            }
            Helpers.HideProgressDialog();

            BusinessLogicHelpers.Documents.EditDocuments(
                this,
                new List<int>() { DodanyDok },
                Enums.DocTypes.ZLDistribution,
                Enums.ZLMMMode.TwoStep,
                -1
            );
        }

        private async void Search_Click(object sender, EventArgs e)
        {
            var Res = await Helpers.AlertAsyncWithPrompt(
                this,
                Resource.String.documents_filter,
                null,
                FilterText,
                InputType.Default
            );

            if (Res.Ok)
            {
                FilterText = Res.Text;
                await RunIsBusyTaskAsync(() => InsertData());
            }
        }

        private void DocumentsListHeader_Click(object sender, EventArgs e)
        {
            ShowCheckboxes = false;
            CheckedItems = new List<int>();
            updateAmountOfPostions();
            for (int i = 0; i < ListView.Count; i++)
            {
                View Item = ListView.GetChildAt(i);

                if (Item != null)
                {
                    CheckBox Chb = (CheckBox)Item.FindViewById(Resource.Id.documents_list_checkbox);
                    Chb.Checked = false;
                }
            }

            if (ListView.Adapter != null)
                (ListView.Adapter as DocumentsListAdapter).NotifyDataSetChanged();
        }

        private void ListView_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            ShowCheckboxes = true;
            CheckedItems = new List<int>();
            (ListView.Adapter as DocumentsListAdapter).NotifyDataSetChanged();
        }

        /// <summary>
        /// Otwarty       0
        /// Do realizacji 1
        /// Pauza         1
        /// W realizacji  2
        /// Wykonany      3
        /// Wstrzymany    4
        /// Zamknięty     5
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (ShowCheckboxes == false)
            {
                if (CheckedItems.Contains(e.Position))
                {
                    // jezeli wlaczony podglad dokumentów wykonywanych przez innych operatorow to pokaz wszystkie w statusie "W realizacji"
                    if (
                        true
                        && ListView.Adapter != null
                        && (ListView.Adapter is DocumentsListAdapter)
                    )
                    {
                        var row = (ListView.Adapter as DocumentsListAdapter)[e.Position];
                        int idDok = Convert.ToInt32(row[0]);
                        string nazwaDok = row[1].ToString();

                        var dokument = Task
                            .Factory.StartNew(
                                () =>
                                    Serwer.dokumentBL.PobierzDokument(
                                        idDok,
                                        "",
                                        nazwaDok,
                                        -1,
                                        -1,
                                        ""
                                    )
                            )
                            .Result;

                        /// Statusy Dokumentów (W realizacji)
                        /// ID      TypDok
                        /// 1036	PW
                        /// 1043    PZ
                        /// 1050    RW
                        /// 1057    WZ
                        var statusy = new[] { 1036, 1043, 1050, 1057 };
                        /// sprawdzamy czy dokument jest w statusie "W realizacji" i wsyswietlamy informacje kto go edytuje
                        if (
                            statusy.Contains(dokument.idStatusDokumentu)
                            && dokument.intEdytowany != Globalne.Operator.ID
                            && dokument.intEdytowany != -1
                        )
                        {
                            var nazwaOperatora = Task
                                .Factory.StartNew(
                                    () => Serwer.operatorBL.PobierzOperatora(dokument.intEdytowany)
                                )
                                .Result.Nazwa;
                            Helpers.CenteredToast(
                                "Dokument jest edytowany przez operatora: " + nazwaOperatora,
                                ToastLength.Long
                            );
                            return;
                        }
                    }
                    Edit_Click(this, null);
                    return;
                }
            }

            CheckedItems = new List<int> { e.Position };
        }

        private void Edit_Click(object sender, EventArgs e)
        {
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

                await RunIsBusyTaskAsync(async () =>
                {
                    ZLMMMode = Enums.ZLMMMode.None;

                    if (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.ZL)
                    {
                        ZLMMMode = await BusinessLogicHelpers.Documents.AskZLMMMode(this, DocType);
                        // fix to ZLMM Documentsactivity
                    }
                });

                // if (DocType == Enums.DocTypes.MM || DocType == Enums.DocTypes.ZL)
                //
                //     if (ZLMMMode == Enums.ZLMMMode.None)
                //
                //         return;

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

                Helpers.ShowProgressDialog(GetString(Resource.String.documents_opening));
                await Task.Delay(100);

                if (ZLMMMode == Enums.ZLMMMode.None)
                    ZLMMMode = Globalne.CurrentSettings.DefaultZLMode; //Enums.ZLMMMode.TwoStep;

                BusinessLogicHelpers.Documents.EditDocuments(
                    this,
                    SelectedDocIDs,
                    DocType,
                    ZLMMMode,
                    -1,
                    Docs,
                    Multipicking
                );

                Helpers.HideProgressDialog();
            }
            catch (Exception ex)
            {
                Helpers.HideProgressDialog();
                Helpers.HandleError(this, ex);
                IsSwitchingActivity = false;
                return;
            }
        }

        async void Delete_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => DoDelete());
        }

        async Task DoDelete()
        {
            try
            {
                if (CheckedItems.Count == 0)
                    return;
                else
                {
                    List<int[]> Docs = new List<int[]>();

                    foreach (int Pos in CheckedItems)
                    {
                        object[] Selected = (ListView.Adapter as DocumentsListAdapter)[Pos];

                        Docs.Add(
                            new int[2]
                            {
                                Convert.ToInt32(
                                    Selected[(int)SQL.Documents.Documents_Results.intUtworzonyPrzez]
                                ),
                                Convert.ToInt32(
                                    Selected[(int)SQL.Documents.Documents_Results.idDokumentu]
                                )
                            }
                        );
                    }

                    bool OK = await BusinessLogicHelpers.Documents.DeleteDocuments(this, Docs);

                    if (OK)
                    {
                        ShowCheckboxes = false;
                        CheckedItems = new List<int>();
                        await Task.Run(() => InsertData());
                    }
                }
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
                if (IsSwitchingActivity)
                    return;

                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(CreatingDocumentActivity));
                i.PutExtra(CreatingDocumentActivity.Vars.DocType, (int)DocType);
                i.SetFlags(ActivityFlags.NewTask);

                StartActivity(i);
                Finish();
            });
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        private void Back_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                if (CallingActivity == null)
                {
                    Helpers.SwitchAndFinishCurrentActivity(this, typeof(ModulesActivity));
                }
            });
        }

        protected override void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            RunIsBusyAction(() => DoBarcode(LastScanData));
        }

        private void DoBarcode(List<string> Barcodes)
        {
            if (Barcodes == null || Barcodes.Count == 0)
                return;

            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            try
            {
                IsBusy = false;

                DokumentVO Dok = Serwer.dokumentBL.PobierzDokument(-1, "", "", -1, -1, Barcodes[0]);
                var rejestr = Serwer.rejestrBL.PobierzRejestr(Dok.idRejestr);

                // dokument WZ - podkreslenie " _ " przed numerem zamowienia
                DokumentVO _Dok = Serwer.dokumentBL.PobierzDokument(
                    -1,
                    "",
                    "",
                    -1,
                    -1,
                    "_" + Barcodes[0]
                );
                var _rejestr = Serwer.rejestrBL.PobierzRejestr(_Dok.idRejestr);

                // Sprawdzanie czy pobrany dokument jest o typie w ktorym znajduje sie oparator (np. dokument to WZ i znajdujemy sie na WZ)
                if (
                    (_Dok.ID >= 0 && _rejestr.strTyp == DocType.ToString())
                    || (Dok.ID >= 0 && rejestr.strTyp == DocType.ToString())
                )
                {
                    if (_Dok.ID >= 0 && _rejestr.strTyp == DocType.ToString())
                        Dok = _Dok;
                }
                else if (
                    (_rejestr.ID > -1 && (_rejestr.strTyp != DocType.ToString()))
                    || (rejestr.ID > -1 && ((rejestr.strTyp != DocType.ToString())))
                )
                {
                    AutoException.ThrowIfNotNull(this, Resource.String.documents_docfound_barcode);
                }
                else if (
                    !Serwer.lokalizacjaBL.SprawdźCzyGrupaLokalizacjiIstnieje(
                        Barcodes[0],
                        Barcodes[0],
                        -1,
                        Globalne.Magazyn.ID
                    )
                )
                    AutoException.ThrowIfNotNull(
                        this,
                        Resource.String.documents_didnotfound_barcode
                    );

                if (Dok.ID >= 0)
                {
                    DoEdit(null, new List<DokumentVO>() { Dok });
                    return;
                }

                // Multipicking działa tylko dla WZ i RW
                if (
                    (DocType != Enums.DocTypes.WZ && DocType != Enums.DocTypes.RW)
                    || !Globalne.CurrentSettings.Multipicking
                )
                    AutoException.ThrowIfNotNull(
                        this,
                        Resource.String.documents_didnotfound_barcode
                    );
                else
                {
                    // Pobranie dokumentu przypisanego już do lokalizacji
                    LokalizacjaVO Lok = Serwer.lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(
                        Barcodes[0],
                        Globalne.Magazyn.ID,
                        true
                    );

                    if (Lok.ID >= 0)
                    {
                        if (Lok.idMagazyn != Globalne.Magazyn.ID)
                            AutoException.ThrowIfNotNull(
                                this,
                                Resource.String.documents_loc_fromdifferent_warehouse
                            );

                        DokumentVO DokLok = Serwer.dokumentBL.PobierzDokument(
                            -1,
                            "",
                            "",
                            -1,
                            Lok.ID,
                            ""
                        );

                        if (DokLok.ID >= 0)
                        {
                            DoEdit(null, new List<DokumentVO>() { DokLok });
                            return;
                        }
                        else
                            AutoException.ThrowIfNotNull(
                                this,
                                Resource.String.documents_didnotfound_assignedtoloc
                            );
                    }

                    // Przypisanie dokumentów do grupy lokalizacji
                    this.Gru = Serwer.lokalizacjaBL.PobierzGrupęlokalizacji(Barcodes[0]);

                    if (this.Gru.ID < 0)
                    {
                        AutoException.ThrowIfNotNull(
                            this,
                            Resource.String.documents_didnotfound_barcode
                        );
                    }
                    else if (this.Gru.idMagazyn != Globalne.Magazyn.ID)
                    {
                        AutoException.ThrowIfNotNull(
                            this,
                            Resource.String.documents_locgroup_fromdifferent_warehouse
                        );
                    }
                    else
                    {
                        // Jeśli do grupy przypisane są już dokumenty...
                        List<int> Docs =
                            Serwer.dokumentBL.PobierzListęIDDokumentówPrzypisanychDoGrupyLokalizacji(
                                this.Gru.ID
                            );

                        if (Docs.Count != 0)
                        {
                            DoEdit(Docs, null, true);
                            return;
                        }
                        else
                        {
                            List<int> idForMultipicking = getIdOfDocumentsInProcessingOnLocation(
                                this.Gru.ID
                            );
                            Enums.WZRWContMode continueMode = Enums.WZRWContMode.NewMul;
                            if (idForMultipicking.Count > 0)
                            {
                                continueMode = BusinessLogicHelpers
                                    .Documents.AskWZRWContinuation(this, DocType)
                                    .Result;
                                if (continueMode == Enums.WZRWContMode.None)
                                {
                                    Helpers.HideProgressDialog();
                                    return;
                                }
                            }

                            Enums.WZRWMode selectionMode = Enums.WZRWMode.AllDocuments;
                            if (continueMode != Enums.WZRWContMode.ContinueMul)
                            {
                                selectionMode = BusinessLogicHelpers
                                    .Documents.AskWZRWMode(this, DocType)
                                    .Result;
                                if (selectionMode == Enums.WZRWMode.None)
                                {
                                    Helpers.HideProgressDialog();
                                    return;
                                }
                            }

                            this.LokGru =
                                Serwer.lokalizacjaBL.PobierzListęLokalizacjaRowZGrupyLokalizacji(
                                    this.Gru.ID
                                );

                            if (selectionMode == Enums.WZRWMode.AllDocuments)
                            {
                                if (continueMode == Enums.WZRWContMode.NewMul)
                                {
                                    idForMultipicking = Serwer.dokumentBL.PobierzNumerki();
                                }
                                GoIntoMultiPicking(idForMultipicking);
                                return;
                            }
                            else
                            {
                                SwitchIntoMultiPickSelection();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                Helpers.HideProgressDialog();
                return;
            }

            Helpers.HideProgressDialog();
        }

        List<int> getIdOfDocumentsInProcessingOnLocation(int idLok)
        {
            ZapytanieZTabeliO Zap = Serwer.ogólneBL.ZapytanieSQL(
                $@"SELECT d.idDokumentu
FROM Dokumenty d
WHERE d.intLokalizacja IN
    (SELECT l.idLokalizacja
     FROM Lokalizacja l
     LEFT JOIN LokalizacjeWGrupie lwg ON lwg.idLokalizacja=l.idLokalizacja
     WHERE lwg.idGrupaLokalizacji={idLok} )
  AND d.idStatusDokumentu=1057 ;"
            );
            return Zap.ListaWierszy.Select(x => Convert.ToInt32(x[0])).ToList();
        }

        void GoIntoMultiPicking(List<int> numerki)
        {
            try
            {
                Helpers.ShowProgressDialog(
                    GetString(Resource.String.global_setting_documents_to_group)
                );

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Helpers.ShowProgressDialog(
                        GetString(Resource.String.global_setting_documents_to_group)
                    );
                });
                List<int> DocsToEdit = new List<int>();
                DocsToEdit = DocsToEdit
                    .Concat(getIdOfDocumentsInProcessingOnLocation(this.Gru.ID))
                    .ToList();

                ZapytanieZTabeliO Zap = Serwer.ogólneBL.ZapytanieSQL(
                    @"select
                                        	d.idDokumentu,
                                        	sum(pd.numIloscZlecona) as numZlecona,
                                        	sum(pd.numIloscZrealizowana) as numZrealizowana
                                        from
                                        	Dokumenty d
                                        left join Lokalizacja l on
                                        	l.idLokalizacja = d.intLokalizacja
                                        	left join PozycjeDokumentu pd on
                                        	pd.idDokumentu  = d.idDokumentu 
                                        where
                                        	intLokalizacja in (
                                        	select
                                        		idlokalizacja
                                        	from
                                        		Lokalizacja l
                                        	where
                                        		l.bkuweta = 1)
                                        GROUP BY
                                            d.idDokumentu,
                                            d.strNazwa,
                                            d.strERP,
                                            d.intLokalizacja,
                                            l.strKod,
                                            l.strNazwa
                                        ORDER BY
                                            d.intLokalizacja ASC;"
                );

                foreach (int numerek in numerki)
                {
                    if (this.LokGru.Count == 0)
                        break;

                    int IDDoc = numerek;

                    // usuniecie lokalizacji w ktorych są przypisane dokumenty do grupy lokalizacji
                    // dodanie tego wpisu ma na celu przyspieszenie wyszukiwania wonych kuwet do zamówień (dokumnetów)
                    // usunie z listy te ktore sa przypisane przez co proces szukania się przyśpieszy

                    //ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)Helpers.HiveInvoke(typeof(WMSServerAccess.Ogólne.OgólneBL), "ZapytanieSQL",
                    //    "select intLokalizacja from Dokumenty d where intLokalizacja in (select idlokalizacja from Lokalizacja l where l.bkuweta = 1)");

                    //foreach (object[] intLokalizacja in Zap.ListaWierszy)
                    //{
                    //    var lokalizacjaWGrupie = this.LokGru.Where(x => x.ID == Convert.ToInt16(intLokalizacja[0])).FirstOrDefault();
                    //    if (lokalizacjaWGrupie != null)
                    //    {
                    //        this.LokGru.Remove(lokalizacjaWGrupie);
                    //        continue;
                    //    }
                    //}

                    decimal SumaPoz = Serwer.dokumentBL.PobierzWykonanąSumęPozycjiDokumentu(IDDoc);

                    if (SumaPoz == 0)
                    {
                        // quesry for completain list


                        foreach (LokalizacjaRow LG in this.LokGru)
                        {
                            int Wynik =
                                Serwer.dokumentBL.SprawdźCzyDokumentMożeByćPrzypisanyDoLokalizacji(
                                    IDDoc,
                                    LG.ID,
                                    true
                                );

                            if (
                                Wynik != 0
                                && Zap.ListaWierszy.Where(x => Convert.ToInt32(x[0]) == IDDoc)
                                    .Select(X => X[1] != X[2])
                                    .FirstOrDefault()
                            )
                            {
                                Wynik = 0;
                            }

                            if (Wynik == 0)
                            {
                                try
                                {
                                    Serwer.dokumentBL.UstawLokalizacjęDokumentu(IDDoc, LG.ID);
                                    DocsToEdit.Add(IDDoc);
                                    this.LokGru.Remove(LG);
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    AutoException.ThrowIfNotNull(ex);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (DocsToEdit.Count == 0)
                    AutoException.ThrowIfNotNull(this, Resource.String.documents_cannot_assign);

                DoEdit(DocsToEdit, null, true);
                return;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                Helpers.HideProgressDialog();
                return;
            }
        }

        // this mehtod should have reverse responsibilty of SwitchBackFromMultiPickSelection
        void SwitchIntoMultiPickSelection()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ShowCheckboxes = true;
                CheckedItems = new List<int>();
                Back.Click -= Back_Click;
                Back.Click += SwitchBackFromMultiPickSelection;
                DocumentsListHeader.Click += SwitchBackFromMultiPickSelection;
                Confirm.Click += doMultiPickingOnSelected;
                Helpers.SetTextOnTextView(
                    this,
                    ScanHint,
                    this.Gru.strNazwa
                        + " ("
                        + GetString(Resource.String.documents_number_postion)
                        + this.LokGru.Count
                        + ")"
                );
                Confirm.Show();
                Delete.Hide();
                Add.Hide();
                Edit.Hide();
                //Search.Hide();
                //Refresh.Hide();
                this.StatsusFilters = new List<int> { 1 };
                updateAmountOfPostions();
                (ListView.Adapter as DocumentsListAdapter).NotifyDataSetChanged();
            });
        }

        // this mehtod should have reverse responsibilty of SwitchIntoMultiPickSelection
        void SwitchBackFromMultiPickSelection(object sender, EventArgs e)
        {
            Back.Click -= SwitchBackFromMultiPickSelection;
            Back.Click += Back_Click;
            DocumentsListHeader.Click -= SwitchBackFromMultiPickSelection;
            Confirm.Click -= doMultiPickingOnSelected;
            Helpers.SetTextOnTextView(
                this,
                ScanHint,
                GetString(Resource.String.documents_activity_scanhint)
            );
            ShowCheckboxes = false;
            CheckedItems = new List<int>();
            Confirm.Hide();
            Delete.Show();
            Add.Show();
            Edit.Show();
            //Search.Show();
            //Refresh.Show();
            updateAmountOfPostions();
            this.StatsusFilters = null;
            (ListView.Adapter as DocumentsListAdapter).NotifyDataSetChanged();
        }

        void doMultiPickingOnSelected(object sender, EventArgs e)
        {
            try
            {
                if (CheckedItems == null || CheckedItems.Count == 0)
                {
                    throw new Exception(GetString(Resource.String.documentitem_cant_be_zero));
                }
                if (CheckedItems.Count > LokGru.Count)
                {
                    throw new Exception(
                        GetString(Resource.String.documentitem_cant_be_less_than_location)
                    );
                }

                object obj = ListView.Adapter.GetItem(0);

                GoIntoMultiPicking(
                    CheckedItems
                        .Select(
                            (int x) =>
                            {
                                object[] Selected = (ListView.Adapter as DocumentsListAdapter)[x];
                                return Convert.ToInt32(
                                    Selected[(int)SQL.Documents.Documents_Results.idDokumentu]
                                );
                            }
                        )
                        .ToList()
                );
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                Helpers.HideProgressDialog();
                return;
            }
        }

        async Task<bool> InsertData()
        {
            try
            {
                //ShowCheckboxes = false;
                CheckedItems = new List<int>();

                await Task.Delay(Globalne.TaskDelay);
                Helpers.ShowProgressDialog(GetString(Resource.String.documents_loading));

                ZapytanieZTabeliO Dokumenty = await Task.Factory.StartNew(() => GetData());

                if (Dokumenty.Poprawność != null && Dokumenty.Poprawność != "")
                    throw new Exception(Dokumenty.Poprawność);

                if (Dokumenty.ListaWierszy != null)
                {
                    RunOnUiThread(() =>
                    {
                        ListView.Adapter = new DocumentsListAdapter(this, Dokumenty.ListaWierszy);
                        updateAmountOfPostions();
                        ((DocumentsListAdapter)ListView.Adapter).StatsusFilters = StatsusFilters;
                    });
                }
                else
                    updateAmountOfPostions();

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

        private ZapytanieZTabeliO GetData()
        {
            string Rejestry = "(";

            List<RejestrRow> DostępneRejestry =
                Serwer.rejestrBL.PobierzListęRejestrówDostępnychDlaOperatora(
                    Helpers.StringDocType(DocType),
                    (Helpers.StringDocType(DocType) == Enums.DocTypes.MM.ToString())
                        ? -1
                        : Globalne.Magazyn.ID,
                    Globalne.Operator.ID
                );

            if (DostępneRejestry.Count == 0)
                return new ZapytanieZTabeliO();

            foreach (RejestrRow R in DostępneRejestry)
                Rejestry += R.ID + ",";

            Rejestry += "-1)";

            string Komenda;

            Komenda = SQL
                .Documents.GetDocs.Replace("<<IDOPERATORA>>", Globalne.Operator.ID.ToString()) // Globalne.Operator.ID.ToString())\
                .Replace(
                    "<<ID_EDYTOWANY>>",
                    Globalne.CurrentUserSettings.ShowHidenDocumentsEditingByOthers
                        ? "-1"
                        : Globalne.Operator.ID.ToString()
                )
                .Replace("<<REJESTRY>>", Rejestry)
                .Replace(
                    "<<DATAPOCZĄTKOWA>>",
                    Serwer
                        .ogólneBL.GetSQLDate()
                        .AddDays(-Globalne.CurrentSettings.DocumentsDaysDisplayThreshhold - 30)
                        .ToString("yyyy-MM-dd")
                );

            if (
                DocType == Enums.DocTypes.PW
                || DocType == Enums.DocTypes.PZ
                || DocType == Enums.DocTypes.ZL
                || DocType == Enums.DocTypes.IN
            )
                Komenda += SQL.Documents.GetDocs_Where_Przychód.Replace(
                    "<<IDMAGAZYNU>>",
                    Globalne.Magazyn.ID.ToString()
                );
            else
                Komenda += SQL.Documents.GetDocs_Where_Rozchód.Replace(
                    "<<IDMAGAZYNU>>",
                    Globalne.Magazyn.ID.ToString()
                );

            if (DocType == Enums.DocTypes.IN)
                Komenda += SQL.Documents.GetDocs_Where_SubDoc;

            if (!String.IsNullOrWhiteSpace(FilterText))
                Komenda += SQL.Documents.GetDocs_Where_Filter.Replace("<<FILTERTEXT>>", FilterText);

            Komenda += SQL.Documents.GetDocs_OrderBy;

            // #warning HiveInvoke
            ZapytanieZTabeliO Zap = Serwer.ogólneBL.ZapytanieSQL(Komenda);

            //ZapytanieZTabeliO Zap = Serwer.ogólneBL.WykonajZapytanieWidokuZFiltrem(Komenda);
            return Zap;
        }

        public void updateAmountOfPostions()
        {
            if (ShowCheckboxes)
            {
                Helpers.SetTextOnTextView(
                    this,
                    ItemCount,
                    GetString(Resource.String.global_liczba_pozycji)
                        + " "
                        + this.CheckedItems.Count
                        + "/"
                        + ListView.Adapter?.Count.ToString()
                );
            }
            else
                Helpers.SetTextOnTextView(
                    this,
                    ItemCount,
                    GetString(Resource.String.global_liczba_pozycji)
                        + " "
                        + ListView.Adapter?.Count.ToString()
                );
        }
    }

    internal class DocumentsListAdapter : BaseAdapter<object[]>
    {
        readonly List<object[]> AllItems;
        List<object[]> Items;

        public List<int> _StatsusFilters;
        public List<int> StatsusFilters
        {
            get { return _StatsusFilters; }
            set
            {
                _StatsusFilters = value;
                peferomFiltering();
            }
        }

        readonly DocumentsActivity Ctx;

        public DocumentsListAdapter(DocumentsActivity Ctx, List<object[]> Items)
            : base()
        {
            this.Ctx = Ctx;
            this.AllItems = Items;
            peferomFiltering();
        }

        private void peferomFiltering()
        {
            this.Items = this
                .AllItems.FindAll(
                    (object[] item) =>
                    {
                        return StatsusFilters == null
                            || this.StatsusFilters.Contains(Convert.ToInt32(item[5]));
                    }
                )
                .ToList();
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override object[] this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items == null ? 0 : Items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Pos = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_documents, null);

            view.FindViewById<TextView>(Resource.Id.documents_list_number).Text = (string)
                Pos[(int)SQL.Documents.Documents_Results.strNazwaDokumentu];
            view.FindViewById<TextView>(Resource.Id.documents_list_contractor).Text = (string)
                Pos[(int)SQL.Documents.Documents_Results.strNazwaKontrahenta];
            view.FindViewById<TextView>(Resource.Id.documents_list_date).Text = (
                (DateTime)Pos[(int)SQL.Documents.Documents_Results.dtDataDokumentu]
            ).ToString(Globalne.CurrentSettings.DateFormat);

            List<string> Infos = new List<string>()
            {
                (string)Pos[(int)SQL.Documents.Documents_Results.strOpis],
                (string)Pos[(int)SQL.Documents.Documents_Results.strDokumentDostawcy],
                (string)Pos[(int)SQL.Documents.Documents_Results.strNazwaERP]
            };

            string Description = String.Join(", ", Infos.Where(s => !String.IsNullOrEmpty(s)));

            TextView v = view.FindViewById<TextView>(Resource.Id.documents_list_description);

            if (!String.IsNullOrEmpty(Description))
            {
                v.Text = Description;
                v.Visibility = ViewStates.Visible;
            }
            else
                v.Visibility = ViewStates.Gone;

            TextView Status = view.FindViewById<TextView>(Resource.Id.documents_list_status);

            Enums.DocumentStatusTypes DocStatus = (Enums.DocumentStatusTypes)
                Convert.ToInt32(Pos[(int)SQL.Documents.Documents_Results.intTypStatusu]);
            bool DocAssigned = (bool)Pos[(int)SQL.Documents.Documents_Results.bZlecenie];

            Status.SetBackgroundColor(Helpers.GetDocStatusColorForStatus(DocStatus));

            if (!DocAssigned)
                Status.SetTextColor(Helpers.GetDocStatusColorForStatus(DocStatus));
            else
                Status.SetTextColor(Android.Graphics.Color.Black);

            // jezeli włączona obsluga koloryzowania dokumentów czesciowo wykonanych i status dokumentu to 'DoRealizacji'
            // to pobieramy liste pozycji i szukamy w ilosciach zleconych i zrealizowanych
            // czy ktorakolwiek pozycja jest zrealizowana czyli ilosc zlecona = ilosci zrealizowanej
            // jezeli jest to oznaczamy taki dokument kolorem wybranym przez uzytkownika => ColorForEditedPositionsOnDocument
            if (
                Globalne.CurrentUserSettings.ShowDifferenceColorOnDocumentsWhenAnyPositionIsComplete
                && DocStatus == Enums.DocumentStatusTypes.DoRealizacji
            )
            {
                int idDok = Convert.ToInt32(Pos[(int)SQL.Documents.Documents_Results.idDokumentu]);
                var pozycje = Task
                    .Factory.StartNew(() => Serwer.dokumentBL.PobierzListęPozycji(idDok))
                    .Result;

                if (pozycje.ToList().Any(x => x.numIloscZlecona == x.numIloscZrealizowana))
                {
                    Status.SetBackgroundColor(
                        Android.Graphics.Color.ParseColor(
                            "#FF" + Globalne.CurrentUserSettings.ColorForEditedPositionsOnDocument
                        )
                    );
                    if (!DocAssigned)
                        Status.SetTextColor(
                            Android.Graphics.Color.ParseColor(
                                "#FF"
                                    + Globalne.CurrentUserSettings.ColorForEditedPositionsOnDocument
                            )
                        );
                    else
                        Status.SetTextColor(Android.Graphics.Color.Black);
                }
            }

            CheckBox chb = view.FindViewById<CheckBox>(Resource.Id.documents_list_checkbox);
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
            Ctx.updateAmountOfPostions();
        }
    }
}
