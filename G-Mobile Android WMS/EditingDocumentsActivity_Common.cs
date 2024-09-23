using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    public static class EditingDocumentsActivity_Common
    {
        public enum ResultCodes
        {
            LocationsActivityResult = 10,
            DocumentItemActivityResult = 20,
        }

        internal static class Vars
        {
            public const string DocType = "DocType";
            public const string DocStatus = "DocStatus";
            public const string DocsJSON = "DocsJSON";
            public const string InventoryLoc = "InventoryLoc";
        }

        public static void ChangeDefaultLocDialog(BaseWMSActivity Ctx, int IDMag)
        {
            Ctx.RunIsBusyAction(() =>
            {
                Ctx.IsSwitchingActivity = true;
                Intent i = new Intent(Ctx.ApplicationContext, typeof(LocationsActivity));
                i.PutExtra(LocationsActivity.Vars.AskOnStart, false);
                i.PutExtra(LocationsActivity.Vars.Bufor, true);
                i.PutExtra(LocationsActivity.Vars.IDMagazynu, IDMag);

                Ctx.RunOnUiThread(
                    () => Ctx.StartActivityForResult(i, (int)ResultCodes.LocationsActivityResult)
                );
            });
        }

        public static void ShowLocDialog(BaseWMSActivity Ctx, int idTow)
        {
            //Intent i = new Intent(Ctx, typeof(StocksActivity));
            //i.PutExtra("IDTowaru", idTow);
            //i.PutExtra(ArticlesActivity.Vars.AskOnStart, false);
            //Ctx.StartActivityForResult(i, (int)StocksActivity.ResultCodes.ArticlesActivityResult);

            Ctx.RunIsBusyAction(() =>
            {
                Ctx.IsSwitchingActivity = true;
                Intent i = new Intent(Ctx.ApplicationContext, typeof(StocksActivity));
                i.PutExtra("IDTowaru", idTow);
                i.PutExtra(ArticlesActivity.Vars.AskOnStart, false);

                Ctx.RunOnUiThread(
                    () =>
                        Ctx.StartActivityForResult(
                            i,
                            (int)StocksActivity.ResultCodes.ArticlesActivityResult
                        )
                );
            });
        }

        public static void SetView(View view, int ResId, string Value, bool Condition)
        {
            TextView View = view.FindViewById<TextView>(ResId);
            if (View != null)
            {
                if (Condition)
                    View.Text = Value;
                else
                    View.Visibility = ViewStates.Gone;
            }
        }

        public static DocItemStatus GetStatusForItem(
            PozycjaRow R,
            DocTypes DocType,
            ZLMMMode Mode,
            Operation CurrentOperation
        )
        {
            if (
                Mode == ZLMMMode.OneStep
                || !DocumentItems.IsGatheringMode(DocType, CurrentOperation)
                || !DocumentItems.IsDistributionMode(DocType, CurrentOperation)
            )
            {
                if (R.numIloscZlecona == R.numIloscZrealizowana)
                    return DocItemStatus.Complete;
                else if (R.numIloscZrealizowana > R.numIloscZlecona)
                    return DocItemStatus.Over;
                else
                    return DocItemStatus.Incomplete;
            }
            else
            {
                if (CurrentOperation == Enums.Operation.In)
                {
                    if (R.numIloscZebrana == R.numIloscZrealizowana)
                        return DocItemStatus.Complete;
                    else if (R.numIloscZrealizowana > R.numIloscZebrana)
                        return DocItemStatus.Over;
                    else
                        return DocItemStatus.Incomplete;
                }
                else
                {
                    if (R.numIloscZebrana == R.numIloscZlecona)
                        return DocItemStatus.Complete;
                    else if (R.numIloscZebrana > R.numIloscZlecona)
                        return DocItemStatus.Over;
                    else
                        return DocItemStatus.Incomplete;
                }
            }
        }

        public static List<DocumentItemRow> GetData(
            List<DokumentVO> Documents,
            DocTypes DocType,
            ZLMMMode Mode,
            Operation CurrentOperation,
            int SelectedDefaultLoc,
            DefaultLocType DefLocType,
            int InventoryLoc = -1,
            string OutType = "",
            List<int> SkipLocs = null,
            int LowestPathID = -1
        )
        {
            ConcurrentBag<DocumentItemRow> Items = new ConcurrentBag<DocumentItemRow>();

            foreach (DokumentVO Doc in Documents)
            {
                if (
                    Doc.bZlecenie
                    || (Mode == ZLMMMode.TwoStep && CurrentOperation == Operation.In)
                    || Mode == ZLMMMode.OneStep
                    || DocType == DocTypes.IN
                )
                {
                    bool? DefLoc = null;

                    if (DefLocType == DefaultLocType.In)
                        DefLoc = true;
                    else
                        DefLoc = false;

                    // decyduja czy maja byc podpowiadane/pobierane lokalizacje dla przychodu i rozchodu
                    bool lokPrzychodu = (
                        (CurrentOperation == Operation.In || CurrentOperation == Operation.OutIn)
                        && !(
                            DocType == DocTypes.ZLDistribution
                            && !Globalne.CurrentSettings.LocationPositionsSuggestedZL
                        )
                    );

                    bool lokRozchodu = (
                        DocumentItems.IsDistributionMode(DocType, CurrentOperation)
                        || CurrentOperation == Operation.Out
                        || CurrentOperation == Operation.OutIn
                    );

                    List<PozycjaRowZPodpowiedzią> Res =
                        Serwer.dokumentBL.PobierzPozycjeIZaproponujLokalizacjeDlaDokumentu(
                            Doc.ID,
                            lokPrzychodu,
                            lokRozchodu,
                            SelectedDefaultLoc,
                            DefLoc,
                            InventoryLoc,
                            DocumentItems.IsDistributionMode(DocType, CurrentOperation),
                            OutType,
                            SkipLocs == null ? new List<int>() : SkipLocs,
                            LowestPathID
                        );
                    Parallel.ForEach(
                        Res,
                        Pzp =>
                        {
                            DocumentItemRow DocItem = new DocumentItemRow(Pzp.Pozycja)
                            {
                                Status = GetStatusForItem(
                                    Pzp.Pozycja,
                                    DocType,
                                    Mode,
                                    CurrentOperation
                                )
                            };

                            if (Pzp.PodpowiedźPrzychód != null)
                            {
                                DocItem.ExIDLokalizacjaP = Pzp.PodpowiedźPrzychód.ID;
                                DocItem.ExLokalizacjaP = Pzp.PodpowiedźPrzychód.strNazwa;
                                DocItem.KolejnośćNaŚcieżce =
                                    Pzp.PodpowiedźPrzychód.intPozycjaNaŚcieżce;
                            }
                            if (Pzp.PodpowiedźRozchód != null)
                            {
                                DocItem.ExIDLokalizacjaW = Pzp.PodpowiedźRozchód.ID;
                                DocItem.ExLokalizacjaW = Pzp.PodpowiedźRozchód.strNazwa;
                                DocItem.KolejnośćNaŚcieżce =
                                    Pzp.PodpowiedźRozchód.intPozycjaNaŚcieżce;
                            }

                            DocItem.EXIDLokalizacjaDokumentu = Doc.intLokalizacja;
                            Items.Add(DocItem);
                        }
                    );
                }
                else
                {
                    List<PozycjaRow> Pozycje = Serwer.dokumentBL.PobierzListęPozycjiRow(Doc.ID);

                    Parallel.ForEach(
                        Pozycje,
                        R =>
                        {
                            DocumentItemRow DocItem = new DocumentItemRow(R)
                            {
                                Status = GetStatusForItem(R, DocType, Mode, CurrentOperation)
                            };

                            if (
                                CurrentOperation == Operation.In
                                || CurrentOperation == Operation.OutIn
                            )
                            {
                                DocItem.ExIDLokalizacjaP = DocItem.Base.idLokalizacjaP;
                                DocItem.ExLokalizacjaP = DocItem.Base.strLokalizacjaP;
                            }
                            if (
                                CurrentOperation == Operation.Out
                                || CurrentOperation == Operation.OutIn
                            )
                            {
                                DocItem.ExIDLokalizacjaW = DocItem.Base.idLokalizacjaW;
                                DocItem.ExLokalizacjaW = DocItem.Base.strLokalizacjaW;
                            }

                            DocItem.KolejnośćNaŚcieżce = 0;
                            DocItem.EXIDLokalizacjaDokumentu = Doc.intLokalizacja;
                            Items.Add(DocItem);
                        }
                    );
                }
            }
            var docs = Items
                .OrderBy(x => x.Status)
                .ThenBy(x => x.KolejnośćNaŚcieżce)
                .ThenBy(x => x.EXIDLokalizacjaDokumentu)
                .ToList();

            return docs;
        }
    }
}
