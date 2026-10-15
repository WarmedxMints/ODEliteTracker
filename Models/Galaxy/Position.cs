using EliteJournalReader;
using Newtonsoft.Json;

namespace ODEliteTracker.Models.Galaxy
{
    public sealed class Position
    {
        [JsonIgnore]
        public const double kEpsilon = 0.00001F;
        [JsonIgnore]
        public const double kEpsilonNormalSqrt = 1e-15F;

        public Position() { }

        public Position(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
       
        public Position(Position starPos) : this(starPos.X, starPos.Y, starPos.Z) { }
        public Position(SystemPosition starPos) : this(starPos.X, starPos.Y, starPos.Z) { }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        [JsonIgnore]
        public Position FlipZ => new(X, Y, Z * -1);
        [JsonIgnore]
        public double sqrMagnitude => (float)(X * X + Y * Y + Z * Z);
        [JsonIgnore]
        public double magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);

        public static Position Normalize(in Position value)
        {
            var num = value.magnitude;
            if (!(num > 1E-05f))
            {
                return new() { X = 0, Y = 0, Z = 0 };
            }

            Position result = new()
            {
                X = value.X / num,
                Y = value.Y / num,
                Z = value.Z / num
            };
            return result;
        }

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

        public static Position GetPointAtDistance(Position pointA, Position pointB, float distance)
        {
            // 1. Get the direction vector from A to B
            Position direction = pointB - pointA;

            // 2. Normalize it so its total length becomes 1 unit
            Position normalizedDirection = Position.Normalize(direction);

            // 3. Multiply the unit direction by your distance and add it to the starting point
            Position targetPoint = pointA + (normalizedDirection * distance);

            return targetPoint;
        }

        public static Position GetClosestPointOnLineSegment(Position lineStart, Position lineEnd, Position point)
        {
            Position lineVector = lineEnd - lineStart;
            double lineLengthSqr = lineVector.sqrMagnitude;

            // If the two points are on top of each other, return the start point
            if (lineLengthSqr == 0f)
                return lineStart;

            // Project the point onto the line vector to find the percentage 't' along the line
            Position pointVector = point - lineStart;
            double t = Dot(pointVector, lineVector) / lineLengthSqr;

            // Clamp 't' to keep the point strictly on the line segment
            t = Math.Clamp(t, 0f, 1f); // Use Math.Clamp(t, 0f, 1f) for non-Unity C#

            // Calculate the actual 3D coordinates
            return lineStart + lineVector * t;
        }

        public static double SqrMagnitude(Position position) { return position.X * position.X + position.Y * position.Y + position.Z * position.Z; }
        public static double Dot(Position lhs, Position rhs) { return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z; }
        public static double Magnitude(Position position) { return Math.Sqrt(position.X * position.X + position.Y * position.Y + position.Z * position.Z); }
        public static Position operator +(Position a, Position b) { return new Position(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Position operator -(Position a, Position b) { return new Position(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Position operator -(Position a) { return new Position(-a.X, -a.Y, -a.Z); }
        public static Position operator *(Position a, float d) { return new Position(a.X * d, a.Y * d, a.Z * d); }
        public static Position operator *(double d, Position a) { return new Position(a.X * d, a.Y * d, a.Z * d); }
        public static Position operator *(Position a, double d) { return new Position(a.X * d, a.Y * d, a.Z * d); }
        public static Position operator /(Position a, float d) { return new Position(a.X / d, a.Y / d, a.Z / d); }

    }
}
