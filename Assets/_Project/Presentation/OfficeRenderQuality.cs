using UnityEngine;
using UnityEngine.Rendering;

namespace EthicalLab.Presentation
{
    /// <summary>Perfil indoor compartido por escena, fallback y capturas. Sin efectos de pantalla borrosos.</summary>
    [DisallowMultipleComponent]
    public sealed class OfficeRenderQuality : MonoBehaviour
    {
        public Material sky;

        void Awake()
        {
            Apply(GetComponentInChildren<Camera>());
            if (sky != null) RenderSettings.skybox = sky;
        }

        public static void Apply(Camera camera)
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.pixelLightCount = 8;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.shadowDistance = 28f;
            QualitySettings.shadowCascades = 2;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.78f, 0.84f, 0.90f);
            RenderSettings.ambientEquatorColor = new Color(0.68f, 0.66f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.43f, 0.40f, 0.35f);
            RenderSettings.fog = false;
            if (camera == null) return;
            camera.allowMSAA = true;
            camera.allowHDR = false;
            camera.renderingPath = RenderingPath.Forward;
        }
    }
}
