using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Expansion;
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
    [Header("プレイヤーモデル")]
    public GameObject[] Player_model = new GameObject[4];//プレイヤーのモデル
    [Header("ステージデータ")]
    public GameObject[] starg = new GameObject[4];//使用するステージ
    [Header("アイテムモデル")]
    public GameObject Itemobj;//アイテムのオブジェクト
    public Text t;
    [Header("各プレイヤーで使用するLayerMask初期番号")]
    public int MaskNo;

    public static  int Lap;//ステージを回る数
    public static int Maxwall;//判定用の最大数

    GameObject Starg;
    GameObject[] PlayerModelData = new GameObject[8];
    int[] kitai_No=new int[8];//プレイヤー達が使用する機体番号
    UgokiIN[] INsystem =new UgokiIN[8];//各プレイヤーの動きのスクリプト
    List<NetworkStream> client = new List<NetworkStream>(8);//クライアントのデータTCP用
    TcpListener lisetensr;//クライアントTPC受け入れ用
    List<itemSy> ItemList=new List<itemSy>();//アイテムのスクリプト用
    bool Tuusin = true, roomMax = false, taiki = true, GameStart = false, haiti = false, haitiOK = false, UDP_Move = false,Roomreset=false;

    //エラー　部屋数が最大  スタート待機　ゲームスタートか  オブジェクトの配置 配置終了したか UDP通信システム用
    int StargNo,Room_ninzuu, zyoutai=0;//ステージ番号 プレイヤー人数  部屋の状態
    static public int portNo;//指定使用ポート番号

    public static List<bool> Gool = new List<bool>();//クライアントがゴールしたのか
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
            var tcp = new TcpClient("127.0.0.1", 2003);//クライアント受け入れサーバーに接続用
            tuusin.nstIN(tcp.GetStream());
            string data=  tuusin.TCPzyusinTime_NO();
            string[] data2 = data.Split('_');
            switch (data2[0])
            {
                case "1111":
                    t.text = "接続完了";
                    tuusin.TCPsosin(strIPAddress);//クライアント受け入れサーバーに部屋のIPを送信
                    portNo = int.Parse(data2[1]);
                    tuusin.UDPIN(ipAdd,portNo+1);//UDPデータ代入
                    Debug.Log("UDP受信待機　ip　" +ipAdd);
                    Debug.Log("UDP受信待機port　" +portNo+1);
                    Task.Run(() => client_tuusin());//クライアントとの通信(TPC)開始
                    Task.Run(() => master_savertusin());//クライアント受け入れサーバーとの(TPC)通信開始
                    lisetensr = new TcpListener(ipAdd, portNo);// クライアント受け入れ用
                    lisetensr.Start();
                    Debug.Log("TCP受信待機　ip　" + ipAdd);
                    Debug.Log("TCP受信待機port　" + portNo);
                    Task.Run(() => TCPclientIN());//クライアント受入待機開始
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
            Tuusin = false;
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
        else if (!Tuusin)//接続エラーの場合　再接続
        {
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return;
        }

        if (Roomreset)
        {
            Debug.Log("部屋をリセットします");
            Lap = Maxwall = 0;
            Destroy(Starg);
            tuusin.IPdataKill();
            for(int i=0;i<PlayerModelData.Length ; i++)//モデル消去
            {
                if (PlayerModelData[i] != null)
                {
                    Destroy(PlayerModelData[i]);
                    PlayerModelData[i] = null;
                }
            }
            for(int i = 0; i < INsystem.Length; i++)//通信用スクリプト削除
            {
                INsystem[i] = null;
            }
            ItemList.Clear();//アイテムスクリプト削除
            for (int i = 0; i < kitai_No.Length; i++)//機体番号削除
            {
                kitai_No[i] = 0;
            }

            t.text = "部屋リセット終了";
            t.gameObject.SetActive(true);
            Invoke("tout", 3);
            Roomreset = false;
            Debug.Log("部屋リセット終了");
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
                    g.name = j.ToString();
                    g.layer = MaskNo+j;
                    PlayerModelData[j] = g;
                    INsystem[j] = g.GetComponent<UgokiIN>();
                }

            }
            Debug.Log("作成終了");
            Debug.Log("ステージ作成開始");
            int[] Max_Lap = { 5,2,2,1 };//マップの周回数
            Starg= Instantiate(starg[StargNo-1]);
            Lap = Max_Lap[StargNo - 1];
            List<GameObject> rodobj = objList.tag_All_obj("item");//アイテムオブジェクトを全て取得
                for (int i = 0; i < rodobj.Count; i++)
                {
                    GameObject setobj = Instantiate(Itemobj);
                    setobj.transform.position = rodobj[i].transform.position;
                    setobj.transform.parent = rodobj[i].transform.parent;
                    ItemList.Add(setobj.GetComponent<itemSy>());
                    rodobj[i].SetActive(false);
                }

            List<GameObject> lacewalldata = objList.tag_All_obj("lacewall");//順番判定用の壁全て取得
            Maxwall = lacewalldata.Count;
                for (int i = 0; i < lacewalldata.Count; i++)
                {
                    lacewalldata[i].AddComponent<lacewall>();
                    lacewalldata[i].GetComponent<lacewall>().NoIN(i+1);
                }  
   
            Debug.Log("作成終了");
            UDP_Move = true;
            Task.Run(() => UDPtuusin(0));//受信待機 No1
            Task.Run(() => UDPtuusin(1));//受信待機 No2
            Task.Run(() => UDPtuusin(2));//受信待機 No3
            Task.Run(() => UDPtuusin(3));//受信待機 No4
            Task.Run(() => UDPtuusin(4));//受信待機 No5
            Task.Run(() => UDPtuusin(5));//受信待機 No6
            Task.Run(() => UDPtuusin(6));//受信待機 No7
            Task.Run(() => UDPtuusin(7));//受信待機 No8

          //  Task.Run(() =>UDPsousoin());//各クライアントに送信開始
        }

        if (GameStart)
        {
            UDPsousoin();
            try//ランキング処理
            {
                int[] Master_RankData = new int[Room_ninzuu];
                for (int i = 0; i < Room_ninzuu; i++)//順位判定用
                {
                    Master_RankData[i] = INsystem[i].hanteiCountOUT();
                }
                Array.Sort(Master_RankData);//昇順に並べ替え
                Array.Reverse(Master_RankData);//場所を反転
                for (int i = 0; i < Room_ninzuu; i++)
                {
                    for (int j = 0; j < Master_RankData.Length; j++)
                    {
                        if (INsystem[i].hanteiCountOUT() == Master_RankData[j])
                        {
                            INsystem[i].RankIN(j + 1);
                        }
                    }
                }
            }
            catch { Debug.Log("順位並び替えエラー"); }
        }
    }

    void UDPtuusin(int i)//UDPでのクライアントデータ受信
    {
        int bangou = i;
        while (UDP_Move)
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

    void UDPsousoin()//クライアントにサーバールームの状態を送信
    {
        string sousindt = "";//プレイヤーのデータを送信用に集約

        try//ここで送信データ作成し送信を行う処理
        {
            sousindt += String.Format("{0}_{1}", 0, ItemList.Count) + "/";//サーバールームのデータ作成
                         //送信データ//アイテム個数　各アイテムの状態
            for (int i = 0; i < INsystem.Length; i++)
            {
                if (INsystem[i] != null)
                {
                    int No = i + 1;
                    sousindt += string.Format("{0}_{1}", No,INsystem[i].trOUT()) + "/";
                    INsystem[i].Datareset();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.Log("変換エラー");
        }

        try//各クライアントにUDPで送信
        {
            tuusin.UDPsousin(sousindt);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.Log("送信エラー");
        }
        //ここまで
    }

    void master_savertusin()//クライアント受け入れサーバーに部屋の状態を送信
    {
        while (Tuusin)
        {

            tuusin.TCPsosin(string.Format("{0}_{1}_", zyoutai,client.Count));//部屋の状態送信
           //送信 //部屋状態,部屋にいる人数
            //0:待機　1：ゲーム開始 2:ゲーム終了
        }
    }

    void client_tuusin()//通信システム
    {
        string sousindata = "";//送信するデータ
        bool stageSelect = false, Start_zyunbOK = false, GameEnd = false, Score_End = false;//ステージ選択中か　全員ゲーム準備完了か ゲーム終了したか スコア処理が終了したか
        List<int> Starg_No = new List<int>(8);//受信ステージ番号用
        List<bool> zyunbiOK = new List<bool>(8);//ゲームの準備完了したか用
        List<bool> ScoreSyoriEnd = new List<bool>(8);//スコア表示処理が終了したか用
        int selct_stage_No = 0;//一番多かったステージ番号
        int room_zyoutai = 0;//部屋の状態
        //1:接続待機 2:メンバー決定 3:ステージ決定 4:クライアント全員が準備完了 5:順位確定(最下位以外ゴール) 　6:プレイヤー切断処理開始
        while (Tuusin)
        {
            string zyusindata = "";
            Debug.Log("TCP受信待機");
            for (int i = 0; i < client.Count; i++)//一人ひとりクライアントのデータを受信
            {
                string zyusindata1 = "";
                try
                {
                    zyusindata1 = tuusin.TCPzyusin_sitei(client[i]);
                    if (zyusindata1 == "") { Debug.Log("データなし"); continue; }
                }
                catch
                {
                    Debug.Log("受信エラー");
                    continue;
                }
                zyusindata += zyusindata1+"/";//ここで各クライアントのでを圧縮
            }

            if (!stageSelect)//ステージを決定する
            {
                if (Starg_No.Count == client.Count & client.Count != 0)//データがそろったら
                {
                    stageSelect = true;

                    selct_stage_No = Starg_No[Randam_e.Randam_System(0, Starg_No.Count)];//ランダムでステージ番号を選択
                    StargNo = selct_stage_No;
                    Debug.Log("ステージデータ揃った :" + selct_stage_No);
                    room_zyoutai = 3;
                    haiti = true;
                }
                else
                {
                    Starg_No.Clear();
                }
            }

            if (!Start_zyunbOK)//クライアント全員がステージ準備完了か
            {
                if (zyunbiOK.Count == client.Count & client.Count != 0)//データがそろったら
                {
                    Start_zyunbOK = true;
                    Debug.Log("全員がゲーム開始準備完了");
                    room_zyoutai = 4;
                    GameStart = true;
                }
                else
                {
                    zyunbiOK.Clear();
                }
            }

            try
            {
                if (!GameEnd & Start_zyunbOK)//順位確定
                {
                    if (Gool.Count == client.Count - 1)//最下位以外ゴールしたか
                    {
                        GameEnd = true;
                        Debug.Log("順位確定");
                        room_zyoutai = 5;
                    }
                }
            }
            catch { }

            if (!Score_End)//クライアントスコア処理完了か
            {
                if (ScoreSyoriEnd.Count == client.Count & client.Count != 0)//データがそろったら
                {
                    Score_End = true;
               
                    try//部屋をリセットする
                    {
                        Debug.Log("全員がスコア処理終了のため切断");
                        room_zyoutai = 6;
                        sousindata = string.Format("{0}_{1}/", 0, room_zyoutai);
                        for (int i = 0; i < client.Count; i++)//各クライアントに送信
                        {
                            tuusin.TCPsosin_sitei(client[i], sousindata);
                        }
                        UDP_Move = false;
                        stageSelect = false;
                        Start_zyunbOK = false;
                        GameEnd = false;
                        Score_End = false;
                        taiki = true;
                        GameStart = false;
                        haiti = false;
                        haitiOK = false;
                        Lap = 0;
                        Maxwall = 0;
                        Room_ninzuu = 0;
                        zyoutai = 0;
                        selct_stage_No = 0;
                        room_zyoutai = 0;
                        client.Clear();
                        ScoreSyoriEnd.Clear();
                        zyunbiOK.Clear();
                        Starg_No.Clear();
                        sousindata = "";
                        Roomreset = true;
                        continue;
                    }
                    catch(Exception e) { Debug.Log(e); }
                    Destroy(Starg);
                    for(int i=0;i<Player_model.Length ; i++)
                    {if(Player_model[i]!=null) Destroy(Player_model[i]);
                    } 
                }
                else
                {
                    ScoreSyoriEnd.Clear();
                }
            }

 

            sousindata = string.Format("{0}_{1}_{2}_{3}_/", 0, room_zyoutai, client.Count, selct_stage_No);
            //サーバーからの送信データ作成//部屋の状態_部屋にいる人数_選択されたステージ番号
            //受信したデータ//　番号　システム状態　プレイヤーの名前　機体番号　ステージ選択番号　プレイヤーのIPアドレス
            string[] data2 = zyusindata.Split('/');
            sousindata += zyusindata;
            for (int i = 0; i < data2.Length; i++)
            {
                string[] data3 = data2[i].Split('_');
                switch (data3[0])
                {
                    case "1":

                        switch (data3[1])
                        {
                            case "2"://部屋マスターが、メンバーを確定したら
                                Debug.Log("メンバー決定");
                                room_zyoutai = 2;
                                Room_ninzuu = client.Count;
                                zyoutai = 1;
                                taiki = false;
                                break;

                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));                                
                                break;

                            case "6"://ゲーム開始準備完了か
                                 zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[0] = int.Parse(data3[3]);//使用機体受信 
                            tuusin.IPdata[0]= data3[5];//クライアントのIPアドレス取得
                        }
 
                        break;

                    case "2":

                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[1] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[1] = data3[5];
                        }
                        break;

                    case "3":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Debug.Log(data3[1] + "  " + data3[4]);
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[2] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[2] = data3[5];
                        }
                        break;

                    case "4":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[3] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[3] = data3[5];
                        }
                        break;

                    case "5":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[4] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[4] = data3[5];
                        }
                        break;

                    case "6":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[5] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[5] = data3[5];
                        }
                        break;

                    case "7":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[6] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[6] = data3[5];
                        }
                        break;

                    case "8":
                        switch (data3[1])
                        {
                            case "4"://ステージセレクトしたか
                                Starg_No.Add(int.Parse(data3[4]));
                                break;

                            case "6"://ゲーム開始準備完了か
                                zyunbiOK.Add(true);
                                break;
                            case "8"://スコア処理終了
                                ScoreSyoriEnd.Add(true);
                                break;
                        }

                        if (taiki)
                        {
                            kitai_No[7] = int.Parse(data3[3]);//使用機体受信   
                            tuusin.IPdata[7] = data3[5];
                        }
                        break;
                }
            }

            for(int i=0;i<client.Count ; i++)//各クライアントに送信
            {
                tuusin.TCPsosin_sitei(client[i], sousindata);
            }
        }
    }


    void TCPclientIN()//TPCでのclient受け入れ非同期
    {
        while (Tuusin)
        {
            try
            {
                var tcp = lisetensr.AcceptTcpClient();//クライアントが接続しようとしたら 
                NetworkStream ns = tcp.GetStream();
                Debug.Log("接続処理開始");
                if (client.Count > client.Capacity) { Debug.Log("人数最大"); return; }
                client.Add(ns);
                tuusin.TCPsosin_sitei(ns, string.Format("{0}", client.Count));
                Debug.Log("クライアント接続完了");
            }
            catch { Debug.Log("クライアント受け入れエラー"); }

        }
    }



    //以下はデータ変換用

    Tuple<Vector3,Quaternion,int,int,Vector3,Quaternion> aiteTr(string s)//受信したプレイヤーのデータを変換 Tupleは複数のデータを返すことができる   複数のアイテムは　Item.1 Item.2となる 最大6個
    {
        string[] data = s.Split(',');
        return Tuple.Create(new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2])),
                            new Quaternion(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6])),
                            int.Parse(data[7]),int.Parse(data[8]),
                            new Vector3(float.Parse(data[9]), float.Parse(data[10]), float.Parse(data[11])),
                            new Quaternion(float.Parse(data[12]), float.Parse(data[13]), float.Parse(data[14]), float.Parse(data[15])));
        //座標　x,y,z    回転x,y,z,w　攻撃 死んだか  攻撃地点　攻撃地点の回転率
    }
}
