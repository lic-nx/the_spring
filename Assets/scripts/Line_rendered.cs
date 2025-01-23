using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
<<<<<<< HEAD
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
=======
     public EdgeCollider2D edgeCollider;
    private LineRenderer lr;
    private List<Vector3> points;

    private void Awake()
    {   edgeCollider = GetComponent<EdgeCollider2D>();
        lr = GetComponent<LineRenderer>();
        points = new List<Vector3>();

    }

    // позволяет менять последнюю точку 
    public void SetLastPoint(Vector3 point)
    {
        if (points.Count > 0)
        {
            points[points.Count-1] = point;
        }
    }

// добавляет точку
    public void SetUpLine(Vector3 point)
    {   Debug.Log("lr before change" + lr.positionCount);
        lr.positionCount++;
        Debug.Log("lr after");
        Debug.Log(lr.positionCount);

        points.Add(point);
        Debug.Log(points.Count);
    }

    private void Update()
    {   Vector2[] pointsArray = new Vector2[points.Count];
        
        for (int i = 0; i < points.Count; i++)
        {   
            pointsArray[i] = new Vector2(points[i].x, points[i].y);
            lr.SetPosition(i, points[i]);
            // edgeCollider.SetPosition(i, new Vector2(points[i].x, points[i].y));
            
        
        }
        edgeCollider.points = pointsArray;
    }
>>>>>>> 65d96d2 (add new scen)
}
