using System;
using System.Collections.Generic;
using System.Linq;
using ProjectS.Data.Definitions;
using ProjectS.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    /// <summary>
    /// Waiting room UI view (MVC): renders slots and exposes user intents.
    /// </summary>
    public class WaitingRoomView : MonoBehaviour, IWaitingRoomView
    {
        [SerializeField] private WaitingRoomPlayerSlotView[] playerSlots;
        [SerializeField] private Button startButton;
        [SerializeField] private Button readyButton;
        [SerializeField] private TMP_Text readyButtonLabel;
        [SerializeField] private Button leaveButton;
        [Header("Class Selection")]
        [SerializeField] private ClassDefinition[] classDefinitions;
        [SerializeField] private Transform classListParent;
        [SerializeField] private WaitingRoomClassOptionView classOptionPrefab;

        public event Action OnReadyClicked;
        public event Action OnStartClicked;
        public event Action OnLeaveClicked;
        public event Action<int> OnKickClicked;
        public event Action<int> OnClassSelected;

        private readonly Dictionary<int, ClassDefinition> classById = new Dictionary<int, ClassDefinition>();
        private readonly Dictionary<int, WaitingRoomClassOptionView> classOptionViews = new Dictionary<int, WaitingRoomClassOptionView>();

        private void Awake()
        {
            WireButtons();
            BuildClassOptions();
        }

        public void RenderSlots(IEnumerable<WaitingRoomPlayerViewModel> players)
        {
            if (playerSlots == null)
            {
                return;
            }

            int index = 0;
            foreach (WaitingRoomPlayerViewModel vm in players)
            {
                if (index < playerSlots.Length)
                {
                    playerSlots[index].Render(vm, ResolveClassDisplayName(vm.ClassId), HandleKickRequested);
                }

                index++;
            }

            for (; index < playerSlots.Length; index++)
            {
                playerSlots[index].Clear();
            }
        }

        public void UpdateControls(bool isHost, bool canStart, bool localReady)
        {
            if (startButton != null)
            {
                startButton.gameObject.SetActive(isHost);
                startButton.interactable = canStart;
            }

            if (readyButton != null)
            {
                readyButton.gameObject.SetActive(!isHost);
                readyButton.interactable = !isHost;
            }

            if (readyButtonLabel != null)
            {
                readyButtonLabel.text = localReady ? "Cancel Ready" : "Ready";
            }
        }

        public void UpdateClassSelection(IReadOnlyCollection<int> takenClassIds, int localClassId)
        {
            if (classOptionViews.Count == 0)
            {
                return;
            }

            HashSet<int> taken = takenClassIds != null ? new HashSet<int>(takenClassIds) : new HashSet<int>();
            foreach (KeyValuePair<int, WaitingRoomClassOptionView> entry in classOptionViews)
            {
                int classId = entry.Key;
                bool isSelected = classId == localClassId;
                bool isAvailable = !taken.Contains(classId) || isSelected;
                entry.Value.SetAvailability(isAvailable, isSelected);
            }
        }

        private void WireButtons()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(() => OnStartClicked?.Invoke());
            }

            if (readyButton != null)
            {
                readyButton.onClick.AddListener(() => OnReadyClicked?.Invoke());
            }

            if (leaveButton != null)
            {
                leaveButton.onClick.AddListener(() => OnLeaveClicked?.Invoke());
            }
        }

        private void HandleKickRequested(int actorNumber)
        {
            OnKickClicked?.Invoke(actorNumber);
        }

        private void HandleClassSelected(int classId)
        {
            OnClassSelected?.Invoke(classId);
        }

        private void BuildClassOptions()
        {
            classById.Clear();
            classOptionViews.Clear();

            if (classDefinitions == null || classListParent == null || classOptionPrefab == null)
            {
                return;
            }

            foreach (ClassDefinition definition in classDefinitions.Where(d => d != null))
            {
                if (!TryParseClassId(definition.id, out int classId))
                {
                    Debug.LogWarning($"[WaitingRoom] Invalid class id '{definition.id}' for {definition.name}.");
                    continue;
                }

                classById[classId] = definition;
                WaitingRoomClassOptionView optionView = Instantiate(classOptionPrefab, classListParent);
                optionView.Initialize(definition, classId, HandleClassSelected);
                classOptionViews[classId] = optionView;
            }
        }

        private string ResolveClassDisplayName(int classId)
        {
            if (classById.TryGetValue(classId, out ClassDefinition definition) && definition != null)
            {
                return string.IsNullOrWhiteSpace(definition.displayName) ? definition.name : definition.displayName;
            }

            return $"Class {classId}";
        }

        private static bool TryParseClassId(string id, out int classId)
        {
            if (int.TryParse(id, out classId))
            {
                return true;
            }

            classId = 0;
            return false;
        }
    }
}
