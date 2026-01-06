using System;
using ProjectS.Data.Definitions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectS.UI
{
    /// <summary>
    /// View for a selectable class option.
    /// </summary>
    public class WaitingRoomClassOptionView : MonoBehaviour
    {
        [SerializeField] private TMP_Text classNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text skillQText;
        [SerializeField] private TMP_Text skillEText;
        [SerializeField] private TMP_Text skillRText;
        [SerializeField] private Button selectButton;
        [SerializeField] private GameObject selectedMarker;
        [SerializeField] private GameObject lockedMarker;

        private int classId;
        private Action<int> onSelected;

        public void Initialize(ClassDefinition definition, int id, Action<int> onSelectedCallback)
        {
            classId = id;
            onSelected = onSelectedCallback;

            if (classNameText != null)
            {
                classNameText.text = string.IsNullOrWhiteSpace(definition.displayName) ? definition.name : definition.displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = definition.description ?? string.Empty;
            }

            if (skillQText != null)
            {
                skillQText.text = FormatSkill("Q", definition.skillQ);
            }

            if (skillEText != null)
            {
                skillEText.text = FormatSkill("E", definition.skillE);
            }

            if (skillRText != null)
            {
                skillRText.text = FormatSkill("R", definition.skillR);
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(HandleSelected);
            }
        }

        public void SetAvailability(bool isAvailable, bool isSelected)
        {
            if (selectButton != null)
            {
                selectButton.interactable = isAvailable;
            }

            if (selectedMarker != null)
            {
                selectedMarker.SetActive(isSelected);
            }

            if (lockedMarker != null)
            {
                lockedMarker.SetActive(!isAvailable);
            }
        }

        private void HandleSelected()
        {
            onSelected?.Invoke(classId);
        }

        private static string FormatSkill(string slot, SkillDefinition skill)
        {
            if (skill == null)
            {
                return $"{slot}: -";
            }

            return $"{slot}: {skill.displayName} (CD {skill.cooldown:0.#}s)";
        }
    }
}
