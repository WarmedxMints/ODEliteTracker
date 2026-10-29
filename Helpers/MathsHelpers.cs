
namespace ODEliteTracker.Helpers
{
    public static class MathsHelpers
    {
        public static double ConvertToLinearScale(double value, double min, double mid, double max)
        {
            // Clamp the input value to ensure it stays within bounds
            if (value <= min) 
                return 0.0;
            if (value >= max) 
                return 1.0;

            var positionInSegment = 0d;
            // Segment 1: Between Minimum and Midway point (Maps to 0.0 - 0.5)
            if (value < mid)
            {
                positionInSegment = (value - min) / (mid - min);
                return positionInSegment * 0.5;
            }
            // Segment 2: Between Midway point and Maximum (Maps to 0.5 - 1.0)
            positionInSegment = (value - mid) / (max - mid);
            return 0.5 + (positionInSegment * 0.5);
        }

        public static double ConvertFromLinearScale(double linearValue, double min, double mid, double max)
        {
            // Clamp the input linear value to ensure it stays within bounds
            if (linearValue <= 0.0) 
                return min;
            if (linearValue >= 1.0) 
                return max;

            var segmentPercentage = 0d;
            // Segment 1: Linear value is between 0.0 and 0.5 (Maps to min to mid)
            if (linearValue < 0.5)
            {
                // Find the percentage of completion inside the first segment (0.0 to 1.0)
                segmentPercentage = linearValue / 0.5;
                return min + (segmentPercentage * (mid - min));
            }
            // Segment 2: Linear value is between 0.5 and 1.0 (Maps to mid to max)
            // Find the percentage of completion inside the second segment (0.0 to 1.0)
            segmentPercentage = (linearValue - 0.5) / 0.5;
            return mid + (segmentPercentage * (max - mid));
        }

        private const double HalfStep = 0.5;
        /// <summary>
        /// Converts a normalized linear value (0.0 to 1.0) into a smooth curved custom value.
        /// </summary>
        public static double FromLinearBezier(double t, double min, double mid, double max)
        {
            // 1. Boundary & configuration checks
            if (min >= mid || mid >= max)
            {
                throw new ArgumentException("Scale constraints violated. Must ensure that: Min < Mid < Max.");
            }
            if (t <= 0.0) return min;
            if (t >= 1.0) return max;

            // 2. Calculate local monotonic coefficients for this specific scale layout
            double delta1 = (mid - min) / HalfStep;
            double delta2 = (max - mid) / HalfStep;

            double slopeMin = delta1;
            double slopeMid = (delta1 * delta2 <= 0) ? 0 : (2.0 * delta1 * delta2) / (delta1 + delta2);

            // 3. Evaluate based on which half of the timeline we are on
            if (t < 0.5)
            {
                double a1 = (slopeMid + slopeMin - 2.0 * delta1) / (HalfStep * HalfStep);
                double b1 = ((delta1 - slopeMin) / HalfStep) - (a1 * HalfStep);

                return (a1 * Math.Pow(t, 3)) + (b1 * Math.Pow(t, 2)) + (slopeMin * t) + min;
            }
            else
            {
                double slopeMax = delta2;
                double a2 = (slopeMax + slopeMid - 2.0 * delta2) / (HalfStep * HalfStep);
                double b2 = ((delta2 - slopeMid) / HalfStep) - (a2 * HalfStep);

                double dt = t - 0.5;
                return (a2 * Math.Pow(dt, 3)) + (b2 * Math.Pow(dt, 2)) + (slopeMid * dt) + mid;
            }
        }

        /// <summary>
        /// Converts a custom scale value back into a normalized linear value (0.0 to 1.0).
        /// </summary>
        public static double ToLinearBezier(double value, double min, double mid, double max)
        {
            // 1. Boundary & configuration checks
            if (min >= mid || mid >= max)
            {
                throw new ArgumentException("Scale constraints violated. Must ensure that: Min < Mid < Max.");
            }
            if (value <= min) return 0.0;
            if (value >= max) return 1.0;
            if (Math.Abs(value - mid) < 1e-9) return 0.5;

            // 2. Binary search bounds targeting the matching monotone curve segment
            double low = value < mid ? 0.0 : 0.5;
            double high = value < mid ? 0.5 : 1.0;

            // 28 iterations yields ultra-precise floating point results
            for (int i = 0; i < 28; i++)
            {
                double midT = (low + high) / 2.0;
                double currentVal = FromLinearBezier(midT, min, mid, max);

                if (currentVal < value) low = midT;
                else high = midT;
            }

            return (low + high) / 2.0;
        }
    }
}
