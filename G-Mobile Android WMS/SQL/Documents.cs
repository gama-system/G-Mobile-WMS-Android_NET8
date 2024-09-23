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
    public static class Documents
    {
        public static string GetDocs = "SELECT " +
                                       "d.idDokumentu, " + // 0 
                                       "d.strNazwa, " + // 1
                                       "ISNULL(k.strNazwa, '---'), " + // 2
                                       "d.dataDokumentu, " + // 3
                                       "d.bZlecenie, " + // 4
                                       "s.intTypStatusu, " + // 5
                                       "d.intUtworzonyPrzez, " + // 6
                                       "ISNULL(d.strNazwaERP, ''), " + // 7
                                       "ISNULL(d.strDokumentDostawcy, ''), " + // 8
                                       //na zyczenie klienta "Lider" dodajemy w polu Uwagi, ilosc pozycji - nazwa pierwszego towaru na liscie
                                       //oraz ograniczamy dlugosc znakow  Opisu + nazwa towaru  do 140 znaków dla rejestrów  'ZS/ BL'
                                       "substring(concat((select top 1 concat('*[', " +
                                           "(select count(distinct(idtowaru)) from pozycjedokumentu pd where iddokumentu = d.iddokumentu), '] ', " +
                                           "upper(t.strnazwa), ' [',RTRIM(t.strKod), ']', " +
                                           "char(10), char(10)) " +
                                        "from " +
                                           "pozycjedokumentu pd " +
                                           "join towar t on pd.idtowaru = t.idtowaru " +
                                        "where " +
                                           "pd.iddokumentu = d.iddokumentu " +
                                           "and d.idrejestr in (select r.idrejestr from rejestry r where r.strerp like 'ZS/ BL') " +
                                           "group by t.strnazwa, t.strKod), " +
                                       "isnull(d.stropis,'')),0,140) " + // 9
                                                                         //"ISNULL(d.strOpis, '') " + // 9
                                       "FROM Dokumenty d LEFT JOIN Kontrahent k ON d.idKontrahent = k.idKontrahent LEFT JOIN StatusyDokumentow s ON d.idStatusDokumentu = s.idStatusDokumentu " +
                                       //"WHERE  ( d.dataDokumentu >= '2022-03-12' OR s.intTypStatusu in (1, 2, 3, 4, 5) ) " +
                                       "WHERE s.intTypStatusu in (1, 2, 3, 4, 5) " +
                                       "AND (d.intEdytowany IS NULL OR <<ID_EDYTOWANY>> = -1 OR d.intEdytowany = <<ID_EDYTOWANY>>) " +
                                       "AND (d.idOperator IS NULL OR <<IDOPERATORA>> = -1 OR d.idOperator = <<IDOPERATORA>>) " +
                                       "AND (d.idRejestr IN <<REJESTRY>>) " +
                                       "AND (d.dataDokumentu >= '<<DATAPOCZĄTKOWA>>' OR (s.intTypStatusu IN (1, 2, 3, 4) AND (d.bZlecenie = 1 OR d.bTworzenieNaTerminalu = 1))) ";

        public static string GetDocs_Where_Przychód = " AND (d.intMagazynP = <<IDMAGAZYNU>>) ";
        public static string GetDocs_Where_Rozchód = " AND (d.intMagazynW = <<IDMAGAZYNU>>) ";
        public static string GetDocs_Where_SubDoc = " AND (d.bDokGlowny = 0) ";
        public static string GetDocs_Where_Filter= " AND (d.strNazwa LIKE '%<<FILTERTEXT>>%' OR k.strNazwa LIKE '%<<FILTERTEXT>>%' OR k.strSymbol LIKE '%<<FILTERTEXT>>%') ";
        public static string GetDocs_OrderBy = " ORDER BY d.intPriorytet DESC, s.intTypStatusu ASC, d.dataDokumentu DESC, d.dtDataERP ASC, d.idOperator DESC ";

        public enum Documents_Results
        {
            idDokumentu = 0,
            strNazwaDokumentu = 1,
            strNazwaKontrahenta = 2,
            dtDataDokumentu = 3,
            bZlecenie = 4,
            intTypStatusu = 5,
            intUtworzonyPrzez = 6,
            strNazwaERP = 7,
            strDokumentDostawcy = 8,
            strOpis = 9,
        }

    }
}