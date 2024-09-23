using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace G_Mobile_Android_WMS.Enums
{
    public static class BarcodeOrder
    {
        public const int Template = -1;
        public const int GS1 = -2;
        public const int Article = -3;
        public const int Partia = -4;
        public const int Paleta = -5;
        public const int SerialNum = -6;
        public const int Lot = -7;
        public const int Amount = -8;
 
        public static string GetBarcodeOrderName(Activity ctx, int Order)
        {
            switch (Order)
            {
                case BarcodeOrder.Template: return ctx.GetString(Resource.String.barcodeorder_templateany);
                case BarcodeOrder.GS1: return ctx.GetString(Resource.String.barcodeorder_gs1);
                case BarcodeOrder.Article: return ctx.GetString(Resource.String.barcodeorder_article);
                case BarcodeOrder.Partia: return ctx.GetString(Resource.String.barcodeorder_partia);
                case BarcodeOrder.Paleta: return ctx.GetString(Resource.String.barcodeorder_paleta); 
                case BarcodeOrder.Lot: return ctx.GetString(Resource.String.barcodeorder_lot);
                case BarcodeOrder.SerialNum: return ctx.GetString(Resource.String.barcodeorder_serialnr);
                default: break;
            }

            return "";
        }
    }

    public enum DocumentLeaveAction
    {
        None,
        Leave,
        Pause,
        Close,
    }

    public enum ZLMMMode
    {
        [Description("Jako zbiórka i roznoszenie")]
        TwoStep,
        [Description("Bezpośrednio")]
        OneStep,
        [Description("Do wyboru")]
        None
    }
    public enum WZRWMode
    {
        [Description("Selekcja poszególnych dokumentów")]
        Selection,
        [Description("Wszytkie dokumenty dostępne")]
        AllDocuments,
        [Description("Do wyboru")]
        None
    }

    public enum WZRWContMode
    {
        [Description("Utwórz nowy multipicking")]
        NewMul,
        [Description("Kontynuj poprzedni multipicking")]
        ContinueMul,
        [Description("Nie wymagane")]
        None
    }

    public enum DefaultLocType
    {
        In,
        Out,
        None
    }

    public enum DefaultDateType
    {
        Today = 0,
        Max = 1,
        Value = 2,
        AddToToday = 3,
    }

    public enum Ustawienia
    {
        ObsługaPartii = 1,
        ObsługaFunkcjiLogistycznych = 2,
        MiejscDziesiętnychPoPrzecinkuWIlościach = 3,
        SposóbWydawki = 5,
        EdycjaZamkniętychDokumentów = 8,
        StylLogowania = 9,
        Skalowanie = 10,
        SposóbPodpowiadaniaLokalizacji = 11,
        ZezwalajNaPowtórzoneKodyKreskowe = 15,
        WersjaBazy = 16,
        OgraniczenieIlościRekordów = 17,
        ObsługaPalet = 18,
        NumeracjaDokumentów = 24,
        KonfiguracjaWinCE = 25,
        KonfiguracjaAndroid = 26,
        ObsługaLokalizacji = 27,
    }

    public enum DocumentItemFields
    {
        [Description("Symbol")]
        Symbol,
        [Description("Artykuł")]
        Article,
        [Description("Partia")]
        Partia,
        [Description("Paleta")]
        Paleta,
        [Description("Paleta docelowa")]
        PaletaIn,
        [Description("Paleta źródłowa")]
        PaletaOut,
        [Description("Numer seryjny")]
        SerialNumber,
        [Description("Lot")]
        Lot,
        [Description("Funkcja logistyczna")]
        Flog,
        [Description("Doc. f. logistyczna")]
        FlogIn,
        [Description("Źr. f. logistyczna")]
        FlogOut,
        [Description("Właściciel")]
        Owner,
        [Description("Lokalizacja")]
        Location,
        [Description("Lokalizacja docelowa")]
        LocationIn,
        [Description("Lokalizacja źródłowa")]
        LocationOut,
        [Description("Jednostka")]
        Unit,
        [Description("DataProdukcji")]
        DataProdukcji,
        [Description("DataPrzydatności")]
        DataPrzydatności,
        [Description("Kod EAN")]
        KodEAN,
        [Description("Nr Katalogowy")]
        NrKat,

    }

    public enum DocumentStatusTypes
    {
        Otwarty = 0,
        DoRealizacji = 1,
        WRealizacji = 2,
        Wykonany = 3,
        Wstrzymany = 4,
        Zamknięty = 5
    }

    public enum DocItemStatus
    {
        Incomplete,
        Complete,
        Over
    }

    public enum DeviceTypes
    {
        Zebra,
        Other,
        Newland
    }

    public enum GlobalResultCodes
    {
        ScannerActivityResult = Int16.MaxValue
    }

    public enum Modules
    {
        [Description("PW")]
        PW,
        [Description("RW")]
        RW,
        [Description("PZ")]
        PZ,
        [Description("WZ")]
        WZ,
        [Description("MM")]
        MM,
        [Description("ZL")]
        ZL,
        [Description("IN")]
        IN,
        [Description("STAN")]
        STAN,
        [Description("KOMPLETACJA")]
        KOMPLETACJA,
    }

    public enum DocTypes
    {
        [Description("PW")]
        PW,
        [Description("RW")]
        RW,
        [Description("PZ")]
        PZ,
        [Description("WZ")]
        WZ,
        [Description("MM")]
        MM,
        [Description("MM - Zbiórka")]
        MMGathering,
        [Description("MM - Roznoszenie")]
        MMDistribution,
        [Description("ZL")]
        ZL,
        [Description("ZL - Zbiórka")]
        ZLGathering,
        [Description("ZL - Roznoszenie")]
        ZLDistribution,
        [Description("IN")]
        IN,
        [Description("Błąd")]
        Error
    }

    public enum DocumentFields
    {
        [Description("Rejestr")]
        Registry,
        [Description("Kontrahent")]
        Contractor,
        [Description("Magazyn docelowy")]
        TargetWarehouse,
        [Description("Źr. funkcja logistyczna")]
        SourceFlog,
        [Description("Doc. funkcja logistyczna")]
        TargetFlog,
        [Description("Dokument powiązany")]
        RelatedDoc,
        [Description("Opis")]
        Description
    }

    public enum EditingDocumentsListDisplayElements
    {
        [Description("Symbol")]
        Symbol,
        [Description("Nazwa artykułu")]
        ArticleName,
        [Description("Partia")]
        Partia,
        [Description("Paleta")]
        Paleta,
        [Description("Paleta docelowa")]
        PaletaIn,
        [Description("Paleta źródłowa")]
        PaletaOut,
        [Description("Funkcja logistyczna")]
        Flog,
        [Description("Źr. funkcja logistyczna")]
        FlogIn,
        [Description("Doc. funkcja logistyczna")]
        FlogOut,
        [Description("Numer seryjny")]
        SerialNumber,
        [Description("Data produkcji")]
        ProductionDate,
        [Description("Data przydatności")]
        BestBefore,
        [Description("Lokalizacja")]
        Location,
        [Description("Lokalizacja docelowa")]
        LocationIn,
        [Description("Lokalizacja źródłowa")]
        LocationOut,
        [Description("Ilość zlecona")]
        SetAmount,
        [Description("Ilość wykonana")]
        DoneAmount,
        [Description("Ilość zebrana")]
        GotAmount,
        [Description("Loty")]
        Lot,
        [Description("Kod EAN")]
        KodEAN,
        [Description("Nr Katalogowy")]
        NrKat,
    }


    public enum DocumentItemDisplayElements
    {
        [Description("Symbol")]
        Symbol,
        [Description("Nazwa artykułu")]
        ArticleName,
        [Description("Partia")]
        Partia,
        [Description("Paleta")]
        Paleta,
        [Description("Funkcja logistyczna")]
        Flog,
        [Description("Numer seryjny")]
        SerialNumber,
        [Description("Data produkcji")]
        ProductionDate,
        [Description("Data przydatności")]
        BestBefore,
        [Description("Lokalizacja")]
        Location,
        [Description("Na dokumencie")]
        OnDoc,
        [Description("Stan na magazynie")]
        InWarehouse,
        [Description("Ilość zlecona")]
        OrderedAmount,
        [Description("Ilość wykonana")]
        Amount,
        [Description("Lot")]
        Lot,
        [Description("Właściciel")]
        Owner,
        [Description("Jednostka")]
        Unit,
        [Description("Można przyjąć")]
        CanBeAddedToLoc,
        [Description("Kod EAN")]
        KodEAN,
        [Description("Nr Katalogowy")]
        NrKat,
    }

    public enum Operation
    {
        In,
        Out,
        OutIn,
    }

    public enum ItemActivityMode
    {
        Create,
        Edit,
        EditAdd,
        Split,
        None
    }
}
