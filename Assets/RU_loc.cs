using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RU_loc : MonoBehaviour
{
    // Start is called before the first frame update
    public void SetRussian()
    {
        LocalizationManager.Instance.SetLanguage("ru");
    }

}
