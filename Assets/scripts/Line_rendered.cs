using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line_rendered : MonoBehaviour
{
<<<<<<< HEAD
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
=======
>>>>>>> develop
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
<<<<<<< HEAD
=======
            lr.SetPosition(lr.positionCount-1, point);
>>>>>>> develop
        }
    }

// добавляет точку
    public void SetUpLine(Vector3 point)
<<<<<<< HEAD
    {   Debug.Log("lr before change" + lr.positionCount);
        lr.positionCount++;
        Debug.Log("lr after");
        Debug.Log(lr.positionCount);

        points.Add(point);
        Debug.Log(points.Count);
=======
    {   
    // Debug.Log("lr before change" + lr.positionCount);
        
    //     Debug.Log("lr after");
    //     Debug.Log(lr.positionCount);

        points.Add(point);
        Debug.Log(points.Count);
        lr.positionCount++;
        lr.SetPosition(lr.positionCount-1, points[points.Count-1]);
>>>>>>> develop
    }

    private void Update()
    {   Vector2[] pointsArray = new Vector2[points.Count];
        
        for (int i = 0; i < points.Count; i++)
        {   
            pointsArray[i] = new Vector2(points[i].x, points[i].y);
<<<<<<< HEAD
            lr.SetPosition(i, points[i]);
            // edgeCollider.SetPosition(i, new Vector2(points[i].x, points[i].y));
=======
        //     lr.SetPosition(i, points[i]);
        //     // edgeCollider.SetPosition(i, new Vector2(points[i].x, points[i].y));
>>>>>>> develop
            
        
        }
        edgeCollider.points = pointsArray;
    }
<<<<<<< HEAD
>>>>>>> 65d96d2 (add new scen)
=======
>>>>>>> develop
}
