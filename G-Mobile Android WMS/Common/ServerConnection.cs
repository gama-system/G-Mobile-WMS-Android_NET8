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
            try
            {
                if (Globalne.CurrentTerminalSettings == null)
                    return -1;

                Globalne.client = new Hive.Rpc.Client();

                Globalne.client.Connect(
                    "tcp://"
                        + Globalne.CurrentTerminalSettings.IP
                        + ":"
                        + Globalne.CurrentTerminalSettings.Port
                );

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential();

                nc.UserName = Globalne.CurrentTerminalSettings.User;
                nc.Password = Globalne.CurrentTerminalSettings.Password;
                nc.Domain = "";

                Globalne.client.Authenticate(nc);

                Connected = true;
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Blad poleczenia api: " + ex.Message);
                Connected = false;
                return -1;
            }
        }

        public static void CloseConnections()
        {
            try
            {
                if (ServerConnection.Connected)
                {
                    if (Globalne.operatorBL != null)
                        Globalne.operatorBL.ZamknijPołączenie();

                    if (Globalne.magazynBL != null)
                        Globalne.magazynBL.ZamknijPołączenie();

                    if (Globalne.ogólneBL != null)
                        Globalne.ogólneBL.ZamknijPołączenie();

                    if (Globalne.podmiotBL != null)
                        Globalne.podmiotBL.ZamknijPołączenie();

                    if (Globalne.jednostkaMiaryBL != null)
                        Globalne.jednostkaMiaryBL.ZamknijPołączenie();

                    if (Globalne.towarBL != null)
                        Globalne.towarBL.ZamknijPołączenie();

                    if (Globalne.wymaganiaBL != null)
                        Globalne.wymaganiaBL.ZamknijPołączenie();

                    if (Globalne.lokalizacjaBL != null)
                        Globalne.lokalizacjaBL.ZamknijPołączenie();

                    if (Globalne.rejestrBL != null)
                        Globalne.rejestrBL.ZamknijPołączenie();

                    if (Globalne.dokumentBL != null)
                        Globalne.dokumentBL.ZamknijPołączenie();

                    if (Globalne.przychrozchBL != null)
                        Globalne.przychrozchBL.ZamknijPołączenie();

                    if (Globalne.kodykreskoweBL != null)
                        Globalne.kodykreskoweBL.ZamknijPołączenie();

                    if (Globalne.partiaBL != null)
                        Globalne.partiaBL.ZamknijPołączenie();

                    if (Globalne.paletaBL != null)
                        Globalne.paletaBL.ZamknijPołączenie();

                    if (Globalne.funklogBL != null)
                        Globalne.funklogBL.ZamknijPołączenie();

                    if (Globalne.menuBL != null)
                        Globalne.menuBL.ZamknijPołączenie();

                    if (Globalne.drukarkaBL != null)
                        Globalne.drukarkaBL.ZamknijPołączenie();
                }
            }
            catch (Exception) { }
        }

        public static bool PingServer()
        {
            if (ServerConnection.Connected)
            {
                try
                {
                    return Globalne.ogólneBL.Ping();
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
            try
            {
                Globalne.aktualizacjeBL = (WMSServerAccess.Aktualizacje.AktualizacjeBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Aktualizacje.AktualizacjeBL)
                    );
                Globalne.licencjaBL = (WMSServerAccess.Licencja.LicencjaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Licencja.LicencjaBL)
                    );
                Globalne.ogólneBL = (WMSServerAccess.Ogólne.OgólneBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Ogólne.OgólneBL)
                    );
                Globalne.drukarkaBL = (WMSServerAccess.Drukarka.DrukarkaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Drukarka.DrukarkaBL)
                    );
                Globalne.operatorBL = (WMSServerAccess.Operator.OperatorBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Operator.OperatorBL)
                    );
                Globalne.magazynBL = (WMSServerAccess.Magazyn.MagazynBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Magazyn.MagazynBL)
                    );
                Globalne.podmiotBL = (WMSServerAccess.Podmiot.PodmiotBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Podmiot.PodmiotBL)
                    );
                Globalne.jednostkaMiaryBL = (WMSServerAccess.JednostkaMiary.JednostkaMiaryBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.JednostkaMiary.JednostkaMiaryBL)
                    );
                Globalne.towarBL = (WMSServerAccess.Towar.TowarBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Towar.TowarBL)
                    );
                Globalne.wymaganiaBL = (WMSServerAccess.Wymagania.WymaganiaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Wymagania.WymaganiaBL)
                    );
                Globalne.lokalizacjaBL = (WMSServerAccess.Lokalizacja.LokalizacjaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Lokalizacja.LokalizacjaBL)
                    );
                Globalne.rejestrBL = (WMSServerAccess.Rejestr.RejestrBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Rejestr.RejestrBL)
                    );
                Globalne.dokumentBL = (WMSServerAccess.Dokument.DokumentBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Dokument.DokumentBL)
                    );
                Globalne.przychrozchBL = (WMSServerAccess.PrzychRozch.PrzychRozchBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.PrzychRozch.PrzychRozchBL)
                    );
                Globalne.kodykreskoweBL = (WMSServerAccess.KodyKreskowe.KodyKreskoweBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.KodyKreskowe.KodyKreskoweBL)
                    );
                Globalne.partiaBL = (WMSServerAccess.Partia.PartiaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Partia.PartiaBL)
                    );
                Globalne.funklogBL = (WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.FunkcjaLogistyczna.FunkcjaLogistycznaBL)
                    );
                Globalne.paletaBL = (WMSServerAccess.Paleta.PaletaBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Paleta.PaletaBL)
                    );
                Globalne.menuBL = (WMSServerAccess.Menu.MenuBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.Menu.MenuBL)
                    );
                Globalne.numerSeryjnyBL = (WMSServerAccess.NumerSeryjny.NumerSeryjnyBL)
                    Globalne.client.Activate(
                        "WMSServerAccess",
                        typeof(WMSServerAccess.NumerSeryjny.NumerSeryjnyBL)
                    );

                ServerConnection.Connected = true;

                return 0;
            }
            catch (Exception)
            {
                ServerConnection.Connected = false;
                return -1;
            }
        }
    }
}
