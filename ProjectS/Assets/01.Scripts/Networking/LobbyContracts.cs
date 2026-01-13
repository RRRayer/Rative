using System;
using System.Collections.Generic;

namespace ProjectS.Networking
{
    public struct LobbyRoomViewModel
    {
        public string Name;
        public int PlayerCount;
        public int MaxPlayers;
        public bool IsPrivate;
    }

    public struct CreateRoomForm
    {
        public string RoomName;
        public bool IsPrivate;
        public string Password;
    }

    public interface ILobbyView
    {
        event Action OnCreateRoomClicked;
        event Action OnRefreshRequested;
        event Action<string> OnJoinRoomRequested;
        event Action<string> OnPasswordSubmitted;
        event Action OnPasswordCancelled;

        CreateRoomForm ReadCreateRoomForm();
        void RenderRooms(IEnumerable<LobbyRoomViewModel> rooms);
        void PromptPassword(string roomName);
        void ClosePasswordPrompt();
        string CurrentPasswordEntry { get; }
    }
}
