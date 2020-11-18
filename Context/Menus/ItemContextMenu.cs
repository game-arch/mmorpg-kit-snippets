using System.Collections;
using System.Collections.Generic;
using MultiplayerARPG;
using UnityEngine;

namespace Context
{
    public class ItemContextMenu : MonoBehaviour
    {
        public UICharacterItem item;

        public void OnEquip()
        {
            item.OnClickEquip();
            ContextConfig.Singleton.DestroyMenu();
        }

        public void OnUnEquip()
        {
            item.OnClickUnEquip();
            ContextConfig.Singleton.DestroyMenu();
        }

        public void OnUse()
        {
            item.OnClickUse();
            ContextConfig.Singleton.DestroyMenu();
        }

        public void OnDrop()
        {
            item.OnClickDrop();
            ContextConfig.Singleton.DestroyMenu();
        }
    }
}