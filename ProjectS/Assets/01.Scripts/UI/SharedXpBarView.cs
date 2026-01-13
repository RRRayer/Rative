using ProjectS.Networking;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    public class SharedXpBarView : MonoBehaviour
    {
        private const string SharedXpBarPrefabName = "SharedXpBarUI";

        [SerializeField] private Slider slider;
        [SerializeField] private Text levelText;
        private SharedProgressionManager progression;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<SharedXpBarView>() == null)
            {
                GameObject prefab = Resources.Load<GameObject>(SharedXpBarPrefabName);
                if (prefab == null)
                {
                    Debug.LogError($"SharedXpBarView prefab not found. Create prefab in Resources/{SharedXpBarPrefabName}.");
                    return;
                }

                Instantiate(prefab);
            }
        }

        private void OnEnable()
        {
            if (slider == null)
            {
                slider = GetComponentInChildren<Slider>(true);
            }

            if (levelText == null)
            {
                levelText = GetComponentInChildren<Text>(true);
            }

            progression = SharedProgressionManager.Instance;
            if (progression != null)
            {
                progression.ProgressionChanged += OnProgressionChanged;
                OnProgressionChanged(progression.Level, progression.CurrentXp, progression.XpToNext);
            }
        }

        private void OnDisable()
        {
            if (progression != null)
            {
                progression.ProgressionChanged -= OnProgressionChanged;
            }
        }

        private void OnProgressionChanged(int level, float currentXp, float xpToNext)
        {
            if (slider == null)
            {
                Debug.LogWarning("SharedXpBarView slider not assigned.", this);
                return;
            }

            slider.maxValue = Mathf.Max(1f, xpToNext);
            slider.value = Mathf.Clamp(currentXp, 0f, slider.maxValue);

            if (levelText != null)
            {
                levelText.text = $"Lv {level}";
            }
        }
    }
}
