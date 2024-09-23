using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Print;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using G_Mobile_Android_WMS.Enums;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;
using Color = System.Drawing.Color;

namespace G_Mobile_Android_WMS.BusinessLogicHelpers
{
    public static class Documents
    {
        public static int CreateDocument(
            Context ctx,
            Enums.DocTypes DocType,
            int RegistryID,
            int ContractorID,
            int SrcFlogID,
            int TarFlogID,
            int TarWarehouse,
            int Buffer,
            int LokDoc,
            string RelatedDoc,
            string Description
        )
        {
            if (RegistryID < 0)
                throw new Exception(
                    ctx.GetString(Resource.String.creating_documents_registry_must_be_set)
                );

            StatusDokumentuO Status = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                Helpers.StringDocType(DocType),
                (int)Enums.DocumentStatusTypes.WRealizacji
            );

            if (Status.ID == -1)
                throw new Exception(ctx.GetString(Resource.String.global_define_statuses));

            DokumentVO Dokument = Serwer.dokumentBL.PustyDokumentVO();
            Dokument.idStatusDokumentu = Status.ID;

            Dokument.intLokalizacjaPozycji = Buffer;
            Dokument.intEdytowany = Globalne.Operator.ID;
            Dokument.intUtworzonyPrzez = Globalne.Operator.ID;
            Dokument.intZmodyfikowanyPrzez = Globalne.Operator.ID;
            Dokument.idRejestr = RegistryID;

            Dokument.intPriorytet = Serwer
                .rejestrBL.PobierzRejestr(Dokument.idRejestr)
                .intPriorytet;
            Dokument.dataDokumentu = Serwer.ogólneBL.GetSQLDate();
            Dokument.bTworzenieNaTerminalu = true;
            Dokument.bDokumentMobilny = true;
            Dokument.bZlecenie = false;
            Dokument.idFunkcjiLogistycznejP = TarFlogID;
            Dokument.idFunkcjiLogistycznejW = SrcFlogID;
            Dokument.strDokumentDostawcy = RelatedDoc;
            Dokument.idKontrahent = ContractorID;
            Dokument.strOpis = Description;
            Dokument.intLokalizacja = LokDoc;

            switch (DocType)
            {
                case Enums.DocTypes.PW:
                case Enums.DocTypes.PZ:
                {
                    Dokument.intMagazynP = Globalne.Magazyn.ID;
                    break;
                }
                case Enums.DocTypes.RW:
                case Enums.DocTypes.WZ:
                {
                    Dokument.intMagazynW = Globalne.Magazyn.ID;
                    break;
                }
                case Enums.DocTypes.ZL:
                case Enums.DocTypes.ZLGathering:
                case Enums.DocTypes.ZLDistribution:
                {
                    Dokument.intMagazynP = Globalne.Magazyn.ID;
                    Dokument.intMagazynW = Globalne.Magazyn.ID;
                    break;
                }
                case Enums.DocTypes.MM:
                case Enums.DocTypes.MMGathering:
                case Enums.DocTypes.MMDistribution:
                {
                    Dokument.intMagazynP = TarWarehouse;
                    Dokument.intMagazynW = Globalne.Magazyn.ID;
                    break;
                }
                default:
                    throw new Exception(
                        ctx.GetString(Resource.String.creating_documents_cannot_create_doc_of_type)
                    );
            }

            int ID = Serwer.dokumentBL.ZróbDokument(Dokument);

            AutoException.ThrowIfNotNull(ctx, ErrorType.DocumentCreationError, ID);

            return ID;
        }

        public static async Task<bool> ShowAndApplyInventoryLocationExitOptions(
            Context ctx,
            DokumentVO Doc,
            int IDInvLoc,
            bool SetClosedWithoutAsking = false
        )
        {
            if (!SetClosedWithoutAsking)
            {
                string[] Options =
                {
                    ctx.GetString(
                        Resource.String.editingdocuments_inventoryloc_leave_without_marking
                    ),
                    ctx.GetString(Resource.String.editingdocuments_inventoryloc_leave_markcomplete),
                };

                string Res = await UserDialogs.Instance.ActionSheetAsync(
                    ctx.GetString(Resource.String.editingdocuments_inventoryloc_leave),
                    ctx.GetString(Resource.String.global_cancel),
                    "",
                    null,
                    Options
                );

                if (Res == ctx.GetString(Resource.String.global_cancel))
                    return false;
                else if (
                    Res
                    == ctx.GetString(
                        Resource.String.editingdocuments_inventoryloc_leave_without_marking
                    )
                )
                {
                    return true;
                }
                else
                {
                    Serwer.dokumentBL.UstawStatusLokInwentaryzacji(IDInvLoc, Doc.ID, true);
                    return true;
                }
            }
            else
            {
                Serwer.dokumentBL.UstawStatusLokInwentaryzacji(IDInvLoc, Doc.ID, true);
                return true;
            }
        }

        public static async Task<bool> ShowAndApplyDocumentExitOptions(
            Context ctx,
            List<DokumentVO> Documents,
            Enums.DocTypes DocType,
            Enums.DocumentLeaveAction Action = DocumentLeaveAction.None,
            bool AskConfirm = true,
            bool Multipicking = false
        )
        {
            string[] przyciski =
            {
                ctx.GetString(Resource.String.editingdocuments_leave_allow_others_to_edit),
                ctx.GetString(Resource.String.editingdocuments_leave_allow_only_me_to_edit),
                ctx.GetString(Resource.String.editingdocuments_leave_closedocument),
            };

            string Res = "";

            // }


            if (Action == DocumentLeaveAction.None)
                Res = await UserDialogs.Instance.ActionSheetAsync(
                    ctx.GetString(Resource.String.editingdocuments_leave),
                    ctx.GetString(Resource.String.global_cancel),
                    "",
                    null,
                    przyciski
                );

            StatusDokumentuO StatusOK = null;
            StatusDokumentuO StatusNOK = null;

            if (Res == ctx.GetString(Resource.String.global_cancel))
                return false;
            else if (
                Res == ctx.GetString(Resource.String.editingdocuments_leave_allow_others_to_edit)
                || Action == DocumentLeaveAction.Leave
            )
            {
                Action = DocumentLeaveAction.Leave;

                if (Globalne.CurrentSettings.StatusesToSetOnDocumentLeave[DocType] >= 0)
                    StatusOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                        Globalne.CurrentSettings.StatusesToSetOnDocumentLeave[DocType]
                    );

                if (StatusOK == null || StatusOK.ID < 0)
                    StatusOK = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                        Helpers.StringDocType(DocType),
                        (int)Enums.DocumentStatusTypes.DoRealizacji
                    );

                if (StatusOK.ID < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.global_define_statuses);
            }
            else if (
                Res == ctx.GetString(Resource.String.editingdocuments_leave_allow_only_me_to_edit)
                || Action == DocumentLeaveAction.Pause
            )
            {
                Action = DocumentLeaveAction.Pause;

                if (Globalne.CurrentSettings.StatusesToSetOnDocumentLeave[DocType] >= 0)
                    StatusOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                        Globalne.CurrentSettings.StatusesToSetOnDocumentPause[DocType]
                    );
                else
                    StatusOK = null;
            }
            else if (
                Res == ctx.GetString(Resource.String.editingdocuments_leave_closedocument)
                || Action == DocumentLeaveAction.Close
            )
            {
                if (AskConfirm)
                {
                    bool Resp = await Helpers.QuestionAlertAsync(
                        ctx,
                        Resource.String.editingdocuments_leave_confirm_close,
                        Resource.Raw.sound_message
                    );

                    if (!Resp)
                        return false;
                    else
                    {
                        // Ustawienia w Desktop



                        // lista wpisanych drukarek
                        var drukarkiEtykiet = Serwer
                            .drukarkaBL.PobierzDrukarkeEtykiet()
                            .strERP.TrimEnd();
                        var drukarkiEtykietList = string.IsNullOrEmpty(drukarkiEtykiet)
                            ? new List<string>()
                            : drukarkiEtykiet.Split(';').ToList();

                        var numeryDrukarkiEtykiet = new List<int>();

                        // dostep do etykiet przez tylko wybranych operatorow
                        var operatorzyEtykiet =
                            Serwer.drukarkaBL.PobierzOperatorowEtykiet().strNazwa ?? "";
                        // jezeli brak wpisow z operatorami uznajemy ze wszyscy moga drukowac etykiety
                        bool czyOperatorPrzypisany = (
                            (
                                !string.IsNullOrEmpty(operatorzyEtykiet)
                                && operatorzyEtykiet.Contains(Globalne.Operator.Login)
                            ) || string.IsNullOrEmpty(operatorzyEtykiet)
                        );

                        // rejestry do wydruku etykiet
                        /// skladnia: rejestr, numer drukarki - RW/WEW,2;PZ/OSO,1
                        var rejestryEtykiet =
                            Serwer.drukarkaBL.PobierzRejestryEtykiet().strNazwa ?? "";
                        var rejestryEtykietList = string.IsNullOrEmpty(rejestryEtykiet)
                            ? new List<string>()
                            : rejestryEtykiet.Split(';').ToList();

                        // pomijamy sprawdzanie rejestrow jezeli nie sa wpisane, tzn umozliwiamy drukowanie etyket na wszystkich mozliwych rejestrach
                        bool czyRejestrZgodnyZDokumentem = rejestryEtykiet.Length == 0;

                        // sprwadzamy czy rejestr dokumentu dopisany jest do konfiguracji
                        foreach (var rejestr in rejestryEtykietList)
                        {
                            // [0] nazwa rejestru
                            var rejestDokumentu = Serwer.rejestrBL.PobierzRejestr(
                                Documents.FirstOrDefault().idRejestr
                            );
                            if (
                                rejestr.Split(',')[0]
                                == rejestDokumentu.strTyp + "/" + rejestDokumentu.strSymbolRej
                            )
                            {
                                czyRejestrZgodnyZDokumentem = true;
                                int numerDrukarki = 0;
                                // [1] numer drukarki
                                if (
                                    rejestr.Split(',')[1] != "*"
                                    && int.TryParse(rejestr.Split(',')[1], out numerDrukarki)
                                )
                                    numeryDrukarkiEtykiet.Add(numerDrukarki);
                                // jezeli "*" to dodajemy wszystkie drukarki
                                else
                                {
                                    int i = 1;
                                    foreach (var item in drukarkiEtykietList)
                                    {
                                        numeryDrukarkiEtykiet.Add(i++);
                                    }
                                }
                            }
                        }

                        DrukarkaO Etykieta = Serwer.drukarkaBL.PowiadomienieEtykiet();

                        //////////////////////////////////////////////////////// DRUKOWANIE ETYKIETY

                        /// Czy włączone drukowanie etykiety
                        if (
                            Etykieta.strNazwa.Contains("1")
                            && czyOperatorPrzypisany
                            && czyRejestrZgodnyZDokumentem
                        )
                        {
                            bool Resp2 = await Helpers.QuestionAlertAsyncEtykieta(
                                ctx,
                                Resource.String.Etykieta,
                                Resource.Raw.sound_message
                            );

                            if (Resp2)
                            {
                                bool PrintIsPossible = await Helpers.DoesPrintPossible(ctx);

                                if (PrintIsPossible)
                                {
                                    try
                                    {
                                        string location = ""; // ?????

                                        // standardowo (jezeli brak konfiguracji) etykieta bedzie sie drukowala na 1 (pierwszej) drukarce
                                        if (numeryDrukarkiEtykiet.Count == 0)
                                            numeryDrukarkiEtykiet.Add(1);

                                        foreach (int numerDrukarki in numeryDrukarkiEtykiet)
                                        {
                                            Serwer.dokumentBL.WydrukEty(
                                                Documents.FirstOrDefault().strNazwa,
                                                location,
                                                numerDrukarki
                                            );
                                        }
                                        //return true;
                                    }
                                    catch (Exception) { }
                                }
                            }
                        }
                    }
                }

                Action = DocumentLeaveAction.Close;

                if (!Multipicking)
                {
                    if (Globalne.CurrentSettings.StatusesToSetOnDocumentFinish[DocType] >= 0)
                        StatusOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                            Globalne.CurrentSettings.StatusesToSetOnDocumentFinish[DocType]
                        );

                    if (StatusOK == null || StatusOK.ID < 0)
                        StatusOK = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                            Helpers.StringDocType(DocType),
                            (int)Enums.DocumentStatusTypes.Zamknięty
                        );
                }
                else if (Multipicking && Globalne.CurrentSettings.MultipickingSetStatusClose)
                {
                    if (Globalne.CurrentSettings.StatusesToSetOnDocumentFinish[DocType] >= 0)
                        StatusOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                            Globalne.CurrentSettings.StatusesToSetOnDocumentFinish[DocType]
                        );

                    if (StatusOK == null || StatusOK.ID < 0)
                        StatusOK = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                            Helpers.StringDocType(DocType),
                            (int)Enums.DocumentStatusTypes.Zamknięty
                        );
                }
                else
                {
                    if (Globalne.CurrentSettings.StatusesToSetOnDocumentFinish[DocType] >= 0)
                        StatusOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                            Globalne.CurrentSettings.StatusesToSetOnDocumentDone[DocType]
                        );

                    if (StatusOK == null || StatusOK.ID < 0)
                        StatusOK = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                            Helpers.StringDocType(DocType),
                            (int)Enums.DocumentStatusTypes.Wykonany
                        );
                }

                if (StatusOK.ID < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.global_define_statuses);

                if (Globalne.CurrentSettings.StatusesToSetOnDocumentFinishIncorrect[DocType] >= 0)
                    StatusNOK = Serwer.dokumentBL.PobierzStatusDokumentu(
                        Globalne.CurrentSettings.StatusesToSetOnDocumentFinishIncorrect[DocType]
                    );

                if (StatusNOK == null || StatusNOK.ID < 0)
                    StatusNOK = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                        Helpers.StringDocType(DocType),
                        (int)Enums.DocumentStatusTypes.Wstrzymany
                    );

                if (StatusNOK.ID < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.global_define_statuses);
            }

            List<int> CannotBeClosed = new List<int>();

            foreach (DokumentVO Doc in Documents)
            {
                switch (Action)
                {
                    case DocumentLeaveAction.Leave:
                    {
                        Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);
                        Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, StatusOK.ID);

                        if (
                            !Doc.bZlecenie
                            && (
                                Doc.intUtworzonyPrzez == Globalne.Operator.ID
                                || Globalne.CurrentUserSettings.CanDeleteAllDocuments
                            )
                        )
                        {
                            // blokowanie usuwania dokumentow IN bez pozycji
                            if (
                                Serwer.dokumentBL.PobierzListęIDPozycji(Doc.ID).Count == 0
                                && DocType != DocTypes.IN
                            )
                            {
                                Serwer.dokumentBL.UsuńDokument(Doc.ID);
                                Helpers.CenteredToast(
                                    ctx,
                                    Resource.String.document_was_empty_and_was_deleted,
                                    ToastLength.Short
                                );
                            }
                        }

                        break;
                    }
                    case DocumentLeaveAction.Pause:
                    {
                        if (StatusOK != null)
                            Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, StatusOK.ID);

                        break;
                    }
                    case DocumentLeaveAction.Close:
                    {
                        if (!Serwer.dokumentBL.SprawdźCzyDokumentMożeZostaćZamknięty(Doc.ID))
                            CannotBeClosed.Add(Doc.ID);

                        if (
                            !Doc.bZlecenie
                            && (
                                Doc.intUtworzonyPrzez == Globalne.Operator.ID
                                || Globalne.CurrentUserSettings.CanDeleteAllDocuments
                            )
                        )
                        {
                            // blokowanie usuwania dokumentow IN bez pozycji
                            if (
                                Serwer.dokumentBL.PobierzListęIDPozycji(Doc.ID).Count == 0
                                && DocType != DocTypes.IN
                            )
                            {
                                Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);
                                Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, StatusOK.ID);
                                Serwer.dokumentBL.UsuńDokument(Doc.ID);
                                Helpers.CenteredToast(
                                    ctx,
                                    Resource.String.document_was_empty_and_was_deleted,
                                    ToastLength.Short
                                );
                            }
                        }

                        break;
                    }
                }
            }

            if (Action == DocumentLeaveAction.Close)
            {
                if (AskConfirm)
                {
                    if (CannotBeClosed.Count != 0)
                    {
                        bool S = await Helpers.QuestionAlertAsync(
                            ctx,
                            ctx.GetString(Resource.String.editingdocuments_leave_incomplete)
                                + " '"
                                + StatusNOK?.strNazwaStatusu
                                + "'?",
                            Resource.Raw.sound_message
                        );

                        if (!S)
                            return false;
                    }
                }

                foreach (DokumentVO Doc in Documents)
                {
                    if (CannotBeClosed.Contains(Doc.ID))
                        Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, StatusNOK.ID);
                    else
                        Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, StatusOK.ID);

                    Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(Doc.ID, -1);

                    //Przy wlaczonej opcji MultipickingAutoKompletacjaAfterFinish usuwamy lokalizacje kuwet
                    //Nastepuje Autokompletacja
                    if (
                        Multipicking
                        && Globalne.CurrentSettings.MultipickingAutoKompletacjaAfterFinish
                    )
                        Serwer.dokumentBL.UstawLokalizacjęDokumentu(Doc.ID, -1);
                }
            }

            return true;
        }

        public static object[] GetDocuments(Context ctx, List<int> DocIDs)
        {
            List<DokumentVO> Documents = new List<DokumentVO>();
            Enums.DocumentStatusTypes Status = Enums.DocumentStatusTypes.Otwarty;

            foreach (int ID in DocIDs)
            {
                DokumentVO Doc = Serwer.dokumentBL.PobierzDokument(ID, "", "", -1, -1, "");

                if (Doc.ID < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_deleted);
                else if (Doc.intEdytowany >= 0 && Doc.intEdytowany != Globalne.Operator.ID)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_edited);
                else if (!Doc.bZlecenie && DocIDs.Count() != 1)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_many_of_this_type
                    );

                int St = Serwer.dokumentBL.PobierzTypStatusuDokumentu(ID, "", "", -1, -1, "");

                if (St < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_deleted);
                else
                    Status = (Enums.DocumentStatusTypes)St;

                if (Status == Enums.DocumentStatusTypes.Otwarty)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_opened_or_closed
                    );

                if (DocIDs.Count() != 1 && Status == Enums.DocumentStatusTypes.Zamknięty)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_many_if_closed
                    );

                Documents.Add(Doc);
            }

            return new object[2] { Status, Documents };
        }

        public static object[] CheckDocuments(Context ctx, List<DokumentVO> Docs)
        {
            Enums.DocumentStatusTypes Status = Enums.DocumentStatusTypes.Otwarty;

            foreach (DokumentVO Doc in Docs)
            {
                if (Doc.ID < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_deleted);
                else if (Doc.intEdytowany >= 0 && Doc.intEdytowany != Globalne.Operator.ID)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_edited);
                else if (!Doc.bZlecenie && Docs.Count() != 1)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_many_of_this_type
                    );

#warning HiveInvoke
                string TypDoc = Serwer.dokumentBL.PobierzTypDokumentu(Doc.ID, "", "", -1, -1, "");

                DocTypes DocType = (DocTypes)Enum.Parse(typeof(DocTypes), TypDoc);

                if (
                    DocType == DocTypes.PW
                    || DocType == DocTypes.PZ
                    || DocType == DocTypes.MM
                    || DocType == DocTypes.ZL
                    || DocType == DocTypes.IN
                )
                {
                    if (Doc.intMagazynP != Globalne.Magazyn.ID)
                        AutoException.ThrowIfNotNull(
                            ctx,
                            Resource.String.documents_wrong_warehouse
                        );
                }

                if (DocType == DocTypes.RW || DocType == DocTypes.WZ)
                {
                    if (Doc.intMagazynW != Globalne.Magazyn.ID)
                        AutoException.ThrowIfNotNull(
                            ctx,
                            Resource.String.documents_wrong_warehouse
                        );
                }

                int St = Serwer.dokumentBL.PobierzTypStatusuDokumentu(Doc.ID, "", "", -1, -1, "");

                if (St < 0)
                    AutoException.ThrowIfNotNull(ctx, Resource.String.documents_cantedit_deleted);
                else
                    Status = (Enums.DocumentStatusTypes)St;

                if (Status == Enums.DocumentStatusTypes.Otwarty)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_opened_or_closed
                    );

                // gdzie logika?
                // metoda GetDocuments nie posiada sprawdzania na status Wstrzymany
                // nie dało sie wejsc w dokument z poziomu skanowania kodu z zamowieniem, ale recznie juz sie dalo wejsc

                // usuniecie Statusu Wstrzymaj
                // if (Docs.Count() != 1 && Status == Enums.DocumentStatusTypes.Zamknięty) || Status == Enums.DocumentStatusTypes.Wstrzymany)
                if (Docs.Count() != 1 && Status == Enums.DocumentStatusTypes.Zamknięty)
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantedit_many_if_closed
                    );
            }

            return new object[2] { Status, Docs };
        }

        public static bool SetDocumentsStatusOnEnter(
            Context ctx,
            List<DokumentVO> Documents,
            Enums.DocTypes DocType
        )
        {
            StatusDokumentuO Status;

            if (Globalne.CurrentSettings.StatusesToSetOnDocumentEnter[DocType] < 0)
                Status = Serwer.dokumentBL.PobierzPierwszyStatusDokumentuOTypie(
                    Helpers.StringDocType(DocType),
                    (int)Enums.DocumentStatusTypes.WRealizacji
                );
            else
                Status = Serwer.dokumentBL.PobierzStatusDokumentu(
                    Globalne.CurrentSettings.StatusesToSetOnDocumentEnter[DocType]
                );

            if (Status.ID < 0)
                AutoException.ThrowIfNotNull(ctx, Resource.String.global_define_statuses);
            else
            {
                foreach (DokumentVO Doc in Documents)
                {
                    Serwer.dokumentBL.UstawOperatoraEdytującegoDokument(
                        Doc.ID,
                        Globalne.Operator.ID
                    );

                    if (
                        Serwer.dokumentBL.PobierzTypStatusuDokumentu(Doc.ID, "", "", -1, -1, "")
                        <= (int)DocumentStatusTypes.Wykonany
                    )
                        Serwer.dokumentBL.UstawStatusDokumentu(Doc.ID, Status.ID);
                }
            }

            return true;
        }

        // Takes a list of int arrays where [0] = ID of the document creator, [1] = ID of the document
        public static async Task<bool> DeleteDocuments(Context ctx, List<int[]> Docs)
        {
            if (Docs.Count == 0)
                return false;
            else if (Docs.Count == 1)
            {
                int IDOfCreator = Docs[0][0];
                int IDDoc = Docs[0][1];

                if (
                    !Globalne.CurrentUserSettings.CanDeleteAllDocuments
                    && (IDOfCreator != Globalne.Operator.ID)
                )
                {
                    AutoException.ThrowIfNotNull(
                        ctx,
                        Resource.String.documents_cantdelete_notyours
                    );
                    return false;
                }
                if (!Globalne.CurrentUserSettings.CanDeleteClosedDocuments)
                {
                    foreach (int[] Doc in Docs)
                    {
                        int Status = Serwer.dokumentBL.PobierzTypStatusuDokumentu(
                            Doc[1],
                            "",
                            "",
                            -1,
                            -1,
                            ""
                        );

                        if (
                            Status == (int)DocumentStatusTypes.Zamknięty
                            || Status == (int)Enums.DocumentStatusTypes.Wstrzymany
                        )
                        {
                            AutoException.ThrowIfNotNull(
                                ctx,
                                Resource.String.documents_cantdelete_closed
                            );
                            return false;
                        }
                    }
                }

                bool Res = await Helpers.QuestionAlertAsync(
                    ctx,
                    Resource.String.documents_delete,
                    Resource.Raw.sound_message
                );

                if (Res)
                {
                    AutoException.ThrowIfNotNull(
                        ctx,
                        ErrorType.DocumentDeletionError,
                        Serwer.dokumentBL.UsuńDokument(IDDoc)
                    );
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (
                    !await Helpers.QuestionAlertAsync(
                        ctx,
                        Resource.String.documents_deletemany,
                        Resource.Raw.sound_message
                    )
                    || !await Helpers.QuestionAlertAsync(
                        ctx,
                        Resource.String.documents_deletemanyconfirm,
                        Resource.Raw.sound_alert
                    )
                )
                    return false;
                else
                {
                    bool ErrorsHappened = false;

                    foreach (int[] Doc in Docs)
                    {
                        int IDOfCreator = Doc[0];
                        int IDDoc = Doc[1];

                        if (
                            !Globalne.CurrentUserSettings.CanDeleteAllDocuments
                            && IDOfCreator == Globalne.Operator.ID
                        )
                            ErrorsHappened = true;
                        else
                        {
                            int Resp = Serwer.dokumentBL.UsuńDokument(IDDoc);

                            if (Resp != 0)
                                ErrorsHappened = true;
                        }
                    }

                    if (ErrorsHappened)
                        await Helpers.AlertAsyncWithConfirm(
                            ctx,
                            Resource.String.documents_cantdelete_many
                        );

                    return true;
                }
            }
        }

        public static async void EditDocuments(
            BaseWMSActivity ctx,
            List<int> DocIds,
            DocTypes DocType,
            ZLMMMode ZLMMMode,
            int InventoryLoc = -1,
            List<DokumentVO> PreDownloadedDocList = null,
            bool Multipicking = false
        )
        {
            if (ctx.IsSwitchingActivity)
                return;

            ctx.IsSwitchingActivity = true;

            object[] Docs = null;

            if (PreDownloadedDocList != null)
                Docs = CheckDocuments(ctx, PreDownloadedDocList);
            else
                Docs = GetDocuments(ctx, DocIds);

            if (Docs == null)
                return;

            SetDocumentsStatusOnEnter(ctx, (List<DokumentVO>)Docs[1], DocType);

            Intent i;

            if (Multipicking && (DocType == DocTypes.WZ || DocType == DocTypes.RW))
            {
                i = new Intent(ctx, typeof(MultipickingActivity));

                i.PutExtra(EditingDocumentsActivity_Common.Vars.DocType, (int)DocType);
                i.PutExtra(EditingDocumentsActivity_Common.Vars.DocStatus, (int)Docs[0]);
                i.PutExtra(
                    EditingDocumentsActivity_Common.Vars.DocsJSON,
                    Helpers.SerializeJSON((List<DokumentVO>)Docs[1])
                );
            }
            else if (DocType == DocTypes.IN && InventoryLoc < 0)
            {
                i = new Intent(ctx, typeof(InventoryLocationsActivity));
                i.PutExtra(
                    InventoryLocationsActivity.Vars.InventoryDoc,
                    Helpers.SerializeJSON((Docs[1] as List<DokumentVO>)[0])
                );
            }
            else
            {
                if (
                    DocType == DocTypes.MM
                    || DocType == DocTypes.ZL
                    || DocType == DocTypes.ZLDistribution
                    || DocType == DocTypes.ZLGathering
                )
                {
                    if (
                        Globalne.CurrentSettings.DefaultZLMode == ZLMMMode.None
                        && ZLMMMode == ZLMMMode.None
                    )
                    {
                        ZLMMMode = await AskZLMMMode(ctx, DocType);
                    }

                    i = new Intent(ctx, typeof(EditingDocumentsActivityZLMM));
                    i.PutExtra(EditingDocumentsActivityZLMM.Vars.Mode, (int)ZLMMMode);
                }
                else
                    i = new Intent(ctx, typeof(EditingDocumentsActivity));

                i.PutExtra(EditingDocumentsActivity_Common.Vars.InventoryLoc, (int)InventoryLoc);
                i.PutExtra(EditingDocumentsActivity_Common.Vars.DocType, (int)DocType);
                i.PutExtra(EditingDocumentsActivity_Common.Vars.DocStatus, (int)Docs[0]);
                i.PutExtra(
                    EditingDocumentsActivity_Common.Vars.DocsJSON,
                    Helpers.SerializeJSON((List<DokumentVO>)Docs[1])
                );
            }

            i.SetFlags(ActivityFlags.NewTask);

            ctx.RunOnUiThread(() =>
            {
                ctx.StartActivity(i);
                ctx.Finish();
            });
        }

        public static async Task<Enums.ZLMMMode> AskZLMMMode(Activity ctx, DocTypes DocType)
        {
            if (DocType == DocTypes.ZL && Globalne.CurrentSettings.DefaultZLMode != ZLMMMode.None)
                return Globalne.CurrentSettings.DefaultZLMode;
            else if (
                DocType == DocTypes.MM
                && Globalne.CurrentSettings.DefaultMMMode != ZLMMMode.None
            )
                return Globalne.CurrentSettings.DefaultMMMode;

            string[] Options =
            {
                ctx.GetString(Resource.String.editingdocuments_zlmm_onestep),
                ctx.GetString(Resource.String.editingdocuments_zlmm_twostep)
            };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.editingdocuments_zlmm_howtoedit),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Options
            );

            if (Res == ctx.GetString(Resource.String.editingdocuments_zlmm_twostep))
                return Enums.ZLMMMode.TwoStep;
            else if (Res == ctx.GetString(Resource.String.editingdocuments_zlmm_onestep))
                return Enums.ZLMMMode.OneStep;
            else
                return Enums.ZLMMMode.None;
        }

        public static async Task<Enums.WZRWMode> AskWZRWMode(Activity ctx, DocTypes DocType)
        {
            if (!Globalne.CurrentSettings.MultipickingSelectDocuments)
                return WZRWMode.AllDocuments;

            string[] Options =
            {
                ctx.GetString(Resource.String.editingdocuments_rwzm_selection),
                ctx.GetString(Resource.String.editingdocuments_rwzm_alldocuments)
            };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.editingdocuments_rwzm_howtoedit),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Options
            );

            if (Res == ctx.GetString(Resource.String.editingdocuments_rwzm_selection))
                return Enums.WZRWMode.Selection;
            else if (Res == ctx.GetString(Resource.String.editingdocuments_rwzm_alldocuments))
                return Enums.WZRWMode.AllDocuments;
            else
                return Enums.WZRWMode.None;
        }

        public static async Task<Enums.WZRWContMode> AskWZRWContinuation(
            Activity ctx,
            DocTypes DocType
        )
        {
            string[] Options =
            {
                ctx.GetString(Resource.String.editingdocuments_rwzm_continue)
                //,ctx.GetString(Resource.String.editingdocuments_rwzm_new)
            };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.editingdocuments_rwzm_howtoedit),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Options
            );

            if (Res == ctx.GetString(Resource.String.editingdocuments_rwzm_continue))
                return Enums.WZRWContMode.ContinueMul;
            else if (Res == ctx.GetString(Resource.String.editingdocuments_rwzm_new))
                return Enums.WZRWContMode.NewMul;
            else
                return Enums.WZRWContMode.None;
        }

        public static async Task<Boolean> AskYesOrNo(Activity ctx, String question)
        {
            string[] Options =
            {
                ctx.GetString(Resource.String.global_tak),
                ctx.GetString(Resource.String.global_nie)
            };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                question,
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Options
            );

            if (Res == ctx.GetString(Resource.String.global_tak))
                return true;
            else
                return false;
        }
    }
}
