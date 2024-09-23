using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace G_Mobile_Android_WMS
{
    public enum ErrorType
    {
        DocumentDeletionError,
        DocumentCreationError,
        ItemCreationError,
        ItemDeletionError,
        ItemEditError,
    }

    public class BusinessLogicException : Exception
    {
        public BusinessLogicException()
        {
        }

        public BusinessLogicException(string message)
            : base(message)
        {
        }

        public BusinessLogicException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }

    public static class AutoException
    {
        public static BusinessLogicException GetException(Context ctx, ErrorType T, int ErrorCode)
        {
            int? Ex = T switch
            {
                ErrorType.DocumentCreationError => BusinessLogicErrors.GetDocumentCreationError(ErrorCode),
                ErrorType.DocumentDeletionError => BusinessLogicErrors.GetDocumentDeletionError(ErrorCode),
                ErrorType.ItemDeletionError => BusinessLogicErrors.GetItemDeletionError(ErrorCode),
                ErrorType.ItemCreationError => BusinessLogicErrors.GetItemCreationEditionError(ErrorCode),
                ErrorType.ItemEditError => BusinessLogicErrors.GetItemCreationEditionError(ErrorCode),
                _ => Resource.String.global_error,
            };

            if (Ex != null)
                return new BusinessLogicException(ctx.GetString((int)Ex));
            else
                return null;
        }
        
        public static BusinessLogicException GetException(Context ctx, int? Ex)
        {
            if (Ex != null)
                return new BusinessLogicException(ctx.GetString((int)Ex));
            else
                return null;
        }

        public static void ThrowIfNotNull(Exception ex)
        {
            if (ex != null)
                throw ex;
        }

        public static void ThrowIfNotNull(Context ctx, ErrorType T, int ErrorCode)
        {
            Exception ex = GetException(ctx, T, ErrorCode);

            if (ex != null)
                throw ex;
        }

        public static void ThrowIfNotNull(Context ctx, int? Ex)
        {
            Exception ex = GetException(ctx, Ex);

            if (ex != null)
                throw ex;
        }

        internal static class BusinessLogicErrors
        {
            public static int? GetDocumentDeletionError(int ErrorCode)
            {
                return ErrorCode switch
                {
                    0 => null,
                    -2 => Resource.String.documents_cantdelete_posused,
                    -3 => Resource.String.documents_cantdelete_used,
                    -4 => Resource.String.documents_cantdelete_beingedited,
                    -20 => Resource.String.documents_cantdelete_locvolumeexceeded,
                    -21 => Resource.String.documents_cantdelete_locmaxweightexceeded,
                    _ => Resource.String.global_error,
                };
            }



            public static int? GetDocumentCreationError(int ErrorCode)
            {
                if (ErrorCode >= 0)
                    return null;

                return ErrorCode switch
                {
                    -1 => Resource.String.creating_documents_registry_not_found,
                    -2 => Resource.String.creating_documents_registry_not_of_correct_type,
                    _ => Resource.String.creating_documents_unknown_error,
                };
            }

            public static int? GetItemCreationEditionError(int ErrorCode)
            {
                if (ErrorCode >= 0)
                    return null;

                return ErrorCode switch
                {
                    -2 => Resource.String.documentitem_creationedition_uniterror,
                    -3 => Resource.String.documentitem_creationedition_amounterror,
                    -4 => Resource.String.documentitem_deletionedition_used,
                    -5 => Resource.String.documentitem_deletionedition_doesntexist,
                    -10 => Resource.String.documentitem_creationedition_docerror,
                    -11 => Resource.String.documentitem_creationedition_locerror,
                    -12 => Resource.String.documentitem_creationedition_partiablocked,
                    -13 => Resource.String.documentitem_creationedition_paletablocked,
                    -14 => Resource.String.documentitem_creationedition_articleblocked,
                    -15 => Resource.String.documentitem_creationedition_locationblocked,
                    -16 => Resource.String.documentitem_creationedition_noarticle,
                    -17 => Resource.String.documentitem_creationedition_volumeexceeded,
                    -18 => Resource.String.documentitem_creationedition_weightexceeded,
                    -19 => Resource.String.documentitem_creationedition_dimensionsexceeded,
                    -20 => Resource.String.documentitem_editiondeletion_volumeexceededonr,
                    -21 => Resource.String.documentitem_editiondeletion_weightexceededonr,
                    _ => Resource.String.creating_documents_unknown_error,
                };
            }

            public static int? GetItemDeletionError(int ErrorCode)
            {
                if (ErrorCode >= 0)
                    return null;

                return ErrorCode switch
                {
                    -2 => Resource.String.documentitem_deletionedition_doesntexist,
                    -4 => Resource.String.documentitem_deletionedition_used,
                    -10 => Resource.String.documentitem_creationedition_docerror,
                    -20 => Resource.String.documentitem_editiondeletion_volumeexceededonr,
                    -21 => Resource.String.documentitem_editiondeletion_weightexceededonr,
                    _ => Resource.String.creating_documents_unknown_error,
                };
            }
        }
    }
}