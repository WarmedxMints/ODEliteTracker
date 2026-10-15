
namespace ODEliteTracker.Extensions
{
    internal static class BoolExtentions
    {
        internal static int ToInt(this bool v, int trueValue = 1, int falseValue = 0)
        {
            return v ? trueValue : falseValue;
        }
    }
}
