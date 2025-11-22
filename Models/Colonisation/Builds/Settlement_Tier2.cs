namespace ODEliteTracker.Models.Colonisation.Builds
{
    internal static class Settlement_Tier2
    {
        internal static readonly Dictionary<string, int> ResearchBio_Small = new()
        {
            { "$copper_name;", 34 },
            { "$aluminium_name;", 607 },
            { "$fruitandvegetables_name;", 27 },
            { "$foodcartridges_name;", 41 },
            { "$polymers_name;", 115 },
            { "$semiconductors_name;", 21 },
            { "$superconductors_name;", 21 },
            { "$basicmedicines_name;", 14 },
            { "$computercomponents_name;", 41 },
            { "$hazardousenvironmentsuits_name;", 27 },
            { "$advancedcatalysers_name;", 95 },
            { "$ceramiccomposites_name;", 41 },
            { "$surfacestabilisers_name;", 81 },
            { "$geologicalequipment_name;", 27 },
            { "$buildingfabricators_name;", 54 },
            { "$mutomimager_name;", 95 },
            { "$structuralregulators_name;", 81 },
            { "$evacuationshelter_name;", 27 },
            { "$liquidoxygen_name;", 351 },
            { "$emergencypowercells_name;", 27 },
            { "$microcontrollers_name;", 14 },
            { "$medicaldiagnosticequipment_name;", 41 },
            { "$survivalequipment_name;", 14 },
            { "$steel_name;", 945 }
        };
        internal static readonly Dictionary<string, int> ResearchBio_Medium = ResearchBio_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> ResearchBio_Large = ResearchBio_Small.ToDictionary(p => p.Key, p => p.Value * 3);

        internal static readonly Dictionary<string, int> Tourism_Small = new()
        {
            { "$copper_name;", 22 },
            { "$aluminium_name;", 509 },
            { "$fruitandvegetables_name;", 71 },
            { "$animalmeat_name;", 71 },
            { "$fish_name;", 36 },
            { "$tea_name;", 170 },
            { "$coffee_name;", 71 },
            { "$polymers_name;", 114 },
            { "$beer_name;", 311 },
            { "$wine_name;", 156 },
            { "$liquor_name;", 128 },
            { "$computercomponents_name;", 15 },
            { "$ceramiccomposites_name;", 43 },
            { "$surfacestabilisers_name;", 57 },
            { "$buildingfabricators_name;", 29 },
            { "$structuralregulators_name;", 57 },
            { "$evacuationshelter_name;", 22 },
            { "$liquidoxygen_name;", 227 },
            { "$emergencypowercells_name;", 15 },
            { "$survivalequipment_name;", 15 },
            { "$steel_name;", 707 }
        };
        internal static readonly Dictionary<string, int> Tourism_Medium = Tourism_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> Tourism_Large = Tourism_Small.ToDictionary(p => p.Key, p => p.Value * 3);
    }
}
