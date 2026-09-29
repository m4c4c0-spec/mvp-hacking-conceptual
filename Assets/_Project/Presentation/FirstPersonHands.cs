using System.Collections.Generic;
using UnityEngine;
using EthicalLab.Shared;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Manos ficticias de primera persona, 100% procedurales (sin assets externos).
    /// Solo decoración: sin colliders, en capa Ignore Raycast, sin sombras.
    /// Reacciona a velocidad real, mirada, agarre (carpeta vs silla) y gesto de uso.
    /// La cámara nunca se mueve desde aquí; todo el movimiento es de las manos.
    /// </summary>
    public sealed class FirstPersonHands : MonoBehaviour
    {
        PcInteractor owner;
        Camera cam;
        bool built;

        Transform rig;
        Transform leftHand;
        Transform rightHand;
        readonly List<Transform> leftProximals = new List<Transform>(4);
        readonly List<Transform> leftDistals = new List<Transform>(4);
        readonly List<Transform> rightProximals = new List<Transform>(4);
        readonly List<Transform> rightDistals = new List<Transform>(4);
        Transform leftThumb;
        Transform rightThumb;

        readonly List<Material> ownedMaterials = new List<Material>(3);
        Material gloveMat;
        Material cuffMat;
        Material bandMat;
        Material tipMat;

        // Estado de animación.
        float grip;          // 0 abierta, 1 puño cerrado.
        float gripTarget = 0.12f;
        float punch;         // 1 justo al pulsar E, decae a 0.
        Vector2 sway;        // Retardo frente a la mirada.
        Vector3 rigOffset;
        float bobPhase;
        float wallPull;      // 0..0.25, retrocede cerca de paredes.
        float carryBlend;    // 0 libre, 1 cargando silla pesada.
        float supportBlend;  // 0 libre, 1 sujetando carpeta.

        Vector3 rightBase = new Vector3(0.32f, -0.30f, 0.48f);
        Vector3 leftBase = new Vector3(-0.32f, -0.31f, 0.44f);
        Vector3 rightCarry = new Vector3(0.22f, -0.22f, 0.54f);
        Vector3 leftCarry = new Vector3(-0.22f, -0.22f, 0.54f);
        Vector3 rightSupport = new Vector3(0.24f, -0.22f, 0.56f);
        Vector3 leftSupport = new Vector3(0.02f, -0.28f, 0.50f);

        /// <summary>Llamado por PcInteractor.Awake tras crear la cámara y el ancla.</summary>
        public void Initialize(PcInteractor interactor, Camera fpsCamera)
        {
            owner = interactor;
            cam = fpsCamera;
            if (built) return;
            if (cam == null) cam = GetComponent<Camera>();
            if (cam == null) return;
            Build();
            Subscribe();
        }

        void OnDestroy()
        {
            Unsubscribe();
            for (int i = 0; i < ownedMaterials.Count; i++)
                if (ownedMaterials[i] != null) Destroy(ownedMaterials[i]);
            ownedMaterials.Clear();
        }

        void OnDisable()
        {
            // Al desactivar, aparcar manos para no reaparecer en pose rara.
            gripTarget = 0.12f;
            punch = 0f;
        }

        void Subscribe()
        {
            if (owner == null) return;
            owner.Used -= OnUsed;
            owner.Grabbed -= OnGrabbed;
            owner.Released -= OnReleased;
            owner.Used += OnUsed;
            owner.Grabbed += OnGrabbed;
            owner.Released += OnReleased;
        }

        void Unsubscribe()
        {
            if (owner == null) return;
            owner.Used -= OnUsed;
            owner.Grabbed -= OnGrabbed;
            owner.Released -= OnReleased;
        }

        void OnUsed(InteractableId id)
        {
            // Gesto de "tocar / pulsar": las manos avanzan un poco y los dedos pinzan.
            punch = 1f;
        }

        void OnGrabbed(InteractableId id)
        {
            punch = 0.7f;
            gripTarget = id.Kind == "chair" ? 1f : 0.7f;
        }

        void OnReleased(InteractableId id)
        {
            punch = 0.25f;
            gripTarget = 0.12f;
        }

        static GameObject Part(Transform parent, string name, Vector3 localPos, Vector3 localScale,
            Material mat, PrimitiveType shape = PrimitiveType.Cube)
        {
            var go = GameObject.CreatePrimitive(shape);
            go.name = name;
            go.layer = 2; // Ignore Raycast: jamás bloquea el rayo central de interacción.
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = mat;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                if (!UnityEngine.Application.isPlaying) DestroyImmediate(collider);
                else Destroy(collider);
            }
            return go;
        }

        Transform BuildHand(string name, Vector3 basePos, Vector3 baseEuler,
            List<Transform> proximals, List<Transform> distals, out Transform thumb, bool mirror)
        {
            var root = new GameObject(name).transform;
            root.SetParent(rig, false);
            root.localPosition = basePos;
            root.localRotation = Quaternion.Euler(baseEuler);
            float side = mirror ? -1f : 1f;

            // Manga redondeada y palma ancha de guante caricaturesco.
            var cuff = Part(root, "Cuff", new Vector3(0f, -0.03f, -0.12f), new Vector3(0.12f, 0.09f, 0.12f), cuffMat, PrimitiveType.Capsule);
            cuff.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var band = Part(root, "Cuff band", new Vector3(0f, -0.01f, -0.04f), new Vector3(0.145f, 0.055f, 0.145f), bandMat, PrimitiveType.Cylinder);
            band.transform.localRotation = Quaternion.Euler(78f, 0f, 0f);
            Part(root, "Palm", new Vector3(0f, 0f, 0.03f), new Vector3(0.155f, 0.062f, 0.16f), gloveMat, PrimitiveType.Sphere);
            Part(root, "Knuckles", new Vector3(0f, 0.018f, 0.095f), new Vector3(0.14f, 0.04f, 0.05f), gloveMat, PrimitiveType.Sphere);

            // Cuatro dedos gruesos: falange proximal + punta redonda.
            float[] xs = { -0.052f, -0.018f, 0.018f, 0.052f };
            float[] lens = { 0.05f, 0.062f, 0.056f, 0.042f };
            proximals.Clear();
            distals.Clear();
            for (int i = 0; i < 4; i++)
            {
                var knuckle = new GameObject("Finger" + i).transform;
                knuckle.SetParent(root, false);
                knuckle.localPosition = new Vector3(xs[i], 0.008f, 0.10f);
                knuckle.localRotation = Quaternion.identity;
                var prox = Part(knuckle, "Proximal", new Vector3(0f, 0f, lens[i] * 0.5f),
                    new Vector3(0.034f, lens[i] * 0.5f, 0.034f), gloveMat, PrimitiveType.Capsule);
                prox.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                var tip = new GameObject("Tip" + i).transform;
                tip.SetParent(knuckle, false);
                tip.localPosition = new Vector3(0f, 0f, lens[i]);
                tip.localRotation = Quaternion.identity;
                Part(tip, "Distal", new Vector3(0f, 0f, 0.018f),
                    new Vector3(0.04f, 0.036f, 0.04f), tipMat, PrimitiveType.Sphere);
                proximals.Add(knuckle);
                distals.Add(tip);
            }

            // Pulgar lateral, también redondeado.
            var thumbPivot = new GameObject("Thumb").transform;
            thumbPivot.SetParent(root, false);
            thumbPivot.localPosition = new Vector3(side * 0.078f, -0.012f, 0.02f);
            thumbPivot.localRotation = Quaternion.Euler(-12f, side * -38f, side * -14f);
            var thumbSeg = Part(thumbPivot, "ThumbSeg", new Vector3(0f, 0f, 0.026f),
                new Vector3(0.034f, 0.028f, 0.034f), gloveMat, PrimitiveType.Capsule);
            thumbSeg.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Part(thumbPivot, "Thumb tip", new Vector3(0f, 0f, 0.055f),
                new Vector3(0.038f, 0.034f, 0.038f), tipMat, PrimitiveType.Sphere);
            thumb = thumbPivot;
            return root;
        }

        void Build()
        {
            gloveMat = CelShading.Create("FP Glove", new Color(0.16f, 0.42f, 0.82f), 0f, 0.04f);
            cuffMat = CelShading.Create("FP Cuff", new Color(0.16f, 0.42f, 0.82f), 0f, 0.04f);
            bandMat = CelShading.Create("FP Band", new Color(0.08f, 0.11f, 0.20f), 0f, 0.03f);
            tipMat = CelShading.Create("FP Tip", new Color(0.94f, 0.88f, 0.76f), 0f, 0.03f);
            ownedMaterials.Add(gloveMat);
            ownedMaterials.Add(cuffMat);
            ownedMaterials.Add(bandMat);
            ownedMaterials.Add(tipMat);

            rig = new GameObject("First-person hands").transform;
            rig.SetParent(cam.transform, false);
            rig.localPosition = Vector3.zero;
            rig.localRotation = Quaternion.identity;
            rig.gameObject.layer = 2;

            rightHand = BuildHand("Right hand", rightBase, new Vector3(-12f, -10f, 6f),
                rightProximals, rightDistals, out rightThumb, false);
            leftHand = BuildHand("Left hand", leftBase, new Vector3(-12f, 10f, -6f),
                leftProximals, leftDistals, out leftThumb, true);
            built = true;
        }

        void LateUpdate()
        {
            if (!built || cam == null) return;
            float dt = Mathf.Min(Time.deltaTime, 0.05f);
            if (dt <= 0f) return;

            bool parked = owner == null || !owner.enabled || owner.MenuOpen || !owner.HasFocus;
            if (parked)
            {
                // Menú / pausa / sin foco: manos abajo, relajadas, sin sacudidas.
                gripTarget = 0.12f;
                grip = Mathf.MoveTowards(grip, gripTarget, dt * 3f);
                punch = Mathf.MoveTowards(punch, 0f, dt * 4f);
                sway = Vector2.MoveTowards(sway, Vector2.zero, dt * 60f);
                carryBlend = Mathf.MoveTowards(carryBlend, 0f, dt * 3f);
                supportBlend = Mathf.MoveTowards(supportBlend, 0f, dt * 3f);
                rigOffset = Vector3.Lerp(rigOffset, new Vector3(0f, -0.10f, -0.06f), 1f - Mathf.Exp(-6f * dt));
                wallPull = Mathf.MoveTowards(wallPull, 0f, dt * 2f);
                ApplyPose(dt, 0f);
                return;
            }

            // Velocidad real del cuerpo para el bob (solo plano, sin contar caída).
            Vector3 vel = owner.Velocity;
            float planarSpeed = new Vector2(vel.x, vel.z).magnitude;
            float speedNorm = Mathf.Clamp01(planarSpeed / 4.4f);

            // Tipo de agarre según lo que hay en la mano.
            var held = owner.Held;
            if (held == null)
            {
                gripTarget = 0.12f;
                carryBlend = Mathf.MoveTowards(carryBlend, 0f, dt * 5f);
                supportBlend = Mathf.MoveTowards(supportBlend, 0f, dt * 5f);
            }
            else if (held.dropInPlace)
            {
                gripTarget = 1f;   // Silla pesada: puño cerrado, dos manos.
                carryBlend = Mathf.MoveTowards(carryBlend, 1f, dt * 5f);
                supportBlend = Mathf.MoveTowards(supportBlend, 0f, dt * 5f);
            }
            else
            {
                gripTarget = 0.7f; // Carpeta: pinza firme con la derecha.
                carryBlend = Mathf.MoveTowards(carryBlend, 0f, dt * 5f);
                supportBlend = Mathf.MoveTowards(supportBlend, 1f, dt * 5f);
            }

            grip = Mathf.MoveTowards(grip, gripTarget, dt * 4f);
            punch = Mathf.MoveTowards(punch, 0f, dt * 5f);

            // Sway con retardo frente a la mirada (solo manos, la cámara no se toca).
            Vector2 look = owner.LookMotion;
            Vector2 swayTarget = new Vector2(
                Mathf.Clamp(-look.x * 0.0016f, -0.045f, 0.045f),
                Mathf.Clamp(-look.y * 0.0012f, -0.035f, 0.035f));
            float swayK = 1f - Mathf.Exp(-10f * dt);
            sway = Vector2.Lerp(sway, swayTarget, swayK);
            // El sway decae solo: si no hay movimiento de ratón, vuelve al centro.
            sway = Vector2.MoveTowards(sway, Vector2.zero, dt * 0.35f);

            // Bob: respiración en reposo + pasos al caminar/correr.
            float freq = 1.6f + speedNorm * 7.5f;
            float amp = 0.0035f + speedNorm * 0.011f;
            bobPhase += dt * freq * Mathf.PI;
            if (planarSpeed < 0.15f) bobPhase += dt * 1.2f; // respiración visible en reposo.

            // Golpe de aterrizaje: baja las manos un instante.
            float dip = owner.LandDip * 0.05f;

            // Anti-clipping: si hay pared a <60cm, replegar.
            float targetPull = 0f;
            Ray ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 0.6f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                // Ignorar lo que estamos sujetando (sus colliders están off, pero por seguridad).
                if (held == null || hit.collider.GetComponentInParent<InteractableView>() != held)
                    targetPull = Mathf.Clamp01((0.6f - hit.distance) / 0.6f) * 0.24f;
            }
            wallPull = Mathf.MoveTowards(wallPull, targetPull, dt * 6f);

            Vector3 bob = new Vector3(
                Mathf.Cos(bobPhase) * amp * 0.7f + sway.x,
                Mathf.Sin(bobPhase * 2f) * amp - dip + sway.y,
                punch * 0.055f - wallPull);
            rigOffset = Vector3.Lerp(rigOffset, bob, 1f - Mathf.Exp(-12f * dt));

            ApplyPose(dt, speedNorm);
        }

        void ApplyPose(float dt, float speedNorm)
        {
            // Mezcla de posiciones según lleve carpeta, silla o nada.
            Vector3 r = Vector3.Lerp(rightBase, rightCarry, carryBlend);
            r = Vector3.Lerp(r, rightSupport, supportBlend);
            Vector3 l = Vector3.Lerp(leftBase, leftCarry, carryBlend);
            l = Vector3.Lerp(l, leftSupport, supportBlend);

            // El puñetazo de "usar" adelanta la derecha un poco más.
            r.z += punch * 0.03f;
            l.z += punch * 0.015f;

            float k = 1f - Mathf.Exp(-14f * Mathf.Max(dt, 0.0001f));
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, r + rigOffset, k);
            leftHand.localPosition = Vector3.Lerp(leftHand.localPosition, l + rigOffset * 0.85f, k);

            // Inclinación leve alstrafear / correr: vida sin mover la cámara.
            float strafe = owner != null ? Vector3.Dot(owner.Velocity, cam.transform.right) : 0f;
            float tilt = Mathf.Clamp(strafe * 1.4f, -6f, 6f) - sway.x * 120f;
            rightHand.localRotation = Quaternion.Slerp(rightHand.localRotation,
                Quaternion.Euler(-12f - punch * 10f - supportBlend * 8f, -10f - carryBlend * 8f, 6f + tilt * 0.4f), k);
            leftHand.localRotation = Quaternion.Slerp(leftHand.localRotation,
                Quaternion.Euler(-12f - punch * 6f - supportBlend * 4f, 10f + carryBlend * 8f, -6f + tilt * 0.4f), k);

            CurlFingers(rightProximals, rightDistals, grip + punch * 0.15f);
            CurlFingers(leftProximals, leftDistals, Mathf.Clamp01(grip * 0.9f + carryBlend * 0.1f));
            if (rightThumb != null)
                rightThumb.localRotation = Quaternion.Slerp(rightThumb.localRotation,
                    Quaternion.Euler(-12f - grip * 18f, -38f, -14f), k);
            if (leftThumb != null)
                leftThumb.localRotation = Quaternion.Slerp(leftThumb.localRotation,
                    Quaternion.Euler(-12f - grip * 18f, 38f, 14f), k);
        }

        static void CurlFingers(List<Transform> proximals, List<Transform> distals, float curl)
        {
            curl = Mathf.Clamp01(curl);
            float proxAngle = -6f - curl * 52f;
            float tipAngle = -4f - curl * 48f;
            for (int i = 0; i < proximals.Count && i < 4; i++)
            {
                if (proximals[i] != null)
                    proximals[i].localRotation = Quaternion.Euler(proxAngle, 0f, 0f);
                if (i < distals.Count && distals[i] != null)
                    distals[i].localRotation = Quaternion.Euler(tipAngle, 0f, 0f);
            }
        }
    }
}
