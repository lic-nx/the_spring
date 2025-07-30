using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class frigger_checker : MonoBehaviour
{
    public bool OnTriggerEnter_ = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("An object entered.");
        player_move._instance.stop();
        
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("sun"))
        {

            Debug.Log("is sun");
            Debug.Log("Object name: " + other.name);
            Debug.Log("find sun = " + other.transform.position );
            StartCoroutine(HandleSunInteraction(other.transform));
        }
        else{    
            OnTriggerEnter_ = true;
        }
    }

    private IEnumerator HandleSunInteraction(Transform sunTransform)
    {
        // Запускаем поворот цветка и ждём его завершения
        yield return flower._instance.rotate_flower_to_sun(sunTransform.position);

        // После того как цветок повернулся — выполняем остальные действия
        player_move._instance.stop();
        player_move._instance.is_sun();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerEnter_ = false;
    }

}
