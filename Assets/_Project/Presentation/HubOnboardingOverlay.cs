using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EthicalLab.Presentation
{
    /// <summary>Onboarding profesional (18–32): qué es y qué no es el juego.</summary>
    public sealed class HubOnboardingOverlay : MonoBehaviour
    {
        public const string PrefsKey = "EthicalLab.OnboardingDismissed";
        public const string ChapterPrefsKey = "EthicalLab.Chapter1Seen";

        enum Card { None, Onboarding, Chapter }

        const string OnboardingCopy =
            "BLUE / RED · ANALYST ACADEMY\n\n" +
            "Eres un analista junior en una consultora ficticia. Investigas, documentas y defiendes conceptos de ciberseguridad.\n\n" +
            "Esto NO es un simulador de ataques reales: no hay exploits, Kali ni herramientas de intrusión.\n\n" +
            "Empieza en la pizarra: acepta un ticket y sigue las pistas en el archivo y las bandejas del escritorio.";

        const string ChapterCopy =
            "CAPÍTULO 1\n" +
            "Estudio 01 · primer día\n\n" +
            "Acabas de entrar a Analyst Academy con la credencial Blue / Red.\n\n" +
            "Blue inventaría: observas, anotas y ordenas lo que hay.\n" +
            "Red solo lee un fallo que ya ocurrió. Aquí no se ataca nada.\n" +
            "El caso se cierra con el informe.\n\n" +
            "Primer destino: la pizarra, al fondo de la oficina.\n" +
            "Hugo custodia los tickets. Acércate y pulsa E en uno disponible.\n" +
            "Vera, en recepción a tu izquierda, te orienta si lo necesitas.";

        Canvas canvas;
        RectTransform panelRt;
        TextMeshProUGUI body;
        TextMeshProUGUI buttonLabel;
        Action onDismiss;
        Card card;
        public bool IsVisible => canvas != null && canvas.enabled;

        public static HubOnboardingOverlay Create(Transform parent)
        {
            var go = new GameObject("Onboarding overlay");
            go.transform.SetParent(parent, false);
            return go.AddComponent<HubOnboardingOverlay>();
        }

        void Awake()
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 30;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();

            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(canvas.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(0, 0, 0, 0.72f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas.transform, false);
            panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(780, 640);
            panel.GetComponent<Image>().color = HubUiTheme.Panel;

            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(TextMeshProUGUI));
            bodyGo.transform.SetParent(panel.transform, false);
            Stretch(bodyGo.GetComponent<RectTransform>(), 24);
            bodyGo.GetComponent<RectTransform>().offsetMin = new Vector2(24, 100);
            body = bodyGo.GetComponent<TextMeshProUGUI>();
            body.fontSize = 22;
            body.color = HubUiTheme.Text;
            body.alignment = TextAlignmentOptions.TopLeft;
            body.raycastTarget = false;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.text = OnboardingCopy;

            var btnGo = new GameObject("Entendido", typeof(RectTransform), typeof(Image), typeof(Button));
            btnGo.transform.SetParent(panel.transform, false);
            var brt = btnGo.GetComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 0);
            brt.anchorMax = new Vector2(0.5f, 0);
            brt.pivot = new Vector2(0.5f, 0);
            brt.anchoredPosition = new Vector2(0, 24);
            brt.sizeDelta = new Vector2(280, 48);
            btnGo.GetComponent<Image>().color = HubUiTheme.Mint;
            var lbl = new GameObject("L", typeof(RectTransform), typeof(TextMeshProUGUI));
            lbl.transform.SetParent(btnGo.transform, false);
            Stretch(lbl.GetComponent<RectTransform>(), 8);
            buttonLabel = lbl.GetComponent<TextMeshProUGUI>();
            buttonLabel.text = "Continuar";
            buttonLabel.fontSize = 20;
            buttonLabel.color = HubUiTheme.Ink;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            btnGo.GetComponent<Button>().onClick.AddListener(Advance);

            canvas.enabled = false;
        }

        static void Stretch(RectTransform rt, float pad = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(pad, pad);
            rt.offsetMax = new Vector2(-pad, -pad);
        }

        public void ShowIfNeeded(Action dismissed)
        {
            onDismiss = dismissed;
            if (PlayerPrefs.GetInt(PrefsKey, 0) != 1)
            {
                Present(Card.Onboarding);
                return;
            }
            ShowChapterOrFinish();
        }

        void Advance()
        {
            if (card == Card.Onboarding)
            {
                PlayerPrefs.SetInt(PrefsKey, 1);
                PlayerPrefs.Save();
                ShowChapterOrFinish();
                return;
            }
            if (card == Card.Chapter)
            {
                PlayerPrefs.SetInt(ChapterPrefsKey, 1);
                PlayerPrefs.Save();
            }
            card = Card.None;
            canvas.enabled = false;
            onDismiss?.Invoke();
        }

        void ShowChapterOrFinish()
        {
            if (PlayerPrefs.GetInt(ChapterPrefsKey, 0) == 1)
            {
                card = Card.None;
                canvas.enabled = false;
                onDismiss?.Invoke();
                return;
            }
            Present(Card.Chapter);
        }

        void Present(Card next)
        {
            card = next;
            if (next == Card.Chapter)
            {
                panelRt.sizeDelta = new Vector2(780, 640);
                body.text = ChapterCopy;
                buttonLabel.text = "Entrar a la oficina";
            }
            else
            {
                panelRt.sizeDelta = new Vector2(760, 520);
                body.text = OnboardingCopy;
                buttonLabel.text = "Continuar";
            }
            canvas.enabled = true;
        }
    }
}
