using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ProTxt
{
    class Protocol
    {
        public string Operation = "";
        public string Message = "";
        public string Nr_s = "";
        public string ID = "";
        public string TS = "";

        public Protocol()
        {
            Operation = "";
            Message = "";
            Nr_s = "";
            ID = "";
            TS = "";
        }
        public Protocol(string o, string w, string t)
        {
            Nr_s = "";
            Operation = o;
            Message = w;
            TS = t;
        }

        public Protocol(string o, string w, string t, string nr_id)
        {
            Nr_s = "";
            Operation = o;
            Message = w;
            TS = t;
            ID = nr_id.ToString();
        }
        //funkcja zwracajaca numer sekwencyjny pakietu
        public string GetNrS(string s)
        {
            string[] m = s.Split(';');
            string[] a = m[1].Split('=');
            string temp = a[1];
            return temp;

        }
        //przydzieleniu numeru ID
        public void GetMessage1(string s)
        {
            string[] m = s.Split(';');
            string[] c = m[0].Split('=');
            string[] d = m[1].Split('=');
            string[] e = m[2].Split('=');
            string[] f = m[3].Split('=');
            ID = e[1];
        }
        //zwrocenie wiadomosci wyslanej z odpowiednim numerem sekwencyjnym 
        public string GetMessage2(string s)
        {
            string[] m = s.Split(';');
            string[] c = m[0].Split('=');
            string[] d = m[1].Split('=');
            string[] e = m[2].Split('=');
            string[] f = m[3].Split('=');
            Message = c[1];
            return Message;
        }

        //zwrocenie wiadomosci z odpowiednimi polami z nr sekwencyjnym 1
        public string Pakuj1()
        {
            Nr_s = "1";
            string msg = "Operacja=" + Operation + ";Nsekwencyjny=" + Nr_s + ";id=" + ID + ";Timestamp=" + TS + ";";
            return msg;
        }
        //zwrocenie wiadomosci z odpowiednimi polami z nr sekwencyjnym 2
        public string Pakuj2()
        {
            Nr_s = "2";
            string msg = "Odpowiedz=" + Message + ";Nsekwencyjny=" + Nr_s + ";id=" + ID + ";Timestamp=" + TS + ";";
            return msg;
        }
    }
}
