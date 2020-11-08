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

        public bool isAutoAttacking;

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

        protected virtual void TabTargetUpdate()
        {
            if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
                return;

            if (Targeting.SelectedTarget == null && Targeting.PotentialTarget == null && Targeting.CastingTarget == null)
                ClearTarget();
            if (CacheGameplayCameraControls != null)
            {
                CacheGameplayCameraControls.target = CameraTargetTransform;
            }

            if (CacheMinimapCameraControls != null)
                CacheMinimapCameraControls.target = CameraTargetTransform;

            if (CacheTargetObject != null)
                CacheTargetObject.gameObject.SetActive(destination.HasValue);

            if (PlayerCharacterEntity.IsDead())
            {
                ClearTarget();
                ClearQueueUsingSkill();
                destination = null;
                isFollowingTarget = false;
                CancelBuild();
                CacheUISceneGameplay.SetTargetEntity(null);
            }
            else
            {
                TabTargetUpdateTarget();
            }

            if (destination.HasValue)
            {
                if (CacheTargetObject != null)
                    CacheTargetObject.transform.position = destination.Value;
                if (Vector3.Distance(destination.Value, MovementTransform.position) < StoppingDistance + 0.5f)
                    destination = null;
            }

            Targeting.UpdateTargeting();
            ClearQueuedSkillIfInSafeZone();
            TabTargetUpdateInput();
            TabTargetUpdateFollowTarget();
        }
        protected virtual void TabTargetOnPointClickOnGround(Vector3 targetPosition)
        {
            OnPointClickOnGround(targetPosition);
            // Disable click to move
            Targeting.UnHighlightPotentialTarget();
            Targeting.UnTarget(Targeting.SelectedTarget);
        }

        public virtual void Activate()
        {
            Transform tempTransform = SelectedEntity ? SelectedEntity.transform : null;
            if (tempTransform == null)
                return;
            targetPlayer = tempTransform.GetComponent<BasePlayerCharacterEntity>();
            targetMonster = tempTransform.GetComponent<BaseMonsterCharacterEntity>();
            targetNpc = tempTransform.GetComponent<NpcEntity>();
            targetItemDrop = tempTransform.GetComponent<ItemDropEntity>();
            targetHarvestable = tempTransform.GetComponent<HarvestableEntity>();
            targetBuilding = null;
            targetVehicle = tempTransform.GetComponent<VehicleEntity>();
            targetActionType = TargetActionType.Activate;
            isFollowingTarget = true;
            isAutoAttacking = false;
            // Priority Player -> Npc -> Buildings
            if (targetPlayer != null)
                CacheUISceneGameplay.SetActivePlayerCharacter(targetPlayer);
            else if (targetMonster != null || targetHarvestable != null)
            {
                targetActionType = TargetActionType.Attack;
                isAutoAttacking = true;
            }
            else if (targetNpc != null)
                PlayerCharacterEntity.CallServerNpcActivate(targetNpc.ObjectId);
            else if (targetBuilding != null)
            {
                SelectedEntity = targetBuilding;
                ActivateBuilding(targetBuilding);
            }
            else if (targetVehicle != null)
                PlayerCharacterEntity.CallServerEnterVehicle(targetVehicle.ObjectId);
            else if (targetWarpPortal != null)
                PlayerCharacterEntity.CallServerEnterWarp(targetWarpPortal.ObjectId);
        }
        protected GameObject cachePotentialTarget;
        protected GameObject cacheSelectedTarget;
        protected GameObject cacheCastingTarget;

        protected BaseGameEntity CacheSelectedTarget
        {
            get
            {
                if (cacheSelectedTarget != Targeting.SelectedTarget)
                    cacheSelectedTarget = Targeting.SelectedTarget;
                return cacheSelectedTarget ? cacheSelectedTarget.GetComponent<BaseGameEntity>() : null;
            }
        }
        protected BaseGameEntity CacheCastingTarget
        {
            get
            {
                if (cacheCastingTarget != Targeting.CastingTarget)
                    cacheCastingTarget = Targeting.CastingTarget;
                return cacheCastingTarget ? cacheCastingTarget.GetComponent<BaseGameEntity>() : null;
            }
        }
        protected BaseGameEntity CachePotentialTarget
        {
            get
            {
                if (cachePotentialTarget != Targeting.PotentialTarget)
                    cachePotentialTarget = Targeting.PotentialTarget;
                return cachePotentialTarget ? cachePotentialTarget.GetComponent<BaseGameEntity>() : null;
            }
        }
        protected BaseGameEntity CacheActionTarget
        {
            get
            {
                return CachePotentialTarget ?? CacheSelectedTarget;
            }
        }
        public virtual void TabTargetUpdateTarget()
        {
            bool hasChanged = cachePotentialTarget != Targeting.PotentialTarget || cacheSelectedTarget != Targeting.SelectedTarget;
            PlayerCharacterEntity.SetSubTarget(CachePotentialTarget);
            PlayerCharacterEntity.SetTargetEntity(CacheSelectedTarget);
            PlayerCharacterEntity.SetCastingTarget(CacheCastingTarget);

            SelectedEntity = CacheSelectedTarget;
            TargetEntity = SelectedEntity;
            if (hasChanged)
            {
                CacheUISceneGameplay.SetTargetEntity(null);
                CacheUISceneGameplay.SetTargetEntity(CacheActionTarget);
            }
        }

        public virtual void TabTargetUpdateInput()
        {
            bool isFocusInputField = GenericUtils.IsFocusInputField() || UIElementUtils.IsUIElementActive();
            bool isPointerOverUIObject = CacheUISceneGameplay.IsPointerOverUIObject();
            if (CacheGameplayCameraControls != null)
            {
                CacheGameplayCameraControls.updateRotationX = false;
                CacheGameplayCameraControls.updateRotationY = false;
                CacheGameplayCameraControls.updateRotation = !isFocusInputField && !isPointerOverUIObject && InputManager.GetButton("CameraRotate");
                CacheGameplayCameraControls.updateZoom = !isFocusInputField && !isPointerOverUIObject;
            }

            if (isFocusInputField)
                return;

            if (PlayerCharacterEntity.IsDead())
                return;

            // If it's building something, don't allow to activate NPC/Warp/Pickup Item
            if (ConstructingBuildingEntity == null)
            {
                Targeting.HandleTargeting();

                if (InputManager.GetButtonDown("PickUpItem"))
                    PickUpItem();
                if (InputManager.GetButtonDown("Reload"))
                    ReloadAmmo();
                if (InputManager.GetButtonDown("ExitVehicle"))
                    PlayerCharacterEntity.CallServerExitVehicle();
                if (InputManager.GetButtonDown("SwitchEquipWeaponSet"))
                    PlayerCharacterEntity.CallServerSwitchEquipWeaponSet((byte)(PlayerCharacterEntity.EquipWeaponSet + 1));
                if (InputManager.GetButtonDown("Sprint"))
                    isSprinting = !isSprinting;
                // Auto reload
                if (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                    PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
                    ReloadAmmo();
            }
            // Update enemy detecting radius to attack distance
            EnemyEntityDetector.detectingRadius = Mathf.Max(PlayerCharacterEntity.GetAttackDistance(false), wasdClearTargetDistance);
            // Update inputs
            TabTargetUpdateQueuedSkill();
            TabTargetUpdatePointClickInput();
            TabTargetUpdateWASDInput();
            // Set sprinting state
            PlayerCharacterEntity.SetExtraMovement(isSprinting ? ExtraMovementState.IsSprinting : ExtraMovementState.None);
        }


        public virtual void TabTargetUpdateWASDInput()
        {
            if (controllerMode == PlayerCharacterControllerMode.PointClick)
                return;

            if (CacheGameplayCameraTransform != null)
            {
                // If mobile platforms, don't receive input raw to make it smooth
                bool raw = !InputManager.useMobileInputOnNonMobile && !Application.isMobilePlatform;
                Vector3 moveDirection = GetMoveDirection(InputManager.GetAxis("Horizontal", raw), InputManager.GetAxis("Vertical", raw));
                moveDirection.Normalize();

                // Move
                if (moveDirection.sqrMagnitude > 0f)
                {
                    HideNpcDialog();
                    ClearQueueUsingSkill();
                    destination = null;
                    isFollowingTarget = false;
                    if (!PlayerCharacterEntity.IsPlayingActionAnimation())
                        PlayerCharacterEntity.SetLookRotation(Quaternion.LookRotation(moveDirection));
                }
                // Always forward
                MovementState movementState = MovementState.Forward;
                if (InputManager.GetButtonDown("Jump"))
                    movementState |= MovementState.IsJump;
                PlayerCharacterEntity.KeyMovement(moveDirection, movementState);
            }
        }



        protected virtual void PickUpItem()
        {
            targetItemDrop = null;
            if (ItemDropEntityDetector.itemDrops.Count > 0)
                targetItemDrop = ItemDropEntityDetector.itemDrops[0];
            if (targetItemDrop != null)
                PlayerCharacterEntity.CallServerPickupItem(targetItemDrop.ObjectId);
        }

        public virtual void TabTargetUpdatePointClickInput()
        {
            if (controllerMode == PlayerCharacterControllerMode.WASD)
                return;

            // If it's building something, not allow point click movement
            if (ConstructingBuildingEntity != null)
                return;

            // If it's aiming skills, not allow point click movement
            if (UICharacterHotkeys.UsingHotkey != null)
                return;

            getMouseDown = Input.GetMouseButtonDown(0);
            getMouseUp = Input.GetMouseButtonUp(0);
            getMouse = Input.GetMouseButton(0);

            if (getMouseDown)
            {
                isMouseDragOrHoldOrOverUI = false;
                mouseDownTime = Time.unscaledTime;
                mouseDownPosition = Input.mousePosition;
            }
            // Read inputs
            isPointerOverUI = CacheUISceneGameplay.IsPointerOverUIObject();
            isMouseDragDetected = (Input.mousePosition - mouseDownPosition).sqrMagnitude > DETECT_MOUSE_DRAG_DISTANCE_SQUARED;
            isMouseHoldDetected = Time.unscaledTime - mouseDownTime > DETECT_MOUSE_HOLD_DURATION;
            isMouseHoldAndNotDrag = !isMouseDragDetected && isMouseHoldDetected;
            if (!isMouseDragOrHoldOrOverUI && (isMouseDragDetected || isMouseHoldDetected || isPointerOverUI))
            {
                // Detected mouse dragging or hold on an UIs
                isMouseDragOrHoldOrOverUI = true;
            }
            // Will set move target when pointer isn't point on an UIs 
            if (!isPointerOverUI && (getMouse || getMouseUp))
            {
                didActionOnTarget = false;
                // Prepare temp variables
                Transform tempTransform;
                Vector3 tempVector3;
                bool tempHasMapPosition = false;
                Vector3 tempMapPosition = Vector3.zero;
                BuildingMaterial tempBuildingMaterial;
                // If mouse up while cursor point to target (character, item, npc and so on)
                bool mouseUpOnTarget = getMouseUp && !isMouseDragOrHoldOrOverUI;
                int tempCount = FindClickObjects(out tempVector3);
                for (int tempCounter = 0; tempCounter < tempCount; ++tempCounter)
                {
                    tempTransform = physicFunctions.GetRaycastTransform(tempCounter);
                    // When holding on target, or already enter edit building mode
                    if (isMouseHoldAndNotDrag)
                    {
                        targetBuilding = null;
                        tempBuildingMaterial = tempTransform.GetComponent<BuildingMaterial>();
                        if (tempBuildingMaterial != null)
                            targetBuilding = tempBuildingMaterial.BuildingEntity;
                        if (targetBuilding && !targetBuilding.IsDead())
                        {
                            Targeting.Target(targetBuilding.gameObject);
                            break;
                        }
                    }
                    else if (mouseUpOnTarget)
                    {
                        if (tempTransform.gameObject.GetComponent<BaseGameEntity>() == CacheSelectedTarget)
                        {
                            Activate();
                        }
                        else
                        {
                            Targeting.UnHighlightPotentialTarget();
                            Targeting.Target(tempTransform.gameObject);
                        }
                    } // End mouseUpOnTarget
                }
                // When clicked on map (Not touch any game entity)
                // - Clear selected target to hide selected entity UIs
                // - Set target position to position where mouse clicked
                if (tempHasMapPosition)
                {
                    targetPosition = tempMapPosition;
                }
                // When clicked on map (any non-collider position)
                // tempVector3 is come from FindClickObjects()
                // - Clear character target to make character stop doing actions
                // - Clear selected target to hide selected entity UIs
                // - Set target position to position where mouse clicked
                if (CurrentGameInstance.DimensionType == DimensionType.Dimension2D && mouseUpOnTarget && tempCount == 0)
                {
                    ClearTarget();
                    tempVector3.z = 0;
                    targetPosition = tempVector3;
                }

                // Found ground position
                if (targetPosition.HasValue)
                {
                    // Close NPC dialog, when target changes
                    HideNpcDialog();
                    ClearQueueUsingSkill();
                    isFollowingTarget = false;
                    if (PlayerCharacterEntity.IsPlayingActionAnimation())
                    {
                        if (pointClickInterruptCastingSkill)
                            PlayerCharacterEntity.CallServerSkillCastingInterrupt();
                    }
                    else
                    {
                        OnPointClickOnGround(targetPosition.Value);
                    }
                }
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
            Vector3? aimPosition = queueUsingSkill.aimPosition;
            if (skill.HasCustomAimControls())
            {
                // Target not required, use skill immediately
                TurnCharacterToPosition(aimPosition.Value);
                RequestUsePendingSkill();
                isFollowingTarget = false;
                return;
            }

            if (skill.IsAttack() || skill.RequiredTarget())
            {
                // Let's stick to tab targeting instead of finding a random entity
                if (CacheActionTarget != null && CacheActionTarget is BaseCharacterEntity)
                {
                    UseSkillOn(CacheActionTarget);
                    return;
                }
                ClearQueueUsingSkill();
                return;
            }
            // Target not required, use skill immediately
            RequestUsePendingSkill();
        }

        private void UseSkillOn(BaseGameEntity target)
        {
            Targeting.CastingTarget = target.gameObject;
            if (Targeting.SelectedTarget == null)
                Targeting.Target(Targeting.CastingTarget);
            Targeting.UnHighlightPotentialTarget();
            TabTargetUpdateTarget();
            TurnCharacterToPosition(target.transform.position);
            RequestUsePendingSkill();
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
                targetActionType = TargetActionType.Activate;
                isAutoAttacking = false;
                return;
            }
            isFollowingTarget = false;
        }
        public bool CanAttack<T>(out T entity)
            where T : DamageableEntity
        {
            if (!TryGetDoActionEntity(out entity, TargetActionType.Attack))
                return false;
            if (entity == PlayerCharacterEntity)
            {
                entity = null;
                return false;
            }
            if (!(entity is HarvestableEntity))
            {
                if (entity.IsInSafeArea)
                {
                    PlayerCharacterEntity.CurrentGameManager.SendServerGameMessage(PlayerCharacterEntity.ConnectionId,
                        CustomGameMessage.NoCastingInSafeArea);
                    isFollowingTarget = false;
                    isAutoAttacking = false;
                    entity = null;
                    return false;
                }
                if (PlayerCharacterEntity.Entity.IsInSafeArea)
                {
                    PlayerCharacterEntity.CurrentGameManager.SendServerGameMessage(PlayerCharacterEntity.ConnectionId,
                        CustomGameMessage.NoCastingInSafeArea);
                    isFollowingTarget = false;
                    isAutoAttacking = false;
                    entity = null;
                    return false;
                }
            }
            return true;
        }

        public void TabTargetUpdateFollowTarget()
        {
            if (!isFollowingTarget)
                return;
            // Use the set target action type for the following actions
            if (TryGetDoActionEntity(out targetPlayer))
            {
                DoActionOrMoveToEntity(targetPlayer, CurrentGameInstance.conversationDistance, () =>
                {
                    // TODO: Do something
                });
                return;
            }

            if (TryGetDoActionEntity(out targetNpc))
            {
                DoActionOrMoveToEntity(targetNpc, CurrentGameInstance.conversationDistance, () =>
                {
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        PlayerCharacterEntity.CallServerNpcActivate(targetNpc.ObjectId);
                    }
                });
                return;
            }

            if (TryGetDoActionEntity(out targetItemDrop))
            {
                DoActionOrMoveToEntity(targetItemDrop, CurrentGameInstance.pickUpItemDistance, () =>
                {
                    PlayerCharacterEntity.CallServerPickupItem(targetItemDrop.ObjectId);
                    ClearTarget();
                });
                return;
            }
            // use attack
            targetActionType = TargetActionType.Attack;
            if (isAutoAttacking && CanAttack(out targetDamageable))
            {
                if (targetDamageable.IsHideOrDead())
                {
                    ClearQueueUsingSkill();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    Targeting.UnTarget(Targeting.SelectedTarget);
                    return;
                }
                float attackDistance = 0f;
                float attackFov = 0f;
                GetAttackDistanceAndFov(isLeftHandAttacking, out attackDistance, out attackFov);
                AttackOrMoveToEntity(targetDamageable, attackDistance, CurrentGameInstance.characterLayer.Mask);
                return;
            }
            // use skill
            targetActionType = TargetActionType.UseSkill;
            if (TryGetUsingSkillEntity(out targetDamageable))
            {
                if (queueUsingSkill.skill.IsAttack() && targetDamageable.IsHideOrDead())
                {
                    ClearQueueUsingSkill();
                    PlayerCharacterEntity.StopMove();
                    ClearTarget();
                    Targeting.UnTarget(Targeting.SelectedTarget);
                    return;
                }
                float castDistance = 0f;
                float castFov = 0f;
                GetUseSkillDistanceAndFov(isLeftHandAttacking, out castDistance, out castFov);
                UseSkillOrMoveToEntity(targetDamageable, castDistance);
                return;
            }
            targetActionType = TargetActionType.Activate;
            if (TryGetDoActionEntity(out targetBuilding, TargetActionType.Activate))
            {
                DoActionOrMoveToEntity(targetBuilding, CurrentGameInstance.conversationDistance, () =>
                {
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        ActivateBuilding(targetBuilding);
                    }
                });
                return;
            }

            targetActionType = TargetActionType.ViewOptions;
            if (TryGetDoActionEntity(out targetBuilding, TargetActionType.ViewOptions))
            {
                DoActionOrMoveToEntity(targetBuilding, CurrentGameInstance.conversationDistance, () =>
                {
                    if (!didActionOnTarget)
                    {
                        didActionOnTarget = true;
                        ShowCurrentBuildingDialog();
                    }
                });
                return;
            }

            targetActionType = TargetActionType.Activate;
            if (TryGetDoActionEntity(out targetVehicle))
            {
                DoActionOrMoveToEntity(targetVehicle, CurrentGameInstance.conversationDistance, () =>
                {
                    PlayerCharacterEntity.CallServerEnterVehicle(targetVehicle.ObjectId);
                    ClearTarget();
                    Targeting.UnTarget(Targeting.SelectedTarget);
                });
            }
        }
    }
}