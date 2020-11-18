using System.Collections;
using System.Collections.Generic;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Context
{
    public class UIContextMenu : MonoBehaviour, IPointerDownHandler
    {

        public MonsterCharacterEntity monster;
        public NpcEntity npc;
        public PlayerCharacterEntity player;
        public UICharacterItem uiCharacterItem;
        public UICharacterSkill uiCharacterSkill;


        public ContextMenuItemData[] menuItems;




        public void OnPointerDown(PointerEventData eventData)
        {
            if (Input.GetMouseButtonDown(1))
                OpenMenu();
        }

        bool HasMatchingComponent<T>(GameObject go, T matcher)
        {
            if (matcher == null)
                return false;
            T found = go.GetComponent<T>();
            if (found == null)
                found = go.GetComponentInParent<T>();
            if (found != null)
                return found.Equals(matcher) || matcher.Equals(go.GetComponentInParent<T>());
            return false;
        }

        void OpenMenu()
        {
            Debug.Log("Open Menu!");
            ContextConfig.Singleton.CreateMenu(menuItems);
        }
    }
}