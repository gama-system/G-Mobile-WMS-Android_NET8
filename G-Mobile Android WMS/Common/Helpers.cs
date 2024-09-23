using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using WMS_DESKTOP_API;
using WMS_DESKTOP_API;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS
{
    public static class Helpers
    {
        public static void SetLanguage(Context cxt, string language)
        {
            Java.Util.Locale.Default = new Java.Util.Locale(language);
#pragma warning disable CS0618 // Type or member is obsolete
            cxt.Resources.Configuration.Locale = Java.Util.Locale.Default;
            cxt.Resources.UpdateConfiguration(
                cxt.Resources.Configuration,
                cxt.Resources.DisplayMetrics
            );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public static void TurnOnScanner()
        {
            try
            {
                switch (Globalne.DeviceType)
                {
                    case Enums.DeviceTypes.Zebra:
                    {
                        if (Globalne.Scanner != null)
                        {
                            Globalne.Scanner.Disable();
                            Globalne.Scanner.Dispose();
                        }

                        Globalne.Scanner = new BarcodeScannerManager(Application.Context);

                        if (Globalne.HasScanner)
                            Globalne.Scanner.Enable();

                        break;
                    }
                    case Enums.DeviceTypes.Newland:
                    {
                        if (Globalne.Scanner != null)
                        {
                            Globalne.Scanner.Disable();
                            Globalne.Scanner.Dispose();
                        }

                        Globalne.Scanner = new BarcodeScannerManagerNewland();

                        if (Globalne.Scanner != null)
                            Globalne.HasScanner = true;

                        break;
                    }
                    default:
                        break;
                }

                Globalne.ScannerError = false;
            }
            catch (Exception)
            {
                Toast.MakeText(
                    Application.Context,
                    "Błąd inicjalizacji skanera!",
                    ToastLength.Long
                );
                Globalne.ScannerError = true;
                return;
            }
        }

        public static void EnableNavigationBar(Context ctx)
        {
            SetEMDKProfileTo(ctx, "EnableNavigationBar");
        }

        public static void DisableNavigationBar(Context ctx)
        {
            SetEMDKProfileTo(ctx, "DisableNavigationBar");
        }

        public static void SetEMDKProfileTo(Context ctx, string ProfileName)
        {
            if (Globalne.DeviceType == Enums.DeviceTypes.Zebra)
                new ProfileSetter(ctx, ProfileName);

            return;
        }

        public static object ObjectCopy(object In, Type type)
        {
            string InStr = JsonConvert.SerializeObject(In);
            return JsonConvert.DeserializeObject(InStr, type);
        }

        private static KodKreskowyZSzablonuO MergeResults(
            KodKreskowyZSzablonuO KodA,
            KodKreskowyZSzablonuO KodB
        )
        {
            KodKreskowyZSzablonuO Out = new KodKreskowyZSzablonuO
            {
                TowaryJednostkiWBazie = new List<TowarJednostkaO>()
            };

            DateTime EmptyDate = new DateTime(1900, 01, 01);

            Out.DataProdukcji =
                KodB.DataProdukcji == EmptyDate ? KodA.DataProdukcji : KodB.DataProdukcji;
            Out.DataPrzydatności =
                KodB.DataPrzydatności == EmptyDate ? KodA.DataPrzydatności : KodB.DataPrzydatności;
            Out.Partia = KodB.Partia == "" ? KodA.Partia : KodB.Partia;
            Out.Producent = KodB.Producent == "" ? KodA.Producent : KodB.Producent;
            Out.Paleta = KodB.Paleta == "" ? KodA.Paleta : KodB.Paleta;
            Out.Towar = KodB.Towar == "" ? KodA.Towar : KodB.Towar;

            if (KodA.TowaryJednostkiWBazie != null)
            {
                foreach (TowarJednostkaO ID in KodA.TowaryJednostkiWBazie)
                {
                    if (!Out.TowaryJednostkiWBazie.Contains(ID))
                        Out.TowaryJednostkiWBazie.Add(ID);
                }
            }

            if (KodB.TowaryJednostkiWBazie != null)
            {
                foreach (TowarJednostkaO ID in KodB.TowaryJednostkiWBazie)
                {
                    if (!Out.TowaryJednostkiWBazie.Contains(ID))
                        Out.TowaryJednostkiWBazie.Add(ID);
                }
            }

            Out.Ilość = KodB.Ilość == 0 ? KodA.Ilość : KodB.Ilość;
            Out.KrajPochodzenia =
                KodB.KrajPochodzenia == "" ? KodA.KrajPochodzenia : KodB.KrajPochodzenia;
            Out.NrSeryjny = KodB.NrSeryjny == "" ? KodA.NrSeryjny : KodB.NrSeryjny;
            Out.NumerZamówienia =
                KodB.NumerZamówienia == "" ? KodA.NumerZamówienia : KodB.NumerZamówienia;
            Out.Lot = KodB.Lot == "" ? KodA.Lot : KodB.Lot;

            return Out;
        }

        public static KodKreskowyZSzablonuO ParseBarcodesAccordingToOrder(
            List<string> Barcodes,
            Enums.DocTypes Type
        )
        {
            KodKreskowyZSzablonuO Ksk = Serwer.kodyKreskoweBL.PustyKodKreskowyZSzablonu();

            List<int> Order = Globalne.CurrentSettings.BarcodeScanningOrder[Type];

            if (Order.Contains((int)Enums.BarcodeOrder.GS1))
            {
                List<string> ToParse = new List<string>();

                for (int i = 0; i < Order.Count; i++)
                    if (Order[i] == Enums.BarcodeOrder.GS1)
                    {
                        if (i <= Barcodes.Count - 1)
                        {
                            ToParse.Add(Barcodes[i]);
                        }
                    }

                Ksk = Serwer.kodyKreskoweBL.ParsujWedługGS1(ToParse);
            }

            for (int i = 0; i < Order.Count; i++)
            {
                if (Barcodes.Count < i + 1)
                    break;

                switch (Order[i])
                {
                    case Enums.BarcodeOrder.Template:
                    {
                        Ksk = MergeResults(
                            Ksk,
                            Serwer.kodyKreskoweBL.WyszukajKodKreskowy(Barcodes[i])
                        );

                        if (Ksk.Towar == "")
                        {
                            var kodKreskowy = Serwer.kodyKreskoweBL.PobierzKodKreskowyZNrKat(
                                Barcodes[i]
                            );
                            if (kodKreskowy.strKod != "")
                                Ksk = MergeResults(
                                    Ksk,
                                    Serwer.kodyKreskoweBL.WyszukajKodKreskowy(kodKreskowy.strKod)
                                );
                        }
                        break;
                    }
                    case Enums.BarcodeOrder.GS1:
                        continue;
                    case Enums.BarcodeOrder.Amount:
                    {
                        bool Res = Decimal.TryParse(Barcodes[i], out decimal Am);

                        if (Res)
                            Ksk.Ilość = Am;

                        break;
                    }
                    case Enums.BarcodeOrder.Article:
                    {
                        List<TowarJednostkaO> IDs =
                            Serwer.towarBL.PobierzTowarJednostkęDlaKoduKreskowego(Barcodes[i]);

                        if (IDs.Count != 0)
                        {
                            Ksk.Towar = Barcodes[i];
                            Ksk.TowaryJednostkiWBazie.AddRange(IDs);
                        }
                        break;
                    }
                    case Enums.BarcodeOrder.Lot:
                        Ksk.Lot = Barcodes[i];
                        break;
                    case Enums.BarcodeOrder.Paleta:
                        Ksk.Paleta = Barcodes[i];
                        break;
                    case Enums.BarcodeOrder.Partia:
                        Ksk.Partia = Barcodes[i];
                        break;
                    case Enums.BarcodeOrder.SerialNum:
                        Ksk.NrSeryjny = Barcodes[i];
                        break;
                    default:
                        break;
                }
            }

            return Ksk;
        }

        public static void SetActivityHeader(Activity ctx, string Name)
        {
            TextView NameView = ctx.FindViewById<TextView>(Resource.Id.header_activityname);
            TextView UserView = ctx.FindViewById<TextView>(Resource.Id.header_user);
            TextView MagView = ctx.FindViewById<TextView>(Resource.Id.header_magazyn);

            if (NameView != null)
                SetTextOnTextView(ctx, NameView, Name);

            if (UserView != null)
            {
                if (Globalne.Operator != null)
                    SetTextOnTextView(
                        ctx,
                        UserView,
                        ctx.GetString(Resource.String.header_user)
                            + " "
                            + Globalne.Operator.Login.Trim()
                    );
                else
                    SetTextOnTextView(
                        ctx,
                        UserView,
                        ctx.GetString(Resource.String.header_user) + " - - -"
                    );
            }

            if (MagView != null)
            {
                if (Globalne.Magazyn != null)
                    SetTextOnTextView(
                        ctx,
                        MagView,
                        ctx.GetString(Resource.String.header_mag)
                            + " "
                            + Globalne.Magazyn.Nazwa.Trim()
                    );
                else
                    SetTextOnTextView(
                        ctx,
                        MagView,
                        ctx.GetString(Resource.String.header_mag) + " - - -"
                    );
            }
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            if (
                fi.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    is DescriptionAttribute[] attributes
                && attributes.Any()
            )
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }

        public static bool IsNetworkConnected(Context ctx)
        {
            ConnectivityManager cm = (ConnectivityManager)
                ctx.GetSystemService(Context.ConnectivityService);
            return cm.ActiveNetworkInfo != null && cm.ActiveNetworkInfo.IsConnected;
        }

        public static async System.Threading.Tasks.Task Alert(
            Context Ctx,
            string Message,
            int? Sound = null,
            string Title = null
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            if (Title != null)
                Title = Ctx.GetString(Resource.String.global_alert);

            await UserDialogs.Instance.AlertAsync(
                new AlertConfig().SetMessage(Message).SetTitle(Title)
            );

            return;
        }

        public static async Task Alert(
            Context Ctx,
            int Message,
            int? Sound = null,
            int Title = Resource.String.global_alert
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            await UserDialogs.Instance.AlertAsync(
                new AlertConfig().SetMessage(Ctx.GetString(Message)).SetTitle(Ctx.GetString(Title))
            );

            return;
        }

        public static async Task<bool?> AlertAsyncWithConfirm(
            Context Ctx,
            string Message,
            int? Sound = null,
            string Title = null,
            string Button = null,
            string Cancel = null,
            int? AndroidStyleID = null,
            CancellationToken? tc = null
        )
        {
            try
            {
                if (Sound != null)
                    Helpers.PlaySound(Ctx, Sound);

                if (Title == null)
                    Title = Ctx.GetString(Resource.String.global_alert);

                if (Button == null)
                    Button = Ctx.GetString(Resource.String.global_ok);

                if (Cancel == null)
                    Cancel = Ctx.GetString(Resource.String.global_cancel);

                var Result = await UserDialogs.Instance.ConfirmAsync(
                    new ConfirmConfig() { AndroidStyleId = AndroidStyleID }
                        .SetOkText(Button == "" ? null : Button)
                        .SetMessage(Message)
                        .SetCancelText(Cancel)
                        .SetTitle(Title),
                    tc
                );

                return Result;
            }
            catch (TaskCanceledException)
            {
                return null;
            }
        }

        public static async Task<bool> AlertAsyncWithConfirm(
            Context Ctx,
            int Message,
            int? Sound = null,
            int Title = Resource.String.global_alert,
            int Button = Resource.String.global_ok
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            var Result = await UserDialogs.Instance.ConfirmAsync(
                new ConfirmConfig()
                    .SetOkText(Ctx.GetString(Button))
                    .SetMessage(Ctx.GetString(Message))
                    .SetTitle(Ctx.GetString(Title))
            );

            return Result;
        }

        public static async Task<PromptResult> AlertAsyncWithPrompt(
            Context Ctx,
            string Message,
            int? Sound = null,
            string DefaultText = "",
            InputType InputType = InputType.Default,
            string OKButton = null,
            string ButtonCancel = null
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            if (OKButton == null)
                OKButton = Ctx.GetString(Resource.String.global_ok);

            if (ButtonCancel == null)
                ButtonCancel = Ctx.GetString(Resource.String.global_cancel);

            var Result = await UserDialogs.Instance.PromptAsync(
                new PromptConfig()
                    .SetTitle(Message)
                    .SetOkText(OKButton)
                    .SetCancelText(ButtonCancel)
                    .SetText(DefaultText)
                    .SetInputMode(InputType)
            );

            return Result;
        }

        public static async Task<PromptResult> AlertAsyncWithPrompt(
            Context Ctx,
            int Message,
            int? Sound = null,
            string DefaultText = "",
            InputType InputType = InputType.Password,
            int OKButton = Resource.String.global_ok,
            int ButtonCancel = Resource.String.global_cancel
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            var Result = await UserDialogs.Instance.PromptAsync(
                new PromptConfig()
                    .SetTitle(Ctx.GetString(Message))
                    .SetOkText(Ctx.GetString(OKButton))
                    .SetCancelText(Ctx.GetString(ButtonCancel))
                    .SetText(DefaultText)
                    .SetInputMode(InputType)
            );

            return Result;
        }

        public static async Task<bool> QuestionAlertAsync(
            Context Ctx,
            string Message,
            int? Sound = null,
            string Title = null,
            string YesButton = null,
            string NoButton = null
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            if (Title == null)
                Title = Ctx.GetString(Resource.String.global_alert);

            if (YesButton == null)
                YesButton = Ctx.GetString(Resource.String.global_tak);

            if (NoButton == null)
                NoButton = Ctx.GetString(Resource.String.global_nie);

            var Result = await UserDialogs.Instance.ConfirmAsync(
                new ConfirmConfig()
                    .SetCancelText(NoButton)
                    .SetOkText(YesButton)
                    .SetMessage(Message)
                    .SetTitle(Title)
            );
            return Result;
        }

        public static async Task<bool> QuestionAlertAsync(
            Context Ctx,
            int Message,
            int? Sound = null,
            int Title = Resource.String.global_alert,
            int YesButton = Resource.String.global_tak,
            int NoButton = Resource.String.global_nie
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            var Result = await UserDialogs.Instance.ConfirmAsync(
                new ConfirmConfig()
                    .SetCancelText(Ctx.GetString(NoButton))
                    .SetOkText(Ctx.GetString(YesButton))
                    .SetMessage(Ctx.GetString(Message))
                    .SetTitle(Ctx.GetString(Title))
            );
            return Result;
        }

        public static async Task<bool> QuestionAlertAsyncEtykieta(
            Context Ctx,
            int Message,
            int? Sound = null,
            int Title = Resource.String.global_alert,
            int YesButton = Resource.String.global_tak,
            int NoButton = Resource.String.global_nie
        )
        {
            if (Sound != null)
                Helpers.PlaySound(Ctx, Sound);

            var Result = await UserDialogs.Instance.ConfirmAsync(
                new ConfirmConfig()
                    .SetCancelText(Ctx.GetString(NoButton))
                    .SetOkText(Ctx.GetString(YesButton))
                    .SetMessage(Ctx.GetString(Message))
                    .SetTitle(Ctx.GetString(Title))
            );

            return Result;
        }

        public static async Task<bool> DoesPrintPossible(Context Ctx)
        {
            var result = false;

            DrukarkaO drukarka = Serwer.drukarkaBL.PobierzDrukarkeEtykiet();

            if (drukarka.strNazwa.Trim().Length > 2 && drukarka.strERP.Trim().Length > 2)
            {
                result = true;
            }
            else
            {
                result = false;
                await Alert(Ctx, Resource.String.Etykieta_Wydruk_Niemozliwy);
            }

            return result;
        }

        public static void ShowProgressDialog(string Message)
        {
            UserDialogs.Instance.ShowLoading(Message, MaskType.None);
        }

        public static void HideProgressDialog()
        {
            UserDialogs.Instance.HideLoading();
        }

        public static void OpenDateEditor(Activity ctx, EditText v, DateTime Default)
        {
            Fragments.DatePickerFragment frag = Fragments.DatePickerFragment.NewInstance(
                delegate(DateTime SelectedTime)
                {
                    v.Text = SelectedTime.ToString(Globalne.CurrentSettings.DateFormat);
                },
                Default
            );

#pragma warning disable CS0618
            frag.Show(ctx.FragmentManager, "");
#pragma warning restore CS0618
        }

        public static void CenteredToast(Context ctx, int Message, ToastLength Length)
        {
            Toast T = Toast.MakeText(Application.Context, ctx.GetString(Message), Length);
            T.SetGravity(GravityFlags.Bottom | GravityFlags.Center, 0, 100);

            TextView V = (TextView)T.View.FindViewById(Android.Resource.Id.Message);
            if (V != null)
                V.Gravity = GravityFlags.Center;

            T.Show();
        }

        public static void CenteredToast(string Text, ToastLength Length)
        {
            Toast T = Toast.MakeText(Application.Context, Text, Length);
            T.SetGravity(GravityFlags.Bottom | GravityFlags.Center, 0, 100);

            TextView V = (TextView)T.View.FindViewById(Android.Resource.Id.Message);
            if (V != null)
                V.Gravity = GravityFlags.Center;

            T.Show();
        }

        public static void SwitchAndFinishCurrentActivity(BaseWMSActivity ctx, Type NextActivity)
        {
            if (ctx.IsSwitchingActivity)
                return;

            ctx.IsSwitchingActivity = true;

            var intent = new Intent(ctx, NextActivity);
            intent.SetFlags(ActivityFlags.NewTask);

            ctx.StartActivity(intent);
            ctx.Finish();
        }

        public static void FinishCurrentActivityWithIntent(Activity context)
        {
            context.OverridePendingTransition(
                Resource.Animation.abc_fade_in,
                Resource.Animation.abc_fade_out
            );
            context.Finish();
        }

        public static void OpenMultiListActivity(
            BaseWMSActivity ctx,
            string Variable,
            string Header,
            Dictionary<string, bool> Items,
            int ResultCode
        )
        {
            if (ctx.IsSwitchingActivity)
                return;

            ctx.IsSwitchingActivity = true;

            Intent i = new Intent(ctx, typeof(MultiSelectListActivity));
            i.PutExtra(MultiSelectListActivity.Vars.Items, Helpers.SerializeJSON(Items));
            i.PutExtra(MultiSelectListActivity.Vars.Header, Header);
            i.PutExtra(MultiSelectListActivity.Vars.Variable, Variable);

            ctx.StartActivityForResult(i, ResultCode);
        }

        public static void PlaySound(Context context, int? Res)
        {
            if (!Globalne.CurrentUserSettings.Sounds)
                return;

            Android.Net.Uri uri = Android.Net.Uri.Parse(
                "android.resource://" + context.PackageName + "/" + Res
            );
            Globalne.Player.Reset();
            Globalne.Player.SetDataSource(context, uri);
            Globalne.Player.Prepare();
            Globalne.Player.Start();
        }

        public static void SetTextOnTextView(Activity context, TextView Widget, string Text)
        {
            if (Widget == null)
                return;

            if (Looper.MainLooper.Thread == Java.Lang.Thread.CurrentThread())
            {
                Widget.Text = Text;
            }
            else
            {
                context.RunOnUiThread(() => Widget.Text = Text);
            }
        }

        public static void SetTextOnButton(Activity context, Button Widget, string Text)
        {
            if (Widget == null)
                return;

            if (Looper.MainLooper.Thread == Java.Lang.Thread.CurrentThread())
            {
                Widget.Text = Text;
            }
            else
            {
                context.RunOnUiThread(() => Widget.Text = Text);
            }
        }

        public static void SetTextOnEditText(Activity context, EditText Widget, string Text)
        {
            if (Widget == null)
                return;

            if (Looper.MainLooper.Thread == Java.Lang.Thread.CurrentThread())
            {
                Widget.Text = Text;
            }
            else
            {
                context.RunOnUiThread(() => Widget.Text = Text);
            }
        }

        //public static object HiveInvoke(Type Namespace, string Method, params object[] Params)
        //{
        //    try
        //    {
        //        MethodInfo MI = Namespace.GetMethod(Method);
        //        return JsonConvert.DeserializeObject(
        //            Serwer.ogólneBL.GetAsJson(Method, Namespace, Params).Json,
        //            MI.ReturnType
        //        );
        //    }
        //    catch (DeserializingException)
        //    {
        //        return null;
        //    }
        //}

        public static async Task<bool> AskToLogOut(Activity ctx)
        {
            return await Helpers.QuestionAlertAsync(
                ctx,
                Resource.String.common_logout_message,
                Resource.Raw.sound_message,
                Resource.String.common_logout_title
            );
        }

        public static View GetViewWithTag(ViewGroup view, string Tag)
        {
            for (int i = 0; i < view.ChildCount; i++)
            {
                View v = view.GetChildAt(i);

                if (v is ViewGroup group)
                {
                    var ret = GetViewWithTag(group, Tag);

                    if (ret != null)
                        return ret;
                }
                else if (v.Tag.ToString() == Tag)
                    return v;
            }

            return null;
        }

        public static string SerializeJSON(object o)
        {
            return JsonConvert.SerializeObject(o);
        }

        public static object DeserializePassedJSON(Intent data, string Name, Type DataType)
        {
            return JsonConvert.DeserializeObject(data.GetStringExtra(Name), DataType);
        }

        public static string StringDocType(Enums.DocTypes Type)
        {
            return Type.ToString().Substring(0, 2);
        }

        public static Android.Graphics.Color GetDocStatusColorForStatus(
            Enums.DocumentStatusTypes Status
        )
        {
            return Status switch
            {
                Enums.DocumentStatusTypes.Otwarty => Android.Graphics.Color.WhiteSmoke,
                Enums.DocumentStatusTypes.DoRealizacji => Android.Graphics.Color.Yellow,
                Enums.DocumentStatusTypes.WRealizacji => Android.Graphics.Color.Aquamarine,
                Enums.DocumentStatusTypes.Wykonany => Android.Graphics.Color.Orange,
                Enums.DocumentStatusTypes.Wstrzymany => Android.Graphics.Color.Red,
                Enums.DocumentStatusTypes.Zamknięty => Android.Graphics.Color.Green,
                _ => Android.Graphics.Color.White,
            };
        }

        public static Android.Graphics.Color GetItemStatusColorForStatus(Enums.DocItemStatus Status)
        {
            return Status switch
            {
                Enums.DocItemStatus.Incomplete => Android.Graphics.Color.Yellow,
                Enums.DocItemStatus.Complete => Android.Graphics.Color.Green,
                Enums.DocItemStatus.Over => Android.Graphics.Color.Red,
                _ => Android.Graphics.Color.White,
            };
        }

        public static DateTime GetDefaultDateForField(bool BestBefore, DateTime initialDateTime)
        {
            //DateTime Ret = new DateTime(2999, 12, 31);
            DateTime Ret = initialDateTime;

            try
            {
                if (!BestBefore)
                {
                    if (Globalne.CurrentSettings.InsertProdDate)
                        Ret = Serwer
                            .ogólneBL.GetDate()
                            .AddDays(Globalne.CurrentSettings.DaysToAddToProdDate);
                }
                else
                {
                    if (Globalne.CurrentSettings.InsertBestBeforeDate)
                        Ret = Serwer
                            .ogólneBL.GetDate()
                            .AddDays(Globalne.CurrentSettings.DaysToAddToBestBeforeDate);
                }
            }
            catch (Exception) { }

            return Ret;
        }

        public static string ZIPExtractAPK(byte[] Data, string OutPath)
        {
            try
            {
                string InPath = System.IO.Path.Combine(OutPath, "temp");

                if (File.Exists(InPath))
                    File.Delete(InPath);

                File.WriteAllBytes(InPath, Data);

                string[] Files = Directory.GetFiles(OutPath);

                // Dumb android 5.0 workaround
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(InPath, OutPath);
                }
                catch (Exception) { }

                string[] Files2 = Directory.GetFiles(OutPath);
                string APKToInstall = "";

                foreach (string File in Files2)
                {
                    if (File.EndsWith(".apk") && !Files.Contains(File))
                        APKToInstall = File;
                }

                if (File.Exists(InPath))
                    File.Delete(InPath);

                return APKToInstall;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetFieldName<T, TValue>(Expression<Func<T, TValue>> memberAccess)
        {
            return ((MemberExpression)memberAccess.Body).Member.Name;
        }

        private static bool IsDiskFull(Exception ex)
        {
            const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
            const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);

            return ex.HResult == HR_ERROR_HANDLE_DISK_FULL || ex.HResult == HR_ERROR_DISK_FULL;
        }

        public static void LogErrorToFile(Exception ex)
        {
            const string LogFile = "Fatal.txt";
            const string ErrorTag = "ANDROIDWMSERROR";

            string FilePath = "";
            string EMessage = "";

            try
            {
                var Path = Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryDocuments
                );

                if (!Directory.Exists(Path.AbsolutePath))
                    Directory.CreateDirectory(Path.AbsolutePath);

                WifiManager wifiManager = (WifiManager)
                    Application.Context.GetSystemService(Context.WifiService);
                string wifiInfo = string.Empty;
                if (wifiManager != null)
                    wifiInfo =
                        "LinkSpeed ["
                        + wifiManager?.ConnectionInfo?.LinkSpeed
                        + "]: "
                        + wifiManager?.ConnectionInfo?.Rssi.ToString()
                        + " dBm, status: "
                        + wifiManager?.WifiState.ToString()
                        + ", IP: "
                        + DecodeIpAddress(wifiManager.DhcpInfo.IpAddress)
                        + ", Ping-Pong: "
                        + Serwer.ogólneBL.Ping()
                        + ", NET_ID: "
                        + wifiManager?.ConnectionInfo.NetworkId;

                FilePath = System.IO.Path.Combine(Path.AbsolutePath, LogFile);
                EMessage =
                    " === B Ł Ą D === "
                    + System.Environment.NewLine
                    + " -- "
                    + DateTime.Now.ToShortDateString()
                    + " "
                    + DateTime.Now.ToLongTimeString()
                    + " -- "
                    + System.Environment.NewLine
                    + ex.Message
                    + System.Environment.NewLine
                    + wifiInfo
                    + System.Environment.NewLine
                    + "================="
                    + System.Environment.NewLine
                    + ex.StackTrace
                    + System.Environment.NewLine
                    + System.Environment.NewLine
                    + System.Environment.NewLine;

                File.AppendAllText(FilePath, EMessage);
                Android.Util.Log.Error(ErrorTag, ex.Message + "; " + ex.StackTrace);
            }
            catch (IOException ioex)
            {
                if (IsDiskFull(ioex))
                {
                    File.Delete(FilePath);
                    File.AppendAllText(FilePath, EMessage);
                    Android.Util.Log.Error(ErrorTag, ex.Message + "; " + ex.StackTrace);
                }
            }
            catch (Exception exz)
            {
                Android.Util.Log.Error(ErrorTag, exz.Message + "; " + exz.StackTrace);
            }
        }

        private static string DecodeIpAddress(int ipAddress)
        {
            return $"{(ipAddress & 0xFF)}.{(ipAddress >> 8 & 0xFF)}.{(ipAddress >> 16 & 0xFF)}.{(ipAddress >> 24 & 0xFF)}";
        }

        public static string[] GetListOfUngrantedPerms(Activity ctx, string[] PermNames)
        {
            List<string> Out = new List<string>();

            foreach (string Perm in PermNames)
            {
                if (
                    ctx.CheckSelfPermission(Manifest.Permission.ReadPhoneState)
                    != (int)Permission.Granted
                )
                    Out.Add(Perm);
            }

            return Out.ToArray();
        }

        public static void CheckAndRequestPermissions(Activity ctx, string[] PermNames)
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                string[] Perms = GetListOfUngrantedPerms(ctx, PermNames);

                if (Perms.Count() != 0)
                    ctx.RequestPermissions(Perms, 1);
            }
        }

        public static void HandleError(Activity ctx, Exception ex, int? Title = null)
        {
            try
            {
                if (!(ex is BusinessLogicException))
                    LogErrorToFile(ex);

                if (ex.StackTrace.Contains("Hive.Rpc.Client"))
                {
                    ServerConnection.CloseConnections();
                }

                if (!ServerConnection.PingServer())
                {
                    int Wynik = -1;

                    try
                    {
                        Wynik = ServerConnection.Connect() + ServerConnection.CreateObjects();
                    }
                    catch (Exception)
                    {
                        Wynik = -1;
                    }

                    if (Wynik == 0)
                    {
                        Helpers.LogErrorToFile(
                            new Exception("HandleError -> reconnected connection...")
                        );

                        // wyrzucamy informacje o tym ze stracil polaczenie z serwerem

                        //ctx.RunOnUiThread(
                        //async () =>
                        //{
                        //    await Helpers.AlertAsyncWithConfirm(ctx,
                        //                                        Resource.String.global_reconnected,
                        //                                        Resource.Raw.sound_miss,
                        //                                        Title != null ? (int)Title : Resource.String.global_reconnected);
                        //});
                    }
                    else
                    {
                        ctx.RunOnUiThread(async () =>
                        {
                            await Helpers.AlertAsyncWithConfirm(
                                ctx,
                                Resource.String.global_connectionerror,
                                Resource.Raw.sound_error,
                                Title != null ? (int)Title : Resource.String.global_error
                            );
                        });
                    }

                    return;
                }
                else
                {
                    ctx.RunOnUiThread(async () =>
                    {
                        Helpers.PlaySound(ctx, Resource.Raw.sound_error);
                        await UserDialogs.Instance.ConfirmAsync(
                            new ConfirmConfig()
                                .SetOkText(ctx.GetString(Resource.String.global_ok))
                                .SetMessage(ex.Message)
                                .SetTitle(
                                    ctx.GetString(
                                        Title != null ? (int)Title : Resource.String.global_error
                                    )
                                )
                        );
                    });
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc.Message);
                Helpers.PlaySound(ctx, Resource.Raw.sound_error);
            }
        }
    }
}
