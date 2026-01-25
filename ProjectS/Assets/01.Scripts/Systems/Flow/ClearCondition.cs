using UnityEngine;

namespace PS.Systems.Flow
{
    public abstract class ClearCondition : MonoBehaviour
    {
        public abstract bool IsComplete { get; }
    }
}
