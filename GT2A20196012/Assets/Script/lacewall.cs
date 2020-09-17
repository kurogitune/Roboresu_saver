using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lacewall : MonoBehaviour//レース用の壁判定処理でデータ//保存用
{
 public   int No;//壁の番号
    public int NoOUT()//番号出力用
    {
        return No;
    } 

    public void NoIN(int i)//番号代入
    {
        No = i;
    }
}
