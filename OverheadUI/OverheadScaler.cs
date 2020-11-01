using MultiplayerARPG;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace OverheadUI
{
    public class OverheadScaler : MonoBehaviour
    {
        public float scale = 0.015f;
        private Vector3 lossyScale;
        private float verticalOffset = 0.1f;

        void Awake()
        {
            lossyScale = transform.lossyScale;
        }

        void FixedUpdate()
        {
            float distanceFromCamera = Camera.main ? Vector3.Distance(Camera.main.transform.position, transform.position) / 10 : 1;
            Resize(distanceFromCamera < 0 ? 1 : distanceFromCamera);
        }
        void Resize(float distance)
        {
            transform.localScale = new Vector3(
                scale / lossyScale.x * distance,
                scale / lossyScale.y * distance,
                scale / lossyScale.z
            );
            transform.localPosition = new Vector3(0, verticalOffset, 0);
        }

    }
}
