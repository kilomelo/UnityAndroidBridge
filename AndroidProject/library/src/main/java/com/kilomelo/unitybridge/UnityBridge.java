package com.kilomelo.unitybridge;

import android.text.TextUtils;
import android.util.Log;

import com.kilomelo.debugutils.DebugUtils;
import com.unity3d.player.UnityPlayer;

import java.util.ArrayDeque;
import java.util.Dictionary;
import java.util.Hashtable;
import java.util.Queue;

public class UnityBridge {
    private static String TAG = UnityBridge.class.getSimpleName();
    private static UnityBridge mInstance;
    public static UnityBridge getInstance()
    {
        if (null == mInstance) mInstance = new UnityBridge();
        return mInstance;
    }
    // 通用成功返回值
    public static String COMMON_SUCCEEDED = "COMMON_SUCCEEDED";
    // 通用失败返回值
    public static String COMMON_FAILED = "COMMON_FAILED";
    public static String PERMISSION_DENIED = "PERMISSION_DENIED";
    private static final int RUNNING_TASK_WARNING_SIZE = 10;
    private Dictionary<String, SyncMethodForUnity> mSyncMethodsForUnity;
    private Dictionary<String, AsyncMethodForUnity> mAsyncMethodsForUnity;
    private UnityPlayer mUnityPlayer;
    private UnityCallbackProxy mUnityCallback;
    private int mTaskUidProvider;
    private Queue<UnityAsyncTask> mTaskPool;
    private int mRunningTaskCount;

    public void init(UnityPlayer unityPlayer)
    {
        DebugUtils.methodLog();
        mUnityPlayer = unityPlayer;
    }
    public void register(String name, SyncMethodForUnity method)
    {
        DebugUtils.methodLog("name: " + name);
        if (TextUtils.isEmpty(name))
        {
            Log.e(TAG, "cant register with empty name ");
            return;
        }
        if (null == method)
        {
            Log.e(TAG, "cant register null method with name " + name);
            return;
        }
        if (null == mSyncMethodsForUnity) mSyncMethodsForUnity = new Hashtable<>();
        mSyncMethodsForUnity.put(name, method);
    }

    public void register(String name, AsyncMethodForUnity method)
    {
        DebugUtils.methodLog("name: " + name);
        if (TextUtils.isEmpty(name))
        {
            Log.e(TAG, "cant register with empty name ");
            return;
        }
        if (null == method)
        {
            Log.e(TAG, "cant register null method with name " + name);
            return;
        }
        if (null == mAsyncMethodsForUnity) mAsyncMethodsForUnity = new Hashtable<>();
        mAsyncMethodsForUnity.put(name, method);
    }

    public void unregister(String name)
    {
        DebugUtils.methodLog("name: " + name);
        if (null != mSyncMethodsForUnity) {
            mSyncMethodsForUnity.remove(name);
        }
        if (null != mAsyncMethodsForUnity)
        {
            mAsyncMethodsForUnity.remove(name);
        }
    }

    private void setUnityCallback(UnityCallbackProxy callback)
    {
        DebugUtils.methodLog("callback: " + callback);
        mUnityCallback = callback;
    }

    private String callFromUnitySync(String methodName, String params)
    {
        DebugUtils.methodLog("methodName: " + methodName + " params: " + params);
        return runMethodSync(methodName, params, false);
    }

    private String callFromUnitySyncOnUiThread(String methodName, String params)
    {
        DebugUtils.methodLog("methodName: " + methodName + " params: " + params);
        return runMethodSync(methodName, params, true);
    }

    private String runMethodSync(String methodName, String params, boolean runOnUiThread) {

        DebugUtils.methodLog("methodName: " + methodName + " params: " + params + " runOnUiThread: " + runOnUiThread);

        if (TextUtils.isEmpty(methodName))
        {
            Log.e(TAG, "callFromUnitySyncOnUiThread with null or empty methodName");
            return COMMON_FAILED;
        }
        if (null == mSyncMethodsForUnity) {
            Log.e(TAG, "method: " + methodName + " not registered.");
            return COMMON_FAILED;
        }
        SyncMethodForUnity method = mSyncMethodsForUnity.get(methodName);
        if (null != method)
        {
            if (!runOnUiThread) {
                return method.apply(params);
            }
            else {
                if (null == mUnityPlayer) {
                    Log.e(TAG, "unity activity is null");
                    return COMMON_FAILED;
                }
                UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        method.apply(params);
                    }
                });
                return COMMON_SUCCEEDED;
            }
        }
        else
        {
            Log.e(TAG, "method: " + methodName + " not registered.");
        }
        return COMMON_FAILED;
    }


    private int callFromUnityAsync(String methodName, String params)
    {
        DebugUtils.methodLog("methodName: " + methodName + " params: " + params);
        return runMethodAsync(methodName, params, false);
    }

    private int callFromUnityAsyncOnUiThread(String methodName, String params)
    {
        DebugUtils.methodLog("methodName: " + methodName + " params: " + params);
        return runMethodAsync(methodName, params, true);
    }

    private int runMethodAsync(String methodName, String params, boolean runOnUiThread)
    {
        DebugUtils.methodLog("methodName: " + methodName + " params: " + params + " runOnUiThread: " + runOnUiThread);

        if (TextUtils.isEmpty(methodName)) {
            Log.e(TAG, "callFromUnitySync with null or empty methodName");
            return -1;
        }
        if (null == mAsyncMethodsForUnity) {
            Log.e(TAG, "method: " + methodName + " not registered.");
            return -1;
        }
        AsyncMethodForUnity method = mAsyncMethodsForUnity.get(methodName);
        if (null != method)
        {
            UnityAsyncTask task = null;
            if (null == mTaskPool) mTaskPool = new ArrayDeque<>();
            if (mTaskPool.isEmpty()) {
                task = new UnityAsyncTask();
            }
            else {
                task = mTaskPool.poll();
            }
            task.mUid = ++mTaskUidProvider;
            if (mTaskUidProvider == Integer.MAX_VALUE) mTaskUidProvider = 0;

            if (!runOnUiThread) {
                method.apply(task, params);

            }
            else {
                if (null == mUnityPlayer) {
                    Log.e(TAG, "unity activity is null");
                    task.mUid = -1;
                    mTaskPool.offer(task);
                    return -1;
                }
                UnityAsyncTask finalTask = task;
                UnityPlayer.currentActivity.runOnUiThread(new Runnable() {
                    @Override
                    public void run() { method.apply(finalTask, params); }
                });
            }

            mRunningTaskCount++;
            String msg = "Start task: " + task.mUid + " running task count: " + mRunningTaskCount;
            if (mRunningTaskCount > RUNNING_TASK_WARNING_SIZE) Log.w(TAG, msg);
            else Log.d(TAG, msg);
            return task.mUid;
        }
        else
        {
            Log.e(TAG, "method: " + methodName + " not registered.");
        }
        return -1;
    }

    public void finishTask(UnityAsyncTask task)
    {
        DebugUtils.methodLog("task: " + task);
        if (null == task) {
            Log.e(TAG, "task is null.");
            return;
        }
        if (null == mUnityCallback) {
            Log.e(TAG, "mUnityCallback not set.");
            return;
        }
        mUnityCallback.JsonParamCallback(task.mUid, task.mParams);
        task.mUid = -1;
        task.mParams = null;
        mTaskPool.offer(task);
        mRunningTaskCount--;
        Log.d(TAG, "task pool size: " + mTaskPool.size());
    }
}
