#pragma warning disable 0162
using UnityEngine;
using System;
using System.Collections.Generic;
using LitJson;

namespace kilomelo.unity_android_bridge
{

    public class AndroidBridge : MonoBehaviour
    {
        private static string TAG = typeof(AndroidBridge).ToString();
        private const string AndroidClassName = "com.kilomelo.unitybridge.UnityBridge";
        private const string GameObjectName = "AndroidBridge";
        // 通用成功返回值
        public const string COMMON_SUCCEEDED = "COMMON_SUCCEEDED";
        // 通用失败返回值
        public const string COMMON_FAILED = "COMMON_FAILED";

        private static AndroidBridge _instance;

        public static AndroidBridge Instance
        {
            get
            {
                if (null == _instance)
                {
                    var gobj = GameObject.Find(GameObjectName);
                    if (null == gobj)
                    {
                        gobj = new GameObject(GameObjectName);
                        DontDestroyOnLoad(gobj);
                    }

                    _instance = gobj.GetComponent<AndroidBridge>();
                }

                if (null == _instance)
                {
                    Debug.LogError($"{TAG} create instance failed.");
                }

                return _instance;
            }
        }

        private AndroidJavaObject _unityBridgeObj;
        private Proxy _proxy;
        // 所有异步任务
        private Dictionary<int, AsyncTask> _asyncTasks = new Dictionary<int, AsyncTask>();

        public void Register(string name, Action<string> method)
        {

        }

        public void Unregister(string name)
        {
        }
#region Async logic

        public string CallAsync(string methodName, Proxy.Callback callback, params object[] args)
        {
            Debug.Log($"{TAG} CallAsync, method: {methodName}");
            return _callAsync(false, methodName, callback, args);
        }
        public string CallAsyncOnAndroidUiThread(string methodName, Proxy.Callback callback, params object[] args)
        {
            Debug.Log($"{TAG} CallAsyncOnAndroidUiThread, method: {methodName}");
            return _callAsync(true, methodName, callback, args);
        }
        private string _callAsync(bool runOnAndroidUiThread, string methodName, Proxy.Callback callback, params object[] args)
        {
            Debug.Log($"{TAG} CallAsync, method: {methodName}, callback: {callback}");
            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError($"{TAG} CallAsync args invalid, code 0");
                return COMMON_FAILED;
            }

            if (args.Length % 2 != 0)
            {
                Debug.LogError($"{TAG} CallAsync args invalid, code 1");
                return COMMON_FAILED;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (null == _unityBridgeObj) TryGetUnityBridge();
            if (null != _unityBridgeObj)
#endif
            {
                var jsonData = new JsonData();
                for (var i = 0; i < args.Length; i += 2)
                {
                    if (args[i].GetType() != typeof(string))
                    {
                        Debug.LogError($"{TAG} CallAsync args invalid, code 2");
                        return COMMON_FAILED;
                    }

                    Debug.Log($"{TAG} argName: {args[i]}, type: {args[i + 1].GetType()}, value: {args[i + 1]}");
                    if (jsonData.ContainsKey((string)args[i]))
                    {
                        Debug.LogError($"{TAG} CallAsync params name dumplicated, name: {args[i]}");
                        continue;
                    }
                    // float 类型不知为何不饿能直接转为JsonData
                    if (args[i + 1] is float)
                    {
                        var fValuae = (float)args[i + 1];
                        jsonData[(string)args[i]] = new JsonData(fValuae);
                    }
                    else
                    {
                        jsonData[(string)args[i]] = new JsonData(args[i + 1]);
                    }
                }

                Debug.Log($"{TAG} CallAsync, jsonData: {jsonData.ToJson()}");
#if UNITY_ANDROID && !UNITY_EDITOR
            var javaMethodName = runOnAndroidUiThread ? "callFromUnityAsyncOnUiThread" : "callFromUnityAsync";
            var asyncTaskUid = _unityBridgeObj.Call<int>(javaMethodName, methodName, jsonData.ToJson());
            Debug.Log($"{TAG} CallAsync, taskId: {asyncTaskUid}");

            if (_asyncTasks.ContainsKey(asyncTaskUid))
            {
                Debug.LogError($"{TAG} create async task failed, uid dumplicated");
                return COMMON_FAILED;
            }
            _asyncTasks[asyncTaskUid] = new AsyncTask()
            {
                Uid = asyncTaskUid,
                Callback = callback
            };
            return COMMON_SUCCEEDED;
#endif
#if UNITY_EDITOR
                return AndroidBridge.COMMON_SUCCEEDED;
#endif
            }
            return COMMON_FAILED;
        }

        private void AsyncCallbackHandler(int taskUid, string args)
        {
            Debug.Log($"{TAG} AsyncCallbackHandler, taskUid: {taskUid}, args: {args}");
            if (_asyncTasks.TryGetValue(taskUid, out var task))
            {
                if (null != task.Callback)
                {
                    Debug.Log($"{TAG} AsyncCallbackHandler, invoke callback, taskUid: {taskUid}, args: {args}");
                    task.Callback.Invoke(args);
                }

                _asyncTasks.Remove(taskUid);
                return;
            }
            else
            {
                Debug.LogError($"{TAG} AsyncCallbackHandler get a wrong taskUid, cant find associatted task, taskUid: {taskUid}, args: {args}");
                return;
            }
        }
#endregion

#region Sync logic
        // 同步调用java方法
        public string CallSync(string methodName, params object[] args)
        {
            Debug.Log($"{TAG} CallSync, method: {methodName}");
            return _callSync(false, methodName, args);
        }

        // 同步调用java方法，在ui线程执行
        public string CallSyncOnAndroidUiThread(string methodName, params object[] args)
        {
            Debug.Log($"{TAG} CallSyncOnAndroidUiThread, method: {methodName}");
            return _callSync(true, methodName, args);
        }

        private string _callSync(bool runOnAndroidUiThread, string methodName, params object[] args)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                Debug.LogError($"{TAG} CallSync args invalid, code 0");
                return COMMON_FAILED;
            }

            if (args.Length % 2 != 0)
            {
                Debug.LogError($"{TAG} CallSync args invalid, code 1");
                return COMMON_FAILED;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            if (null == _unityBridgeObj) TryGetUnityBridge();
            if (null != _unityBridgeObj)
#endif
            {
                var jsonData = new JsonData();
                for (var i = 0; i < args.Length; i += 2)
                {
                    if (args[i].GetType() != typeof(string))
                    {
                        Debug.LogError($"{TAG} CallSync args invalid, code 2");
                        return COMMON_FAILED;
                    }

                    Debug.Log($"{TAG} argName: {args[i]}, type: {args[i + 1].GetType()}, value: {args[i + 1]}");
                    if (jsonData.ContainsKey((string)args[i]))
                    {
                        Debug.LogError($"{TAG} CallSync params name dumplicated, name: {args[i]}");
                        continue;
                    }
                    // float 类型不知为何不饿能直接转为JsonData
                    if (args[i + 1] is float)
                    {
                        var fValuae = (float)args[i + 1];
                        jsonData[(string)args[i]] = new JsonData(fValuae);
                    }
                    else
                    {
                        jsonData[(string)args[i]] = new JsonData(args[i + 1]);
                    }
                }

                Debug.Log($"{TAG} CallSync, jsonData: {jsonData.ToJson()}");
#if UNITY_ANDROID && !UNITY_EDITOR
            var javaMethodName = runOnAndroidUiThread ? "callFromUnitySyncOnUiThread" : "callFromUnitySync";
            var returnValue = _unityBridgeObj.Call<string>(javaMethodName, methodName, jsonData.ToJson());
            Debug.Log($"{TAG} CallSync, return {returnValue}");
            return returnValue;
#endif
#if UNITY_EDITOR
            return AndroidBridge.COMMON_SUCCEEDED;
#endif
            }
            Debug.LogError($"{TAG} CallSync failed.");
            return COMMON_FAILED;
        }
#endregion

        private void TryGetUnityBridge()
        {
            try
            {
                using var unityBridgeClass = new AndroidJavaClass(AndroidClassName);
                _unityBridgeObj = unityBridgeClass.CallStatic<AndroidJavaObject>("getInstance");
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} get unity bridge instance failed.");
                Debug.LogError(e);
            }
        }

        private void SetUnityCallback(Proxy proxy)
        {
            Debug.Log($"{TAG} SetUnityCallback, proxy: {proxy}");
#if UNITY_ANDROID && !UNITY_EDITOR
            if (null == _unityBridgeObj) TryGetUnityBridge();
            if (null != _unityBridgeObj) {
                _unityBridgeObj.Call("setUnityCallback", proxy);
            } else {
                Debug.LogError($"{TAG} get unity bridge instance failed.");
            }
#endif
        }


        #region life cycle

        void Start()
        {
            Debug.Log($"{TAG} started");
            _proxy = new Proxy(AsyncCallbackHandler);
            SetUnityCallback(_proxy);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            Debug.Log($"{TAG} on application focus, hasFocus: {hasFocus}");
        }

        void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"{TAG} on application pause, pauseStatus: {pauseStatus}");
        }

        #endregion

        private struct AsyncTask
        {
            public int Uid;
            public Proxy.Callback Callback;
        }
    }
}