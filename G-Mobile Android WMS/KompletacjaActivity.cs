using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Serialization;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Common.BusinessLogicHelpers;
using G_Mobile_Android_WMS.Enums;
using G_Mobile_Android_WMS.ExtendedModel;
using Hive.Serialization.Core;
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
    public class KompletacjaActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        FloatingActionButton Refresh;
        RelativeLayout KompletacjaView;

        Button OK;

        ListView ListView;
        TextView ItemCount;
        TextView ScanHint;
        TextView ScanHint2;
        TextView ItemSum;
        TextView DocExternal;

        public DocTypes DocType = DocTypes.PW;
        public Operation CurrentOperation = Operation.In;

        DokumentVO CurrentDoc;

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
            SetContentView(Resource.Layout.activity_kompletacja);

            DocType = (DocTypes)Intent.GetIntExtra(EditingDocumentsActivity_Common.Vars.DocType, 0);

            GetAndSetControls();
            IsBusy = false;
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
                    case EditingDocumentsListDisplayElements.ArticleName:
                        VisibilityToSet = true;
                        v.Text = (CurrentDoc == null ? "Nazwa dokumentu ERP" : "Nazwa towaru");
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
            Helpers.SetActivityHeader(this, GetString(Resource.String.kompletacja_activity_name));

            ScanHint = FindViewById<TextView>(Resource.Id.scanhint);
            ScanHint2 = FindViewById<TextView>(Resource.Id.scanhint2);
            KompletacjaView = FindViewById<RelativeLayout>(Resource.Id.kompletacja_view);
            DocExternal = FindViewById<TextView>(Resource.Id.kompletacja_strNazwaERP);

            Back = FindViewById<FloatingActionButton>(Resource.Id.editingdocuments_btn_back);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.documents_btn_refresh);

            ListView = FindViewById<ListView>(Resource.Id.list_view_editingdocuments);
            ItemCount = FindViewById<TextView>(Resource.Id.editingdocuments_item_count);
            ItemSum = FindViewById<TextView>(Resource.Id.editingdocuments_item_sum);

            OK = FindViewById<Button>(Resource.Id.kompletacja_ok);

            Back.Click += Back_Click;
            OK.Click += OK_Click;
            ListView.ItemClick += ListView_ItemClick;
            Refresh.Click += Refresh_Click;
        }

        private async void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var selected = (ListView.Adapter as KompletacjaListAdapter)[e.Position];
            await RunIsBusyTaskAsync(
                () =>
                    ShowProgressAndDecideOperation(
                        new List<string>()
                        {
                            Globalne
                                .lokalizacjaBL.PobierzLokalizację(selected.ExIDLokalizacjaP)
                                .strKod
                        }
                    )
            );
        }

        private async void OK_Click(object sender, EventArgs e)
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.kompletacja_confirming));
            await Task.Run(async () =>
            {
                try
                {
                    Serwer.dokumentBL.UstawLokalizacjęDokumentu(CurrentDoc.ID, -1);
                    await BusinessLogicHelpers.Documents.ShowAndApplyDocumentExitOptions(
                        this,
                        new List<DokumentVO>() { CurrentDoc },
                        DocType,
                        DocumentLeaveAction.Close,
                        false,
                        false
                    );
                    RunOnUiThread(() =>
                    {
                        ScanHint2.Visibility = ViewStates.Visible;
                        KompletacjaView.Visibility = ViewStates.Gone;
                        Helpers.SetActivityHeader(
                            this,
                            GetString(Resource.String.kompletacja_activity_name)
                        );
                    });
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }

                try
                {
                    DrukarkaO Etykieta = Serwer.drukarkaBL.PowiadomienieEtykiet();
                    bool Resp2 = await Helpers.QuestionAlertAsyncEtykieta(
                        this,
                        Resource.String.Etykieta,
                        Resource.Raw.sound_message
                    );

                    if (!Resp2) { }
                    else
                    {
                        DrukarkaO drukarka = Serwer.drukarkaBL.PobierzDrukarkeEtykiet();
                        bool PrintIsPossible = await Helpers.DoesPrintPossible(this);

                        if (PrintIsPossible)
                        {
                            try
                            {
                                LokalizacjaVO intlokalizacja =
                                    Serwer.lokalizacjaBL.PobierzLokalizację(
                                        CurrentDoc.intLokalizacja
                                    );

                                Serwer.dokumentBL.WydrukEty(
                                    CurrentDoc.strERP,
                                    intlokalizacja.strNazwa,
                                    1
                                );
                            }
                            catch (Exception ex) { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            });
            LastScanData = null;
            CurrentDoc = null;

            Helpers.HideProgressDialog();
            return;
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData(CurrentDoc));
        }

        async void Back_Click(object sender, EventArgs e)
        {
            IsSwitchingActivity = true;

            Intent i = new Intent(this, typeof(ModulesActivity));
            i.SetFlags(ActivityFlags.NewTask);

            StartActivity(i);
            Finish();
        }

        async Task InsertData(DokumentVO Doc)
        {
            try
            {
                Helpers.ShowProgressDialog(GetString(Resource.String.editing_documents_loading));

                await Task.Delay(Globalne.TaskDelay);

                List<DocumentItemRow> Items = new List<DocumentItemRow>();

                if (Doc == null)
                {
                    OK.Visibility = ViewStates.Gone;
                    ItemSum.Visibility = ViewStates.Gone;
                    DocExternal.Visibility = ViewStates.Gone;
#warning HiveInvoke
                    ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)
                        Helpers.HiveInvoke(
                            typeof(WMSServerAccess.Ogólne.OgólneBL),
                            "ZapytanieSQL",
                            @"select
                                        	d.idDokumentu,
                                        	d.strNazwa,
                                        	d.strERP,
                                        	d.intLokalizacja,
                                        	l.strNazwa,
                                        	sum(pd.numIloscZlecona) as numZlecona,
                                        	sum(pd.numIloscZrealizowana) as numZrealizowana,
                                            l.strKod
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

                    int licznik = 0;
                    foreach (object[] wiersz in Zap.ListaWierszy)
                    {
                        var basePoz = Serwer.dokumentBL.PustaPozycjaRow();
                        basePoz.strNazwaTowaru =
                            wiersz[1].ToString() + " / " + wiersz[2].ToString();
                        basePoz.idDokumentu = Convert.ToInt32(wiersz[0]);
                        basePoz.numIloscZlecona = Convert.ToInt32(wiersz[5]);
                        basePoz.numIloscZrealizowana = Convert.ToInt32(wiersz[6]);

                        var isComplete = Convert.ToInt32(wiersz[5]) == Convert.ToInt32(wiersz[6]);

                        Items.Add(
                            new DocumentItemRow()
                            {
                                Base = basePoz,

                                ExLokalizacjaP =
                                    wiersz[4].ToString()
                                    + String.Format(", kod {0}", wiersz[7].ToString()),
                                ExIDLokalizacjaP = Convert.ToInt16(wiersz[3]),
                                ExLokalizacjaW =
                                    wiersz[4].ToString()
                                    + String.Format(", kod {0}", wiersz[7].ToString()),
                                ExIDLokalizacjaW = Convert.ToInt16(wiersz[3]),
                                KolejnośćNaŚcieżce = licznik++,
                                Status = isComplete
                                    ? DocItemStatus.Complete
                                    : DocItemStatus.Incomplete
                            }
                        );
                        ;
                    }
                    if (Items.Count > 0)
                    {
#warning HiveInvoke
                        string TypDoc = (string)
                            Helpers.HiveInvoke(
                                typeof(WMSServerAccess.Dokument.DokumentBL),
                                "PobierzTypDokumentu",
                                new object[]
                                {
                                    Items.FirstOrDefault().Base.idDokumentu,
                                    "",
                                    "",
                                    -1,
                                    -1,
                                    ""
                                }
                            );
                        DocType = (DocTypes)Enum.Parse(typeof(DocTypes), TypDoc);

                        CurrentOperation =
                            (
                                DocType == Enums.DocTypes.PW
                                || DocType == Enums.DocTypes.PZ
                                || DocType == Enums.DocTypes.IN
                            )
                                ? Enums.Operation.In
                                : Enums.Operation.Out;
                    }
                }
                else
                {
                    #region Sprawdzanie po numerze dok lub kuwety (po staremu)



                    // maybe like this
                    //string TypDoc1 = Serwer.dokumentBL.PobierzTypDokumentu( Doc.ID, "", "", -1, -1, "");
#warning HiveInvoke
                    string TypDoc = (string)
                        Helpers.HiveInvoke(
                            typeof(WMSServerAccess.Dokument.DokumentBL),
                            "PobierzTypDokumentu",
                            new object[] { Doc.ID, "", "", -1, -1, "" }
                        );
                    DocType = (DocTypes)Enum.Parse(typeof(DocTypes), TypDoc);

                    CurrentOperation =
                        (
                            DocType == Enums.DocTypes.PW
                            || DocType == Enums.DocTypes.PZ
                            || DocType == Enums.DocTypes.IN
                        )
                            ? Enums.Operation.In
                            : Enums.Operation.Out;

                    Items = await Task.Factory.StartNew(
                        () =>
                            EditingDocumentsActivity_Common.GetData(
                                new List<DokumentVO>() { Doc },
                                DocType,
                                ZLMMMode.None,
                                CurrentOperation,
                                -1,
                                DefaultLocType.None,
                                -1
                            )
                    );

                    #endregion
                }

                // miejsce w ktorym mozna sprawdzic lokalizacje kuweta
                RunOnUiThread(() =>
                {
                    if (ListView.Adapter == null)
                        ListView.Adapter = new KompletacjaListAdapter(this, Items);
                    else
                    {
                        (ListView.Adapter as KompletacjaListAdapter).Items = Items;
                        (ListView.Adapter as KompletacjaListAdapter).NotifyDataSetChanged();
                    }

                    SetVisibilityOnHeaderItems();
                    if (Doc == null)
                    {
                        Helpers.SetActivityHeader(this, "Lista zajętych kuwet");
                    }
                    else
                    {
                        Helpers.SetActivityHeader(this, Doc.strNazwa);
                    }

                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + ListView.Adapter.Count.ToString()
                    );

                    decimal? Sum = (ListView.Adapter as KompletacjaListAdapter).Sum;
                    Helpers.SetTextOnTextView(
                        this,
                        ItemSum,
                        GetString(Resource.String.global_suma_pozycji)
                            + " "
                            + (Sum == null ? "---" : Sum.ToString())
                    );

                    Helpers.SetTextOnTextView(
                        this,
                        DocExternal,
                        GetString(Resource.String.kompletacja_docexternal)
                            + " "
                            + CurrentDoc?.strNazwaERP
                    );

                    if (
                        Items.Count() > 0
                        && Items.Where(x => x.Status == DocItemStatus.Incomplete).Count() > 0
                    )
                    {
                        OK.SetBackgroundColor(Android.Graphics.Color.Gray);
                        OK.Enabled = false;
                    }
                    else
                    {
                        OK.SetBackgroundColor(Android.Graphics.Color.Blue);
                        OK.Enabled = true;
                    }

                    ScanHint2.Visibility = ViewStates.Gone;
                    KompletacjaView.Visibility = ViewStates.Visible;

                    if (Doc != null)
                    {
                        OK.Visibility = ViewStates.Visible;
                        ItemSum.Visibility = ViewStates.Visible;
                        DocExternal.Visibility = ViewStates.Visible;
                    }
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
                IsBusy = false;
            }
        }

        protected override async void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);

            await RunIsBusyTaskAsync(() => ShowProgressAndDecideOperation(LastScanData));
        }

        async Task ShowProgressAndDecideOperation(List<string> Scanned)
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_searching_barcode));

            await Task.Run(async () =>
            {
                try
                {
                    int IDLok = Globalne
                        .lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(
                            Scanned[0],
                            Globalne.Magazyn.ID,
                            false
                        )
                        .ID; //kuwety

                    // dodano skanowanie dokumnetu po numerze w ERP -> StrERP
                    if (IDLok < 0)
                    {
                        CurrentDoc = Serwer.dokumentBL.PobierzDokument(
                            -1,
                            "",
                            "",
                            -1,
                            -1,
                            Scanned[0]
                        );
                        if (CurrentDoc.ID > 0)
                        {
                            Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(
                                IDLok,
                                Globalne.Operator.ID
                            );

                            await InsertData(CurrentDoc);
                            Helpers.HideProgressDialog();
                            return;
                        }
                    }
                    if (IDLok < 0)
                    {
                        Helpers.HideProgressDialog();
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.kompletacja_didnotfind_locbarcode,
                            Resource.Raw.sound_error
                        );
                        return;
                    }

                    if (IDLok >= 0)
                        CurrentDoc = Serwer.dokumentBL.PobierzDokument(-1, "", "", -1, IDLok, "");
                    else
                        CurrentDoc = Serwer.dokumentBL.PobierzDokument(
                            -1,
                            Scanned[0],
                            "",
                            -1,
                            -1,
                            ""
                        );

                    if (CurrentDoc.ID < 0)
                    {
                        Helpers.HideProgressDialog();
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.kompletacja_didnotfind_barcode,
                            Resource.Raw.sound_error
                        );
                        return;
                    }

                    if (
                        CurrentDoc.intEdytowany >= 0
                        && CurrentDoc.intEdytowany != Globalne.Operator.ID
                    )
                    {
                        Helpers.HideProgressDialog();
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.documents_cantedit_edited,
                            Resource.Raw.sound_error
                        );
                        return;
                    }
                    else
                        Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(
                            IDLok,
                            Globalne.Operator.ID
                        );

                    await InsertData(CurrentDoc);
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
    }

    internal class KompletacjaListAdapter : BaseAdapter<DocumentItemRow>
    {
        public List<DocumentItemRow> Items;
        readonly KompletacjaActivity Ctx;

        public KompletacjaListAdapter(KompletacjaActivity Ctx, List<DocumentItemRow> Items)
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
                Resource.Id.editingdocuments_list_setamount,
                Pos.Base.numIloscZlecona.ToString() + " " + Pos.Base.strNazwaJednostki,
                Set[EditingDocumentsListDisplayElements.DoneAmount]
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
            //view.FindViewById<TextView>(Resource.Id.editingdocuments_list_setamount).Visibility = ViewStates.Gone;
            //view.FindViewById<TextView>(Resource.Id.editingdocuments_list_setamount).Visibility = ViewStates.Gone;

            return view;
        }
    }
}
