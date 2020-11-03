using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController : BasePlayerCharacterController
    {
        protected TabTargeting _targeting;

        protected TabTargeting Targeting
        {
            get
            {
                if (!_targeting)
                {
                    GameObject go = new GameObject();
                    _targeting = go.AddComponent<TabTargeting>();
                    go.transform.SetParent(BasePlayerCharacterController.OwningCharacter.gameObject.transform);
                }
                return _targeting;
            }
        }
        protected virtual void UpdateSelectedTarget()
        {
            if (SelectedEntity != null && SelectedEntity != Targeting.m_currentlySelectedTarget)
            {
                Targeting.m_currentlySelectedTarget = SelectedEntity.gameObject;
            }
        }
        public virtual void HandleTargetChange(Transform tempTransform)
        {
            if (tempTransform)
            {
                targetPlayer = tempTransform.GetComponent<BasePlayerCharacterEntity>();
                targetMonster = tempTransform.GetComponent<BaseMonsterCharacterEntity>();
                targetNpc = tempTransform.GetComponent<NpcEntity>();
                targetItemDrop = tempTransform.GetComponent<ItemDropEntity>();
                targetHarvestable = tempTransform.GetComponent<HarvestableEntity>();
                targetBuilding = null;
                targetVehicle = tempTransform.GetComponent<VehicleEntity>();
                if (targetPlayer)
                {
                    // Found activating entity as player character entity
                    if (!targetPlayer.IsHideOrDead() && !targetPlayer.IsAlly(PlayerCharacterEntity))
                        SetTarget(targetPlayer, TargetActionType.Attack);
                    else
                        SetTarget(targetPlayer, TargetActionType.Activate);
                }
                else if (targetMonster && !targetMonster.IsHideOrDead())
                {
                    // Found activating entity as monster character entity
                    SetTarget(targetMonster, TargetActionType.Attack);
                }
                else if (targetNpc)
                {
                    // Found activating entity as npc entity
                    SetTarget(targetNpc, TargetActionType.Activate);
                }
                else if (targetItemDrop)
                {
                    // Found activating entity as item drop entity
                    SetTarget(targetItemDrop, TargetActionType.Activate);
                }
                else if (targetHarvestable && !targetHarvestable.IsDead())
                {
                    // Found activating entity as harvestable entity
                    SetTarget(targetHarvestable, TargetActionType.Attack);
                }
                else if (targetBuilding && !targetBuilding.IsDead() && targetBuilding.Activatable)
                {
                    // Found activating entity as building entity
                    SetTarget(targetBuilding, TargetActionType.Activate);
                }
                else if (targetVehicle)
                {
                    // Found activating entity as vehicle entity
                    SetTarget(targetVehicle, TargetActionType.Activate);
                }
                else
                {
                    SetTarget(null, TargetActionType.Attack);
                    isFollowingTarget = false;
                }
            }
            else
            {
                SetTarget(null, TargetActionType.Attack);
                isFollowingTarget = false;
            }
        }
        public virtual void Activate()
        {
            isFollowingTarget = true;
        }

    }
}