using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
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
            lr.SetPosition(lr.positionCount-1, point);
        }
    }

// добавляет точку
    public void SetUpLine(Vector3 point)
    {   
        points.Add(point);
        Debug.Log(points.Count);
        lr.positionCount++;
        lr.SetPosition(lr.positionCount-1, points[points.Count-1]);
    }

    public Vector3 GetLastPoint()
    {
        return points[points.Count-2];

    }
    private void Update()
    {   Vector2[] pointsArray = new Vector2[points.Count];
        
        for (int i = 0; i < points.Count; i++)
        {   
            pointsArray[i] = new Vector2(points[i].x, points[i].y);
        //     lr.SetPosition(i, points[i]);
        //     // edgeCollider.SetPosition(i, new Vector2(points[i].x, points[i].y));
            
        
        }
        edgeCollider.points = pointsArray;
    }
}
