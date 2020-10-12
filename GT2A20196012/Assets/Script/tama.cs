using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tama : MonoBehaviour
{
    UgokiIN UgokiINdata;

    public void utu(Vector3 v)
    {
        gameObject.GetComponent<Rigidbody>().AddForce(v);
    }

    private void Update()
    {
        Destroy(gameObject, 10);
    }

    private void OnCollisionEnter(Collision collision)
    {
       Destroy(gameObject);
    }

    public void DataIN(UgokiIN data)
    {
        UgokiINdata = data;
    }

    public UgokiIN DataOUT()
    {
        return UgokiINdata;
    }
}
