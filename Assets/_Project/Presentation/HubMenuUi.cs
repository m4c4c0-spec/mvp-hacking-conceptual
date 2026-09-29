using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EthicalLab.Presentation
{
    /// <summary>Menú ESC (Misiones, Terminal, etc.) en uGUI + TMP.</summary>
    public sealed class HubMenuUi : MonoBehaviour
    {
        public event Action<string> TabClicked;
        public event Action BackToOffice;

        Canvas canvas;
        RectTransform contentRoot;
        TextMeshProUGUI statusLine;
        string activeTab = "home";

        public static HubMenuUi Create(Transform parent)
        {
            var go = new GameObject("Hub menu UI");
            go.transform.SetParent(parent, false);
            return go.AddComponent<HubMenuUi>();
        }

        void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();

            var bg = Panel(canvas.transform, "Menu bg", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.GetComponent<Image>().color = HubUiTheme.Ink;

            var tabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            tabs.transform.SetParent(bg.transform, false);
            var tabsRt = tabs.GetComponent<RectTransform>();
            tabsRt.anchorMin = new Vector2(0, 1);
            tabsRt.anchorMax = new Vector2(1, 1);
            tabsRt.pivot = new Vector2(0.5f, 1);
            tabsRt.anchoredPosition = new Vector2(0, -16);
            tabsRt.sizeDelta = new Vector2(-80, 48);
            var h = tabs.GetComponent<HorizontalLayoutGroup>();
            h.spacing = 8;
            h.childControlWidth = h.childControlHeight = true;
            h.childForceExpandWidth = true;

            AddTabButton(tabs.transform, "Inicio", "home");
            AddTabButton(tabs.transform, "Misiones", "missions");
            AddTabButton(tabs.transform, "Terminal", "terminal");
            AddTabButton(tabs.transform, "Expediente", "case");
            AddTabButton(tabs.transform, "Glosario", "glossary");
            AddTabButton(tabs.transform, "Oficina", "office");

            statusLine = Label(bg.transform, "Status", new Vector2(40, -72), new Vector2(1800, 32), 20);
            var viewport = Panel(bg.transform, "Viewport", Vector2.zero, Vector2.one, new Vector2(40, 40), new Vector2(-40, -120));
            viewport.AddComponent<RectMask2D>();
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            content.transform.SetParent(viewport.transform, false);
            contentRoot = content.GetComponent<RectTransform>();
            contentRoot.anchorMin = new Vector2(0, 1);
            contentRoot.anchorMax = Vector2.one;
            contentRoot.pivot = new Vector2(0.5f, 1);
            contentRoot.sizeDelta = Vector2.zero;
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12;
            layout.padding = new RectOffset(16, 16, 16, 16);
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var scroll = viewport.AddComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRoot;
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 32;
        }

        void AddTabButton(Transform parent, string label, string id)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = HubUiTheme.Raised;
            var txt = Label(go.transform, "T", Vector2.zero, new Vector2(200, 40), 18);
            txt.text = label;
            txt.alignment = TextAlignmentOptions.Center;
            Stretch(txt.rectTransform, 4);
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                activeTab = id;
                if (id == "office") BackToOffice?.Invoke();
                else TabClicked?.Invoke(id);
            });
        }

        public void SetVisible(bool on) => canvas.enabled = on;

        public void SetStatus(string text) => statusLine.text = text ?? "";

        public void ClearContent()
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                contentRoot.GetChild(i).gameObject.SetActive(false);
                Destroy(contentRoot.GetChild(i).gameObject);
            }
            contentRoot.anchoredPosition = Vector2.zero;
        }

        public void AddHeading(string text) => AddLabel(text, 26, FontStyles.Bold);

        public void AddBody(string text) => AddLabel(text, 20, FontStyles.Normal);

        public void AddButton(string label, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(contentRoot, false);
            go.GetComponent<Image>().color = HubUiTheme.Mint;
            go.GetComponent<LayoutElement>().minHeight = 44;
            var tmp = Label(go.transform, "L", Vector2.zero, new Vector2(400, 40), 20);
            tmp.text = label;
            tmp.color = HubUiTheme.Ink;
            tmp.alignment = TextAlignmentOptions.Center;
            Stretch(tmp.rectTransform, 8);
            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        }

        public TMP_InputField AddTerminalInput(string initial)
        {
            var go = new GameObject("Terminal input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
            go.transform.SetParent(contentRoot, false);
            go.GetComponent<Image>().color = HubUiTheme.Raised;
            go.GetComponent<LayoutElement>().minHeight = 40;
            var text = Label(go.transform, "Text", Vector2.zero, new Vector2(400, 36), 18);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            Stretch(text.rectTransform, 8);
            var input = go.GetComponent<TMP_InputField>();
            input.targetGraphic = go.GetComponent<Image>();
            input.textViewport = go.GetComponent<RectTransform>();
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.textComponent = text;
            text.raycastTarget = false;
            go.AddComponent<RectMask2D>();
            input.text = initial ?? "";
            return input;
        }

        public void AddSlider(string label, float value, float min, float max, Action<float> changed, bool wholeNumbers = false)
        {
            var row = new GameObject(label, typeof(RectTransform), typeof(LayoutElement));
            row.transform.SetParent(contentRoot, false);
            row.GetComponent<LayoutElement>().minHeight = 74;
            var caption = Label(row.transform, "Value", Vector2.zero, new Vector2(1000, 28), 20);
            caption.raycastTarget = false;
            void RefreshCaption(float v) => caption.text = label + " · " + v.ToString(wholeNumbers ? "0" : "0.00");
            RefreshCaption(value);
            var track = Panel(row.transform, "Track", new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 8), new Vector2(0, 32));
            track.GetComponent<Image>().color = HubUiTheme.Raised;
            var slider = track.AddComponent<Slider>();
            var area = new GameObject("Handle area", typeof(RectTransform));
            area.transform.SetParent(track.transform, false);
            Stretch(area.GetComponent<RectTransform>(), 0);
            area.GetComponent<RectTransform>().offsetMin = new Vector2(12, 0);
            area.GetComponent<RectTransform>().offsetMax = new Vector2(-12, 0);
            var handle = Panel(area.transform, "Handle", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            handle.GetComponent<Image>().color = HubUiTheme.Mint;
            var rt = handle.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(24, 0);
            slider.handleRect = rt;
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = wholeNumbers;
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => { RefreshCaption(v); changed?.Invoke(v); });
        }

        void AddLabel(string text, float size, FontStyles style)
        {
            var tmp = Label(contentRoot, "Line", Vector2.zero, new Vector2(1600, 32), size);
            tmp.fontStyle = style;
            tmp.text = text;
            var le = tmp.gameObject.AddComponent<LayoutElement>();
            le.minHeight = size + 12;
        }

        static GameObject Panel(Transform parent, string name, Vector2 amin, Vector2 amax, Vector2 offMin, Vector2 offMax)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = amin;
            rt.anchorMax = amax;
            rt.offsetMin = offMin;
            rt.offsetMax = offMax;
            go.GetComponent<Image>().color = HubUiTheme.Panel;
            return go;
        }

        static TextMeshProUGUI Label(Transform parent, string name, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            if (parent is RectTransform prt && prt.GetComponent<VerticalLayoutGroup>() != null)
            {
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.sizeDelta = new Vector2(0, size.y);
            }
            else
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                rt.anchoredPosition = pos;
                rt.sizeDelta = size;
            }
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            tmp.color = HubUiTheme.Text;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        static void Stretch(RectTransform rt, float pad)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }
    }
}
