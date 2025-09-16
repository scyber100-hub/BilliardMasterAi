using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace BilliardMasterAi.Encoding
{
    public static class FfmpegWrapper
    {
        public static bool Available(string ffmpegPath="ffmpeg")
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo{ FileName = ffmpegPath, Arguments = "-version", UseShellExecute=false, RedirectStandardOutput=true, CreateNoWindow=true });
                p.WaitForExit(1000); return p.ExitCode == 0 || p.ExitCode == 1;
            } catch { return false; }
        }

        public static void PngToMp4(string dir, string pattern="overlay_%04d.png", string outName="overlay.mp4", int fps=30, string ffmpegPath="ffmpeg")
        {
            string input = Path.Combine(dir, pattern);
            string output = Path.Combine(dir, outName);
            string args = $"-y -framerate {fps} -i \"{input}\" -c:v libx264 -pix_fmt yuv420p -crf 18 \"{output}\"";
            Run(ffmpegPath, args);
        }

        private static void Run(string exe, string args)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo{ FileName = exe, Arguments = args, UseShellExecute=false, RedirectStandardOutput=true, RedirectStandardError=true, CreateNoWindow=true });
                p.WaitForExit();
                UnityEngine.Debug.Log($"ffmpeg exit {p.ExitCode}: {p.StandardError.ReadToEnd()}");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"ffmpeg run failed: {e.Message}");
            }
        }
    }
}

