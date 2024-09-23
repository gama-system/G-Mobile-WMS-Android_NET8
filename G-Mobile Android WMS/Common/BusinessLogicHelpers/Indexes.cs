using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WMS_Model.ModeleDanych;

namespace G_Mobile_Android_WMS.BusinessLogicHelpers
{
    class Indexes
    {
        public static async Task<FunkcjaLogistycznaO> ShowLogisticFunctionsListAndSet(
            Activity ctx,
            int WarehouseID,
            TextView v = null
        )
        {
            List<FunkcjaLogistycznaO> FLogs = Globalne.funklogBL.PobierzListęFunkcjiLogistycznych(
                WarehouseID
            );

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.select_flog),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                FLogs.Select(x => x.strNazwa).ToArray()
            );

            if (Res == ctx.GetString(Resource.String.global_cancel))
                return null;
            else
            {
                FunkcjaLogistycznaO Selected = FLogs.Find(x => x.strNazwa == Res);

                if (v != null)
                {
                    Helpers.SetTextOnTextView(ctx, v, Selected.strNazwa);
                    v.Tag = Selected.ID;
                }

                return Selected;
            }
        }

        public static async Task<RejestrRow> ShowRegistryListAndSet(
            Activity ctx,
            Enums.DocTypes DocType,
            int WarehouseID,
            TextView v = null
        )
        {
            List<RejestrRow> Regs =
                Globalne.rejestrBL.PobierzListęRejestrówDostępnychDlaOperatoraNaTerminalu(
                    Helpers.StringDocType(DocType),
                    WarehouseID,
                    Globalne.Operator.ID
                );

            if (Regs == null)
                return new RejestrRow() { ID = -1 };

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.select_reg),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Regs.Select(x => x.strNazwaRej).ToArray()
            );

            if (Res == ctx.GetString(Resource.String.global_cancel))
                return null;
            else
            {
                RejestrRow Selected = Regs.Find(x => x.strNazwaRej == Res);

                if (v != null && Selected != null)
                {
                    Helpers.SetTextOnTextView(ctx, v, Selected.strNazwaRej);
                    v.Tag = Selected.ID;
                    return Selected;
                }

                return new RejestrRow() { ID = -1 };
            }
        }

        public static async Task<JednostkaPrzeliczO> ShowUnitListAndSet(
            Activity ctx,
            int ArticleID,
            TextView v = null
        )
        {
            List<JednostkaPrzeliczO> Units = Globalne.towarBL.PobierzWszystkieJednostkiTowaru(
                ArticleID
            );

            string Res = await UserDialogs.Instance.ActionSheetAsync(
                ctx.GetString(Resource.String.select_unit),
                ctx.GetString(Resource.String.global_cancel),
                "",
                null,
                Units.Select(x => x.strNazwaJednostkiPrzel).ToArray()
            );

            if (Res == ctx.GetString(Resource.String.global_cancel))
                return null;
            else
            {
                JednostkaPrzeliczO Selected = Units.Find(x => x.strNazwaJednostkiPrzel == Res);

                if (v != null)
                {
                    Helpers.SetTextOnTextView(ctx, v, Selected.strNazwaJednostkiPrzel);
                    v.Tag = Selected.idJednostkaMiaryPrzel;
                }

                return Selected;
            }
        }

        public static async Task<TowarJednostkaO> SelectOneArticleFromMany(
            Activity ctx,
            List<TowarJednostkaO> ToChooseFrom
        )
        {
            try
            {
                List<string> Tow = new List<string>();

                Dictionary<string, TowarJednostkaO> ResDict =
                    new Dictionary<string, TowarJednostkaO>();

                foreach (TowarJednostkaO TowarJedn in ToChooseFrom)
                {
#warning HiveInvoke
                    string T = Helpers
                        .HiveInvoke(
                            typeof(WMSServerAccess.Towar.TowarBL),
                            "PobierzNazwęTowaru",
                            TowarJedn.IDTowaru
                        )
                        .ToString();
                    string S = Globalne.towarBL.PobierzTowar(TowarJedn.IDTowaru).strSymbol;
                    string J = Helpers
                        .HiveInvoke(
                            typeof(WMSServerAccess.JednostkaMiary.JednostkaMiaryBL),
                            "PobierzNazwęJednostki",
                            TowarJedn.IDJednostki
                        )
                        .ToString();

                    string ResE = string.Format("{0} - {1} ({2}) ", S, T, J);

                    Tow.Add(ResE);
                    ResDict[ResE] = TowarJedn;
                }

                string Res = await UserDialogs.Instance.ActionSheetAsync(
                    ctx.GetString(Resource.String.articles_many),
                    ctx.GetString(Resource.String.global_cancel),
                    "",
                    null,
                    Tow.ToArray()
                );

                if (Res == ctx.GetString(Resource.String.global_cancel))
                    return new TowarJednostkaO() { IDTowaru = -2, IDJednostki = -2 };
                else
                    return ResDict[Res];
            }
            catch (Exception ex)
            {
                Helpers.HandleError(ctx, ex);
                return new TowarJednostkaO() { IDTowaru = -4, IDJednostki = -4 };
            }
        }
    }
}
