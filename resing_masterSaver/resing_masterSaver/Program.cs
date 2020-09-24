using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.IO;


namespace resing_masterSaver
{
    class Program
    {
        static bool End = false;//サーバー管理用フラグ
        static Encoding ecn = Encoding.UTF8;//文字コード指定 
        static bool[] room_Nyusitu = new bool[4];//レース中か　true　プレイ中　false　待機中
        static int[] Room_Ninzuu = new int[4];//各部屋の人数
        static int[] roompots = { 2004, 2013, 2021, 2029 };//サーバールーム通信用 port番号
        static int Maxninzuu = 8;//各部屋の最大人数
        static void Main(string[] args)
        {
           room_Nyusitu[1] = room_Nyusitu[2] = room_Nyusitu[3] = true;
            string[] RoomIP = new string[4];//各部屋のIPアドレス
            var roomedata = new List<NetworkStream>(1);//サーバールーム用の箱
                                                       //  NetworkStream mas = null;//管理者保存

            int port = 2002;//初期接続ポート番号TCP用
            int roomport = 2003;//サーバールーム接続用

            string strHostName = Dns.GetHostName();//PCのホストネームを取得
                                                   // マシンのIPアドレスを取得する 
            string strIPAddress = string.Empty;//使用するIPアドレス
            IPHostEntry ipentry = Dns.GetHostEntry(strHostName);
            foreach (IPAddress ip in ipentry.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    strIPAddress = ip.ToString();
                    break;
                }
            }

            IPAddress ipAdd = IPAddress.Parse(strIPAddress);//IPアドレス代入
            IPAddress ipAddmas = IPAddress.Parse("127.0.0.1");//ローカルIP

            TcpListener lisetensr = new TcpListener(ipAdd, port);// 起動に必要なオブジェクトを作成（クライアント用）
            TcpListener roomsetensr = new TcpListener(ipAddmas, roomport);// 起動に必要なオブジェクトを作成（サーバールーム用）
            lisetensr.Start();//システム開始
            roomsetensr.Start();

            Task t = Task.Run(() => heyaIN(roomsetensr, roomedata, roompots, RoomIP));
            t.Wait();//タスク終了まで待機
            Task.Run(() => run(lisetensr, RoomIP));//クライアント受け入れ非同期

            Console.WriteLine("TCPサーバー起動 IP {0}  port {1}", ((IPEndPoint)lisetensr.LocalEndpoint).Address,
                                                      ((IPEndPoint)lisetensr.LocalEndpoint).Port);//IPアドレス　ポート番号

            Console.WriteLine("TCPサーバールーム用起動 IP {0}  port {1}", ((IPEndPoint)roomsetensr.LocalEndpoint).Address,
                                                      ((IPEndPoint)roomsetensr.LocalEndpoint).Port);//IPアドレス　ポート番号


            Task.Run(() => heya(0, roomedata[0]));//クライアント受け入れ非同期
            Console.WriteLine("Room1　起動");

            //Task.Run(() => heya(1, roomedata[1]));//クライアント受け入れ非同期
            //Console.WriteLine("Room2　起動");

            //Task.Run(() => heya(2, roomedata[2]));//クライアント受け入れ非同期
            //Console.WriteLine("Room3　起動");

            //Task.Run(() => heya(3, roomedata[3]));//クライアント受け入れ非同期
            //Console.WriteLine("Room4　起動");
            //Task.Run(() => mas_run(mastar, mas, client));//管理者受け入れ非同期

            while (!End)
            {
                Console.ReadLine();
                Console.Clear();
            }

            lisetensr.Stop();
            //room.Stop();
            Console.WriteLine("サーバーを終了します");
            Console.ReadLine();
        }

        static void run(TcpListener tcl, string[] IP)//非同期処理クライアント接続待機
        {
            while (!End)
            {
                try
                {
                    var tcp = tcl.AcceptTcpClient();//クライアントが接続しようとしたら 
                    NetworkStream ns = tcp.GetStream();
                    Console.WriteLine("クライアントが接続");
                    string data = string.Format("{0}_{1}/{2}_{3}/{4}_{5}/{6}_{7}",
                                               Room_Ninzuu[0], room_Nyusitu[0], Room_Ninzuu[1], room_Nyusitu[1], Room_Ninzuu[2], room_Nyusitu[2], Room_Ninzuu[3], room_Nyusitu[3]);//送信データ作成

                    sosin(ns, data);//接続してきたクライアントに部屋の人数を送信
                    string zyusindata = zyusin(ns);//選んだ部屋のデータ取得
                    switch (zyusindata)
                    {
                        case "0":
                            Ninzuhantei(Room_Ninzuu[0], ns, zyusindata, IP[0]);
                            break;

                        case "1":
                            Ninzuhantei(Room_Ninzuu[1], ns, zyusindata, IP[1]);
                            break;

                        case "2":
                            Ninzuhantei(Room_Ninzuu[2], ns, zyusindata, IP[2]);
                            break;

                        case "3":
                            Ninzuhantei(Room_Ninzuu[3], ns, zyusindata, IP[3]);
                            break;

                        default:
                            sosin(ns, "3333");
                            break;
                    }
                }
                catch (FormatException e)//エラーの場合
                {
                    Console.WriteLine(e);
                    continue;
                }
            }
        }

        static void Ninzuhantei(int count, NetworkStream ns, string s, string IP)//部屋が最大人数か判定 　接続する部屋　接続しようとしているクライアント　部屋番号
        {
            if (room_Nyusitu[int.Parse(s)])//部屋がゲーム中だったら強制的に接続不可
            {
                sosin(ns, "2222");//ゲームをしているため部屋に入れないとを送信
                return;
            }

            if (count < Maxninzuu)
            {
                Console.WriteLine(s + "_" + count);
                sosin(ns, string.Format("{0}_{1}_{2}_{3}", s, count + 1, roompots[int.Parse(s)], IP));
                //接続した部屋の名前　番号 部屋のIPアドレス
                ns.WriteTimeout = 1000;
                System.Threading.Thread.Sleep(1000);//指定時間処理を停止させる            
            }
            else
            {
                sosin(ns, "1111");//人数最大とを送信
            }
        }

        static void heya(int No, NetworkStream room)//部屋の処理  リスト 部屋番号  サーバルーム
        {
            while (!End)
            {

                string roomdata = zyusin(room);//ゲームサーバーからのデータ受信
                                               //受信  //部屋の状態_部屋にいる人数

                string[] data = roomdata.Split('_');
                switch (data[0])//0:待機 　1:ゲーム開始　２：ゲーム終了
                {
                    case "0"://待機
                        room_Nyusitu[No] = false;
                        break;
                    case "1"://ゲーム開始
                        room_Nyusitu[No] = true;
                        break;
                }

                Room_Ninzuu[No] = int.Parse(data[1]);//部屋人数
            }
        }

        static void heyaIN(TcpListener tcl, List<NetworkStream> l, int[] potr, string[] IP)//サーバールーム接続
                                                                                           //接続入口　リスト　　サーバールームに使うポート番号指定　　
        {
            for (int i = 1; i < l.Capacity + 1; i++)
            {
                Console.WriteLine("サーバールーム " + i + "を接続してください");
                var tcp = tcl.AcceptTcpClient();//クライアントが接続しようとしたら 
                NetworkStream ns = null;
                if (l.Count < l.Capacity)
                {
                    ns = tcp.GetStream();
                    l.Add(ns);
                    Console.WriteLine("サーバールーム " + l.Count + "接続");
                    sosin(ns, "1111_" + potr[l.Count - 1]);//接続完了　ポート番号送信
                    IP[i - 1] = zyusin(l[i - 1]);//部屋IPアドレスを受信

                    Console.WriteLine("サーバールーム1 IP  " + IP[i - 1]);
                }
                else
                {
                    sosin(ns, "3333_0");
                    tcp.Close();
                }
            }
            //Console.Clear();
        }


        static string zyusin(NetworkStream nst)//受信したbyteデータを文字列に変換
        {
            string zyusindata = "";
            using (var ms = new MemoryStream())//メモリ自動開放してくれるやつ（べんりやね～）//サーバーからの受信
            {
                try
                {
                    byte[] resByte = new byte[256];
                    int resSize = 0;//受信した文字数

                    do
                    {
                        resSize = nst.Read(resByte, 0, resByte.Length);//文字数記録
                        if (resSize == 0) break;

                        ms.Write(resByte, 0, resSize);//受信データ蓄積　文字　？　文字数
                    }
                    while (resByte[resSize - 1] != '\n');//データの最後が\nではない場合は受信を継続

                    string resMsg = ecn.GetString(ms.GetBuffer(), 0, (int)ms.Length);//受信データを文字列に変換
                    resMsg = resMsg.TrimEnd('\n');//文字最後の\nを消す
                    zyusindata = resMsg;
                }
                catch
                {

                }

                return zyusindata;
            }
        }


        static void sosin(NetworkStream ns, string s)//データ送信
        {
            byte[] bun = ecn.GetBytes(s + '\n');//byteデータ作
            ns.Write(bun, 0, bun.Length);//送信
        }
    }
}
