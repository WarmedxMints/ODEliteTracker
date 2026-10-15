using EliteJournalReader;
using EliteJournalReader.Events;

namespace ODEliteTracker.Models.Galaxy
{
    public sealed class SystemBody
    {
        public StarSystem Owner { get; }
        public uint BodyID { get; private set; }
        public List<BodyParent>? Parents { get; private set; }
        public StarType StarType { get; private set; }
        public PlanetClass PlanetClass { get; private set; }
        public double Radius { get; private set; }
        public string BodyName { get; private set; }
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

        public double SurfaceGravity { get; private set; }
        public double SurfacePressure { get; private set; }
        public double SurfaceTemp { get; private set; }
        public AtmosphereClass AtmosphereType { get; private set; }
        public AtmosphereDescription Atmosphere { get; private set; }
        public bool Landable { get; set; }
        public double DistanceFromArrivalLs { get; private set; }
        public uint GeoSignalCount { get; private set; }
        public uint BioSignalCount { get; private set; }
        public uint PlanetaryMiningSignalCount { get; private set; }
        public DateTime ScanTime { get; private set; } = DateTime.MinValue;
        public SystemBody(ScanEvent.ScanEventArgs e, StarSystem owner)
        {
            Owner = owner;
            Parents = e.Parents is not null ? [.. e.Parents] : [];
            Radius = e.Radius / 1000 ?? 0;
            StarType = e.StarType;
            PlanetClass = e.PlanetClass;
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            Landable = e.Landable ?? false;
            SurfaceGravity = e.SurfaceGravity / 9.81 ?? 0;
            SurfacePressure = e.SurfacePressure ?? 0;
            SurfaceTemp = e.SurfaceTemperature ?? 0;
            AtmosphereType = e.AtmosphereType;
            Atmosphere = e.Atmosphere;
            BodyName = e.BodyName;
            BodyID = e.BodyID;
            ScanTime = e.Timestamp;
        }

        public SystemBody(LocationEvent.LocationEventArgs args, StarSystem starSystem)
        {
            BodyName = args.Body;
            BodyID = args.BodyID;
            Owner = starSystem;
        }

        public void UpdateFromScan(ScanEvent.ScanEventArgs e)
        {
            BodyName = e.BodyName;
            BodyID = e.BodyID;
            Parents = e.Parents is not null ? [.. e.Parents] : [];
            DistanceFromArrivalLs = e.DistanceFromArrivalLs;
            StarType = e.StarType;
            PlanetClass = e.PlanetClass;
            AtmosphereType = e.AtmosphereType;
            Atmosphere = e.Atmosphere;
            Landable = e.Landable ?? false;
            SurfaceGravity = e.SurfaceGravity / 10 ?? 0;
            SurfacePressure = e.SurfacePressure ?? 0;
            SurfaceTemp = e.SurfaceTemperature ?? 0;
        }
    }
}
