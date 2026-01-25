using System.Collections.Generic;
using ProjectS.Core.Skills;
using ProjectS.Networking;
using UnityEngine;

namespace ProjectS.UI
{
    public class TeamUpgradeVoteController : MonoBehaviour
    {
        private const string TeamUpgradeVotePrefabName = "TeamUpgradeVoteViewUI";

        private TeamUpgradeManager upgradeManager;
        [SerializeField] private TeamUpgradeVoteView voteView;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<TeamUpgradeVoteController>() != null)
            {
                return;
            }

            GameObject controller = new GameObject("TeamUpgradeVoteController");
            controller.AddComponent<TeamUpgradeVoteController>();
        }

        private void Awake()
        {
            if (voteView == null)
            {
                voteView = FindObjectOfType<TeamUpgradeVoteView>();
            }

            if (voteView == null)
            {
                GameObject prefab = Resources.Load<GameObject>(TeamUpgradeVotePrefabName);
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab);
                    voteView = instance.GetComponentInChildren<TeamUpgradeVoteView>();
                }
            }

            if (voteView == null)
            {
                Debug.LogError($"TeamUpgradeVoteView not found. Create prefab in Resources/{TeamUpgradeVotePrefabName}.", this);
            }
        }

        private void OnEnable()
        {
            upgradeManager = TeamUpgradeManager.Instance;
            if (upgradeManager != null)
            {
                upgradeManager.VoteStarted += HandleVoteStarted;
                upgradeManager.VoteUpdated += HandleVoteUpdated;
                upgradeManager.UpgradeGranted += HandleUpgradeGranted;
            }
        }

        private void OnDisable()
        {
            if (upgradeManager != null)
            {
                upgradeManager.VoteStarted -= HandleVoteStarted;
                upgradeManager.VoteUpdated -= HandleVoteUpdated;
                upgradeManager.UpgradeGranted -= HandleUpgradeGranted;
            }
        }

        private void HandleVoteStarted(IReadOnlyList<TeamUpgradeOption> options, float duration)
        {
            if (voteView == null)
            {
                return;
            }

            voteView.Show(options, duration, option =>
            {
                upgradeManager?.SubmitVote(option.Type);
            },
            () =>
            {
                upgradeManager?.SubmitVote(null);
            });
        }

        private void HandleUpgradeGranted(TeamUpgradeType type)
        {
            voteView?.Hide();
        }

        private void HandleVoteUpdated(IReadOnlyList<float> ratios)
        {
            voteView?.UpdatePercentages(ratios);
        }
    }
}
