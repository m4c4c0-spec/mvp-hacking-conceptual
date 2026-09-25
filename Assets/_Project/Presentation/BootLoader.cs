using UnityEngine;
using UnityEngine.SceneManagement;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Splash mínimo en Boot → SceneManager carga Hub. Preferible Play en Hub.unity para saltar el splash.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        const string Splash = "Cargando oficina…";
        float untilLoad;
        bool loading;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (SceneManager.GetActiveScene().name != "Boot") return;
            if (FindFirstObjectByType<BootLoader>() != null) return;
            new GameObject("Boot Loader").AddComponent<BootLoader>();
        }

        void Awake()
        {
            untilLoad = Time.unscaledTime + 0.55f;
        }

        void Update()
        {
            if (loading) return;
            if (Time.unscaledTime < untilLoad) return;
            loading = true;
            SceneManager.LoadScene("Hub");
        }

        void OnGUI()
        {
            float w = Mathf.Min(420f, Screen.width - 48f);
            float h = 52f;
            float x = (Screen.width - w) * 0.5f;
            float y = (Screen.height - h) * 0.5f;
            GUI.Box(new Rect(x, y, w, h), "");
            GUI.Label(new Rect(x + 16, y + 14, w - 32, h - 20), Splash);
        }
    }
}
