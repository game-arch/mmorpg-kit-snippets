using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MultiplayerARPG;

namespace CharacterBuilds
{
    public class UICharacterBuild : UIBase
    {

        public UnityEvent<int> OnSet = new UnityEvent<int>();
        public UnityEvent<int> OnUpdate = new UnityEvent<int>();
        public UnityEvent<int> OnDelete = new UnityEvent<int>();
        public int index;

        public TextWrapper uiBuildNumber;
        public TextWrapper uiBuildName;


        void OnEnable()
        {
            UpdateData();
        }


        public void UpdateData()
        {
            if (CharacterBuildController.builds?.builds.Length > index)
            {
                Build build = CharacterBuildController.builds.builds[index];
                if (uiBuildNumber)
                    uiBuildNumber.text = (index + 1) + "";
                if (uiBuildName)
                    uiBuildName.text = build.label;
            }
        }

        public void OnClickSet()
        {
            OnSet.Invoke(index);
        }
        public void OnClickUpdate()
        {
            OnUpdate.Invoke(index);
        }

        public void OnClickDelete()
        {
            OnDelete.Invoke(index);
        }
    }
}