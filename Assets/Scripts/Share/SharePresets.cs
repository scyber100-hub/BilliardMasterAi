using System.Collections.Generic;

namespace BilliardMasterAi.Share
{
    public class SharePreset
    {
        public string id;
        public string name;  // UI label
        public int width;
        public int height;
        public int fps;
        public float defaultDuration; // seconds
    }

    public static class SharePresets
    {
        public static readonly List<SharePreset> Presets = new List<SharePreset>
        {
            new SharePreset{ id="shorts",   name="YouTube Shorts (1080x1920 30fps)", width=1080, height=1920, fps=30, defaultDuration=10f },
            new SharePreset{ id="reels",    name="Instagram Reels (1080x1920 30fps)", width=1080, height=1920, fps=30, defaultDuration=10f },
            new SharePreset{ id="feed45",   name="Instagram Feed 4:5 (1080x1350 30fps)", width=1080, height=1350, fps=30, defaultDuration=10f },
            new SharePreset{ id="tiktok",   name="TikTok (1080x1920 30fps)", width=1080, height=1920, fps=30, defaultDuration=10f },
            new SharePreset{ id="kakao720", name="KakaoTalk (1280x720 30fps)", width=1280, height=720, fps=30, defaultDuration=8f },
        };
    }
}

