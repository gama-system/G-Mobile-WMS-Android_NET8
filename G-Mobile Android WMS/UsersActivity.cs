using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Plugin.DeviceInfo;
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
    public class UsersActivity : ActivityWithScanner
    {
        FloatingActionButton Back;
        ListView OperatorList;
        FloatingActionButton Refresh;
        TextView ItemCount;

        UsersActivityModes Mode = UsersActivityModes.LogIn;

        public enum UsersActivityModes
        {
            LogIn = 0,
            IsAllowedToEditSettings = 1,
            IsAllowedToCloseApplication = 2,
        }

        internal static class Vars
        {
            public const string Mode = "Mode";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_users);

            Mode = (UsersActivityModes)Intent.GetIntExtra(Vars.Mode, 0);

            GetAndSetControls();
            Task.Run(() => InsertData());

            Helpers.CheckAndRequestPermissions(
                this,
                new string[]
                {
                    Manifest.Permission.ReadPhoneState,
                    Manifest.Permission.WriteExternalStorage
                }
            );
        }

        private void GetAndSetControls()
        {
            switch (Mode)
            {
                case UsersActivityModes.IsAllowedToCloseApplication:
                    Helpers.SetActivityHeader(
                        this,
                        GetString(Resource.String.users_activity_closing_name)
                    );
                    break;
                case UsersActivityModes.IsAllowedToEditSettings:
                    Helpers.SetActivityHeader(
                        this,
                        GetString(Resource.String.users_activity_settings_name)
                    );
                    break;
                default:
                    Helpers.SetActivityHeader(this, GetString(Resource.String.users_activity_name));
                    break;
            }

            if (!Globalne.CurrentSettings.CheckCanBarcodeLogin)
            {
                Button Scn = FindViewById<Button>(Resource.Id.scanbutton);

                if (Scn != null)
                    Scn.Visibility = ViewStates.Gone;
            }

            Back = FindViewById<FloatingActionButton>(Resource.Id.UsersBtnPrev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.UsersBtnRefresh);
            OperatorList = FindViewById<ListView>(Resource.Id.list_view_users);
            ItemCount = FindViewById<TextView>(Resource.Id.users_liczba_pozycji);

            Back.Click += Back_Click;
            Refresh.Click += Refresh_ClickAsync;
            OperatorList.ItemClick += OperatorList_ItemClick;
        }

        async Task DoLogin(OperatorRow Selected)
        {
            PromptResult Res;
            int ResLogin = -1;

            while (true)
            {
                var tryEmptyPassword = Globalne.operatorBL.SprawdźHasłoOperatora(
                    Selected.ID,
                    Encryption.RSAEncrypt("")
                );
                if (!tryEmptyPassword)
                {
                    Res = await Helpers.AlertAsyncWithPrompt(
                        this,
                        Resource.String.users_getpass,
                        null,
                        "",
                        InputType.Password
                    );

                    if (!Res.Ok)
                        break;

                    ResLogin = await TryLogin(Selected, Res.Text);
                }
                else
                {
                    Helpers.CenteredToast("Logowanie...", ToastLength.Short);
                    ResLogin = await TryLogin(Selected, "");
                }
                switch (ResLogin)
                {
                    case 0:
                    {
                        try
                        {
                            UstawienieMobilneOpe Ust = Globalne.menuBL.PobierzUstawienieMobOpe(
                                Selected.idUstawienieMobOpe
                            );

                            if (Ust.ID < 0)
                            {
                                await Helpers.AlertAsyncWithConfirm(
                                    this,
                                    Resource.String.users_no_settings
                                );
                                return;
                            }
                            else
                                Globalne.CurrentUserSettings = (UserSettings)
                                    JsonConvert.DeserializeObject(
                                        Ust.strUstawienie,
                                        typeof(UserSettings)
                                    );

                            List<MagazynO> Magazyny =
                                Globalne.magazynBL.PobierzListęDostępnychDlaOperatora(
                                    Globalne.Operator.ID
                                );

                            if (Magazyny.Count == 1)
                            {
                                Globalne.Magazyn = Magazyny[0];
                                Helpers.SwitchAndFinishCurrentActivity(
                                    this,
                                    typeof(ModulesActivity)
                                );
                            }
                            else
                                Helpers.SwitchAndFinishCurrentActivity(
                                    this,
                                    typeof(WarehousesActivity)
                                );
                            return;
                        }
                        catch (Exception ex)
                        {
                            Helpers.HandleError(this, ex);
                            return;
                        }
                    }
                    case -1:
                        return;
                    default:
                        break;
                }
            }
        }

        async Task DoBarcodeLogin(string Barcode)
        {
            try
            {
                int ResLogin = await TryLoginFromBarcode(Barcode);

                switch (ResLogin)
                {
                    case 0:
                    {
                        try
                        {
                            UstawienieMobilneOpe Ust = Globalne.menuBL.PobierzUstawienieMobOpe(
                                Globalne.Operator.idUstawienieMobOpe
                            );

                            if (Ust.ID < 0)
                            {
                                await Helpers.AlertAsyncWithConfirm(
                                    this,
                                    Resource.String.users_no_settings
                                );
                                Globalne.Operator = null;
                                return;
                            }
                            else
                                Globalne.CurrentUserSettings = (UserSettings)
                                    JsonConvert.DeserializeObject(
                                        Ust.strUstawienie,
                                        typeof(UserSettings)
                                    );

                            List<MagazynO> Magazyny =
                                Globalne.magazynBL.PobierzListęDostępnychDlaOperatora(
                                    Globalne.Operator.ID
                                );

                            if (Magazyny.Count == 1)
                            {
                                Globalne.Magazyn = Magazyny[0];
                                Helpers.SwitchAndFinishCurrentActivity(
                                    this,
                                    typeof(ModulesActivity)
                                );
                            }
                            else
                                Helpers.SwitchAndFinishCurrentActivity(
                                    this,
                                    typeof(WarehousesActivity)
                                );

                            return;
                        }
                        catch (Exception ex)
                        {
                            Helpers.HandleError(this, ex);
                            return;
                        }
                    }
                    case -1:
                        return;
                    default:
                        return;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        async void OperatorList_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (IsBusy || IsSwitchingActivity)
                return;

            OperatorRow Selected = (OperatorList.Adapter as UsersListAdapter)[e.Position];

            if (Selected == null)
                return;

            switch (Mode)
            {
                case UsersActivityModes.LogIn:
                {
                    bool? Res = await TryUpdate();

                    if (Res == true)
                        await RunIsBusyTaskAsync(() => DoLogin(Selected));
                    else if (Res == false)
                        Helpers.CenteredToast(
                            GetString(Resource.String.update_failed),
                            ToastLength.Long
                        );
                    else if (Res == null)
                        return;

                    break;
                }
                case UsersActivityModes.IsAllowedToEditSettings:
                {
                    bool Ret = false;

                    await RunIsBusyTaskAsync(async () =>
                    {
                        Ret = await LoginAndCheckIfCanEditSettings(Selected);
                    });

                    if (!Ret)
                        return;
                    else
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent();
                        SetResult(Result.Ok, i);
                        Finish();
                    }

                    break;
                }
                case UsersActivityModes.IsAllowedToCloseApplication:
                {
                    bool Ret = false;

                    await RunIsBusyTaskAsync(async () =>
                    {
                        Ret = await LoginAndCheckIfCanCloseApplication(Selected);
                    });

                    if (!Ret)
                        return;
                    else
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent();
                        SetResult(Result.Ok, i);
                        Finish();
                    }

                    break;
                }
            }
        }

        private async Task<bool> LoginAndCheckIfCanEditSettings(OperatorRow Selected)
        {
            try
            {
                if (!Selected.bMozeZarzadzacUprawnieniamiMobilnymi && Selected.ID != Int32.MaxValue)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.user_cannot_edit_settings,
                        Resource.Raw.sound_error
                    );

                    return false;
                }
                else
                {
                    var Res = await Helpers.AlertAsyncWithPrompt(
                        this,
                        Resource.String.users_getpass,
                        null,
                        "",
                        InputType.Password
                    );

                    if (!Res.Ok)
                        return false;

                    return await CheckPassword(Selected, Res.Text);
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return false;
            }
        }

        private async Task<bool> BarcodeLoginAndCheckIfCanEditSettings(string Barcode)
        {
            try
            {
                int ID = Globalne.operatorBL.SprawdźLegitymacjęOperatora(Barcode);

                if (ID == -1)
                    return false;

                OperatorRow Selected = Globalne.operatorBL.PobierzOperatorRow(ID);

                if (!Selected.bMozeZarzadzacUprawnieniamiMobilnymi && Selected.ID != Int32.MaxValue)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.user_cannot_edit_settings,
                        Resource.Raw.sound_error
                    );

                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return false;
            }
        }

        private async Task<bool> LoginAndCheckIfCanCloseApplication(OperatorRow Selected)
        {
            try
            {
                UstawienieMobilneOpe MobOpe = null;
                UserSettings Set = null;

                if (Selected.ID != Int32.MaxValue)
                {
                    MobOpe = Globalne.menuBL.PobierzUstawienieMobOpe(Selected.idUstawienieMobOpe);

                    if (Selected.idUstawienieMobOpe == -1 || MobOpe.strUstawienie == "")
                    {
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.user_cannot_leave_app,
                            Resource.Raw.sound_error
                        );

                        return false;
                    }

                    Set = (UserSettings)
                        JsonConvert.DeserializeObject(MobOpe.strUstawienie, typeof(UserSettings));
                }

                if (Selected.ID != Int32.MaxValue && !Set.CanCloseApp)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.user_cannot_leave_app,
                        Resource.Raw.sound_error
                    );

                    return false;
                }
                else
                {
                    var Res = await Helpers.AlertAsyncWithPrompt(
                        this,
                        Resource.String.users_getpass,
                        null,
                        "",
                        InputType.Password
                    );

                    if (!Res.Ok)
                        return false;

                    return await CheckPassword(Selected, Res.Text);
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return true;
            }
        }

        private async Task<bool> BarcodeLoginAndCheckIfCanCloseApplication(string Barcode)
        {
            try
            {
                int ID = Globalne.operatorBL.SprawdźLegitymacjęOperatora(Barcode);

                if (ID == -1)
                    return false;

                OperatorRow Selected = Globalne.operatorBL.PobierzOperatorRow(ID);

                UstawienieMobilneOpe MobOpe = null;
                UserSettings Set = null;

                if (Selected.ID != Int32.MaxValue)
                {
                    MobOpe = Globalne.menuBL.PobierzUstawienieMobOpe(Selected.idUstawienieMobOpe);

                    if (Selected.idUstawienieMobOpe == -1 || MobOpe.strUstawienie == "")
                    {
                        await Helpers.AlertAsyncWithConfirm(
                            this,
                            Resource.String.user_cannot_leave_app,
                            Resource.Raw.sound_error
                        );

                        return false;
                    }

                    Set = (UserSettings)
                        JsonConvert.DeserializeObject(MobOpe.strUstawienie, typeof(UserSettings));
                }

                if (Selected.ID != Int32.MaxValue && !Set.CanCloseApp)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.user_cannot_leave_app,
                        Resource.Raw.sound_error
                    );

                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return true;
            }
        }

        private async Task<bool?> TryUpdate()
        {
            try
            {
                if (IsBusy || IsSwitchingActivity)
                    return null;

                IsBusy = true;

                string WersjaNaSerwerze = (string)
                    Helpers.HiveInvoke(
                        typeof(WMSServerAccess.Aktualizacje.AktualizacjeBL),
                        "PobierzNajnowsząWersję",
                        new object[] { "Android" }
                    );

                if (WersjaNaSerwerze == "0")
                {
                    IsBusy = false;
                    return true;
                }
                var versionOnServerWithoutDots = WersjaNaSerwerze.Replace(".", "");
                var programVersionWithoutDots = Globalne.AppVer.Replace(".", "");

                if (
                    Convert.ToInt32(versionOnServerWithoutDots)
                    > Convert.ToInt32(programVersionWithoutDots)
                )
                {
                    bool Resp = await Helpers.QuestionAlertAsync(
                        this,
                        Resource.String.update_available,
                        Resource.Raw.sound_alert
                    );

                    if (!Resp)
                    {
                        IsBusy = false;
                        return false;
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                    {
                        Helpers.CheckAndRequestPermissions(
                            this,
                            new string[]
                            {
                                Manifest.Permission.WriteExternalStorage,
                                Manifest.Permission.ReadExternalStorage,
                                Manifest.Permission.RequestInstallPackages,
                                Manifest.Permission.InstallPackages
                            }
                        );
                    }

                    Helpers.ShowProgressDialog(GetString(Resource.String.updating));

#nullable enable
                    string? Path = await Task.Factory.StartNew(() => GetData(WersjaNaSerwerze));

#nullable disable

                    Helpers.HideProgressDialog();

                    if (Path == null)
                    {
                        IsBusy = false;
                        return false;
                    }

                    Helpers.EnableNavigationBar(this);

                    if (Build.VERSION.SdkInt < BuildVersionCodes.N)
                    {
                        Android.Net.Uri uri = Android.Net.Uri.Parse(@"file://" + Path);

                        Intent intentS = new Intent(Intent.ActionInstallPackage);
                        intentS.SetData(uri);
                        intentS.SetFlags(ActivityFlags.GrantReadUriPermission);
                        StartActivity(intentS);
                    }
                    else
                    {
                        Android.Net.Uri u = FileProvider.GetUriForFile(
                            this,
                            ApplicationContext.PackageName + ".provider",
                            new Java.IO.File(Path)
                        );
                        Intent intentS = new Intent(Intent.ActionInstallPackage);
                        intentS.SetData(u);
                        intentS.SetFlags(ActivityFlags.GrantReadUriPermission);
                        StartActivity(intentS);
                    }

                    Finish();
                    return null;
                }
                else
                {
                    IsBusy = false;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                IsBusy = false;
                return false;
            }
        }

#nullable enable
        private string? GetData(string WersjaNaSerwerze)
        {
            try
            {
                List<AktualizacjaO> Akt = Globalne.aktualizacjeBL.PobierzAktualizację(
                    "Android",
                    WersjaNaSerwerze
                );

                if (
                    Android.OS.Environment.DirectoryDownloads == null
                    || Android.OS.Environment.GetExternalStoragePublicDirectory(
                        Android.OS.Environment.DirectoryDownloads
                    ) == null
                )
                    return null;
                else
                {
                    string Path = System.IO.Path.Combine(
                        Android
                            .OS.Environment.GetExternalStoragePublicDirectory(
                                Android.OS.Environment.DirectoryDownloads
                            )
                            .ToString(),
                        Akt[0].NazwaPliku
                    );

                    if (File.Exists(Path))
                        File.Delete(Path);

                    if (Path.EndsWith(".zip"))
                    {
                        if (System.IO.File.Exists(Path.Replace(".zip", ".apk")))
                            System.IO.File.Delete(Path.Replace(".zip", ".apk"));

                        string Dir = System.IO.Path.GetDirectoryName(Path);
                        string File = Helpers.ZIPExtractAPK(Akt[0].Plik, Dir);
                        Path = System.IO.Path.Combine(Dir, File);

                        if (Path == "" || !System.IO.File.Exists(Path))
                            return null;
                        else
                            return Path;
                    }
                    else if (Path.EndsWith(".apk"))
                    {
                        File.WriteAllBytes(Path, Akt[0].Plik);

                        return Path;
                    }
                    else
                        return null;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return null;
            }
        }

#nullable disable

        private async Task<bool> CheckNumberOfAvailableLicences()
        {
            try
            {
                int LoggedIn = Globalne.operatorBL.SprawdźIlośćZalogowań();

                if (LoggedIn >= Globalne.Licencja.Stanowisk)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.global_licence_occupied,
                        Resource.Raw.sound_miss
                    );

                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return false;
            }
        }

        private async Task<bool> LogOutElsewhere(int IDOperatora)
        {
            try
            {
                Helpers.PlaySound(this, Resource.Raw.sound_miss);
                bool Resp = await Helpers.QuestionAlertAsync(
                    this,
                    Resource.String.users_already_logged_in,
                    Resource.Raw.sound_miss
                );

                if (Resp)
                    Globalne.operatorBL.WylogujOperatora(IDOperatora);

                return Resp;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return false;
            }
        }

        private async Task<bool> CheckLicence()
        {
            try
            {
                if (!Globalne.Licencja.Aktywna)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.global_licence_inactive,
                        Resource.Raw.sound_error
                    );

                    return false;
                }

                DateTime DateNow = Globalne.ogólneBL.GetDate();
                DateTime TwoMonthsWhen = Globalne.Licencja.TerminLicencji.AddDays(-60);

                if (
                    (
                        (DateNow < Globalne.Licencja.PoczLicencji)
                        || (DateNow > Globalne.Licencja.TerminLicencji)
                    ) && (Globalne.Licencja.LicencjaCzasowa)
                )
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.global_licence_expired,
                        Resource.Raw.sound_error
                    );

                    return false;
                }

                if ((DateNow > TwoMonthsWhen) && (Globalne.Licencja.LicencjaCzasowa))
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        GetString(Resource.String.global_licence_nearly_expired)
                            + " "
                            + (Globalne.Licencja.TerminLicencji - DateTime.Now).Days,
                        Resource.Raw.sound_message
                    );
                }

                return true;
            }
            catch (Exception)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.global_licence_invalid,
                    Resource.Raw.sound_error
                );

                return false;
            }
        }

        private async Task<bool> CheckPassword(OperatorRow Selected, string Pass)
        {
            if (
                !Globalne.operatorBL.SprawdźHasłoOperatora(Selected.ID, Encryption.RSAEncrypt(Pass))
            )
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.users_password_incorrect,
                    Resource.Raw.sound_error
                );

                return false;
            }

            return true;
        }

        async Task<int> TryLoginFromBarcode(string Legitymacja)
        {
            if (!await CheckLicence())
                return -1;

            int ID = Globalne.operatorBL.SprawdźLegitymacjęOperatora(Legitymacja);

            if (ID == -1)
            {
                await Helpers.AlertAsyncWithConfirm(
                    this,
                    Resource.String.users_passport_incorrect,
                    Resource.Raw.sound_error
                );

                return -2;
            }

#pragma warning disable CS0618
            if (
                Globalne.operatorBL.SprawdźCzyZalogowanyNaInnymUrządzeniu(
                    ID,
                    (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        ? CrossDeviceInfo.Current.Id.ToString()
                        : Build.Serial
                )
            )
#pragma warning restore CS0618
            {
                var Res = await LogOutElsewhere(ID);

                if (Res)
                    Globalne.operatorBL.WylogujOperatora(ID);
                else
                    return -1;
            }

            if (!await CheckNumberOfAvailableLicences())
                return -1;

            if (ID == Int32.MaxValue)
            {
                Globalne.Operator = new OperatorVO
                {
                    ID = Int32.MaxValue,
                    Nazwa = "Administrator Systemu",
                    Login = "SYSADM",
                    Jezyk = "PL"
                };
            }
            else
            {
                OperatorVO Operator = Globalne.operatorBL.PobierzOperatora(ID);

                if (Operator.ID == -1 || !Operator.Aktywny)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.users_user_deactivated,
                        Resource.Raw.sound_error
                    );

                    return -1;
                }
                else
                    Globalne.Operator = Operator;
            }

#pragma warning disable CS0618
            Globalne.operatorBL.ZalogujOperatora(
                ID,
                (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    ? CrossDeviceInfo.Current.Id.ToString()
                    : Build.Serial
            );
#pragma warning restore CS0618
            return 0;
        }

        async Task<int> TryLogin(OperatorRow Selected, string Pass)
        {
            if (!await CheckLicence())
                return -1;

            if (!await CheckPassword(Selected, Pass))
                return -2;

#pragma warning disable CS0618
            if (
                Globalne.operatorBL.SprawdźCzyZalogowanyNaInnymUrządzeniu(
                    Selected.ID,
                    (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                        ? CrossDeviceInfo.Current.Id.ToString()
                        : Build.Serial
                )
            )
#pragma warning restore CS0618
            {
                var Res = await LogOutElsewhere(Selected.ID);

                if (Res)
                    Globalne.operatorBL.WylogujOperatora(Selected.ID);
                else
                    return -1;
            }

            if (!await CheckNumberOfAvailableLicences())
                return -1;

            if (Selected.ID == Int32.MaxValue)
            {
                Globalne.Operator = new OperatorVO
                {
                    ID = Int32.MaxValue,
                    Nazwa = "Administrator Systemu",
                    Login = "SYSADM",
                    Jezyk = "PL"
                };
            }
            else
            {
                OperatorVO Operator = Globalne.operatorBL.PobierzOperatora(Selected.ID);

                if (Operator.ID == -1 || !Operator.Aktywny)
                {
                    await Helpers.AlertAsyncWithConfirm(
                        this,
                        Resource.String.users_user_deactivated,
                        Resource.Raw.sound_error
                    );

                    return -1;
                }
                else
                    Globalne.Operator = Operator;
            }

#pragma warning disable CS0618
            Globalne.operatorBL.ZalogujOperatora(
                Selected.ID,
                (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    ? CrossDeviceInfo.Current.Id.ToString()
                    : Build.Serial
            );
#pragma warning restore CS0618
            return 0;
        }

        private async void Refresh_ClickAsync(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        async Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.users_loading));

                List<OperatorRow> Operatorzy = await Task.Factory.StartNew(() => GetData());

                RunOnUiThread(() =>
                {
                    OperatorList.Adapter = new UsersListAdapter(this, Operatorzy);
                    Helpers.SetTextOnTextView(
                        this,
                        ItemCount,
                        GetString(Resource.String.global_liczba_pozycji)
                            + " "
                            + OperatorList.Adapter.Count.ToString()
                    );
                });

                Helpers.HideProgressDialog();

                return true;
            }
            catch (Exception ex)
            {
                Helpers.HideProgressDialog();
                Helpers.HandleError(this, ex);

                return false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private List<OperatorRow> GetData()
        {
            List<OperatorRow> Operatorzy = Globalne.operatorBL.PobierzListęNaTerminal();

            OperatorRow R = Operatorzy.Find(x => x.ID == Int32.MaxValue);

            if (R != null)
                Operatorzy.Remove(R);

            OperatorRow Admin = Globalne.operatorBL.PobierzOperatorRow(Int32.MaxValue);
            Admin.ID = Int32.MaxValue;
            Admin.Login = "SYSADM";
            Admin.Nazwa = "SERWIS";
            Admin.bMozeZarzadzacUprawnieniamiMobilnymi = true;
            Operatorzy.Add(Admin);

            return Operatorzy;
        }

        private void Back_Click(object sender, EventArgs e)
        {
            switch (Mode)
            {
                case UsersActivityModes.LogIn:
                    RunIsBusyAction(
                        () => Helpers.SwitchAndFinishCurrentActivity(this, typeof(MainActivity))
                    );
                    break;
                default:
                {
                    IsSwitchingActivity = true;

                    Intent i = new Intent();
                    SetResult(Result.Canceled, i);
                    Finish();
                    break;
                }
            }
        }

        protected override async void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.OnScan(sender, e);

            if (!Globalne.CurrentSettings.CheckCanBarcodeLogin)
                return;

            if (IsBusy || IsSwitchingActivity)
                return;

            switch (Mode)
            {
                case UsersActivityModes.LogIn:
                {
                    bool? Res = await TryUpdate();

                    #region sprawdzenie czy wersja bazy danych jest zgodna z wersja androida
                    int WersjaBazy = Convert.ToInt32(
                        Helpers.HiveInvoke(
                            typeof(WMSServerAccess.Menu.MenuBL),
                            "PobierzUstawienie",
                            (int)Enums.Ustawienia.WersjaBazy
                        )
                    );

                    if (WersjaBazy != Globalne.WymaganaWersjaBazy && Res == true)
                    {
                        var ResInfo = await Helpers.AlertAsyncWithConfirm(
                            this,
                            GetString(Resource.String.main_activity_versioninsuff)
                                + " "
                                + Globalne.WymaganaWersjaBazy.ToString(),
                            Resource.Raw.sound_alert
                        );
                        if (!ResInfo.Value)
                            return;
                        var ResPass = await Helpers.AlertAsyncWithPrompt(
                            this,
                            Resource.String.users_getpass,
                            null,
                            "",
                            InputType.Password
                        );
                        if (!ResPass.Ok || ResPass.Text != "GamaFA")
                            return;
                    }
                    #endregion

                    if (Res == true)
                        await RunIsBusyTaskAsync(() => DoBarcodeLogin(LastScanData[0]));
                    else if (Res == false)
                        Helpers.CenteredToast(
                            GetString(Resource.String.update_failed),
                            ToastLength.Long
                        );
                    else if (Res == null)
                        return;

                    break;
                }
                case UsersActivityModes.IsAllowedToEditSettings:
                {
                    bool Ret = false;

                    await RunIsBusyTaskAsync(async () =>
                    {
                        Ret = await BarcodeLoginAndCheckIfCanEditSettings(LastScanData[0]);
                    });

                    if (!Ret)
                        return;
                    else
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent();
                        SetResult(Result.Ok, i);
                        Finish();
                    }
                    break;
                }
                case UsersActivityModes.IsAllowedToCloseApplication:
                {
                    bool Ret = false;

                    await RunIsBusyTaskAsync(async () =>
                    {
                        Ret = await BarcodeLoginAndCheckIfCanCloseApplication(LastScanData[0]);
                    });

                    if (!Ret)
                        return;
                    else
                    {
                        IsSwitchingActivity = true;

                        Intent i = new Intent();
                        SetResult(Result.Ok, i);
                        Finish();
                    }
                    break;
                }
            }
        }
    }

    internal class UsersListAdapter : BaseAdapter<OperatorRow>
    {
        readonly List<OperatorRow> Items;
        readonly Activity Ctx;

        public UsersListAdapter(Activity Ctx, List<OperatorRow> Items)
            : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override OperatorRow this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Operator = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_users, null);

            view.FindViewById<TextView>(Resource.Id.users_list_login).Text = Operator.Login;
            view.FindViewById<TextView>(Resource.Id.users_list_username).Text = Operator.Nazwa;

            return view;
        }
    }
}
