using I18N.Common;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using System.Linq;
using System.Collections.Generic;

public class TightenedPlayerController : PlayerCharacterController
{

    protected override void OnUseSkillOnEntity()
    {
        subTarget = SelectedEntity;
    }


    // Update is called once per frame
    public override void UpdateInput()
    {
        if (PlayerCharacterEntity.IsAttackingOrUsingSkill && PlayerCharacterEntity.GetTargetEntity() != subTarget)
        {
            PlayerCharacterEntity.SetTargetEntity(subTarget);
        }
        else if (subTarget != null && !PlayerCharacterEntity.IsAttackingOrUsingSkill && PlayerCharacterEntity.GetTargetEntity() == subTarget)
        {
            PlayerCharacterEntity.SetTargetEntity(SelectedEntity);
            subTarget = null;
        }

        ClearQueuedSkillIfInSafeZone();
        InheritedUpdateInput();
    }

    protected override void OnPointClickOnGround(Vector3 targetPosition)
    {
        Debug.Log("clicked on ground!");
        // Disable click to move
        Targeting.UnTarget(Targeting.SelectedTarget);
    }

    public virtual void InheritedUpdateInput()
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
            UpdateSelectedTarget();
            Targeting.HandleTargeting();

            if (InputManager.GetButtonDown("PickUpItem"))
            {
                PickUpItem();
            }
            if (InputManager.GetButtonDown("Reload"))
            {
                ReloadAmmo();
            }
            if (InputManager.GetButtonDown("ExitVehicle"))
            {
                PlayerCharacterEntity.CallServerExitVehicle();
            }
            if (InputManager.GetButtonDown("SwitchEquipWeaponSet"))
            {
                PlayerCharacterEntity.CallServerSwitchEquipWeaponSet((byte)(PlayerCharacterEntity.EquipWeaponSet + 1));
            }
            if (InputManager.GetButtonUp("Sprint"))
            {
                isSprinting = false;
            }
            if (InputManager.GetButtonDown("Sprint"))
            {
                isSprinting = true;
            }
            // Auto reload
            if (PlayerCharacterEntity.EquipWeapons.rightHand.IsAmmoEmpty() ||
                PlayerCharacterEntity.EquipWeapons.leftHand.IsAmmoEmpty())
            {
                ReloadAmmo();
            }
        }
        // Update enemy detecting radius to attack distance
        EnemyEntityDetector.detectingRadius = Mathf.Max(PlayerCharacterEntity.GetAttackDistance(false), wasdClearTargetDistance);
        // Update inputs
        UpdateQueuedSkill();
        UpdatePointClickInput();
        UpdateWASDInput();
        // Set sprinting state
        PlayerCharacterEntity.SetExtraMovement(isSprinting ? ExtraMovementState.IsSprinting : ExtraMovementState.None);
    }

    protected virtual void PickUpItem()
    {
        targetItemDrop = null;
        if (ItemDropEntityDetector.itemDrops.Count > 0)
            targetItemDrop = ItemDropEntityDetector.itemDrops[0];
        if (targetItemDrop != null)
            PlayerCharacterEntity.CallServerPickupItem(targetItemDrop.ObjectId);
    }

}