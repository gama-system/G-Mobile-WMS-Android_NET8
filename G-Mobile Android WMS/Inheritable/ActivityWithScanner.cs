using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using WMS_DESKTOP_API;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked
    )]
    public class ActivityWithScanner : BaseWMSActivity
    {
        private CancellationTokenSource ctts;

        public TextView ScanTarget;
        public ColorFilter ScanTargetColor;
        public View LastTappedView;
        public static Enums.DocTypes DocType;
        BarcodeScannerManagerNewland NewlandScanner;

        public List<int> BarcodeOrder = new List<int>() { Enums.BarcodeOrder.Template };

        private bool ScanClearFlag = true;

        public List<string> LastScanData { get; set; }
        public System.Timers.Timer ScanTimer = new System.Timers.Timer() { Interval = 100 };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ScanTimer.Elapsed += OnScan;
            ctts = new CancellationTokenSource();

            TurnOnNewlandScanner();
        }

        private void TurnOnNewlandScanner()
        {
            if (Globalne.DeviceType == Enums.DeviceTypes.Newland && NewlandScanner == null)
            {
                NewlandScanner = new BarcodeScannerManagerNewland();
                NewlandScanner.DataChanged += BarCodeManager_DataChanged;

                Intent intent = new Intent("ACTION_BAR_SCANCFG");

                //intent.PutExtra("EXTRA_SCAN_NOTY_SND", 1);
                intent.PutExtra("EXTRA_SCAN_MODE", 3);
                intent.PutExtra("EXTRA_SCAN_AUTOENT", 0);
                // wylaczamy PREFIX
                intent.PutExtra("SCAN_PREFIX_ENABLE", 0);
                // wylaczamy SUFFIX
                intent.PutExtra("SCAN_SUFFIX_ENABLE", 0);
                this.SendBroadcast(intent);
                var mFilter = new IntentFilter("nlscan.action.SCANNER_RESULT");
                this.RegisterReceiver(NewlandScanner, mFilter);
            }
        }

        private void BarCodeManager_DataChanged(string newValue)
        {
            OnScanReceived(new List<string> { newValue }, new Scanner.DataEventArgs(null));
        }

        public void EnableScanner()
        {
            try
            {
                if (Globalne.DeviceType == Enums.DeviceTypes.Zebra)
                {
                    if (Globalne.HasScanner)
                    {
                        Globalne.Scanner.ScanReceived -= OnScanReceived;
                        Globalne.Scanner.ScanReceived += OnScanReceived;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Helpers.PlaySound(this, Resource.Raw.sound_alert);
                Helpers.LogErrorToFile(ex);
            }
        }

        protected virtual void OnClickTargettableTextView(object sender, EventArgs e)
        {
            if ((sender as View) == LastTappedView && (sender is TextView))
            {
                ScanTargetColor = (sender as View).Background.ColorFilter;
                (sender as View).Background.SetColorFilter(Color.Red, PorterDuff.Mode.SrcAtop);
                ScanTarget = (TextView)LastTappedView;
                return;
            }
            else if ((sender as View) == ScanTarget)
            {
                if (ScanTarget != null && ScanTargetColor != null)
                    (sender as View).Background.SetColorFilter(ScanTargetColor);

                LastTappedView = (sender as View);
                ScanTarget = null;
                return;
            }
            else
            {
                if (ScanTarget != null && ScanTargetColor != null)
                    (sender as View).Background.SetColorFilter(ScanTargetColor);

                LastTappedView = (sender as View);
                ScanTarget = null;
            }
        }

        protected virtual void OnFocusChangeTargettableTextView(
            object sender,
            View.FocusChangeEventArgs e
        )
        {
            if ((sender as View) == ScanTarget && !(sender as View).IsFocused)
            {
                if (ScanTarget != null && ScanTargetColor != null)
                    (sender as View).Background.SetColorFilter(ScanTargetColor);

                LastTappedView = (sender as View);
                ScanTarget = null;
                return;
            }
            else if ((sender as View).IsFocused)
            {
                LastTappedView = (sender as View);
            }
        }

        private void DoImageScanner(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                Intent i = new Intent(this, typeof(ScannerActivity));
                i.PutExtra(ScannerActivity.Vars.ScanningOrder, BarcodeOrder.ToArray());

                StartActivityForResult(i, (int)Enums.GlobalResultCodes.ScannerActivityResult);
            });
        }

        protected virtual void OnScan(object sender, System.Timers.ElapsedEventArgs e)
        {
            ScanTimer.Stop();
        }

        protected override void OnStart()
        {
            base.OnStart();

            try
            {
                Button ScanButton = FindViewById<Button>(Resource.Id.scanbutton);

                if (
                    Globalne.CurrentSettings != null
                    && Globalne.CurrentSettings.EnableCameraCaptureButton
                )
                {
                    if (ScanButton != null)
                    {
                        if (
                            !Globalne.HasCamera
                            || (
                                Globalne.DeviceType == Enums.DeviceTypes.Other
                                && Globalne.HasScanner
                            )
                        )
                        {
                            ScanButton.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            ScanButton.Click -= DoImageScanner;
                            ScanButton.Click += DoImageScanner;
                        }
                    }
                }
                else
                    ScanButton.Visibility = ViewStates.Gone;
            }
            catch (InvalidOperationException)
            {
                Helpers.PlaySound(this, Resource.Raw.sound_alert);
            }
        }

        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (Globalne.DeviceType == Enums.DeviceTypes.Zebra)
            {
                if ((int)keyCode == 10036)
                {
                    if (Globalne.Scanner.Error)
                    {
                        Helpers.PlaySound(this, Resource.Raw.sound_alert);

                        try
                        {
                            if (Globalne.Scanner != null)
                                Globalne.Scanner.Dispose();

                            Globalne.Scanner = new BarcodeScannerManager(Application.Context);
                            Globalne.Scanner.Enable();
                            Globalne.Scanner.ScanReceived -= OnScanReceived;
                            Globalne.Scanner.ScanReceived += OnScanReceived;
                        }
                        catch (Exception ex)
                        {
                            Helpers.LogErrorToFile(ex);
                        }

                        return base.OnKeyUp(keyCode, e);
                    }
                }
            }

            return base.OnKeyUp(keyCode, e);
        }

        protected virtual async Task<bool> CheckBeforeAssumingScanningPath(List<string> Data)
        {
            if (ScanTarget != null)
            {
                Helpers.SetTextOnTextView(this, ScanTarget, Data.Count == 0 ? "" : Data[0]);
                ScanTarget.Background.SetColorFilter(ScanTargetColor);
                ScanTarget = null;
                LastTappedView = null;
                return false;
            }
            else
                return true;
        }

        protected virtual async void OnScanReceived(object sender, Scanner.DataEventArgs args)
        {
            if (IsBusy || IsSwitchingActivity)
                return;

            List<string> Barcodes = new List<string>();
            IList<ScanDataCollection.ScanData> Data = null;

            if (args != null && args.P0 != null)
                Data = args.P0.GetScanData();

            // dodajemy obsluge skanera Newland, wtedy senderem jest lista string
            if (args != null && args.P0 == null && sender is List<string>)
            {
                Barcodes.Add((sender as List<string>).FirstOrDefault());
            }

            if (Data != null)
            {
                foreach (var D in Data)
                    Barcodes.Add(D.Data);
            }
            // obsluga skanera Newland (dla zachowania kompatybilnosci dalszego kodu ze skanerem Zebra)
            if (Data == null && sender is List<string>)
            {
                Data = new List<ScanDataCollection.ScanData>();
            }

            // dla kodow kreskowych powyżej 30 znaków sprawdz czy nie występują znaki specjalne
            // edit: 24.06.2024, sugestia usuwania specjalnych znakow dla firmy SOPP (etykiety IKEA) na => 29
            if (
                Globalne.CurrentSettings.BarcodeScanningRemoveSpecialCharacters
                && Barcodes[0].Length >= 29
            )
            {
                string barcodeFixed = "";
                foreach (var code in Barcodes[0].ToCharArray())
                {
                    if (code.ToString() != "")
                        barcodeFixed += code;
                }
                Barcodes[0] = barcodeFixed;
            }
            #region IKEA - obsługa dlugich kodow SSCC zawierajaca w sobie kod kreskowy towaru

            // kodIkea                        => kod kreskowy (EAN) towaru w WMS
            //"240702135455015791242430280" => 70213545
            //"240702135455015791242430280"   => 70213545
            if (
                (Barcodes[0].Length == 29 || Barcodes[0].Length == 27)
                && Barcodes[0].StartsWith("240")
            )
            {
                var kodEan = Barcodes[0].Substring(3, 8);
                if (Serwer.kodyKreskoweBL.WyszukajKodKreskowy(kodEan).Towar != "")
                    Barcodes[0] = kodEan;
            }
            #endregion
            if (ScanTarget != null)
            {
                Helpers.SetTextOnTextView(this, ScanTarget, Barcodes.Count == 0 ? "" : Barcodes[0]);
                ScanTarget.Background.SetColorFilter(ScanTargetColor);
                ScanTarget = null;
                LastTappedView = null;
                return;
            }

            if (LastScanData == null || LastScanData.Count == 0)
            {
                if (!await CheckBeforeAssumingScanningPath(Barcodes))
                    return;
            }

            if (ScanClearFlag)
            {
                LastScanData = new List<string>();
                ScanClearFlag = false;
            }

            if (Data == null)
                LastScanData.Add("");
            else
            {
                foreach (string Code in Barcodes)
                {
                    if (!LastScanData.Contains(Code))
                        LastScanData.Add(Code);
                }
            }

            // jezeli nie Error to przypisz domyslnie skanowanie po Template...
            if (DocType != Enums.DocTypes.Error)
                BarcodeOrder = Globalne.CurrentSettings?.BarcodeScanningOrder[DocType];

            if (LastScanData.Count >= BarcodeOrder.Count())
            {
                ctts.Cancel();
                ScanTimer.Start();
                ScanClearFlag = true;
            }
            else
            {
                ctts.Cancel();
                ctts = new CancellationTokenSource();

                var btnKoniec =
                    LastScanData.Count >= BarcodeOrder.Count - 1
                        ? GetString(Resource.String.global_end)
                        : GetString(Resource.String.global_skip);

                // nie pozwala anulowac skanowania kolejnych elementow, wymusza skanowanie, ukrywa przycik "Koniec"
                btnKoniec = Globalne.CurrentSettings.BarcodeScanningOrderForce ? "" : btnKoniec;

                bool? Resp = await Helpers.AlertAsyncWithConfirm(
                    this,
                    GetString(Resource.String.global_scanning_now)
                        + " "
                        + Enums.BarcodeOrder.GetBarcodeOrderName(
                            this,
                            BarcodeOrder[LastScanData.Count]
                        ),
                    null,
                    GetString(Resource.String.global_youscanned)
                        + " "
                        + LastScanData.Count
                        + " / "
                        + BarcodeOrder.Count(),
                    btnKoniec,
                    null,
                    Resource.Style.AlertDialogCustom,
                    ctts.Token
                );

                if (Resp == null)
                    return;
                else if (Resp == true)
                {
                    if (LastScanData.Count >= BarcodeOrder.Count - 1)
                    {
                        ScanTimer.Start();
                        ScanClearFlag = true;
                    }
                    else
                    {
                        OnScanReceived(null, null);
                        return;
                    }
                }
                else
                {
                    ScanClearFlag = true;
                }
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            LastScanData = null;

            if (
                requestCode == (int)Enums.GlobalResultCodes.ScannerActivityResult
                && resultCode == Result.Ok
            )
            {
                LastScanData = data.GetStringArrayExtra(ScannerActivity.Results.ScannedCode)
                    .ToList();

                if (ScanTarget != null)
                {
                    Helpers.SetTextOnTextView(this, ScanTarget, LastScanData[0]);
                    ScanTarget.Background.SetColorFilter(ScanTargetColor);
                    ScanTarget = null;
                    LastTappedView = null;
                    return;
                }
                else
                {
                    Task<bool> Res = CheckBeforeAssumingScanningPath(LastScanData);

                    if (Res.Result)
                    {
                        ScanTimer.Start();
                    }
                }
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (Globalne.ScannerError)
            {
                Helpers.TurnOnScanner();
            }

            if (
                Globalne.Scanner != null
                && Globalne.DeviceType == Enums.DeviceTypes.Zebra
                && Globalne.HasScanner
            )
            {
                Globalne.Scanner.ScanReceived -= OnScanReceived;
                Globalne.Scanner.ScanReceived += OnScanReceived;
            }
            if (Globalne.Scanner != null && Globalne.DeviceType == Enums.DeviceTypes.Newland)
            {
                NewlandScanner.DataChanged -= BarCodeManager_DataChanged;
                NewlandScanner.DataChanged += BarCodeManager_DataChanged;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            if (
                Globalne.Scanner != null
                && Globalne.DeviceType == Enums.DeviceTypes.Zebra
                && Globalne.HasScanner
            )
            {
                Globalne.Scanner.ScanReceived -= OnScanReceived;
            }
            if (Globalne.Scanner != null && Globalne.DeviceType == Enums.DeviceTypes.Newland)
            {
                NewlandScanner.DataChanged -= BarCodeManager_DataChanged;
            }
        }

        protected override void OnRestart()
        {
            base.OnRestart();

            Helpers.TurnOnScanner();

            if (
                Globalne.Scanner != null
                && Globalne.DeviceType == Enums.DeviceTypes.Zebra
                && Globalne.HasScanner
            )
            {
                Globalne.Scanner.ScanReceived += OnScanReceived;
            }
            if (Globalne.Scanner != null && Globalne.DeviceType == Enums.DeviceTypes.Newland)
            {
                NewlandScanner.DataChanged += BarCodeManager_DataChanged;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (
                Globalne.Scanner != null
                && Globalne.DeviceType == Enums.DeviceTypes.Zebra
                && Globalne.HasScanner
            )
            {
                Globalne.Scanner.ScanReceived -= OnScanReceived;
            }
            if (Globalne.Scanner != null && Globalne.DeviceType == Enums.DeviceTypes.Newland)
            {
                NewlandScanner.DataChanged -= BarCodeManager_DataChanged;
            }
        }

        public override async System.Threading.Tasks.Task RunIsBusyTaskAsync(
            Func<System.Threading.Tasks.Task> AwaitableTask
        )
        {
            if (!await CheckAndSetBusy())
                return;

            try
            {
                await AwaitableTask();
                TimesBusy = 0;
            }
            // Handle unhandled errors
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public override void RunIsBusyAction(Action a)
        {
            if (!CheckAndSetBusyAction())
                return;

            try
            {
                a();
                TimesBusy = 0;
            }
            // Handle unhandled errors
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
