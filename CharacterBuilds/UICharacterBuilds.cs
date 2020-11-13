using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MultiplayerARPG;
using System;

namespace CharacterBuilds
{
    public class UICharacterBuilds : UIBase
    {
        protected List<UICharacterBuild> uiBuilds;
        public PlayerCharacterEntity Player
        {
            get
            {
                return BasePlayerCharacterController.OwningCharacter as PlayerCharacterEntity;
            }
        }

        public CharacterBuilds Builds
        {
            get
            {
                return CharacterBuildController.builds;
            }
        }
        public UICharacterBuild buildPrefab;
        public GameObject buildsContainer;
        public InputField input;

        void Start()
        {
            uiBuilds = new List<UICharacterBuild>();
        }

        void OnEnable()
        {
            UpdateData();
            CharacterBuildController.OnLoad.AddListener(UpdateData);
        }

        void OnDisable()
        {
            CharacterBuildController.OnLoad.RemoveListener(UpdateData);
        }

        public void OnCreateBuild()
        {
            string label = input.text;
            if (label?.Length > 2)
            {
                Build build = CharacterBuildController.CreateBuild();
                build.label = label + "";
                Build[] dest = new Build[Builds.builds.Length + 1];
                Array.Copy(Builds.builds, 0, dest, 0, Builds.builds.Length);
                dest[Builds.builds.Length] = build;
                Builds.builds = dest;
                CharacterBuildController.SaveBuilds();
            }
        }
        public void OnDeleteBuild(int index)
        {

            if (Builds.builds.Length > index)
            {
                Build[] dest = new Build[Builds.builds.Length - 1];
                if (index > 0)
                    Array.Copy(Builds.builds, 0, dest, 0, index);

                if (index < Builds.builds.Length - 1)
                    Array.Copy(Builds.builds, index + 1, dest, index, Builds.builds.Length - index - 1);

                Builds.builds = dest;
                CharacterBuildController.SaveBuilds();
            }
        }

        public void OnSelectBuild(int index)
        {
            if (Builds.builds.Length > index)
            {
                CharacterBuildController.ApplyBuild(index);
            }
        }

        public void OnUpdateBuild(int index)
        {
            if (Builds.builds.Length > index)
            {
                Build build = CharacterBuildController.CreateBuild();
                Build existing = Builds.builds[index];
                existing.equipment = build.equipment;
                existing.hotKeys = build.hotKeys;
                CharacterBuildController.SaveBuilds();
            }
        }

        public void UpdateData()
        {
            if (Builds == null) return;
            buildsContainer.RemoveObjectsByComponentInChildren<UICharacterBuild>(true);
            for (int i = 0; i < Builds.builds.Length; i++)
            {
                Build build = Builds.builds[i];
                UICharacterBuild uiBuild = Instantiate(buildPrefab, buildsContainer.transform);
                uiBuild.index = i;
                uiBuild.OnSet.AddListener(OnSelectBuild);
                uiBuild.OnUpdate.AddListener(OnUpdateBuild);
                uiBuild.OnDelete.AddListener(OnDeleteBuild);
                uiBuild.UpdateData();

            }
        }

    }
}