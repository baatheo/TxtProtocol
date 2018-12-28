using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ProTxt
{
    class Server
    {
        private static readonly Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new List<Socket>();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 9876;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private static Random random = new Random();
        private static int num = random.Next(1, 101);
        private static string guess_num = num.ToString();
        private static bool wynik = false;
        private static List<int> ids = new List<int>();
        private static DateTime startTime;
        private static int t;
        private static int index = 0;
        private static bool koniec = false;

        static void Main()
        {
            Console.Title = "Serwer";
            SetupServer();
            Console.ReadLine();
            CloseAllSockets();
        }
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
            Environment.Exit(0);

        }
        private static void SetupServer()
        {
            Console.WriteLine("Uruchamianie serwera...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(0);
            serverSocket.BeginAccept(Connect, null);
            Console.WriteLine("Serwer uruchomiony");
            Console.WriteLine("Liczba do odgadniecia: " + guess_num);
        }
        private static void PrzydzielID(Socket socket)
        {
            string n = ids[index].ToString();
            Protocol aaa = new Protocol("ID", "Przydzielilem_ID", DateTime.Now.ToLongTimeString(), n);
            index++;
            byte[] data = Encoding.ASCII.GetBytes(aaa.Pakuj1());
            socket.Send(data);
            Thread.Sleep(100);
            byte[] data2 = Encoding.ASCII.GetBytes(aaa.Pakuj2());
            socket.Send(data2);                   
        }

        private static void Connect(IAsyncResult AR)
        {
            Socket socket;
            int id = random.Next(7, 65);
            ids.Add(id);
            string n = id.ToString();
            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            clientSockets.Add(socket);
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine("Polaczono z klientem. ID klienta: " + id + ", czekanie...");
            serverSocket.BeginAccept(Connect, null);
            odliczanko();
        }
        public static void odliczanko()
        {
            t = ids[0];
            if (ids.Count > 1)
            {
                for (int i = 1; i < ids.Count; i++)
                {
                    t = t - ids[i];
                }
                if (t < 0) t = t * (-1);
                t = ((t * 74) % 90) + 25;
                Console.WriteLine("Czas na zgadniecie: " + t + " sekund");
                startTime = DateTime.Now;
            }
        }
        public static void wygrana(Socket socket)
        {
            //foreach (Socket socket in clientSockets)
            //{
                Protocol aaa = new Protocol("odpowiedz", "Liczba zostala odgadnieta! Koniec gry ", DateTime.Now.ToLongTimeString());
                byte[] data = Encoding.ASCII.GetBytes(aaa.Pakuj1());
                socket.Send(data);
                byte[] data2 = Encoding.ASCII.GetBytes(aaa.Pakuj2());
                socket.Send(data2);
                //socket.Shutdown(SocketShutdown.Both);
                //socket.Close();

                //serverSocket.Close();
            //}
        }
        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received=0;
            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine("Klient zerwal polaczenie");
                current.Close();
                clientSockets.Remove(current);
                return;
            }
            byte[] recBuf1 = new byte[received];
            Array.Copy(buffer, recBuf1, received);
            string text1 = Encoding.ASCII.GetString(recBuf1);
            Protocol aa = new Protocol();
            string msg = "";
            if (aa.GetNrS(text1) == "1")
            {
                aa.GetMessage1(text1);
            }
            else if (aa.GetNrS(text1) == "2")
            {
                msg = aa.GetMessage2(text1);
            }

            int otrzymane = 0;
            var buffer2 = new byte[2048];
            Boolean y = false;
            do
            {
                int received2 = current.Receive(buffer2, SocketFlags.None);
                if (received2 != 0)
                {
                    otrzymane = received2;
                    y = true;
                }
            } while (!y);

            byte[] recBuf2 = new byte[otrzymane];
            Array.Copy(buffer2, recBuf2, otrzymane);
            string text2 = Encoding.ASCII.GetString(recBuf2);
            if (aa.GetNrS(text2) == "1")
            {
                aa.GetMessage1(text2);
            }
            else if (aa.GetNrS(text2) == "2")
            {
                msg = aa.GetMessage2(text2);
            }
            if (koniec)
            {
                Protocol a = new Protocol("odpowiedz", "Ktos inny zgadl liczby, przegrales", DateTime.Now.ToLongTimeString(), aa.ID);
                byte[] data1 = Encoding.ASCII.GetBytes(a.Pakuj1());
                current.Send(data1);
                Thread.Sleep(100);
                byte[] data2 = Encoding.ASCII.GetBytes(a.Pakuj2());
                current.Send(data2);
                return;
            }
            Console.WriteLine("Klient: " + msg);


            if(msg == "przydzielID")
            {
                PrzydzielID(current);             
            }
            else if (msg.ToLower() == guess_num)
            {
                Console.WriteLine("Gracz trafil liczbe");
                koniec = true;
                wygrana(current);
            }
            else if (msg.ToLower() == "wyjscie")
            {
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientSockets.Remove(current);
                Console.WriteLine("Klient sie rozlaczyl");
                return;
            }
            else
            {
                Console.WriteLine("Gracz nie trafil liczby");
                DateTime tempTime = DateTime.Now;
                Protocol a = new Protocol("odpowiedz", "Bledna liczba", DateTime.Now.ToLongTimeString(), aa.ID);
                byte[] data1 = Encoding.ASCII.GetBytes(a.Pakuj1());
                current.Send(data1);
                Thread.Sleep(100);
                byte[] data2 = Encoding.ASCII.GetBytes(a.Pakuj2());
                current.Send(data2);
                Console.WriteLine("Wyslano powiadomienie, " + DateTime.Now.ToLongTimeString());
            }
            if (!koniec)
            {
                current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }

        }

    }
}
