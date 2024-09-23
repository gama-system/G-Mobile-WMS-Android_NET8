using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WMS_DESKTOP_API;
using WMS_Model.Interfejsy;

namespace G_Mobile_Android_WMS
{
    public static class ServerConnection
    {
        public static bool Connected = false;

        public static int Connect()
        {
            // TESTOWANIE POALCZENIA
            //try
            //{
            //    WMS_DESKTOP_API.PolaczenieApi.PolaczenieApi.UstawBazowyAdres(
            //        "https://10.1.0.236:6446/"
            //    );
            //    IAutentykacja autentykacja =
            //        new WMS_DESKTOP_API.PolaczenieApi.EndpointAcces.AutentykacjaEndpoint();
            //    String token = autentykacja.Autentykuj("Terminal1", "Terminal1");
            //    Console.WriteLine("Token: " + token);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine("Endpoint: " + e.Message);
            //}

            Thread WT = new Thread(new ThreadStart(() => ConnectInternal()));
            WT.Start();

            bool Finished = WT.Join(5000);
            if (!Finished)
            {
                WT.Abort();
            }

            return Connected ? 0 : -1;
        }

        static int ConnectInternal()
        {
            //todo: dodac poaclzenie zz serwem
            return -1;
            //try
            //{
            //    if (Globalne.CurrentTerminalSettings == null)
            //        return -1;

            //    Globalne.client = new Hive.Rpc.Client();

            //    Globalne.client.Connect(
            //        "tcp://"
            //            + Globalne.CurrentTerminalSettings.IP
            //            + ":"
            //            + Globalne.CurrentTerminalSettings.Port
            //    );

            //    System.Net.NetworkCredential nc = new System.Net.NetworkCredential();

            //    nc.UserName = Globalne.CurrentTerminalSettings.User;
            //    nc.Password = Globalne.CurrentTerminalSettings.Password;
            //    nc.Domain = "";

            //    Globalne.client.Authenticate(nc);

            //    Connected = true;
            //    return 0;
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("Blad poleczenia api: " + ex.Message);
            //    Connected = false;
            //    return -1;
            //}
        }

        public static void CloseConnections()
        {
            //todo: dostwac do api


            //try
            //{
            //    if (ServerConnection.Connected)
            //    {
            //        if (Serwer.operatorBL != null)
            //            Serwer.operatorBL.ZamknijPołączenie();

            //        if (Serwer.magazynBL != null)
            //            Serwer.magazynBL.ZamknijPołączenie();

            //        if (Serwer.ogólneBL != null)
            //            Serwer.ogólneBL.ZamknijPołączenie();

            //        if (Serwer.podmiotBL != null)
            //            Serwer.podmiotBL.ZamknijPołączenie();

            //        if (Serwer.jednostkaMiaryBL != null)
            //            Serwer.jednostkaMiaryBL.ZamknijPołączenie();

            //        if (Serwer.towarBL != null)
            //            Serwer.towarBL.ZamknijPołączenie();

            //        if (Globalne.wymaganiaBL != null)
            //            Globalne.wymaganiaBL.ZamknijPołączenie();

            //        if (Serwer.lokalizacjaBL != null)
            //            Serwer.lokalizacjaBL.ZamknijPołączenie();

            //        if (Serwer.rejestrBL != null)
            //            Serwer.rejestrBL.ZamknijPołączenie();

            //        if (Serwer.dokumentBL != null)
            //            Serwer.dokumentBL.ZamknijPołączenie();

            //        if (Serwer.przychRozchBL != null)
            //            Serwer.przychRozchBL.ZamknijPołączenie();

            //        if (Serwer.kodykreskoweBL != null)
            //            Serwer.kodykreskoweBL.ZamknijPołączenie();

            //        if (Serwer.partiaBL != null)
            //            Serwer.partiaBL.ZamknijPołączenie();

            //        if (Serwer.paletaBL != null)
            //            Serwer.paletaBL.ZamknijPołączenie();

            //        if (Serwer.funklogBL != null)
            //            Serwer.funklogBL.ZamknijPołączenie();

            //        if (Serwer.menuBL != null)
            //            Serwer.menuBL.ZamknijPołączenie();

            //        if (Serwer.drukarkaBL != null)
            //            Serwer.drukarkaBL.ZamknijPołączenie();
            //    }
            //}
            //catch (Exception) { }
        }

        public static bool PingServer()
        {
            if (ServerConnection.Connected)
            {
                try
                {
                    return Serwer.ogólneBL.Ping();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
                return false;
        }

        public static int CreateObjects()
        {
            //todo: dostować do api
            return 0;

            //try
            //{
            //    Serwer.aktualizacjeBL = (WMSServerAccess.Aktualizacje.AktualizacjeBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Aktualizacje.AktualizacjeBL)
            //        );
            //    Serwer.licencjaBL = (WMSServerAccess.Licencja.LicencjaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Licencja.LicencjaBL)
            //        );
            //    Serwer.ogólneBL = (WMSServerAccess.Ogólne.OgólneBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Ogólne.OgólneBL)
            //        );
            //    Serwer.drukarkaBL = (WMSServerAccess.Drukarka.DrukarkaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Drukarka.DrukarkaBL)
            //        );
            //    Serwer.operatorBL = (WMSServerAccess.Operator.OperatorBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Operator.OperatorBL)
            //        );
            //    Serwer.magazynBL = (WMSServerAccess.Magazyn.MagazynBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Magazyn.MagazynBL)
            //        );
            //    Serwer.podmiotBL = (WMSServerAccess.Podmiot.PodmiotBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Podmiot.PodmiotBL)
            //        );
            //    Serwer.jednostkaMiaryBL = (WMSServerAccess.JednostkaMiary.JednostkaMiaryBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.JednostkaMiary.JednostkaMiaryBL)
            //        );
            //    Serwer.towarBL = (WMSServerAccess.Towar.TowarBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Towar.TowarBL)
            //        );
            //    Globalne.wymaganiaBL = (WMSServerAccess.Wymagania.WymaganiaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Wymagania.WymaganiaBL)
            //        );
            //    Serwer.lokalizacjaBL = (WMSServerAccess.Lokalizacja.LokalizacjaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL)
            //        );
            //    Serwer.rejestrBL = (WMSServerAccess.Rejestr.RejestrBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Rejestr.RejestrBL)
            //        );
            //    Serwer.dokumentBL = (WMSServerAccess.Dokument.DokumentBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Dokument.DokumentBL)
            //        );
            //    Serwer.przychRozchBL = (WMSServerAccess.PrzychRozch.PrzychRozchBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.PrzychRozch.PrzychRozchBL)
            //        );
            //    Serwer.kodykreskoweBL = (WMSServerAccess.KodyKreskowe.KodyKreskoweBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.KodyKreskowe.KodyKreskoweBL)
            //        );
            //    Serwer.partiaBL = (WMSServerAccess.Partia.PartiaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Partia.PartiaBL)
            //        );
            //    Serwer.funklogBL = (WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL)
            //        );
            //    Serwer.paletaBL = (WMSServerAccess.Paleta.PaletaBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Paleta.PaletaBL)
            //        );
            //    Serwer.menuBL = (WMSServerAccess.Menu.MenuBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.Menu.MenuBL)
            //        );
            //    Serwer.numerSeryjnyBL = (WMSServerAccess.NumerSeryjny.NumerSeryjnyBL)
            //        Globalne.client.Activate(
            //            "WMSServerAccess",
            //            typeof(WMSServerAccess.NumerSeryjny.NumerSeryjnyBL)
            //        );

            //    ServerConnection.Connected = true;

            //    return 0;
            //}
            //catch (Exception)
            //{
            //    ServerConnection.Connected = false;
            //    return -1;
            //}
        }
    }
}
