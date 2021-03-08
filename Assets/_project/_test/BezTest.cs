using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezTest : MonoBehaviour
{
    [ContextMenuItem("DrawTheLine", "DrawTheLine")]
    public Transform p0, p1, p2, p3;

    public void DrawTheLine() {
        //Vector3[] points = new Vector3[16];
        //for(int i = 0; i < points.Length; ++i) {
        //    float p = i / (points.Length - 1f);
        //    points[i]=Lines.GetBezierPoint(p0.position, p1.position, p2.position, p3.position, p);
        //}
        //Lines.Make("test").Line(points, Color.red);
        Lines.Make("test").Bezier(p0.position, p1.position, p2.position, p3.position, Color.red, Lines.End.Arrow);
    }
}
