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

namespace G_Mobile_Android_WMS.SQL
{
    public static class Stocks
    {
        public static string GetStocks = "SELECT " +
                                         "(t.strSymbol + ' - ' + t.strNazwa) as strNazwaTowaru, " +  // 0
                                         "l.strNazwa as strNazwaLok, " + // 1

                                         "CASE " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -1 " +
                                             "THEN " +
                                                "(SELECT j.strNazwa FROM JednostkaMiary j LEFT JOIN Towar tw ON tw.idJednostkaDomyslna = j.idJednostkaMiary WHERE tw.idTowaru = t.idTowaru) " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -2 " +
                                             "THEN " +
                                                "(SELECT j.strNazwa FROM JednostkaMiary j LEFT JOIN Towar tw ON tw.idJednostkaMiary = j.idJednostkaMiary WHERE tw.idTowaru = t.idTowaru) " +
                                             "ELSE " +
                                                "(SELECT strNazwa FROM JednostkaMiary WHERE idJednostkaMiary = <<IDJEDNOSTKI>>)" +
                                         "END as strNazwaJednostki, " + // 2

                                         "CASE " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -1 " +
                                             "THEN " +
                                                "(SELECT SUM(p.numZostalo) FROM Przychod p " +
                                                "WHERE p.numZostalo != 0 AND p.idLokalizacja = l.idLokalizacja AND p.idTowaru = t.idTowaru <<SUM>>) / " +
                                                "ISNULL((SELECT TOP 1 jp.numIle FROM JednostkaPrzelicz jp WHERE jp.idTowaru = t.idTowaru AND jp.idJednostkaMiary = t.idJednostkaDomyslna), 1) " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -2 " +
                                             "THEN " +
                                                "(SELECT SUM(p.numZostalo) FROM Przychod p " +
                                                "WHERE p.numZostalo != 0 AND p.idLokalizacja = l.idLokalizacja AND p.idTowaru = t.idTowaru <<SUM>>) " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = t.idJednostkaMiary " +
                                             "THEN " +
                                                "(SELECT SUM(p.numZostalo) FROM Przychod p " +
                                                "WHERE p.numZostalo != 0 AND p.idLokalizacja = l.idLokalizacja AND p.idTowaru = t.idTowaru <<SUM>>)" +
                                             "WHEN " +
                                                "ISNULL((SELECT TOP 1 jp.numIle FROM JednostkaPrzelicz jp WHERE jp.idTowaru = t.idTowaru AND jp.idJednostkaMiary = <<IDJEDNOSTKI>>), -1) = -1 " +
                                             "THEN " +
                                                "-1 " +
                                             "ELSE " +
                                                "(SELECT SUM(p.numZostalo) FROM Przychod p " +
                                                "WHERE p.numZostalo != 0 AND p.idLokalizacja = l.idLokalizacja AND p.idTowaru = t.idTowaru <<SUM>>) / " +
                                                "(SELECT TOP 1 jp.numIle FROM JednostkaPrzelicz jp " +
                                                "WHERE jp.idTowaru = t.idTowaru AND jp.idJednostkaMiary = <<IDJEDNOSTKI>>) " +
                                         "END " +
                                            " - <<ROZNOSZENIE>> " +
                                            "as numStan " +            // 3

                                         "FROM Przychod prz LEFT JOIN Towar t ON prz.idTowaru = t.idTowaru " +
                                         "LEFT JOIN Lokalizacja l ON prz.idLokalizacja = l.idLokalizacja " +
                                         "left join SciezkaZbiorki sz on sz.idLokalizacja = l.idLokalizacja " +
                                        "LEFT JOIN PozycjeDokumentu pozd ON pozd.idPozDokumentu = prz.idPozDokumentu " +
                                         "WHERE prz.numZostalo != 0 " +
                                         "AND pozd.numIloscZrealizowana = pozd.numIloscZlecona " +
                                         //"AND l.bBufor = 0 " +
                                         " <<WHERE>> " +
                                         "GROUP BY T.strNazwa ,T.strSymbol, L.strNazwa, T.idTowaru, L.idLokalizacja, L.idMagazyn, SZ.intPozycja, T.idJednostkaMiary, T.idJednostkaDomyslna, l.bBufor " +
                                         "ORDER by l.bBufor asc, SZ.intPozycja  asc";


        public static string GetStocks_RoznoszoneZBufora =
            "SELECT (t.strSymbol + ' - ' + t.strNazwa) strNazwaTowaru, " +
            "lokz.strNazwa + ' (Roznoszenie)' strNazwaLok, " +
            "CASE " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -1 " +
                                             "THEN " +
                                                "(SELECT j.strNazwa FROM JednostkaMiary j LEFT JOIN Towar tw ON tw.idJednostkaDomyslna = j.idJednostkaMiary WHERE tw.idTowaru = t.idTowaru) " +
                                             "WHEN " +
                                                "<<IDJEDNOSTKI>> = -2 " +
                                             "THEN " +
                                                "(SELECT j.strNazwa FROM JednostkaMiary j LEFT JOIN Towar tw ON tw.idJednostkaMiary = j.idJednostkaMiary WHERE tw.idTowaru = t.idTowaru) " +
                                             "ELSE " +
                                                "(SELECT strNazwa FROM JednostkaMiary WHERE idJednostkaMiary = <<IDJEDNOSTKI>>)" +
                                         "END as strNazwaJednostki, " + // 2
                                                                        // "jm.strNazwa as strNazwaJednostki, " +
            "Case when (<<IDJEDNOSTKI>> =1  or <<IDJEDNOSTKI>> =-2)  then SUM(pozd.numIloscZlecona) * isnull(jjp.numIle,1) " +
             "else SUM(pozd.numIloscZlecona)  end  " +
           //"SUM(pozd.numIloscZlecona)  " +
            "FROM PozycjeDokumentu pozd " +
            "LEFT JOIN Lokalizacja lokz ON pozd.idLokalizacjaW = lokz.idLokalizacja " +
            "LEFT JOIN Dokumenty dokk ON pozd.idDokumentu = dokk.idDokumentu " +
            "LEFT JOIN Rejestry rerj ON dokk.idRejestr = rerj.idRejestr " +
            "LEFT JOIN JednostkaPrzelicz jjp ON jjp.idTowaru = pozd.idTowaru " +
            "join dbo.JednostkaMiary jm on jm.idJednostkaMiary = jjp.idJednostkaMiary " +
            "join Towar t on t.idTowaru = pozd.idTowaru " +
            "WHERE pozd.numIloscZrealizowana != pozd.numIloscZlecona AND rerj.strTyp = 'ZL' " +
            //"AND lokz.bBufor = 1 "+
            "AND pozd.idTowaru = <<IDTOWARU>> " +
            "GROUP BY t.strSymbol, t.strNazwa, lokz.strNazwa, jm.strNazwa, jjp.numIle, t.idTowaru " +
            "ORDER by lokz.strNazwa desc";
        
        // odejmujemy ilosci ktore sa w trakcie roznoszenia
        public static string W_Roznoszeniu = " isnull((SELECT case " +
                                            "when (<<IDJEDNOSTKI>> =1  or <<IDJEDNOSTKI>> =2)  then SUM(pozd.numIloscZlecona) * isnull(jjp.numIle,1) " +
			                                "else SUM(pozd.numIloscZlecona) " +
                                            "end " +
                                            " FROM " +
                                            " 	PozycjeDokumentu pozd " +
                                            " LEFT JOIN Lokalizacja lokz ON" +
                                            " 	pozd.idLokalizacjaW = lokz.idLokalizacja" +
                                            " LEFT JOIN Dokumenty dokk ON" +
                                            " 	pozd.idDokumentu = dokk.idDokumentu" +
                                            " LEFT JOIN Rejestry rerj ON" +
                                            " 	dokk.idRejestr = rerj.idRejestr" +
                                            " join Towar t on" +
                                            " 	t.idTowaru = pozd.idTowaru" +
                                            " LEFT JOIN JednostkaPrzelicz jjp ON jjp.idTowaru = pozd.idTowaru " +
                                            " left join dbo.JednostkaMiary jm on jm.idJednostkaMiary = <<IDJEDNOSTKI>> " +
                                            " WHERE" +
                                            " 	pozd.numIloscZrealizowana != pozd.numIloscZlecona" +
                                            " 	AND rerj.strTyp = 'ZL'" +
                                            " 	AND pozd.idTowaru =  <<IDTOWARU>>" +
                                            "   AND l.idLokalizacja  = lokz.idLokalizacja " +
                                            " GROUP BY" +
                                            " 	t.strSymbol," +
                                            " 	lokz.strNazwa," +
                                            " 	jm.strNazwa," +
                                            "   jjp.numIle),0) ";

        public static string GetStocks_Where_Mag = " AND l.idMagazyn = <<ID_MAG>> ";
        public static string GetStocks_Where_Flog = " AND p.idFunkcjiLogistycznej = <<ID_FLOG>> ";
        public static string GetStocks_Where_Part = " AND p.idPartia = <<ID_PART>> ";
        public static string GetStocks_Where_Pal = " AND p.idPaleta = <<ID_PAL>> ";
        public static string GetStocks_Where_Kh = " AND p.idKontrahenta = <<ID_KH>> ";
        public static string GetStocks_Where_Article = " AND p.idTowaru = <<IDTOWARU>> ";
        public static string GetStocks_Where_Location = " AND p.idLokalizacja = <<IDLOKALIZACJA>> ";


        public static string GetStocks_Where_Mag_Where = " AND l.idMagazyn = <<ID_MAG>> ";
        public static string GetStocks_Where_Flog_Where = " AND prz.idFunkcjiLogistycznej = <<ID_FLOG>> ";
        public static string GetStocks_Where_Part_Where = " AND prz.idPartia = <<ID_PART>> ";
        public static string GetStocks_Where_Pal_Where = " AND prz.idPaleta = <<ID_PAL>> ";
        public static string GetStocks_Where_Kh_Where = " AND prz.idKontrahenta = <<ID_KH>> ";
        public static string GetStocks_Where_Article_Where = " AND prz.idTowaru = <<IDTOWARU>> ";
        public static string GetStocks_Where_Location_Where = " AND prz.idLokalizacja = <<IDLOKALIZACJA>> ";

        public enum Stocks_Results
        {
            strNazwaTowaru = 0,
            strNazwaLok = 1,
            strNazwaJednostki = 2,
            numStan = 3
        }
    }
}