using System;
using System.Collections;
using System.Collections.Generic;
using MultiplayerARPG;
using UnityEngine;

namespace CharacterBuilds
{
    [Serializable]
    public class CharacterBuilds
    {
        public int playerEntityId;

        public int currentBuild = 0;
        public Build[] builds;


    }
}
