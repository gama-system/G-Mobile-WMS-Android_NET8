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
    public static class Contractors
    {
        public static string GetContactors = "SELECT * FROM " +
                                               "( " +
                                                   "SELECT " +
                                                   "idKontrahent, " + // 0
                                                   "strSymbol, " + // 1
                                                   "strNazwa, " + // 2

                                                   "CASE " +
                                                   "WHEN " +
                                                        "idKontrahNadrz IS NULL " +
                                                   "THEN " +
                                                        "idKontrahent " +
                                                   "ELSE " +
                                                        "idKontrahNadrz " +
                                                   "END as KontrahNadrz " + // 3

                                                   "FROM Kontrahent " +
                                                   "WHERE bAktywny = 1 AND bZablokowany = 0 AND strSymbol LIKE '%<<FILTR>>%' OR strNazwa LIKE '%<<FILTR>>%' OR strKod LIKE '%<<FILTR>>%' " +
                                                ") " +

                                                "as Ktr ORDER BY KontrahNadrz";

        public enum Contractors_Results
        {
            idKontrahenta = 0,
            strSymbol = 1,
            strNazwa = 2,
            idKontrahentaNadrz = 3
        }
    }
}