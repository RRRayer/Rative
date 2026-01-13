using ProjectS.Core.Skills;
using ProjectS.Networking;
using UnityEngine;

namespace ProjectS.UI
{
    public class UpgradeSelectionController : MonoBehaviour
    {
        private const string UpgradeSelectionPrefabName = "UpgradeSelectionUI";

        private SharedProgressionManager sharedProgression;
        [SerializeField] private UpgradeSelectionView selectionView;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<UpgradeSelectionController>() != null)
            {
                return;
            }

            GameObject controller = new GameObject("UpgradeSelectionController");
            controller.AddComponent<UpgradeSelectionController>();
        }

        private void Awake()
        {
            if (selectionView == null)
            {
                selectionView = FindObjectOfType<UpgradeSelectionView>();
            }

            if (selectionView == null)
            {
                GameObject prefab = Resources.Load<GameObject>(UpgradeSelectionPrefabName);
                if (prefab != null)
                {
                    GameObject instance = Instantiate(prefab);
                    selectionView = instance.GetComponentInChildren<UpgradeSelectionView>();
                }
            }

            if (selectionView == null)
            {
                Debug.LogError($"UpgradeSelectionView not found. Create prefab in Resources/{UpgradeSelectionPrefabName}.", this);
            }
        }

        private void OnEnable()
        {
            sharedProgression = SharedProgressionManager.Instance;
            if (sharedProgression != null)
            {
                sharedProgression.SelectionStarted += HandleSelectionStarted;
                sharedProgression.SelectionEnded += HandleSelectionEnded;
            }
        }

        private void OnDisable()
        {
            if (sharedProgression != null)
            {
                sharedProgression.SelectionStarted -= HandleSelectionStarted;
                sharedProgression.SelectionEnded -= HandleSelectionEnded;
            }
        }

        private void HandleSelectionStarted()
        {
            IUpgradeProvider provider = FindLocalUpgradeProvider();
            if (provider == null)
            {
                sharedProgression?.SubmitSelection();
                return;
            }

            var options = provider.BuildUpgradeOptions(3);
            if (options.Count == 0)
            {
                sharedProgression?.SubmitSelection();
                return;
            }

            selectionView.Show(options, option =>
            {
                provider.ApplyUpgradeChoice(option);
                selectionView.Hide();
                sharedProgression?.SubmitSelection();
            });
        }

        private void HandleSelectionEnded()
        {
            selectionView.Hide();
        }

        private IUpgradeProvider FindLocalUpgradeProvider()
        {
            Photon.Pun.PhotonView[] views = FindObjectsOfType<Photon.Pun.PhotonView>();
            for (int i = 0; i < views.Length; i++)
            {
                if (!views[i].IsMine)
                {
                    continue;
                }

                IUpgradeProvider provider = views[i].GetComponentInParent<IUpgradeProvider>();
                if (provider != null)
                {
                    return provider;
                }
            }

            MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is IUpgradeProvider provider)
                {
                    return provider;
                }
            }

            return null;
        }
    }
}
