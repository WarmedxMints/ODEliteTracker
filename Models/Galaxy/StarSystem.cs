﻿using EliteJournalReader;
using EliteJournalReader.Events;

namespace ODEliteTracker.Models.Galaxy
{
    public class StarSystem
    {
        public StarSystem(LocationEvent.LocationEventArgs evt)
        {
            Name = evt.StarSystem;
            Address = evt.SystemAddress;
            Position = new(evt.StarPos);
            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            ControllingFaction = evt.SystemFaction?.Name;
            SystemAllegiance = evt.SystemAllegiance;
            Security = string.IsNullOrEmpty(evt.SystemSecurity_Localised) ? evt.SystemSecurity : evt.SystemSecurity_Localised;
        }

        public StarSystem(FSDJumpEvent.FSDJumpEventArgs evt)
        {
            Name = evt.StarSystem;
            Address = evt.SystemAddress;
            Position = new(evt.StarPos);
            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            ControllingFaction = evt.SystemFaction?.Name;
            SystemAllegiance = evt.SystemAllegiance;
            Security = string.IsNullOrEmpty(evt.SystemSecurity_Localised) ? evt.SystemSecurity : evt.SystemSecurity_Localised;
        }

        public StarSystem(CarrierJumpEvent.CarrierJumpEventArgs evt)
        {
            Name = evt.StarSystem;
            Address = evt.SystemAddress;
            Position = new(evt.StarPos);
            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            ControllingFaction = evt.SystemFaction?.Name;
            SystemAllegiance = evt.SystemAllegiance;
            Security = string.IsNullOrEmpty(evt.SystemSecurity_Localised) ? evt.SystemSecurity : evt.SystemSecurity_Localised;
        }

        public string Name { get; set; }
        public long Address { get; set; }
        public Position Position { get; set; }
        public string? ControllingPower { get; set; }
        public PowerplayState PowerState { get; set; }
        public string? ControllingFaction { get; set; }
        public string? SystemAllegiance { get; set; }
        public string? Security { get; set; }
        public Dictionary<long, SystemBody> Bodies { get; set; } = [];
        public void UpdateSystem(StarSystem system)
        {
            ControllingPower = system.ControllingPower;
            PowerState = system.PowerState;
            ControllingFaction = system.ControllingFaction;
            SystemAllegiance = system.SystemAllegiance;
            Security = system.Security;
        }

        internal void AddBody(SystemBody body)
        {
            if (Bodies.ContainsKey(body.BodyID))
            {
                return;
            }

            Bodies.TryAdd(body.BodyID, body);
        }

        internal void AddBody(ScanEvent.ScanEventArgs scan)
        {
            if (Bodies.TryGetValue(scan.BodyID, out var body))
            {
                body.UpdateFromScan(scan);
                return;
            }

            Bodies.TryAdd(scan.BodyID, new(scan, this));
        }
    }
}
