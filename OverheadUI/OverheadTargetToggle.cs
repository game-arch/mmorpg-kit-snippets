using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OverheadUI
{
    public class OverheadTargetToggle : MonoBehaviour
    {
        public UICharacterEntity character;
        public UINpcEntity npc;
        public UIHarvestableEntity harvestable;
        public GameObject targetIndicator;
        public GameObject subTargetIndicator;
        public string targetedLayer = "TargetedUI";
        public string defaultLayer = "CharacterUI";

        void OnGUI()
        {
            PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            if (controller)
            {
                BaseGameEntity target = controller.SelectedEntity;
                BaseGameEntity subTarget = controller.subTarget;
                if (character)
                {
                    subTargetIndicator.SetActive(subTarget != null && subTarget.ObjectId == character.Data.ObjectId);
                    targetIndicator.SetActive(target != null && target.ObjectId == character.Data.ObjectId);
                }
                if (npc)
                {
                    subTargetIndicator.SetActive(subTarget != null && subTarget.ObjectId == npc.Data.ObjectId);
                    targetIndicator.SetActive(target != null && target.ObjectId == npc.Data.ObjectId);
                }
                if (harvestable)
                {
                    subTargetIndicator.SetActive(subTarget != null && subTarget.ObjectId == harvestable.Data.ObjectId);
                    targetIndicator.SetActive(target != null && target.ObjectId == harvestable.Data.ObjectId);
                }
                gameObject.layer = LayerMask.NameToLayer(targetIndicator.activeSelf || subTargetIndicator.activeSelf ? targetedLayer : defaultLayer);
            }
        }
    }
}
