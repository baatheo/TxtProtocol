using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace ProTxt
{
    class Client
    {
        private static Protocol paczka = new Protocol();
        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 9876;
        private static Random random = new Random();

        static void Main()
        {
            Console.Title = "Klient";
            ConnectToServer();
            paczka = GetID();
            Console.WriteLine("Gramy!");
            Console.WriteLine(@"Napisz ""wyjscie"" zeby sie rozlaczyc");
            while (true)
            {
                WysylanieLiczby();
                OdebranieOdp();
            }
        }
        //polaczenie z serverem
        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Proba polaczenia " + attempts);
                    ClientSocket.Connect("192.168.43.216", PORT);

                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Polaczono");
        }

        //uzyskanie ID od servera
        private static Protocol GetID()
        {
            int otrzymane = 0;
            Console.WriteLine("Wysyłam prośbę o przydzielenie ID");
            Protocol paczkaID = new Protocol("ID", "przydzielID", DateTime.Now.ToLongTimeString(), "0");
            SendString(paczkaID.Pakuj1());
            Thread.Sleep(100);
            SendString(paczkaID.Pakuj2());
            Boolean x = false;
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received != 0)
            {
                otrzymane = received;
                x = true;
            }

            var data = new byte[otrzymane];
            Array.Copy(buffer, data, otrzymane);
            string text = Encoding.ASCII.GetString(data);
            if (paczka.GetNrS(text) == "1")
            {
                paczka.GetMessage1(text);
            }
            else if (paczka.GetNrS(text) == "2")
            {
                Console.WriteLine(paczka.GetMessage2(text));
            }
            Boolean y = false;

            int received2 = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received2 != 0)
            {
                otrzymane = received2;
                y = true;
            }

            var data2 = new byte[otrzymane];
            Array.Copy(buffer, data2, otrzymane);
            string text2 = Encoding.ASCII.GetString(data2);
            if (paczka.GetNrS(text2) == "1")
            {
                paczka.GetMessage1(text2);
            }
            else if (paczka.GetNrS(text2) == "2")
            {
                string xyz = paczka.GetMessage2(text2);
            }
            Console.WriteLine("moje ID: " + paczka.ID);
            return paczka;
        }
        //wyslanie liczby podanej przez uzytownika do servera
        private static void WysylanieLiczby()
        {
            Console.Write("> ");
            string n = Console.ReadLine();
            paczka.Operation = "Gra";
            paczka.Message = n;
            paczka.TS = DateTime.Now.ToLongTimeString();
            SendString(paczka.Pakuj1());
            Thread.Sleep(100);
            SendString(paczka.Pakuj2());
            if (n == "wyjscie")
            {
                Exit();
            }
        }
        //odebranie odpowiedzi zwrotnej od servera
        private static void OdebranieOdp()
        {
            string p = "";
            int otrzymane = 0;
            Boolean x = false;
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received != 0)
            {
                otrzymane = received;
                x = true;
            }

            var data = new byte[otrzymane];
            Array.Copy(buffer, data, otrzymane);
            string text = Encoding.ASCII.GetString(data);
            if (paczka.GetNrS(text) == "1")
            {
                paczka.GetMessage1(text);
            }
            else if (paczka.GetNrS(text) == "2")
            {
                p = paczka.GetMessage2(text);
            }

            var buffer2 = new byte[2048];
            Boolean y = false;
            try {
                int received2 = ClientSocket.Receive(buffer2, SocketFlags.None);
                if (received2 != 0)
                {
                    otrzymane = received2;
                    y = true;
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex);
            }

            var data2 = new byte[otrzymane];
            Array.Copy(buffer2, data2, otrzymane);
            string text2 = Encoding.ASCII.GetString(data2);
            if (paczka.GetNrS(text2) == "1")
            {
                paczka.GetMessage1(text2);
            }
            else if (paczka.GetNrS(text2) == "2")
            {
                p = paczka.GetMessage2(text2);
                Console.WriteLine(p);
            }

            if (p == "Liczba zostala odgadnieta! Koniec gry ")
            {
                Exit();
            }
            if (p == "Ktos inny zgadl liczby, przegrales")
            {
                Exit();
            }
        }

        //wylaczenie klienta
        private static void Exit()
        {
            Console.ReadLine();
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }
        //wyslanie do servera podanej wiadomosci
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

    }
}
