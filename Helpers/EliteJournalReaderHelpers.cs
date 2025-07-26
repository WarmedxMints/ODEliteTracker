namespace ODEliteTracker.Helpers
{
    public static class EliteJournalReaderHelpers
    {
        public static string LandPadText(this EliteJournalReader.LandingPads landingPads)
        {
            if (landingPads.Large > 0)
            {
                return "Pad Size : Large";
            }
            if (landingPads.Medium > 0)
            {
                return "Pad Size : Medium";
            }

            return "Pad Size : Small";
        }

        public static string StationTypeText(string stationType)
        {
            return stationType switch
            {
                "AsteroidBase" => "Asteroid Base",
                "Coriolis" or "Orbis" or "Outpost" or "Ocellus" => stationType,
                "OutpostScientific" => "Scientific Outpost",
                "SurfaceStation" => "Surface Port",
                "FleetCarrier" => "Fleet Carrier",
                "OnFootSettlement" => "Odyssey Settlement",
                "MegaShip" => "Mega Ship",
                "MegaShipCivilian" => "Civilian Mega Ship",
                "CraterOutpost" => "Crater Port",
                "CraterPort" => "Crater Port",
                "PlanetaryConstructionDepot" => "Surface Construction Depot",
                "SpaceConstructionDepot" => "Orbital Construction Depot",
                _ => stationType,
            };
        }
    }
}
