using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace kilomelo.unity_android_bridge
{
    public class Main : MonoBehaviour
    {
        private static string TAG = typeof(Main).ToString();
        private const float LOAD_FIRST_SCENE_TIME_OUT = 10000f;
        [SerializeField] private string _firstScene;
        
        private void Start()
        {
            Debug.Log($"{TAG} Start");
            if (string.IsNullOrEmpty(_firstScene)) {
                Debug.Log($"{TAG} first scene is empty");
                return;
            }
            DontDestroyOnLoad(gameObject);
            var asyncOperation = SceneManager.LoadSceneAsync(_firstScene, LoadSceneMode.Additive);
            StartCoroutine(LoadFirstScene(asyncOperation));
        }

        private IEnumerator LoadFirstScene(AsyncOperation asyncOperation)
        {
            var startTimeMS = DateTime.UtcNow.Ticks / 10000;
            while (!asyncOperation.isDone)
            {
                if (DateTimeUtils.GetNowSeconds() - startTimeMS > LOAD_FIRST_SCENE_TIME_OUT)
                {
                    startTimeMS = long.MaxValue;
                    Debug.LogError($"{TAG} load first scene time out.");
                }
                yield return 0;
            }
            var timeCost = DateTime.UtcNow.Ticks / 10000 - startTimeMS;
            Debug.Log($"{TAG} load first scene cost {timeCost} milliseconds, real time since startup: {Time.realtimeSinceStartup} (s)");
            AndroidBridge.Instance.CallSyncOnAndroidUiThread("onUnityReady");
        }
    }
}