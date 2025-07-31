using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class GridSize
    {
        public GridLength this[int index]
        {
            get
            {
                if (index < 0 || GridLengths is null || GridLengths.Count - 1 < index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return GridLengths[index];
            }
            set
            {
                if (index < 0 || GridLengths is null || GridLengths.Count - 1 < index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                GridLengths[index] = value;
            }
        }

        public List<GridLength>? GridLengths { get; set; }
    }
}
