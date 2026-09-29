using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>Acabados del entorno. Personajes y manos conservan su shader cel independiente.</summary>
    public static class OfficeSurface
    {
        public static Material Create(string name, Color color)
        {
            var shader = Resources.Load<Shader>("OfficeSurface");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader) { name = name, color = color, enableInstancing = true };
            string key = name.ToLowerInvariant();
            float pattern = 0, strength = 0, smoothness = 0.32f, metal = 0;
            float textureScale = 1, textureStrength = 1;
            if (key.Contains("oak") || key.Contains("walnut"))
            { pattern = 1; strength = 0.16f; smoothness = 0.38f; textureScale = 1.25f; textureStrength = 0.9f; }
            else if (key.Contains("carpet") || key.Contains("upholstery"))
            { pattern = 2; strength = 0.12f; smoothness = 0.08f; textureScale = 4; textureStrength = 0.7f; }
            else if (key.Contains("plaster"))
            { pattern = 3; strength = 0.045f; smoothness = 0.16f; textureStrength = 0.45f; }
            else if (key.Contains("steel") || key.Contains("graphite"))
            { metal = 0.5f; smoothness = 0.52f; }
            else if (key.Contains("glazing")) smoothness = 0.7f;
            if (material.HasProperty("_Pattern")) material.SetFloat("_Pattern", pattern);
            if (material.HasProperty("_GrainStrength")) material.SetFloat("_GrainStrength", strength);
            if (material.HasProperty("_TextureStrength")) material.SetFloat("_TextureStrength", 0);
            material.SetFloat("_Glossiness", smoothness);
            material.SetFloat("_Metallic", metal);
            string textureName = pattern == 1 ? "oak" : pattern == 2 ? "fabric" : pattern == 3 ? "plaster" : null;
            // Recursos opcionales revisados: el ejecutable no llama a servicios de IA.
            if (textureName != null)
            {
                var texture = Resources.Load<Texture2D>("OfficeTextures/" + textureName);
                if (texture != null)
                {
                    material.mainTexture = texture;
                    if (material.HasProperty("_GrainStrength")) material.SetFloat("_GrainStrength", strength * 0.3f);
                    if (material.HasProperty("_TextureScale")) material.SetFloat("_TextureScale", textureScale);
                    if (material.HasProperty("_TextureStrength")) material.SetFloat("_TextureStrength", textureStrength);
                }
            }
            return material;
        }
    }
}
