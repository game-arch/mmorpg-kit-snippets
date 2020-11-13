using System.Linq;

using LiteNetLib;
using LiteNetLibManager;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CharacterBuilds;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterEntity : BasePlayerCharacterEntity
    {


        private bool CallServerEquipWeapon(short nonEquipIndex, byte equipWeaponSet, bool isLeftHand)
        {
            if (!CanDoActions() ||
                nonEquipIndex >= NonEquipItems.Count)
                return false;
            RPC(ServerEquipWeapon, nonEquipIndex, equipWeaponSet, isLeftHand);
            return true;
        }
        public void Equip(string id, string slot, int equipSet = 0)
        {
            short nonEquipIndex = (short)BasePlayerCharacterController.OwningCharacter.IndexOfNonEquipItem(id);
            if (nonEquipIndex > -1)
            {
                if (slot.Contains("_HAND"))
                    CallServerEquipWeapon(nonEquipIndex, (byte)equipSet, slot.Contains("LEFT_"));
                else
                    RequestEquipItem(nonEquipIndex);
            }
        }

        public void UnequipAll()
        {
            CharacterItem[] equipment = EquipItems.ToArray();
            for (int i = 0; i < equipment.Length; i++)
            {
                if (equipment[i].GetArmorItem() != null)
                    RequestUnEquipItem(InventoryType.EquipItems, equipment[i].equipSlotIndex, 0);
            }
            for (int i = 0; i < SelectableWeaponSets.Count; i++)
            {
                if (SelectableWeaponSets[i].rightHand.GetWeaponItem() != null)
                    RequestUnEquipItem(InventoryType.EquipWeaponRight, 0, (byte)i);
                if (SelectableWeaponSets[i].leftHand.GetWeaponItem() != null)
                    RequestUnEquipItem(InventoryType.EquipWeaponLeft, 0, (byte)i);
            };
        }
        public void EquipAll(BuildEquipItem[] items)
        {
            foreach (BuildEquipItem equipItem in items)
            {
                Equip(equipItem.id, equipItem.slot, equipItem.equipSet);
            }
        }
        public void UnSetHotKeys()
        {
            Hotkeys.Clear();
        }
        public void SetHotKeys(BuildHotKey[] hotKeys)
        {
            foreach (BuildHotKey hotKey in hotKeys)
            {
                Debug.Log("Load " + hotKey.type + " " + hotKey.relateId + " in " + hotKey.hotkeyId);
            }
        }

        public string GetEquipPosition(string equipPositionId, byte equipSlotIndex)
        {
            return equipPositionId + ":" + equipSlotIndex;
        }

        public byte GetEquipSlotIndexFromEquipPosition(string equipPosition)
        {
            string[] splitEquipPosition = equipPosition.Split(':');
            return byte.Parse(splitEquipPosition[splitEquipPosition.Length - 1]);
        }
    }
}