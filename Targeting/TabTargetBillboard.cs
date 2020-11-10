using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabTargetBillboard : MonoBehaviour
{
    public Camera targetCamera;

    public Transform CacheTransform { get; private set; }
    public Transform CacheCameraTransform { get; private set; }

    private void OnEnable()
    {
        CacheTransform = transform;
        SetupCamera();
    }

    private bool SetupCamera()
    {
        if (targetCamera == null)
        {
            PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            if (controller != null)
            {
                targetCamera = controller.CacheGameplayCamera;
                if (targetCamera != null)
                    CacheCameraTransform = targetCamera.transform;
            }
        }
        return targetCamera != null;
    }

    private void LateUpdate()
    {
        if (!SetupCamera())
            return;
        CacheTransform.rotation = Quaternion.Euler(Quaternion.LookRotation(CacheCameraTransform.forward, CacheCameraTransform.up).eulerAngles);
    }
}
