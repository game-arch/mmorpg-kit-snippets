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
    public static bool lerpOffset = false;

    protected float cameraPitchSpeed = 2.0f;
    protected float cameraYawSpeed = 2.0f;
    public float cameraPitchMin = -10.0f;
    public float cameraPitchMax = 80.0f;
    public float cameraDistanceSpeed = 5.0f;
    public float cameraDistanceMin = 2.0f;
    public float cameraDistanceMax = 12.0f;
    public float cameraYOffset = 2f;

    public string savePrefsPrefix = "GAMEPLAY";

    protected float yawOffset = 0f;
    protected float pitchOffset = 0f;
    protected float maxOffset = 40f;


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
        cameraYaw = PlayerPrefs.GetFloat(savePrefsPrefix + "_XRotation", cameraYaw);
        cameraPitch = PlayerPrefs.GetFloat(savePrefsPrefix + "_YRotation", cameraPitch);
        cameraDistance = PlayerPrefs.GetFloat(savePrefsPrefix + "_ZoomDistance", cameraDistance);
    }
    private void Update()
    {

        if (!Player)
            return;
        PlayerPrefs.SetFloat(savePrefsPrefix + "_XRotation", cameraYaw + yawOffset);
        PlayerPrefs.SetFloat(savePrefsPrefix + "_YRotation", cameraPitch + pitchOffset);
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
        if (Input.GetMouseButton(1))
            UpdateInput();
        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
            UpdateZoom();

        if (IsFocusing())
            FollowSelectedTarget();
        else
            FollowPlayer();
    }

    protected virtual void UpdateZoom()
    {
        cameraDistance -= Input.GetAxis("Mouse ScrollWheel") * cameraDistanceSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, cameraDistanceMin, cameraDistanceMax);
        lerpDistance = false;
    }

    protected virtual void FollowPlayer()
    {
        if (pitchOffset > -1)
            cameraPitch = cameraPitch + pitchOffset;
        MoveCameraTo(camera.transform, Player.transform, cameraYaw, cameraPitch);
        camera.transform.LookAt(Player.transform.position + (Vector3.up * cameraYOffset));
        yawOffset = 0;
        pitchOffset = -1;
    }

    protected virtual void FollowSelectedTarget()
    {
        Vector3 focusPosition = FocusTarget.transform.position;
        Vector3 diffOfCamera = Player.transform.position - camera.transform.position;
        Vector3 diffOfCameraFromNewTarget = focusPosition - camera.transform.position;
        Vector3 diff = focusPosition - Player.transform.position;
        Vector3 angles = Quaternion.LookRotation(diff).eulerAngles;
        float horizontal = angles.y;
        float vertical = angles.x;
        float angle = Vector3.Angle(diff, camera.transform.right);
        if (angle < 80 || angle > 100 || Input.GetMouseButton(1))
            cameraYaw = Mathf.LerpAngle(cameraYaw, horizontal, 2f * Time.deltaTime);

        cameraYaw = cameraYaw % 360;

        if (pitchOffset == -1)
            pitchOffset = (cameraPitch * diffOfCamera.magnitude) / diffOfCameraFromNewTarget.magnitude;
        pitchOffset = Mathf.Clamp(pitchOffset, 0, maxOffset);
        cameraPitch = vertical;
        if (lerpOffset)
        {
            yawOffset = Mathf.Lerp(yawOffset, 0, Time.deltaTime / 2);
            lerpOffset = false;
        }
        MoveCameraTo(camera.transform, FocusTarget.transform, (cameraYaw + yawOffset), cameraPitch + pitchOffset, diff.magnitude);
        camera.transform.LookAt(focusPosition + (Vector3.up * cameraYOffset));
    }

    protected virtual void UpdateInput()
    {
        if (IsFocusing())
        {
            pitchOffset = pitchOffset - Input.GetAxis("Mouse Y") * cameraPitchSpeed;
            yawOffset = yawOffset + Input.GetAxis("Mouse X") * cameraYawSpeed;
            lerpOffset = false;
            float radius = maxOffset / 2;
            Vector3 newLocation = new Vector3(yawOffset, pitchOffset);
            Vector3 centerPosition = new Vector3(0, 0); //center of *black circle*
            float distance = Vector3.Distance(newLocation, centerPosition); //distance from ~green object~ to *black circle*

            if (distance > radius)
            {
                Vector3 fromOriginToObject = newLocation - centerPosition;
                fromOriginToObject *= radius / distance;
                newLocation = centerPosition + fromOriginToObject;
                yawOffset = newLocation.x;
                pitchOffset = newLocation.y;

            }
            return;
        }
        cameraPitch -= Input.GetAxis("Mouse Y") * cameraPitchSpeed;
        cameraPitch = Mathf.Clamp(cameraPitch, cameraPitchMin, cameraPitchMax);
        cameraYaw += Input.GetAxis("Mouse X") * cameraYawSpeed;
        cameraYaw = cameraYaw % 360.0f;
    }

    protected virtual bool IsFocusing()
    {
        return FocusTarget != null && Controller.Targeting.focusingTarget;
    }

    protected virtual void MoveCameraTo(Transform camera, Transform target, float x, float y, float distanceOffset = 0f)
    {
        Vector3 newCameraPosition = target.position + (Quaternion.Euler(y, x, 0) * Vector3.back * (distanceOffset + cameraDistance));

        RaycastHit hitInfo;
        LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Building");
        if (Physics.Linecast(target.position, newCameraPosition, out hitInfo, mask.value))
        {
            newCameraPosition = hitInfo.point;
            lerpDistance = true;
        }
        else if (lerpDistance)
        {
            float newCameraDistance = Mathf.Lerp(Vector3.Distance(target.position, camera.position), distanceOffset + cameraDistance, 5.0f * Time.deltaTime);
            newCameraPosition = target.position + (Quaternion.Euler(y, x, 0) * Vector3.back * newCameraDistance);
        }

        camera.position = newCameraPosition;
    }

}