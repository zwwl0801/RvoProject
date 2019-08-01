using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KFrameWork;

public class SegmentTest : MonoBehaviour {
    public KInt2 v1;

    public KInt2 v2;

    public KInt2 v3;

    public KInt2 v4;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnDrawGizmos ()
    {
        #if UNITY_EDITOR
        UnityEditor.Handles.Label((new Vector3(v1.x,0,v1.y)+new Vector3(v2.x,0,v2.y))/2,"第一个线");
        Gizmos.DrawLine(new Vector3(v1.x,0,v1.y),new Vector3(v2.x,0,v2.y));

        Gizmos.color = Color.cyan;
        UnityEditor.Handles.Label((new Vector3(v3.x,0,v3.y)+new Vector3(v4.x,0,v4.y))/2,"第二个线");
        Gizmos.DrawLine(new Vector3(v3.x,0,v3.y),new Vector3(v4.x,0,v4.y));
        KInt2 result;
        if(Segment.Intersect(v1,v2,v3,v4,out result))
        {
            UnityEditor.Handles.Label(new Vector3(result.x,0,result.y),"交点");
        }

        #endif
    }
}
