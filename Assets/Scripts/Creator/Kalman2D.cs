using UnityEngine;

namespace BilliardMasterAi.Creator
{
    // Constant-velocity 2D Kalman filter: state [x y vx vy]
    public class Kalman2D
    {
        private Vector4 x; // state
        private Matrix4x4 P = Matrix4x4.identity * 1f;
        private readonly Matrix4x4 Q = Matrix4x4.identity * 0.05f; // process noise
        private readonly Matrix4x4 R = Matrix4x4.identity * 0.5f;  // measurement noise (applied to position terms via H)

        public Vector2 Update(Vector2 z, float dt)
        {
            // Build F and H
            var F = Matrix4x4.identity; F.m02 = dt; F.m13 = dt; // x += vx*dt; y += vy*dt
            var H = new Matrix4x4(); H.m00 = 1; H.m11 = 1; // measure x,y only

            // Predict
            x = F * x; P = F * P * F.transpose + Q;

            // Update
            var zVec = new Vector4(z.x, z.y, 0, 0);
            var yVec = zVec - H * x; // innovation
            var S = H * P * H.transpose + R;
            var K = P * H.transpose * S.inverse; // Kalman gain
            x = x + K * yVec; P = (Matrix4x4.identity - K * H) * P;
            return new Vector2(x.x, x.y);
        }
    }
}

