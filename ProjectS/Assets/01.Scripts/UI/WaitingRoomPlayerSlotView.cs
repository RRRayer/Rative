using System;
using ProjectS.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    /// <summary>
    /// View for a player slot in the waiting room.
    /// </summary>
    public class WaitingRoomPlayerSlotView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text classNameText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private GameObject hostMarker;
        [SerializeField] private GameObject readyMarker;
        [SerializeField] private GameObject localMarker;
        [SerializeField] private Button kickButton;

        private int actorNumber;
        private Action<int> onKickRequested;

        public void Render(WaitingRoomPlayerViewModel viewModel, string classDisplayName, Action<int> kickCallback)
        {
            actorNumber = viewModel.ActorNumber;
            onKickRequested = kickCallback;

            if (nicknameText != null)
            {
                nicknameText.text = viewModel.Nickname ?? "-";
            }

            if (classNameText != null)
            {
                classNameText.text = classDisplayName ?? string.Empty;
            }

            if (stateText != null)
            {
                stateText.text = viewModel.IsReady ? "Ready" : "Not Ready";
            }

            if (hostMarker != null)
            {
                hostMarker.SetActive(viewModel.IsHost);
            }

            if (readyMarker != null)
            {
                readyMarker.SetActive(viewModel.IsReady);
            }

            if (localMarker != null)
            {
                localMarker.SetActive(viewModel.IsLocal);
            }

            if (kickButton != null)
            {
                kickButton.gameObject.SetActive(viewModel.CanKick);
                kickButton.onClick.RemoveAllListeners();
                if (viewModel.CanKick)
                {
                    kickButton.onClick.AddListener(HandleKickClicked);
                }
            }

            gameObject.SetActive(true);
        }

        public void Clear()
        {
            actorNumber = 0;
            onKickRequested = null;

            if (nicknameText != null)
            {
                nicknameText.text = "-";
            }

            if (stateText != null)
            {
                stateText.text = string.Empty;
            }

            if (classNameText != null)
            {
                classNameText.text = string.Empty;
            }

            if (hostMarker != null)
            {
                hostMarker.SetActive(false);
            }

            if (readyMarker != null)
            {
                readyMarker.SetActive(false);
            }

            if (localMarker != null)
            {
                localMarker.SetActive(false);
            }

            if (kickButton != null)
            {
                kickButton.gameObject.SetActive(false);
                kickButton.onClick.RemoveAllListeners();
            }

            gameObject.SetActive(false);
        }

        private void HandleKickClicked()
        {
            onKickRequested?.Invoke(actorNumber);
        }
    }
}
