using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Crush_block : MonoBehaviour, IPointerClickHandler
{
    public bool canCrush = true;
    public GameObject breakEffect; // частицы
    public string obj_name; // имя объекта под которым нужно будет сохранить текстуры
    private Material matDefault; // материал на объекте изначально
    private Material matCrash; // материал при поломке
    private SpriteRenderer SpriteRend;
    public int count = 0;
    public int max_count;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (canCrush)
        {
            GameObject explosion = Instantiate(breakEffect, this.gameObject.transform.position, Quaternion.identity);
            explosion.GetComponent<ParticleSystem>().Play();
            count += 1;
            if (count >= max_count)
            {
                Destroy(gameObject);
            }
            ChangeMaterial();
            Destroy(explosion, explosion.GetComponent<ParticleSystem>().main.startLifetime.constantMax);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        SpriteRend = GetComponent<SpriteRenderer>();

        // Получаем название текущего материала
        string currentMaterialName = SpriteRend.material.name;
        Debug.Log("Current Material Name: " + currentMaterialName);

        // Сохраняем текущий материал
        matDefault = SpriteRend.material;
    }

    void ChangeMaterial()
    {
        // Получаем название текущего материала
        string currentMaterialName = SpriteRend.material.name;

        // Разделяем название по нижнему подчеркиванию
        string[] nameParts = currentMaterialName.Split(new char[] { '_', ' ' });

        // Изменяем порядковый номер в конце
        if (nameParts.Length > 0)
        {
            Debug.Log("nameParts[nameParts.Length-1] " + nameParts[nameParts.Length-1]);
            int currentNumber = 1;
            if(int.TryParse(nameParts[nameParts.Length-2], out currentNumber)){
                currentNumber += 1;
                nameParts[nameParts.Length - 2] = currentNumber.ToString();
            }
        }
        // Собираем новое название материала
        string newMaterialName = string.Join("_", nameParts, 0, nameParts.Length - 1);

        // Загружаем новый материал из ресурсов
        matCrash = Resources.Load<Material>(newMaterialName);

        // Проверяем, что новый материал загружен
        if (matCrash != null)
        {
            SpriteRend.material = matCrash;
        }
        else
        {
            Debug.LogError("Material " + newMaterialName + " not found in Resources.");
        }
    }
}
