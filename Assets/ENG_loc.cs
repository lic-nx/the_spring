using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ENG_loc : MonoBehaviour
{
    // Start is called before the first frame update
    public void SetEnglish()
    {
        LocalizationManager.Instance.SetLanguage("en");
    }

}
