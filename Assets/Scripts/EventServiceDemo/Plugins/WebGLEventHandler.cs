using System;
using System.Runtime.InteropServices;
using AOT;

namespace EventServiceDemo.Plugins
{
    // MonoBehaviour's OnApplicationQuit method is not called when in WebGL,
    // so we subscribe to browser's onbeforeunload event instead
    // see WebGLEventHandler.jslib
    
    public static class WebGLEventHandler
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void WebGLEventHandler_Init(Action onApplicationQuitAction);
#endif

        private static event Action OnApplicationQuitEvent;

        public static void Subscribe(Action onApplicationQuitAction)
        {
#if UNITY_WEBGL && !UNITY_EDITOR            
            if (OnApplicationQuitEvent == null)
            {
                WebGLEventHandler_Init(OnApplicationQuit);
            }
            OnApplicationQuitEvent += onApplicationQuitAction;
#endif
        }

        [MonoPInvokeCallback(typeof(Action))]
        private static void OnApplicationQuit()
        {
            OnApplicationQuitEvent?.Invoke();
        }
    }
}