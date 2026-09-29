using EliteJournalReader;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.Dynamic;

namespace ODEliteTracker.Models.Colonisation
{
    public record class ColonisationRouteSystem
    {
        public ColonisationRouteSystem()
        {
            SystemName = string.Empty;
            Position = new();
            StarClass = string.Empty;
            Notes = string.Empty;
        }

        public ColonisationRouteSystem(ColonisationRouteSystemDTO dto)
        {
            SystemAddress = dto.SystemAddress;
            HopNumber = dto.HopNumber;
            SystemName = dto.SystemName;
            Position = new(dto.X, dto.Y, dto.Z);
            StarClass = dto.StarClass;
            Built = dto.Built;
            Hub = dto.Hub;
            Notes = dto.Notes;
        }

        public ColonisationRouteSystem(ColonisationRouteSystemVM vm)
        {
            SystemAddress = vm.SystemAddress;
            HopNumber = vm.DataBaseHopNumber;
            SystemName = vm.SystemName;
            Position = new(vm.Position.Position);
            StarClass = vm.StarClass;
            Built = vm.Built;
            Hub = vm.Hub;
            Notes = vm.Notes;
        }

        public long SystemAddress { get; set; }
        public int HopNumber { get; set; }
        public string SystemName { get; set; }
        public Position Position { get; set; }
        public string StarClass { get; set; }
        public bool Built { get; set; }
        public bool Hub { get; set; }
        public string Notes { get; set; }
    }
}
