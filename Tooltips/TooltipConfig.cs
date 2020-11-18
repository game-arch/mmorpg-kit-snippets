using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG;

namespace Tooltips
{
    public class TooltipConfig : MonoBehaviour
    {

        public UICharacterItem uiItemDialog;
        public UICharacterSkill uiSkillDialog;

        public static TooltipConfig Singleton { get; protected set; }
        // Start is called before the first frame update
        void Start()
        {
            Singleton = this;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}