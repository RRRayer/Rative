using System;
using System.Collections.Generic;

namespace ProjectS.Networking
{
    public struct WaitingRoomPlayerViewModel
    {
        public int ActorNumber;
        public string Nickname;
        public bool IsReady;
        public bool IsHost;
        public bool CanKick;
        public bool IsLocal;
        public int ClassId;
    }

    public interface IWaitingRoomView
    {
        event Action OnReadyClicked;
        event Action OnStartClicked;
        event Action OnLeaveClicked;
        event Action<int> OnKickClicked;
        event Action<int> OnClassSelected;

        void RenderSlots(IEnumerable<WaitingRoomPlayerViewModel> players);
        void UpdateControls(bool isHost, bool canStart, bool localReady);
        void UpdateClassSelection(IReadOnlyCollection<int> takenClassIds, int localClassId);
    }
}
