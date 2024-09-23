using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;
using Xamarin.Essentials;

namespace G_Mobile_Android_WMS
{
    public static class Globalne
    {
        // todo: mmozliwe ze czesc rzeczy jest tu nie potrzebna i znajduje sie w biibliiotece desktop api
        public static string AppName = "G-Mobile WMS Android";
        public static string AppVer = AppInfo.Version.ToString();
        public static int WersjaBazy = 0;
        public static int WymaganaWersjaBazy = 84;

        public static Enums.DocTypes DocumentMode;

        public static Bitmap Logo = null;

        public static MediaPlayer Player;

        public static IBarcodeScannerManager Scanner;
        public static bool ScannerError = false;

        public static TerminalSettings CurrentTerminalSettings = new TerminalSettings();
        public static WMSSettings CurrentSettings = new WMSSettings();
        public static UserSettings CurrentUserSettings = new UserSettings();

        public static Enums.DeviceTypes DeviceType = Enums.DeviceTypes.Other;
        public static bool HasCamera = false;
        public static bool HasScanner = false;

        public static LicencjaPortableO Licencja = null;
        public static OperatorVO Operator;
        public static MagazynO Magazyn;

        //public static List<Scanner> Scanners = new List<Scanner>();

        public const int TaskDelay = 50;
        public const string SkipScanner = "SkipScanner";

        // DEBUG mode: 0 = Disable | 1 = Enable
        public static int debug_mode = 0;
    }
}
