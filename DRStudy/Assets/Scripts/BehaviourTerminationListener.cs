using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    public abstract class BehaviourTerminationListener : MonoBehaviour
    {
        public abstract void BehaviourExecuted(MonoBehaviour originator);
    }
}