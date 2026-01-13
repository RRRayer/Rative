using UnityEngine;
using TMPro;
using Photon.Pun;

namespace ProjectS.UI
{
    public class UINickNameField : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _nicknameInputField;
        private void Start()
        {
            // Load saved nickname
            string savedNickname = PlayerPrefs.GetString("PlayerNickname", string.Empty);
            if (!string.IsNullOrEmpty(savedNickname))
            {
                _nicknameInputField.text = savedNickname;
            }
        }
        public void SetNickname()
        {
            string nickname = _nicknameInputField.text;
            nickname = nickname.Trim();

            if (string.IsNullOrWhiteSpace(nickname))
            {
                Debug.LogError("Nickname is null or empty");
                return;
            }
            
            PhotonNetwork.NickName = nickname;
            PlayerPrefs.SetString("PlayerNickname", nickname);
        }
    }
}