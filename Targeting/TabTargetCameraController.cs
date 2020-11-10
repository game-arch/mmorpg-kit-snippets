using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class TabTargetCameraController : MonoBehaviour
{

    float cameraPitch = 40.0f;
    float cameraYaw = 0;
    float cameraDistance = 5.0f;
    bool lerpDistance = false;

    public float cameraPitchSpeed = 2.0f;
    public float cameraPitchMin = -10.0f;
    public float cameraPitchMax = 80.0f;
    public float cameraYawSpeed = 5.0f;
    public float cameraDistanceSpeed = 5.0f;
    public float cameraDistanceMin = 2.0f;
    public float cameraDistanceMax = 12.0f;
    public float cameraYOffset = 2f;

    public string savePrefsPrefix = "GAMEPLAY";

    protected float yawOffset = 0f;
    protected float pitchOffset = 0f;
    protected float maxOffset = 20f;
    protected GameObject dummy;


    protected GameObject FocusTarget
    {
        get
        {
            return Controller.Targeting.SelectedTarget;
        }
    }
    protected PlayerCharacterController Controller
    {
        get
        {
            return BasePlayerCharacterController.Singleton as PlayerCharacterController;
        }
    }
    protected GameObject Player
    {
        get
        {
            return Controller?.PlayerCharacterEntity?.gameObject;
        }
    }

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
        dummy = new GameObject();
        cameraYaw = PlayerPrefs.GetFloat(savePrefsPrefix + "_XRotation", cameraYaw);
        cameraPitch = PlayerPrefs.GetFloat(savePrefsPrefix + "_YRotation", cameraPitch);
        cameraDistance = PlayerPrefs.GetFloat(savePrefsPrefix + "_ZoomDistance", cameraDistance);
    }
    private void Update()
    {

        if (!Player)
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
        if (!Player)
            return;

        // If mouse button down then allow user to look around
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {

            if (IsFocusing())
            {
                float modifiedPitch = pitchOffset - Input.GetAxis("Mouse Y") * cameraPitchSpeed;
                float modifiedYaw = yawOffset + Input.GetAxis("Mouse X") * cameraYawSpeed;
                pitchOffset = Mathf.Clamp(modifiedPitch, 0, maxOffset);
                yawOffset = Mathf.Clamp(modifiedYaw, -maxOffset, maxOffset);
            }
            else
            {
                yawOffset = 0;
                pitchOffset = 0;
                cameraPitch -= Input.GetAxis("Mouse Y") * cameraPitchSpeed;
                cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
                cameraYaw += Input.GetAxis("Mouse X") * cameraYawSpeed;
                cameraYaw = cameraYaw % 360.0f;
            }
        }
        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraDistanceSpeed;
            cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMin, cameraDistanceMax);
            lerpDistance = false;
        }

        if (IsFocusing())
        {
            Vector3 focusPosition = FocusTarget.transform.position;
            Vector3 diff = (focusPosition - Player.transform.position);
            Vector3 angles = Quaternion.LookRotation(diff).eulerAngles;
            if (angles.y != cameraYaw)
            {

                cameraYaw = angles.y;
            }
            if (angles.x + cameraYOffset != cameraPitch)
            {
                cameraPitch = angles.x + cameraYOffset;
            }

            MoveCameraTo(camera.transform, FocusTarget.transform, (cameraYaw + yawOffset) % 360, cameraPitch + pitchOffset, diff.magnitude);
            camera.transform.LookAt(focusPosition + (Vector3.up * cameraYOffset));
            return;
        }
        MoveCameraTo(camera.transform, Player.transform, cameraYaw, cameraPitch);
        camera.transform.LookAt(Player.transform.position + (Vector3.up * cameraYOffset));
    }

    bool IsFocusing()
    {
        return FocusTarget != null && Controller.Targeting.focusingTarget;
    }

    void MoveCameraTo(Transform camera, Transform target, float x, float y, float distanceOffset = 0f)
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
                float newCameraDistance = Mathf.Lerp(Vector3.Distance(target.position, camera.position), distanceOffset + cameraDistance, 5.0f * Time.deltaTime);
                newCameraPosition = target.position + (Quaternion.Euler(y, x, 0) * Vector3.back * newCameraDistance);
            }
        }

        camera.position = newCameraPosition;
    }

}