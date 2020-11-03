## MMORPG Kit Snippets
Utility scripts and components for the everyman.
Developed for:
https://assetstore.unity.com/packages/templates/systems/mmorpg-kit-2d-3d-survival-110188

### Overhead UI
The purpose of these scripts are to improve the rendering and behavior of Overhead UI.

Instructions:
- Attach any of the scripts to the following Prefabs (or similarly created prefabs)
    - CanvasOwningCharacterUI
    - CanvasNonOwningCharacterUI
    - CanvasMonsterCharacterUI
    - CanvasNPCUI
    - (To leverage the harvestable overhead ui, you need to create a CanvasHarvestableUI with UI Harvestable Entity and replace instances of Harvestable Entity with "Harvestable Entity with UI")
- Configure the parameters of the scripts to meet your needs
- Watch the magic!

### Tab Targeting
The purpose of these scripts are to improve tab targeting
Instructions:
- In your overwritten PlayerCharacterController, add the following:
    ```
    private TabTargeting _targeting;

    TabTargeting Targeting
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

    public override void UpdateInput() {
        
        if (SelectedEntity != Targeting.m_currentlySelectedTarget)
        {
            HandleTargetChange(Targeting.m_currentlySelectedTarget?.transform);
        }
        // some logic
        if (ConstructingBuildingEntity == null)
        {
            Targeting.HandleTargeting();
            if (SelectedEntity != Targeting.m_currentlySelectedTarget)
            {
                HandleTargetChange(Targeting.m_currentlySelectedTarget?.transform);
            }
            // some more logic
        }
        // the rest of the logic
    }

    protected override void OnPointClickOnGround(Vector3 targetPosition)
    {
        Targeting.m_currentlySelectedTarget = null;
        // do stuff or not (if you dont call base.OnPointClickOnGround it will disable click to move on the ground)
    }

    // Make sure the tab targeting addon updates the selected target properly
    protected virtual void HandleTargetChange(Transform tempTransform)
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
            }
        }
        else
        {
            SetTarget(null, TargetActionType.Attack);
        }
    }

    ```
Controls:
- Hitting tab will select closest targetable harvestable/npc/player/monster
- Hitting tab after will move the target to the right
- Holding shift and hitting tab will move the target to the left
- Hitting tab/shift+tab will cycle the target from the other end if you reach the end of the targetable list