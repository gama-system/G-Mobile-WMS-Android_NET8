using System;
using Android;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Hardware;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;
using EDMTDev.ZXingXamarinAndroid;


using System.Threading;
using Android.Content;
using System.Collections.Generic;
using System.Linq;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class ScannerActivity : BaseWMSActivity
    {
        public ZXingScannerView ViewFinder;
        public TextView NumBarcodes;
        public TextView BarcodeNext;
        public FloatingActionButton OK;
        public List<int> BarcodeOrder = new List<int>()
        {
            Enums.BarcodeOrder.Template
        };

        public List<string> Scanned = new List<string>();

        internal static class Vars
        {
            public const string ScanningOrder = "NumOfCodesScannedMax";
        }

        internal static class Results
        {
            public const string ScannedCode = "ScannedCode";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.RequestedOrientation = ScreenOrientation.Portrait;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_scanner);

            BarcodeOrder = Intent.GetIntArrayExtra(Vars.ScanningOrder).ToList();

            GetAndSetControls();

            if (Globalne.HasCamera)
            {
                if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                {
                    if (CheckSelfPermission(Manifest.Permission.Camera) != (int)Permission.Granted)
                    {
                        RequestPermissions(new String[] { Manifest.Permission.Camera }, 1);
                    }
                    else
                        StartScan();
                }
                else
                    StartScan();
            }

            IsBusy = false;
        }

        protected override void OnDestroy()
        {
            try
            {
                base.OnDestroy();
                ViewFinder.StopCamera();
            }
            catch (Exception)
            {

            }
        }

        public override void OnRequestPermissionsResult(int ReqCode, string[] Perms, Permission[] GrantedResults)
        {
            if (ReqCode == 1)
            {
                if ((GrantedResults.Length == 1) && (GrantedResults[0] == Permission.Granted))
                    StartScan();
            }
            else
            {
                base.OnRequestPermissionsResult(ReqCode, Perms, GrantedResults);
            }
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.scanner_activity_name));

            FindViewById<FloatingActionButton>(Resource.Id.BtnZamknijViewFinder).Click += BtnZamknijViewFinder_Click;

            OK = FindViewById<FloatingActionButton>(Resource.Id.ScannerScanningOK);
            OK.Click += OK_Click;

            NumBarcodes = FindViewById<TextView>(Resource.Id.ScanningNum);
            BarcodeNext = FindViewById<TextView>(Resource.Id.ScanningNext);

            if (BarcodeOrder.Count == 1)
            {
                OK.Visibility = Android.Views.ViewStates.Gone;
                NumBarcodes.Visibility = Android.Views.ViewStates.Gone;
                BarcodeNext.Visibility = Android.Views.ViewStates.Gone;
            }
            else
                SetBarcodesNum(0);


            ViewFinder = FindViewById<ZXingScannerView>(Resource.Id.viewfinder);
        }

        public void SetBarcodesNum(int Num)
        {
            if (Scanned.Count >= BarcodeOrder.Count - 1)
                OK.SetImageDrawable(GetDrawable(Resource.Drawable.checkmark));
            else
                OK.SetImageDrawable(GetDrawable(Resource.Drawable.arrow_next));

            BarcodeNext.Text = GetString(Resource.String.global_scanning_now) + " " + Enums.BarcodeOrder.GetBarcodeOrderName(this, BarcodeOrder[Scanned.Count]);
            NumBarcodes.Text = GetString(Resource.String.global_youscanned) + " " + Num + " / " + BarcodeOrder.Count;
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (Scanned.Count >= BarcodeOrder.Count - 1)
            {
                if (IsBusy || IsSwitchingActivity)
                    return;

                this.IsSwitchingActivity = true;

                Intent i = new Intent();
                i.PutExtra(ScannerActivity.Results.ScannedCode, Scanned.ToArray());
                SetResult(Result.Ok, i);

                Finish();
            }
            else
            {
                Scanned.Add("");
                SetBarcodesNum(Scanned.Count);
            }
        }

        public void StartScan()
        {
            try
            {
                ViewFinder.SetResultHandler(new ScanResult(this));
                ViewFinder.StartCamera();
            }
            catch (Exception)
            {
                Helpers.CenteredToast(GetString(Resource.String.global_cameraerror), ToastLength.Short);
            }
        }

        private void BtnZamknijViewFinder_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                SetResult(Result.Canceled);
                this.Finish();
            });
        }
    }

    internal class ScanResult : IResultHandler
    {
        private readonly ScannerActivity cameraActivity;

        public ScanResult(ScannerActivity cameraActivity)
        {
            this.cameraActivity = cameraActivity;
        }

        public void HandleResult(ZXing.Result rawResult)
        {
            Helpers.PlaySound(cameraActivity, Resource.Raw.sound_scan);

            if (!cameraActivity.Scanned.Contains(rawResult.Text))
                cameraActivity.Scanned.Add(rawResult.Text);

            if (cameraActivity.Scanned.Count >= cameraActivity.BarcodeOrder.Count)
            {
                if (cameraActivity.IsBusy || cameraActivity.IsSwitchingActivity)
                    return;

                cameraActivity.IsSwitchingActivity = true;

                Intent i = new Intent();
                i.PutExtra(ScannerActivity.Results.ScannedCode, cameraActivity.Scanned.ToArray());
                cameraActivity.SetResult(Result.Ok, i);

                cameraActivity.Finish();
            }
            else
            {
                cameraActivity.SetBarcodesNum(cameraActivity.Scanned.Count);

                cameraActivity.ViewFinder.StopCamera();
                cameraActivity.StartScan();
            }

        }
    }
}

