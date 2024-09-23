using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Util;
using Android.App;
using Android.Content;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;

using System.Threading.Tasks;

namespace G_Mobile_Android_WMS
{
    public class BarcodeScannerManager : Java.Lang.Object, EMDKManager.IEMDKListener, IBarcodeScannerManager
    {
        // EMDK variables
        private EMDKManager emdkManager;
        private BarcodeManager barcodeManager;
        private Scanner scanner;
        public bool Error { get; set; }

        // IBarcodeScannerManager Properties
        public event EventHandler<Scanner.DataEventArgs> ScanReceived;
        public bool IsScannerEnabled { get; set; }

        public BarcodeScannerManager(Context context)
        {
            this.Error = false;

            EMDKResults results = EMDKManager.GetEMDKManager(context, this);
            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                Error = true;
                // If there is a problem initializing throw an exception
                throw new InvalidOperationException("Unable to initialize EMDK Manager");
            }
        }


        //Metoda dodające dodatkowe dekodery kodów kreskowych
        private void SetDecoders()
        {
            if((scanner!=null) && (scanner.IsEnabled))
            {
                ScannerConfig scannerConfig = scanner.GetConfig();
                scannerConfig.DecoderParams.I2of5.Enabled = true;
                scanner.SetConfig(scannerConfig);
            }
        }
       

        public void Enable()
        {
            if (emdkManager == null)
                return;

            if (barcodeManager != null)
                return;

            if (IsScannerEnabled)
                return;

            try
            {
                int Tries = 5;
                barcodeManager = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);
                scanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);

               
        

                while (scanner == null && Tries != 0)
                {
                    Tries--;
                    scanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);
                }

                if (scanner != null)
                {
                    
                    scanner.Data += OnScanReceived;
                    scanner.Status += OnStatusChanged;

                    scanner.Enable();

                    scanner.TriggerType = Scanner.TriggerTypes.Hard;
                    IsScannerEnabled = true;
                    Error = false;    

                    Globalne.HasScanner = true;

                    
                }
                else
                {
                    Globalne.HasScanner = false;
                    Error = true;
                }
            }
            catch (ScannerException e)
            {
                Log.Debug(this.Class.SimpleName, "Scanner exception:" + e.Result.Description);
                Error = true;
                Globalne.HasScanner = false;
            }
            catch (Exception e)
            {
                Log.Debug(this.Class.SimpleName, "Exception:" + e.Message);
                Error = true;
                Globalne.HasScanner = false;
            }
        }

        public void Disable()
        {
            if (emdkManager == null)
                return;

            if (barcodeManager == null)
                return;

            if (!IsScannerEnabled)
                return;

            try
            {
                scanner.Status -= OnStatusChanged;
                scanner.Data -= OnScanReceived;
                scanner.Disable();

                scanner.Dispose();

                IsScannerEnabled = false;
            }
            catch (ScannerException e)
            {
                Log.Debug(this.Class.SimpleName, "Scanner exception:" + e.Result.Description);
            }
            catch (Exception e)
            {
                Log.Debug(this.Class.SimpleName, "Exception:" + e.Message);
            }

            if (barcodeManager != null)
            {
                emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
            }
            barcodeManager = null;
            scanner = null;
        }

        public void OnClosed()
        {
            if (scanner != null)
            {
                scanner.Disable();
                scanner.Release();
                scanner = null;
            }

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnOpened(EMDKManager manager)
        {
            emdkManager = manager;
            Enable();
        }


        void OnScanReceived(object sender, Scanner.DataEventArgs args)
        {
            var scanDataCollection = args.P0;

            if (scanDataCollection?.Result == ScannerResults.Success)
            {
                ScanReceived?.Invoke(sender, args);
            }
        }

        void OnStatusChanged(object sender, Scanner.StatusEventArgs args)
        {
            try
            {
                if (args?.P0?.State == StatusData.ScannerStates.Idle)
                { 
                    Task.Delay(800);
                    SetDecoders(); //Wywołanie metody dekodujące rodzaje kodów
                    scanner.Read();
                }
            }
            catch (Exception)
            {
            }
        }

   
    }
}