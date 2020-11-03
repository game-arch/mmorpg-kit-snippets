using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class HarvestableEntityWithUI : HarvestableEntity
    {
        [Header("Relates Element Containers")]
        public Transform uiElementTransform;
        private UIHarvestableEntity uiHarvestableEntity;
        public Transform UIElementTransform
        {
            get
            {
                if (uiElementTransform == null)
                    uiElementTransform = CacheTransform;
                return uiElementTransform;
            }
        }
        public override void OnSetup()
        {
            base.OnSetup();
            Debug.Log(CurrentGameInstance.harvestableUI);
            if (CurrentGameInstance.harvestableUI != null)
                InstantiateUI(CurrentGameInstance.harvestableUI);
        }

        public void InstantiateUI(UIHarvestableEntity prefab)
        {
            if (prefab == null)
                return;
            if (uiHarvestableEntity != null)
                Destroy(uiHarvestableEntity.gameObject);
            uiHarvestableEntity = Instantiate(prefab, UIElementTransform);
            uiHarvestableEntity.transform.localPosition = Vector3.zero;
            uiHarvestableEntity.Data = this;
            Debug.Log(uiHarvestableEntity);
        }
    }
}