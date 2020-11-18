using System.Collections;
using System.Collections.Generic;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Context
{
    public class UIContextMenu : MonoBehaviour, IPointerUpHandler
    {

        public MonsterCharacterEntity monster;
        public NpcEntity npc;
        public PlayerCharacterEntity player;
        public UICharacterItem uiCharacterItem;
        public UICharacterSkill uiCharacterSkill;

        public ItemContextMenu itemMenu;
        public SkillContextMenu skillMenu;
        public PlayerContextMenu playerMenu;
        public NPCContextMenu npcMenu;
        public MonsterContextMenu monsterMenu;

        // Update is called once per frame


        public void OnPointerUp(PointerEventData eventData)
        {
            if (Input.GetMouseButtonUp(1))
                OpenMenu();
        }
        public void OnMouseOver()
        {
            if (Input.GetMouseButtonUp(1))
                OpenMenu();
        }

        void OpenMenu()
        {
            if (itemMenu != null && uiCharacterItem != null)
            {
                ItemContextMenu menu = Instantiate(itemMenu);
                menu.item = uiCharacterItem;
                ContextConfig.Singleton.CreateMenu(menu.gameObject);
            }
            
            if (skillMenu != null && uiCharacterSkill != null)
            {
                SkillContextMenu menu = Instantiate(skillMenu);
                menu.skill = uiCharacterSkill;
                ContextConfig.Singleton.CreateMenu(menu.gameObject);
            }
            
            if (playerMenu != null && player != null)
            {
                PlayerContextMenu menu = Instantiate(playerMenu);
                menu.player = player;
                ContextConfig.Singleton.CreateMenu(menu.gameObject);
            }
            
            if (npcMenu != null && npc != null)
            {
                NPCContextMenu menu = Instantiate(npcMenu);
                menu.npc = npc;
                ContextConfig.Singleton.CreateMenu(menu.gameObject);
            }
            
            if (monsterMenu != null && monster != null)
            {
                MonsterContextMenu menu = Instantiate(monsterMenu);
                menu.monster = monster;
                ContextConfig.Singleton.CreateMenu(menu.gameObject);
            }
        }
    }
}