using System;
namespace CharacterBuilds
{
    [Serializable]
    public class BuildEquipItem
    {
        public string slot;
        public int equipSet;
        public string id;

        public BuildEquipItem(string slot, string id, int equipSet = 0)
        {
            this.slot = slot;
            this.id = id;
            this.equipSet = equipSet;
        }
    }
}