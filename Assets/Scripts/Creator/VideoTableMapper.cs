using System;
using UnityEngine;

namespace BilliardMasterAi.Creator
{
    // Maps video image pixel coordinates (normalized 0..1) to table local meters via homography.
    public class VideoTableMapper : MonoBehaviour
    {
        // Image-space corner points (normalized 0..1), order: TL, TR, BR, BL
        public Vector2 topLeft = new Vector2(0.1f, 0.1f);
        public Vector2 topRight = new Vector2(0.9f, 0.1f);
        public Vector2 bottomRight = new Vector2(0.9f, 0.9f);
        public Vector2 bottomLeft = new Vector2(0.1f, 0.9f);

        private bool _dirty = true;
        private float[] _H = new float[9];   // 3x3 homography src(image)->dst(rect)
        private float[] _Hi = new float[9];  // inverse homography dst(rect)->src(image)

        void OnValidate() { _dirty = true; }

        // Map image normalized position to table local (meters), origin center
        public Vector2 ImageToTable(Vector2 uv)
        {
            EnsureHomography();
            // Map image->rect [0,1]^2
            float x = uv.x; float y = uv.y;
            float w = _H[6] * x + _H[7] * y + _H[8];
            if (Mathf.Abs(w) < 1e-6f) w = 1e-6f;
            float X = (_H[0] * x + _H[1] * y + _H[2]) / w;
            float Y = (_H[3] * x + _H[4] * y + _H[5]) / w;

            // Rect (0,0)-(1,1) -> table meters centered
            float tx = (X - 0.5f) * BilliardMasterAi.Physics.CaromConstants.TableWidth;
            float ty = ((1f - Y) - 0.5f) * BilliardMasterAi.Physics.CaromConstants.TableHeight; // invert Y so top->+Y
            return new Vector2(tx, ty);
        }

        // Map table local meters -> image normalized UV (0..1)
        public Vector2 TableToImage(Vector2 table)
        {
            EnsureHomography();
            // table meters -> rect [0,1]^2
            float X = (table.x / BilliardMasterAi.Physics.CaromConstants.TableWidth) + 0.5f;
            float Y = 1f - ((table.y / BilliardMasterAi.Physics.CaromConstants.TableHeight) + 0.5f);
            // rect -> image via inverse homography
            float w = _Hi[6] * X + _Hi[7] * Y + _Hi[8];
            if (Mathf.Abs(w) < 1e-6f) w = 1e-6f;
            float x = (_Hi[0] * X + _Hi[1] * Y + _Hi[2]) / w;
            float y = (_Hi[3] * X + _Hi[4] * Y + _Hi[5]) / w;
            return new Vector2(x, y);
        }

        private void EnsureHomography()
        {
            if (!_dirty) return;
            Vector2[] src = { topLeft, topRight, bottomRight, bottomLeft };
            Vector2[] dst = { new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1) };
            _H = ComputeHomography(src, dst);
            _Hi = Invert3x3(_H);
            _dirty = false;
        }

        // Direct Linear Transform for 4 point pairs
        private static float[] ComputeHomography(Vector2[] src, Vector2[] dst)
        {
            // Solve A h = b, 8x8 for h[0..7], h8=1
            double[,] A = new double[8,8];
            double[] b = new double[8];
            for (int i = 0; i < 4; i++)
            {
                double x = src[i].x, y = src[i].y;
                double X = dst[i].x, Y = dst[i].y;
                int r = i * 2;
                // row for X
                A[r,0] = x; A[r,1] = y; A[r,2] = 1; A[r,3] = 0; A[r,4] = 0; A[r,5] = 0; A[r,6] = -x * X; A[r,7] = -y * X;
                b[r] = X;
                // row for Y
                A[r+1,0] = 0; A[r+1,1] = 0; A[r+1,2] = 0; A[r+1,3] = x; A[r+1,4] = y; A[r+1,5] = 1; A[r+1,6] = -x * Y; A[r+1,7] = -y * Y;
                b[r+1] = Y;
            }
            double[] h = SolveLinearSystem(A, b);
            float[] H = new float[9];
            H[0]=(float)h[0]; H[1]=(float)h[1]; H[2]=(float)h[2];
            H[3]=(float)h[3]; H[4]=(float)h[4]; H[5]=(float)h[5];
            H[6]=(float)h[6]; H[7]=(float)h[7]; H[8]=1f;
            return H;
        }

        private static float[] Invert3x3(float[] m)
        {
            // matrix layout: [0 1 2; 3 4 5; 6 7 8]
            float a=m[0], b=m[1], c=m[2], d=m[3], e=m[4], f=m[5], g=m[6], h=m[7], i=m[8];
            float A = e*i - f*h;
            float B = c*h - b*i;
            float C = b*f - c*e;
            float D = f*g - d*i;
            float E = a*i - c*g;
            float F = c*d - a*f;
            float G = d*h - e*g;
            float H = b*g - a*h;
            float I = a*e - b*d;
            float det = a*A + b*D + c*G;
            if (Mathf.Abs(det) < 1e-8f) det = 1e-8f;
            float invDet = 1f/det;
            return new float[]{ A*invDet, B*invDet, C*invDet, D*invDet, E*invDet, F*invDet, G*invDet, H*invDet, I*invDet };
        }

        private static double[] SolveLinearSystem(double[,] A, double[] b)
        {
            int n = b.Length; // 8
            double[,] M = new double[n, n+1];
            for (int i=0;i<n;i++) { for (int j=0;j<n;j++) M[i,j]=A[i,j]; M[i,n]=b[i]; }

            // Gaussian elimination
            for (int col=0; col<n; col++)
            {
                // pivot
                int piv = col;
                double max = Math.Abs(M[piv,col]);
                for (int r=col+1;r<n;r++){ double v = Math.Abs(M[r,col]); if (v>max){max=v;piv=r;} }
                if (max < 1e-12) continue;
                if (piv != col)
                {
                    for (int c=col;c<=n;c++){ double tmp=M[col,c]; M[col,c]=M[piv,c]; M[piv,c]=tmp; }
                }
                // normalize
                double div = M[col,col];
                for (int c=col;c<=n;c++) M[col,c] /= div;
                // eliminate
                for (int r=0;r<n;r++)
                {
                    if (r==col) continue;
                    double f = M[r,col];
                    for (int c=col;c<=n;c++) M[r,c] -= f * M[col,c];
                }
            }

            var x = new double[n];
            for (int i=0;i<n;i++) x[i]=M[i,n];
            return x;
        }
    }
}
