using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace BilliardMasterAi.Reports
{
    // Minimal single-page PDF generator (Helvetica text only).
    public static class SimplePdf
    {
        public static void Generate(string path, string title, IList<string> lines)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var objects = new List<byte[]>();

            // 1: Catalog
            objects.Add(Encode($"<< /Type /Catalog /Pages 2 0 R >>\n"));
            // 2: Pages
            objects.Add(Encode($"<< /Type /Pages /Kids [3 0 R] /Count 1 >>\n"));
            // 3: Page
            objects.Add(Encode($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\n"));
            // 4: Font
            objects.Add(Encode($"<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\n"));

            // 5: Contents stream
            var content = BuildContent(title, lines);
            var streamDict = $"<< /Length {content.Length} >>\nstream\n";
            var contentBytes = Combine(Encode(streamDict), content, Encode("\nendstream\n"));
            objects.Add(contentBytes);

            // Assemble PDF
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Write(fs, Encoding.ASCII.GetBytes("%PDF-1.4\n"));

            var xref = new List<long>();
            xref.Add(0); // obj 0 placeholder
            for (int i = 0; i < objects.Count; i++)
            {
                xref.Add(fs.Position);
                Write(fs, Encoding.ASCII.GetBytes($"{i + 1} 0 obj\n"));
                Write(fs, objects[i]);
                Write(fs, Encoding.ASCII.GetBytes("endobj\n"));
            }

            long xrefPos = fs.Position;
            var sb = new StringBuilder();
            sb.Append("xref\n");
            sb.AppendFormat("0 {0}\n", objects.Count + 1);
            sb.Append("0000000000 65535 f \n");
            for (int i = 1; i <= objects.Count; i++)
            {
                sb.AppendFormat("{0:0000000000} 00000 n \n", xref[i]);
            }
            Write(fs, Encoding.ASCII.GetBytes(sb.ToString()));

            var trailer = $"trailer\n<< /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPos}\n%%EOF";
            Write(fs, Encoding.ASCII.GetBytes(trailer));
        }

        private static byte[] BuildContent(string title, IList<string> lines)
        {
            var sb = new StringBuilder();
            sb.Append("BT\n");
            sb.Append("/F1 16 Tf 50 800 Td\n");
            sb.AppendFormat("({0}) Tj\n", Escape(title));
            sb.Append("/F1 11 Tf 0 -22 Td\n");
            foreach (var line in lines)
            {
                sb.AppendFormat("({0}) Tj\n", Escape(line));
                sb.Append("0 -16 Td\n");
            }
            sb.Append("ET\n");
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        }

        private static byte[] Encode(string s) => Encoding.ASCII.GetBytes(s);

        private static byte[] Combine(params byte[][] arrays)
        {
            int len = 0; foreach (var a in arrays) len += a.Length;
            byte[] buf = new byte[len]; int off = 0;
            foreach (var a in arrays) { Buffer.BlockCopy(a, 0, buf, off, a.Length); off += a.Length; }
            return buf;
        }

        private static void Write(Stream s, byte[] buf) => s.Write(buf, 0, buf.Length);
    }
}

