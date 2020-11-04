using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class TightenedUISceneGameplay : UISceneGameplay
    {

        protected override void Update()
        {
            if (GenericUtils.IsFocusInputField())
                return;

            UpdateToggles();
            base.Update();
        }
    }
}