namespace ODEliteTracker.Models.Colonisation.Builds
{
    internal static class Settlement_Tier1
    {
        internal static readonly Dictionary<string, int> Agriculture_Small = new()
        {
            { "$copper_name;", 21 },
            { "$aluminium_name;", 559 },
            { "$fruitandvegetables_name;", 63 },
            { "$grain_name;", 77 },
            { "$foodcartridges_name;", 42 },
            { "$polymers_name;", 91 },
            { "$pesticides_name;", 126 },
            { "$cropharvesters_name;", 210 },
            { "$computercomponents_name;", 14 },
            { "$terrainenrichmentsystems_name;", 70 },
            { "$biowaste_name;", 280 },
            { "$ceramiccomposites_name;", 28 },
            { "$surfacestabilisers_name;", 56 },
            { "$buildingfabricators_name;", 42 },
            { "$structuralregulators_name;", 56 },
            { "$evacuationshelter_name;", 14 },
            { "$liquidoxygen_name;", 224 },
            { "$emergencypowercells_name;", 84 },
            { "$survivalequipment_name;", 14 },
            { "$steel_name;", 768 }
        };
        internal static readonly Dictionary<string, int> Agriculture_Medium = Agriculture_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> Agriculture_Large = Agriculture_Small.ToDictionary(p => p.Key, p => p.Value * 3);

        internal static readonly Dictionary<string, int> Extraction_Small = new()
        {
            { "$copper_name;", 30 },
            { "$aluminium_name;", 632 },
            { "$fruitandvegetables_name;", 30 },
            { "$foodcartridges_name;", 45 },
            { "$polymers_name;", 118 },
            { "$computercomponents_name;", 30 },
            { "$hazardousenvironmentsuits_name;", 15 },
            { "$robotics_name;", 59 },
            { "$bioreducinglichen_name;", 162 },
            { "$ceramiccomposites_name;", 59 },
            { "$surfacestabilisers_name;", 89 },
            { "$geologicalequipment_name;", 52 },
            { "$buildingfabricators_name;", 59 },
            { "$mutomimager_name;", 45 },
            { "$structuralregulators_name;", 74 },
            { "$evacuationshelter_name;", 30 },
            { "$liquidoxygen_name;", 177 },
            { "$emergencypowercells_name;", 96 },
            { "$survivalequipment_name;", 15 },
            { "$steel_name;", 1028 }
        };
        internal static readonly Dictionary<string, int> Extraction_Medium = Extraction_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> Extraction_Large = Extraction_Small.ToDictionary(p => p.Key, p => p.Value * 3);

        internal static readonly Dictionary<string, int> Industrial_Small = new()
        {
            { "$copper_name;", 28 },
            { "$aluminium_name;", 585 },
            { "$fruitandvegetables_name;", 28 },
            { "$foodcartridges_name;", 41 },
            { "$polymers_name;", 109 },
            { "$semiconductors_name;", 14 },
            { "$superconductors_name;", 14 },
            { "$heliostaticfurnaces_name;", 82 },
            { "$mineralextractors_name;", 164 },
            { "$computercomponents_name;", 28 },
            { "$hazardousenvironmentsuits_name;", 14 },
            { "$robotics_name;", 55 },
            { "$advancedcatalysers_name;", 41 },
            { "$resonatingseparators_name;", 14 },
            { "$ceramiccomposites_name;", 55 },
            { "$surfacestabilisers_name;", 68 },
            { "$thermalcoolingunits_name;", 14 },
            { "$buildingfabricators_name;", 55 },
            { "$structuralregulators_name;", 68 },
            { "$evacuationshelter_name;", 21 },
            { "$liquidoxygen_name;", 272 },
            { "$emergencypowercells_name;", 28 },
            { "$microcontrollers_name;", 14 },
            { "$survivalequipment_name;", 14 },
            { "$steel_name;", 1019 }
        };
        internal static readonly Dictionary<string, int> Industrial_Medium = Industrial_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> Industrial_Large = Industrial_Small.ToDictionary(p => p.Key, p => p.Value * 3);

        internal static readonly Dictionary<string, int> Military_Small = new()
        {
            { "$copper_name;", 30 },
            { "$aluminium_name;", 593 },
            { "$fruitandvegetables_name;", 24 },
            { "$foodcartridges_name;", 36 },
            { "$polymers_name;", 119 },
            { "$basicmedicines_name;", 36 },
            { "$computercomponents_name;", 42 },
            { "$battleweapons_name;", 24 },
            { "$reactivearmour_name;", 24 },
            { "$combatstabilisers_name;", 36 },
            { "$ceramiccomposites_name;", 60 },
            { "$surfacestabilisers_name;", 60 },
            { "$buildingfabricators_name;", 60 },
            { "$structuralregulators_name;", 101 },
            { "$evacuationshelter_name;", 36 },
            { "$liquidoxygen_name;", 172 },
            { "$cmmcomposite_name;", 356 },
            { "$emergencypowercells_name;", 24 },
            { "$microcontrollers_name;", 12 },
            { "$militarygradefabrics_name;", 24 },
            { "$survivalequipment_name;", 24 },
            { "$steel_name;", 949 }
        };
        internal static readonly Dictionary<string, int> Military_Medium = Military_Small.ToDictionary(p => p.Key, p => p.Value * 2);
        internal static readonly Dictionary<string, int> Military_Large = Military_Small.ToDictionary(p => p.Key, p => p.Value * 3);
    }
}
