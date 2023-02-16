package com.kilomelo.unitylauncher;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import android.content.res.Configuration;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.FrameLayout;
import com.kilomelo.debugutils.DebugUtils;
import com.kilomelo.tools.PersistentData;
import com.kilomelo.unitybridge.UnityBridge;
import com.unity3d.player.MultiWindowSupport;
import com.unity3d.player.UnityPlayer;

public class MainActivity extends AppCompatActivity implements View.OnClickListener {
    private static String TAG = MainActivity.class.getSimpleName();
    private FrameLayout mMainUnityWindow;
    private UnityPlayer mUnityPlayer;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mUnityPlayer = new UnityPlayer(this);
        UnityBridge.getInstance().init(mUnityPlayer);
        PersistentData.getInstance().init(this);
        mMainUnityWindow = findViewById(R.id.unity_frame);
        mMainUnityWindow.addView(mUnityPlayer);

        // 显示状态栏
        Window window = getWindow();
        window.clearFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN);
        getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_LIGHT_STATUS_BAR);

        UnityBridge.getInstance().register("onUnityReady", this::onUnityReady);
    }

    @Override protected void onDestroy ()
    {
        DebugUtils.methodLog();
        UnityBridge.getInstance().unregister("onUnityReady");
        if (null != mUnityPlayer) mUnityPlayer.destroy();
        super.onDestroy();
    }

    @Override protected void onStop()
    {
        super.onStop();
        DebugUtils.methodLog();
        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;
        if (null != mUnityPlayer) mUnityPlayer.pause();
    }

    @Override protected void onStart()
    {
        super.onStart();
        DebugUtils.methodLog();

        if (!MultiWindowSupport.getAllowResizableWindow(this))
            return;

        if (null != mUnityPlayer) mUnityPlayer.resume();
    }

    @Override protected void onPause()
    {
        super.onPause();
        DebugUtils.methodLog();

        if (MultiWindowSupport.getAllowResizableWindow(this))
            return;
        if (null != mUnityPlayer) mUnityPlayer.pause();
    }

    @Override protected void onResume()
    {
        super.onResume();
        DebugUtils.methodLog();

        if (MultiWindowSupport.getAllowResizableWindow(this))
            return;
        if (null != mUnityPlayer) mUnityPlayer.resume();
    }

    @Override public void onLowMemory()
    {
        super.onLowMemory();
        DebugUtils.methodLog();

        if (null != mUnityPlayer) mUnityPlayer.lowMemory();
    }

    @Override public void onTrimMemory(int level)
    {
        super.onTrimMemory(level);
        DebugUtils.methodLog();

        if (level == TRIM_MEMORY_RUNNING_CRITICAL)
        {
            if (null != mUnityPlayer) mUnityPlayer.lowMemory();
        }
    }

    // This ensures the layout will be correct.
    @Override public void onConfigurationChanged(@NonNull Configuration newConfig)
    {
        super.onConfigurationChanged(newConfig);
        DebugUtils.methodLog();

        if (null != mUnityPlayer) mUnityPlayer.configurationChanged(newConfig);
    }

    // Notify Unity of the focus change.
    @Override public void onWindowFocusChanged(boolean hasFocus)
    {
        super.onWindowFocusChanged(hasFocus);
        DebugUtils.methodLog("hasFocus: " + hasFocus);

        if (null != mUnityPlayer) mUnityPlayer.windowFocusChanged(hasFocus);
    }
    //endregion

    private String onUnityReady(String params)
    {
        DebugUtils.methodLog();
        if (null != mMainUnityWindow) {
            mMainUnityWindow.setAlpha(1f);
        }
        return UnityBridge.COMMON_SUCCEEDED;
    }
    @Override
    public void onClick(View view) {
        DebugUtils.methodLog();
        int viewId = view.getId();

    }
}