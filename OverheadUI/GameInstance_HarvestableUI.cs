using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class GameInstance : MonoBehaviour
    {
        [Header("Gameplay Objects")]
        [Tooltip("This UI will be instaniate as Harvestable's child to show the harvestable name")]
        public UIHarvestableEntity harvestableUI;
    }
}