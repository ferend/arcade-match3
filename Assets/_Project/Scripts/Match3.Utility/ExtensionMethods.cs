using UnityEngine;

namespace Game.Utility._Project.Scripts.Game.Utility
{
    public static class ExtensionMethods
    {
		
        // Given a min (a) and a max (b) value,
        // this returns the percentage at which
        // v lies, in that range
        static float InverseLerp(float a, float b, float v)
        {
            return (v - a) / (b - a);
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
    }
}
