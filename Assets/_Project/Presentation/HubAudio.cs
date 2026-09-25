using UnityEngine;

namespace EthicalLab.Presentation
{
    /// <summary>
    /// Beeps procedurales (sine) para Use / Grab / toast / error / success. Sin asset packs.
    /// </summary>
    public static class HubAudio
    {
        static AudioSource source;
        static AudioClip useClip;
        static AudioClip grabClip;
        static AudioClip toastClip;
        static AudioClip documentedClip;
        static AudioClip errorClip;
        static AudioClip successClip;

        public static void Ensure(GameObject host)
        {
            if (host == null) return;
            if (source != null) return;
            source = host.GetComponent<AudioSource>();
            if (source == null) source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 0.22f;
            if (useClip == null) useClip = MakeBeep(660f, 0.06f, 0.35f);
            if (grabClip == null) grabClip = MakeBeep(440f, 0.08f, 0.3f);
            if (toastClip == null) toastClip = MakeBeep(880f, 0.1f, 0.28f);
            if (documentedClip == null) documentedClip = MakeBeep(990f, 0.12f, 0.32f);
            if (errorClip == null) errorClip = MakeBeep(196f, 0.16f, 0.4f);
            if (successClip == null) successClip = MakeBeep(784f, 0.09f, 0.32f);
        }

        public static void PlayUse() => Play(useClip);
        public static void PlayGrab() => Play(grabClip);
        public static void PlayToast() => Play(toastClip);
        public static void PlayDocumented() => Play(documentedClip);
        public static void PlayError() => Play(errorClip);
        public static void PlaySuccess() => Play(successClip);

        static void Play(AudioClip clip)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip, 0.45f);
        }

        static AudioClip MakeBeep(float freq, float duration, float amplitude)
        {
            const int rate = 22050;
            int samples = Mathf.Max(1, Mathf.RoundToInt(rate * duration));
            var clip = AudioClip.Create("hub-beep-" + Mathf.RoundToInt(freq), samples, 1, rate, false);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / rate;
                float env = 1f - t / duration;
                if (env < 0f) env = 0f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * amplitude * env;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
