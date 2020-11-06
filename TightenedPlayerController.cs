using I18N.Common;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using System.Linq;
using System.Collections.Generic;

public class TightenedPlayerController : PlayerCharacterController
{



    protected override void Update()
    {
        if (PlayerCharacterEntity == null || !PlayerCharacterEntity.IsOwnerClient)
            return;

        if (Targeting.SelectedTarget == null && Targeting.PotentialTarget == null && Targeting.castingOnTarget == null)
            ClearTarget();
        if (CacheGameplayCameraControls != null)
            CacheGameplayCameraControls.target = CameraTargetTransform;

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

    protected override void OnPointClickOnGround(Vector3 targetPosition)
    {
        // Disable click to move
        Targeting.UnHighlightPotentialTarget();
        Targeting.UnTarget(Targeting.SelectedTarget);
    }


}