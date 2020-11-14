using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity : DamageableEntity, ICharacterData
    {
        public readonly UnityEvent<BaseCharacterEntity> CharacterDied = new UnityEvent<BaseCharacterEntity>();
        private readonly List<BaseCharacterEntity> m_targetedBy = new List<BaseCharacterEntity>(); //list of candidate game objects



        [DevExtMethods("Awake")]
        void DevExt_Awake()
        {

            onDead.AddListener(() => CharacterDied.Invoke(this));
        }

        [DevExtMethods("OnDestroy")]
        void DevExt_Destroy()
        {
            onDead = null;
            CharacterDied.RemoveAllListeners();
        }


        public void OnTargeted(BaseCharacterEntity entity)
        {
            m_targetedBy.Add(entity);
        }
        public void OnUnTargeted(BaseCharacterEntity entity)
        {
            m_targetedBy.Remove(entity);
        }
    }
}
