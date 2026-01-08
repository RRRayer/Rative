using System.Collections;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class AirborneStatus : MonoBehaviour
    {
        public bool IsAirborne { get; private set; }

        public void Apply(float duration)
        {
            if (duration <= 0f)
            {
                IsAirborne = false;
                return;
            }

            StopAllCoroutines();
            StartCoroutine(AirborneRoutine(duration));
        }

        private IEnumerator AirborneRoutine(float duration)
        {
            IsAirborne = true;
            yield return new WaitForSeconds(duration);
            IsAirborne = false;
        }
    }
}
