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
        [SerializeField] private float staminaPerSpi = 10f;
        [SerializeField] private float staminaRegenPerSecond = 8f;
        [SerializeField] private float basicAttackStaminaCost = 5f;
        [SerializeField] private float sprintStaminaCostPerSecond = 8f;

        public StatBlock BaseStats => baseStats;
        public float MaxHealth => Mathf.Max(1f, baseStats.vit * healthPerVit);
        public float MaxStamina => Mathf.Max(0f, baseStats.spi * staminaPerSpi);
        public float CurrentStamina { get; private set; }
        public float CritChance => Mathf.Clamp01(baseCritChance + baseStats.luk * critChancePerLuk);
        public float CritMultiplier => critMultiplier;
        public float BasicAttackStaminaCost => basicAttackStaminaCost;
        public float SprintStaminaCostPerSecond => sprintStaminaCostPerSecond;

        private void Awake()
        {
            if (baseStats.IsEmpty)
            {
                baseStats = defaultStats;
            }
            ResetStaminaFull();
        }

        public void SetBaseStats(StatBlock stats)
        {
            baseStats = stats.IsEmpty ? defaultStats : stats;
            ResetStaminaFull();
        }

        public void ResetStaminaFull()
        {
            CurrentStamina = MaxStamina;
        }

        public float GetMoveSpeedMultiplier()
        {
            return 1f + baseStats.agi * moveSpeedPerAgi;
        }

        public float GetDamageMultiplier(SkillSlot slot)
        {
            if (slot == SkillSlot.Basic)
            {
                float staminaRatio = MaxStamina > 0f ? CurrentStamina / MaxStamina : 1f;
                float staminaBonus = Mathf.Lerp(0.5f, 1f, staminaRatio);
                return 1f + (baseStats.str * basicDamagePerStr * staminaBonus);
            }

            return 1f + (baseStats.intel * skillDamagePerInt);
        }

        public bool TryConsumeStamina(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (CurrentStamina < amount)
            {
                return false;
            }

            CurrentStamina -= amount;
            return true;
        }

        public void TickStamina(float deltaTime, bool isSprinting)
        {
            if (isSprinting)
            {
                CurrentStamina = Mathf.Max(0f, CurrentStamina - (sprintStaminaCostPerSecond * deltaTime));
            }
            else
            {
                CurrentStamina = Mathf.Min(MaxStamina, CurrentStamina + (staminaRegenPerSecond * deltaTime));
            }
        }
    }
}
