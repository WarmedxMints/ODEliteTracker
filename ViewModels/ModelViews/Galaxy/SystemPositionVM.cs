using EliteJournalReader;
using ODEliteTracker.Models.Spansh;
using Position = ODEliteTracker.Models.Galaxy.Position;

namespace ODEliteTracker.ViewModels.ModelViews.Galaxy
{
    public sealed class SystemPositionVM
    {
        public SystemPositionVM()
        {
            Position = new Position();
        }
        public SystemPositionVM(Position pos)
        {
            Position = pos;
        }

        public SystemPositionVM(SystemPosition starPos)
        {
            Position = new(starPos);
        }

        public SystemPositionVM(ApiColonisationSystemJump system)
        {
            Position = new(system.X, system.Y, system.Z);
        }

        public Position Position { get; internal set; }
        public double X => Position.X;
        public string XString => X.ToString("N3");
        public double Y => Position.Y;
        public string YString => Y.ToString("N3");
        public double Z => Position.Z;
        public string ZString => Z.ToString("N3");

        public double DistanceFrom(SystemPositionVM other) => Position.DistanceFrom(other.Position);

        public static Position GetClosestPointOnLineSegment(Position lineStart, Position lineEnd, Position point)
        {
            Position lineVector = lineEnd - lineStart;
            var lineLengthSqr = lineVector.sqrMagnitude;

            // If the two points are on top of each other, return the start point
            if (lineLengthSqr == 0f)
                return lineStart;

            // Project the point onto the line vector to find the percentage 't' along the line
            Position pointVector = point - lineStart;
            var t = Position.Dot(pointVector, lineVector) / lineLengthSqr;

            // Clamp 't' to keep the point strictly on the line segment
            t = Math.Clamp(t, 0f, 1f); 

            // Calculate the actual 3D coordinates
            return lineStart + lineVector * t;
        }
    }
}
