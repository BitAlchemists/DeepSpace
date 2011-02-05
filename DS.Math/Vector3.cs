using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DS.Math
{
    public class Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double Length
        {
            get
            {
                return System.Math.Sqrt(X * X + Y * Y + Z * Z);
            }
        }

        public Vector3()
        {
            Reset();
        }

        public Vector3(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public void Reset()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public static Vector3 operator -(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.X - vec2.X, vec1.Y - vec2.Y, vec1.Z - vec2.Z);
        }
        
        public static Vector3 operator + (Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        }

        public static Vector3 operator *(Vector3 vec, double d)
        {
            return new Vector3(vec.X * d, vec.Y * d, vec.Z * d);
        }

        public static Vector3 operator /(Vector3 vec, double d)
        {
            return new Vector3(vec.X / d, vec.Y / d, vec.Z / d);
        }


        public void Normalize()
        {
            double length = this.Length;
            if (length == 0)
            {
                return;
            }

            X /= length;
            Y /= length;
            Z /= length;
        }
    }
}
