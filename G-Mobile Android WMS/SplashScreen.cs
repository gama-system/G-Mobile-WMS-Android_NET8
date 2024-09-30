using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Android;
using Android.Animation;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Common;
using Newtonsoft.Json;
using Symbol.XamarinEMDK;
using WMS_DESKTOP_API;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "G-Mobile Android WMS",
        MainLauncher = true,
        Theme = "@style/Theme.Splash",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        NoHistory = true,
        Icon = "@drawable/Icon"
    )]
    public class SplashScreen : BaseWMSActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetDefaults();

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            Helpers.SetLanguage(this, "pl");

            //Helpers.DisableNavigationBar(this);

            string Errors = "";

            if (LoadConfig())
            {
                if (Serwer.PołączZSerwerem() == -1)
                {
                    Errors = GetString(Resource.String.splash_activity_connecterror);
                }
                else
                {
                    Globalne.Licencja = Serwer.licencjaBL.GetLicence_Portable();
                }
            }
            else
            {
                Errors = GetString(Resource.String.splash_activity_blad_konfig);
            }

            if (Errors != "")
                Helpers.CenteredToast(Errors, ToastLength.Long);
            else
                BusinessLogicHelpers.Config.GetDatabaseConfig();

            Helpers.TurnOnScanner();

            IsBusy = false;
            Helpers.SwitchAndFinishCurrentActivity(this, typeof(MainActivity));
        }

        bool LoadConfig()
        {
#if DEBUG
            Globalne.CurrentTerminalSettings = new TerminalSettings();
            //Globalne.CurrentTerminalSettings.IP = "10.1.0.50";
            Globalne.CurrentTerminalSettings.IP = "10.1.0.236";
            Globalne.CurrentTerminalSettings.Port = "6446";
            Globalne.CurrentTerminalSettings.User = "Terminal1";
            Globalne.CurrentTerminalSettings.Password = "Terminal1";
            Globalne.CurrentTerminalSettings.Password = "Terminal1";
            //Globalne.CurrentTerminalSettings.Orientation = ScreenOrientation.Landscape;
            return true;
#endif

            TerminalSettings Set = TerminalSettings.GetSettings();

            if (Set != null)
                Globalne.CurrentTerminalSettings = Set;
            else
                Globalne.CurrentTerminalSettings = new TerminalSettings();

            if (Set != null)
                return true;
            else
                return false;
        }

        void SetDefaults()
        {
            //todo: dosotwąć do api
            SetDeviceData();

            Globalne.AppName = Xamarin.Essentials.AppInfo.Name;
            Globalne.AppVer = Xamarin.Essentials.AppInfo.VersionString;

            Globalne.Player = new Android.Media.MediaPlayer();
            Globalne.CurrentSettings = null;

            Serwer.Konfiguracja = Globalne.CurrentTerminalSettings;
            Globalne.Licencja = null;
            Globalne.Operator = null;
            Globalne.Magazyn = null;
        }

        void SetDeviceData()
        {
            string Manufacturer = Android.OS.Build.Manufacturer;

            if (
                Manufacturer.Contains("Zebra Technologies")
                || Manufacturer.Contains("Motorola Solutions")
            )
                Globalne.DeviceType = Enums.DeviceTypes.Zebra;
            else if (Manufacturer.Contains("Newland"))
                Globalne.DeviceType = Enums.DeviceTypes.Newland;
            else
                Globalne.DeviceType = Enums.DeviceTypes.Other;

            Android.Content.PM.PackageManager PM = PackageManager;

            if (PM.HasSystemFeature(Android.Content.PM.PackageManager.FeatureCamera))
                Globalne.HasCamera = true;
        }
    }
}
