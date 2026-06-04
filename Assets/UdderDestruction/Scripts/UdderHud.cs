using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UdderDestruction
{
    public sealed class UdderHud : MonoBehaviour
    {
        public UdderGameController game;
        public UdderPlayer player;
        public TMP_FontAsset font;
        public Sprite panelSprite;
        public Sprite buttonSprite;
        public Sprite buttonPressedSprite;
        public Sprite barSprite;

        [Header("Inspectable HUD Elements")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image bovinityFill;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private GameObject powerChoicePanel;
        [SerializeField] private GameObject powerDescriptionPanel;
        [SerializeField] private TMP_Text powerDescriptionTitle;
        [SerializeField] private TMP_Text powerDescriptionBody;
        [SerializeField] private List<Button> powerChoiceButtons = new();

        private Action<UdderPower> powerChoiceCallback;

        private void Awake()
        {
            EnsureEventSystem();
        }

        private void Update()
        {
            if (!game || !player || !healthFill || !bovinityFill || !statusText)
                return;

            SetBarFill(healthFill, player.Health01);
            SetBarFill(bovinityFill, player.Bovinity01);
            statusText.text = $"MOOLISSA HOOFMAN WAVE {game.Wave} LVL {player.BovinityLevel} DD {game.BankedDairyDoubles}";
        }

        private static void SetBarFill(Image fill, float amount)
        {
            if (!fill)
                return;

            Vector3 scale = fill.rectTransform.localScale;
            scale.x = Mathf.Clamp01(amount);
            fill.rectTransform.localScale = scale;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem && eventSystem.GetComponent<BaseInputModule>())
                return;

            Debug.LogWarning("UdderHud requires an EventSystem with a UI input module in the scene.");
        }

        public void BindInspectableElements(
            Image health,
            Image bovinity,
            TMP_Text status,
            TMP_Text hint,
            GameObject choicePanel,
            List<Button> choiceButtons)
        {
            healthFill = health;
            bovinityFill = bovinity;
            statusText = status;
            hintText = hint;
            powerChoicePanel = choicePanel;
            powerChoiceButtons = choiceButtons;
        }

        public void ShowPowerChoices(IReadOnlyList<UdderPower> choices, Action<UdderPower> onChosen)
        {
            powerChoiceCallback = onChosen;
            EnsurePowerDescriptionPanel();
            powerChoicePanel.SetActive(true);
            powerDescriptionPanel.SetActive(false);

            for (int i = 0; i < powerChoiceButtons.Count; i++)
            {
                Button button = powerChoiceButtons[i];
                bool active = i < choices.Count;
                button.gameObject.SetActive(active);
                button.onClick.RemoveAllListeners();
                if (!active)
                    continue;

                UdderPower power = choices[i];
                TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                label.text = $"{UdderPlayer.GetPowerLabel(power).ToUpperInvariant()}\nLV {player.GetPowerLevel(power)} > {Mathf.Min(10, player.GetPowerLevel(power) + 1)}";
                button.onClick.AddListener(() => powerChoiceCallback?.Invoke(power));
                ConfigureHoverDescription(button, power);
            }
        }

        public void HidePowerChoices()
        {
            powerChoicePanel.SetActive(false);
            if (powerDescriptionPanel)
                powerDescriptionPanel.SetActive(false);
            powerChoiceCallback = null;
        }

        private void ConfigureHoverDescription(Button button, UdderPower power)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (!trigger)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            trigger.triggers.Clear();

            EventTrigger.Entry enter = new() { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => ShowPowerDescription(power));
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new() { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => HidePowerDescription());
            trigger.triggers.Add(exit);
        }

        private void ShowPowerDescription(UdderPower power)
        {
            EnsurePowerDescriptionPanel();
            powerDescriptionTitle.text = UdderPlayer.GetPowerLabel(power).ToUpperInvariant();
            powerDescriptionBody.text = UdderPlayer.GetPowerDescription(power);
            powerDescriptionPanel.SetActive(true);
        }

        private void HidePowerDescription()
        {
            if (powerDescriptionPanel)
                powerDescriptionPanel.SetActive(false);
        }

        private void EnsurePowerDescriptionPanel()
        {
            if (powerDescriptionPanel || !powerChoicePanel)
                return;

            powerDescriptionPanel = new GameObject("Power Description Panel");
            powerDescriptionPanel.transform.SetParent(powerChoicePanel.transform, false);
            var panelRect = powerDescriptionPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 1f);
            panelRect.anchorMax = new Vector2(0.5f, 1f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 18f);
            panelRect.sizeDelta = new Vector2(560f, 148f);

            var panelImage = powerDescriptionPanel.AddComponent<Image>();
            ApplyWoodenPanel(panelImage, 0.96f);

            powerDescriptionTitle = CreateDescriptionText(powerDescriptionPanel.transform, "POWER", new Vector2(52f, -30f), 20, TextAlignmentOptions.Left);
            powerDescriptionTitle.color = new Color(1f, 0.86f, 0.12f);
            powerDescriptionTitle.rectTransform.sizeDelta = new Vector2(456f, 28f);

            powerDescriptionBody = CreateDescriptionText(powerDescriptionPanel.transform, "DESCRIPTION", new Vector2(52f, -62f), 14, TextAlignmentOptions.TopLeft);
            powerDescriptionBody.rectTransform.sizeDelta = new Vector2(456f, 54f);
            powerDescriptionBody.textWrappingMode = TextWrappingModes.Normal;
            powerDescriptionPanel.SetActive(false);
        }

        private TMP_Text CreateDescriptionText(Transform parent, string text, Vector2 anchoredPosition, int size, TextAlignmentOptions alignment)
        {
            GameObject textObject = new("Description " + text);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;

            var label = textObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset resolvedFont = font ? font : statusText ? statusText.font : null;
            if (resolvedFont)
                label.font = resolvedFont;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            return label;
        }

        public void ApplyWoodenSkin()
        {
            if (powerChoicePanel && powerChoicePanel.TryGetComponent(out Image powerPanelImage))
                ApplyWoodenPanel(powerPanelImage, 0.96f);

            foreach (Button button in powerChoiceButtons)
                ApplyWoodenButton(button);

            if (healthFill && healthFill.transform.parent && healthFill.transform.parent.TryGetComponent(out Image healthBack))
                ApplyWoodenBar(healthBack);

            if (bovinityFill && bovinityFill.transform.parent && bovinityFill.transform.parent.TryGetComponent(out Image bovinityBack))
                ApplyWoodenBar(bovinityBack);
        }

        private void ApplyWoodenButton(Button button)
        {
            if (!button || !button.TryGetComponent(out Image image))
                return;

            image.sprite = buttonSprite;
            image.type = buttonSprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = buttonSprite ? Color.white : new Color(1f, 0.86f, 0.12f, 0.92f);
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.02f, 0.88f, 1f);
            colors.pressedColor = new Color(0.82f, 0.74f, 0.58f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            if (buttonPressedSprite)
            {
                SpriteState state = button.spriteState;
                state.pressedSprite = buttonPressedSprite;
                state.selectedSprite = buttonSprite ? buttonSprite : image.sprite;
                button.spriteState = state;
            }
        }

        private void ApplyWoodenPanel(Image image, float alpha)
        {
            if (!image)
                return;

            image.sprite = panelSprite;
            image.type = panelSprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = panelSprite ? new Color(1f, 1f, 1f, alpha) : new Color(0f, 0f, 0f, 0.88f * alpha);
        }

        private void ApplyWoodenBar(Image image)
        {
            if (!image)
                return;

            image.sprite = barSprite ? barSprite : panelSprite;
            image.type = image.sprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = image.sprite ? Color.white : new Color(0f, 0f, 0f, 0.72f);
        }

        private static void FitBarContent(Image fill)
        {
            if (!fill)
                return;

            var fillRect = fill.rectTransform;
            fillRect.offsetMin = new Vector2(95.55f, 9.61f);
            fillRect.offsetMax = new Vector2(-29.6f, -9.62f);

            foreach (TMP_Text label in fill.transform.parent.GetComponentsInChildren<TMP_Text>(true))
            {
                var rect = label.rectTransform;
                rect.anchoredPosition = new Vector2(34f, -7f);
                rect.sizeDelta = new Vector2(-68f, 22f);
                label.fontSize = Mathf.Min(label.fontSize, 13f);
                label.color = Color.black;
            }
        }

    }
}
