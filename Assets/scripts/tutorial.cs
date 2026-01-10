using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tutorial : MonoBehaviour
{
    public GameObject activeBlock;
    public GameObject shadowPanel;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = activeBlock.transform.position;
        pos.z -= 1; // в 2D: меньший Z = ближе к камере
        activeBlock.transform.position = pos;

        Debug.Log("Текущая Z: " + transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (activeBlock == null)
        {
            Destroy(shadowPanel);
        }
    }
}
