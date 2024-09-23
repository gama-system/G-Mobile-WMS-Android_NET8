using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using WMSServerAccess.Model;

namespace G_Mobile_Android_WMS.BusinessLogicHelpers
{
    public static class UserSettingGroups
    {
        public async static Task<int> ShowSelectListOfUserGroups(Activity ctx, int Message, bool AddNewOption)
        {
            try
            {
                List<UstawienieMobilneOpe> Settings = Globalne.menuBL.PobierzListęUstawieńMobOpe();

                if (AddNewOption)
                    Settings.Add(new UstawienieMobilneOpe() { ID = -1, strNazwa = ctx.GetString(Resource.String.global_new) });

                string Res = await UserDialogs.Instance.ActionSheetAsync(ctx.GetString(Message),
                                                                         ctx.GetString(Resource.String.global_cancel),
                                                                         "",
                                                                         null,
                                                                         Settings.Select(x => x.strNazwa).ToArray());

                if (Res == ctx.GetString(Resource.String.global_cancel))
                    return -1;
                else
                {
                    UstawienieMobilneOpe Set = Settings.Find(x => x.strNazwa == Res);

                    if (Set.ID == -1)
                    {
                        PromptResult NewResp = await Helpers.AlertAsyncWithPrompt(ctx, Resource.String.users_input_new_setting_group_name, null, "", InputType.Default);

                        if (!NewResp.Ok)
                            return -1;
                        else
                        {
                            UstawienieMobilneOpe OpeMob = new UstawienieMobilneOpe() { strNazwa = NewResp.Text, strUstawienie = JsonConvert.SerializeObject(new UserSettings()) };
                            int ID = Globalne.menuBL.WstawUstawienieMobOpe(OpeMob);

                            if (ID == -1)
                            {
                                Helpers.CenteredToast(ctx.GetString(Resource.String.settings_activity_group_exists), ToastLength.Long);
                                return -1;
                            }

                            return ID;
                        }
                    }
                    else return Set.ID;
                }
            }
            catch (Exception ex)
            {
                Helpers.HandleError(ctx, ex);
                return -1;
            }
        }
    }
}