using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PS.Manager
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Called when local player left the room
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// 룸 종료 버튼에 바인딩
        /// </summary>
        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }

        /// <summary>
        /// 플레이어가 룸에 들어올 때 실행되는 이벤트
        /// </summary>
        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            Debug.Log($"OnPlayerEnteredRoom: {newPlayer.NickName}");
            
            // 만약 마스터 클라이언트라면, 레벨 로드
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"OnPlayerEnteredRoom IsMasterClient: {PhotonNetwork.IsMasterClient}");
                LoadArena();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"OnPlayerLeftRoom: {otherPlayer.NickName}");

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"OnPlayerLeftRoom IsMasterClient: {PhotonNetwork.IsMasterClient}");
                
                LoadArena();
            }
        }

        /// <summary>
        /// PhotonNetwork.automaticallySyncScene을 사용하기 때문에 모든 접속한 클라이언트에 대해
        /// 레벨 로드를 유니티가 하는게 아닌 포톤이 한다.
        /// </summary>
        private void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.Log($"PhotonNetwork : Loading Level : {PhotonNetwork.CurrentRoom.PlayerCount}");
            PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }
}

