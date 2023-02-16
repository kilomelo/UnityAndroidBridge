package com.kilomelo.tools;

import static android.content.Context.MODE_PRIVATE;
import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

public class PersistentData {
    private static String TAG = PersistentData.class.getSimpleName();
    private static PersistentData mInstance;
    public static PersistentData getInstance()
    {
        if (null == mInstance) mInstance = new PersistentData();
        return mInstance;
    }

    private Context mContext;
    private SharedPreferences mSharedPreferences;

    public void init(Context context)
    {
        mContext = context;
    }

    public SharedPreferences getSharedPreferences()
    {
        if (mSharedPreferences == null) mSharedPreferences = mContext.getSharedPreferences("saveData", MODE_PRIVATE);
        if (mSharedPreferences == null) Log.e(TAG, "get sharedPreferences failed.");
        return mSharedPreferences;
    }
}
