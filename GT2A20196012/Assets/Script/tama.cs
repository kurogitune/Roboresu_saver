using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tama : MonoBehaviour
{
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
}
