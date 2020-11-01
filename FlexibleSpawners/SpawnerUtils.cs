using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiplayerARPG;

namespace FlexibleSpawners
{
    public static class SpawnerUtils
    {
        public static Vector3 GetRandomPosition(Transform transform, GameAreaType type, float randomRadius, float squareWidth, float squareDepth, float squareHeight, RaycastHit[] findGroundRaycastHits, int groundLayerMask)
        {
            Vector3 randomedPosition = transform.position;

            switch (type)
            {
                case GameAreaType.Radius:
                    randomedPosition += new Vector3(Random.Range(-1f, 1f) * randomRadius, 0, Random.Range(-1f, 1f) * randomRadius);
                    break;
                case GameAreaType.Square:
                    randomedPosition += new Vector3(Random.Range(-0.5f, 0.5f) * squareWidth, 0, Random.Range(-0.5f, 0.5f) * squareDepth);
                    break;
            }
            return PhysicUtils.FindGroundedPosition(randomedPosition, findGroundRaycastHits, squareHeight, groundLayerMask);
        }
    }
}