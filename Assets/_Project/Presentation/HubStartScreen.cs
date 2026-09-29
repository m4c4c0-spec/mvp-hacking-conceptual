using System;
using UnityEngine;
using UnityEngine.UI;

namespace EthicalLab.Presentation
{
    /// <summary>Pantalla de entrada sobre la oficina. No modifica la partida guardada.</summary>
    public sealed class HubStartScreen : MonoBehaviour
    {
        Canvas canvas;
        Text enterLabel;
        Text progressLabel;
        Action enter;

        public bool IsVisible => canvas != null && canvas.enabled;

        public static HubStartScreen Create(Transform parent)
        {
            var go = new GameObject("Welcome to Analyst Academy", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.AddComponent<HubStartScreen>();
        }

        void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 40;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();

            var shade = Rect("Shade", transform, Vector2.zero, Vector2.one);
            shade.gameObject.AddComponent<Image>().color = new Color(0.02f, 0.04f, 0.06f, 0.3f);
            var panel = Rect("Welcome panel", transform, Vector2.zero, new Vector2(0.45f, 1));
            panel.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.055f, 0.075f, 0.96f);
            Label(panel, "ESTUDIO 01     /     FORMACIÓN DE ANALISTAS", 24, HubUiTheme.Mint, 0.82f, 0.88f);
            Label(panel, "BLUE / RED", 64, HubUiTheme.Text, 0.69f, 0.81f);
            Label(panel, "ANALYST ACADEMY", 25, HubUiTheme.Muted, 0.63f, 0.7f);
            Label(panel, "Tu primer día empieza aquí.", 30, HubUiTheme.Text, 0.51f, 0.59f);
            Label(panel, "Explora la oficina, investiga casos y convierte cada evidencia en una decisión informada.", 23, HubUiTheme.Muted, 0.39f, 0.51f);
            enterLabel = Button(panel, "Entrar a la oficina", 0.27f, 0.35f, HubUiTheme.Mint, () =>
            {
                if (!IsVisible) return;
                canvas.enabled = false;
                enter?.Invoke();
            });
            progressLabel = Label(panel, "", 19, HubUiTheme.Muted, 0.21f, 0.26f);
            Button(panel, "Salir del juego", 0.11f, 0.18f, HubUiTheme.Raised, Quit);
            Label(panel, "EXPERIENCIA INDIVIDUAL  /  GUARDADO LOCAL", 16, HubUiTheme.Muted, 0.04f, 0.09f);

            var controls = Rect("Controls card", transform, new Vector2(0.64f, 0.06f), new Vector2(0.96f, 0.3f));
            controls.gameObject.AddComponent<Image>().color = new Color(0.035f, 0.055f, 0.075f, 0.9f);
            Label(controls, "MUÉVETE POR TU ESPACIO", 20, HubUiTheme.Mint, 0.69f, 0.91f);
            Label(controls, "WASD  Caminar       MOUSE  Mirar\nE  Interactuar          G  Tomar / devolver\nSHIFT  Correr          ESC  Menú y cursor", 21, HubUiTheme.Text, 0.13f, 0.68f);
        }

        public void Show(bool hasProgress, int completed, int total, Action onEnter)
        {
            enter = onEnter;
            enterLabel.text = hasProgress ? "Continuar en la oficina   →" : "Entrar a la oficina   →";
            progressLabel.text = hasProgress ? completed + " / " + total + " casos cerrados · progreso recuperado" : "Tu primera misión te espera en la pizarra.";
            canvas.enabled = true;
        }

        static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        static Text Label(Transform parent, string text, int size, Color color, float bottom, float top)
        {
            var rt = Rect(text, parent, new Vector2(0.09f, bottom), new Vector2(0.92f, top));
            var label = rt.gameObject.AddComponent<Text>();
            label.font = HubOffice.Font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAnchor.MiddleLeft;
            label.raycastTarget = false;
            return label;
        }

        static Text Button(Transform parent, string text, float bottom, float top, Color color, Action action)
        {
            var rt = Rect(text, parent, new Vector2(0.09f, bottom), new Vector2(0.92f, top));
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.85f, 0.95f, 0.92f);
            colors.pressedColor = new Color(0.65f, 0.8f, 0.76f);
            button.colors = colors;
            button.onClick.AddListener(() => action());
            return Label(rt, text, 24, color == HubUiTheme.Mint ? HubUiTheme.Ink : HubUiTheme.Text, 0, 1);
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}
