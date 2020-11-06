using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {

        [DevExtMethods("Awake")]
        protected void DevExt_Awake()
        {
            if (!DefaultLocale.Texts.ContainsKey(CustomGameMessage.NoCastingInSafeArea.ToString()))
            {
                DefaultLocale.Texts.Add(CustomGameMessage.NoCastingInSafeArea.ToString(), "Unable to attack in the Safe Zone");
            }
        }
        protected virtual void ClearQueuedSkillIfInSafeZone()
        {
            if (queueUsingSkill.skill == null || queueUsingSkill.level <= 0)
                return;
            BaseSkill skill = queueUsingSkill.skill;

            if (PlayerCharacterEntity.IsInSafeArea && skill.IsAttack())
            {
                ClearQueueUsingSkill();
                PlayerCharacterEntity.CurrentGameManager.SendServerGameMessage(PlayerCharacterEntity.ConnectionId,
                    CustomGameMessage.NoCastingInSafeArea);
                return;
            }
        }
    }

}