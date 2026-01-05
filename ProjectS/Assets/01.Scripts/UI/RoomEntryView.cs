using System;
using ProjectS.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    /// <summary>
    /// View for a single room entry in the lobby list.
    /// </summary>
    public class RoomEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text roomNameText;
        [SerializeField] private TMP_Text playerCountText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Button joinButton;

        private LobbyRoomViewModel viewModel;
        private Action<string> onJoinRequested;

        public void Render(LobbyRoomViewModel model, Action<string> joinCallback)
        {
            viewModel = model;
            onJoinRequested = joinCallback;

            if (roomNameText != null)
            {
                roomNameText.text = model.Name;
            }

            if (playerCountText != null)
            {
                playerCountText.text = $"{model.PlayerCount}/{model.MaxPlayers}";
            }

            if (lockIcon != null)
            {
                lockIcon.SetActive(model.IsPrivate);
            }

            if (joinButton != null)
            {
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(HandleJoinClicked);
            }
        }

        private void HandleJoinClicked()
        {
            onJoinRequested?.Invoke(viewModel.Name);
        }
    }
}
