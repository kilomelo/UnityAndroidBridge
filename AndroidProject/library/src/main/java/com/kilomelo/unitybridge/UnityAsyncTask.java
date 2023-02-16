package com.kilomelo.unitybridge;

public class UnityAsyncTask implements UnityCallback {
    public int mUid;
    public String mParams;

    @Override
    public void apply(String params) {
        mParams = params;
        UnityBridge.getInstance().finishTask(this);
    }

    @Override
    public String toString() {
        return "[mUid: ".concat(String.valueOf(mUid)).concat(" mParams: ").concat(mParams == null ? "null" : mParams).concat("]");
    }
}
