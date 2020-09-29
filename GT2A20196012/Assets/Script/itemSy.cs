using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemSy : MonoBehaviour
{
    SphereCollider coll;
    int zyoutai;//アイテムの状態// 1:存在している 2:プレイヤーに取られた
    float ItemrisponTime=3;//アイテムが取られて再設置されるまでの時間
    public  bool status;//取られたか
    // Start is called before the first frame update

    // Update is called once per frame
    private void Start()
    {
        zyoutai = 1;
        coll = GetComponent<SphereCollider>();
    }
    void Update()
    {
        if (!status) return;

        ItemrisponTime -= Time.deltaTime;

        if (ItemrisponTime <= 0)
        {
            ItemrisponTime = 3;
            status = false;
            coll.enabled = true;
            zyoutai = 1;
        }
    }


    public void PlayerItemGET()//プレイヤーがアイテムを取ったら取得
    {
        if (status) return;
        status = true;
        coll.enabled = false;
        zyoutai = 2;
    }
}
