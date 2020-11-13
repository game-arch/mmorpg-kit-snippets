using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterBuilds
{
    [Serializable]
    public class Build
    {

        public string label;
        public int playerEntityId;
        public BuildEquipItem[] equipment;
        public BuildHotKey[] hotKeys;

        public static Build Create()
        {
            Build build = new Build();
            return build;
        }

    }
}
