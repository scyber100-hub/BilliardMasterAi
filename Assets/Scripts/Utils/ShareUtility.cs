using System.IO;
using UnityEngine;

namespace BilliardMasterAi.Utils
{
    public static class ShareUtility
    {
        public static void ShareFile(string filePath, string mimeType = "application/pdf", string chooserTitle = "공유")
        {
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"ShareUtility: file not found {filePath}");
                return;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (var intent = new AndroidJavaObject("android.content.Intent"))
                {
                    intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
                    intent.Call<AndroidJavaObject>("setType", mimeType);

                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        using (var fileObj = new AndroidJavaObject("java.io.File", filePath))
                        using (var buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
                        {
                            int sdkInt = buildVersion.GetStatic<int>("SDK_INT");
                            AndroidJavaObject uri;
                            if (sdkInt >= 24)
                            {
                                string authority = activity.Call<AndroidJavaObject>("getPackageName").Call<string>("toString") + ".fileprovider";
                                using (var fileProvider = new AndroidJavaClass("androidx.core.content.FileProvider"))
                                {
                                    uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", activity, authority, fileObj);
                                }
                                intent.Call<AndroidJavaObject>("addFlags", 1 /*FLAG_GRANT_READ_URI_PERMISSION*/);
                            }
                            else
                            {
                                uri = new AndroidJavaClass("android.net.Uri").CallStatic<AndroidJavaObject>("fromFile", fileObj);
                            }
                            intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uri);
                        }
                    }

                    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                    using (var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, chooserTitle))
                    {
                        activity.Call("startActivity", chooser);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"ShareUtility Android share failed: {e.Message}");
            }
#elif UNITY_IOS && !UNITY_EDITOR
            // Requires native plugin for full share sheet; fallback: open the PDF
            Application.OpenURL("file://" + filePath);
#else
            // Editor/Desktop fallback: open the file location
            var dir = Path.GetDirectoryName(filePath);
            Application.OpenURL(dir);
#endif
        }
    }
}
