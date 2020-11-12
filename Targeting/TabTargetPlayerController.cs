
using MultiplayerARPG;
using UnityEngine;

public class TabTargetPlayerController : PlayerCharacterController
{
    protected override void Update()
    {
        TabTargetUpdate();
    }

    protected override void OnPointClickOnGround(Vector3 targetPosition)
    {
        TabTargetOnPointClickOnGround(targetPosition);
    }


}