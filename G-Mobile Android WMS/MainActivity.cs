using System;
using System.Globalization;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Widget;
using Java.Nio.Channels;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;
using static Android.Views.View;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustNothing
            | Android.Views.SoftInput.StateHidden
    )]
    public class MainActivity : BaseWMSActivity
    {
        FloatingActionButton BtnStart;
        FloatingActionButton BtnInfo;
        FloatingActionButton BtnZamknij;
        TextView TxWersja;

        public enum ResultCodes
        {
            CanQuitApplicationResult,
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Ustawienia regionalne
            //CultureInfo englishUSCulture = new CultureInfo("pl-PL"); // przecinek
            CultureInfo englishUSCulture = new CultureInfo("en-US"); // kropka
            CultureInfo.DefaultThreadCurrentCulture = englishUSCulture;
            CultureInfo.DefaultThreadCurrentUICulture = englishUSCulture;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            GetAndSetControls();
            IsBusy = false;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.main_activity_name));

            TxWersja = FindViewById<TextView>(Resource.Id.TextWersja);
            Helpers.SetTextOnTextView(
                this,
                TxWersja,
                GetString(Resource.String.main_activity_wersja) + " " + Globalne.AppVer
            );

            BtnZamknij = FindViewById<FloatingActionButton>(Resource.Id.BtnZamknij);
            BtnZamknij.Click += BtnZamknij_Click;

            BtnInfo = FindViewById<FloatingActionButton>(Resource.Id.BtnInfo);
            BtnInfo.Click += BtnInfo_Click;

            BtnStart = FindViewById<FloatingActionButton>(Resource.Id.BtnStart);
            BtnStart.Click += BtnStart_Click;

            ImageView Logo = FindViewById<ImageView>(Resource.Id.LogoKlienta);

            if (Globalne.Logo != null)
                Logo.SetImageBitmap(Globalne.Logo);

            if (!ServerConnection.Connected)
            {
                Android.Content.Res.ColorStateList CS1 = new Android.Content.Res.ColorStateList(
                    new int[][] { new int[0] },
                    new int[] { Color.ParseColor("#000000") }
                );
                BtnStart.BackgroundTintList = CS1;
                BtnStart.Enabled = false;
            }
            else
            {
                if (Globalne.Logo == null)
                {
                    FirmaO Firma = Serwer.podmiotBL.PobierzFirmę(1);

                    if (Globalne.Logo == null && Firma.Logo != null)
                    {
                        byte[] bytes = Convert.FromBase64String((string)Firma.Logo);

                        if (bytes.Length != 0)
                        {
                            Bitmap bmp = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);

                            Logo.Measure(
                                MeasureSpec.MakeMeasureSpec(
                                    0,
                                    Android.Views.MeasureSpecMode.Unspecified
                                ),
                                MeasureSpec.MakeMeasureSpec(
                                    0,
                                    Android.Views.MeasureSpecMode.Unspecified
                                )
                            );
                            Globalne.Logo = Bitmap.CreateScaledBitmap(
                                bmp,
                                Logo.MeasuredWidth,
                                Logo.MeasuredHeight,
                                true
                            );
                            Logo.SetImageBitmap(Globalne.Logo);
                        }
                    }
                }
            }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(
                () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(UsersActivity))
            );
        }

        private void BtnInfo_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(
                () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(InfoActivity))
            );
        }

        async Task AskAboutClosingAndClose()
        {
            //TODO: DOSTOSOWAĆ DO API


            //await Task.Delay(Globalne.TaskDelay);

            //var Odp = await Helpers.QuestionAlertAsync(this,
            //                                           Resource.String.main_activity_wyjściezprogramu_message,
            //                                           Resource.Raw.sound_alert,
            //                                           Resource.String.main_activity_wyjściezprogramu_title);

            //if (Odp)
            //{
            //    try
            //    {
            //        Helpers.EnableNavigationBar(this);

            //        this.FinishAffinity();

            //        if (ServerConnection.Connected)
            //            Globalne.client.Release();


            //        if (Globalne.Scanner != null)
            //        {
            //            Globalne.Scanner.Disable();
            //            Globalne.Scanner.Dispose();
            //        }
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
        }

        async void BtnZamknij_Click(object sender, EventArgs e)
        {
            if (
                Globalne.CurrentSettings != null
                && Globalne.CurrentSettings.CheckCanCloseApp
                && ServerConnection.Connected
            )
            {
                if (IsSwitchingActivity)
                    return;

                IsSwitchingActivity = true;

                Intent i = new Intent(this, typeof(UsersActivity));
                i.PutExtra(
                    UsersActivity.Vars.Mode,
                    (int)UsersActivity.UsersActivityModes.IsAllowedToCloseApplication
                );

                RunOnUiThread(
                    () => StartActivityForResult(i, (int)ResultCodes.CanQuitApplicationResult)
                );
            }
            else
                await RunIsBusyTaskAsync(() => AskAboutClosingAndClose());
        }

        protected override async void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            IsSwitchingActivity = false;

            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ResultCodes.CanQuitApplicationResult && resultCode == Result.Ok)
                await RunIsBusyTaskAsync(() => AskAboutClosingAndClose());
        }
    }
}
