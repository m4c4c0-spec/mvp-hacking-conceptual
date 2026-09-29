using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Material de cell shading para el pipeline integrado.
    /// El shader está en Resources: las manos se crean en runtime y el player debe incluirlo.
    /// </summary>
    public static class CelShading
    {
        public static Material Create(string name, Color color, float outline = 0.0011f, float rim = 0.1f)
        {
            Shader shader = Resources.Load<Shader>("CelToon");
            if (shader == null) shader = Shader.Find("EthicalLab/Cel");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            var material = new Material(shader) { name = name, color = color };
            if (material.HasProperty("_Steps")) material.SetFloat("_Steps", 3f);
            if (material.HasProperty("_OutlineWidth")) material.SetFloat("_OutlineWidth", outline);
            if (material.HasProperty("_Rim")) material.SetFloat("_Rim", rim);
            // Only clothing receives the authored fabric. Skin, eyes, hair, shoes and
            // first-person hands retain their original flat pigments and outlines.
            string key = name.ToLowerInvariant();
            if (key.Contains("jacket") || key.Contains("shirt") || key.Contains("trousers"))
            {
                var fabric = Resources.Load<Texture2D>("OfficeTextures/fabric");
                if (fabric != null && material.HasProperty("_MainTex"))
                {
                    material.mainTexture = fabric;
                    if (material.HasProperty("_TextureScale"))
                        material.SetFloat("_TextureScale", key.Contains("shirt") ? 6f : 4f);
                    if (material.HasProperty("_TextureStrength"))
                        material.SetFloat("_TextureStrength", key.Contains("shirt") ? 0.38f : 0.58f);
                }
            }
            if (material.HasProperty("_OutlineColor"))
            {
                // Sombra del propio pigmento (ocre, roble, menta), no un negro de contorno.
                material.SetColor("_OutlineColor", new Color(
                    Mathf.Clamp01(color.r * 0.34f + 0.03f),
                    Mathf.Clamp01(color.g * 0.28f + 0.02f),
                    Mathf.Clamp01(color.b * 0.22f + 0.015f),
                    1f));
            }
            return material;
        }
    }
}
