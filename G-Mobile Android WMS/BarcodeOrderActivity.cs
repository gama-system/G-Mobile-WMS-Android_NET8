using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.ExtendedModel;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false
    )]
    public class BarcodeOrderActivity : BaseWMSActivity
    {
        ListView ListView;
        List<int> Order;
        int DocType;

        internal static class Vars
        {
            public const string Order = "Order";
            public const string DocType = "DocType";
        }

        internal static class Results
        {
            public const string Order = "Order";
            public const string DocType = "DocType";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_barcodeorder);

            Order = (List<int>)Helpers.DeserializePassedJSON(Intent, Vars.Order, typeof(List<int>));
            DocType = Intent.GetIntExtra(Vars.DocType, (int)Enums.DocTypes.Error);

            GetAndSetControls();
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.barcodesettings_name));

            FindViewById<FloatingActionButton>(Resource.Id.barcodeorder_back).Click += Back_Click;
            FindViewById<FloatingActionButton>(Resource.Id.barcodeorder_ok).Click += OK_Click;
            FindViewById<FloatingActionButton>(Resource.Id.barcodeorder_plus).Click += PlusClick;
            ListView = FindViewById<ListView>(Resource.Id.genericlist_listview);
            ListView.Adapter = new BarcodeOrderActivityAdapter(this, Order);
            ListView.ItemClick += ListView_ItemClick;
        }

        private async void PlusClick(object sender, EventArgs e)
        {
            Dictionary<string, int> Options = GetOptionsList();

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.barcodesettings_target),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Options.Keys.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                (ListView.Adapter as BarcodeOrderActivityAdapter).Items.Add(Options[Res]);
                (ListView.Adapter as BarcodeOrderActivityAdapter).NotifyDataSetChanged();
            }
        }

        private Dictionary<string, int> GetOptionsList()
        {
            Dictionary<string, int> Options = new Dictionary<string, int>();
            int i = -1;

            Options[GetString(Resource.String.barcodeorder_remove)] = Int32.MinValue;

            while (true)
            {
                string Option = Enums.BarcodeOrder.GetBarcodeOrderName(this, i);

                if (Option == "")
                    break;

                Options[Option] = i;
                i--;
            }

            return Options;
        }

        private async void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            int S = (ListView.Adapter as BarcodeOrderActivityAdapter)[e.Position];
            Dictionary<string, int> Options = GetOptionsList();

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.barcodesettings_target),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Options.Keys.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else if (Res == GetString(Resource.String.barcodeorder_remove))
            {
                (ListView.Adapter as BarcodeOrderActivityAdapter).Items.RemoveAt(e.Position);
                (ListView.Adapter as BarcodeOrderActivityAdapter).NotifyDataSetChanged();
            }
            else
            {
                (ListView.Adapter as BarcodeOrderActivityAdapter).Items[e.Position] = Options[Res];
                (ListView.Adapter as BarcodeOrderActivityAdapter).NotifyDataSetChanged();
            }
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            SetResult(Result.Canceled, i);
            Finish();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            i.PutExtra(
                Results.Order,
                Helpers.SerializeJSON((ListView.Adapter as BarcodeOrderActivityAdapter).Items)
            );
            i.PutExtra(Results.DocType, DocType);

            SetResult(Result.Ok, i);
            Finish();
        }

        internal class BarcodeOrderActivityAdapter : BaseAdapter<int>
        {
            public List<int> Items;
            readonly BarcodeOrderActivity Ctx;

            public BarcodeOrderActivityAdapter(BarcodeOrderActivity Ctx, List<int> Items)
                : base()
            {
                this.Ctx = Ctx;
                this.Items = Items;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override int this[int position]
            {
                get { return Items[position]; }
            }
            public override int Count
            {
                get { return Items.Count; }
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var Pos = Items[position];

                View view = convertView;
                if (view == null)
                    view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_barcode, null);

                view.FindViewById<TextView>(Resource.Id.barcode_list_settingA).Text =
                    Enums.BarcodeOrder.GetBarcodeOrderName(Ctx, (int)Pos);
                view.FindViewById<TextView>(Resource.Id.barcode_list_settingB).Text = "";

                return view;
            }
        }
    }
}
