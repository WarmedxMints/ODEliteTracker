using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODMVVM.Helpers;
using System.Text.Json.Serialization;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class ApiTradePlannerStationVM
    {
        public ApiTradePlannerStationVM(ApiTradePlannerStation station) 
        {
            SystemName = station.System;
            StationName = station.Station;
            Position = new(new Models.Galaxy.Position(station.X, station.Y, station.Z));
            DistanceToArrival = station.DistanceToArrival;
            MarketId = station.MarketId;
            UpdateEpoch = station.MarketUpdatedAt;
        }

        public ApiTradePlannerStationVM(ApiTradePlannerStationVM vm)
        {
            SystemName = vm.SystemName;
            StationName = vm.StationName;
            Position = new(vm.Position.Position);
            DistanceToArrival = vm.DistanceToArrival;
            MarketId = vm.MarketId;
            UpdateEpoch = vm.UpdateEpoch;
        }

        public ApiTradePlannerStationVM(SpanshSystem system, SpanshStation station)
        {
            SystemName = system.Name;
            StationName = station.Name ?? string.Empty;
            Position = new(new Models.Galaxy.Position(system.Coords.X, system.Coords.Y, system.Coords.Z));
            DistanceToArrival = station.DistanceToArrival ?? 0;
            MarketId = station.Id;
            UpdateEpoch = station.UpdateTime.ToUnixTimeSeconds();
        }

        [JsonConstructor]
        public ApiTradePlannerStationVM()
        {
            SystemName = string.Empty;
            StationName = string.Empty;
            Position = new(new Models.Galaxy.Position());
        }

        public string SystemName { get; set; }
        public string StationName { get; set; }
        public SystemPositionVM Position { get; set; }
        public double DistanceToArrival { get; set; } 
        public ulong MarketId { get; set; }
        public long UpdateEpoch { get; set; }
        [JsonIgnore]
        public string LastUpdate => EliteHelpers.EpochToString(UpdateEpoch);
        [JsonIgnore]
        public string DistanceToArrivalString => $"{DistanceToArrival:N0} ls";
    }
}
