using LiteNetLibManager;
using MultiplayerARPG;
using UnityEngine;

namespace FlexibleSpawners
{
    public class FlexibleMonsterSpawnArea : MonsterSpawnArea
    {
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];

        public new Vector3 GetRandomPosition()
        {
            return SpawnerUtils.GetRandomPosition(transform, type, randomRadius, squareSizeX, squareSizeZ, squareGizmosHeight, findGroundRaycastHits, GroundLayerMask);
        }
        protected override void SpawnInternal()
        {
            Vector3 spawnPosition = GetRandomPosition();
            Quaternion spawnRotation = GetRandomRotation();
            GameObject spawnObj = Instantiate(asset.gameObject, spawnPosition, spawnRotation);
            BaseMonsterCharacterEntity entity = spawnObj.GetComponent<BaseMonsterCharacterEntity>();
            entity.gameObject.SetActive(false);
            // This is the change from GROUND_DETECTION_DISTANCE to squareGizmosHeight
            if (FindGroundedPosition(entity, spawnPosition, out spawnPosition))
            {
                entity.Level = level;
                entity.SetSpawnArea(this, spawnPosition);
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            else
            {
                // Destroy the entity (because it can't find ground position)
                Destroy(entity.gameObject);
                ++pending;
                Logging.LogWarning(ToString(), "Cannot spawn monster, it cannot find grounded position, pending monster amount " + pending);
            }
        }
        protected virtual bool FindGroundedPosition(BaseMonsterCharacterEntity entity, Vector3 spawnPosition, out Vector3 result)
        {
            return entity.FindGroundedPosition(spawnPosition, squareGizmosHeight, out result);
        }
    }
}