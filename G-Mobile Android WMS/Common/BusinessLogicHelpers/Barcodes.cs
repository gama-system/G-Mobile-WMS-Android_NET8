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
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS.Common.BusinessLogicHelpers
{
    public static class Barcodes
    {
        public static LokalizacjaVO GetLocationFromBarcode(string Barcode, bool PominKuwety)
        {
            return Serwer.lokalizacjaBL.PobierzLokalizacjęWgKoduKreskowego(
                Barcode,
                Globalne.Magazyn.ID,
                PominKuwety
            );
        }
    }
}
