using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OverheadHPToggle : MonoBehaviour
{
    public GameObject hpContainer;
    public GameObject hpGauge;

    void FixedUpdate()
    {
        if (hpGauge.GetComponent<Image>().fillAmount == 1)
        {
            hpContainer.SetActive(false);
        }
        else
        {
            hpContainer.SetActive(true);
        }
    }
}
