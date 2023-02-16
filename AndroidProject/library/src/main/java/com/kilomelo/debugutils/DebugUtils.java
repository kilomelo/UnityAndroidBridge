package com.kilomelo.debugutils;

import android.text.TextUtils;
import android.util.Log;

public class DebugUtils {
    // 打印函数调用日志开关
    public static boolean DEBUG_DEF_METHOD_LOG = true;
    // 打印函数调用日志
    public static void methodLog() {
        if (!DEBUG_DEF_METHOD_LOG) return;
        StackTraceElement ste = new Throwable().getStackTrace()[1];
        String fullClassName = ste.getClassName();
        String tag = fullClassName.substring(fullClassName.lastIndexOf('.') + 1);
        String info = ste.getMethodName();
        Log.d(tag, info);
    }

    public static void methodLog(String extraInfo) {
        if (!DEBUG_DEF_METHOD_LOG) return;
        StackTraceElement ste = new Throwable().getStackTrace()[1];
        String fullClassName = ste.getClassName();
        String tag = fullClassName.substring(fullClassName.lastIndexOf('.') + 1);
        String info = ste.getMethodName();
        if (!TextUtils.isEmpty(extraInfo))
        {
            info = info.concat(" | ").concat(extraInfo);
        }
        Log.d(tag, info);
    }
}
