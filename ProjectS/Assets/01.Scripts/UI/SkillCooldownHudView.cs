using ProjectS.Core.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    public class SkillCooldownHudView : MonoBehaviour
    {
        private const string SkillCooldownPrefabName = "SkillCooldownHudUI";

        [SerializeField] private Image qCooldownMask;
        [SerializeField] private Image eCooldownMask;
        [SerializeField] private Image rCooldownMask;

        private ICooldownProvider cooldownProvider;
        private SlotView[] slots;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<SkillCooldownHudView>() != null)
            {
                return;
            }

            GameObject prefab = Resources.Load<GameObject>(SkillCooldownPrefabName);
            if (prefab == null)
            {
                Debug.LogError($"SkillCooldownHudView prefab not found. Create prefab in Resources/{SkillCooldownPrefabName}.");
                return;
            }

            Instantiate(prefab);
        }

        private void Awake()
        {
            slots = new[]
            {
                new SlotView { Slot = SkillSlot.Q, Mask = qCooldownMask },
                new SlotView { Slot = SkillSlot.E, Mask = eCooldownMask },
                new SlotView { Slot = SkillSlot.R, Mask = rCooldownMask }
            };
        }

        private void Update()
        {
            if (cooldownProvider == null)
            {
                cooldownProvider = FindLocalCooldownProvider();
            }

            if (cooldownProvider == null || slots == null)
            {
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                SlotView slot = slots[i];
                float duration = cooldownProvider.GetCooldownDuration(slot.Slot);
                float remaining = cooldownProvider.GetCooldownRemaining(slot.Slot);
                float ratio = duration > 0f ? Mathf.Clamp01(remaining / duration) : 0f;
                slot.SetCooldown(ratio);
            }
        }

        private ICooldownProvider FindLocalCooldownProvider()
        {
            Photon.Pun.PhotonView[] views = FindObjectsOfType<Photon.Pun.PhotonView>();
            for (int i = 0; i < views.Length; i++)
            {
                if (!views[i].IsMine)
                {
                    continue;
                }

                ICooldownProvider provider = views[i].GetComponentInParent<ICooldownProvider>();
                if (provider != null)
                {
                    return provider;
                }
            }

            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is ICooldownProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }

        private struct SlotView
        {
            public SkillSlot Slot;
            public Image Mask;

            public void SetCooldown(float ratio)
            {
                if (Mask != null)
                {
                    Mask.fillAmount = ratio;
                }
            }
        }
    }
}
