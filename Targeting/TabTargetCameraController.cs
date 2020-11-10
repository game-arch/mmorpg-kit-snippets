using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class TabTargetCameraController : MonoBehaviour
{
    public GameObject target;

    float cameraPitch = 40.0f;
    float cameraYaw = 0;
    float cameraDistance = 5.0f;
    bool lerpYaw = false;
    bool lerpDistance = false;

    public float cameraPitchSpeed = 2.0f;
    public float cameraPitchMin = -10.0f;
    public float cameraPitchMax = 80.0f;
    public float cameraYawSpeed = 5.0f;
    public float cameraDistanceSpeed = 5.0f;
    public float cameraDistanceMin = 2.0f;
    public float cameraDistanceMax = 12.0f;
    public float cameraYOffset = 5f;

    public float focusFollowAngle = 150;
    public string savePrefsPrefix = "GAMEPLAY";

    protected Camera camera
    {
        get
        {
            PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            return controller?.CacheGameplayCamera;
        }
    }
    private void Start()
    {

        cameraYaw = PlayerPrefs.GetFloat(savePrefsPrefix + "_XRotation", cameraYaw);
        cameraPitch = PlayerPrefs.GetFloat(savePrefsPrefix + "_YRotation", cameraPitch);
        cameraDistance = PlayerPrefs.GetFloat(savePrefsPrefix + "_ZoomDistance", cameraDistance);
    }
    private void Update()
    {

        PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
        if (target != controller?.PlayerCharacterEntity?.gameObject)
            return;
        PlayerPrefs.SetFloat(savePrefsPrefix + "_XRotation", cameraYaw);
        PlayerPrefs.SetFloat(savePrefsPrefix + "_YRotation", cameraPitch);
        PlayerPrefs.SetFloat(savePrefsPrefix + "_ZoomDistance", cameraDistance);
        PlayerPrefs.Save();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!camera)
            return;
        PlayerCharacterController controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
        if (target != controller?.PlayerCharacterEntity?.gameObject)
            return;

        // If mouse button down then allow user to look around
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            cameraPitch -= Input.GetAxis("Mouse Y") * cameraPitchSpeed;
            cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
            cameraYaw += Input.GetAxis("Mouse X") * cameraYawSpeed;
            cameraYaw = cameraYaw % 360.0f;
        }
        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraDistanceSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMin, cameraDistanceMax);
            lerpDistance = false;
        }

        if (controller.Targeting && controller.Targeting.SelectedTarget != null && controller.Targeting.focusingTarget)
        {
            Vector3 focusPosition = controller.Targeting.SelectedTarget.transform.position;
            Vector3 diff = (target.transform.position - focusPosition);
            float angle = Vector3.SignedAngle(diff, camera.transform.forward, Vector3.up);
            if ((angle < 0 && angle > -focusFollowAngle) || (angle > 0 && angle < focusFollowAngle))
            {
                float angleDifference = focusFollowAngle - Mathf.Abs(angle);
                cameraYaw = Mathf.Lerp(cameraYaw, cameraYaw + (angleDifference * (angle < 0 ? -1 : 1)), cameraYawSpeed * Time.deltaTime);
                cameraYaw = cameraYaw % 360.0f;
            }

            cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, 20.0f);

            MoveCameraTo(controller.Targeting.SelectedTarget.transform, cameraYaw, cameraPitch, diff.magnitude);
            camera.transform.LookAt(focusPosition + (Vector3.up * cameraYOffset));
            return;
        }
        MoveCameraTo(target.transform, cameraYaw, cameraPitch);
        camera.transform.LookAt(target.transform.position + (Vector3.up * cameraYOffset));
    }

    void MoveCameraTo(Transform target, float x, float y, float distanceOffset = 0f)
    {
        Vector3 newCameraPosition = target.position + (Quaternion.Euler(y, x, 0) * Vector3.back * (distanceOffset + cameraDistance));

        RaycastHit hitInfo;
        LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Building");
        if (Physics.Linecast(target.position, newCameraPosition, out hitInfo, mask.value))
        {
            newCameraPosition = hitInfo.point;
            lerpDistance = true;
        }
        else
        {
            if (lerpDistance)
            {
                float newCameraDistance = Mathf.Lerp(Vector3.Distance(target.position, camera.transform.position), distanceOffset + cameraDistance, 5.0f * Time.deltaTime);
                newCameraPosition = target.position + (Quaternion.Euler(y, x, 0) * Vector3.back * newCameraDistance);
            }
        }

        camera.transform.position = newCameraPosition;
    }
}
