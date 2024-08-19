using System;
using UnityEngine;

namespace Assets.Scripts
{
    [System.Serializable]
    public class VolumePrefab
    {
        public GameObject UnderlyingPrefab;
        public bool ShouldRotate;
        public bool AlreadyInstantiated;
        public float scaleMultiplier = 1.0f;
    }
}