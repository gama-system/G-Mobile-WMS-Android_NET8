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


namespace G_Mobile_Android_WMS
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked, MainLauncher = false)]

    public class MultiSelectListActivity : BaseWMSActivity
    {
        ListView ListView;

        List<CheckableItem> CheckedItems = new List<CheckableItem>();
        string Header = "";
        string Variable = "";

        internal static class Vars
        {
            public const string Items = "Items";
            public const string Header = "Header";
            public const string Variable = "Variable";
        }

        internal static class Results
        {
            public const string CheckedItems = "CheckedItems";
            public const string Variable = "Variable";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_genericlist);

            Dictionary<string, bool> Items = (Dictionary<string, bool>)Helpers.DeserializePassedJSON(Intent, Vars.Items, typeof(Dictionary<string, bool>));
            CheckedItems = GetCheckableItemsList(Items);

            Header = Intent.GetStringExtra(Vars.Header);
            Variable = Intent.GetStringExtra(Vars.Variable);

            GetAndSetControls();
        }

        private List<CheckableItem> GetCheckableItemsList(Dictionary<string, bool> Dict)
        {
            List<CheckableItem> Items = new List<CheckableItem>();

            foreach (string Key in Dict.Keys)
            {
                Items.Add(new CheckableItem() { Text = Key, Checked = Dict[Key] });
            }

            return Items;
        }

        private Dictionary<string, bool> GetCheckableItemsDict(List<CheckableItem> Items)
        {
            Dictionary<string, bool> Dict = new Dictionary<string, bool>();

            foreach (CheckableItem Item in Items)
            {
                Dict[Item.Text] = Item.Checked;
            }

            return Dict;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, Header);

            FindViewById<FloatingActionButton>(Resource.Id.genericlist_back).Click += Back_Click;
            FindViewById<FloatingActionButton>(Resource.Id.genericlist_ok).Click += OK_Click;

            ListView = FindViewById<ListView>(Resource.Id.genericlist_listview);
            ListView.Adapter = new MultiSelectListAdapter(this, CheckedItems);
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
            i.PutExtra(Results.CheckedItems, Helpers.SerializeJSON(GetCheckableItemsDict(CheckedItems)));
            i.PutExtra(Results.Variable, Variable);

            SetResult(Result.Ok, i);
            Finish();
        }

        internal class MultiSelectListAdapter : BaseAdapter<CheckableItem>
        {
            public List<CheckableItem> Items;
            readonly MultiSelectListActivity Ctx;

            public MultiSelectListAdapter(MultiSelectListActivity Ctx, List<CheckableItem> Items) : base()
            {
                this.Ctx = Ctx;
                this.Items = Items;
            }

            public override long GetItemId(int position)
            {
                return position;
            }
            public override CheckableItem this[int position]
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
                    view = Ctx.LayoutInflater.Inflate(Resource.Layout.list_item_multiselect, null);

                view.FindViewById<TextView>(Resource.Id.multiselect_list_text).Text = Pos.Text;

                CheckBox chb = view.FindViewById<CheckBox>(Resource.Id.multiselect_list_checkbox);

                chb.CheckedChange -= Chb_CheckedChange;

                chb.Checked = Pos.Checked;
                chb.Tag = position;

                chb.CheckedChange += Chb_CheckedChange;

                return view;
            }

            private void Chb_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                CheckBox Chb = (sender as CheckBox);
                Items[(int)Chb.Tag].Checked = Chb.Checked;
            }
        }


        public class CheckableItem
        {
            public string Text { get; set; }
            public bool Checked { get; set; }

            public CheckableItem()
            {
                Text = "";
                Checked = false;
            }
        }
    }
}

