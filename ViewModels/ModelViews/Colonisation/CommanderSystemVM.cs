using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class CommanderSystemVM(ulong systemAddress, string systemName) : ODObservableObject
    {
        public ulong SystemAddress { get; } = systemAddress;
        public string SystemName { get; } = systemName;
        public string SystemNameUpper => SystemName.ToUpper();
        public ObservableCollection<ConstructionDepotVM> Depots { get; } = [];
    }
}
