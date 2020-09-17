using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Expansion
{
    public static class Randam_e
    {
        public static int Randam_Unity(int min, int max)
        {
            int d=UnityEngine.Random.Range(min, max);
            return d;
        }

        public static int Randam_System(int min, int max)
        {
            System.Random ran = new System.Random();
            int d=   ran.Next(min, max);
            return d;
        }
    }

    public class objList//指定のタグの付いたオブジェクトを全て取得しリストで返す
    {
        public static List<GameObject> tag_All_obj(string tag)
        {
            List<GameObject> objdata=new List<GameObject>();
            List<GameObject> returnobjdata=new List<GameObject>();
            foreach(GameObject boj in UnityEngine.Object.FindObjectsOfType(typeof(GameObject)))
            {
                if (boj.tag == tag)
                {
                    objdata.Add(boj);
                }
            }
            for(int i=objdata.Count-1;i>-1 ; i--)
            {
                returnobjdata.Add(objdata[i]);
            }
            return returnobjdata;
        }
    }
}
