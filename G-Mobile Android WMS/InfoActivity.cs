using System;
using System.Collections.Generic;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false
    )]
    public class InfoActivity : BaseWMSActivity
    {
        FloatingActionButton InfoBtnPrev = null;
        FloatingActionButton InfoBtnSettings = null;
        TextView InfoWersja = null;
        TextView InfoZarejestrowanoDla = null;
        TextView InfoCzasLicencji = null;

        public enum ResultCodes
        {
            CanEditSettingsResult,
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_info);

            GetAndSetControls();
            IsBusy = false;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.info_activity_name));

            InfoBtnPrev = FindViewById<FloatingActionButton>(Resource.Id.InfoBtnPrev);
            InfoBtnPrev.Click += InfoBtnPrev_Click;

            InfoBtnSettings = FindViewById<FloatingActionButton>(Resource.Id.InfoBtnSettings);
            InfoBtnSettings.Click += InfoBtnSettings_Click;

            InfoWersja = FindViewById<TextView>(Resource.Id.InfoWersja);
            Helpers.SetTextOnTextView(
                this,
                InfoWersja,
                GetString(Resource.String.info_activity_wersja) + " " + Globalne.AppVer
            );

            if (Globalne.Licencja != null)
            {
                InfoZarejestrowanoDla = FindViewById<TextView>(Resource.Id.InfoZarejestrowanoDlaX);
                Helpers.SetTextOnTextView(
                    this,
                    InfoZarejestrowanoDla,
                    Globalne.Licencja.FirmaLicencjonowana
                );

                InfoCzasLicencji = FindViewById<TextView>(Resource.Id.InfoCzasLicencjiX);

                if (Globalne.Licencja.LicencjaCzasowa)
                    Helpers.SetTextOnTextView(
                        this,
                        InfoCzasLicencji,
                        Globalne.Licencja.PoczLicencji.ToString(Globalne.CurrentSettings.DateFormat)
                            + " - "
                            + Globalne.Licencja.TerminLicencji.ToString(
                                Globalne.CurrentSettings.DateFormat
                            )
                    );
                else
                    Helpers.SetTextOnTextView(
                        this,
                        InfoCzasLicencji,
                        GetString(Resource.String.info_activity_licencjapelna)
                    );
            }
        }

        private void InfoBtnSettings_Click(object sender, EventArgs e)
        {
            if (!Serwer.Connected)
                RunIsBusyAction(
                    () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(SettingsActivity))
                );
            else
            {
                if (IsSwitchingActivity)
                    return;

                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(UsersActivity));
                i.PutExtra(
                    UsersActivity.Vars.Mode,
                    (int)UsersActivity.UsersActivityModes.IsAllowedToEditSettings
                );

                RunOnUiThread(
                    () => StartActivityForResult(i, (int)ResultCodes.CanEditSettingsResult)
                );
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ResultCodes.CanEditSettingsResult && resultCode == Result.Ok)
                RunIsBusyAction(
                    () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(SettingsActivity))
                );
        }

        private void InfoBtnPrev_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(
                () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(MainActivity))
            );
        }
    }
}
