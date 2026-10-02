using ODEliteTracker.Models.Galaxy;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Galaxy
{
    public sealed class SystemBodyVM(SystemBody body, StarSystemVM owner) : ODObservableObject
    {
        public StarSystemVM Owner { get; } = owner;
        public uint BodyID { get;  } = body.BodyID;
        public string BodyName { get; set; } = body.BodyName;
        public string BodyNameLocal
        {
            get
            {
                if (string.IsNullOrEmpty(Owner.Name))
                {
                    return BodyName;
                }

                return BodyName.StartsWith(Owner.Name, StringComparison.OrdinalIgnoreCase) && BodyName.Length > Owner.Name.Length
                    ? BodyName[(Owner.Name.Length + 1)..]
                    : BodyName;
            }
        }
    }
}
