/*******************************************************************/
/* \project: Spicy Dice
 * \author: Amir Azmi
 * \date: 11/13/2019
 * \brief: Locks onto the closest object when a key F is pressed and
 *         unlocks when F is pressed again, if the ai gets out of the 
 *         range of the camera collider, then targeting is broken free,
 *         if the camera gets out of range, then it also breaks free
 *         You can also cycle targets with the left and right arrow
 *         key within the Camera.main.fieldOfView range
 *         
*/
/*******************************************************************/

using System.Linq; //Sorting for Orderby
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG;

public class TabTargeting : MonoBehaviour
{

    protected GameObject selectedTarget; //object your targeting
    protected GameObject potentialTarget;
    public GameObject castingTarget;

    protected readonly List<GameObject> m_CandidateTargets = new List<GameObject>(); //list of candidate game objects
    PlayerCharacterController controller;
    new SphereCollider collider;

    public bool sortByDistance = true;

    protected GameObject targetRecticle;
    protected float recticleMoveTime = 0f;
    protected float recticleFinishTime = 0.5f;
    protected bool verticalTargetingInUse;
    protected bool horizontalTargetingInUse;



    public bool IsTargeting
    {
        get
        {
            return selectedTarget != null || potentialTarget != null;
        }
    }
    public GameObject SelectedTarget
    {
        get
        {
            return selectedTarget;
        }
    }
    public GameObject PotentialTarget
    {
        get
        {
            return potentialTarget;
        }
    }
    public GameObject CastingTarget
    {
        get
        {
            return castingTarget;
        }
        set
        {
            castingTarget = value;
        }
    }

    protected bool targeting;


    protected BasePlayerCharacterEntity playerEntity;

    public BasePlayerCharacterEntity PlayerEntity
    {
        get
        {
            if (playerEntity == null)
                playerEntity = BasePlayerCharacterController.Singleton.PlayerCharacterEntity;
            return playerEntity;
        }
    }

    public PlayerCharacterController Controller
    {
        get
        {
            if (controller == null)
                controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
            return controller;
        }
    }

    public void UpdateTargeting()
    {
        if (castingTarget && !PlayerEntity.IsAttackingOrUsingSkill)
            castingTarget = null;
    }


    void LateUpdate()
    {
        if (targetRecticle)
        {
            if (potentialTarget != null)
            {
                targeting = true;
                recticleMoveTime += Time.deltaTime;
                if (!targetRecticle.transform.GetChild(0).gameObject.activeSelf)
                    targetRecticle.transform.GetChild(0).gameObject.SetActive(true);
                targetRecticle.transform.position = Vector3.Lerp(targetRecticle.transform.position, GetCenter(potentialTarget), recticleMoveTime / recticleFinishTime);
            }
            else
            {
                if (targetRecticle.transform.GetChild(0).gameObject.activeSelf)
                    targetRecticle.transform.GetChild(0).gameObject.SetActive(false);
                if (selectedTarget == null && potentialTarget == null)
                    targetRecticle.transform.position = GetCenter(BasePlayerCharacterController.OwningCharacter.gameObject);
            }
        }
    }

    protected virtual bool TryGetButtonDown(string name)
    {
        if (InputManager.HasInputSetting(name))
        {
            return InputManager.GetButtonDown(name);
        }
        return false;
    }
    protected virtual bool IsTargetButtonPressed()
    {
        return Input.GetKeyDown(KeyCode.Tab) || TryGetButtonDown("Activate");
    }

    protected virtual bool IsTargetNextPressed()
    {
        return (Input.GetAxisRaw("TargetHorizontal") > 0.0f || (Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift)));
    }
    protected virtual bool IsTargetPreviousPressed()
    {
        return (Input.GetAxisRaw("TargetHorizontal") < 0.0f || (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift)));
    }

    protected virtual void Start()
    {

        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = Mathf.Max(BasePlayerCharacterController.OwningCharacter.GetAttackDistance(false), Controller.TargetDistance);
        collider.isTrigger = true;
        targetRecticle = GameObject.Find("Recticle");
        targetRecticle = targetRecticle != null ? targetRecticle : Instantiate(Controller.recticle, new Vector3(0, 0, 0), Quaternion.identity);
        targetRecticle.name = "Recticle";
    }

    protected virtual float GetDistanceWeight(GameObject go)
    {
        float distance = (go.transform.position - transform.position).magnitude;
        if (distance > 40)
            return distance * 16;
        if (distance > 20)
            return distance * 8;
        if (distance > 10)
            return distance * 4;
        return distance;
    }

    protected virtual float GetAngleWeight(GameObject go)
    {

        float angle = GetAngle(go);
        if (angle > 120)
            return angle * 48;
        if (angle > 90)
            return angle * 32;
        if (angle > Camera.main.fieldOfView)
            return angle * 16;
        return angle;
    }

    protected virtual float GetSortWeight(GameObject go, bool cycling = false)
    {
        if (cycling)
        {
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(go.transform.position);
            return screenPosition.x; //get the angle between the two vectors with higher being to the left and lower being to the right
        }
        return GetDistanceWeight(go) + GetAngleWeight(go); //get the angle between the two vectors
    }

    protected virtual float GetAngle(GameObject go, Transform transForm = null)
    {
        Transform origin = transForm != null ? transForm : Camera.main.transform;
        Vector3 target_direction = go.transform.position - origin.position; //get vector from player to target
        var camera_forward = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z); //convert camera forward direction into 2D vector
        var target_dir = new Vector2(target_direction.x, target_direction.z); //do the same with target direction
        return Vector2.Angle(camera_forward, target_dir); //get the angle between the two vectors with higher being to the left and lower being to the right
    }
    protected virtual void PickNextTarget(List<GameObject> list, bool right = true)
    {
        GameObject targetToCheck = (potentialTarget != null ? potentialTarget : selectedTarget);
        bool hasValidTarget = targetToCheck ? targetToCheck?.activeInHierarchy == true : false;
        int index = hasValidTarget ? list.IndexOf(targetToCheck) : -1;
        index = index > -1 && index < list.Count ? index : 0;
        if (right)
        {
            index = index + 1 < list.Count ? index + 1 : 0;
        }
        else
        {
            index = index - 1 >= 0 ? index - 1 : list.Count - 1;
        }
        HighlightPotentialTarget(list[index]);
    }

    public void HandleTargeting()
    {
        // remove null objects in the list and decrement the counter
        // Could optimize through some onDelete event system, probably really not worth
        for (int i = m_CandidateTargets.Count - 1; i >= 0; --i)
        {
            if (m_CandidateTargets[i] == null || !m_CandidateTargets[i].activeInHierarchy)
            {
                m_CandidateTargets.RemoveAt(i);
            }
        }
        if ((selectedTarget != null || potentialTarget != null) && Input.GetKeyUp(KeyCode.Escape) && !Controller.uisOpen)
        {
            if (potentialTarget != null)
                UnHighlightPotentialTarget();
            else
                UnTarget(selectedTarget);
            return;
        }

        if (TryGetButtonDown("Activate"))
        {
            if (m_CandidateTargets.Count > 0)
            {
                if (potentialTarget != null)
                    Target(potentialTarget);
                else if (selectedTarget == null)
                    TargetClosest();
                else
                    Controller.Activate();
            }
            return;
        }
        if (Input.GetAxisRaw("TargetHorizontal") != 0.0f)
        {
            if (!horizontalTargetingInUse)
            {
                horizontalTargetingInUse = true;
                if (m_CandidateTargets.Count > 0)
                {
                    Debug.Log("Target Next!");
                    if (potentialTarget == null && selectedTarget == null)
                        TargetClosest(true);
                    else
                        TargetNextBasedOnDirection();
                }
            }
            return;
        }
        else
            horizontalTargetingInUse = false;


        if (Input.GetAxisRaw("TargetVertical") != 0.0f)
        {
            if (!verticalTargetingInUse)
            {
                verticalTargetingInUse = true;
                Debug.Log("Target Party!");
                if (Controller.PlayerCharacterEntity.PartyId > 0)
                {
                    PartyData tempPartyData;
                    Controller.PlayerCharacterEntity.CurrentGameManager.TryGetParty(Controller.PlayerCharacterEntity.PartyId, out tempPartyData);
                    Debug.Log(tempPartyData);
                }
                return;
            }
        }
        else
            verticalTargetingInUse = false;

        if (IsTargetButtonPressed())
        {
            if (m_CandidateTargets.Count > 0)
            {
                if (potentialTarget == null && selectedTarget == null)
                    TargetClosest(true);
                else
                    TargetNextBasedOnDirection();
            }
        }
    }

    protected virtual void TargetNextBasedOnDirection()
    {
        List<GameObject> objectsInView = SortObjectsInView(true);
        if (objectsInView.Count > 0)
        {
            PickNextTarget(objectsInView, IsTargetNextPressed());
        }
    }

    protected virtual void TargetClosest(bool potential = false)
    {
        List<GameObject> objectsInView = SortObjectsInView(false);

        if (objectsInView.Count > 0)
        {
            if (potential)
            {
                HighlightPotentialTarget(objectsInView.First());
            }
            else
            {
                Target(objectsInView.First());
                selectedTarget = objectsInView.First();
            }
        }
    }

    protected Vector3 GetCenter(GameObject go)
    {
        CapsuleCollider obj = go.GetComponentInChildren<CapsuleCollider>();
        Debug.DrawLine(Controller.CacheGameplayCamera.transform.position, obj.bounds.center, Color.yellow);
        return obj.bounds.center;
    }

    protected virtual List<GameObject> SortObjectsInView(bool cycling = false)
    {
        List<GameObject> Sorted_List = m_CandidateTargets.OrderBy(go => GetSortWeight(go, cycling)).ToList();
        List<GameObject> objectsInView = new List<GameObject>();

        for (var i = 0; i < Sorted_List.Count(); ++i)
        {
            IDamageableEntity entity = Sorted_List[i].GetComponent<IDamageableEntity>();
            if (entity != null && entity.IsHideOrDead())
                continue;
            Vector2 screenPosition = Camera.main.WorldToScreenPoint(Sorted_List[i].transform.position);
            // Entity is within the screen (no targeting offscreen)
            if (screenPosition.x > 0 && screenPosition.x < Screen.width && screenPosition.y > 0 && screenPosition.y < Screen.height && Sorted_List[i].activeInHierarchy)
            {
                // MAKE SURE ALL NON-COLLIDING ENTITIES ARE OFF OF THE DEFAULT LAYER PLEASE
                // If this does not work, change the layer for playercharactercontroller to Player or something
                LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Building");
                RaycastHit hit;
                bool didHit = Physics.Linecast(Controller.CacheGameplayCamera.transform.position, GetCenter(Sorted_List[i]), out hit, mask);
                if (!didHit)
                {
                    objectsInView.Add(Sorted_List[i]);
                }
                else
                {
                    Debug.Log("It hit " + hit.transform.name);
                }
            }
        }

        return objectsInView;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MonsterTag" || other.tag == "HarvestableTag" || other.tag == "NpcTag")
        {
            if (other.gameObject.activeInHierarchy)
            {
                m_CandidateTargets.Add(other.gameObject);
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (m_CandidateTargets.IndexOf(other.gameObject) != -1)
        {
            m_CandidateTargets.Remove(other.gameObject);
            UnTarget(other.gameObject);
        }
    }

    public virtual void HighlightPotentialTarget(GameObject enemy)
    {
        recticleMoveTime = 0;
        if (enemy != null)
            potentialTarget = enemy;
    }
    public virtual void UnHighlightPotentialTarget()
    {
        recticleMoveTime = 0;
        if (potentialTarget != null)
            potentialTarget = null;
    }


    public virtual void Target(GameObject enemy)
    {
        if (enemy != null)
        {
            BaseGameEntity agent = enemy.GetComponentInParent<BaseGameEntity>();

            if (agent != null)
            {
                if (selectedTarget != null && selectedTarget == enemy)
                    UnTarget(selectedTarget);
                selectedTarget = enemy;
                if (potentialTarget == selectedTarget)
                    potentialTarget = null;
                if (agent is BaseCharacterEntity)
                {
                    recticleMoveTime = 0;
                    BaseCharacterEntity character = agent as BaseCharacterEntity;
                    character.OnTargeted(GetComponent<BaseCharacterEntity>());
                    character.CharacterDied.AddListener(AgentDies);
                }
            }
        }
    }

    public virtual void UnTarget(GameObject enemy, bool deselect = true)
    {
        if (enemy != null)
        {
            var agent = enemy.GetComponentInParent<BaseGameEntity>();

            if (agent != null)
            {
                if (agent is BaseCharacterEntity)
                {
                    BaseCharacterEntity character = agent as BaseCharacterEntity;
                    character.OnUnTargeted(GetComponent<BaseCharacterEntity>());
                    character.CharacterDied.RemoveListener(AgentDies);
                }
                if (selectedTarget == enemy)
                {
                    if (deselect)
                    {
                        selectedTarget = null;
                        targeting = false;
                    }
                    recticleMoveTime = 0;
                }
            }
        }
    }

    protected virtual void AgentDies(BaseGameEntity agent)
    {
        m_CandidateTargets.Remove(agent.gameObject.gameObject);
        if (castingTarget == agent.gameObject)
            castingTarget = null;
        if (potentialTarget == agent.gameObject)
            UnHighlightPotentialTarget();
        UnTarget(agent.gameObject);
    }


}