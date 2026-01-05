using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace PS.UI
{
    [RequireComponent(typeof(TMP_InputField))]
    public class UIPlayerNameInputField : MonoBehaviour
    {
        private const string playerNamePrefKey = "PlayerName";

        private void Start()
        {
            // Load and apply the stored nickname, if any.
            string defaultName = string.Empty;
            TMP_InputField inputfield = GetComponent<TMP_InputField>();
            if (inputfield != null)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                    inputfield.text = defaultName;
                }
            }
            PhotonNetwork.NickName = defaultName;
        }

        /// <summary>
        /// Called by the input field submit/check action.
        /// </summary>
        /// <param name="value"></param>
        public void SetPlayerName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(playerNamePrefKey, value);
        }
    }
}
