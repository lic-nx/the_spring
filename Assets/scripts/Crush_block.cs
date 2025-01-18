using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Crush_block : MonoBehaviour, IPointerClickHandler
{   public bool canCrush;
    public GameObject breakEffect; // частицы 
    public string obj_name; // имя объекта под которым нужно будет сохранить текстуры
    private Material matDefault; // материал на объекте изначально 
    private Material matCrash; // материал при поломке
    private SpriteRenderer SpriteRend;
    public int count = 0;
    public int max_count;
    public void OnPointerClick(PointerEventData eventData){
        if (canCrush == true){
            GameObject explosion = Instantiate(breakEffect, this.gameObject.transform.position, Quaternion.identity);
            explosion.GetComponent<ParticleSystem>().Play();
            count += 1;
            SpriteRend.material = matCrash;
            if (count >= max_count){
                Destroy(gameObject);
                
            }
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.startLifetime.constantMax);
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        
        SpriteRend = GetComponent<SpriteRenderer>();
        string materialName = obj_name + "Crash";
        matCrash = Resources.Load(materialName, typeof(Material)) as Material;
        matDefault = SpriteRend.material;
    }


}
