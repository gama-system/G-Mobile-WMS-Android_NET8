using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Widget;
using G_Mobile_Android_WMS.Controls;
using G_Mobile_Android_WMS.ExtendedModel;
using Newtonsoft.Json;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    [Activity(
        Label = "@string/app_name",
        Theme = "@style/AppTheme.NoActionBar",
        MainLauncher = false,
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Locked,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustPan
            | Android.Views.SoftInput.StateHidden
    )]
    public class UserSettingsActivity : BaseWMSActivity
    {
        CheckBox CanCloseApp;
        CheckBox Sounds;
        CheckBox Can_Delete_Own_Docs;
        CheckBox Can_Delete_Others_Docs;
        CheckBox Can_Delete_Items;
        CheckBox Can_Delete_Items_On_Orders;
        CheckBox Can_Delete_Closed_Docs;
        CheckBox Show_Hidden_Docs;
        CheckBox Show_Different_Color_On_Document;
        TextView Color_Preview_Panel;

        UserSettings Edited;
        UstawienieMobilneOpe EditedDBObject;

        public enum ResultCodes
        {
            ModulesListResult = 10,
        }

        internal static class Vars
        {
            public const string IDSettingGroup = "IDSettingGroup";
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_settings_user);
            int IDSettingGroup = Intent.GetIntExtra(Vars.IDSettingGroup, -1);

            if (GetData(IDSettingGroup))
                GetAndSetControls();
            else
                Finish();

            IsBusy = false;
        }

        private bool GetData(int ID)
        {
            try
            {
                EditedDBObject = Serwer.menuBL.PobierzUstawienieMobOpe(ID);

                if (EditedDBObject.ID != -1 && EditedDBObject.strUstawienie != "")
                {
                    Edited = (UserSettings)
                        JsonConvert.DeserializeObject(
                            EditedDBObject.strUstawienie,
                            typeof(UserSettings)
                        );

                    if (Edited == null)
                        Edited = new UserSettings();

                    return true;
                }
                else
                {
                    Helpers.CenteredToast(
                        GetString(Resource.String.usersettings_removed),
                        ToastLength.Short
                    );
                    return false;
                }
            }
            catch (Exception)
            {
                Edited = new UserSettings();
                return true;
            }
        }

        private void GetAndSetControls()
        {
            Helpers.SetActivityHeader(
                this,
                GetString(Resource.String.users_activity_settings_name)
            );

            CanCloseApp = FindViewById<CheckBox>(Resource.Id.settings_can_close_app);
            Sounds = FindViewById<CheckBox>(Resource.Id.settings_sounds);
            Can_Delete_Own_Docs = FindViewById<CheckBox>(Resource.Id.settings_can_delete_own_docs);
            Can_Delete_Others_Docs = FindViewById<CheckBox>(
                Resource.Id.settings_can_delete_others_docs
            );
            Can_Delete_Items = FindViewById<CheckBox>(Resource.Id.settings_can_delete_items);
            Can_Delete_Closed_Docs = FindViewById<CheckBox>(
                Resource.Id.settings_can_delete_closed_docs
            );
            Can_Delete_Items_On_Orders = FindViewById<CheckBox>(
                Resource.Id.settings_can_delete_items_on_orders
            );

            Show_Hidden_Docs = FindViewById<CheckBox>(Resource.Id.settings_show_hidden_docs);
            Show_Different_Color_On_Document = FindViewById<CheckBox>(
                Resource.Id.settings_show_difference_colors
            );
            Color_Preview_Panel = FindViewById<TextView>(
                Resource.Id.settings_show_difference_colors_value_color
            );

            FindViewById<Button>(Resource.Id.settings_modules).Click += Modules_Click;
            SetupBasedOnCurrentSettings();

            Color_Preview_Panel.AfterTextChanged += Color_Preview_Panel_AfterTextChanged;

            FindViewById<FloatingActionButton>(Resource.Id.SettingsUsersPrev).Click +=
                SettingsBtnPrev_Click;
            FindViewById<FloatingActionButton>(Resource.Id.SettingsUsersSave).Click +=
                SettingsBtnExport_Click;
            FindViewById<FloatingActionButton>(Resource.Id.SettingsUsersDelete).Click +=
                SettingsBtnDelete_Click;
            FindViewById<FloatingActionButton>(Resource.Id.SettingsUsersCopy).Click +=
                SettingsBtnCopy_Click;
        }

        private void Color_Preview_Panel_AfterTextChanged(
            object sender,
            Android.Text.AfterTextChangedEventArgs e
        )
        {
            if (Color_Preview_Panel.Text.Length == 6)
            {
                var color = Android.Graphics.Color.ParseColor("#FF" + Color_Preview_Panel.Text);
                Color_Preview_Panel.SetBackgroundColor(color);
            }
        }

        private async void SettingsBtnCopy_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => DoCopySettings());
        }

        private async Task DoCopySettings()
        {
            try
            {
                int ID = await BusinessLogicHelpers.UserSettingGroups.ShowSelectListOfUserGroups(
                    this,
                    Resource.String.settings_activity_selectsetgrouptocopy,
                    false
                );

                if (ID == -1)
                    return;
                else
                {
                    UstawienieMobilneOpe MobSet = Serwer.menuBL.PobierzUstawienieMobOpe(ID);

                    if (MobSet.ID == -1)
                    {
                        Helpers.CenteredToast(
                            GetString(Resource.String.usersettings_removed),
                            ToastLength.Short
                        );
                        return;
                    }
                    else
                    {
                        Edited = (UserSettings)
                            JsonConvert.DeserializeObject(
                                MobSet.strUstawienie,
                                typeof(UserSettings)
                            );
                        SetupBasedOnCurrentSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void SettingsBtnDelete_Click(object sender, EventArgs e)
        {
            await RunIsBusyTaskAsync(() => DoDelete());
        }

        private async Task DoDelete()
        {
            try
            {
                bool Resp = await Helpers.QuestionAlertAsync(
                    this,
                    Resource.String.usersettings_delete_group,
                    Resource.Raw.sound_message
                );

                if (Resp)
                {
                    Serwer.menuBL.UsuńUstawienieMobilneOpe(EditedDBObject.ID);
                    Intent i = new Intent();
                    SetResult(Result.Ok, i);
                    Finish();
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return;
            }
        }

        private async void SettingsBtnExport_Click(object sender, EventArgs e)
        {
            bool Resp = await Helpers.QuestionAlertAsync(
                this,
                Resource.String.settings_do_export,
                Resource.Raw.sound_message
            );

            int Ret = 0;

            if (Resp)
            {
                await RunIsBusyTaskAsync(async () =>
                {
                    Helpers.ShowProgressDialog(GetString(Resource.String.global_wait));
                    Ret = await Task.Factory.StartNew(() => Do_Export());
                    Helpers.HideProgressDialog();

                    if (Ret == 0)
                    {
                        Helpers.CenteredToast(
                            GetString(Resource.String.settings_exported),
                            ToastLength.Long
                        );
                        Intent i = new Intent();
                        SetResult(Result.Ok, i);
                        Finish();
                    }
                });
            }
        }

        private int Do_Export()
        {
            try
            {
                Edited.Sounds = Sounds.Checked;
                Edited.CanDeleteOwnDocuments = Can_Delete_Own_Docs.Checked;
                Edited.CanDeleteAllDocuments = Can_Delete_Others_Docs.Checked;
                Edited.CanDeleteItems = Can_Delete_Items.Checked;

                Edited.ShowHidenDocumentsEditingByOthers = Show_Hidden_Docs.Checked;
                Edited.ShowDifferenceColorOnDocumentsWhenAnyPositionIsComplete =
                    Show_Different_Color_On_Document.Checked;
                Edited.ColorForEditedPositionsOnDocument = Color_Preview_Panel.Text;

                Edited.CanDeleteItemsOnOrders = Can_Delete_Items_On_Orders.Checked;
                Edited.CanCloseApp = CanCloseApp.Checked;
                Edited.CanDeleteClosedDocuments = Can_Delete_Closed_Docs.Checked;

                EditedDBObject.strUstawienie = JsonConvert.SerializeObject(Edited);

                Serwer.menuBL.AktualizujUstawienieMobilneOpe(EditedDBObject);

                return 0;
            }
            catch (Exception ex)
            {
                Helpers.HandleError(this, ex);
                return -1;
            }
        }

        protected override void OnActivityResult(
            int requestCode,
            [GeneratedEnum] Result resultCode,
            Intent data
        )
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                Dictionary<string, bool> Dict =
                    (Dictionary<string, bool>)
                        Helpers.DeserializePassedJSON(
                            data,
                            MultiSelectListActivity.Results.CheckedItems,
                            typeof(Dictionary<string, bool>)
                        );

                switch (requestCode)
                {
                    case (int)ResultCodes.ModulesListResult:
                    {
                        Dictionary<string, Enums.Modules> ModulesDict =
                            new Dictionary<string, Enums.Modules>();

                        foreach (Enums.Modules Key in Edited.Modules.Keys)
                            ModulesDict[Helpers.GetEnumDescription(Key)] = Key;

                        foreach (string Key in Dict.Keys)
                        {
                            if (ModulesDict.ContainsKey(Key))
                                Edited.Modules[ModulesDict[Key]] = Dict[Key];
                        }

                        break;
                    }
                }
            }
        }

        private void Modules_Click(object sender, EventArgs e)
        {
            Dictionary<string, bool> Dict = new Dictionary<string, bool>();

            foreach (Enums.Modules Key in Enum.GetValues(typeof(Enums.Modules)))
            {
                if (Globalne.CurrentSettings.Modules.ContainsKey(Key))
                    if (Globalne.CurrentSettings.Modules[Key] == true)
                        Dict[Helpers.GetEnumDescription(Key)] = Edited.Modules[Key];
            }

            Helpers.OpenMultiListActivity(
                this,
                "",
                GetString(Resource.String.settings_modules),
                Dict,
                (int)ResultCodes.ModulesListResult
            );
        }

        private void SetupBasedOnCurrentSettings()
        {
            Sounds.Checked = Edited.Sounds;
            Can_Delete_Own_Docs.Checked = Edited.CanDeleteOwnDocuments;
            Can_Delete_Others_Docs.Checked = Edited.CanDeleteAllDocuments;
            Can_Delete_Items.Checked = Edited.CanDeleteItems;
            Can_Delete_Items_On_Orders.Checked = Edited.CanDeleteItemsOnOrders;
            CanCloseApp.Checked = Edited.CanCloseApp;
            Can_Delete_Closed_Docs.Checked = Edited.CanDeleteClosedDocuments;

            Show_Different_Color_On_Document.Checked = Edited.ShowHidenDocumentsEditingByOthers;
            Show_Hidden_Docs.Checked = Edited.ShowHidenDocumentsEditingByOthers;
            Color_Preview_Panel.Text = Edited.ColorForEditedPositionsOnDocument;
        }

        private void SettingsBtnPrev_Click(object sender, EventArgs e)
        {
            Intent i = new Intent();
            SetResult(Result.Ok, i);
            Finish();
        }
    }
}
