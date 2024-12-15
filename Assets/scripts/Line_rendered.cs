using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
    private LineRenderer lr;
    private Transform[] points;
    private void Awake(){
        lr = GetComponent<LineRenderer>();
    }
    public void SetUpLine(Transform points){
        lr.positionCount++;
        this.points.add(points);
    }
    private void Update(){
        for (int i = 0; i < points.Length; i++){
            lr.SetPosition(i, points[i].position);
        }
    }
}
