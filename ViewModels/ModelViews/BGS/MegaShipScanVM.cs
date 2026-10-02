using ODEliteTracker.Models.BGS;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class MegaShipScanVM(MegaShipScan scan)
    {
        public DateTime ScanDate { get; } = scan.ScanDate;
        public string SystemName { get; } = scan.SystemName;
        public ulong SystemAddress {get; } = scan.SystemAddress;
        public string MegaShipName {get; } = scan.MegaShipName;
    }
}
