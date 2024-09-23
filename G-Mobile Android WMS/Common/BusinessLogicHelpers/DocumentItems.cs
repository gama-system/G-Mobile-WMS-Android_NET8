using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using System.Threading.Tasks;
using WMSServerAccess.Model;
using G_Mobile_Android_WMS.ExtendedModel;
using G_Mobile_Android_WMS.Enums;

namespace G_Mobile_Android_WMS.BusinessLogicHelpers
{
    class DocumentItems
    {
        public static bool IsGatheringMode(DocTypes DocType, Operation Operation)
        {
            return ((DocType == DocTypes.ZLGathering || DocType == DocTypes.MMGathering) || (DocType == DocTypes.ZL && Operation == Operation.Out) || (DocType == DocTypes.MM && Operation == Operation.Out));
        }

        public static bool IsDistributionMode(DocTypes DocType, Operation Operation)
        {
            return ((DocType == DocTypes.ZLDistribution || DocType == DocTypes.MMDistribution) || (DocType == DocTypes.ZL && Operation == Operation.In) || (DocType == DocTypes.MM && Operation == Operation.In));
        }

        public static bool IsZLMM(DocTypes DocType)
        {
            return (DocType == DocTypes.ZL ||
                    DocType == DocTypes.MM ||
                    DocType == DocTypes.ZLDistribution ||
                    DocType == DocTypes.ZLGathering ||
                    DocType == DocTypes.MMDistribution ||
                    DocType == DocTypes.MMGathering);
        }

        public static async Task<bool> DeleteDocumentItems(Context ctx, List<int> DocItems, Enums.DocTypes DocType)
        {
            if (DocItems.Count == 0)
                return false;

            if (DocItems.Count == 1)
            {
                bool Res = await Helpers.QuestionAlertAsync(ctx, Resource.String.editingdocuments_deleteitem_confirm, Resource.Raw.sound_message);

                if (Res)
                {
                    AutoException.ThrowIfNotNull(ctx, ErrorType.ItemDeletionError, Globalne.dokumentBL.UsuńPozycję(Helpers.StringDocType(DocType), DocItems[0]));
                    return true;
                }
                else
                    return false;
            }
            else
            {
                if (!await Helpers.QuestionAlertAsync(ctx, Resource.String.editingdocuments_deleteitem_many, Resource.Raw.sound_message) ||
                    !await Helpers.QuestionAlertAsync(ctx, Resource.String.editingdocuments_deleteitem_many_confirm, Resource.Raw.sound_alert))
                    return false;
                else
                {
                    bool ErrorsHappened = false;

                    foreach (int DocItem in DocItems)
                    {
                        int Resp = Globalne.dokumentBL.UsuńPozycję(Helpers.StringDocType(DocType), DocItem);

                        if (Resp != 0)
                            ErrorsHappened = true;
                    }

                    if (ErrorsHappened)
                        await Helpers.AlertAsyncWithConfirm(ctx, Resource.String.editingdocuments_cannot_deleteitem_many);

                    return true;
                }
            }
        }

        private static void SetProperties(DocumentItemVO Item, KodKreskowyZSzablonuO Kod, Operation Operation)
        {
            if (Kod.TowaryJednostkiWBazie.Count != 0)
            {
                Item.ExIDArticle = Kod.TowaryJednostkiWBazie[0].IDTowaru;
                Item.ExIDUnit = Kod.TowaryJednostkiWBazie[0].IDJednostki;
                Item.ExUnit = Globalne.jednostkaMiaryBL.PobierzJednostkę(Item.ExIDUnit).strNazwa;
            }

            foreach (string Property in Globalne.CurrentSettings.CodeParsing.Keys)
            {
                try
                {
                    string TargetProperty = Globalne.CurrentSettings.CodeParsing[Property];

                    if (TargetProperty == "")
                        continue;

                    switch (TargetProperty)
                    {
                        case nameof(Item.ExIDArticle):
                            {
                                if (Item.ExIDArticle >= 0)
                                    break;

                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                List<int> Value = (List<int>)pi2.GetValue(Kod, null);

                                Item.ExIDArticle = Value[0];
                                break;
                            }
                        case nameof(Item.ExArticle):
                            {
                                if (Item.ExIDArticle >= 0)
                                    break;

                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = (string)pi2.GetValue(Kod, null);

                                if (Value == "")
                                    continue;

                                List<int> IDs = Globalne.towarBL.PobierzIDTowarów(Value);
                                Item.ExIDArticle = IDs.Count == 0 ? -1 : IDs[0];

                                break;
                            }
                        case nameof(Item.ExOwner):
                            {
                                if (Item.ExIDOwner >= 0)
                                    break;

                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = (string)pi2.GetValue(Kod, null);

                                if (Value == "")
                                    continue;

                                KontrahentVO KtrVO = Globalne.podmiotBL.PobierzKontrahentaWgNazwy(Value);

                                if (KtrVO.ID >= 0)
                                {
                                    Item.ExIDOwner = KtrVO.ID;
                                    Item.ExOwner = KtrVO.strNazwa;
                                }
                                else
                                {
                                    KtrVO = Globalne.podmiotBL.PobierzKontrahentaWgSymbolu(Value);

                                    if (KtrVO.ID >= 0)
                                    {
                                        Item.ExIDOwner = KtrVO.ID;
                                        Item.ExOwner = KtrVO.strNazwa;
                                    }
                                    else
                                    {
                                        KtrVO = Globalne.podmiotBL.PobierzKontrahentaWgSymboluERP(Value);

                                        if (KtrVO.ID >= 0)
                                        {
                                            Item.ExIDOwner = KtrVO.ID;
                                            Item.ExOwner = KtrVO.strNazwa;
                                        }
                                    }
                                }

                                break;
                            }
                        case nameof(Item.ExUnit):
                            {
                                if (Item.Base.idJednostkaMiary >= 0)
                                    break;

                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = (string)pi2.GetValue(Kod, null);

                                if (Value == "")
                                    continue;

                                JednostkaMiaryO Jednostka = Globalne.jednostkaMiaryBL.PobierzJednostkę(Value);

                                if (Jednostka.ID >= 0)
                                {
                                    Item.ExIDUnit = Jednostka.ID;
                                    Item.ExUnit = Jednostka.strNazwa;
                                }

                                break;
                            }
                        case nameof(Item.Base.numIloscZrealizowana):
                        case nameof(Item.Base.numIloscZebrana):
                        case nameof(Item.DefaultAmount):
                            {
                                System.Reflection.PropertyInfo pi1 = Item.GetType().GetProperty(TargetProperty);
                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                decimal Value = (decimal)pi2.GetValue(Kod, null);

                                Item.DefaultAmount = Value;

                                break;
                            }
                        case nameof(Item.ExProductionDate):
                        case nameof(Item.ExBestBefore):
                            {
                                System.Reflection.PropertyInfo pi1 = Item.GetType().GetProperty(TargetProperty);
                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                DateTime Value = (DateTime)pi2.GetValue(Kod, null);

                                if (Value.Year > 1900)
                                    pi1.SetValue(Item, Value);

                                break;
                            }
                        case nameof(Item.ExPaletaP):
                        case nameof(Item.ExPaletaW):
                            {
                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = (string)pi2.GetValue(Kod, null);

                                if (Value != "")
                                {
                                    int ID = Globalne.paletaBL.PobierzIDPalety(Value);

                                    if (Operation == Operation.In || Operation == Operation.OutIn)
                                    {
                                        Item.ExIDPaletaP = ID;
                                        Item.ExPaletaP = Value;
                                    }
                                    if (Operation == Operation.Out || Operation == Operation.OutIn)
                                    {
                                        Item.ExIDPaletaW = ID;
                                        Item.ExPaletaW = Value;
                                    }
                                }

                                break;
                            }
                        case nameof(Item.ExPartia):
                            {
                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = (string)pi2.GetValue(Kod, null);

                                if (Value != "")
                                {
                                    int ID = Globalne.partiaBL.PobierzIDPartii(Value);

                                    Item.ExPartia = Value;
                                    Item.ExIDPartia = ID;
                                }

                                break;
                            }
                        default:
                            {
                                System.Reflection.PropertyInfo pi1 = Item.GetType().GetProperty(TargetProperty);

                                if (pi1 == null)
                                    pi1 = Item.Base.GetType().GetProperty(TargetProperty);

                                if (pi1 == null)
                                    break;

                                System.Reflection.PropertyInfo pi2 = Kod.GetType().GetProperty(Property);

                                string Value = pi2.GetValue(Kod, null).ToString();

                                if (Value != "")
                                    pi1.SetValue(Item, Value);

                                break;
                            }
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public static void SetLogisticFunction(Activity Ctx, DokumentVO Doc, ref DocumentItemVO Item)
        {
            try
            {
                if (Item.ExIDFunkcjaLogistycznaP < 0)
                {
                    if (Doc.idFunkcjiLogistycznejP >= 0 && Item.Base.idFunkcjiLogistycznejP < 0)
                        Item.ExIDFunkcjaLogistycznaP = Doc.idFunkcjiLogistycznejP;
                    else if (Item.ExIDFunkcjaLogistycznaP >= 0)
                        Item.ExIDFunkcjaLogistycznaP = Item.Base.idFunkcjiLogistycznejP;
                }

                if (Item.ExIDFunkcjaLogistycznaW < 0)
                {
                    if (Doc.idFunkcjiLogistycznejW >= 0 && Item.Base.idFunkcjiLogistycznejW < 0)
                        Item.ExIDFunkcjaLogistycznaW = Doc.idFunkcjiLogistycznejW;
                    else if (Item.ExIDFunkcjaLogistycznaW >= 0)
                        Item.ExIDFunkcjaLogistycznaW = Item.Base.idFunkcjiLogistycznejW;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(Ctx, ex);
                return;
            }
        }

#warning HiveInvoke
        private static void GetNames(ref DocumentItemVO Item)
        {
            if (Item.ExIDFunkcjaLogistycznaP >= 0 && Item.ExFunkcjaLogistycznaP == "")
                Item.ExFunkcjaLogistycznaP = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL), "PobierzNazwęFunkcjiLogistycznej", Item.ExIDFunkcjaLogistycznaP);

            if (Item.ExIDFunkcjaLogistycznaW >= 0 && Item.ExFunkcjaLogistycznaW == "")
                Item.ExFunkcjaLogistycznaW = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL), "PobierzNazwęFunkcjiLogistycznej", Item.ExIDFunkcjaLogistycznaW);

            if (Item.ExIDLokalizacjaP >= 0 && Item.ExLokalizacjaP == "")
                Item.ExLokalizacjaP = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL), "PobierzNazwęLokalizacji", Item.ExIDLokalizacjaP);

            if (Item.ExIDLokalizacjaW >= 0 && Item.ExLokalizacjaW == "")
                Item.ExLokalizacjaW = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL), "PobierzNazwęLokalizacji", Item.ExIDLokalizacjaW);

            if (Item.ExIDArticle >= 0 && Item.ExArticle == "")
            {
                TowarRow tr = Globalne.towarBL.PobierzTowarRow(Item.ExIDArticle);
                Item.ExArticle = tr.strNazwa;
                Item.ExSymbol = tr.strSymbol;
                Item.ExNrKat = tr.NrKat;
                Item.ExKodEAN = tr.strKod;
            }

            if (Item.ExIDOwner >= 0 && Item.ExOwner == "")
                Item.ExOwner = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Podmiot.PodmiotBL), "PobierzNazwęKontrahenta", Item.ExIDOwner);

            if (Item.ExIDPaletaP >= 0 && Item.ExPaletaP == "")
                Item.ExPaletaP = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Paleta.PaletaBL), "PobierzKodPalety", Item.ExIDPaletaP);

            if (Item.ExIDPaletaW >= 0 && Item.ExPaletaW == "")
                Item.ExPaletaW = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Paleta.PaletaBL), "PobierzKodPalety", Item.ExIDPaletaW);

            if (Item.ExIDPartia >= 0 && Item.ExPartia == "")
                Item.ExPartia = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Partia.PartiaBL), "PobierzKodPartii", Item.ExIDPartia);
        }

        private static void GetDataFromFirstSSCEntry_SQL(ref DocumentItemVO Item, string SSCC, int IDFirstEntry, Operation CurrentOperation)
        {
            ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)Helpers.HiveInvoke(typeof(WMSServerAccess.Ogólne.OgólneBL),
                                                                          "ZapytanieSQL",
                                                                          SQL.DocumentItems.Paleta_GetFirstInData.Replace("<<IDPOZDOK>>", IDFirstEntry.ToString()).Replace("<<PAL>>", SSCC));


            Item.ExIDUnit = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idJednostki]);
            Item.ExUnit = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strNazwaJednostki];
            Item.ExIDPartia = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idPartii]);
            Item.ExPartia = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strPartia];
            Item.ExIDArticle = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idTowaru]);
            Item.ExArticle = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strNazwaTowaru];

            if (CurrentOperation == Operation.In || CurrentOperation == Operation.OutIn)
                Item.ExIDFunkcjaLogistycznaP = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idFunkcjiLogistycznej]);
            if (CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn)
                Item.ExIDFunkcjaLogistycznaW = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idFunkcjiLogistycznej]);

            Item.ExLot = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strLoty];
            Item.ExSerialNum = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strNumerySeryjne];
            Item.ExIDOwner = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idKontrahenta]);
            Item.ExOwner = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strNazwaKtr];
            Item.ExProductionDate = (DateTime)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.dtDataProdukcji];
            Item.ExBestBefore = (DateTime)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.dtDataPrzydatnosci];

            if (CurrentOperation == Operation.In || CurrentOperation == Operation.OutIn)
                Item.ExIDPaletaP = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idPalP]);
            if (CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn)
                Item.ExIDPaletaW = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idPalP]);
        }

        public static void InsertSSCCData(ref KodKreskowyZSzablonuO Kod)
        {
            if (Kod.Paleta != "")
            {
                int IDPozPal = Globalne.paletaBL.PobierzIDPierwszejPozycjiPalety(-1, Kod.Paleta);

                if (IDPozPal >= 0)
                {
                    ZapytanieZTabeliO Zap = (ZapytanieZTabeliO)Helpers.HiveInvoke(typeof(WMSServerAccess.Ogólne.OgólneBL),
                                                                                              "ZapytanieSQL",
                                                                                              SQL.DocumentItems.Paleta_GetFirstInData
                                                                                              .Replace("<<IDPOZDOK>>", IDPozPal.ToString())
                                                                                              .Replace("<<PAL>>", Kod.Paleta));


                    Kod.Partia = (string)Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.strPartia];
                    Kod.TowaryJednostkiWBazie = new List<TowarJednostkaO>();

                    Kod.TowaryJednostkiWBazie.Add(new TowarJednostkaO()
                    {
                        IDTowaru = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idTowaru]),
                        IDJednostki = Convert.ToInt32(Zap.ListaWierszy[0][(int)SQL.DocumentItems.Paleta_GetFirstInData_Results.idJednostki])

                    });
                }
            }
        }

        public static bool GetSSCCData(string Paleta, ref DocumentItemVO Item, Operation CurrentOperation)
        {
            if (Paleta != "")
            {
                int IDPozPal = Globalne.paletaBL.PobierzIDPierwszejPozycjiPalety(-1, Paleta);

                if (IDPozPal >= 0)
                {
                    GetDataFromFirstSSCEntry_SQL(ref Item, Paleta, IDPozPal, CurrentOperation);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        public static bool GetSSCCData(KodKreskowyZSzablonuO Kod, ref DocumentItemVO Item, Operation CurrentOperation)
        {
            if (Kod.Paleta != "")
            {
                int IDPozPal = Globalne.paletaBL.PobierzIDPierwszejPozycjiPalety(-1, Kod.Paleta);

                if (IDPozPal >= 0)
                {
                    GetDataFromFirstSSCEntry_SQL(ref Item, Kod.Paleta, IDPozPal, CurrentOperation);
                    Item.DefaultAmount = Kod.Ilość;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        private static decimal GetInWarehouse_OnItem_OutOperation(DokumentVO Doc, DocumentItemVO Item, bool skipSearch = false)
        {
            //if (Globalne.CurrentSettings.OneInsteadOfSetAmount)
                if (skipSearch)
                    return Item.Base.numIloscZlecona;
            else
            {
                decimal Stan = Globalne.przychrozchBL.PobierzStanTowaruWJednostce(Item.ExIDArticle,
                                                                                  Doc.intMagazynW,
                                                                                  Item.ExIDLokalizacjaW,
                                                                                  Item.ExIDPartia,
                                                                                  Item.ExPartia,
                                                                                  Item.ExIDPaletaW,
                                                                                  Item.ExPaletaW,
                                                                                  Item.ExIDFunkcjaLogistycznaW,
                                                                                  Item.ExIDUnit,
                                                                                  Item.ExIDNumerSeryjny);

                if (Item.Base.numIloscZlecona == 0)
                    return Stan;
                else
                    return Math.Min(Stan, Item.Base.numIloscZlecona);
            }
        }

        public static void ProposeLocations(ref DokumentVO Doc, ref DocumentItemVO Item, Operation CurrentOperation, DefaultLocType DefLocType)
        {
            if ((CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn) && (DefLocType == DefaultLocType.In || DefLocType == DefaultLocType.None))
            {
                if (Item.Base.idLokalizacjaW < 0 && Item.ExIDArticle >= 0)
                {
                    PodpowiedźLokalizacjiO Pdp = Globalne.przychrozchBL.ZaproponujLokalizacjęDlaRozchodu_Nazwa_PozycjaNaŚć(Item.ExIDArticle, Doc.intMagazynW,
                                                                                                                           Item.ExIDPartia, Item.ExPartia,
                                                                                                                           Item.ExIDPaletaW, Item.ExPaletaW,
                                                                                                                           Item.ExIDFunkcjaLogistycznaW,
                                                                                                                           new List<int>(), "", -1);

                    Item.ExLokalizacjaW = Pdp.strNazwa;
                    Item.ExIDLokalizacjaW = Pdp.ID;
                }
                else
                    Item.ExIDLokalizacjaW = Item.Base.idLokalizacjaW;
            }
            if ((CurrentOperation == Operation.In || CurrentOperation == Operation.OutIn) && (DefLocType == DefaultLocType.Out || DefLocType == DefaultLocType.None))
            {
                if (Item.Base.idLokalizacjaP < 0 && Item.ExIDArticle >= 0)
                {
                    PodpowiedźLokalizacjiO Pdp = Globalne.przychrozchBL.ZaproponujLokalizacjęDlaPrzychodu_Nazwa_PozycjaNaŚć(Item.ExIDArticle,
                                                                                                                            Doc.intMagazynP,
                                                                                                                            new List<int>() { Item.ExIDLokalizacjaW },
                                                                                                                            -1,
                                                                                                                            Item.ExIDUnit,
                                                                                                                            Item.Base.numIloscZlecona);

                    Item.ExLokalizacjaP = Pdp.strNazwa;
                    Item.ExIDLokalizacjaP = Pdp.ID;
                }
                else
                    Item.ExIDLokalizacjaP = Item.Base.idLokalizacjaP;
            }
        }

        public static void SetLocations(ref Intent i, ref DokumentVO Doc, ref DocumentItemVO Item, Operation CurrentOperation,
                                        int SelectedDefaultLoc, string SelectedDefaultLocName, DefaultLocType DefLocType)
        {
            // Jeśli ustawiona jest domyślna lokalizacja, albo domyślna lokalizacja dokumentu, włączamy tryb bufora.
            if (SelectedDefaultLoc >= 0 || Doc.intLokalizacjaPozycji >= 0)
                i.PutExtra(DocumentItemActivity_Common.Vars.BufferSet, true);
            else
                i.PutExtra(DocumentItemActivity_Common.Vars.BufferSet, false);

            Item.ExIDLokalizacjaP = Item.Base.idLokalizacjaP;
            Item.ExIDLokalizacjaW = Item.Base.idLokalizacjaW;


            // Warunek na przyjęciach
            bool WarunekP = (Item.Base.idLokalizacjaP < 0 && (CurrentOperation == Operation.In || CurrentOperation == Operation.OutIn) && (DefLocType == DefaultLocType.In || DefLocType == DefaultLocType.None));

            // Warunek na rozchodach
            bool WarunekR = (Item.Base.idLokalizacjaW < 0 && (CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn) && (DefLocType == DefaultLocType.Out || DefLocType == DefaultLocType.None));

            // Jeśli nie jest włączona obsługa lokalizaji, pobierz ID lokalizacji głównej dla magazynu i ustaw ją.
            if (!Globalne.CurrentSettings.Lokalizacje)
            {
                if (WarunekP)
                {
                    int IDDef = Globalne.lokalizacjaBL.PobierzIDLokalizacjiPodstawowejDlaMagazynu(Doc.intMagazynP);
                    string NazwaDef = Globalne.lokalizacjaBL.PobierzLokalizację(IDDef).strNazwa;

                    Item.ExLokalizacjaP = NazwaDef;
                    Item.ExIDLokalizacjaP = IDDef;
                }

                if (WarunekR)
                {
                    int IDDef = Globalne.lokalizacjaBL.PobierzIDLokalizacjiPodstawowejDlaMagazynu(Doc.intMagazynW);
                    string NazwaDef = Globalne.lokalizacjaBL.PobierzLokalizację(IDDef).strNazwa;

                    Item.ExLokalizacjaP = NazwaDef;
                    Item.ExIDLokalizacjaP = IDDef;
                }

                return;
            }

            // Jeśli ustawiona jest tymczasowa domyślna lokalizacja, a nie jest ustawiona lokalizacja w samej pozycji, ustaw lokalizację domyślną
            // jako wybraną według przypisanego typu. Jeśli przypisany typ lokalizacji domyślnej to "None", przypisz obie. Zaproponuj lokalizację drugiego typu.
            if (SelectedDefaultLoc >= 0)
            {
                if (WarunekP)
                {
                    Item.ExLokalizacjaP = SelectedDefaultLocName;
                    Item.ExIDLokalizacjaP = SelectedDefaultLoc;
                }

                if (WarunekR)
                {
                    Item.ExLokalizacjaW = SelectedDefaultLocName;
                    Item.ExIDLokalizacjaW = SelectedDefaultLoc;
                }

                if (DefLocType != DefaultLocType.None)
                    ProposeLocations(ref Doc, ref Item, CurrentOperation, DefLocType);

            }
            // Jeśli ustawiona jest domyślna lokalizacja dokumentu, a nie jest ustawiona lokalizacja w samej pozycji, ustaw lokalizację domyślną dokumentu
            // jako wybraną według przypisanego typu. Jeśli przypisany typ lokalizacji domyślnej to "None", przypisz obie. Zaproponuj lokalizację drugiego typu.
            else if (Doc.intLokalizacjaPozycji >= 0)
            {
                if (WarunekP)
                    Item.ExIDLokalizacjaP = Doc.intLokalizacjaPozycji;

                if (WarunekR)
                    Item.ExIDLokalizacjaW = Doc.intLokalizacjaPozycji;

                if (DefLocType != DefaultLocType.None)
                    ProposeLocations(ref Doc, ref Item, CurrentOperation, DefLocType);
            }
            else
            {
                // Jeśli nie są ustawione domyślna lokalizacja ani domyślna lokalizacja dokumentu ani lokalizacja w samej pozycji, wstaw lokalizację proponowaną.
                // Najpierw ustalana jest lokalizacja wyjścia, żeby w razie potrzeb wysłać jej ID z zapytaniem o lokalizację przyjścia - dzięki temu
                // zostanie podpowiedziana inna lokalizacja przychodu na dokumentach ZL/MM.
                // Jeśli lokalizacja jest ustawiona w samej pozycji, wstaw ją jako proponowaną.
                // Dla dokumentów jednokrokowego ZL/MM ustawiane są i lokalizacja W i lokalizacja P.
                ProposeLocations(ref Doc, ref Item, CurrentOperation, DefaultLocType.None);
            }
        }

        public static async void EditItem(BaseWMSActivity ctx, List<DokumentVO> Documents, DocumentItemRow SelectedItem,
                                    DocTypes DocType, Operation CurrentOperation, int SelectedDefaultLoc, DefaultLocType DefLocType,
                                    string SelectedDefaultLocName, int ResultCode, bool Add, bool FromScanner, KodKreskowyZSzablonuO Kod = null)
        {
            Helpers.ShowProgressDialog(ctx.GetString(Resource.String.editing_documents_opening));
            await Task.Delay(Globalne.TaskDelay);

            if (DocType == DocTypes.MM || DocType == DocTypes.ZL || IsGatheringMode(DocType, CurrentOperation) || IsDistributionMode(DocType, CurrentOperation))
                EditItemZLMM(ctx, Documents, SelectedItem, DocType, CurrentOperation, SelectedDefaultLoc, DefLocType, SelectedDefaultLocName, ResultCode, Add, FromScanner, Kod);
            else
                EditItemPWPZRWWZIN(ctx, Documents, SelectedItem, DocType, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, ResultCode, Add, FromScanner, Kod);

            Helpers.HideProgressDialog();
        }

        public static void EditItemPWPZRWWZIN(BaseWMSActivity ctx, List<DokumentVO> Documents, DocumentItemRow SelectedItem,
                                            DocTypes DocType, Operation CurrentOperation, int SelectedDefaultLoc,
                                            string SelectedDefaultLocName, int ResultCode, bool Add, bool FromScanner, KodKreskowyZSzablonuO Kod = null)
        {
            try
            {
                if (ctx.IsSwitchingActivity)
                    return;

                ctx.IsSwitchingActivity = true;

                DocumentItemVO EItem = new DocumentItemVO(SelectedItem);
                DokumentVO Doc = Documents.Find(x => x.ID == EItem.Base.idDokumentu);

                // Jeśli mamy podany zeskanowany kod, uzupełniamy dane według niego, chyba że wpisane zostało IDSSCC oraz ustawione jest pobieranie danych z SSCC.
                if (Kod != null)
                {
                    if (Kod.Paleta != "" && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry && Globalne.CurrentSettings.Palety)
                    {
                        if (!GetSSCCData(Kod, ref EItem, CurrentOperation))
                            SetProperties(EItem, Kod, CurrentOperation);
                    }
                    else
                        SetProperties(EItem, Kod, CurrentOperation);
                }

                Intent i = new Intent(ctx, typeof(DocumentItemActivity));

                i.PutExtra(DocumentItemActivity_Common.Vars.DocType, (int)DocType);
                i.PutExtra(DocumentItemActivity_Common.Vars.DocJSON, Helpers.SerializeJSON(Doc));

                // Wstaw lokalizacje
                SetLocations(ref i, ref Doc, ref EItem, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, DefaultLocType.None);
                // Wstaw funkcję logistyczną
                SetLogisticFunction(ctx, Doc, ref EItem);

                // Ustaw tryb działania. Jeśli dokument jest zleceniem a ilość zlecona jest większa niż zrealizowana, ustawiamy tryb rozbijania. Jeśli są takie same, edycji.
                // Jeśli dokument nie jest zleceniem, ustawiamy edytowanie, chyba że użytkownik kliknął przycisk "zwiększ".
                ItemActivityMode Mode = ItemActivityMode.None;

                if (Doc.bZlecenie)
                {
                    if (EItem.Base.numIloscZlecona > EItem.Base.numIloscZrealizowana)
                        Mode = ItemActivityMode.Split;
                    else
                        Mode = ItemActivityMode.Edit;
                }
                else
                    Mode = Add ? ItemActivityMode.EditAdd : ItemActivityMode.Edit;

                // Ustawiamy domyślną ilość.
                // Jeśli włączony jest tryb zwiększania, ustawiamy 1.
                // Jeśli ilość zrealizowana jest równa 0, dla dokumentów przychodu wpisujemy maksymalną ilość możliwą do wstawienia do lokalizacji - lub ilość zleconą jeśli ta jest mniejsza
                // jeśli jednak jest to rozchód, sprawdzamy najpierw czy w podpowiedzianej lokalizacji jest wystarczająca liczba towaru. Jeśli nie, wpisujemy tą ilość, która jest.
                // Jeśli ilość zrealizowana nie jest równa 0, to wpisujemy ilość zrealizowaną jako domyślną do edycji.
                if (EItem.DefaultAmount == 0)
                {
                    if (Add)
                        EItem.DefaultAmount = 1;
                    else
                    {
                        if (EItem.Base.numIloscZrealizowana <= 0)
                        {
                            if (CurrentOperation == Operation.In)
                            {
                                EItem.DefaultAmount = Math.Min(
                                                                Globalne.przychrozchBL.ObliczIleCałkTowaruZmieściSięWLokalizacji(EItem.ExIDArticle, EItem.ExIDUnit, EItem.ExIDLokalizacjaP),
                                                                EItem.Base.numIloscZlecona
                                                               );
                                if (EItem.DefaultAmount == -1)
                                    EItem.DefaultAmount = GetDefaultValueByDocType(Doc, DocType, EItem, true);
                            }
                            else
                            {
                                EItem.DefaultAmount = GetDefaultValueByDocType(Doc, DocType, EItem);
                            }

                        }
                        else
                            EItem.DefaultAmount = EItem.Base.numIloscZrealizowana;
                    }
                }

                // Pobieramy nazwy wszędzie gdzie ich nie ma.
                GetNames(ref EItem);

                i.PutExtra(DocumentItemActivity_Common.Vars.ItemJSON, Helpers.SerializeJSON(EItem));
                i.PutExtra(DocumentItemActivity_Common.Vars.Mode, (int)Mode);
                i.PutExtra(DocumentItemActivity_Common.Vars.Operation, (int)CurrentOperation);
                i.PutExtra(DocumentItemActivity_Common.Vars.FromScanner, FromScanner);

                if (Globalne.CurrentSettings.InstantScanning[DocType] && (FromScanner || Mode == ItemActivityMode.EditAdd))
                    i.PutExtra(Globalne.SkipScanner, true);

                ctx.RunOnUiThread(() => ctx.StartActivityForResult(i, ResultCode));
            }
            catch (Exception ex)
            {
                Helpers.HandleError(ctx, ex);
                ctx.IsSwitchingActivity = false;
            }
        }

        public static void EditItemZLMM(BaseWMSActivity ctx, List<DokumentVO> Documents, DocumentItemRow SelectedItem,
                                        DocTypes DocType, Operation CurrentOperation, int SelectedDefaultLoc, Enums.DefaultLocType DefLocType,
                                        string SelectedDefaultLocName, int ResultCode, bool Add, bool FromScanner, KodKreskowyZSzablonuO Kod = null)
        {
            try
            {
                if (ctx.IsSwitchingActivity)
                    return;

                ctx.IsSwitchingActivity = true;

                DocumentItemVO EItem = new DocumentItemVO(SelectedItem);
                DokumentVO Doc = Documents.Find(x => x.ID == EItem.Base.idDokumentu);

                // Jeśli jest to tryb roznoszenia, przepisujemy dane ze zbiórki.
                if (IsDistributionMode(DocType, CurrentOperation))
                {
                    EItem.ExIDFunkcjaLogistycznaP = EItem.Base.idFunkcjiLogistycznejW;
                    EItem.ExIDPaletaP = EItem.Base.idPaletaW;
                }
                // Jeśli nie jest to jednak tryb roznoszenia (gdyż wtedy mamy już wszystko w pozycji wpisane, a mamy podany zeskanowany kod uzupełniamy dane według niego. 
                // Jeśli mamy podany zeskanowany kod, uzupełniamy dane według niego, chyba że wpisane zostało IDSSCC oraz ustawione jest pobieranie danych z SSCC.
                else if (Kod != null)
                {
                    if (Kod.Paleta != "" && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry && Globalne.CurrentSettings.Palety)
                    {
                        if (!GetSSCCData(Kod, ref EItem, CurrentOperation))
                            SetProperties(EItem, Kod, CurrentOperation);
                    }
                    else
                        SetProperties(EItem, Kod, CurrentOperation);
                }

                Intent i;

                // Jeśli operacja jest typu OutIn to wiemy, że jest to jednokrokowy ZL/MM, więc potrzebna nam jest inna activity
                if (CurrentOperation == Operation.OutIn)
                {
                    i = new Intent(ctx, typeof(DocumentItemActivityOneStep_ZLMM));
                    i.PutExtra(DocumentItemActivity_Common.Vars.BufferType, (int)DefLocType);
                }
                else
                    i = new Intent(ctx, typeof(DocumentItemActivity));

                i.PutExtra(DocumentItemActivity_Common.Vars.DocType, (int)DocType);
                i.PutExtra(DocumentItemActivity_Common.Vars.DocJSON, Helpers.SerializeJSON(Doc));

                // Wstaw lokalizacje
                SetLocations(ref i, ref Doc, ref EItem, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, DefLocType);
                // Wstaw funkcję logistyczną
                SetLogisticFunction(ctx, Doc, ref EItem);

                // Ustawiamy tryb pozycji
                ItemActivityMode Mode = ItemActivityMode.None;

                // Jeśli jest to zlecenie lub roznoszenie, sprawdzamy czy ilość nadrzędna jest równa ilości podrzędnej i jeśli nie są równe, ustawiamy tryb rozbijania pozycji.
                if (Doc.bZlecenie || IsDistributionMode(DocType, CurrentOperation))
                {
                    // Dla trybu zbiórki (czyli tylko na zleceniach), nadrzędna jest ilość zlecona, a podrzędna zebrana.
                    if (IsGatheringMode(DocType, CurrentOperation))
                    {
                        if (EItem.Base.numIloscZlecona > EItem.Base.numIloscZebrana)
                            Mode = ItemActivityMode.Split;
                        else
                            Mode = ItemActivityMode.Edit;
                    }
                    // Dla trybu roznoszenia, ilość nadrzędna to ilość zebrana, a podrzędna zrealizowana.
                    else if (IsDistributionMode(DocType, CurrentOperation))
                    {
                        if (EItem.Base.numIloscZebrana > EItem.Base.numIloscZrealizowana)
                            Mode = ItemActivityMode.Split;
                        else
                            Mode = ItemActivityMode.Edit;
                    }
                    // Dla trybu jednokrokowego nadrzędna jest ilość zlecona, a podrzędna zrealizowana.
                    else if (CurrentOperation == Operation.OutIn)
                    {
                        if (EItem.Base.numIloscZlecona > EItem.Base.numIloscZrealizowana)
                            Mode = ItemActivityMode.Split;
                        else
                            Mode = ItemActivityMode.Edit;
                    }
                }
                else
                {
                    // Jeśli nie jest to zlecenie, ale jest tryb zbiórki albo jednokrokowy, to znaczenie ma czy operator wybrał zwiększanie czy nie.
                    if (IsGatheringMode(DocType, CurrentOperation) || CurrentOperation == Operation.OutIn)
                        Mode = Add ? ItemActivityMode.EditAdd : ItemActivityMode.Edit;
                    // Failsafe. Nigdy nie powinno do tego momentu dojść.
                    else
                        Mode = ItemActivityMode.Edit;
                }

                // Ustawiamy ilość domyślną...
                if (EItem.DefaultAmount == 0)
                {
                    // Jeśli jest to tryb zwiększania ustawiamy 1.
                    if (Add)
                    {
                        EItem.DefaultAmount = 1;
                    }
                    else
                    {
                        // Jeśli jest to zbiórka albo tryb jednokrokowy, traktujemy to jako rozchód.
                        if (IsGatheringMode(DocType, CurrentOperation) || CurrentOperation == Operation.OutIn)
                        {
                            // Jeśli nic jeszcze nie zebrano...
                            if (EItem.Base.numIloscZebrana <= 0)
                            {
                                // Jeśli jest to zlecenie, sprawdzamy ilość w podpowiedzianej lokalizacji i wpisujemy mniejszą pomiędzy zleconą a tą na stanie...
                           //     if (Doc.bZlecenie)
                           //         EItem.DefaultAmount = GetInWarehouse_OnItem_OutOperation(Doc, EItem);
                                // Jeśli nie jest to zlecenie, to wpisujemy 1.
                           //     else
                                    EItem.DefaultAmount = GetDefaultValueByDocType(Doc, DocType, EItem);
                            }
                            // Jeśli ilośc zebrana to nie jest 0, a jesteśmy w trybie zbiórki lub roznoszenia, to wpisujemy ilośc zebraną do tej pory do edycji.
                            else if (CurrentOperation != Operation.OutIn)
                                EItem.DefaultAmount = EItem.Base.numIloscZebrana;
                            // Jesteśmy w trybie jednokrokowym, więc wpisujemy ilośc zrealizowaną do edycji.
                            else
                                EItem.DefaultAmount = EItem.Base.numIloscZrealizowana;
                        }
                        // Jesteśmy na roznoszeniu...
                        else if (IsDistributionMode(DocType, CurrentOperation))
                        {
                            // Jeśli nic z tego roznoszenia jeszcze nie wykonano, to wpisujemy maksymalną ilość możliwą do włożenia do lokalizacji (lub ilość zebraną, jeśli ta jest mniejsza). 
                            // Jeśli było, wpisujemy ilość zrealizowaną do edycji.
                            if (EItem.Base.numIloscZrealizowana <= 0)
                            {
                                EItem.DefaultAmount = Math.Min(
                                                                Globalne.przychrozchBL.ObliczIleCałkTowaruZmieściSięWLokalizacji(EItem.ExIDArticle, EItem.ExIDUnit, EItem.ExIDLokalizacjaP),
                                                                EItem.Base.numIloscZebrana
                                                               );
                                if(EItem.DefaultAmount == -1)
                                    EItem.DefaultAmount = GetDefaultValueByDocType(Doc, DocType, EItem, true);
                            }
                            else
                                EItem.DefaultAmount = EItem.Base.numIloscZrealizowana;
                        }
                        // Failsafe. Do tego momentu nie powinno nigdy dojść.
                        else if (CurrentOperation == Operation.Out)
                        {
                            EItem.DefaultAmount = GetInWarehouse_OnItem_OutOperation(Doc, EItem);
                        }
                    }
                }

                // Pobieramy nazwy wszędzie gdzie ich nie ma.
                GetNames(ref EItem);

                i.PutExtra(DocumentItemActivity_Common.Vars.ItemJSON, Helpers.SerializeJSON(EItem));
                i.PutExtra(DocumentItemActivity_Common.Vars.Mode, (int)Mode);
                i.PutExtra(DocumentItemActivity_Common.Vars.Operation, (int)CurrentOperation);
                i.PutExtra(DocumentItemActivity_Common.Vars.FromScanner, FromScanner);

                if (Globalne.CurrentSettings.InstantScanning[DocType] && (FromScanner || Mode == ItemActivityMode.EditAdd))
                    i.PutExtra(Globalne.SkipScanner, true);

                ctx.RunOnUiThread(() => ctx.StartActivityForResult(i, ResultCode));
            }
            catch (Exception ex)
            {
                Helpers.HandleError(ctx, ex);
                ctx.IsSwitchingActivity = false;
            }
        }

        public static void AddItem(BaseWMSActivity ctx, DokumentVO Doc, DocTypes DocType, Operation CurrentOperation,
                                   int SelectedDefaultLoc, string SelectedDefaultLocName, DefaultLocType DefLocType, int ResultCode, bool FromScanner, KodKreskowyZSzablonuO Kod = null)
        {
            try
            {
                if (ctx.IsSwitchingActivity)
                    return;

                ctx.IsSwitchingActivity = true;

                DocumentItemVO Item = new DocumentItemVO(Globalne.dokumentBL.PustaPozycjaVO());

                Intent i;

                // Jeśli operacja jest typu OutIn to wiemy, że jest to jednokrokowy ZL/MM, więc potrzebna nam jest inna activity
                if (CurrentOperation == Operation.OutIn)
                {
                    i = new Intent(ctx, typeof(DocumentItemActivityOneStep_ZLMM));
                    i.PutExtra(DocumentItemActivity_Common.Vars.BufferType, (int)DefLocType);
                }
                else
                    i = new Intent(ctx, typeof(DocumentItemActivity));

                i.PutExtra(DocumentItemActivity_Common.Vars.DocType, (int)DocType);
                i.PutExtra(DocumentItemActivity_Common.Vars.DocJSON, Helpers.SerializeJSON(Doc));

                // Ustawiamy domyślne daty produkcji i przydatności.
                Item.ExProductionDate = Helpers.GetDefaultDateForField(true, Item.ExProductionDate);
                Item.ExBestBefore = Helpers.GetDefaultDateForField(true, Item.ExBestBefore);

                // Jeśli mamy podany zeskanowany kod, uzupełniamy dane według niego, chyba że wpisane zostało IDSSCC oraz ustawione jest pobieranie danych z SSCC.
                if (Kod != null)
                {
                    if (Kod.Paleta != "" && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry && Globalne.CurrentSettings.Palety)
                    {
                        if (!GetSSCCData(Kod, ref Item, CurrentOperation))
                            SetProperties(Item, Kod, CurrentOperation);
                    }
                    else
                        SetProperties(Item, Kod, CurrentOperation);
                }

                // Jeśli po zassaniu z kodu wartości wstawiono ID artykułu a nie jest ustawiona jednostka, ustawiamy domyślną jednostkę miary.
                // Teoretycznie nie powinno się to nigdy stać
                if (Item.ExIDArticle >= 0)
                {
                    if (Item.ExIDUnit <= 0)
                    {
                        JednostkaMiaryO Jedn = Globalne.towarBL.PobierzJednostkęDomyślnąTowaru(Item.ExIDArticle);
                        Item.ExIDUnit = Jedn.ID;
                        Item.ExUnit = Jedn.strNazwa;
                    }
                }

                // Ustawiamy domyślną ilość jeśli nie jest już wcześniej wpisana.
                // Jeśli jest to rozchód bądź przeniesienie jednokrokowe, a mamy pobrać dane SSCC, wstawiamy tu stan towaru.
                if (Item.DefaultAmount == 0)
                {
                    if ((CurrentOperation == Operation.Out || CurrentOperation == Operation.OutIn) && Globalne.CurrentSettings.GetDataFromFirstSSCCEntry)
                        Item.DefaultAmount = GetInWarehouse_OnItem_OutOperation(Doc, Item);
                    else
                    {
                        Item.DefaultAmount = GetDefaultValueByDocType(Doc, DocType, Item, true);
                    }
                }

                // Ustawiamy lokalizacje
                SetLocations(ref i, ref Doc, ref Item, CurrentOperation, SelectedDefaultLoc, SelectedDefaultLocName, DefLocType);
                // Ustawiamy funkcję logistyczną
                SetLogisticFunction(ctx, Doc, ref Item);

                // Pobieramy nazwy wszędzie gdzie ich nie ma.
                GetNames(ref Item);

                i.PutExtra(DocumentItemActivity_Common.Vars.ItemJSON, Helpers.SerializeJSON(Item));
                i.PutExtra(DocumentItemActivity_Common.Vars.Mode, (int)Enums.ItemActivityMode.Create);
                i.PutExtra(DocumentItemActivity_Common.Vars.Operation, (int)CurrentOperation);
                i.PutExtra(DocumentItemActivity_Common.Vars.FromScanner, FromScanner);

                if (Globalne.CurrentSettings.InstantScanning[DocType] && FromScanner)
                    i.PutExtra(Globalne.SkipScanner, true);

                ctx.RunOnUiThread(() => ctx.StartActivityForResult(i, ResultCode));
            }
            catch (Exception ex)
            {
                Helpers.HandleError(ctx, ex);
                ctx.IsSwitchingActivity = false;
            }
        }

        private static decimal GetDefaultValueByDocType(DokumentVO Doc, DocTypes DocType, DocumentItemVO itemVO, bool skipSearchInLocation = false)
        {
            decimal numIloscZlecona = 0;

            switch (DocType)
            {
                case DocTypes.PW:
                    if (Doc.bZlecenie)
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnOrderDocPW == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnOrderDocPW;
                    else
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnDocPW == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnDocPW;
                    break;
                case DocTypes.RW:
                    if (Doc.bZlecenie)
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnOrderDocRW == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnOrderDocRW;
                    else
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnDocRW == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1: GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnDocRW;
                    break;
                case DocTypes.PZ:
                    if (Doc.bZlecenie)
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnOrderDocPZ == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnOrderDocPZ;
                    else
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnDocPZ == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnDocPZ;
                    break;
                case DocTypes.WZ:
                    if (Doc.bZlecenie)
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnOrderDocWZ == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnOrderDocWZ;
                    else
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnDocWZ == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnDocWZ;
                    break;
                case DocTypes.MM:
                case DocTypes.MMGathering:
                case DocTypes.MMDistribution:
                    break;
                case DocTypes.ZL:
                case DocTypes.ZLGathering:
                case DocTypes.ZLDistribution:
                    if (Doc.bZlecenie)
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnOrderDocZL == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnOrderDocZL;
                    else
                        numIloscZlecona = Globalne.CurrentSettings.DefaultValueOnDocZL == -1 ? (itemVO.Base.numIloscZlecona == 0 ? 1 : GetInWarehouse_OnItem_OutOperation(Doc, itemVO, skipSearchInLocation)) : Globalne.CurrentSettings.DefaultValueOnDocZL;
                    break;
                case DocTypes.IN:
                    break;
                case DocTypes.Error:
                    break;
                default:
                    break;
            }
            return numIloscZlecona;
        }

        public static void EditSplitItem(Activity ctx, PozycjaVO Original, bool Change, DocTypes DocType, Operation Operation, decimal Amount = -1)
        {
            PozycjaVO NewItem;

            if (Change)
            {
                NewItem = (PozycjaVO)Helpers.ObjectCopy(Original, typeof(PozycjaVO));

                NewItem.intZmodyfikowanyPrzez = Globalne.Operator.ID;

                if (BusinessLogicHelpers.DocumentItems.IsGatheringMode(DocType, Operation))
                    NewItem.numIloscZlecona = Original.numIloscZlecona - Amount;
                else if (Operation == Operation.OutIn || BusinessLogicHelpers.DocumentItems.IsDistributionMode(DocType, Operation))
                {
                    NewItem.numIloscZlecona = Original.numIloscZlecona - Amount;
                    NewItem.numIloscZebrana = Original.numIloscZebrana - Amount < 0 ? 0 : Original.numIloscZebrana - Amount;
                    NewItem.numIloscZrealizowana = 0;
                }
                else
                {
                    NewItem.numIloscZlecona = Original.numIloscZlecona - Amount;
                    NewItem.numIloscZrealizowana = 0;
                }
            }
            else
                NewItem = Original;

            NewItem.bezposrednieZL = Operation == Operation.OutIn ? true : false;

            if (NewItem.numIloscZlecona == 0)
                Globalne.dokumentBL.UsuńPozycję(Helpers.StringDocType(DocType), Original);
            else
            {
                PozycjaVO Poz = Globalne.dokumentBL.PobierzPozycję(NewItem.ID);

                if (Poz.ID == -1)
                    AutoException.ThrowIfNotNull(ctx, ErrorType.ItemCreationError, Globalne.dokumentBL.ZróbPozycję(Helpers.StringDocType(DocType), NewItem, true));
                else
                    AutoException.ThrowIfNotNull(ctx, ErrorType.ItemEditError, Globalne.dokumentBL.EdytujPozycję(Helpers.StringDocType(DocType), NewItem, true));
            }
            return;
        }
    }
}