using System;
using System.Numerics;
using System.Windows.Media.Media3D;
using Vector3D = Assimp.Vector3D;
using MVector3D = System.Windows.Media.Media3D.Vector3D;
using Quaternion = System.Numerics.Quaternion;

namespace FBXViewer
{
    public static class MathExtensions
    {
        public static Vector3D AsVector3D(this Point3D self)
        {
            return new Vector3D((float)self.X, (float)self.Y, (float)self.Z);
        }

        public static Vector3 AsVector3(this Point3D self)
        {
            return new Vector3((float)self.X, (float)self.Y, (float)self.Z);
        }
        
        public static Vector3 AsVector3(this MVector3D self)
        {
            return new Vector3((float)self.X, (float)self.Y, (float)self.Z);
        }
        
        public static Vector3 AsVector3(this Size3D self)
        {
            return new Vector3((float)self.X, (float)self.Y, (float)self.Z);
        }
        
        public static Vector3D AsVector3D(this MVector3D self)
        {
            return new Vector3D((float)self.X, (float)self.Y, (float)self.Z);
        }

        public static MVector3D AsMVector3D(this Vector3 self)
        {
            return new MVector3D(self.X, self.Y, self.Z);
        }
        
        public static Point3D AsPoint3D(this Vector3 self)
        {
            return new Point3D(self.X, self.Y, self.Z);
        }

        public static Vector3 Forward(this Quaternion self)
        {
            return Vector3.Transform(Vector3.UnitZ, self);
        }
        
        public static Vector3 Up(this Quaternion self)
        {
            return Vector3.Transform(Vector3.UnitY, self);
        }
        
        public static Vector3 Right(this Quaternion self)
        {
            return Vector3.Transform(Vector3.UnitX, self);
        }
        
        public static Quaternion ToLookRotation(this Vector3 forward, Vector3 up)
        {
            forward = Vector3.Normalize(forward);
 
            Vector3 vector = Vector3.Normalize(forward);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(up, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            var m00 = vector2.X;
            var m01 = vector2.Y;
            var m02 = vector2.Z;
            var m10 = vector3.X;
            var m11 = vector3.Y;
            var m12 = vector3.Z;
            var m20 = vector.X;
            var m21 = vector.Y;
            var m22 = vector.Z;
 
 
            float num8 = (m00 + m11) + m22;
            var quaternion = new Quaternion();
            if (num8 > 0f)
            {
                var num = (float)Math.Sqrt(num8 + 1f);
                quaternion.W = num * 0.5f;
                num = 0.5f / num;
                quaternion.X = (m12 - m21) * num;
                quaternion.Y = (m20 - m02) * num;
                quaternion.Z = (m01 - m10) * num;
                return quaternion;
            }
            if ((m00 >= m11) && (m00 >= m22))
            {
                var num7 = (float)Math.Sqrt(((1f + m00) - m11) - m22);
                var num4 = 0.5f / num7;
                quaternion.X = 0.5f * num7;
                quaternion.Y = (m01 + m10) * num4;
                quaternion.Z = (m02 + m20) * num4;
                quaternion.W = (m12 - m21) * num4;
                return quaternion;
            }
            if (m11 > m22)
            {
                var num6 = (float)Math.Sqrt(((1f + m11) - m00) - m22);
                var num3 = 0.5f / num6;
                quaternion.X = (m10+ m01) * num3;
                quaternion.Y = 0.5f * num6;
                quaternion.Z = (m21 + m12) * num3;
                quaternion.W = (m20 - m02) * num3;
                return quaternion; 
            }
            var num5 = (float)Math.Sqrt(((1f + m22) - m00) - m11);
            var num2 = 0.5f / num5;
            quaternion.X = (m20 + m02) * num2;
            quaternion.Y = (m21 + m12) * num2;
            quaternion.Z = 0.5f * num5;
            quaternion.W = (m01 - m10) * num2;
            return quaternion;
        }
    }
}