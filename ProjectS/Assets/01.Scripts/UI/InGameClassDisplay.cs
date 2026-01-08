using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using ProjectS.Data.Definitions;
using ProjectS.Networking;
using TMPro;
using UnityEngine;

namespace ProjectS.UI
{
    public class InGameClassDisplay : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Text classLabel;
        [SerializeField] private ClassDefinition[] classDefinitions;

        private readonly Dictionary<int, ClassDefinition> classById = new Dictionary<int, ClassDefinition>();

        private void Awake()
        {
            BuildClassLookup();
        }

        private void OnEnable()
        {
            UpdateDisplay();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
        {
            if (targetPlayer == PhotonNetwork.LocalPlayer &&
                changedProps.ContainsKey(NetworkPropertyKeys.Player.ClassId))
            {
                UpdateDisplay();
            }
        }

        private void BuildClassLookup()
        {
            classById.Clear();

            if (classDefinitions == null)
            {
                return;
            }

            foreach (ClassDefinition definition in classDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                if (TryParseClassId(definition.id, out int classId))
                {
                    classById[classId] = definition;
                }
            }
        }

        private void UpdateDisplay()
        {
            if (classLabel == null)
            {
                return;
            }

            int classId = GetLocalClassId();
            string displayName = ResolveDisplayName(classId);
            classLabel.text = $"Class: {displayName}";
        }

        private int GetLocalClassId()
        {
            if (PhotonNetwork.LocalPlayer?.CustomProperties == null)
            {
                return 0;
            }

            if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(NetworkPropertyKeys.Player.ClassId, out object value))
            {
                if (value is int intValue)
                {
                    return intValue;
                }

                if (value is byte byteValue)
                {
                    return byteValue;
                }
            }

            return 0;
        }

        private string ResolveDisplayName(int classId)
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
