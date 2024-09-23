using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using G_Mobile_Android_WMS.Enums;

namespace G_Mobile_Android_WMS
{
    public static class DocumentItemActivity_Common
    {
        public static class Vars
        {
            public const string DocType = "DocType";
            public const string DocJSON = "DocJSON";
            public const string ItemJSON = "ItemJSON";
            public const string Mode = "Mode";
            public const string Operation = "Operation";
            public const string BufferSet = "BufferSet";
            public const string FromScanner = "FromScanner";
            public const string BufferType = "BufferType";
        }

        public enum ResultCodes
        {
            ArticlesActivityResult = 10,
            LocationsActivityResult = 20,
            ContractorsActivityResult = 30,
            LocationsActivityResultIn = 40,
            LocationsActivityResultOut = 50,
            PartiaActivityResult = 60,
            PaletaActivityResult = 70,
            PaletaOutActivityResult = 80,
            PaletaInActivityResult = 90,
            SerialActivityResult = 100,
        }

        public static class Results
        {
            public const string WereScanned = "WereScanned";
        }


        public static Dictionary<DocumentItemDisplayElements, int> DisplayElementsDict = new Dictionary<DocumentItemDisplayElements, int>()
        {
            [DocumentItemDisplayElements.Symbol] = Resource.Id.document_item_layout_symbol,
            [DocumentItemDisplayElements.ArticleName] = Resource.Id.document_item_layout_article,
            [DocumentItemDisplayElements.Paleta] = Resource.Id.document_item_layout_paleta,
            [DocumentItemDisplayElements.Partia] = Resource.Id.document_item_layout_partia,
            [DocumentItemDisplayElements.Flog] = Resource.Id.document_item_layout_flog,
            [DocumentItemDisplayElements.SerialNumber] = Resource.Id.document_item_layout_serial,
            [DocumentItemDisplayElements.ProductionDate] = Resource.Id.document_item_layout_proddate,
            [DocumentItemDisplayElements.BestBefore] = Resource.Id.document_item_layout_bestbefore,
            [DocumentItemDisplayElements.OrderedAmount] = Resource.Id.document_item_layout_ordered,
            [DocumentItemDisplayElements.Amount] = Resource.Id.document_item_amount,
            [DocumentItemDisplayElements.Unit] = Resource.Id.document_item_layout_unit,
            [DocumentItemDisplayElements.Location] = Resource.Id.document_item_layout_location,
            [DocumentItemDisplayElements.Lot] = Resource.Id.document_item_layout_lot,
            [DocumentItemDisplayElements.Owner] = Resource.Id.document_item_layout_owner,
            [DocumentItemDisplayElements.InWarehouse] = Resource.Id.document_item_layout_amountinwarehouse,
            [DocumentItemDisplayElements.OnDoc] = Resource.Id.document_item_layout_ondoc,
            [DocumentItemDisplayElements.CanBeAddedToLoc] = Resource.Id.document_item_layout_amountcanbeaddedtoloc,
            [DocumentItemDisplayElements.KodEAN] = Resource.Id.document_item_layout_kodean,
            [DocumentItemDisplayElements.NrKat] = Resource.Id.document_item_layout_NrKat,

        };

        public static Dictionary<DocumentItemFields, int> RequiredElementsDict = new Dictionary<DocumentItemFields, int>()
        {
            [DocumentItemFields.Symbol] = Resource.Id.document_item_symbol,
            [DocumentItemFields.Article] = Resource.Id.document_item_article,
            [DocumentItemFields.Paleta] = Resource.Id.document_item_paleta,
            [DocumentItemFields.PaletaIn] = Resource.Id.document_item_paleta_in,
            [DocumentItemFields.PaletaOut] = Resource.Id.document_item_paleta_out,
            [DocumentItemFields.Partia] = Resource.Id.document_item_partia,
            [DocumentItemFields.Flog] = Resource.Id.document_item_flog,
            [DocumentItemFields.FlogIn] = Resource.Id.document_item_flog_in,
            [DocumentItemFields.FlogOut] = Resource.Id.document_item_flog_out,
            [DocumentItemFields.SerialNumber] = Resource.Id.document_item_serial,
            [DocumentItemFields.Unit] = Resource.Id.document_item_unit,
            [DocumentItemFields.Lot] = Resource.Id.document_item_lot,
            [DocumentItemFields.Owner] = Resource.Id.document_item_owner,
            [DocumentItemFields.DataProdukcji] = Resource.Id.document_item_proddate,
            [DocumentItemFields.DataPrzydatności] = Resource.Id.document_item_bestbefore,
            [DocumentItemFields.Location] = Resource.Id.document_item_location,
            [DocumentItemFields.LocationIn] = Resource.Id.document_item_location_in,
            [DocumentItemFields.LocationOut] = Resource.Id.document_item_location_out,
            [DocumentItemFields.KodEAN] = Resource.Id.document_item_kodean,
            [DocumentItemFields.NrKat] = Resource.Id.document_item_NrKat,
        };

        public static void SetVisibilityOfFields(Activity ctx, DocTypes DocType, bool Zlecenie, ItemActivityMode Mode)
        {
            Dictionary<DocumentItemDisplayElements, bool> Set = Globalne.CurrentSettings.EditingDocumentItemDisplayElementsListsKAT[DocType];

            foreach (DocumentItemDisplayElements Item in DisplayElementsDict.Keys)
            {
                View v = ctx.FindViewById<View>(DisplayElementsDict[Item]);

                if (v == null)
                    continue;

                var VisibilityToSet = Item switch
                {
                    DocumentItemDisplayElements.Symbol => (Set.ContainsKey(Item) && Set[Item]),
                    DocumentItemDisplayElements.OnDoc => (Set[Item] && Mode != ItemActivityMode.Create),
                    DocumentItemDisplayElements.OrderedAmount => (Set[Item] && Zlecenie),
                    DocumentItemDisplayElements.Flog => (Set[Item] && Globalne.CurrentSettings.FunkcjeLogistyczne),
                    DocumentItemDisplayElements.Paleta => (Set[Item] && Globalne.CurrentSettings.Palety),
                    DocumentItemDisplayElements.Partia => (Set[Item] && Globalne.CurrentSettings.Partie),
                    _ => Set[Item],
                };

                v.Visibility = VisibilityToSet ? ViewStates.Visible : ViewStates.Gone;
            }
        }

        public static void SetHeaderBasedOnMode(Activity ctx, ItemActivityMode Mode)
        {
            switch (Mode)
            {
                case ItemActivityMode.Create:
                    {
                        Helpers.SetActivityHeader(ctx, ctx.GetString(Resource.String.documentitem_activity_name_adding));
                        break;
                    }
                case ItemActivityMode.Split:
                    {
                        Helpers.SetActivityHeader(ctx, ctx.GetString(Resource.String.documentitem_activity_name_splitting));
                        break;
                    }
                case ItemActivityMode.EditAdd:
                    {
                        Helpers.SetActivityHeader(ctx, ctx.GetString(Resource.String.documentitem_activity_name_increasing));
                        break;
                    }
                case ItemActivityMode.Edit:
                    {
                        Helpers.SetActivityHeader(ctx, ctx.GetString(Resource.String.documentitem_activity_name_editing));
                        break;
                    }
            }
        }




    }
}