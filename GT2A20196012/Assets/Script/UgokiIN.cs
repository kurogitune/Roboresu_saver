using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UgokiIN : MonoBehaviour//モデルの動きをぶち込むスクリプト 当たり判定も管理
{
    Vector3 iti,sousiniti;//座標
    CapsuleCollider bx;
    Quaternion Rote,sosusinRote;//回転

    Vector3 Hanteiiti;
    Quaternion HanteiRote;
    [Header("機体種類 true:近距離 false:遠距離")]
    public bool kin_enn;

    [Header("近距離当たり判定")]
    public GameObject Hantei;
    [Header("遠距離弾")]
    public GameObject Tama;

    GameObject kinhantei;
    int Attack,hit,Gool,Des,Itemget,Rank;//攻撃　攻撃が自分に当たったか ゴールしたか 死んだか アイテムをゲットしたか 自分の順位

    int Lap,MaxLap,count, Maxcount,hanteiCount;//現在のLap数 Lapの最大値数　現在の壁判定用　最大の壁判定番号 順位判定用
    bool PlayerGool;
    // Start is called before the first frame update
    void Start()
    {
        Maxcount = Server_RoomSystem.Maxwall;
        MaxLap = Server_RoomSystem.Lap;
        bx = GetComponent<CapsuleCollider>();
        count = 1;
        Lap = 1;
        hanteiCount = 1;
        if (kin_enn)
        {
            kinhantei = Instantiate(Hantei);
            kinhantei.SetActive(false);
            kinhantei.layer = gameObject.layer;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = iti;
        transform.rotation = Rote;
        sousiniti = transform.position;
        sosusinRote= transform.rotation;
        switch (Attack)//攻撃したら
        {
            case 1://攻撃

                if (kin_enn)
                {
                    kinhantei.SetActive(true);
                    kinhantei.transform.position = Hanteiiti;
                    kinhantei.transform.rotation = HanteiRote;
                }
                else
                {
                    GameObject g = Instantiate(Tama);
                    g.transform.position = Hanteiiti;
                    g. transform.rotation = HanteiRote;
                    g.layer = gameObject.layer;
                    g.GetComponent<tama>().utu(Hanteiiti*2000);
                }
                Attack = 0;
                if (kin_enn)
                {
                    kinhantei.SetActive(false);
                }
                break;
        }

        switch (Des)//死んだら
        {
            case 0:
                bx.enabled = true;
                break;
            case 1:
                bx.enabled = false;
                break;
        }
        Debug.Log("周回数  "+Lap+"  壁count   "+count);
    }

    public  void trIN(Tuple<Vector3,Quaternion,int,int,Vector3,Quaternion>tp)//クライアントからのデータを代入　座標　回転　
    {
        iti = tp.Item1;
        Rote = tp.Item2;
        if (Attack==0)
            Attack = tp.Item3;
        Des = tp.Item4;
        Hanteiiti = tp.Item5;
        HanteiRote = tp.Item6;
    }

    public string trOUT()//モデルの情報を文字列に変換
    {
          return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                                sousiniti.x,sousiniti.y,sousiniti.z,
                                sosusinRote.x,sosusinRote.y,sosusinRote.z,sosusinRote.w,
                                Attack,Des,hit,Gool,Itemget,Lap,Rank);
        // 座標x,y,z　回転z,y,z,w　攻撃 死んだか　当たった　ゴール　アイテムをゲット　周回数　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "sokusi")//即死判定に当たったら
        {
            hit = 5;
            Debug.Log("即死");
        }
      
        if (other.tag == "item")//アイテムボックスに当たったら
        {
            Debug.Log("アイテム取得");
            Itemget = 1;
            other.gameObject.GetComponent<itemSy>().PlayerItemGET();
        }

        if (other.tag == "lacewall"&!PlayerGool)//判定用壁に当たったら
        {
            if (Lap>=MaxLap& count >= Maxcount & other.gameObject.GetComponent<lacewall>().NoOUT() == 1)
            {
                Gool = 1;
                PlayerGool = true;
                Server_RoomSystem.Gool.Add(true);
                Debug.Log(gameObject.name + "ゴール");
            }
            else if (count>=Maxcount& other.gameObject.GetComponent<lacewall>().NoOUT() == 1)//最大数の壁に当たった後に最初の壁に当たったら
            {
                Lap++;
                hanteiCount++;
                count=1;
                Debug.Log("一周した");
            }
            else if (other.gameObject.GetComponent<lacewall>().NoOUT() == count + 1)//次の壁番号と同じだったら
            {
                count++;
                hanteiCount++;
                Debug.Log("壁に当たった");

            }
        }
    }

    public Tuple<int,int> RoteData()//周回数を出力
    {
        return Tuple.Create(Lap, count);
    }

    public void RankIN(int i)//順位を代入
    {
        Rank = i;
        Debug.Log(gameObject.name+"  順位 "+i);
    }

    public int hanteiCountOUT()//順位判定用の値出力用
    {
        return hanteiCount;
    }

    public void Datareset()//データをクライアントに送信してからデータををリセット
    {
        hit = 0;
        Itemget = 0;
    }
}
