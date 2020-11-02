using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG;
using System;
using System.Linq;

namespace TargetSorting
{
    public class TargetSort : MonoBehaviour
    {
        public NearbyEntityDetector detector;
        public List<BaseCharacterEntity> sorted;
        public BaseCharacterEntity targeted;

        void FixedUpdate()
        {
            if (detector)
            {
                sorted = detector.characters.ToList();
                for (int i = sorted.Count - 1; i >= 0; i--)
                {
                    if (!CheckLineOfSight(sorted[i]))
                    {
                        sorted.RemoveAt(i);
                    }
                }
                sorted.Sort((BaseCharacterEntity x, BaseCharacterEntity y) =>
                 {
                     return (y.transform.position - transform.position).magnitude.CompareTo((x.transform.position - transform.position).magnitude);
                 });
            }
        }
        private bool IsTargetVisible(BaseCharacterEntity target)
        {
            RaycastHit ObstacleHit;
            if (target)
            {   // make sure we have an objective first or we get a dirty error. 
                bool result = (Physics.Raycast(Camera.main.transform.position, target.transform.position - Camera.main.transform.position, out ObstacleHit, Mathf.Infinity) && ObstacleHit.transform != transform && ObstacleHit.transform == target.transform);
                if (ObstacleHit.transform)
                {
                    Debug.DrawLine(Camera.main.transform.position, ObstacleHit.transform.position, IndexOf(target) == 0 ? Color.yellow : Color.blue);
                }
                return result;
            }
            else
                return false;
        }
        private bool CheckLineOfSight(BaseCharacterEntity target)
        {

            float angle = Vector3.Angle(target.transform.position - Camera.main.transform.position, Camera.main.transform.forward);

            if (angle <= Camera.main.fieldOfView * 0.9f)
            {
                return IsTargetVisible(target);
            }
            return false;
        }
        public int IndexOf(BaseCharacterEntity value)
        {
            int index = 0;
            foreach (BaseCharacterEntity item in sorted)
            {
                if (item == null)
                {
                    index++;
                    continue;
                }
                if (item.Entity.ObjectId == value.Entity.ObjectId) return index;
                index++;
            }
            return -1;
        }
    }
}
