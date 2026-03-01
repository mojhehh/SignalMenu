using System;
using System.Collections;
using UnityEngine;
using SignalMenu.Managers;

namespace SignalMenu.SignalSafety
{
    public class Plugin
    {
        private static Plugin _instance;
        public static Plugin Instance
        {
            get
            {
                if (_instance == null) _instance = new Plugin();
                return _instance;
            }
        }

        private static MonoBehaviour _coroutineRunner;

        public static void Initialize(MonoBehaviour runner)
        {
            _instance = new Plugin();
            _coroutineRunner = runner;
        }

        public void Log(string message)
        {
            try { LogManager.Log(message); } catch { }
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            if (_coroutineRunner != null)
                return _coroutineRunner.StartCoroutine(routine);
            return null;
        }

        public void StopCoroutine(Coroutine coroutine)
        {
            if (_coroutineRunner != null && coroutine != null)
                _coroutineRunner.StopCoroutine(coroutine);
        }

        public void ScheduleDelayedBypass(float delay)
        {
            try
            {
                if (_coroutineRunner != null)
                    _coroutineRunner.StartCoroutine(DelayedBypass(delay));
            }
            catch { }
        }

        private IEnumerator DelayedBypass(float delay)
        {
            yield return new WaitForSeconds(delay);
            try { Patches.SafetyPatches.BypassModCheckers(); } catch { }
        }
    }
}
