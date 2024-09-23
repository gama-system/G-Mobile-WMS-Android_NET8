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
    class DocumentItems
    {
        public static string Paleta_GetFirstInData = "SELECT TOP 1 " +
                                                     "pd.idTowaru as 'idTowaru', " +                                       // 0
                                                     "pd.idPartia as 'idPartii', " +                                       // 1
                                                     "ISNULL(pa.strKod, '----') as 'strPartii', " +                        // 2
                                                     "pd.dtDataPrzydatnosci as 'Data przyd', " +                           // 3
                                                     "pd.dtDataProdukcji as 'Data prod', " +                               // 4
                                                     "(SELECT TOP 1 p.idFunkcjiLogistycznej FROM Przychod p " +
                                                     "LEFT JOIN Paleta plp ON p.idPaleta = plp.idPaleta WHERE plp.strOznaczenie = '<<PAL>>' ORDER BY idPrzychod DESC) as 'FLOG', " + // 5
                                                     "ISNULL(pd.idPaletaW, -1) as 'idPalW', " +                                        // 6
                                                     "ISNULL(pd.idPaletaP, -1) as 'idPalP', " +                                        // 7
                                                     "ISNULL(pd.idKontrahenta, -1) as 'idKontrah', " +                                 // 8
                                                     "t.strSymbol + ' - ' + t.strNazwa as 'strNazwaTow', " +                           // 9
                                                     "pd.strLoty as 'Loty', " +                                                        // 10
                                                     "pd.strNumerySeryjne as 'NrSer', " +                                              // 11
                                                     "ISNULL(k.strNazwa, '') as 'Kontrah', " +                                         // 12
                                                     "ISNULL(pd.idJednostkaMiary, -1) as 'Jednostka', " +                              // 13
                                                     "ISNULL(j.strNazwa, '') as 'strNazwaJedn' " +                                     // 14
                                                     "FROM PozycjeDokumentu pd " +
                                                     "LEFT JOIN Partia pa ON pd.idPartia = pa.idPartia " +
                                                     "LEFT JOIN Towar t ON pd.idTowaru = t.idTowaru " +
                                                     "LEFT JOIN Kontrahent k ON pd.idKontrahenta = k.idKontrahent " +
                                                     "LEFT JOIN JednostkaMiary j ON pd.idJednostkaMiary = j.idJednostkaMiary " +
                                                     "WHERE pd.idPozDokumentu = <<IDPOZDOK>> " +
                                                     "ORDER BY " +
                                                     "pd.idPozDokumentu ASC ";

        public enum Paleta_GetFirstInData_Results
        {
            idTowaru = 0,
            idPartii = 1,
            strPartia = 2,
            dtDataPrzydatnosci = 3,
            dtDataProdukcji = 4,
            idFunkcjiLogistycznej = 5,
            idPalW = 6,
            idPalP = 7,
            idKontrahenta = 8,
            strNazwaTowaru = 9,
            strLoty = 10,
            strNumerySeryjne = 11,
            strNazwaKtr = 12,
            idJednostki = 13,
            strNazwaJednostki = 14,
        }
    }
}