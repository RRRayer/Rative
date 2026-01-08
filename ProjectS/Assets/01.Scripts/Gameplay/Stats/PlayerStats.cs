using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Stats
{
    public sealed class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private StatBlock baseStats;
        [SerializeField] private StatBlock defaultStats = new StatBlock
        {
            str = 5,
            intel = 5,
            luk = 5,
            agi = 5,
            vit = 5,
            spi = 5
        };

        [Header("Tuning")]
        [SerializeField] private float baseCritChance = 0f;
        [SerializeField] private float critChancePerLuk = 0.01f;
        [SerializeField] private float critMultiplier = 1.5f;
        [SerializeField] private float basicDamagePerStr = 0.05f;
        [SerializeField] private float skillDamagePerInt = 0.05f;
        [SerializeField] private float moveSpeedPerAgi = 0.05f;
        [SerializeField] private float healthPerVit = 10f;
        public StatBlock BaseStats => baseStats;
        public float MaxHealth => Mathf.Max(1f, baseStats.vit * healthPerVit);
        public float CritChance => Mathf.Clamp01(baseCritChance + baseStats.luk * critChancePerLuk);
        public float CritMultiplier => critMultiplier;

        private void Awake()
        {
            if (baseStats.IsEmpty)
            {
                baseStats = defaultStats;
            }
        }

        public void SetBaseStats(StatBlock stats)
        {
            baseStats = stats.IsEmpty ? defaultStats : stats;
        }

        public float GetMoveSpeedMultiplier()
        {
            return 1f + baseStats.agi * moveSpeedPerAgi;
        }

        public float GetDamageMultiplier(SkillSlot slot)
        {
            if (slot == SkillSlot.Basic)
            {
                return 1f + (baseStats.str * basicDamagePerStr);
            }

            return 1f + (baseStats.intel * skillDamagePerInt);
        }
    }
}
