using System;
using UnityEngine;

namespace kilomelo.unity_android_bridge
{
    public class Proxy : AndroidJavaProxy
    {
        private static string TAG = typeof(Proxy).ToString();
        private const string JAVA_INTERFACE_CLASS = "com.kilomelo.unitybridge.UnityCallbackProxy";

        public delegate void Callback(string args);

        private Action<int, string> _handler;
        public Proxy(Action<int, string> handler) : base(JAVA_INTERFACE_CLASS)
        {
            _handler = handler;
        }
        public void JsonParamCallback(int taskUid, string args)
        {
            Debug.Log($"{TAG} JsonParamCallback, taskUid: {taskUid}, args: {args}");
            _handler.Invoke(taskUid, args);
        }
    }
}