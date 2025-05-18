﻿using EliteJournalReader;

namespace ODEliteTracker.Models.Galaxy
{
    public sealed class Position
    {
        public const double kEpsilon = 0.00001F;
        public const double kEpsilonNormalSqrt = 1e-15F;

        public Position() { }

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Position(SystemPosition starPos) : this(starPos.X, starPos.Y, starPos.Z) { }

        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Position FlipZ => new(X, Y, Z * -1);

        public static double Distance(Position a, Position b)
        {
            double diff_x = a.X - b.X;
            double diff_y = a.Y - b.Y;
            double diff_z = a.Z - b.Z;
            return Math.Sqrt((diff_x * diff_x) + (diff_y * diff_y) + (diff_z * diff_z));
        }

        public double DistanceFrom(Position a)
        {
            var ret = Distance(this, a);
            return ret;
        }

        public static double Angle(Position from, Position to)
        {
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            double denominator = Math.Sqrt(SqrMagnitude(from) * SqrMagnitude(to));
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            double dot = Math.Clamp(Dot(from, to) / denominator, -1F, 1F);
            return Math.Acos(dot) * 360 / (Math.PI * 2);
        }

        public static double SignedAngle(Position from, Position to, Position axis)
        {
            double unsignedAngle = Angle(from, to);

            double cross_x = from.Y * to.Z - from.Z * to.Y;
            double cross_y = from.Z * to.X - from.X * to.Z;
            double cross_z = from.X * to.Y - from.Y * to.X;
            double sign = Math.Sign(axis.X * cross_x + axis.Y * cross_y + axis.Z * cross_z);
            return unsignedAngle * sign;
        }

        public static Position Cross(Position lhs, Position rhs)
        {
            return new Position(
                lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.X * rhs.Y - lhs.Y * rhs.X);
        }

        public static Position ProjectOnPlane(Position position, Position planeNormal)
        {
            double sqrMag = Dot(planeNormal, planeNormal);
            if (sqrMag < kEpsilon)
                return position;
            else
            {
                var dot = Dot(position, planeNormal);
                return new Position(position.X - planeNormal.X * dot / sqrMag,
                    position.Y - planeNormal.Y * dot / sqrMag,
                    position.Z - planeNormal.Z * dot / sqrMag);
            }
        }

        public static double SqrMagnitude(Position position) { return position.X * position.X + position.Y * position.Y + position.Z * position.Z; }
        public static double Dot(Position lhs, Position rhs) { return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z; }
        public static double Magnitude(Position position) { return Math.Sqrt(position.X * position.X + position.Y * position.Y + position.Z * position.Z); }
        public static Position operator +(Position a, Position b) { return new Position(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Position operator -(Position a, Position b) { return new Position(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Position operator -(Position a) { return new Position(-a.X, -a.Y, -a.Z); }
        public static Position operator *(Position a, float d) { return new Position(a.X * d, a.Y * d, a.Z * d); }
        public static Position operator *(double d, Position a) { return new Position(a.X * d, a.Y * d, a.Z * d); }
        public static Position operator /(Position a, float d) { return new Position(a.X / d, a.Y / d, a.Z / d); }

    }
}
