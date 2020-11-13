using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterBuilds {
    
    [Serializable]
    public class BuildHotKey {
        public string hotkeyId;
        public string relateId;
        public HotkeyType type;

        public BuildHotKey(string hotkeyId, HotkeyType type, string relateId) {
            this.hotkeyId = hotkeyId;
            this.type = type;
            this.relateId = relateId;
        }
    }
}