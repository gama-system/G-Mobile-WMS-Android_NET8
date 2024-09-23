using Android.Content;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace G_Mobile_Android_WMS
{
    public delegate void ScannerRecivedEventHandler(string newValue);
    public class BarcodeScannerManagerNewland : BroadcastReceiver, IBarcodeScannerManager
    {
        public bool Error { get;set; } = false;
        public bool IsScannerEnabled { get; set; } = true;

        public event ScannerRecivedEventHandler DataChanged;
        public event EventHandler<Scanner.DataEventArgs> ScanReceived;

        public BarcodeScannerManagerNewland()
        {
            
        }
        public void Disable()
        {
            //if (Globalne.DeviceType == Enums.DeviceTypes.Newland)
            //{
            //    Intent intent = new Intent("nlscan.action.STOP_SCAN”");
            //    intent.PutExtra("EXTRA_TRIG_MODE", 1);
            //    Context.SendBroadcast(intent);
            //    Context.UnregisterReceiver(this);
            //}
            
        }

        public void Enable()
        {
            //if (Globalne.DeviceType == Enums.DeviceTypes.Newland)
            //{
            //    DataChanged += BarCodeManager_DataChanged;
            //    Intent intent = new Intent("nlscan.action.SCANNER_TRIG");
            //    intent.PutExtra("EXTRA_TRIG_MODE", 1);
            //    intent.PutExtra("EXTRA_SCAN_MODE", 3);
            //    intent.PutExtra("SCAN_TIMEOUT", 1);
            //    Context.SendBroadcast(intent);
            //    var mFilter = new IntentFilter("nlscan.action.SCANNER_RESULT");
            //    Context.RegisterReceiver(this, mFilter);
            //}
        }
        
        public override void OnReceive(Context context, Intent intent)
        {
            var scanResult_1 = intent.GetStringExtra("SCAN_BARCODE1");
            //var scanResult_2 = intent.GetStringExtra("SCAN_BARCODE2");
            
            // Raw byte data of the scan result
            var scanResultByte_1 = intent.GetByteArrayExtra("scan_result_one_bytes");
            //var scanResultByte_2 = intent.GetByteArrayExtra("scan_result_two_bytes");

            var barcodeType = intent.GetIntExtra("SCAN_BARCODE_TYPE", -1); // -1:unknown

            var scanStatus = intent.GetStringExtra("SCAN_STATE");


            if ("ok".Equals(scanStatus))
            {
                //Helpers.PlaySound(context, Resource.Raw.sound_scan);
                OnDataChanged(scanResult_1);
                //ScanReceived?.Invoke(this, new Scanner.DataEventArgs(null));
                //Helpers.CenteredToast(scanResult_1, Android.Widget.ToastLength.Long);
                   
            }
            else
            {
                //Helpers.PlaySound(context, Resource.Raw.sound_scan);

                // Failure, e.g. operation timed out
            }
        }

        protected virtual void OnDataChanged(string newValue)
        {
            // Sprawdzenie, czy zdarzenie ma subskrybentów
            if (DataChanged != null)
            {
                // Wywo³anie zdarzenia, przekazuj¹c now¹ wartoœæ
                DataChanged(newValue);
            }
        }
    }
}