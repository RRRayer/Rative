using UnityEngine;

namespace ProjectS.Systems.Flow
{
    public abstract class ClearCondition : MonoBehaviour
    {
        public abstract bool IsComplete { get; }
    }
}
