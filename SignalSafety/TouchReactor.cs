using UnityEngine;
using System;

namespace SignalMenu.SignalSafety
{
    public class TouchReactor : MonoBehaviour
    {
        public static SphereCollider Probe;

        private Action _onActivate;
        private bool _isSwitch;
        private Renderer _rend;
        private Material _mat;
        private Color _baseColor;
        private bool _gazing;

        private static float _globalCooldown;

        private static readonly Color GazeHighlight = new Color(0.18f, 0.3f, 0.45f, 0.95f);
        private static readonly Color TapFlash = new Color(0.3f, 0.85f, 1f, 1f);
        private const float TapGate = 0.5f;
        private const float GazeFadeRate = 10f;
        private const float ColorEpsilon = 0.005f;

        private const int TapMaterialID = 67;
        private const float TapGain = 0.05f;

        public void Init(Action onActivate, bool isSwitch)
        {
            _onActivate = onActivate;
            _isSwitch = isSwitch;
            _rend = GetComponent<Renderer>();
            if (_rend != null)
            {
                _mat = _rend.material;
                _baseColor = _mat.color;
            }
        }

        void Update()
        {
            if (_mat == null) return;

            Color target = _gazing ? GazeHighlight : _baseColor;
            Color current = _mat.color;
            if (Mathf.Abs(current.r - target.r) < ColorEpsilon &&
                Mathf.Abs(current.g - target.g) < ColorEpsilon &&
                Mathf.Abs(current.b - target.b) < ColorEpsilon)
            {
                _gazing = false;
                return;
            }
            _mat.color = Color.Lerp(current, target, Time.deltaTime * GazeFadeRate);
            _gazing = false;
        }

        void OnTriggerStay(Collider other)
        {
            if (Probe == null || other != Probe) return;
            _gazing = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (Probe == null || other != Probe) return;
            if (Time.time < _globalCooldown) return;
            _globalCooldown = Time.time + TapGate;

            try
            {
                GorillaTagger.Instance?.offlineVRRig?.PlayHandTapLocal(TapMaterialID, false, TapGain);
            }
            catch { }

            try
            {
                GorillaTagger.Instance?.StartVibration(false, 0.15f, 0.04f);
            }
            catch { }

            if (_mat != null)
                _mat.color = TapFlash;

            try { _onActivate?.Invoke(); } catch { }
        }

        public void SetBaseColor(Color c)
        {
            _baseColor = c;
        }
    }
}
