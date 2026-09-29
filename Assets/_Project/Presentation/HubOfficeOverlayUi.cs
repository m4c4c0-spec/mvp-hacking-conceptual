using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EthicalLab.Presentation
{
    /// <summary>HUD de oficina (crosshair, guía, toast, lectura) en Canvas overlay.</summary>
    public sealed class HubOfficeOverlayUi : MonoBehaviour
    {
        Canvas canvas;
        Image crossH;
        Image crossV;
        TextMeshProUGUI prompt;
        TextMeshProUGUI controls;
        TextMeshProUGUI guide;
        GameObject toastRoot;
        TextMeshProUGUI toast;
        RectTransform toastPanel;
        GameObject readingRoot;
        TextMeshProUGUI reading;

        public static HubOfficeOverlayUi Create(Transform parent)
        {
            var go = new GameObject("Office overlay UI");
            go.transform.SetParent(parent, false);
            return go.AddComponent<HubOfficeOverlayUi>();
        }

        void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();

            crossH = Bar("Cross H", 0.5f, 0.5f, new Vector2(16, 2));
            crossV = Bar("Cross V", 0.5f, 0.5f, new Vector2(2, 16));
            controls = Label("Controls", 0, 1, new Vector2(24, -24), new Vector2(900, 28), 18, TextAlignmentOptions.TopLeft);
            guide = Label("Guide", 0, 1, new Vector2(24, -120), new Vector2(780, 108), 20, TextAlignmentOptions.TopLeft);
            prompt = Label("Prompt", 0, 0, new Vector2(24, 24), new Vector2(1800, 56), 22, TextAlignmentOptions.BottomLeft);

            readingRoot = Panel("Reading", 1, 1, new Vector2(-24, -24), new Vector2(460, 600));
            reading = Label("Reading text", 0, 1, Vector2.zero, new Vector2(420, 560), 20, TextAlignmentOptions.TopLeft);
            reading.transform.SetParent(readingRoot.transform, false);
            Stretch(reading.rectTransform, 16);

            toastRoot = Panel("Toast", 0.5f, 1, new Vector2(0, -24), new Vector2(680, 96));
            toastPanel = toastRoot.GetComponent<RectTransform>();
            toast = Label("Toast text", 0.5f, 1, Vector2.zero, new Vector2(640, 80), 22, TextAlignmentOptions.Top);
            toast.transform.SetParent(toastRoot.transform, false);
            Stretch(toast.rectTransform, 12);
        }

        static void Stretch(RectTransform rt, float pad)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        Image Bar(string name, float ax, float ay, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = HubUiTheme.Text;
            go.GetComponent<Image>().raycastTarget = false;
            return go.GetComponent<Image>();
        }

        TextMeshProUGUI Label(string name, float ax, float ay, Vector2 pos, Vector2 size, float fontSize, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(ax < 0.5f ? 0f : ax > 0.5f ? 1f : 0.5f, ay > 0.5f ? 1f : 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = HubUiTheme.Text;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            return tmp;
        }

        GameObject Panel(string name, float ax, float ay, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(ax, ay > 0.5f ? 1f : 0f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = HubUiTheme.Panel;
            go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        public void SetVisible(bool on) => canvas.enabled = on;

        public void Refresh(string promptText, string controlsText, string guideText, string toastText, bool showToast, string readingText, bool showControls)
        {
            prompt.text = promptText ?? "";
            controls.text = showControls ? controlsText ?? "" : "H · ayuda";
            guide.text = string.IsNullOrEmpty(guideText) ? "" : "→ " + guideText;
            guide.gameObject.SetActive(!string.IsNullOrEmpty(guideText));
            reading.text = readingText ?? "";
            readingRoot.SetActive(!string.IsNullOrEmpty(readingText));
            toast.text = toastText ?? "";
            bool toastOn = showToast && !string.IsNullOrEmpty(toastText);
            toastRoot.SetActive(toastOn);
            if (toastOn && toastPanel != null)
            {
                float height = ToastPanelHeight(toastText);
                var size = toastPanel.sizeDelta;
                if (Mathf.Abs(size.y - height) > 0.5f)
                    toastPanel.sizeDelta = new Vector2(size.x, height);
            }
        }

        static float ToastPanelHeight(string text)
        {
            int lines = 1;
            int column = 0;
            const int wrap = 46;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    lines++;
                    column = 0;
                    continue;
                }
                column++;
                if (column >= wrap)
                {
                    lines++;
                    column = 0;
                }
            }
            return Mathf.Clamp(28f + lines * 30f, 96f, 280f);
        }
    }
}
