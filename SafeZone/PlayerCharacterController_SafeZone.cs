using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class PlayerCharacterController
    {


        protected void DevExt_Awake()
        {
            if (!DefaultLocale.Texts.ContainsKey(CustomGameMessage.NoCastingInSafeArea.ToString()))
            {
                DefaultLocale.Texts.Add(CustomGameMessage.NoCastingInSafeArea.ToString(), "You may not attack in the safe zone");
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