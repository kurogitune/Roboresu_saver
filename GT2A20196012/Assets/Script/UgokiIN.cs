using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UgokiIN : MonoBehaviour//モデルの動きをぶち込むスクリプト 当たり判定も管理
{
    Vector3 iti,sousiniti;//座標
    CapsuleCollider bx;
    Quaternion Rote,sosusinRote;//回転
    UgokiIN GUgokiData;
    Vector3 Hanteiiti;
    Quaternion HanteiRote;
    GameObject kinhantei,zyuukouhantei;//近接当たり判定 遠距離銃口位置
    [Header("機体種類 true:近距離 false:遠距離")]
    public bool kin_enn;
    [Header("近距離当たり判定")]
    public GameObject Hantei;
    [Header("遠距離銃口位置")]
    public GameObject zyuukou;
    [Header("遠距離弾")]
    public GameObject Tama;
    [Header("遠距離種類")]
    public bool M_S;//trueマシンガン　false:スナイパー

    [Header("攻撃当たった後の無敵時間")]
    public float HitMutekiTime;

    int Attack,hit,Gool,Des,Itemget,Rank,EnemyKill,Anime_data,Itemdata;//攻撃　攻撃が自分に当たったか ゴールしたか 死んだか アイテムをゲットしたか 自分の順位 相手を殺したか アニメーションデータ　使用したアイテムデータ

    int Lap,MaxLap,count, Maxcount,hanteiCount;//現在のLap数 Lapの最大値数　現在の壁判定用　最大の壁判定番号 順位判定用
    bool PlayerGool,Muteki,Desb;//プレイヤーがゴールしたか 一時的に無敵か 死んでいるか
    float MutekiTime;//攻撃を食らっての一時的な無敵時間
    // Start is called before the first frame update
    void Start()
    {
        Maxcount = Server_RoomSystem.Maxwall;
        MaxLap = Server_RoomSystem.Lap;
        bx = GetComponent<CapsuleCollider>();
        MutekiTime = HitMutekiTime;
        count = 1;
        Lap = 1;
        hanteiCount = 1;
        if (kin_enn)
        {
            kinhantei = Instantiate(Hantei);
            kinhantei.SetActive(false);
            kinhantei.layer = gameObject.layer;
        }
        else
        {
            zyuukouhantei = Instantiate(zyuukou);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = iti;
        transform.rotation = Rote;
        sousiniti = transform.position;
        sosusinRote= transform.rotation;

        if (PlayerGool) return;

        if (Muteki)
        {
            MutekiTime -= Time.deltaTime;
            if (MutekiTime < 0)
            {
                Muteki = false;
                MutekiTime = HitMutekiTime;
            }
        }
        switch (Attack)//攻撃したら
        {
            case 1://攻撃

                if (kin_enn)
                {//近接用攻撃処理
                    kinhantei.SetActive(true);
                    kinhantei.transform.parent = transform;
                    kinhantei.transform.position = Hanteiiti;
                    kinhantei.transform.rotation = HanteiRote;
                }
                else
                {//遠距離用攻撃処理
                    zyuukouhantei.transform.position = Hanteiiti;
                    zyuukouhantei.transform.rotation = HanteiRote;
                    GameObject g = Instantiate(Tama);
                    g.transform.position = zyuukouhantei.transform.position;
                    g. transform.rotation = zyuukouhantei.transform.rotation;
                    g.layer = gameObject.layer;
                    g.GetComponent<tama>().DataIN(GetComponent<UgokiIN>());
                    if(M_S) g.GetComponent<tama>().utu(zyuukouhantei.transform.up * 2000);
                    else g.GetComponent<tama>().utu(zyuukouhantei.transform.up * 4000);

                }
                Attack = 0;
                break;

            case 0:
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
                if (GUgokiData != null)
                {
                    GUgokiData.KillDes();
                    DataDes();
                    Debug.Log("相手殺した");
                }
                break;
        }
        Debug.Log("周回数  "+Lap+"  壁count   "+count);
    }

    public void CliantData_IN(string s)//クライアントのデータ（文字代入） そして　データを変換
    {
        string[] data = s.Split(',');

        iti = new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));//座標

        Rote = new Quaternion(float.Parse(data[3]), float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));//回転率

        if (Attack == 0)//攻撃
            Attack = int.Parse(data[7]);

        Des = int.Parse(data[8]);//死んだか

        Hanteiiti = new Vector3(float.Parse(data[9]), float.Parse(data[10]), float.Parse(data[11]));//攻撃位置

        HanteiRote = new Quaternion(float.Parse(data[12]), float.Parse(data[13]), float.Parse(data[14]), float.Parse(data[15]));//攻撃の回転率

        Anime_data = int.Parse(data[16]);//アニメーションデータ

        Itemdata = int.Parse(data[17]);//使用アイテムデータ

        //座標　x,y,z    回転x,y,z,w　攻撃 死んだか  攻撃地点　攻撃地点の回転率　アニメーションデータ　アイテム使用データ
    }

    public string trOUT()//モデルの情報を送信用文字列に変換
    {
          return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16}",
                                sousiniti.x,sousiniti.y,sousiniti.z,
                                sosusinRote.x,sosusinRote.y,sosusinRote.z,sosusinRote.w,
                                Attack,Des, Anime_data,Itemdata,
                                hit,Gool,Itemget,Lap,Rank,EnemyKill);
        // 座標x,y,z　回転z,y,z,w　攻撃 死んだか アニメーション（送信先の敵情報）
        //当たった　ゴール　アイテムをゲット　周回数　敵を殺したか（送信先のクライアントのデータ）
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
                hanteiCount++;
                Server_RoomSystem.Gool.Add(true);
                Debug.Log(gameObject.name + "ゴール");
                PlayerGool = true;
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

    public void OnCollisionEnter(Collision collision)
    {
        Debug.Log("攻撃当たった");
        if (Muteki) return;
        if (collision.gameObject.tag == "Tama_M")//マシンガン弾Hit
        {
            hit = 1;
            Muteki = true;
            GUgokiData = collision.gameObject.GetComponent<tama>().DataOUT();
            Invoke("DataDes", 1);
        }

        if (collision.gameObject.tag == "Tama_S")//スナイパー弾Hit
        {
            hit = 2;
            Muteki = true;
            GUgokiData = collision.gameObject.GetComponent<tama>().DataOUT();
            Invoke("DataDes", 1);
        }

        if (collision.gameObject.tag == "Kobusi")//防御Hit
        {
            hit = 3;
            Muteki = true;
            GUgokiData = collision.gameObject.transform.parent.GetComponent<UgokiIN>();
            Invoke("DataDes", 1);
        }

        if (collision.gameObject.tag == "Sword")//剣Hit
        {
            hit = 4;
            Muteki = true;
            GUgokiData = collision.gameObject.transform.parent.GetComponent<UgokiIN>();
            Invoke("DataDes", 1);
        }
    }

    void DataDes()
    {
        GUgokiData = null;
    }

    public void KillDes()//殺したか
    {
        EnemyKill = 1;
    }

    public Tuple<int,int> RoteData()//周回数を出力
    {
        return Tuple.Create(Lap, count);
    }

    public void RankIN(int i)//順位を代入
    {
        if (!PlayerGool) return;
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
        EnemyKill = 0;
    }
}
