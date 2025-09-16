using System;
using System.Collections.Generic;
using UnityEngine;

namespace BilliardMasterAi.Analytics
{
    // Simple logistic regression with SGD for binary labels; for demo purposes.
    public class LogisticModel
    {
        private float[] w; // weights
        public int Dim => w?.Length ?? 0;

        public LogisticModel(int dim)
        {
            w = new float[dim];
        }

        private static float Sigmoid(float z) => 1f / (1f + Mathf.Exp(-z));

        public float Predict(float[] x)
        {
            float z = 0f; for (int i=0;i<w.Length;i++) z += w[i]*x[i];
            return Sigmoid(z);
        }

        public void Fit(List<float[]> X, List<int> y, int epochs=200, float lr=0.1f)
        {
            if (X.Count == 0) return;
            for (int e=0;e<epochs;e++)
            {
                for (int i=0;i<X.Count;i++)
                {
                    var x = X[i]; int yi = y[i]; float p = Predict(x);
                    float err = p - yi; // derivative of logloss
                    for (int j=0;j<w.Length;j++) w[j] -= lr * err * x[j];
                }
                lr *= 0.99f;
            }
        }

        public string ToJson() => JsonUtility.ToJson(new Wrapper{ w=w });
        public static LogisticModel FromJson(string json)
        {
            var w = JsonUtility.FromJson<Wrapper>(json).w; var m = new LogisticModel(w.Length); Array.Copy(w, m.w, w.Length); return m;
        }
        [Serializable] private struct Wrapper { public float[] w; }
    }
}

