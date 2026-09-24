using System;
using UnityEngine;

namespace Academy
{
    public sealed class AcademyBootstrap : MonoBehaviour
    {
        private AcademySession session;
        private AcademyUI ui;
        private float saveTimer;
        private bool hasFocus = true;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            session = new AcademySession(ContentLoader.Load(), LocalStorage.Read());
            session.Changed += Save;
            CreateUI();
            var camera = OfficeBuilder.Build(action => ui.Interact(action));
            var player = new GameObject("Player / first person");
            player.transform.position = new Vector3(0, .05f, -3.1f);
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = new Vector3(0, 1.67f, 0); camera.transform.localRotation = Quaternion.identity;
            var controller = player.AddComponent<DesktopOfficeController>(); controller.view = camera; controller.ui = ui;
        }
        private void CreateUI()
        {
            ui = new GameObject("Academy interface", typeof(RectTransform)).AddComponent<AcademyUI>();
            ui.Initialize(session); ui.ResetRequested += ResetProgress;
        }
        private void ResetProgress()
        {
            try
            {
                if (System.IO.File.Exists(LocalStorage.SavePath))
                    System.IO.File.Copy(LocalStorage.SavePath, LocalStorage.SavePath + ".before-reset-" + DateTime.UtcNow.Ticks, false);
            }
            catch (Exception ex) { Debug.LogWarning("No se reinició: no se pudo conservar la copia. " + ex.Message); return; }
            session.Changed -= Save;
            session = new AcademySession(ContentLoader.Load()); session.Changed += Save;
            ui.gameObject.SetActive(false); Destroy(ui.gameObject); CreateUI();
            FindFirstObjectByType<DesktopOfficeController>().ui = ui; Save();
        }
        private void Update()
        {
            if (hasFocus && ui != null && !ui.IsPaused) session.Tick(Time.unscaledDeltaTime);
            saveTimer += Time.unscaledDeltaTime;
            if (saveTimer >= 20) { saveTimer = 0; Save(); }
        }
        private void Save() { if (session != null) LocalStorage.Write(session.Save); }
        private void OnApplicationFocus(bool focus) { hasFocus = focus; if (!focus) Save(); }
        private void OnApplicationPause(bool pause) { if (pause) Save(); }
        private void OnApplicationQuit() => Save();
    }
}
