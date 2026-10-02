using ODEliteTracker.Models.Galaxy;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Galaxy
{
    public sealed class StarSystemVM : ODObservableObject
    {
        public StarSystemVM(StarSystem system)
        { 
            Name = system.Name;
            Address = system.Address;
            Position = new(system.Position);
            ControllingPower = system.ControllingPower;
            PowerState = system.PowerState.GetEnumDescription();
            ControllingFaction = system.ControllingFaction;
            SystemAllegiance = system.SystemAllegiance;
            Security = system.Security;
            Bodies = [.. system.Bodies.Where(body => body.Value.Landable).OrderBy(x => x.Value.BodyNameLocal).Select(x => new SystemBodyVM(x.Value, this))];
        }

        public string Name { get; }
        public ulong Address { get; }
        public SystemPositionVM Position { get;  }
        public string? ControllingPower { get;  }
        public string PowerState { get; }
        public string? ControllingFaction { get;  }
        public string? SystemAllegiance { get; }
        public string? Security { get; }
        public ObservableCollection<SystemBodyVM> Bodies { get; }
        public string? EdsmUrl { get; internal set; }
    }
}
