using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UISceneGameplay : BaseUISceneGameplay
    {
        public UIOrdering uiSystemDialog;
        [HideInInspector] public List<UIOrdering> orderedUIs = new List<UIOrdering>();


        protected UIOrdering[] uis;
        UIOrdering[] Uis
        {
            get
            {
                if (uis == null)
                    uis = GetComponentsInChildren<UIOrdering>(true);
                return uis;
            }
        }


        protected PlayerCharacterController controller;

        PlayerCharacterController Controller
        {
            get
            {
                if (controller == null)
                    controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
                return controller;
            }
        }

        protected virtual void UpdateToggles()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && orderedUIs.Count > 0)
            {
                UIOrdering lastUi = orderedUIs.Last();
                lastUi.Hide();
                foreach (UIOrdering peer in lastUi.closePeersOnEscape)
                {
                    peer.Hide();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && Controller.Targeting.SelectedTarget == null && Controller.Targeting.PotentialTarget == null)
            {
                uiSystemDialog.Show();
            }
            Controller.uisOpen = orderedUIs.Count > 0;
            foreach (UIOrdering ui in Uis)
            {
                ui.UpdateInput();
            }

        }
    }
}