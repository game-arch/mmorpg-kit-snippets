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

    [HideInInspector] public GameObject m_currentlySelectedTarget; //object your targeting
    private readonly List<GameObject> m_CandidateTargets = new List<GameObject>(); //list of candidate game objects
    PlayerCharacterController controller;
    new SphereCollider collider;

    public bool sortByDistance = true;

    private bool TryGetButtonDown(string name)
    {
        if (InputManager.HasInputSetting(name))
        {
            return InputManager.GetButtonDown(name);
        }
        return false;
    }
    private bool IsTargetButtonPressed()
    {
        return (
            Input.GetAxisRaw("TargetHorizontal") > 0.0f
            || Input.GetAxisRaw("TargetHorizontal") < 0.0f
            || Input.GetKeyDown(KeyCode.Tab)
            || TryGetButtonDown("Activate")
            );
    }
    private bool IsTargetNextPressed()
    {
        return (Input.GetAxisRaw("TargetHorizontal") > 0.0f || (Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift)));
    }
    private bool IsTargetPreviousPressed()
    {
        return (Input.GetAxisRaw("TargetHorizontal") < 0.0f || (Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift)));
    }

    private void Start()
    {
        gameObject.transform.localPosition = new Vector3(0, 0, 0);
        collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = 30f;
        collider.isTrigger = true;
        controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
    }

    private float GetSortWeight(GameObject go, bool subsequent = false)
    {
        Vector3 target_direction = go.transform.position - (subsequent ? Camera.main.transform.position : transform.position); //get vector from player to target
        var camera_forward = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z); //convert camera forward direction into 2D vector
        var target_dir = new Vector2(target_direction.x, target_direction.z); //do the same with target direction
        float angle = GetAngle(go, transform) - 90;
        if (angle < 0)
            angle = 1;

        if (subsequent)
        {
            return Vector2.SignedAngle(camera_forward, target_dir) * angle; //get the angle between the two vectors with higher being to the left and lower being to the right
        }
        return target_dir.magnitude * angle; //get the angle between the two vectors
    }

    private float GetAngle(GameObject go, Transform transForm = null)
    {
        Transform origin = transForm ?? Camera.main.transform;
        Vector3 target_direction = go.transform.position - origin.position; //get vector from player to target
        var camera_forward = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z); //convert camera forward direction into 2D vector
        var target_dir = new Vector2(target_direction.x, target_direction.z); //do the same with target direction
        return Vector2.Angle(camera_forward, target_dir); //get the angle between the two vectors with higher being to the left and lower being to the right
    }
    private void SelectNextTarget(List<GameObject> list, bool right = true)
    {
        bool hasValidTarget = m_currentlySelectedTarget?.activeInHierarchy == true;
        int index = hasValidTarget ? list.IndexOf(m_currentlySelectedTarget) : -1;
        index = index > -1 && index < list.Count ? index : 0;
        if (!right)
        {
            index = index + 1 < list.Count ? index + 1 : 0;
        }
        else
        {
            index = index - 1 > 0 ? index - 1 : list.Count - 1;
        }
        if (m_currentlySelectedTarget != null)
        {
            UnTarget(m_currentlySelectedTarget);
        }
        Target(list[index]);
        m_currentlySelectedTarget = list[index];
    }

    public void HandleTargeting()
    {
        if (controller == null)
        {
            controller = BasePlayerCharacterController.Singleton as PlayerCharacterController;
        }
        // remove null objects in the list and decrement the counter
        // Could optimize through some onDelete event system, probably really not worth
        for (int i = m_CandidateTargets.Count - 1; i >= 0; --i)
        {
            if (m_CandidateTargets[i] == null || !m_CandidateTargets[i].activeInHierarchy)
            {
                m_CandidateTargets.RemoveAt(i);
            }
        }
        if (controller.SelectedEntity?.gameObject != null && controller.SelectedEntity != null && Input.GetKeyDown(KeyCode.Escape))
        {
            UnTarget(controller.SelectedEntity.gameObject);
            m_currentlySelectedTarget = null;
            return;
        }
        //if I am targeting, there are candidate objects within my radius, and current target is not null and the object is alive aka in the scene
        if (IsTargetButtonPressed())
        {
            if (m_CandidateTargets.Count > 0)
            {
                // On initial target, or if you hit the "confirm" button, choose the closest mob
                if (m_currentlySelectedTarget != null && TryGetButtonDown("Activate"))
                {
                    controller.Activate();
                }
                if (m_currentlySelectedTarget == null)
                {
                    TargetInitially();
                }
                else if (!TryGetButtonDown("Activate"))
                {
                    TargetNextBasedOnDirection();
                }
            }
            else
            {
                Debug.Log("No Entities to Target");
            }

        }
    }

    private void TargetNextBasedOnDirection()
    {
        List<GameObject> objectsInView = SortObjectsInView(true);
        if (objectsInView.Count > 0)
        {
            SelectNextTarget(objectsInView, IsTargetNextPressed());
        }
    }

    private void TargetInitially()
    {
        List<GameObject> objectsInView = SortObjectsInView(false);

        if (objectsInView.Count > 0)
        {
            Target(objectsInView.First());
            m_currentlySelectedTarget = objectsInView.First();
        }
    }

    private List<GameObject> SortObjectsInView(bool subsequent = false)
    {
        List<GameObject> Sorted_List = m_CandidateTargets.OrderBy(go => GetSortWeight(go, subsequent)).ToList();
        List<GameObject> objectsInView = new List<GameObject>();
        for (var i = 0; i < Sorted_List.Count(); ++i)
        {
            if (GetAngle(Sorted_List[i]) < Camera.main.fieldOfView && m_CandidateTargets.IndexOf(Sorted_List[i]) != -1 && m_CandidateTargets[i].activeInHierarchy)
            {
                objectsInView.Add(Sorted_List[i]);
            }
        }

        return objectsInView;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MonsterTag" || other.tag == "HarvestableTag" || other.tag == "NpcTag")
        {
            if (other.gameObject.activeInHierarchy)
            {
                m_CandidateTargets.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (m_CandidateTargets.IndexOf(other.gameObject) != -1)
        {
            m_CandidateTargets.Remove(other.gameObject);
        }
    }

    #region Private

    private void Target(GameObject enemy)
    {
        if (enemy != null)
        {
            BaseGameEntity agent = enemy.GetComponentInParent<BaseGameEntity>();

            if (agent != null)
            {
                controller.HandleTargetChange(agent.transform);
                if (agent is BaseCharacterEntity)
                {
                    BaseCharacterEntity character = agent as BaseCharacterEntity;
                    character.OnTargeted(GetComponent<BaseCharacterEntity>());
                    character.CharacterDied.AddListener(AgentDies);
                }
            }
        }
    }

    private void UnTarget(GameObject enemy)
    {
        if (enemy != null)
        {
            var agent = enemy.GetComponentInParent<BaseGameEntity>();

            if (agent != null)
            {
                if (controller.SelectedEntity == agent)
                {
                    controller.HandleTargetChange(null);
                }
                if (agent is BaseCharacterEntity)
                {
                    BaseCharacterEntity character = agent as BaseCharacterEntity;
                    character.OnUnTargeted(GetComponent<BaseCharacterEntity>());
                    character.CharacterDied.RemoveListener(AgentDies);
                }
            }
        }
    }

    private void AgentDies(BaseGameEntity agent)
    {
        m_CandidateTargets.Remove(agent.gameObject.gameObject);
        UnTarget(agent.gameObject);
        m_currentlySelectedTarget = null;
    }

    #endregion
}