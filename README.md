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
- In the extended PlayerCharacterController, remember to remove the `Input.GetButtonDown("FindEnemy")` handling, as this will replace it
- In your overwritten PlayerCharacterController, add the following:
    ```
    public override void UpdateInput() {
        // some logic
        if (ConstructingBuildingEntity == null)
        {
            UpdateSelectedTarget();
            Targeting.HandleTargeting();
            // Remove the Input.GetButtonDown("FindEnemy") check and execution!
            // some more logic
        }
        // the rest of the logic
    }

    protected override void OnPointClickOnGround(Vector3 targetPosition)
    {
        Targeting.m_currentlySelectedTarget = null;
        // do stuff or not (if you dont call base.OnPointClickOnGround it will disable click to move on the ground)
    }
    ```
Controls:
- Hitting tab will select closest targetable harvestable/npc/player/monster
- Hitting tab after will move the target to the right
- Holding shift and hitting tab will move the target to the left
- Hitting tab/shift+tab will cycle the target from the other end if you reach the end of the targetable list