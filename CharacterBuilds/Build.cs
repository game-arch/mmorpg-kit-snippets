using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterBuilds
{
    public class Build
    {
        public int buildIndex { get; set; }
        public int playerEntityId { get; set; }
        public Dictionary<int, BuildHotBar> hotBars { get; set; }
        public Dictionary<string, BuildGearItem> gear { get; set; }

        public Build()
        {
            hotBars = new Dictionary<int, BuildHotBar>();
            hotBars.Add(1, new BuildHotBar(1));
            gear = new Dictionary<string, BuildGearItem>();
        }
    }
}