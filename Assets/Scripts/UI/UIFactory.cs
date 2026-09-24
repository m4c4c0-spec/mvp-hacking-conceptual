using System;
using UnityEngine;
using UnityEngine.UI;

namespace Academy
{
    public static class UIFactory
    {
        public static readonly Color Ink = new Color(.035f, .055f, .085f, .98f);
        public static readonly Color Panel = new Color(.065f, .095f, .135f, .98f);
        public static readonly Color Raised = new Color(.105f, .15f, .19f);
        public static readonly Color Mint = new Color(.43f, .91f, .76f);
        public static readonly Color White = new Color(.91f, .94f, .94f);
        public static readonly Color Muted = new Color(.63f, .71f, .76f);
        public static readonly Color Amber = new Color(.98f, .75f, .4f);
        private static Font font;
        public static Font Font => font != null ? font : (font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
        public static void Fill(RectTransform rect, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(-right, -top);
        }
        public static void Size(Transform t, float height, float width = -1)
        {
            var layout = t.gameObject.GetComponent<LayoutElement>() ?? t.gameObject.AddComponent<LayoutElement>();
            if (height >= 0) { layout.minHeight = height; layout.preferredHeight = height; }
            if (width >= 0) { layout.minWidth = width; layout.preferredWidth = width; }
        }
        public static Image Background(RectTransform rect, Color color)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; return image;
        }
        public static RectTransform Column(string name, Transform parent, int padding = 0, int spacing = 12, Color? color = null)
        {
            var rect = Rect(name, parent);
            if (color.HasValue) Background(rect, color.Value);
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding); layout.spacing = spacing;
            layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            return rect;
        }
        public static RectTransform Row(string name, Transform parent, int spacing = 12)
        {
            var rect = Rect(name, parent); var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = spacing; layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
            return rect;
        }
        public static Text Label(Transform parent, string value, int size = 22, Color? color = null, bool bold = false)
        {
            var rect = Rect("Text", parent); var text = rect.gameObject.AddComponent<Text>();
            text.font = Font; text.text = value; text.fontSize = size; text.color = color ?? White;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false; text.lineSpacing = 1.12f;
            return text;
        }
        public static Button Button(Transform parent, string label, Action action, bool primary = false, bool enabled = true, float height = 54)
        {
            var rect = Rect(label, parent); var image = Background(rect, primary ? Mint : Raised);
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(.85f, 1, .96f); colors.pressedColor = new Color(.65f, .8f, .75f);
            colors.disabledColor = new Color(.42f, .46f, .48f, .65f); button.colors = colors;
            button.interactable = enabled; button.onClick.AddListener(() => action?.Invoke()); Size(rect, height);
            var text = Label(rect, label, 20, primary ? Ink : White, primary);
            Fill(text.rectTransform, 16, 7, 16, 7); text.alignment = TextAnchor.MiddleLeft;
            return button;
        }
        public static InputField Input(Transform parent, string placeholder, string value, bool multi = false, int limit = 4000)
        {
            var rect = Rect("Input", parent); Background(rect, Ink); Size(rect, multi ? 235 : 58);
            var input = rect.gameObject.AddComponent<InputField>();
            var text = Label(rect, value, 22); Fill(text.rectTransform, 16, 12, 16, 12); text.supportRichText = false;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            var hint = Label(rect, placeholder, 21, Muted); Fill(hint.rectTransform, 16, 12, 16, 12);
            input.textComponent = text; input.placeholder = hint; input.text = value;
            input.lineType = multi ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
            input.characterLimit = limit; input.customCaretColor = true; input.caretColor = Mint;
            input.selectionColor = new Color(.3f, .7f, .65f, .4f);
            return input;
        }
        public static RectTransform Scroll(Transform parent, out ScrollRect scroll)
        {
            var root = Rect("Scroll", parent); Fill(root);
            scroll = root.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 42;
            var viewport = Rect("Viewport", root); Fill(viewport, 0, 0, 14, 0);
            viewport.gameObject.AddComponent<RectMask2D>(); Background(viewport, new Color(0, 0, 0, .001f));
            var content = Column("Content", viewport, 28, 18);
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero; content.sizeDelta = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            var track = Rect("Scrollbar", root); track.anchorMin = new Vector2(1, 0); track.anchorMax = Vector2.one;
            track.offsetMin = new Vector2(-10, 8); track.offsetMax = new Vector2(-2, -8); Background(track, Ink);
            var thumb = Rect("Thumb", track); Fill(thumb); var image = Background(thumb, Muted);
            var scrollbar = track.gameObject.AddComponent<Scrollbar>(); scrollbar.handleRect = thumb;
            scrollbar.targetGraphic = image; scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar = scrollbar;
            return content;
        }
        public static void Clear(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i).gameObject; child.SetActive(false); UnityEngine.Object.Destroy(child);
            }
        }
    }
}
