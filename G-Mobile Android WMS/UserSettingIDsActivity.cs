using Acr.UserDialogs;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

using WMSServerAccess.Model;
using Android.Content;
using Android.Runtime;
using Android.Views;
using System.Timers;
using System.Threading.Tasks;
using System.Reflection;
using G_Mobile_Android_WMS.ExtendedModel;
using System.ComponentModel;
using System.Collections;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, MainLauncher = false)]

    public class UserSettingIDsActivity : BaseWMSActivity
    {
        ListView ListView;
        List<UstawienieMobilneOpe> Settings = new List<UstawienieMobilneOpe>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_genericlist);

            GetAndSetControls();

            Task.Run(() => InsertData());
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.usersettingids_name));

            FindViewById<FloatingActionButton>(Resource.Id.genericlist_back).Click += Back_Click;
            FindViewById<FloatingActionButton>(Resource.Id.genericlist_ok).Click += OK_Click;
        }

        async Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.global_wait));

                Settings.Clear();
                Settings = Globalne.menuBL.PobierzListęUstawieńMobOpe();
                Settings.Add(new UstawienieMobilneOpe { ID = -1, strNazwa = GetString(Resource.String.global_none) });

                Dictionary<OperatorRow, UstawienieMobilneOpe> UsersDict = await Task.Factory.StartNew(() => GetData());

                RunOnUiThread(() =>
                {
                    ListView = FindViewById<ListView>(Resource.Id.genericlist_listview);
                    ListView.Adapter = new UserSettingIDsActivityAdapter(this, UsersDict);
                    ListView.ItemClick += ListView_ItemClick;
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

        private Dictionary<OperatorRow, UstawienieMobilneOpe> GetData()
        {
            List<OperatorRow> Operatorzy = Globalne.operatorBL.PobierzListęNaTerminal();

            OperatorRow A = Operatorzy.Find(x => x.ID == Int32.MaxValue);

            if (A != null)
                Operatorzy.Remove(A);

            OperatorRow Admin = Globalne.operatorBL.PobierzOperatorRow(Int32.MaxValue);
            Admin.ID = Int32.MaxValue;
            Admin.Login = "SYSADM";
            Admin.Nazwa = "SERWIS";
            Admin.bMozeZarzadzacUprawnieniamiMobilnymi = true;
            Operatorzy.Add(Admin);

            Dictionary<OperatorRow, UstawienieMobilneOpe> Dict = new Dictionary<OperatorRow, UstawienieMobilneOpe>();

            foreach (OperatorRow R in Operatorzy)
                Dict[R] = Settings.Find(x => x.ID == R.idUstawienieMobOpe);
 
            return Dict;
        }

        private async void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            KeyValuePair<OperatorRow, UstawienieMobilneOpe> User = (ListView.Adapter as UserSettingIDsActivityAdapter)[e.Position];
            await RunIsBusyTaskAsync(() => SetUserSettingID(User));
        }

        private async Task SetUserSettingID(KeyValuePair<OperatorRow, UstawienieMobilneOpe> User)
        {
            try
            {
                string Res = await UserDialogs.Instance.ActionSheetAsync(GetString(Resource.String.usersettingids_selectopsetid),
                                                                         GetString(Resource.String.global_cancel),
                                                                         "",
                                                                         null,
                                                                         Settings.Select(x => x.strNazwa).ToArray());

                if (Res == GetString(Resource.String.global_cancel))
                    return;
                else
                {
                    UstawienieMobilneOpe Set = Settings.Find(x => x.strNazwa == Res);

                    if (Set == null)
                        return;
                    else
                    {
                        Globalne.operatorBL.UstawUstawienieMobOpe(User.Key.ID, Set.ID);
                        (ListView.Adapter as UserSettingIDsActivityAdapter).Items[User.Key] = Set;
                        (ListView.Adapter as UserSettingIDsActivityAdapter).NotifyDataSetChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity || IsBusy)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent();
            SetResult(Result.Canceled, i);
            Finish();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            if (IsSwitchingActivity || IsBusy)
                return;

            IsSwitchingActivity = true;

            Intent i = new Intent();
            SetResult(Result.Ok, i);
            Finish();
        }

        internal class UserSettingIDsActivityAdapter : BaseAdapter<KeyValuePair<OperatorRow, UstawienieMobilneOpe>>
        {
            public Dictionary<OperatorRow, UstawienieMobilneOpe> Items;
            readonly UserSettingIDsActivity Ctx;

            public UserSettingIDsActivityAdapter(UserSettingIDsActivity Ctx, Dictionary<OperatorRow, UstawienieMobilneOpe> Items) : base()
            {
                this.Ctx = Ctx;
                this.Items = Items;
            }

            public override long GetItemId(int position)
            {
                return position;
            }
            public override KeyValuePair<OperatorRow, UstawienieMobilneOpe> this[int position]
            {
                get { return Items.ElementAt(position); }
            }
            public override int Count
            {
                get { return Items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var Pos = Items.ElementAt(position);

                View view = convertView;
                if (view == null)
                    view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_barcode, null);

                view.FindViewById<TextView>(Resource.Id.barcode_list_settingA).Text = Pos.Key.Nazwa;
                view.FindViewById<TextView>(Resource.Id.barcode_list_settingB).Text = Pos.Value.strNazwa;

                return view;
            }
        }
    }
}

