using System;
using System.Numerics;

namespace FBXViewer.OpenGL
{
    public class Matrix
    {
        private static float Radians(float angle)
        {
            return angle / 180.0f * (float)Math.PI;
        }
        public static Matrix4x4 PerspectiveProjection(in float fovY, in float aspectRatio, in float near, in float far)
        {
            float yMax = near * (float) Math.Tan(Radians(fovY / 2.0f));
            float xMax = yMax * aspectRatio;

            return Frustrum(-xMax, xMax, -yMax, yMax, near, far);
        }

        public static Matrix4x4 Frustrum(in float xMin, in float xMax, float yMin, in float yMax, in float near, in float far)
        {
            var m = new Matrix4x4();
            m.M11 = 2.0f * near / (xMax - xMin);
            m.M22 = 2.0f * near / (yMax - yMin);
            m.M31 = (xMax + xMin) / (xMax - xMin);
            m.M32 = (yMax + yMin) / (yMax - yMin);
            m.M33 = (-far - near) / (far - near);
            m.M34 = -1.0f;
            m.M43 = -2.0f * far * near / (far - near);
            return m;
        }

        public static Matrix4x4 LookAt(Vector3 position, Vector3 target, Vector3 up)
        {
            return LookAtDirection(position, target - position, up);
        }

        public static Matrix4x4 LookAtDirection(Vector3 position, Vector3 lookDirection, Vector3 up)
        {
            lookDirection = Vector3.Normalize(lookDirection);
            var right = Vector3.Normalize(Vector3.Cross(lookDirection, Vector3.Normalize(up)));

            up = Vector3.Normalize(Vector3.Cross(right, lookDirection));
            
            var matrix = new Matrix4x4();
            matrix.M11 = right.X;
            matrix.M12 = right.Y;
            matrix.M13 = right.Z;

            matrix.M21 = up.X;
            matrix.M22 = up.Y;
            matrix.M23 = up.Z;

            matrix.M31 = -lookDirection.X;
            matrix.M32 = -lookDirection.Y;
            matrix.M33 = -lookDirection.Z;

            matrix.M44 = 1;
            
            var translation = Translated(-position);
            matrix *= translation;

            return matrix;
        }

        public static Matrix4x4 Translated(Vector3 vector3)
        {
            var matrix = new Matrix4x4();
            matrix.M11 = matrix.M22 = matrix.M33 = matrix.M44 = 1;
            matrix.M14 = vector3.X;
            matrix.M24 = vector3.Y;
            matrix.M34 = vector3.Z;

            return matrix;
        }
    }
}