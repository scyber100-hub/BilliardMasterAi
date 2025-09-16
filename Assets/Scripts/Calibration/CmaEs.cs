using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Calibration
{
    // Minimal CMA-ES style optimizer (diagonal covariance, rank-μ update) for 6D parameter vector.
    public static class CmaEs
    {
        public class Options
        {
            public int lambda = 16;     // population size
            public int iterations = 200;
            public float sigma0 = 0.1f; // initial relative step
            public System.Random rnd = new System.Random(123);
        }

        public static float[] Optimize(Func<float[], float> cost, float[] x0, float[] min, float[] max, Options opt = null)
        {
            opt ??= new Options();
            int n = x0.Length; int λ = opt.lambda; int μ = Mathf.Max(1, λ / 2);
            var weights = new float[μ];
            float wsum = 0f; for (int i = 0; i < μ; i++) { weights[i] = Mathf.Log(μ + 0.5f) - Mathf.Log(i + 1); wsum += weights[i]; }
            for (int i = 0; i < μ; i++) weights[i] /= wsum;

            var x = (float[])x0.Clone();
            var sigma = new float[n]; for (int i = 0; i < n; i++) sigma[i] = opt.sigma0 * Mathf.Max(1e-4f, (max[i] - min[i]));

            var cand = new float[λ][];
            var costs = new float[λ];
            for (int it = 0; it < opt.iterations; it++)
            {
                for (int k = 0; k < λ; k++)
                {
                    var xi = new float[n];
                    for (int i = 0; i < n; i++)
                    {
                        float z = NextGaussian(opt.rnd);
                        xi[i] = Clamp(x[i] + z * sigma[i], min[i], max[i]);
                    }
                    cand[k] = xi;
                    costs[k] = cost(xi);
                }
                Array.Sort(costs, cand);
                // recombination
                var xnew = new float[n];
                for (int i = 0; i < n; i++)
                {
                    float s = 0f; for (int k = 0; k < μ; k++) s += weights[k] * cand[k][i];
                    xnew[i] = s;
                    // simple step-size adaptation (diagonal): shrink/grow based on spread of top μ
                    float spread = 0f; for (int k = 0; k < μ; k++) spread += weights[k] * Mathf.Abs(cand[k][i] - x[i]);
                    sigma[i] = Mathf.Lerp(sigma[i], Mathf.Max(1e-6f, spread), 0.3f);
                }
                x = xnew;
            }
            return x;
        }

        private static float Clamp(float v, float a, float b) => Mathf.Clamp(v, a, b);
        private static float NextGaussian(System.Random r)
        {
            // Box–Muller
            double u1 = 1.0 - r.NextDouble();
            double u2 = 1.0 - r.NextDouble();
            return (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2));
        }
    }
}

