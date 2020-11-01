using LiteNetLibManager;
using MultiplayerARPG;
using UnityEngine;

namespace FlexibleSpawners
{
    public class FlexibleHarvestableSpawnArea : HarvestableSpawnArea
    {
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];

        public new Vector3 GetRandomPosition()
        {
            return SpawnerUtils.GetRandomPosition(transform, type, randomRadius, squareSizeX, squareSizeZ, squareGizmosHeight, findGroundRaycastHits, GroundLayerMask);
        }

        protected override void SpawnInternal()
        {
            float colliderDetectionRadius = asset.ColliderDetectionRadius;
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            bool overlapEntities = false;
            Collider[] overlaps = Physics.OverlapSphere(spawnPosition, colliderDetectionRadius);
            foreach (Collider overlap in overlaps)
            {
                if (OverlapsEntities(overlap))
                {
                    overlapEntities = true;
                    break;
                }
            }
            if (!overlapEntities)
            {
                GameObject spawnObj = Instantiate(asset.gameObject, spawnPosition, spawnRotation);
                HarvestableEntity entity = spawnObj.GetComponent<HarvestableEntity>();
                entity.gameObject.SetActive(false);
                // This is the change from GROUND_DETECTION_DISTANCE to squareGizmosHeight
                if (entity.FindGroundedPosition(spawnPosition, squareGizmosHeight, out spawnPosition))
                {
                    entity.SetSpawnArea(this, spawnPosition);
                    BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
                }
                else
                {
                    // Destroy the entity (because it can't find ground position)
                    Destroy(entity.gameObject);
                    ++pending;
                    Logging.LogWarning(ToString(), "Cannot spawn harvestable, it cannot find grounded position, pending harvestable amount " + pending);
                }
            }
            else
            {
                ++pending;
                Logging.LogWarning(ToString(), "Cannot spawn harvestable, it is collided to another entities, pending harvestable amount " + pending);
            }
        }

        private bool OverlapsEntities(Collider overlap)
        {
            return overlap.gameObject.layer == CurrentGameInstance.characterLayer ||
                                overlap.gameObject.layer == CurrentGameInstance.itemDropLayer ||
                                overlap.gameObject.layer == CurrentGameInstance.buildingLayer ||
                                overlap.gameObject.layer == CurrentGameInstance.harvestableLayer;
        }
    }
}