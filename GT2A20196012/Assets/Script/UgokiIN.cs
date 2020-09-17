using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UgokiIN : MonoBehaviour//モデルの動きをぶち込むスクリプト 当たり判定も管理
{
    Vector3 iti,sousiniti;//座標
    BoxCollider bx;
    Quaternion Rote,sosusinRote;//回転
    GameObject utu, tama;
    int Attack,hit,Gool,Des,Itemget,Rank;//攻撃　攻撃が自分に当たったか ゴールしたか 死んだか アイテムをゲットしたか 自分の順位

    int Lap,MaxLap,count, Maxcount;//現在のLap数 Lapの最大値数　現在の壁判定用　最大の壁判定番号
    // Start is called before the first frame update
    void Start()
    {
        Maxcount = Server_RoomSystem.Maxwall;
        MaxLap = Server_RoomSystem.Lap;
        tama = Resources.Load("Tama") as GameObject;
        bx = GetComponent<BoxCollider>();
        utu = transform.Find("Cube").gameObject;
        count = 1;
        Lap = 1;
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
                GameObject g = Instantiate(tama);
                g.transform.position = utu.transform.position;
                g.GetComponent<tama>().utu(utu.transform.forward*1000);
                Debug.Log("生成");
                Attack = 0;
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

    public  void trIN(Tuple<Vector3,Quaternion,int,int>tp)//クライアントからのデータを代入　座標　回転　
    {
        iti = tp.Item1;
        Rote = tp.Item2;
        if (Attack==0)
            Attack = tp.Item3;
        Des = tp.Item4;
    }

    public Tuple<Vector3,Quaternion,int,int,int,int,int> trOUT()//モデルの情報を出力
    {
        return Tuple.Create(sousiniti,sosusinRote,Attack, Des, hit,Gool,Itemget);
         // 座標x,y,z　回転z,y,z,w　攻撃 死んだか　当たった　ゴール　アイテムをゲット　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "tama")
        {
            hit = 1;
            Debug.Log(gameObject.name + " 攻撃当たった");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "sokusi")//即死判定に当たったら
        {
            hit = 3;
            Debug.Log("即死");
        }
        if (other.tag == "tama")//弾に当たったら
        {
            hit = 1;
        }

        if (other.tag == "item")//アイテムボックスに当たったら
        {
            Debug.Log("アイテム取得");
            Itemget = 1;
            other.gameObject.GetComponent<itemSy>().PlayerItemGET();
        }

        if (other.tag == "lacewall")//判定用壁に当たったら
        {
            if (Lap>=MaxLap& count >= Maxcount & other.gameObject.GetComponent<lacewall>().NoOUT() == 1)
            {
                Gool = 1;
                Debug.Log(gameObject.name + "ゴール");
            }
            else if (count>=Maxcount& other.gameObject.GetComponent<lacewall>().NoOUT() == 1)//最大数の壁に当たった後に最初の壁に当たったら
            {
                Lap++;
                count=1;
                Debug.Log("一周した");
            }
            else if (other.gameObject.GetComponent<lacewall>().NoOUT() == count + 1)//次の壁番号と同じだったら
            {
                count++;
                Debug.Log("壁に当たった");

            }
        }
    }

    public void Datareset()//データをクライアントに送信してからデータををリセット
    {
        hit = 0;
        Itemget = 0;
    }
}
