using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;

namespace System.Expansion//自作拡張
{
    public static class tuusin//通信システム
    {
        static Encoding ecn = Encoding.UTF8;//文字コード指定 
        static NetworkStream nst;//マスターサーバーデータ
        static UdpClient[] zyusinudp = new UdpClient[8];//ゲームサーバールームデータ
        public static string[] IPdata = new string[8];//送信する相手のIPアドレスデータ保存用

        public static void nstIN(NetworkStream ns)//サーバーマスターを取得
        {
            nst = ns;

        }

        public static void tuusinKill()//通信エラーの場合サーバーデータを削除
        {
            nst = null;
            zyusinudp = null;
        }

        public static void UDPIN(IPAddress ip, int portNo)//UDPデータを代入 ipアドレス　使用ポート最初の番号
        {
            for (int i = 0; i < zyusinudp.Length; i++)
            {
                int port = portNo + i;
                zyusinudp[i] = new UdpClient(new IPEndPoint(ip, port));
            }
        }

        public static void TCPsosin(string s)//データ送信処理
        {
            byte[] bun = ecn.GetBytes(s + '\n');//byteデータ作
            nst.Write(bun, 0, bun.Length);//送信
        }

        public static string TCPzyusi()//受信処理　タイムアウトあり
        {
            string s = "";
            nst.WriteTimeout = 1000;//受信タイムアウト設定　１秒
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    byte[] resByte = new byte[256];
                    int resSize = 0;//受信した文字数

                    do
                    {
                        resSize = nst.Read(resByte, 0, resByte.Length);//文字数記録
                        if (resSize == 0) break;

                        ms.Write(resByte, 0, resSize);//受信データ蓄積　文字　？　文字数
                    }
                    while (nst.DataAvailable || resByte[resSize - 1] != '\n');//読み取り可能データがあるか、データの最後が\nではない場合は受信を継続

                    string resMsg = ecn.GetString(ms.GetBuffer(), 0, (int)ms.Length);//受信データを文字列に変換
                    resMsg = resMsg.TrimEnd('\n');//文字最後の\nを消す
                    s = resMsg;
                }
            }
            catch
            {
                s = "11";
                Debug.Log("データなし");
            }
            return s;
        }

        public static string TCPzyusinTime_NO()//データ受信でタイムアウトなし版
        {
            using (MemoryStream ms = new MemoryStream())
            {
                byte[] resByte = new byte[256];
                int resSize = 0;//受信した文字数

                do
                {
                    resSize = nst.Read(resByte, 0, resByte.Length);//文字数記録
                    if (resSize == 0) break;

                    ms.Write(resByte, 0, resSize);//受信データ蓄積　文字　？　文字数
                }
                while (nst.DataAvailable || resByte[resSize - 1] != '\n');//読み取り可能データがあるか、データの最後が\nではない場合は受信を継続

                string resMsg = ecn.GetString(ms.GetBuffer(), 0, (int)ms.Length);//受信データを文字列に変換
                resMsg = resMsg.TrimEnd('\n');//文字最後の\nを消す
                return resMsg;
            }
        }

        public static void UDPsousin(string s)//UDPでのデータ送信
        {
            byte[] bun = ecn.GetBytes(s);//送信データ作成
            for (int i = 0; i < IPdata.Length; i++)
            {
                if (IPdata[i] != null)
                {
                    UdpClient sousinudp = new UdpClient();
                    sousinudp.Send(bun, bun.Length, IPdata[i], Server_RoomSystem.portNo);//送信   
                }
            }
        }

        public static string UDPzyusin(int i)//UDPでの受信
        {
            IPEndPoint ep = null;
            byte[] zusin = zyusinudp[i].Receive(ref ep);
            return ecn.GetString(zusin);
        }
    }
}

