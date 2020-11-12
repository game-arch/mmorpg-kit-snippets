using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterBuilds
{
    public class BuildHotBarHotKey
    {
        public int hotkeyIndex;
        public string hotkeyId;
        public HotkeyType type;
    }
    public class BuildHotBar
    {
        public int hotBarIndex { get; set; }
        public List<BuildHotBarHotKey> hotKeys { get; set; }

        public BuildHotBar(int index = 1)
        {
            hotBarIndex = index;
            hotKeys = new List<BuildHotBarHotKey>();
        }
    }
}