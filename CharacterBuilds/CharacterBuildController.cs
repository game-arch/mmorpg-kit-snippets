using UnityEngine;
using UnityEngine.Events;
using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLibManager;
using System;

namespace CharacterBuilds
{
    public class CharacterBuildEvent : UnityEvent<Build>
    {

    }
    public class CharacterBuildController : MonoBehaviour
    {
        public static UnityEvent OnLoad = new UnityEvent();
        public static PlayerCharacterEntity player;
        public static CharacterBuilds builds;
        void Start()
        {
            player = BasePlayerCharacterController.OwningCharacter as PlayerCharacterEntity;
            LoadBuilds();
            if (builds == null)
                InitializeBuilds(player.DataId);
        }
        public static void InitializeBuilds(int playerEntityId)
        {
            builds = new CharacterBuilds();
            builds.playerEntityId = playerEntityId;
            SaveBuilds();
        }
        public static void LoadBuilds()
        {
            try
            {
                string json = PlayerPrefs.GetString("builds_" + player.DataId);
                if (json?.Length > 0)
                {
                    builds = JsonUtility.FromJson<CharacterBuilds>(json);
                    Debug.Log(json);
                    OnLoad.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void ApplyBuild(int index)
        {
            if (builds.builds.Length > index)
            {
                try
                {
                    player.UnequipAll();
                    player.UnSetHotKeys();
                    player.EquipAll(builds.builds[index].equipment);
                    player.SetHotKeys(builds.builds[index].hotKeys);
                    builds.currentBuild = index;
                    CharacterBuildController.SaveBuilds();
                }
                catch (Exception e)
                {
                    Debug.Log("Could not apply build " + builds.builds[index].label);
                    Debug.Log(e);
                }
            }
        }

        public static void SaveBuilds()
        {
            string json = JsonUtility.ToJson(builds, true);
            PlayerPrefs.SetString("builds_" + player.DataId, json);
            OnLoad.Invoke();
        }
        public static Build CreateBuild()
        {
            Build build = Build.Create();
            build.equipment = BuildGear().ToArray();
            build.hotKeys = BuildHotKeys().ToArray();

            return build;
        }

        public static List<BuildHotKey> BuildHotKeys()
        {
            List<BuildHotKey> hotKeys = new List<BuildHotKey>();
            CharacterHotkey hotKey;
            for (int i = 0; i < player.Hotkeys.Count; i++)
            {
                hotKey = player.Hotkeys[i];
                hotKeys.Add(new BuildHotKey(hotKey.hotkeyId, hotKey.type, hotKey.relateId));
            }
            return hotKeys;
        }
        public static List<BuildEquipItem> BuildGear()
        {
            IArmorItem tempArmorItem;
            CharacterItem tempEquipItem;
            List<BuildEquipItem> gear = new List<BuildEquipItem>();
            for (int i = 0; i < player.EquipItems.Count; i++)
            {
                tempEquipItem = player.EquipItems[i];
                tempArmorItem = tempEquipItem.GetArmorItem();
                if (tempArmorItem == null)
                    continue;
                string equipPosition = GetEquipPosition(tempArmorItem.EquipPosition, tempEquipItem.equipSlotIndex);
                gear.Add(new BuildEquipItem(equipPosition, tempEquipItem.id));
            }
            EquipWeapons weapons;
            for (int i = 0; i < player.SelectableWeaponSets.Count; i++)
            {
                weapons = player.SelectableWeaponSets[i];
                string leftPosition = GetEquipPosition(GameDataConst.EQUIP_POSITION_LEFT_HAND, (byte)i);
                string rightPosition = GetEquipPosition(GameDataConst.EQUIP_POSITION_RIGHT_HAND, (byte)i);
                if (weapons.rightHand?.GetEquipmentItem()?.DataId != null)
                    gear.Add(new BuildEquipItem(rightPosition, weapons.rightHand.id, i));
                if (weapons.leftHand?.GetEquipmentItem()?.DataId != null)
                    gear.Add(new BuildEquipItem(leftPosition, weapons.leftHand.id, i));
            };
            return gear;
        }

        public static string GetEquipPosition(string equipPositionId, byte equipSlotIndex)
        {
            return equipPositionId + ":" + equipSlotIndex;
        }

        public static byte GetEquipSlotIndexFromEquipPosition(string equipPosition)
        {
            string[] splitEquipPosition = equipPosition.Split(':');
            return byte.Parse(splitEquipPosition[splitEquipPosition.Length - 1]);
        }
    }


}