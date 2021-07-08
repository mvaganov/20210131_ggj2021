using NonStandard;
using NonStandard.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HeapTest : MonoBehaviour
{
    public GameObject obj;
    public int count = 100;
    public int grabClosest = 20;
    public float generationRadius = 20;
    void Start()
    {
        List<GameObject> list = new List<GameObject>();
        for(int i = 0; i < count; ++i) {
            GameObject go = Instantiate(obj);
            go.transform.SetParent(transform);
            go.transform.localPosition = Random.insideUnitSphere * generationRadius;
            if(go == null) { throw new Exception("null instantiate?"); }
            list.Add(go);
		}
        Vector3 p = transform.position;
        GameObject[] closest = list.GetClosest(grabClosest, i => i.transform.position.Distance(p));
        //Debug.Log(closest.JoinToString(", ", i => Vector3.Distance(i.transform.position, p).ToString()));
        Array.ForEach(closest, go => { go.transform.localScale *= 0.5f; });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
