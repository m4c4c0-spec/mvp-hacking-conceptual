using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Carga asíncrona de la oficina, con estado visible y diagnóstico si falta la escena.
    /// </summary>
    public sealed class BootLoader : MonoBehaviour
    {
        AsyncOperation loading;
        string error;
        GUIStyle titleStyle;
        GUIStyle bodyStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RegisterScenes()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene loaded, LoadSceneMode mode) => AutoStart();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (SceneManager.GetActiveScene().name != "Boot") return;
            if (FindFirstObjectByType<BootLoader>() != null) return;
            new GameObject("Boot Loader").AddComponent<BootLoader>();
        }

        void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            var camera = new GameObject("Boot camera").AddComponent<Camera>();
            camera.transform.SetParent(transform, false);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = HubOffice.Navy;
            camera.cullingMask = 0;
        }

        IEnumerator Start()
        {
            yield return null;
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded("Hub"))
            {
                error = "No se encontró la oficina. Activa Assets/Scenes/Hub.unity en Build Profiles.";
                yield break;
            }
            loading = SceneManager.LoadSceneAsync("Hub");
            loading.allowSceneActivation = false;
            float revealAt = Time.unscaledTime + 0.6f;
            while (loading.progress < 0.9f || Time.unscaledTime < revealAt) yield return null;
            loading.allowSceneActivation = true;
        }

        void OnGUI()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 30, alignment = TextAnchor.MiddleCenter };
                titleStyle.normal.textColor = HubOffice.Paper;
                bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                bodyStyle.normal.textColor = HubOffice.Paper;
            }
            float w = Mathf.Min(560f, Screen.width - 48f);
            float x = (Screen.width - w) * 0.5f;
            float y = Screen.height * 0.42f;
            GUI.Label(new Rect(x, y, w, 48), "BLUE / RED", titleStyle);
            GUI.Label(new Rect(x, y + 48, w, 35), "ANALYST ACADEMY", bodyStyle);
            GUI.Label(new Rect(x, y + 106, w, 65), error ?? "Preparando tu oficina…", bodyStyle);
            GUI.color = HubOffice.Slate;
            GUI.DrawTexture(new Rect(x, y + 92, w, 3), Texture2D.whiteTexture);
            GUI.color = HubOffice.Mint;
            float progress = loading == null ? 0.05f : Mathf.Clamp01(loading.progress / 0.9f);
            GUI.DrawTexture(new Rect(x, y + 92, w * progress, 3), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
