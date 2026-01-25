using System;
using System.Collections.Generic;
using PS.Core.Skills;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace PS.UI
{
    public class TeamUpgradeVoteView : MonoBehaviour
    {
        private const float DefaultCardWidth = 320f;
        private const float DefaultCardHeight = 420f;

        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Text timerLabel;
        [SerializeField] private Text headerLabel;
        [SerializeField] private Text[] percentageLabels;

        private readonly List<Button> optionButtons = new List<Button>();
        private float voteEndTime;
        private float voteDuration;
        private bool ticking;
        private float cardWidth = DefaultCardWidth;
        private float cardHeight = DefaultCardHeight;

        private void Awake()
        {
            EnsureUI();
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

        public void Show(
            IReadOnlyList<TeamUpgradeOption> options,
            float durationSeconds,
            Action<TeamUpgradeOption> onSelected,
            Action onAbstain)
        {
            EnsureUI();
            ClearOptions();

            if (canvas == null || panelRoot == null)
            {
                return;
            }

            if (options == null || options.Count == 0)
            {
                canvas.enabled = false;
                return;
            }

            canvas.enabled = true;
            voteDuration = Mathf.Max(0.1f, durationSeconds);
            voteEndTime = Time.unscaledTime + voteDuration;
            ticking = true;

            if (headerLabel != null && string.IsNullOrEmpty(headerLabel.text))
            {
                headerLabel.text = "Team Upgrade Vote";
            }

            if (timerSlider != null)
            {
                timerSlider.minValue = 0f;
                timerSlider.maxValue = 1f;
                timerSlider.value = 1f;
            }

            UpdateLayout(options.Count);
            for (int i = 0; i < options.Count; i++)
            {
                TeamUpgradeOption option = options[i];
                Button button = CreateOptionButton(option);
                if (button == null)
                {
                    continue;
                }

                button.onClick.AddListener(() =>
                {
                    ticking = false;
                    onSelected?.Invoke(option);
                });
                optionButtons.Add(button);
            }

            UpdatePercentages(null);

            if (onAbstain != null)
            {
                StartCoroutine(WaitForTimeout(onAbstain));
            }
        }

        public void Hide()
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }

            ticking = false;
            ClearOptions();
        }

        public void UpdatePercentages(IReadOnlyList<float> ratios)
        {
            if (percentageLabels == null || percentageLabels.Length == 0)
            {
                return;
            }

            for (int i = 0; i < percentageLabels.Length; i++)
            {
                if (percentageLabels[i] == null)
                {
                    continue;
                }

                float value = 0f;
                if (ratios != null && i < ratios.Count)
                {
                    value = ratios[i];
                }

                percentageLabels[i].text = $"{Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void Update()
        {
            if (!ticking)
            {
                return;
            }

            float remaining = Mathf.Max(0f, voteEndTime - Time.unscaledTime);
            float ratio = Mathf.Clamp01(remaining / Mathf.Max(0.01f, voteDuration));

            if (timerSlider != null)
            {
                timerSlider.value = ratio;
            }

            if (timerLabel != null)
            {
                timerLabel.text = $"{Mathf.CeilToInt(remaining)}s";
            }
        }

        private void EnsureUI()
        {
            if (canvas == null)
            {
                canvas = GetComponentInChildren<Canvas>(true);
            }

            if (panelRoot == null)
            {
                HorizontalLayoutGroup layout = GetComponentInChildren<HorizontalLayoutGroup>(true);
                if (layout != null)
                {
                    panelRoot = layout.GetComponent<RectTransform>();
                }
            }

            SetupEventSystem();

            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

        private Button CreateOptionButton(TeamUpgradeOption option)
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

            Rect rect = panelRoot.rect;
            float availableWidth = Mathf.Max(0f, rect.width);
            float availableHeight = Mathf.Max(0f, rect.height);
            int count = Mathf.Max(1, optionCount);

            float spacing = 0f;
            float horizontalPadding = 0f;
            float verticalPadding = 0f;
            HorizontalLayoutGroup layout = panelRoot.GetComponent<HorizontalLayoutGroup>();
            if (layout != null)
            {
                spacing = layout.spacing;
                horizontalPadding = layout.padding.left + layout.padding.right;
                verticalPadding = layout.padding.top + layout.padding.bottom;
            }

            float targetWidth = (availableWidth - horizontalPadding - (spacing * (count - 1))) / count;
            cardWidth = Mathf.Max(0f, targetWidth);
            cardHeight = Mathf.Max(0f, availableHeight - verticalPadding);
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

        private System.Collections.IEnumerator WaitForTimeout(Action onAbstain)
        {
            float endTime = voteEndTime;
            while (Time.unscaledTime < endTime && ticking)
            {
                yield return null;
            }

            if (ticking)
            {
                ticking = false;
                onAbstain?.Invoke();
            }
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

