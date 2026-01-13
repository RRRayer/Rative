using System;
using System.Collections.Generic;
using ProjectS.Core.Skills;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace ProjectS.UI
{
    public class UpgradeSelectionView : MonoBehaviour
    {
        private const float PanelPadding = 32f;
        private const float CardSpacing = 24f;
        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform panelRoot;
        private readonly List<Button> optionButtons = new List<Button>();
        private float cardWidth = 320f;
        private float cardHeight = 420f;

        public void Show(IReadOnlyList<UpgradeOption> options, Action<UpgradeOption> onSelected)
        {
            EnsureUI();
            ClearOptions();

            if (options == null || options.Count == 0)
            {
                canvas.enabled = false;
                return;
            }

            canvas.enabled = true;
            UpdateLayout(options.Count);
            for (int i = 0; i < options.Count; i++)
            {
                UpgradeOption option = options[i];
                Button button = CreateOptionButton(option);
                if (button == null)
                {
                    continue;
                }
                button.onClick.AddListener(() => onSelected?.Invoke(option));
                optionButtons.Add(button);
            }
        }

        public void Hide()
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }
            ClearOptions();
        }

        private void EnsureUI()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>(true);
            }

            if (panelRoot == null && canvas != null)
            {
                HorizontalLayoutGroup layout = canvas.GetComponentInChildren<HorizontalLayoutGroup>(true);
                if (layout != null)
                {
                    panelRoot = layout.GetComponent<RectTransform>();
                }
            }

            if (canvas == null || panelRoot == null)
            {
                Debug.LogError("UpgradeSelectionView requires a Canvas and PanelRoot in the prefab.", this);
                return;
            }

            SetupEventSystem();
        }

        private Button CreateOptionButton(UpgradeOption option)
        {
            if (panelRoot == null)
            {
                return null;
            }

            GameObject buttonObject = new GameObject($"Option_{option.Type}");
            buttonObject.transform.SetParent(panelRoot, false);

            RectTransform rect = buttonObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(cardWidth, cardHeight);

            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.9f, 0.9f, 0.9f, 0.95f);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            button.colors = colors;

            GameObject titleObject = new GameObject("Title");
            titleObject.transform.SetParent(buttonObject.transform, false);
            Text titleText = titleObject.AddComponent<Text>();
            titleText.text = option.Title;
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 20;
            titleText.alignment = TextAnchor.UpperCenter;
            RectTransform titleRect = titleObject.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.1f, 0.7f);
            titleRect.anchorMax = new Vector2(0.9f, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;

            GameObject descObject = new GameObject("Description");
            descObject.transform.SetParent(buttonObject.transform, false);
            Text descText = descObject.AddComponent<Text>();
            descText.text = option.Description;
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 16;
            descText.alignment = TextAnchor.UpperCenter;
            RectTransform descRect = descObject.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.1f);
            descRect.anchorMax = new Vector2(0.9f, 0.65f);
            descRect.offsetMin = Vector2.zero;
            descRect.offsetMax = Vector2.zero;

            return button;
        }

        private void UpdateLayout(int optionCount)
        {
            if (panelRoot == null)
            {
                return;
            }

            int count = Mathf.Max(1, optionCount);
            Rect rect = panelRoot.rect;
            float availableWidth = Mathf.Max(0f, rect.width);
            float availableHeight = Mathf.Max(0f, rect.height);
            float targetWidth = (availableWidth - (CardSpacing * (count - 1))) / count;
            cardWidth = Mathf.Max(0f, targetWidth);
            cardHeight = Mathf.Max(0f, availableHeight);
        }

        private void ClearOptions()
        {
            for (int i = 0; i < optionButtons.Count; i++)
            {
                if (optionButtons[i] != null)
                {
                    Destroy(optionButtons[i].gameObject);
                }
            }

            optionButtons.Clear();
        }

        private void SetupEventSystem()
        {
            EventSystem existing = FindObjectOfType<EventSystem>();
            if (existing != null)
            {
#if ENABLE_INPUT_SYSTEM
                if (existing.GetComponent<InputSystemUIInputModule>() == null)
                {
                    existing.gameObject.AddComponent<InputSystemUIInputModule>();
                }
#else
                if (existing.GetComponent<StandaloneInputModule>() == null)
                {
                    existing.gameObject.AddComponent<StandaloneInputModule>();
                }
#endif
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
