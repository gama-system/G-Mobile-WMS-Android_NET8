using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Symbol.XamarinEMDK;

namespace G_Mobile_Android_WMS
{
    public class ProfileSetter : Java.Lang.Object, EMDKManager.IEMDKListener
    {
        private EMDKManager emdkManager;
        private ProfileManager profileManager;
        private readonly string ProfileName;

        public ProfileSetter(Context context, string ProfileNameToSet)
        {
            ProfileName = ProfileNameToSet;
            EMDKResults results = EMDKManager.GetEMDKManager(context, this);
            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                
                // If there is a problem initializing throw an exception
                //throw new InvalidOperationException("Unable to initialize EMDK Manager");
            }
        }

        public void OnOpened(EMDKManager _emdkManager)
        {
            this.emdkManager = _emdkManager;
            profileManager = (ProfileManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Profile);
            profileManager.ProcessProfile(ProfileName, ProfileManager.PROFILE_FLAG.Set, (string[])null);

            emdkManager.Release();
            emdkManager = null;
            profileManager = null;

            this.Dispose();

        }

        public void OnClosed()
        {
            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }

            profileManager = null;
        }
    }
}