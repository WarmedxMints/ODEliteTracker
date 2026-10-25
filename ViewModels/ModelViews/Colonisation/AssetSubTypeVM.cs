using ODEliteTracker.Models.Colonisation.Builds;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class AssetSubTypeVM(AssetSubType subType)
    {
        public string Name => subType.Name;
        public string SPad => PadSizeToString(subType.SmallPads);
        public string MPad => PadSizeToString(subType.MediumPads);
        public string LPad => PadSizeToString(subType.LargePads);

        private static string PadSizeToString(int pad)
        {
            if (pad == 0)
                return "-";

            return pad.ToString();
        }
    }
}