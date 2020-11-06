using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        [SerializeField]
        protected SyncFieldUInt castingTargetEntityId = new SyncFieldUInt();
        [SerializeField]
        protected SyncFieldUInt subTargetEntityId = new SyncFieldUInt();
        #region Tab Target Entity Getter/Setter
        public void SetSubTarget(BaseGameEntity entity)
        {
            if (entity == null)
            {
                subTargetEntityId.Value = 0;
                return;
            }

            subTargetEntityId.Value = entity.ObjectId;
            subTargetEntityId.UpdateImmediately();
        }
        public void SetCastingTarget(BaseGameEntity entity)
        {
            if (entity == null)
            {
                castingTargetEntityId.Value = 0;
                return;
            }

            castingTargetEntityId.Value = entity.ObjectId;
            castingTargetEntityId.UpdateImmediately();
        }
        #endregion


        /**
         * Use GetTargetEntity/TryGetTargetEntity for NPC dialogue, Autoattack and harvesting
         */


        /**
         * Use this method when applying skills that were mid-cast when changing targets
         */
        public virtual BaseGameEntity GetCastingTargetEntity()
        {
            BaseGameEntity entity;
            if (castingTargetEntityId.Value == 0 || !Manager.Assets.TryGetSpawnedObject(castingTargetEntityId.Value, out entity))
                return null;
            return entity;
        }
        /**
         * Use this method for using skills/items on sub-targets
         */
        public virtual BaseGameEntity GetSubTargetEntity()
        {
            BaseGameEntity entity;
            if (subTargetEntityId.Value == 0 || !Manager.Assets.TryGetSpawnedObject(subTargetEntityId.Value, out entity))
                return null;
            return entity;
        }


        public virtual bool TryGetCastingTargetEntity<T>(out T entity) where T : class
        {
            if (GetCastingTargetEntity() == null)
                return TryGetSubTargetEntity<T>(out entity);
            entity = GetCastingTargetEntity() as T;
            return entity != null;
        }
        public virtual bool TryGetSubTargetEntity<T>(out T entity) where T : class
        {
            // Fallback to selected entity if sub target is not available
            if (GetSubTargetEntity() == null)
                return TryGetTargetEntity<T>(out entity);
            Debug.Log("Getting Sub Target");
            entity = GetSubTargetEntity() as T;
            return entity != null;
        }
        public Vector3 GetDefaultAttackAimPositionForSkill(DamageInfo damageInfo, bool isLeftHand)
        {
            // No aim position set, set aim position to forward direction
            BaseGameEntity targetEntity;
            TryGetCastingTargetEntity(out targetEntity);
            if (targetEntity)
            {
                if (targetEntity is DamageableEntity)
                {
                    return (targetEntity as DamageableEntity).OpponentAimTransform.position;
                }
                else
                {
                    return targetEntity.CacheTransform.position;
                }
            }
            return damageInfo.GetDamageTransform(this, isLeftHand).position + CacheTransform.forward * damageInfo.GetDistance();
        }
    }
}