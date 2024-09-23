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
using WMS_Model.ModeleDanych;
using Xamarin.Essentials;

namespace G_Mobile_Android_WMS
{
    public static class Globalne
    {
        public static string AppName = "G-Mobile WMS Android";
        public static string AppVer = AppInfo.Version.ToString();
        public static int WersjaBazy = 0;
        public static int WymaganaWersjaBazy = 84;

        public static Enums.DocTypes DocumentMode;

        public static Bitmap Logo = null;

        public static MediaPlayer Player;

        public static Hive.Rpc.Client client;

        public static IBarcodeScannerManager Scanner;
        public static bool ScannerError = false;

        public static TerminalSettings CurrentTerminalSettings = new TerminalSettings();
        public static WMSSettings CurrentSettings = new WMSSettings();
        public static UserSettings CurrentUserSettings = new UserSettings();

        public static Enums.DeviceTypes DeviceType = Enums.DeviceTypes.Other;
        public static bool HasCamera = false;
        public static bool HasScanner = false;

        public static WMSServerAccess.Licencja.LicencjaBL licencjaBL;
        public static WMSServerAccess.Operator.OperatorBL operatorBL;
        public static WMSServerAccess.Drukarka.DrukarkaBL drukarkaBL;
        public static WMSServerAccess.Magazyn.MagazynBL magazynBL;
        public static WMSServerAccess.Ogólne.OgólneBL ogólneBL;
        public static WMSServerAccess.Podmiot.PodmiotBL podmiotBL;
        public static WMSServerAccess.JednostkaMiary.JednostkaMiaryBL jednostkaMiaryBL;
        public static WMSServerAccess.Towar.TowarBL towarBL;
        public static WMSServerAccess.Wymagania.WymaganiaBL wymaganiaBL;
        public static WMSServerAccess.Lokalizacja.LokalizacjaBL lokalizacjaBL;
        public static WMSServerAccess.Rejestr.RejestrBL rejestrBL;
        public static WMSServerAccess.Dokument.DokumentBL dokumentBL;
        public static WMSServerAccess.PrzychRozch.PrzychRozchBL przychrozchBL;
        public static WMSServerAccess.KodyKreskowe.KodyKreskoweBL kodykreskoweBL;
        public static WMSServerAccess.Partia.PartiaBL partiaBL;
        public static WMSServerAccess.Paleta.PaletaBL paletaBL;
        public static WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL funklogBL;
        public static WMSServerAccess.Menu.MenuBL menuBL;
        public static WMSServerAccess.Aktualizacje.AktualizacjeBL aktualizacjeBL;
        public static WMSServerAccess.NumerSeryjny.NumerSeryjnyBL numerSeryjnyBL;

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
