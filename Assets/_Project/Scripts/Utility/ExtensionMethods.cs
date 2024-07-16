using UnityEngine;

namespace _Project.Scripts.Match3.Utility
{
    public static class ExtensionMethods
    {
		
        // Given a min (a) and a max (b) value,
        // this returns the percentage at which
        // v lies, in that range
        static float InverseLerp(float a, float b, float v) => (v - a) / (b - a);

        public static float SmoothStep(float value) => value * value * (3 - 2 * value);

        public static bool IsInBounds(int x, int y, int targetX, int targetY)
        {
            return (x >= 0 && x < targetX && y >= 0 && y<targetY);
        }

        // Extension method for round.
        public static Vector3 Round(this Vector3 v)
        {
            v.x = Mathf.Round(v.x);
            v.y = Mathf.Round(v.y);
            v.z = Mathf.Round(v.z);

            return v;
        }
        public static Quaternion Rotation(float angleA, float angleB, float angleC) =>
            Quaternion.Euler(angleA, angleB, angleC);

        public static Quaternion InverseRotation(Quaternion rot) => Quaternion.Inverse(rot);
    
        private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
        public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
    }
    

}
