using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>Texto del mundo con oclusión y límites físicos de legibilidad.</summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMesh))]
    public sealed class HubWorldLabel : MonoBehaviour
    {
        public float width = 3f;
        public float height = 0.65f;
        public float characterSize = 0.02f;
        static Material shared;
        TextMesh text;
        Renderer surface;
        string previous;

        void OnEnable()
        {
            text = GetComponent<TextMesh>();
            surface = GetComponent<Renderer>();
            Font.textureRebuilt += OnFontRebuilt;
            RefreshMaterial();
            previous = null;
        }

        void OnDisable() => Font.textureRebuilt -= OnFontRebuilt;

        void OnFontRebuilt(Font font)
        {
            if (text != null && text.font == font) RefreshMaterial();
        }

        void RefreshMaterial()
        {
            if (text == null || text.font == null || surface == null) return;
            if (shared == null)
                shared = new Material(Resources.Load<Shader>("WorldText")) { name = "World signage" };
            shared.mainTexture = text.font.material.mainTexture;
            surface.sharedMaterial = shared;
        }

        void OnWillRenderObject() => RefreshMaterial();

        void LateUpdate()
        {
            if (text == null || surface == null) return;
            // Normaliza escala incluso en props con padres escalados.
            var scale = transform.lossyScale;
            if (scale.x > 0 && scale.y > 0 && scale.z > 0)
                transform.localScale = Vector3.Scale(transform.localScale, new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z));
            if (previous == text.text && UnityEngine.Application.isPlaying) return;
            previous = text.text;
            // Las bounds del Renderer llegan un frame tarde al cambiar el texto.
            // Usar métricas de fuente permite ajustar también el primer frame.
            if (text.font == null) return;
            string value = text.text ?? "";
            text.font.RequestCharactersInTexture(value, text.fontSize, text.fontStyle);
            float widest = 0f;
            float line = 0f;
            int lines = 1;
            foreach (char letter in value)
            {
                if (letter == '\n')
                {
                    widest = Mathf.Max(widest, line);
                    line = 0;
                    lines++;
                }
                else if (text.font.GetCharacterInfo(letter, out var info, text.fontSize, text.fontStyle))
                    line += info.advance;
            }
            widest = Mathf.Max(widest, line);
            float measuredWidth = widest * characterSize * 0.1f;
            float measuredHeight = lines * text.fontSize * text.lineSpacing * characterSize * 0.1f;
            float fit = Mathf.Min(1f, width / Mathf.Max(0.001f, measuredWidth), height / Mathf.Max(0.001f, measuredHeight));
            text.characterSize = characterSize * fit;
        }

        public void FitNow()
        {
            previous = null;
            LateUpdate();
        }
    }
}
