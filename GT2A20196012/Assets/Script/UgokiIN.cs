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
    int Attack,hit,Gool,Des;//攻撃　攻撃が自分に当たったか ゴールしたか 死んだか
    // Start is called before the first frame update
    void Start()
    {
        tama = Resources.Load("Tama") as GameObject;
        bx = GetComponent<BoxCollider>();
        utu = transform.Find("Cube").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = iti;
        transform.rotation = Rote;
        sousiniti = transform.position;
       sosusinRote= transform.rotation;
        switch (Attack)
        {
            case 1://攻撃
                GameObject g = Instantiate(tama);
                g.transform.position = utu.transform.position;
                g.GetComponent<tama>().utu(utu.transform.forward*1000);
                Debug.Log("生成");
                Attack = 0;
                break;
        }

        switch (Des)
        {
            case 0:
                bx.enabled = true;
                break;
            case 1:
                bx.enabled = false;
                break;
        }

    }

    public  void trIN(Tuple<Vector3,Quaternion,int,int>tp)//座標　回転　
    {
        iti = tp.Item1;
        Rote = tp.Item2;
        if (Attack==0)
            Attack = tp.Item3;
        Des = tp.Item4;
    }

    public Tuple<Vector3,Quaternion,int,int,int,int> trOUT()//モデルの情報を出力
    {
        return Tuple.Create(sousiniti,sosusinRote,Attack, Des, hit,Gool);
         // 座標x,y,z　回転z,y,z,w　攻撃 死んだか　当たった　ゴール　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　　
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "tama")
        {
            Debug.Log(gameObject.name + " 攻撃当たった");
        }

        if (collision.gameObject.tag == "gool")
        {
            Gool = 1;
            Debug.Log(gameObject.name + " ゴール");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "gool")
        {
            Gool = 1;
            Debug.Log(gameObject.name + "ゴール");
        }

        if (other.tag == "sokusi")
        {
            hit = 2;
            Debug.Log("即死");
            Invoke("Hitstyokika", 0.1f);
        }
        if (other.tag == "tama")
        {
            hit = 1;
            Invoke("Hitstyokika", 0.1f);
        }
    }

    void Hitstyokika()
    {
        hit = 0;
    }
}
