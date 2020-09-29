﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UgokiIN : MonoBehaviour//モデルの動きをぶち込むスクリプト 当たり判定も管理
{
    Vector3 iti,sousiniti;//座標
    BoxCollider bx;
    Quaternion Rote,sosusinRote;//回転
    GameObject utu, tama;
    public LayerMask mask1;

    int Attack,hit,Gool,Des,Itemget,Rank;//攻撃　攻撃が自分に当たったか ゴールしたか 死んだか アイテムをゲットしたか 自分の順位

    int Lap,MaxLap,count, Maxcount,hanteiCount;//現在のLap数 Lapの最大値数　現在の壁判定用　最大の壁判定番号 順位判定用
    bool PlayerGool;
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
        hanteiCount = 1;
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
                //GameObject g = Instantiate(tama);
                //g.transform.position = utu.transform.position;
                //g.GetComponent<tama>().utu(utu.transform.forward*1000);
                //Debug.Log("生成");
                RaycastHit hit;
                Ray ray=new Ray(utu.transform.position,transform.forward);
                if(Physics.Raycast(ray, out hit,10,mask1))
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        hit.collider.gameObject.GetComponent<UgokiIN>().Tamahit();
                    }
                }
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

    public string trOUT()//モデルの情報を文字列に変換
    {
          return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
                                sousiniti.x,sousiniti.y,sousiniti.z,
                                sosusinRote.x,sosusinRote.y,sosusinRote.z,sosusinRote.w,
                                Attack,Des,hit,Gool,Itemget,Lap,Rank);
        // 座標x,y,z　回転z,y,z,w　攻撃 死んだか　当たった　ゴール　アイテムをゲット　周回数　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
    }

    public void Tamahit()
    {
            hit = 2;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "sokusi")//即死判定に当たったら
        {
            hit = 4;
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
