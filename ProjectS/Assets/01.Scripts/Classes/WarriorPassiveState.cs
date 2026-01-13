using System;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Classes
{
    public sealed class WarriorPassiveState : MonoBehaviour
    {
        [Serializable]
        public struct PassiveLevel
        {
            [Range(1, 5)] public int level;
            [Range(0f, 0.5f)] public float attackSpeedPerStackPercent;
            public float stackDurationSeconds;
            [Range(0f, 0.5f)] public float moveSpeedPerStackPercent;
            public int maxStacks;
            [Range(0f, 1f)] public float cooldownReductionAtMaxPercent;
        }

        [SerializeField] private PassiveUpgradeTrack upgradeTrack;
        [SerializeField] private PassiveLevel[] levels;

        private PassiveLevel currentLevel;
        private int stacks;
        private float stackExpiresAt;

        public event Action Changed;

        public int Stacks => stacks;
        public int CurrentLevel => currentLevel.level;

        public float ComboResetMultiplier
        {
            get
            {
                float speedBonus = stacks * currentLevel.attackSpeedPerStackPercent;
                return 1f / Mathf.Max(0.1f, 1f + speedBonus);
            }
        }

        public float MoveSpeedMultiplier => 1f + (stacks * currentLevel.moveSpeedPerStackPercent);

        public float CooldownMultiplier => stacks >= currentLevel.maxStacks
            ? Mathf.Max(0.1f, 1f - currentLevel.cooldownReductionAtMaxPercent)
            : 1f;

        private void Awake()
        {
            if (levels == null || levels.Length == 0)
            {
                levels = CreateDefaultLevels();
            }

            SetLevel(0);
        }

        private void Update()
        {
            if (stacks > 0 && Time.time >= stackExpiresAt)
            {
                stacks = 0;
                stackExpiresAt = 0f;
                Changed?.Invoke();
            }
        }

        public void SetLevel(int level)
        {
            PassiveLevel resolved = ResolveLevel(level);

            currentLevel = resolved;
            if (stacks > currentLevel.maxStacks)
            {
                stacks = currentLevel.maxStacks;
            }

            Changed?.Invoke();
        }

        public void SetUpgradeTrack(PassiveUpgradeTrack track)
        {
            upgradeTrack = track;
            SetLevel(0);
        }

        public void RegisterHit()
        {
            if (currentLevel.level <= 0)
            {
                return;
            }

            int nextStacks = Mathf.Clamp(stacks + 1, 0, Mathf.Max(1, currentLevel.maxStacks));
            if (nextStacks != stacks)
            {
                stacks = nextStacks;
                Changed?.Invoke();
            }

            stackExpiresAt = Time.time + Mathf.Max(0.1f, currentLevel.stackDurationSeconds);
        }

        private PassiveLevel[] CreateDefaultLevels()
        {
            return new[]
            {
                new PassiveLevel
                {
                    level = 1,
                    attackSpeedPerStackPercent = 0.03f,
                    stackDurationSeconds = 5f,
                    moveSpeedPerStackPercent = 0f,
                    maxStacks = 5,
                    cooldownReductionAtMaxPercent = 0f
                },
                new PassiveLevel
                {
                    level = 2,
                    attackSpeedPerStackPercent = 0.04f,
                    stackDurationSeconds = 8f,
                    moveSpeedPerStackPercent = 0f,
                    maxStacks = 5,
                    cooldownReductionAtMaxPercent = 0f
                },
                new PassiveLevel
                {
                    level = 3,
                    attackSpeedPerStackPercent = 0.04f,
                    stackDurationSeconds = 8f,
                    moveSpeedPerStackPercent = 0.02f,
                    maxStacks = 5,
                    cooldownReductionAtMaxPercent = 0f
                },
                new PassiveLevel
                {
                    level = 4,
                    attackSpeedPerStackPercent = 0.04f,
                    stackDurationSeconds = 8f,
                    moveSpeedPerStackPercent = 0.02f,
                    maxStacks = 7,
                    cooldownReductionAtMaxPercent = 0f
                },
                new PassiveLevel
                {
                    level = 5,
                    attackSpeedPerStackPercent = 0.04f,
                    stackDurationSeconds = 8f,
                    moveSpeedPerStackPercent = 0.02f,
                    maxStacks = 7,
                    cooldownReductionAtMaxPercent = 0.2f
                }
            };
        }

        private PassiveLevel ResolveLevel(int level)
        {
            if (level <= 0)
            {
                return default;
            }

            if (upgradeTrack != null)
            {
                PassiveUpgradeStep step = upgradeTrack.Evaluate(level);
                if (step.level > 0)
                {
                    return new PassiveLevel
                    {
                        level = step.level,
                        attackSpeedPerStackPercent = step.attackSpeedPerStackPercent,
                        stackDurationSeconds = step.stackDurationSeconds,
                        moveSpeedPerStackPercent = step.moveSpeedPerStackPercent,
                        maxStacks = step.maxStacks,
                        cooldownReductionAtMaxPercent = step.cooldownReductionAtMaxPercent
                    };
                }
            }

            PassiveLevel resolved = levels[0];
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].level <= level)
                {
                    resolved = levels[i];
                }
            }

            return resolved;
        }
    }
}
