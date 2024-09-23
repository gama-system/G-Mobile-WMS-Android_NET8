using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Content.Res;
using Android.Util;
using Android.Views;
using Android.Widget;
using G_Mobile_Android_WMS.Enums;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        MainLauncher = false
    )]
    public class ModulesActivity : BaseWMSActivity
    {
        LinearLayout ButtonContainer;

        readonly System.Timers.Timer Timer = new System.Timers.Timer()
        {
            Interval = Globalne.CurrentSettings.ModulesCheckRefreshRate
        };

        public enum ResultCodes
        {
            WarehousesActivityResult = 10,
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_modules);

            GetAndSetControls();

            Timer.Start();
            IsBusy = false;

            CheckForNewDocuments();
        }

        private void GetAndSetControls()
        {
            try
            {
                Helpers.SetActivityHeader(this, GetString(Resource.String.modules_activity_name));

                FindViewById<FloatingActionButton>(Resource.Id.modules_btn_prev).Click +=
                    Back_Click;
                FindViewById<Button>(Resource.Id.modules_btn_pw).Click += Module_Click;
                FindViewById<Button>(Resource.Id.modules_btn_pz).Click += Module_Click;
                FindViewById<Button>(Resource.Id.modules_btn_rw).Click += Module_Click;
                FindViewById<Button>(Resource.Id.modules_btn_wz).Click += Module_Click;
                FindViewById<Button>(Resource.Id.modules_btn_zl).Click += Module_Click;

                Button MM = FindViewById<Button>(Resource.Id.modules_btn_mm);
                Button IN = FindViewById<Button>(Resource.Id.modules_btn_in);
                Button Stan = FindViewById<Button>(Resource.Id.modules_btn_stan);
                Button Kompletacja = FindViewById<Button>(Resource.Id.modules_btn_kompletacja);

                FloatingActionButton ChangeWarehouse = FindViewById<FloatingActionButton>(
                    Resource.Id.modules_btn_changewarehouse
                );
                FindViewById<FloatingActionButton>(Resource.Id.modules_btn_checkdocs).Click +=
                    ModulesActivity_Click;
                ButtonContainer = FindViewById<LinearLayout>(Resource.Id.modules_button_container);

                // Wyłączenie modułów które nie są przypisane do operatora
                foreach (Enums.Modules m in Enum.GetValues(typeof(Enums.Modules)))
                {
                    if (
                        Globalne.CurrentSettings.Modules.ContainsKey(m)
                            && !Globalne.CurrentUserSettings.Modules[m]
                        || !Globalne.CurrentSettings.Modules[m]
                    )
                    {
                        Button B = (Button)Helpers.GetViewWithTag(ButtonContainer, m.ToString());
                        B.Visibility = ViewStates.Gone;

                        if (B.Parent is LinearLayout)
                        {
                            int C = (B.Parent as LinearLayout).ChildCount;
                            bool fV = false;

                            for (int i = 0; i < C; i++)
                            {
                                View v = (B.Parent as LinearLayout).GetChildAt(i);

                                if (v.Visibility == ViewStates.Visible)
                                    fV = true;
                            }

                            if (!fV)
                                (B.Parent as LinearLayout).Visibility = ViewStates.Gone;
                        }
                    }
                }

                MM.Click += Module_Click;
                IN.Click += Module_Click;
                Stan.Click += Module_Click;
                Kompletacja.Click += Module_Click;
                ChangeWarehouse.Click += ChangeWarehouse_Click;
                Timer.Elapsed += Timer_Elapsed;

                List<MagazynO> Magazyny = Serwer.magazynBL.PobierzListęDostępnychDlaOperatora(
                    Globalne.Operator.ID
                );

                if (Magazyny.Count == 1)
                {
                    MM.Visibility = ViewStates.Gone;
                    ChangeWarehouse.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void ModulesActivity_Click(object sender, EventArgs e)
        {
            Helpers.ShowProgressDialog(GetString(Resource.String.global_wait));

            await Task.Run(() => CheckForNewDocuments());

            Helpers.HideProgressDialog();
        }

        private void ChangeWarehouse_Click(object sender, EventArgs e)
        {
            Intent i = new Intent(this, typeof(WarehousesActivity));
            RunOnUiThread(
                () => StartActivityForResult(i, (int)ResultCodes.WarehousesActivityResult)
            );
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == (int)ResultCodes.WarehousesActivityResult && resultCode == Result.Ok)
            {
                MagazynO Selected = (MagazynO)
                    Helpers.DeserializePassedJSON(
                        data,
                        WarehousesActivity.Results.SelectedWarehouseJson,
                        typeof(MagazynO)
                    );

                if (Selected.ID != -1)
                {
                    Globalne.Magazyn = Selected;
                    Helpers.SetActivityHeader(
                        this,
                        GetString(Resource.String.modules_activity_name)
                    );
                    CheckForNewDocuments();
                }
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Timer.Stop();
            CheckForNewDocuments();
            Timer.Start();
        }

        void CheckForNewDocuments()
        {
            try
            {
                if (IsBusy == true || IsSwitchingActivity == true)
                    return;

                IsBusy = true;

                foreach (Enums.Modules m in Enum.GetValues(typeof(Enums.Modules)))
                {
                    Button B = (Button)Helpers.GetViewWithTag(ButtonContainer, m.ToString());

                    if (B != null)
                    {
                        if (
                            Serwer.dokumentBL.SprawdźCzySąZleconeDokumentyTypu(
                                m.ToString(),
                                Globalne.Operator.ID,
                                Globalne.Magazyn.ID
                            )
                        )
                            B.BackgroundTintList = AppCompatResources.GetColorStateList(
                                this,
                                Resource.Color.floating_button_red
                            );
                        else
                            B.BackgroundTintList = AppCompatResources.GetColorStateList(
                                this,
                                Resource.Color.button_blue
                            );
                    }
                }

                IsBusy = false;
            }
            catch (Exception) { }
        }

        async Task DoLogOut()
        {
            try
            {
                bool Res = await Helpers.AskToLogOut(this);

                if (Res)
                {
                    Serwer.operatorBL.WylogujOperatora(Globalne.Operator.ID);
                    Globalne.Operator = null;
                    Globalne.Magazyn = null;
                    Helpers.SwitchAndFinishCurrentActivity(this, typeof(UsersActivity));
                }
            }
            catch (Exception)
            {
                Globalne.Operator = null;
                Globalne.Magazyn = null;
                Helpers.SwitchAndFinishCurrentActivity(this, typeof(UsersActivity));
                return;
            }
        }

        async void Back_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => DoLogOut());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Timer.Stop();
        }

        protected override void OnStop()
        {
            base.OnStop();
            Timer.Stop();
        }

        protected override void OnResume()
        {
            base.OnResume();
            Timer.Start();
        }

        private void Module_Click(object sender, EventArgs e)
        {
            RunIsBusyAction(() =>
            {
                switch ((sender as Button).Tag.ToString())
                {
                    case "PW":
                    case "RW":
                    case "PZ":
                    case "WZ":
                    case "IN":
                    case "ZL":
                    case "MM":
                    {
                        if (IsSwitchingActivity)
                            return;

                        ActivityWithScanner.DocType = (DocTypes)
                            Enum.Parse(typeof(DocTypes), (sender as Button).Tag.ToString(), true);

                        IsSwitchingActivity = true;

                        Intent i = new Intent(this, typeof(DocumentsActivity));
                        i.PutExtra(
                            DocumentsActivity.Vars.DocType,
                            (int)
                                Enum.Parse(
                                    typeof(Enums.DocTypes),
                                    (sender as Button).Tag.ToString()
                                )
                        );
                        i.SetFlags(ActivityFlags.NewTask);

                        StartActivity(i);
                        Finish();

                        break;
                    }
                    case "STAN":
                    {
                        ActivityWithScanner.DocType = DocTypes.Error;
                        StocksActivity.ExitToModules = true;
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(StocksActivity));
                        break;
                    }
                    case "KOMPLETACJA":
                    {
                        Helpers.SwitchAndFinishCurrentActivity(this, typeof(KompletacjaActivity));
                        break;
                    }
                    default:
                        break;
                }
            });
        }
    }
}
