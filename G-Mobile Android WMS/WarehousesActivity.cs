using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Util;
using System.Collections.Generic;
using System.Threading;
using Android.Media;
using Android.Views.InputMethods;

using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;

using Acr.UserDialogs;

using WMSServerAccess.Model;
using Android.Content;
using System.Threading.Tasks;

namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar",  MainLauncher = false, ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustNothing | Android.Views.SoftInput.StateHidden)]
    public class WarehousesActivity : BaseWMSActivity
    {
        FloatingActionButton Back;
        ListView WarehousesList;
        FloatingActionButton Refresh;
        TextView ItemCount;

        Modes Mode;
        int SkipWarehouse = -1;

        internal static class Vars
        {
            public const string Mode = "Mode";
            public const string SkipWarehouse = "SkipWarehouse";
        }

        public enum Modes
        {
            Normal,
            Target
        }

        internal static class Results
        {
            public const string SelectedWarehouseID = "SelectedWarehouseID";
            public const string SelectedWarehouseJson = "SelectedWarehouseJson";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_warehouses);

            Mode = (Modes)Intent.GetIntExtra(Vars.Mode, 0);
            SkipWarehouse = Intent.GetIntExtra(Vars.SkipWarehouse, -1);

            GetAndSetControls();

            Task.Run(() => InsertData());
        }


        private void GetAndSetControls()
        {
            if (Mode == Modes.Normal)
            {
                if (CallingActivity == null)
                    Helpers.SetActivityHeader(this, GetString(Resource.String.warehouses_activity_name_context));
                else
                    Helpers.SetActivityHeader(this, GetString(Resource.String.warehouses_activity_name_change));
            }
            else
                Helpers.SetActivityHeader(this, GetString(Resource.String.warehouses_activity_name_target));

            Back = FindViewById<FloatingActionButton>(Resource.Id.warehouses_btn_prev);
            Refresh = FindViewById<FloatingActionButton>(Resource.Id.warehouses_btn_refresh);
            WarehousesList = FindViewById<ListView>(Resource.Id.list_view_warehouses);
            ItemCount = FindViewById<TextView>(Resource.Id.warehouses_liczba_pozycji);

            Back.Click += Back_Click;
            Refresh.Click += Refresh_Click;
            WarehousesList.ItemClick += List_ItemClick;
        }

        private void SetWarehouse(int ItemPosition)
        {
            MagazynO Selected = (WarehousesList.Adapter as WarehousesListAdapter)[ItemPosition];

            if (CallingActivity != null)
            {
                Intent i = new Intent();
                i.PutExtra(Results.SelectedWarehouseID, Selected.ID);
                i.PutExtra(Results.SelectedWarehouseJson, Helpers.SerializeJSON(Selected));
                SetResult(Result.Ok, i);

                Helpers.FinishCurrentActivityWithIntent(this);
            }
            else
            {
                Globalne.Magazyn = Selected;
                Helpers.SwitchAndFinishCurrentActivity(this, typeof(ModulesActivity));
            }
        }

        private void List_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            RunIsBusyAction(() => SetWarehouse(e.Position));
        }

        async void Refresh_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => InsertData());
        }

        async Task<bool> InsertData()
        {
            try
            {
                await Task.Delay(Globalne.TaskDelay);

                Helpers.ShowProgressDialog(GetString(Resource.String.warehouses_loading));

                List<MagazynO> Magazyny = await System.Threading.Tasks.Task.Factory.StartNew(() => GetData());

                MagazynO Skipped = Magazyny.Find(x => x.ID == SkipWarehouse);

                if (Skipped != null)
                    Magazyny.Remove(Skipped);

                RunOnUiThread(() =>
                {
                    WarehousesList.Adapter = new WarehousesListAdapter(this, Magazyny);
                    Helpers.SetTextOnTextView(this, ItemCount, GetString(Resource.String.global_liczba_pozycji) + " " + WarehousesList.Adapter.Count.ToString());
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

        private List<MagazynO> GetData()
        {
            List<MagazynO> Magazyny = Globalne.magazynBL.PobierzListęDostępnychDlaOperatora(Globalne.Operator.ID);

            if (Mode == Modes.Target)
                Magazyny.Remove(Globalne.Magazyn);

            return Magazyny;
        }

        async System.Threading.Tasks.Task DoLogoutIfCallingActivityIsNull()
        {
            if (CallingActivity != null)
            {
                SetResult(Result.Canceled);
                this.Finish();
            }
            else
            {
                try
                {
                    bool Res = await Helpers.AskToLogOut(this);

                    if (Res)
                    {
                        Globalne.operatorBL.WylogujOperatora(Globalne.Operator.ID);
                        Globalne.Operator = null;
                        Globalne.Magazyn = null;
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(UsersActivity));
                    }
                }
                catch (Exception ex)
                {
                    Helpers.HandleError(this, ex);
                    return;
                }
            }
        }

        async void Back_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => DoLogoutIfCallingActivityIsNull());
        }
    }

    internal class WarehousesListAdapter : BaseAdapter<MagazynO>
    {
        readonly List<MagazynO> Items;
        readonly Activity Ctx;

        public WarehousesListAdapter(Activity Ctx, List<MagazynO> Items) : base()
        {
            this.Ctx = Ctx;
            this.Items = Items;
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override MagazynO this[int position]
        {
            get { return Items[position]; }
        }
        public override int Count
        {
            get { return Items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var Magazyn = Items[position];

            View view = convertView;
            if (view == null)
                view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_warehouses, null);

            view.FindViewById<TextView>(Resource.Id.warehouses_list_name).Text = Magazyn.Nazwa;

            return view;
        }

    }
}

