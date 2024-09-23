using System;
using System.Collections.Generic;
using Symbol.XamarinEMDK.Barcode;

namespace G_Mobile_Android_WMS
{
	public interface IBarcodeScannerManager : IDisposable
	{
		public bool Error { get; set; }

		event EventHandler<Scanner.DataEventArgs> ScanReceived;
		public bool IsScannerEnabled { get; set;  }
		void Enable();
		void Disable();
	}
}