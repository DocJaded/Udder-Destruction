using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UdderDestruction
{
    public sealed class UdderHud : MonoBehaviour
    {
        public UdderGameController game;
        public UdderPlayer player;
        public TMP_FontAsset font;

        private Image healthFill;
        private Image bovinityFill;
        private Image milkFill;
        private TMP_Text statusText;
        private TMP_Text hintText;

        private void Start()
        {
            BuildHud();
        }

        private void Update()
        {
            if (!game || !player)
                return;

            healthFill.fillAmount = player.Health01;
            bovinityFill.fillAmount = player.Bovinity01;
            milkFill.fillAmount = player.Milk01;
            statusText.text = $"WAVE {game.Wave}   {game.WaveStatusText}   URCHINS {game.GroundedSeaUrchins}/5   CREAM {game.Cream}   CHEESE {player.cheeseSaves}   {player.milkMode}";
        }

        private void BuildHud()
        {
            GameObject canvasObject = new("Udder HUD Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            healthFill = CreateBar(canvasObject.transform, "HEALTH", new Vector2(22f, -22f), new Color(0.9f, 0.04f, 0.04f), 1f);
            bovinityFill = CreateBar(canvasObject.transform, "BOVINITY", new Vector2(22f, -58f), new Color(1f, 0.86f, 0.08f), 0f);
            milkFill = CreateBar(canvasObject.transform, "MILK", new Vector2(22f, -94f), new Color(0.9f, 0.95f, 1f), 1f);
            statusText = CreateText(canvasObject.transform, "STATUS", new Vector2(22f, -134f), 18, TextAlignmentOptions.Left);
            hintText = CreateText(canvasObject.transform, "WASD/ARROWS MOVE. ATTACKS FIRE ON THEIR OWN TIMERS.", new Vector2(22f, 24f), 15, TextAlignmentOptions.Left);
            hintText.rectTransform.anchorMin = new Vector2(0f, 0f);
            hintText.rectTransform.anchorMax = new Vector2(0f, 0f);
            hintText.rectTransform.pivot = new Vector2(0f, 0f);
        }

        private Image CreateBar(Transform parent, string label, Vector2 anchoredPosition, Color fillColor, float fillAmount)
        {
            GameObject back = new(label + " Back");
            back.transform.SetParent(parent, false);
            var backRect = back.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = anchoredPosition;
            backRect.sizeDelta = new Vector2(300f, 26f);
            var backImage = back.AddComponent<Image>();
            backImage.color = new Color(0f, 0f, 0f, 0.72f);

            GameObject fill = new(label + " Fill");
            fill.transform.SetParent(back.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(3f, 3f);
            fillRect.offsetMax = new Vector2(-3f, -3f);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillAmount = fillAmount;

            TMP_Text text = CreateText(back.transform, label, new Vector2(8f, -4f), 16, TextAlignmentOptions.Left);
            text.rectTransform.anchorMin = new Vector2(0f, 1f);
            text.rectTransform.anchorMax = new Vector2(1f, 1f);
            text.rectTransform.sizeDelta = new Vector2(0f, 24f);
            return fillImage;
        }

        private TMP_Text CreateText(Transform parent, string text, Vector2 anchoredPosition, int size, TextAlignmentOptions alignment)
        {
            GameObject textObject = new("HUD " + text);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(1120f, 34f);

            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.text = text;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = Color.black;
            return tmp;
        }
    }
}
