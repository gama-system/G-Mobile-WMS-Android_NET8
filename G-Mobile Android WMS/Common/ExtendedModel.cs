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
using G_Mobile_Android_WMS.Enums;
using Newtonsoft.Json;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS.ExtendedModel
{
    public class DocumentItemRow
    {
        public PozycjaRow Base { get; set; }

        public int KolejnośćNaŚcieżce { get; set; }
        public int ExIDLokalizacjaW { get; set; }
        public string ExLokalizacjaW { get; set; }

        public int EXIDLokalizacjaDokumentu { get; set; }
        public int ExIDLokalizacjaP { get; set; }
        public string ExLokalizacjaP { get; set; }
        public int ExIDPaletaP { get; set; }
        public string ExPaletaP { get; set; }
        public int ExIDPaletaW { get; set; }
        public string ExPaletaW { get; set; }
        public int ExIDPartia { get; set; }
        public string ExPartia { get; set; }
        public int ExIDFunkcjaLogistycznaP { get; set; }
        public string ExFunkcjaLogistycznaP { get; set; }
        public int ExIDFunkcjaLogistycznaW { get; set; }
        public string ExFunkcjaLogistycznaW { get; set; }

        public string ExSymbol { get; set; }

        public string ExKODEAN { get; set; }

        public string ExNrKat { get; set; }

        public int ExIDNumerSeryjny { get; set; }
        public string ExNumerSeryjny { get; set; }

        public DocItemStatus Status { get; set; }

        public DocumentItemRow() { }

        public DocumentItemRow(PozycjaRow R)
        {
            Base = R;
            Status = DocItemStatus.Incomplete;
            KolejnośćNaŚcieżce = -1;
            ExIDLokalizacjaW = R.idLokalizacjaW;
            ExLokalizacjaW = R.strLokalizacjaW;
            ExIDLokalizacjaP = R.idLokalizacjaP;
            ExLokalizacjaP = R.strLokalizacjaP;
            ExIDPaletaP = R.idPaletaP;
            ExPaletaP = R.strPaletaP;
            ExIDPaletaW = R.idPaletaW;
            ExPaletaW = R.strPaletaW;
            ExIDPartia = R.idPartia;
            ExPartia = R.strPartia;
            ExIDFunkcjaLogistycznaP = R.idFunkcjiLogistycznejP;
            ExFunkcjaLogistycznaP = R.strFunkcjiLogistycznejP;
            ExIDFunkcjaLogistycznaW = R.idFunkcjiLogistycznejW;
            ExFunkcjaLogistycznaW = R.strFunkcjiLogistycznejW;
            ExSymbol = R.strSymbolTowaru;
            ExKODEAN = R.kodean;
            ExNrKat = R.NrKat;
            ExNumerSeryjny = R.strNumerySeryjne;
            EXIDLokalizacjaDokumentu = -1;
        }

        public DocumentItemRow(
            PozycjaRow R,
            DocItemStatus Stat,
            int Kolejność,
            int ExIDLokW,
            int ExIDLokP,
            string ExLokW,
            string ExLokP,
            int ExIDPalP,
            string ExPalP,
            int ExIDPalW,
            string ExPalW,
            int ExIDPar,
            string ExPar,
            int ExIDFlogW,
            string ExFlogW,
            int ExIDFlogP,
            string ExFlogP,
            string Exkod,
            string ExNrKat
        )
        {
            Base = R;
            Status = Stat;
            KolejnośćNaŚcieżce = Kolejność;
            ExIDLokalizacjaW = ExIDLokW;
            ExLokalizacjaW = ExLokW;
            ExIDLokalizacjaP = ExIDLokP;
            ExLokalizacjaP = ExLokP;
            ExIDPaletaP = ExIDPalP;
            ExPaletaP = ExPalP;
            ExIDPaletaW = ExIDPalW;
            ExPaletaW = ExPalW;
            ExIDPartia = ExIDPar;
            ExPartia = ExPar;
            ExIDFunkcjaLogistycznaP = ExIDFlogP;
            ExFunkcjaLogistycznaP = ExFlogP;
            ExIDFunkcjaLogistycznaW = ExIDFlogW;
            ExFunkcjaLogistycznaW = ExFlogW;
            // ExKODEAN = Exkod;
        }
    }

    public class DocumentItemVO
    {
        public PozycjaVO Base { get; set; }

        public int ExIDLokalizacjaW { get; set; }
        public string ExLokalizacjaW { get; set; }
        public int ExIDLokalizacjaP { get; set; }
        public string ExLokalizacjaP { get; set; }
        public int ExIDPaletaP { get; set; }
        public string ExPaletaP { get; set; }
        public int ExIDPaletaW { get; set; }
        public string ExPaletaW { get; set; }
        public int ExIDPartia { get; set; }
        public string ExPartia { get; set; }
        public int ExIDFunkcjaLogistycznaW { get; set; }
        public string ExFunkcjaLogistycznaW { get; set; }
        public int ExIDFunkcjaLogistycznaP { get; set; }
        public string ExFunkcjaLogistycznaP { get; set; }
        public int ExIDArticle { get; set; }
        public string ExArticle { get; set; }
        public DateTime ExProductionDate { get; set; }
        public DateTime ExBestBefore { get; set; }
        public int ExIDOwner { get; set; }
        public string ExOwner { get; set; }
        public string ExLot { get; set; }
        public string ExSerialNum { get; set; }
        public int ExIDUnit { get; set; }
        public string ExUnit { get; set; }
        public string ExKodEAN { get; set; }

        public string ExNrKat { get; set; }
        public ItemActivityMode EditMode { get; set; }

        public decimal DefaultAmount { get; set; }

        public string ExSymbol { get; set; }
        public int ExIDNumerSeryjny { get; set; }
        public string ExNumerSeryjny { get; set; }

        public DocumentItemVO() { }

        public DocumentItemVO(PozycjaVO V)
        {
            Base = V;
            ExIDLokalizacjaW = -1;
            ExLokalizacjaW = "";
            ExIDLokalizacjaP = -1;
            ExLokalizacjaP = "";
            ExIDPaletaP = -1;
            ExPaletaP = "";
            ExIDPaletaW = -1;
            ExPaletaW = "";
            ExIDPartia = -1;
            ExPartia = "";
            ExIDFunkcjaLogistycznaP = -1;
            ExFunkcjaLogistycznaP = "";
            ExIDFunkcjaLogistycznaW = -1;
            ExFunkcjaLogistycznaW = "";
            ExProductionDate = new DateTime(2999, 12, 31);
            ExBestBefore = new DateTime(2999, 12, 31);
            ExIDOwner = -1;
            ExOwner = "";
            ExLot = "";
            ExSerialNum = "";
            ExIDArticle = -1;
            ExArticle = "";
            ExUnit = "";
            ExIDUnit = -1;
            DefaultAmount = 0;
            ExKodEAN = "";
            ExNrKat = "";
            ExSymbol = "";
            ExIDNumerSeryjny = -1;
            ExNumerSeryjny = "";
        }

        public DocumentItemVO(
            PozycjaVO V,
            int ExIDLokW,
            int ExIDLokP,
            string ExLokW,
            string ExLokP,
            int ExIDPalP,
            string ExPalP,
            int ExIDPalW,
            string ExPalW,
            int ExIDPar,
            string ExPar,
            int _ExIDUnit,
            string _ExUnit,
            int ExIDFlogP,
            string ExFlogP,
            int ExIDFlogW,
            string ExFlogW,
            DateTime ExDtProd,
            DateTime ExDtBestBefore,
            int _ExIDOwner,
            string _ExOwner,
            string _ExLot,
            string _ExSerialNum,
            string _ExArticle,
            int ExIdArt,
            string _Exkod,
            string _ExNrKat,
            string _ExSymbol,
            int _ExIDNumerSeryjny,
            string _ExNumerSeryjny
        )
        {
            Base = V;
            ExIDLokalizacjaW = ExIDLokW;
            ExLokalizacjaW = ExLokW;
            ExIDLokalizacjaP = ExIDLokP;
            ExLokalizacjaP = ExLokP;
            ExIDPaletaP = ExIDPalP;
            ExPaletaP = ExPalP;
            ExIDPaletaW = ExIDPalW;
            ExPaletaW = ExPalW;
            ExIDPartia = ExIDPar;
            ExPartia = ExPar;
            ExIDFunkcjaLogistycznaP = ExIDFlogP;
            ExFunkcjaLogistycznaP = ExFlogP;
            ExIDFunkcjaLogistycznaP = ExIDFlogW;
            ExFunkcjaLogistycznaP = ExFlogW;
            ExProductionDate = ExDtProd;
            ExBestBefore = ExDtBestBefore;
            ExIDOwner = _ExIDOwner;
            ExOwner = _ExOwner;
            ExLot = _ExLot;
            ExSerialNum = _ExSerialNum;
            ExArticle = _ExArticle;
            ExIDArticle = ExIdArt;
            ExIDUnit = _ExIDUnit;
            ExUnit = _ExUnit;
            ExKodEAN = _Exkod;
            ExNrKat = _ExNrKat;
            DefaultAmount = 0;
            ExSymbol = _ExSymbol;
            ExIDNumerSeryjny = _ExIDNumerSeryjny;
            ExNumerSeryjny = _ExNumerSeryjny;
        }

        public DocumentItemVO(DocumentItemRow R)
        {
            try
            {
                Base = Serwer.dokumentBL.PobierzPozycję(R.Base.ID);

                ExIDLokalizacjaW = R.ExIDLokalizacjaW;
                ExLokalizacjaW = R.ExLokalizacjaW;
                ExIDLokalizacjaP = R.ExIDLokalizacjaP;
                ExLokalizacjaP = R.ExLokalizacjaP;

                ExIDPaletaP = R.ExIDPaletaP;
                ExPaletaP = R.ExPaletaP;
                ExIDPaletaW = R.ExIDPaletaW;
                ExPaletaW = R.ExPaletaW;
                ExIDPartia = R.ExIDPartia;
                ExPartia = R.ExPartia;
                ExIDFunkcjaLogistycznaP = R.ExIDFunkcjaLogistycznaP;
                ExFunkcjaLogistycznaP = R.ExFunkcjaLogistycznaP;
                ExIDFunkcjaLogistycznaW = R.ExIDFunkcjaLogistycznaW;
                ExFunkcjaLogistycznaW = R.ExFunkcjaLogistycznaW;

                //ExArticle = R.Base.strSymbolTowaru + " - " + R.Base.strNazwaTowaru;
                ExArticle = R.Base.strNazwaTowaru;
                ExSymbol = R.Base.strSymbolTowaru;

                ExIDArticle = R.Base.idTowaru;
                ExUnit = R.Base.strNazwaJednostki;
                ExIDUnit = R.Base.idJednostkaMiary;

                ExLot = R.Base.strLoty;
                ExKodEAN = R.Base.kodean;
                ExNrKat = R.Base.NrKat;
                ExSerialNum = R.Base.strNumerySeryjne;
                ExProductionDate = R.Base.dtDataProdukcji;
                ExBestBefore = R.Base.dtDataPrzydatności;
                ExIDOwner = R.Base.idKontrahent;
                ExOwner = Serwer.podmiotBL.PobierzNazwęKontrahenta(R.Base.idKontrahent);
                DefaultAmount = 0;
                ExNumerSeryjny = R.Base.strNumerySeryjne;
            }
            catch (Exception)
            {
                Base = null;
            }
        }

        public DocumentItemVO(
            DocumentItemRow R,
            DateTime ExDtProd,
            DateTime ExDtBestBefore,
            int _ExIDOwner,
            string _ExOwner,
            string _ExLot,
            string _ExSerialNum,
            string _Exkod,
            string _ExNrKat,
            int _ExIDNumerSeryjny,
            string _ExNumerSeryjny
        )
        {
            try
            {
                Base = Serwer.dokumentBL.PobierzPozycję(R.Base.ID);

                ExIDLokalizacjaW = R.ExIDLokalizacjaW;
                ExLokalizacjaW = R.ExLokalizacjaW;
                ExIDLokalizacjaP = R.ExIDLokalizacjaP;
                ExLokalizacjaP = R.ExLokalizacjaP;

                ExIDPaletaP = R.ExIDPaletaP;
                ExPaletaP = R.ExPaletaP;
                ExIDPaletaW = R.ExIDPaletaW;
                ExPaletaW = R.ExPaletaW;
                ExIDPartia = R.ExIDPartia;
                ExPartia = R.ExPartia;
                ExIDFunkcjaLogistycznaP = R.ExIDFunkcjaLogistycznaP;
                ExFunkcjaLogistycznaP = R.ExFunkcjaLogistycznaP;
                ExIDFunkcjaLogistycznaW = R.ExIDFunkcjaLogistycznaW;
                ExFunkcjaLogistycznaW = R.ExFunkcjaLogistycznaW;

                //ExArticle = R.Base.strSymbolTowaru + " - " + R.Base.strNazwaTowaru;
                ExArticle = R.Base.strNazwaTowaru;
                ExSymbol = R.Base.strSymbolTowaru;

                ExIDArticle = R.Base.idTowaru;
                ExUnit = R.Base.strNazwaJednostki;
                ExIDUnit = R.Base.idJednostkaMiary;

                ExLot = _ExLot;
                ExSerialNum = _ExSerialNum;
                ExKodEAN = _Exkod;
                ExNrKat = _ExNrKat;
                ExProductionDate = ExDtProd;
                ExBestBefore = ExDtBestBefore;
                ExIDOwner = _ExIDOwner;
                ExOwner = _ExOwner;
                DefaultAmount = 0;
                ExIDNumerSeryjny = _ExIDNumerSeryjny;
                ExNumerSeryjny = _ExNumerSeryjny;
            }
            catch (Exception ex)
            {
                Base = null;
            }
        }
    }

    public class DefaultLocation
    {
        public int IDLoc { get; set; }
        public string LocName { get; set; }
        public DefaultLocType Type { get; set; }

        public DefaultLocation()
        {
            IDLoc = -1;
            LocName = "";
            Type = DefaultLocType.None;
        }
    }
}
