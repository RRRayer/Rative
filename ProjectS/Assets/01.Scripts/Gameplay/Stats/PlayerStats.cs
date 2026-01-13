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
            def = 0,
            spi = 0
        };

        [Header("Tuning")]
        [SerializeField] private float basePhysicalAttack = 100f;
        [SerializeField] private float physicalAttackPerStr = 5f;
        [SerializeField] private float baseSkillAttack = 100f;
        [SerializeField] private float skillAttackPerInt = 7f;
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float moveSpeedPerAgi = 0.05f;
        [SerializeField] private float baseHealth = 1000f;
        [SerializeField] private float healthPerVit = 150f;
        [SerializeField] private float baseCritChance = 0.05f;
        [SerializeField] private float critChancePerLuk = 0.005f;
        [SerializeField] private float critMultiplier = 2f;
        [SerializeField] private float damageReductionPerDef = 0.01f;
        [SerializeField] private float maxDamageReduction = 0.5f;
        public StatBlock BaseStats => baseStats;
        public float MaxHealth => Mathf.Max(1f, baseHealth + (baseStats.vit * healthPerVit));
        public float CritChance => Mathf.Clamp01(baseCritChance + baseStats.luk * critChancePerLuk);
        public float CritMultiplier => critMultiplier;
        public float PhysicalAttack => basePhysicalAttack + (baseStats.str * physicalAttackPerStr);
        public float SkillAttack => baseSkillAttack + (baseStats.intel * skillAttackPerInt);
        public float MoveSpeed => baseMoveSpeed + (baseStats.agi * moveSpeedPerAgi);
        public float DamageReductionPercent => Mathf.Clamp(GetDefenseValue() * damageReductionPerDef, 0f, maxDamageReduction);

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
            if (baseMoveSpeed <= 0f)
            {
                return 1f;
            }

            return MoveSpeed / baseMoveSpeed;
        }

        public float GetDamageMultiplier(SkillSlot slot)
        {
            return slot == SkillSlot.Basic ? PhysicalAttack : SkillAttack;
        }

        private int GetDefenseValue()
        {
            return baseStats.def > 0 ? baseStats.def : baseStats.spi;
        }
    }
}
