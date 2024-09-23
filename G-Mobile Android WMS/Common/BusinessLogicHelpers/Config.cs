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
using Newtonsoft.Json;

namespace G_Mobile_Android_WMS.BusinessLogicHelpers
{
    public static class Config
    {
        public static void GetDatabaseConfig()
        {
            try
            {
                try
                {
                    string Set = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.KonfiguracjaAndroid);
                    
                    
                    WMSSettings New = (WMSSettings)JsonConvert.DeserializeObject(Encryption.AESDecrypt(Set), typeof(WMSSettings));

                    #region OMG!! Dodawanie nowych widoków/tekstow/opisow do istniejacych ustawien (ustawienia dokumentów, widoczność pól)

                    // ogolnie jezeli dodalo sie nowe pola do wyswietlania np Symbol, Nazwa towaru,
                    // to jezeli mielismy zapisane wczesniej ustawienia to mozliwe ze nowo dodane pola sie nie pojawią
                    // tutaj szukamy czy brakuje jakis wpisow odczytanych z konfiguracji a nowych dodanych w programie

                    foreach (var docType in new WMSSettings().EditingDocumentsListDisplayElementsListsINNNR)
                    {
                        var docTypeDataBase = New.EditingDocumentsListDisplayElementsListsINNNR.Where(x => x.Key == docType.Key).FirstOrDefault();
                        for (int i = 0; i < docType.Value.Count; i++)
                        {
                            var displayElement = docType.Value.ToList()[i].Key;
                            if (!docTypeDataBase.Value.ContainsKey(displayElement))
                                docTypeDataBase.Value.Add(displayElement, true);
                        }
                    }

                    foreach (var docType in new WMSSettings().EditingDocumentItemDisplayElementsListsKAT)
                    {
                        var docTypeDataBase = New.EditingDocumentItemDisplayElementsListsKAT.Where(x => x.Key == docType.Key).FirstOrDefault();
                        for (int i = 0; i < docType.Value.Count; i++)
                        {
                            var displayElement = docType.Value.ToList()[i].Key;
                            if (!docTypeDataBase.Value.ContainsKey(displayElement))
                                docTypeDataBase.Value.Add(displayElement, true);
                        }
                    }
                    #endregion

                    if (New != null)
                        Globalne.CurrentSettings = New;
                    else
                        throw new Exception();
                }
                catch (Exception)
                {
                    Globalne.CurrentSettings = new WMSSettings();
                }

                Globalne.CurrentSettings.DecimalSpaces = Convert.ToInt32(Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.MiejscDziesiętnychPoPrzecinkuWIlościach));
                Globalne.CurrentSettings.FunkcjeLogistyczne = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.ObsługaFunkcjiLogistycznych) == "t";
                Globalne.CurrentSettings.Palety = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.ObsługaPalet) == "t";
                Globalne.CurrentSettings.Partie = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.ObsługaPartii) == "t";
                Globalne.CurrentSettings.Lokalizacje = (string)Helpers.HiveInvoke(typeof(WMSServerAccess.Menu.MenuBL), "PobierzUstawienie", (int)Enums.Ustawienia.ObsługaLokalizacji) == "t";
            }
            catch (Exception)
            {
                Globalne.CurrentSettings = new WMSSettings();
            }
        }
    }
}