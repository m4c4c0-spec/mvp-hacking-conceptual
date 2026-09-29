using System.Collections.Generic;
using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Guía los primeros movimientos: caminar, mirar, correr y tomar una silla.
    /// El progreso queda en PlayerPrefs para no repetir la lección en cada Play.
    /// </summary>
    public sealed class MovementCoach : MonoBehaviour
    {
        public const string PrefsKey = "EthicalLab.MovementCoachStep";

        public PcInteractor Person;

        static readonly string[] Lessons =
        {
            "Coach · Camina con WASD un par de metros.",
            "Coach · Mueve el mouse para mirar la oficina.",
            "Coach · Mantén Shift mientras caminas para correr.",
            "Coach · Mira una silla hasta que se resalte.",
            "Coach · Pulsa G para tomar la silla.",
            "Coach · Pulsa G otra vez. La silla se queda donde cae."
        };

        int step;
        float walked;
        float looked;
        Vector3 lastPosition;
        bool tracking;
        readonly List<Transform> figures = new List<Transform>(4);

        public bool Active => step < Lessons.Length;
        public string Lesson => Active ? Lessons[step] : "";

        void Awake()
        {
            step = Mathf.Clamp(PlayerPrefs.GetInt(PrefsKey, 0), 0, Lessons.Length);
        }

        void Start()
        {
            if (Person == null) Person = GetComponent<PcInteractor>();
            Transform office = transform.parent;
            if (office != null)
            {
                string[] paths =
                {
                    "Office furniture/Movement coach",
                    "Office furniture/Reception guide",
                    "Office furniture/Tickets guide",
                    "Office furniture/Report guide"
                };
                for (int i = 0; i < paths.Length; i++)
                {
                    Transform found = office.Find(paths[i]);
                    if (found != null) figures.Add(found);
                }
            }
            if (Person != null)
            {
                Person.Hovered += OnHovered;
                Person.Grabbed += OnGrabbed;
                Person.Released += OnReleased;
            }
        }

        void OnDestroy()
        {
            if (Person == null) return;
            Person.Hovered -= OnHovered;
            Person.Grabbed -= OnGrabbed;
            Person.Released -= OnReleased;
        }

        public void Restart()
        {
            step = 0;
            walked = 0f;
            looked = 0f;
            tracking = false;
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
        }

        void Update()
        {
            if (!Active || Person == null || Person.MenuOpen)
            {
                tracking = false;
                return;
            }

            Vector3 now = Person.transform.position;
            if (!tracking)
            {
                lastPosition = now;
                tracking = true;
                return;
            }

            Vector3 delta = now - lastPosition;
            delta.y = 0f;
            float moved = delta.magnitude;
            lastPosition = now;

            if (step == 0)
            {
                walked += moved;
                if (walked >= 1.6f) Advance();
            }
            else if (step == 1)
            {
                looked += PcButtons.LookDelta.magnitude;
                if (looked >= 28f) Advance();
            }
            else if (step == 2 && PcButtons.Sprint && moved > 0.02f)
            {
                Advance();
            }
        }

        void LateUpdate()
        {
            if (Person == null) return;
            for (int i = 0; i < figures.Count; i++)
            {
                Transform body = figures[i];
                if (body == null) continue;
                Vector3 to = Person.transform.position - body.position;
                to.y = 0f;
                if (to.sqrMagnitude > 0.05f)
                    body.rotation = Quaternion.LookRotation(to);
            }
        }

        void OnHovered(InteractableId id, bool on)
        {
            if (on && step == 3 && id.Kind == "chair") Advance();
        }

        void OnGrabbed(InteractableId id)
        {
            if (step == 4 && id.Kind == "chair") Advance();
        }

        void OnReleased(InteractableId id)
        {
            if (step == 5 && id.Kind == "chair") Advance();
        }

        void Advance()
        {
            if (!Active) return;
            step++;
            PlayerPrefs.SetInt(PrefsKey, step);
            PlayerPrefs.Save();
        }
    }
}
