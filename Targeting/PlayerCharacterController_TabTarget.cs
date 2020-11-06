using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{

    public partial class PlayerCharacterController : BasePlayerCharacterController
    {

        public GameObject recticle;
        public bool uisOpen;
        public LayerMask TabTargetIgnoreLayers;

        public float TargetDistance
        {
            get
            {
                return lockAttackTargetDistance;
            }
        }

        protected TabTargeting _targeting;

        public TabTargeting Targeting
        {
            get
            {
                if (!_targeting)
                {
                    _targeting = BasePlayerCharacterController.OwningCharacter.gameObject.GetComponentInChildren<TabTargeting>();
                    if (!_targeting)
                    {
                        GameObject go = new GameObject();
                        _targeting = go.AddComponent<TabTargeting>();
                        go.transform.SetParent(BasePlayerCharacterController.OwningCharacter.gameObject.transform);
                    }
                }
                return _targeting;
            }
        }

        protected void TabTargetUpdateWASDAttack()
        {
            destination = null;
            BaseCharacterEntity targetEntity;

            if (TryGetSelectedTargetAsAttackingEntity(out targetEntity))
                SetTarget(targetEntity, TargetActionType.Attack, false);

            if (targetEntity != null && !targetEntity.IsHideOrDead())
            {
                // Set target, then attack later when moved nearby target
                SetTarget(targetEntity, TargetActionType.Attack, false);
                isFollowingTarget = true;
            }
            else
            {
                // No nearby target, so attack immediately
                if (PlayerCharacterEntity.CallServerAttack(isLeftHandAttacking))
                    isLeftHandAttacking = !isLeftHandAttacking;
                isFollowingTarget = false;
            }
        }

        protected virtual void TabTargetUpdateQueuedSkill()
        {
            if (PlayerCharacterEntity.IsDead())
            {
                ClearQueueUsingSkill();
                return;
            }
            if (queueUsingSkill.skill == null || queueUsingSkill.level <= 0)
                return;
            if (PlayerCharacterEntity.IsPlayingActionAnimation())
                return;
            destination = null;
            BaseSkill skill = queueUsingSkill.skill;
            short skillLevel = queueUsingSkill.level;
            Vector3? aimPosition = queueUsingSkill.aimPosition;
            BaseCharacterEntity targetEntity;
            // Point click mode always lock on target
            bool wasdLockAttackTarget = this.wasdLockAttackTarget || controllerMode == PlayerCharacterControllerMode.PointClick;

            if (skill.HasCustomAimControls())
            {
                // Target not required, use skill immediately
                TurnCharacterToPosition(aimPosition.Value);
                RequestUsePendingSkill();
                isFollowingTarget = false;
                return;
            }

            if (skill.IsAttack())
            {
                RequestUsePendingSkill();
                isFollowingTarget = false;
            }
            else
            {
                // Not attack skill, so use skill immediately
                if (skill.RequiredTarget())
                {
                    if (wasdLockAttackTarget)
                    {
                        // Set target, then use skill later when moved nearby target
                        if (TargetEntity != null && TargetEntity is BaseCharacterEntity)
                        {
                            SetTarget(TargetEntity, TargetActionType.UseSkill, false);
                            isFollowingTarget = true;
                        }
                        else
                        {
                            ClearQueueUsingSkill();
                            isFollowingTarget = false;
                        }
                    }
                    else
                    {
                        // Try apply skill to selected entity immediately, it will fail if selected entity is far from the character
                        if (TargetEntity != null && TargetEntity is BaseCharacterEntity)
                        {
                            if (TargetEntity != PlayerCharacterEntity)
                            {
                                // Look at target and use skill
                                TurnCharacterToEntity(TargetEntity);
                            }
                            RequestUsePendingSkill();
                            isFollowingTarget = false;
                        }
                        else
                        {
                            ClearQueueUsingSkill();
                            isFollowingTarget = false;
                        }
                    }
                }
                else
                {
                    // Target not required, use skill immediately
                    RequestUsePendingSkill();
                    isFollowingTarget = false;
                }
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