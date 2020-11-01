using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OverheadUI
{
    public class OverheadHPToggle : MonoBehaviour
    {
        public GameObject hpContainer;
        public GameObject hpGauge;

        void FixedUpdate()
        {
            hpContainer.SetActive(hpGauge.GetComponent<Image>().fillAmount < 1);
        }
    }
}
