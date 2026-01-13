using System;
using System.Collections.Generic;
using ProjectS.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    /// <summary>
    /// Lobby UI view (MVC): renders room list and surfaces user intent via events.
    /// </summary>
    public class LobbyView : MonoBehaviour, ILobbyView
    {
        [Header("Room List UI")]
        [SerializeField] private Transform roomListParent;
        [SerializeField] private RoomEntryView roomEntryPrefab;
        [SerializeField] private Button refreshButton;

        [Header("Create Room")]
        [SerializeField] private TMP_InputField roomNameInput;
        [SerializeField] private Toggle privateRoomToggle;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button createRoomButton;

        [Header("Password Prompt")]
        [SerializeField] private GameObject passwordPopup;
        [SerializeField] private TMP_InputField passwordPopupInput;
        [SerializeField] private Button passwordConfirmButton;
        [SerializeField] private Button passwordCancelButton;

        private readonly List<RoomEntryView> spawnedEntries = new List<RoomEntryView>();

        public event Action OnCreateRoomClicked;
        public event Action OnRefreshRequested;
        public event Action<string> OnJoinRoomRequested;
        public event Action<string> OnPasswordSubmitted;
        public event Action OnPasswordCancelled;

        public string CurrentPasswordEntry => passwordPopupInput != null ? passwordPopupInput.text : string.Empty;

        private void Awake()
        {
            WireButtons();
        }

        public CreateRoomForm ReadCreateRoomForm()
        {
            return new CreateRoomForm
            {
                RoomName = roomNameInput != null ? roomNameInput.text : string.Empty,
                IsPrivate = privateRoomToggle != null && privateRoomToggle.isOn,
                Password = passwordInput != null ? passwordInput.text : string.Empty
            };
        }

        public void RenderRooms(IEnumerable<LobbyRoomViewModel> rooms)
        {
            ClearRoomEntries();

            if (roomListParent == null || roomEntryPrefab == null || rooms == null)
            {
                return;
            }

            foreach (LobbyRoomViewModel room in rooms)
            {
                RoomEntryView entry = Instantiate(roomEntryPrefab, roomListParent);
                entry.Render(room, HandleJoinRoomClicked);
                spawnedEntries.Add(entry);
            }
        }

        public void PromptPassword(string roomName)
        {
            if (passwordPopup != null)
            {
                passwordPopup.SetActive(true);
            }

            passwordPopupInput?.SetTextWithoutNotify(string.Empty);
        }

        public void ClosePasswordPrompt()
        {
            if (passwordPopup != null)
            {
                passwordPopup.SetActive(false);
            }

            passwordPopupInput?.SetTextWithoutNotify(string.Empty);
        }

        private void WireButtons()
        {
            if (refreshButton != null)
            {
                refreshButton.onClick.AddListener(() => OnRefreshRequested?.Invoke());
            }

            if (createRoomButton != null)
            {
                createRoomButton.onClick.AddListener(() => OnCreateRoomClicked?.Invoke());
            }

            if (passwordConfirmButton != null)
            {
                passwordConfirmButton.onClick.AddListener(() => OnPasswordSubmitted?.Invoke(CurrentPasswordEntry));
            }

            if (passwordCancelButton != null)
            {
                passwordCancelButton.onClick.AddListener(() => OnPasswordCancelled?.Invoke());
            }
        }

        private void HandleJoinRoomClicked(string roomName)
        {
            OnJoinRoomRequested?.Invoke(roomName);
        }

        private void ClearRoomEntries()
        {
            foreach (RoomEntryView entry in spawnedEntries)
            {
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }

            spawnedEntries.Clear();
        }
    }
}
