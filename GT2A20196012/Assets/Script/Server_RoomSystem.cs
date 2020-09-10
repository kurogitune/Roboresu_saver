using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net;
using System.Expansion;
using System.IO;

public class Server_RoomSystem : MonoBehaviour
{ 
    public GameObject[] Player_model = new GameObject[4];//プレイヤーのモデル
    public GameObject[] starg = new GameObject[4];//使用するステージ
    public GameObject[] syokiiti = new GameObject[8];
    public Text t;

    int[] kitai_No=new int[8];//プレイヤー達が使用する機体番号
     UgokiIN[] INsystem =new UgokiIN[8];

    bool erro = false,roomMax=false,taiki=true,GameStart=false,haiti=false,haitiOK=false;
    string sousindt;//送信するデータ用
    //エラー　部屋数が最大  スタート待機　ゲームスタートか  オブジェクトの配置 配置終了したか
    int StargNo,Ninzuu,zyoutai=0;//ステージ番号 プレイヤー人数  部屋の状態
    static public int portNo;//指定使用ポート番号
    // Start is called before the first frame update
    void Start()
    {
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

        try
        {
            var tcp = new TcpClient("127.0.0.1", 2003);//サーバー管理システムに接続用
            tuusin.nstIN(tcp.GetStream());
            string data=  tuusin.TCPzyusinTime_NO();
            string[] data2 = data.Split('_');
            switch (data2[0])
            {
                case "1111":
                    t.text = "接続完了";
                    tuusin.TCPsosin(strIPAddress);
                    portNo = int.Parse(data2[1]);
                    tuusin.UDPIN(ipAdd,portNo);//UDPデータ代入
                    Debug.Log("受信待機ip　" +ipAdd);
                    Debug.Log("受信待機ポート　" +portNo);
                    Task.Run(() => client_tuusin());//クライアントとの通信(TPC)開始
                    Task.Run(() => master_savertusin());//マスターサーバとの(TPC)通信開始
                    Invoke("tout",3);
                    break;

                case "3333":
                    t.text = "部屋数が最大";
                    tuusin.tuusinKill();
                    roomMax = true;
                    break;
            }
        }
        catch
        {
            t.text = "接続エラー";
            tuusin.tuusinKill();
            erro = true;
        }
    }
    void tout()
    {
        t.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (roomMax)//ルーム数が最大の場合終了処理
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
            return;
        }
        else if (erro)//接続エラーの場合　再接続
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (haiti&!haitiOK)//ステージステージ,機体作成
        {
            Debug.Log("model作成開始");
            haiti = false;
            haitiOK = true;
            for (int j = 0; j < kitai_No.Length; j++)
            {
                if (kitai_No[j] != 0)
                {
                    GameObject g = Instantiate(Player_model[kitai_No[j]-1]);
                    g.transform.position = syokiiti[j].transform.position;
                    INsystem[j] = g.GetComponent<UgokiIN>();
                }

            }
            Debug.Log("作成終了");

            Task.Run(() => UDPtuusin(0));//受信待機 No1
            Task.Run(() => UDPtuusin(1));//受信待機 No2
            Task.Run(() => UDPtuusin(2));//受信待機 No3
            Task.Run(() => UDPtuusin(3));//受信待機 No4
            Task.Run(() => UDPtuusin(4));//受信待機 No5
            Task.Run(() => UDPtuusin(5));//受信待機 No6
            Task.Run(() => UDPtuusin(6));//受信待機 No7
            Task.Run(() => UDPtuusin(7));//受信待機 No8

            Task.Run(() =>UDPsousoin());//各クライアントに送信開始
        }

        if (GameStart)
        {
            try//ここで送信データ作成
            {
                string data = "";//プレイヤーのデータを送信用に集約
                for (int i = 0; i < INsystem.Length; i++)
                {
                    if (INsystem[i] != null)
                    {
                        int No = i + 1;
                        data += string.Format("{0}_{1}", No, PlayerTr(INsystem[i].trOUT())) + "/";
                    }
                }
                sousindt = data;
            }
            catch
            {
                Debug.Log("変換エラー");
            }//ここまで
        }
    }

    void UDPtuusin(int i)//UDPでのクライアントデータ受信
    {
        int bangou = i;
        while (!erro)
        {
            string s= tuusin.UDPzyusin(bangou);

            string[] data = s.Split('_');
            try
            {
                switch (data[0])
                {
                    case "1":
                        INsystem[0].trIN(aiteTr(data[1]));
                        break;

                    case "2":
                        INsystem[1].trIN(aiteTr(data[1]));
                        break;

                    case "3":
                        INsystem[2].trIN(aiteTr(data[1]));
                        break;

                    case "4":
                        INsystem[3].trIN(aiteTr(data[1]));
                        break;

                    case "5":
                        INsystem[4].trIN(aiteTr(data[1]));
                        break;

                    case "6":
                        INsystem[5].trIN(aiteTr(data[1]));
                        break;

                    case "7":
                        INsystem[6].trIN(aiteTr(data[1]));
                        break;

                    case "8":
                        INsystem[7].trIN(aiteTr(data[1]));
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                Debug.Log("変換エラー");
            }

        }
    }
    void UDPsousoin()//UDPでのクライアントにサーバールームの状態を送信
    {
        while (!erro)
        {
            if (!GameStart) continue;
            try
            {
                string data =sousindt;
                tuusin.UDPsousin(data);
            }
            catch
            {
                Debug.Log("送信エラー");
            }
        }
    }

    void master_savertusin()//マスターサーバーに部屋の状態を送信
    {
        while (!erro)
        {
            tuusin.TCPsosin(string.Format("{0}", zyoutai));//部屋の状態送信
                                                           //0:通常待機　1：ゲーム開始 2:ゲーム中　3:ゲーム終了
        }
    }


    //void dt()//クライアントとのTPC通信
    //{
    //    var l = new List<NetworkStream>(8);//部屋　1　　システム通信
    //    string sousindata = "";//送信するデータ
    //    bool stageSelect = false, Start_zyunbOK = false;//ステージ選択中か　全員ゲーム準備完了か
    //    List<int> Starg_No = new List<int>(8);//受信ステージ番号用
    //    List<int> zyunbiOK = new List<int>(8);//ゲームの準備完了したか用
    //    int selct_stage_No = 0;//一番多かったステージ番号
    //    int room_zyoutai = 1;//部屋の状態
    //    //0:サーバー切断 1:接続待機 ２：メンバー決定  3:ステージが決定した  4:全員がゲームを開始する準備完了した 5:ゲーム中
    //    while (!erro)
    //    {
    //        sousindata = string.Format("{0}_{1}_{2}_{3}/", 0, room_zyoutai, l.Count, selct_stage_No);//部屋の状態　部屋にいる人数　  選択されたステージ番号

    //        for (int i = 0; i < l.Count; i++)//一人ひとりクライアントのデータを受信
    //        {
    //            string data = "";
    //            try
    //            {
    //                data = tuusin.TCPzyusi(l[i]);
    //                if (data == "") { Debug.Log("データなし"); continue; }
    //            }
    //            catch
    //            {
    //               Debug.Log("受信エラー");
    //                continue;
    //            }

    //            string[] data2 = data.Split('_');

    //            if (!room_Gametyuu[No] & data2[0] == "1" & data2[1] == "2" || !room_Gametyuu[No] & l.Count == l.Capacity)//メンバーが決定したことをマスターのデータで取得  又は最大人数がそろったら
    //            {
    //                room_zyoutai = 2;
    //            }

    //            switch (data2[1])
    //            {
    //                case "0"://クライアントが切断
    //                    l.Remove(l[i]);
    //                    break;

    //                case "4"://ステージセレクト用
    //                    Starg_No.Add(int.Parse(data2[4]));//受信ステージ番号取得
    //                    break;

    //                case "6"://ゲーム開始準備完了
    //                    zyunbiOK.Add(0);//受信ステージ番号取得
    //                    break;

    //                default:
    //                    break;
    //            }
    //            sousindata += data + "/";
    //        }

    //        sosin(room, sousindata);//ゲームサーバーに送信
    //        string roomdata = zyusin(room);//ゲームサーバーからのデータ受信
    //        switch (roomdata)
    //        {
    //            case "1":
    //                room_zyoutai = 5;
    //                break;
    //        }

    //        if (!stageSelect)//ステージを決定する
    //        {
    //            if (Starg_No.Count == l.Count & l.Count != 0)//データがそろったら
    //            {
    //                stageSelect = true;
    //                selct_stage_No = Starg_No[Random.Range(0,l.Count)];//ランダムでステージ番号を選択
    //                Debug.Log("ステージデータ揃った :" + selct_stage_No);
    //                room_zyoutai = 3;
    //            }
    //            else
    //            {
    //                Starg_No.Clear();
    //            }
    //        }

    //        if (!Start_zyunbOK)//クライアント全員がステージ準備完了か
    //        {
    //            if (zyunbiOK.Count == l.Count & l.Count != 0)//データがそろったら
    //            {
    //                Start_zyunbOK = true;
    //               Debug.Log("全員がゲーム開始準備完了");
    //                room_zyoutai = 4;
    //            }
    //            else
    //            {
    //                zyunbiOK.Clear();
    //            }
    //        }


    //        //192.168.10.3
    //        for (int i = 0; i < l.Count; i++)//部屋にいるクライアント全員にデータを送信
    //        {
    //            Console.WriteLine(sousindata);
    //            UDPsousoin(l[i], sousindata);
    //        }
    //    }
    //}

    void client_tuusin()//通信システム
    {


        while (!erro)
        {

            string sousindata = "";//送信するデータ
            string data = tuusin.TCPzyusi();
            //受信したデータ　番号　システム状態　プレイヤーの名前　機体番号　ステージ選択番号　プレイヤーのIPアドレス
            if (data == "11") continue;
            string[] data2 = data.Split('/');
            sousindata += data2[0] + "/";
            for (int i = 0; i < data2.Length; i++)
            {
                string[] data3 = data2[i].Split('_');
                switch (data3[0])
                {
                    case "0":
                        switch (data3[1])
                        {
                            case "2"://メンバー決定
                                Ninzuu = int.Parse(data3[2]);
                                taiki = false;
                                break;

                            case "3"://ステージ決定
                                StargNo = int.Parse(data3[3]);
                                haiti = true;
                                break;

                            case "4"://全員準備完了
                                if (!GameStart)
                                {
                                    zyoutai = 1;
                                    GameStart = true;
                                    Debug.Log("準備OK");
                                }
                                break;
                        }
                        break;

                    case "1":
                        if (taiki)
                        {
                            kitai_No[0] = int.Parse(data3[3]);//使用機体受信 
                            tuusin.IPdata[0]= data3[5];//クライアントのIPアドレス取得
                        }   
                        break;

                    case "2":
                        if (taiki)
                        {
                            kitai_No[1] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[1] = data3[5];
                        }
                        break;

                    case "3":
                        if (taiki)
                        {
                            kitai_No[2] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[2] = data3[5];
                        }
                        break;

                    case "4":
                        if (taiki)
                        {
                            kitai_No[3] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[3] = data3[5];
                        }
                        break;

                    case "5":
                        if (taiki)
                        {
                            kitai_No[4] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[4] = data3[5];
                        }
                        break;

                    case "6":
                        if (taiki)
                        {
                            kitai_No[5] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[5] = data3[5];
                        }
                        break;

                    case "7":
                        if (taiki)
                        {
                            kitai_No[6] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[6] = data3[5];
                        }
                        break;

                    case "8":
                        if (taiki)
                        {
                            kitai_No[7] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[7] = data3[5];
                        }
                        break;
                }
            }
        }
    }

    string PlayerTr(Tuple<Vector3,Quaternion,int,int,int,int>tp)//モデルの情報を文字列に変換
    {
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    tp.Item1.x, tp.Item1.y, tp.Item1.z,
                    tp.Item2.x, tp.Item2.y,tp.Item2.z,tp.Item2.w, 
                    tp.Item3,
                    tp.Item4,
                    tp.Item5,
                    tp.Item6);
                  //座標　x,y,z    回転x,y,z,w　 攻撃したか　  死んだか  当たった　ゴールした   
    }

    Tuple<Vector3,Quaternion,int,int> aiteTr(string s)//受信したプレイヤーのデータを変換 Tupleは複数のデータを返すことができる   複数のアイテムは　Item.1 Item.2となる
    {
        string[] data = s.Split(',');
        return Tuple.Create(new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])),
                            new Quaternion(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6])),int.Parse(data[7]),int.Parse(data[8]));
        //座標　x,y,z    回転x,y,z,w　攻撃 死んだか  
    }
}
