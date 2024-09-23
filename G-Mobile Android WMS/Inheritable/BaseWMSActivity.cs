using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Plugin.DeviceInfo;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;
using Xamarin.Essentials;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = ScreenOrientation.Locked
    )]
    public class BaseWMSActivity : AppCompatActivity
    {
        public bool IsBusy { get; set; }
        public bool IsSwitchingActivity { get; set; }
        public int TimesBusy { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.RequestedOrientation = Globalne.CurrentTerminalSettings.Orientation;

            IsBusy = true;
            IsSwitchingActivity = false;
            TimesBusy = 0;

            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            if (Globalne.Operator != null && Helpers.IsNetworkConnected(this))
            {
                try
                {
                    if (
                        !Serwer.operatorBL.SprawdźZalogowanieNaUrządzeniu(
                            Globalne.Operator.ID,
                            CrossDeviceInfo.Current.Id.ToString()
                        )
                    )
                    {
                        IsSwitchingActivity = true;
                        Globalne.Operator = null;
                        Globalne.Magazyn = null;
                        Helpers.PlaySound(this, Resource.Raw.sound_alert);

                        Helpers.CenteredToast(
                            GetString(Resource.String.global_logged_out),
                            ToastLength.Long
                        );

                        StartActivity(typeof(UsersActivity));
                        this.Finish();
                    }
                }
                catch (Exception) { }
            }

            base.OnCreate(savedInstanceState);
            Acr.UserDialogs.UserDialogs.Init(this);
        }

        //protected override void OnDestroy()
        //{
        //    this.Window.CloseAllPanels();
        //    base.OnDestroy();
        //}

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            this.RequestedOrientation = Globalne.CurrentTerminalSettings.Orientation;
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Helpers.EnableNavigationBar(this);
        }

        private void TaskScheduler_UnobservedTaskException(
            object sender,
            UnobservedTaskExceptionEventArgs e
        )
        {
            Helpers.HandleError(this, e.Exception);
            Helpers.EnableNavigationBar(this);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Helpers.HandleError(
                this,
                e.ExceptionObject as Exception,
                Resource.String.unhandled_exception
            );
            Helpers.EnableNavigationBar(this);
        }

        public virtual void OnSettingsChangedAsync()
        {
            return;
        }

        /// <summary>
        ///  TODO - WTF? Do czego to mialo sluzyc i w jaki sposob dzialac?
        /// </summary>
        /// <returns></returns>
        public async Task<bool> CheckAndSetBusy()
        {
            if ((IsBusy || IsSwitchingActivity) && TimesBusy < 2)
            {
                TimesBusy++;
                //Helpers.PlaySound(ApplicationContext, Resource.Raw.sound_miss);
                return false;
            }

            if (TimesBusy >= 2)
            {
                await Task.Delay(Globalne.TaskDelay);
                bool Resp = true; // await Helpers.QuestionAlertAsync(this, Resource.String.global_busy, Resource.Raw.sound_error);

                if (!Resp)
                    return false;

                TimesBusy = 0;
            }

            IsBusy = true;
            return true;
        }

        public bool CheckAndSetBusyAction()
        {
            if ((IsBusy || IsSwitchingActivity) && TimesBusy < 2)
            {
                TimesBusy++;
                Helpers.PlaySound(ApplicationContext, Resource.Raw.sound_miss);
                return false;
            }

            if (TimesBusy >= 2)
                TimesBusy = 0;

            IsBusy = true;
            return true;
        }

        public virtual async Task RunIsBusyTaskAsync(Func<Task> AwaitableTask)
        {
            if (!await CheckAndSetBusy())
                return;

            try
            {
                await AwaitableTask();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
            }
            finally
            {
                IsBusy = false;
                TimesBusy = 0;
            }
        }

        public virtual void RunIsBusyAction(Action a)
        {
            if (!CheckAndSetBusyAction())
                return;

            try
            {
                a();
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
            }
            finally
            {
                IsBusy = false;
                TimesBusy = 0;
            }
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

        public override void OnBackPressed()
        {
            return;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Escape)
                return false;
            else
                return base.OnKeyDown(keyCode, e);
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Android.Content.PM.Permission[] grantResults
        )
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(
                requestCode,
                permissions,
                grantResults
            );
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public override bool DispatchTouchEvent(MotionEvent ev)
        {
            if (this.CurrentFocus != null)
            {
                InputMethodManager imm = (InputMethodManager)
                    this.BaseContext.GetSystemService(Android.Content.Context.InputMethodService);
                imm.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, 0);
            }
            return base.DispatchTouchEvent(ev);
        }
    }
}
