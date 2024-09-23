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
    public class BarcodeSettingsActivity : BaseWMSActivity
    {
        ListView ListView;
        List<BarcodeSetting> CodeParsingSettings = new List<BarcodeSetting>();
        readonly List<string> Skip = new List<string>()
        {
            nameof(KodKreskowyZSzablonuO.DataProdukcji),
            nameof(KodKreskowyZSzablonuO.DataPrzydatności),
            nameof(KodKreskowyZSzablonuO.TowaryJednostkiWBazie),
            nameof(KodKreskowyZSzablonuO.Producent)
        };

        internal static class Vars
        {
            public const string Settings = "Settings";
        }

        internal static class Results
        {
            public const string Settings = "Settings";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_genericlist);

            Dictionary<string, string> Set =
                (Dictionary<string, string>)
                    Helpers.DeserializePassedJSON(
                        Intent,
                        Vars.Settings,
                        typeof(Dictionary<string, string>)
                    );

            CodeParsingSettings = GetBarcodeItems(Set);

            GetAndSetControls();

            this.IsBusy = false;
        }

        private List<BarcodeSetting> GetBarcodeItems(Dictionary<string, string> Dict)
        {
            List<BarcodeSetting> Items = new List<BarcodeSetting>();

            foreach (string Key in Dict.Keys)
            {
                Items.Add(new BarcodeSetting() { SetA = Key, SetB = Dict[Key] });
            }

            return Items;
        }

        private Dictionary<string, string> GetReturnDictionary(List<BarcodeSetting> Items)
        {
            Dictionary<string, string> Dict = new Dictionary<string, string>();

            foreach (BarcodeSetting Item in Items)
                Dict[Item.SetA] = Item.SetB;

            return Dict;
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(this, GetString(Resource.String.barcodesettings_name));

            FindViewById<FloatingActionButton>(Resource.Id.genericlist_back).Click += Back_Click;
            FindViewById<FloatingActionButton>(Resource.Id.genericlist_ok).Click += OK_Click;

            ListView = FindViewById<ListView>(Resource.Id.genericlist_listview);
            ListView.Adapter = new BarcodeSettingsActivityAdapter(this, CodeParsingSettings);
            ListView.ItemClick += ListView_ItemClick;
        }

        private async void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            BarcodeSetting S = (ListView.Adapter as BarcodeSettingsActivityAdapter)[e.Position];

            if (Skip.Find(x => x == S.SetA) != null)
            {
                Helpers.CenteredToast(
                    GetString(Resource.String.barcodesettings_shouldntedit),
                    ToastLength.Long
                );
                return;
            }

            PropertyDescriptorCollection propertyInfosA;
            PropertyDescriptorCollection propertyInfosB;
            propertyInfosA = TypeDescriptor.GetProperties(typeof(DocumentItemVO));
            propertyInfosB = TypeDescriptor.GetProperties(typeof(PozycjaVO));

            List<string> Options = new List<string>();

            foreach (PropertyDescriptor Prop in propertyInfosA)
            {
                if (Prop.PropertyType != typeof(string))
                    continue;

                if (S.SetB == Prop.Name)
                    Options.Add(">> " + Prop.Name + "<< ");
                else
                {
                    if (
                        Prop.Name != nameof(DocumentItemVO.Base)
                        && Prop.Name != nameof(DocumentItemVO.DefaultAmount)
                        && Prop.Name != nameof(DocumentItemVO.EditMode)
                    )
                        Options.Add(Prop.Name);
                }
            }

            foreach (PropertyDescriptor Prop in propertyInfosB)
            {
                if (Prop.PropertyType != typeof(string))
                    continue;

                if (S.SetB == Prop.Name)
                    Options.Add(">> " + Prop.Name + "<< ");
                else
                    Options.Add(Prop.Name);
            }

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                GetString(Resource.String.barcodesettings_target),
                GetString(Resource.String.global_cancel),
                "",
                null,
                Options.ToArray()
            );

            if (Res == GetString(Resource.String.global_cancel))
                return;
            else
            {
                if (Res.StartsWith("<<"))
                    Res = Res.Replace(">> ", "").Replace(" <<", "");

                S.SetB = Res;

                (ListView.Adapter as BarcodeSettingsActivityAdapter).NotifyDataSetChanged();
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
            i.PutExtra(
                Results.Settings,
                Helpers.SerializeJSON(GetReturnDictionary(CodeParsingSettings))
            );

            SetResult(Result.Ok, i);
            Finish();
        }

        internal class BarcodeSettingsActivityAdapter : BaseAdapter<BarcodeSetting>
        {
            public List<BarcodeSetting> Items;
            readonly BarcodeSettingsActivity Ctx;

            public BarcodeSettingsActivityAdapter(
                BarcodeSettingsActivity Ctx,
                List<BarcodeSetting> Items
            )
                : base()
            {
                this.Ctx = Ctx;
                this.Items = Items;
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override BarcodeSetting this[int position]
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

                view.FindViewById<TextView>(Resource.Id.barcode_list_settingA).Text = Pos.SetA;
                view.FindViewById<TextView>(Resource.Id.barcode_list_settingB).Text = Pos.SetB;

                return view;
            }
        }

        public class BarcodeSetting
        {
            public string SetA { get; set; }
            public string SetB { get; set; }

            public BarcodeSetting()
            {
                SetA = "";
                SetB = "";
            }
        }
    }
}
