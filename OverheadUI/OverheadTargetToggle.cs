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
        public GameObject targetIndicator;

        void OnGUI()
        {
            BaseGameEntity target = BasePlayerCharacterController.Singleton.SelectedEntity;
            if (this.character)
                targetIndicator.SetActive(target != null && target.ObjectId == this.character.Data.ObjectId);
            if (npc)
                targetIndicator.SetActive(target != null && target.ObjectId == npc.Data.ObjectId);
        }
    }
}
