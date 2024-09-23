using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.Content.Res;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustPan
            | Android.Views.SoftInput.StateHidden
    )]
    public class SettingsActivity : ActivityWithScanner
    {
        FloatingActionButton SettingsBtnPrev;
        FloatingActionButton SettingsBtnSave;
        TextView TestSkaneraX;
        EditText SettingsPassX;
        EditText SettingsUserX;
        EditText SettingsPortX;
        EditText SettingsIPX;
        Button Orientation;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_settings);
            GetAndSetControls();

            IsBusy = false;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.settings_activity_name));

            SettingsBtnPrev = FindViewById<FloatingActionButton>(Resource.Id.SettingsBtnPrev);
            SettingsBtnPrev.Click += SettingsBtnPrev_Click;

            SettingsBtnSave = FindViewById<FloatingActionButton>(Resource.Id.SettingsBtnSave);
            SettingsBtnSave.Click += SettingsBtnSave_Click;

            Button InnerSettings = FindViewById<Button>(Resource.Id.settings_activity_inner);
            Button IdsSettings = FindViewById<Button>(Resource.Id.settings_activity_usersettingids);
            Button UserSettings = FindViewById<Button>(Resource.Id.settings_activity_usersettings);
            Orientation = FindViewById<Button>(Resource.Id.settings_activity_orientation);

            InnerSettings.Click += InnerSettings_Click;
            IdsSettings.Click += UserSettingsIds_Click;
            UserSettings.Click += UserSettings_Click;
            Orientation.Click += Orientation_Click;

            InnerSettings.BackgroundTintList = AppCompatResources.GetColorStateList(
                this,
                Resource.Color.floating_button_red
            );
            IdsSettings.BackgroundTintList = AppCompatResources.GetColorStateList(
                this,
                Resource.Color.button_blue
            );
            UserSettings.BackgroundTintList = AppCompatResources.GetColorStateList(
                this,
                Resource.Color.button_blue
            );
            Orientation.BackgroundTintList = AppCompatResources.GetColorStateList(
                this,
                Resource.Color.button_blue
            );

            if (!ServerConnection.Connected)
            {
                InnerSettings.Visibility = ViewStates.Gone;
                IdsSettings.Visibility = ViewStates.Gone;
                UserSettings.Visibility = ViewStates.Gone;
            }

            TestSkaneraX = FindViewById<TextView>(Resource.Id.TestSkaneraX);
            SettingsIPX = FindViewById<EditText>(Resource.Id.SettingsIPX);
            SettingsPortX = FindViewById<EditText>(Resource.Id.SettingsPortX);
            SettingsUserX = FindViewById<EditText>(Resource.Id.SettingsUserX);
            SettingsPassX = FindViewById<EditText>(Resource.Id.SettingsPassX);

            Helpers.SetTextOnEditText(this, SettingsIPX, Globalne.CurrentTerminalSettings.IP);
            Helpers.SetTextOnEditText(this, SettingsPortX, Globalne.CurrentTerminalSettings.Port);
            Helpers.SetTextOnEditText(this, SettingsUserX, Globalne.CurrentTerminalSettings.User);
            Helpers.SetTextOnEditText(
                this,
                SettingsPassX,
                Globalne.CurrentTerminalSettings.Password
            );

            string OrientationStr = "";

            switch (Globalne.CurrentTerminalSettings.Orientation)
            {
                case Android.Content.PM.ScreenOrientation.Portrait:
                    OrientationStr = this.GetString(Resource.String.orientation_vert);
                    break;
                case Android.Content.PM.ScreenOrientation.Landscape:
                    OrientationStr = this.GetString(Resource.String.orientation_horiz);
                    break;
                case Android.Content.PM.ScreenOrientation.ReversePortrait:
                    OrientationStr = this.GetString(Resource.String.orientation_revvert);
                    break;
                case Android.Content.PM.ScreenOrientation.ReverseLandscape:
                    OrientationStr = this.GetString(Resource.String.orientation_revhoriz);
                    break;
                default:
                    OrientationStr = this.GetString(Resource.String.orientation_vert);
                    break;
            }

            Helpers.SetTextOnButton(this, Orientation, OrientationStr);
        }

        private async void Orientation_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity)
                return;

            string[] Options = new string[]
            {
                this.GetString(Resource.String.orientation_vert),
                this.GetString(Resource.String.orientation_horiz),
                this.GetString(Resource.String.orientation_revvert),
                this.GetString(Resource.String.orientation_revhoriz)
            };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                this.GetString(Resource.String.select_screen_orientation),
                this.GetString(Resource.String.global_cancel),
                "",
                null,
                Options
            );

            if (Res == this.GetString(Resource.String.global_cancel))
                return;
            else
            {
                if (Res == Options[0])
                    Globalne.CurrentTerminalSettings.Orientation = Android
                        .Content
                        .PM
                        .ScreenOrientation
                        .Portrait;
                else if (Res == Options[1])
                    Globalne.CurrentTerminalSettings.Orientation = Android
                        .Content
                        .PM
                        .ScreenOrientation
                        .Landscape;
                else if (Res == Options[2])
                    Globalne.CurrentTerminalSettings.Orientation = Android
                        .Content
                        .PM
                        .ScreenOrientation
                        .ReversePortrait;
                else if (Res == Options[3])
                    Globalne.CurrentTerminalSettings.Orientation = Android
                        .Content
                        .PM
                        .ScreenOrientation
                        .ReverseLandscape;
                else
                    Globalne.CurrentTerminalSettings.Orientation = Android
                        .Content
                        .PM
                        .ScreenOrientation
                        .Portrait;

                Helpers.SetTextOnButton(this, Orientation, Res);
                this.RequestedOrientation = Globalne.CurrentTerminalSettings.Orientation;
            }
        }

        private void UserSettings_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent(this, typeof(UserSettingIDsActivity));
            RunOnUiThread(() => StartActivityForResult(i, 10));
        }

        private async void UserSettingsIds_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => SelectSettingsGroup());
        }

        private async Task SelectSettingsGroup()
        {
            try
            {
                int ID = await BusinessLogicHelpers.UserSettingGroups.ShowSelectListOfUserGroups(
                    this,
                    Resource.String.settings_activity_selectsetgroup,
                    true
                );

                if (ID == -1)
                    return;
                else
                {
                    Intent i = new Intent(this, typeof(UserSettingsActivity));
                    i.PutExtra(UserSettingsActivity.Vars.IDSettingGroup, ID);

                    RunOnUiThread(() => StartActivityForResult(i, 20));
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void InnerSettings_Click(object sender, EventArgs e)
        {
            try
            {
                PromptResult Res = await Helpers.AlertAsyncWithPrompt(
                    this,
                    Resource.String.settings_getpass,
                    null,
                    "",
                    InputType.Password
                );

                if (!Res.Ok)
                    return;

                if (
                    !Serwer.operatorBL.SprawdźHasłoOperatora(
                        Int32.MaxValue,
                        Encryption.RSAEncrypt(Res.Text)
                    )
                )
                {
                    Helpers.PlaySound(this, Resource.Raw.sound_alert);
                    return;
                }

                RunIsBusyAction(
                    () =>
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(InnerSettingsActivity))
                );
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void SettingsBtnSave_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(async () =>
            {
                Helpers.ShowProgressDialog(GetString(Resource.String.global_wait));

                await Task.Run(() => SaveSettingsAndConnect());

                Helpers.HideProgressDialog();
            });
        }

        public void SaveSettingsAndConnect()
        {
            //WMS_DESKTOP_API.PolaczenieApi.EndpointAcces.AutentykacjaEndpoint autentykacja =new WMS_DESKTOP_API.PolaczenieApi.EndpointAcces.AutentykacjaEndpoint();
            //String token = autentykacja.


            Globalne.CurrentTerminalSettings.IP = SettingsIPX.Text;
            Globalne.CurrentTerminalSettings.Port = SettingsPortX.Text;
            Globalne.CurrentTerminalSettings.User = SettingsUserX.Text;
            Globalne.CurrentTerminalSettings.Password = SettingsPassX.Text;

            TerminalSettings.SaveSettings(Globalne.CurrentTerminalSettings);

            if (ServerConnection.Connect() == -1)
            {
                RunOnUiThread(
                    () =>
                        Helpers.CenteredToast(
                            GetString(Resource.String.settings_activity_connecterror),
                            ToastLength.Short
                        )
                );
            }
            else
            {
                int Res = ServerConnection.CreateObjects();

                if (Res == 0)
                {
                    Globalne.Licencja = Serwer.licencjaBL.GetLicence_Portable();
                    BusinessLogicHelpers.Config.GetDatabaseConfig();
                    RunOnUiThread(
                        () =>
                            Helpers.CenteredToast(
                                GetString(Resource.String.settings_activity_saved),
                                ToastLength.Short
                            )
                    );
                }
                else
                    RunOnUiThread(
                        () =>
                            Helpers.CenteredToast(
                                GetString(Resource.String.settings_activity_connecterror),
                                ToastLength.Short
                            )
                    );
            }
        }

        private void SettingsBtnPrev_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(
                () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(InfoActivity))
            );
        }

        protected override void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);
            Helpers.SetTextOnTextView(this, TestSkaneraX, String.Join(", ", this.LastScanData));
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            IsSwitchingActivity = false;
            base.OnActivityResult(requestCode, resultCode, data);
        }
    }
}
