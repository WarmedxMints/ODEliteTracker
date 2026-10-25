using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.ComponentModel;

namespace ODEliteTracker.Models.Colonisation.Builds
{
    public enum AssetEconomy
    {
        None,
        Agriculture,
        Colony,
        Extraction,
        Industrial,
        HighTech,
        Military,
        Refinery,
        Service,
        Tourism
    }

    public enum PointTier
    {
        None,
        [Description("T2")]
        Tier2,
        [Description("T3")]
        Tier3,
    }

    internal class ColonisationAssetStats
    {
        private static readonly List<AssetStats> Outpost =
            [
                new ("Commercial Outpost", [ new("Plutus", 3, 1)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 1, 1, -1, 3, 0, 5, 0, 3, AssetFilter.SpaceOutpost),
                new ("Industrial Outpost", [ new("Vulcan", 2, 1)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 3, 0, 3, 3, AssetFilter.SpaceOutpost),
                new ("Pirate Outpost", [ new("Dysnomia", 2, 1)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Service, 1, 1, -2, 3, 0, 0, 0, 3, AssetFilter.SpaceOutpost),
                new ("Civilian Outpost", [ new("Vesta", 4, 1, 9)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 1, 1, -1, 1, 0, 2, 1, 3, AssetFilter.SpaceOutpost),
                new ("Scientific Outpost", [ new("Prometheus", 2, 1)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.HighTech, 1, 1, 0, 0, 3, 0, 0, 3, AssetFilter.SpaceOutpost),
                new ("Military Outpost", [ new("Nemesis", 2, 1)], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 2, 0, 0, 0, 0, 3, AssetFilter.SpaceOutpost)
            ];

        private static readonly List<AssetStats> StarportT2 =
            [
                new ("Coriolis StarPort", [ new("No Truss", 8, 11, 5), new("Dual Truss", 8, 11, 5), new("Quad Truss", 8, 11, 6)], LandingPadSize.Large, 2, 3, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Colony, 1, 1, -2, 3, 2, 3, 3, 8, AssetFilter.Starport),
                new ("Asteroid StarPort", [ new("Asteroid", 8, 1, 5)], LandingPadSize.Large, 2, 3, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, -1, 5, 3, -4, 7, 8, AssetFilter.Starport)
            ];

        private static readonly List<AssetStats> StarportT3 =
            [
                new ("Ocellus StarPort", [ new("Ocellus", 8, 11, 5) ], LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 5, 1, -3, 8, 7, 5, 9, 15, AssetFilter.Starport),
                new ("Orbis StarPort", [ new("Apollo", 8, 11, 5), new("Artemis", 8, 11, 5)], LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 5, 1, -3, 8, 7, 5, 9, 15, AssetFilter.Starport)
            ];

        private static readonly List<AssetStats> Dodec =
            [
                new("Dodec", [ new("Dodec", 4, 11, 6), new("Quint Truss", 4, 11, 6), new("Dec Truss", 4, 11, 6)], LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 8, 4, -4, 9, 8, 7, 10, 15, AssetFilter.Starport)
            ];

        private static readonly List<AssetStats> Satellite_Comms =
            [
                new("Satellite Installation", [ new("Angelia"), new("Eirene"), new("Hermes") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.None, 0, 0, 0, 1, 0, 2, 1, 3, AssetFilter.SpaceInstallation),
                new("Communication Installation", [ new("Alethia"), new("Pistis"), new("Soter") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.None, 0, 0, 1, 0, 3, 0, 0, 3, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> SpaceFarm =
            [
                new("Space Farm Installation", [ new("Demeter") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 0, 0, 0, 0, 0, 5, 1, 2, AssetFilter.SpaceInstallation),
            ];

        private static readonly List<AssetStats> PirateInstallation =
            [
                new("Pirate Base Installation", [ new("Apate"), new("Laverna") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Service, 0, 0, -4, 4, 0, 0, 0, 3, AssetFilter.SpaceInstallation),
            ];

        private static readonly List<AssetStats> MiningOutpost =
            [
                new("Mining Installation", [ new("Euthenia"), new("Phorcys") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 0, 0, 0, 4, 0, -2, 0, 2, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> RelayStation =
            [
                new("Relay Installation", [ new("Enodia"), new("Ichnaea") ], LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 0, 0, 1, 0, 0, 0, 1, 3, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> MilitaryInstallation =
            [
                new("Military Installation", [ new("Alastor"), new("Vacuna") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 0, 0, 7, 0, 0, 0, 0, 4, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> SecurityStation =
            [
                new("Security Installation", [ new("Dicaeosyne"), new("Eunomia"), new("Nomos"), new("Poena") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 0, 0, 8, 0, 0, 3, 3, 3, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> GovernmentInstallation =
            [
                new("Government Installation", [ new("Harmonia") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 0, 0, 2, 0, 0, 7, 3, 3, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> MedicalInstallation =
            [
                new("Medical Installation", [ new("Asclepius"), new("Eupraxia") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 0, 0, 0, 0, 3, 5, 0, 4, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> ResearchStation =
            [
                new("Research Installation", [ new("Astraeus"), new("Coeus"), new("Dione"), new("Dodona") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 0, 0, 0, 0, 8, 0, 3, 4, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> TouristInstallation =
            [
                new("Tourist Installation", [ new("Hedone"), new("Opora"), new("Pasithea") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 0, 0, -3, 6, 0, 0, 3, 3, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> BarInstallation =
            [
                new("Space Bar Installation", [ new("Bacchus"), new("Dionysus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 0, 0, -2, 3, 0, 3, 0, 4, AssetFilter.SpaceInstallation)
            ];

        private static readonly List<AssetStats> SurfaceOutpostT1 =
            [
                new("Civilian Surface Outpost", 
                    [
                        new ("Atropos", 4, 2, 2),
                        new ("Clotho", 4, 4, 2),
                        new ("Decima", 4, 4, 4),
                        new ("Hestia", 4, 4, 4),
                        new ("Lachesis", 4, 2, 2),
                        new ("Nona", 4, 4, 4)
                    ],
                    LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 2, 1, -2, 0, 0, 3, 0, 4, AssetFilter.SurfaceOutpost),
                new("Industrial Surface Outpost",
                    [
                        new ("Bia", 4, 2, 2),
                        new ("Hephaestus", 4, 4, 4),
                        new ("Mefitis", 4, 4, 2),
                        new ("Opis", 4, 4, 4),
                        new ("Ponos", 4, 2, 2),
                        new ("Tethys", 4, 4, 4)
                    ],
                    LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, -1, 3, 0, 0, 0, 4, AssetFilter.SurfaceOutpost),
                new("Scientific Surface Outpost",
                    [
                        new ("Ananke", 4, 4, 4),
                        new ("Antevorta", 4, 2, 2),
                        new ("Fauna", 4, 2, 2),
                        new ("Necessitas", 4, 4, 4),
                        new ("Porrima", 4, 4, 2),
                        new ("Providentia", 4, 4, 4)
                    ],
                    LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.HighTech, 1, 1, -1, 0, 5, 0, 1, 4, AssetFilter.SurfaceOutpost)
            ];

        private static readonly List<AssetStats> SurfaceOutpostT3 =
            [
                new("Large Planetary Port",
                    [
                        new("Aphrodite", 4, 4, 8),
                        new("Hera", 4, 4, 8),
                        new("Poseidon", 4, 4, 8),
                        new("Zeus", 4, 4, 8)
                    ],
                    LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 10, 10, -3, 5, 5, 7, 10, 15, AssetFilter.SurfaceOutpost)

            ];

        private static readonly List<AssetStats> SettlementAgriSmall =
            [
                new("Agriculture Settlement [Small]", [ new("Consus", 1) ], LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 5, 0, 1, AssetFilter.Settlement)

            ];

        private static readonly List<AssetStats> SettlementAgriMedium =
            [
                new("Agriculture Settlement [Medium]", [ new("Annona", 1), new("Picumnus", 1, 0, 1) ], LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 7, 0, 2, AssetFilter.Settlement)

            ];

        private static readonly List<AssetStats> SettlementAgriLarge =
            [
                new("Agriculture Settlement [Large]", [ new("Ceres", 1, 0, 1), new("Fornax", 2, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 10, 0, 4, AssetFilter.Settlement)

            ];

        private static readonly List<AssetStats> SettlementExtractionSmall =
            [
                new("Mining Settlement [Small]", [ new("Ourea", 1) ], LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 1, 1, 0, 3, 0, 0, 0, 1, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementExtractionMedium =
            [
                new("Mining Settlement [Medium]", [ new("Mantus", 0, 1), new("Orcus", 1, 0, 1) ], LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 1, 1, 0, 5, 0, 0, 0, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementExtractionLarge =
            [
                new("Mining Settlement [Large]", [ new("Aerecura", 0, 0, 1), new("Erebus", 1, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, 0, 8, 2, -2, 0, 4, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementIndustrialSmall =
            [
                new("Industrial Settlement [Small]", [ new("Fontus", 1) ], LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 0, 0, 3, 1, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementIndustrialMedium =
            [
                new("Industrial Settlement [Medium]", [ new("Meteope", 1, 0, 1), new("Minthe", 0, 1), new("Palici", 0, 0, 1) ], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 0, 0, 6, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementIndustrialLarge =
            [
                new("Industrial Settlement [Large]", [ new("Gaea",0, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Industrial, 1, 1, 0, 3, 0, 0, 9, 4, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementMiltarySmall =
            [
                new("Military Settlement [Small]", [ new("Fontus", 1) ], LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 2, 0, 0, 0, 0, 1, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementMiltaryMedium =
            [
                new("Military Settlement [Medium]", [ new("Bellona", 1), new("Enyo", 1, 1), new("Polemos", 1, 1) ], LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 4, 0, 0, 0, 0, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementMiltaryLarge =
            [
                new("Military Settlement [Large]", [ new("Minerva", 2, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Military, 1, 1, 7, 0, 0, 0, 3, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementResearchBioSmall =
            [
                new("Research Bio Settlement [Small]", [ new("Pheobe",1 ) ], LandingPadSize.Small, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 3, 0, 1, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementResearchBioMedium =
            [
                new("Research Bio Settlement [Medium]", [ new("Asteria", 1), new("Caerus", 3) ], LandingPadSize.Small, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 7, 0, 1, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementResearchBioLarge =
            [
                new("Research Bio Settlement [Large]", [ new("Chronos", 1, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 10, 0, 3, 4, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementTourismSmall =
            [
                new("Tourist Settlement [Small]", [ new("Aergia", 0, 1) ], LandingPadSize.Medium, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 1, 0, 0, 0, 1, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementTourismMedium =
            [
                new("Tourist Settlement [Medium]", [ new("Cornus", 2, 0, 1), new("Gelos", 1, 0, 1) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 3, 0, 0, 0, 2, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> SettlementTourismLarge =
            [
                new("Tourist Settlement [Large]", [ new("Fufluns", 2, 0, 2) ], LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 5, 0, 0, 0, 4, AssetFilter.Settlement)
            ];

        private static readonly List<AssetStats> HubExtraction =
            [
                new("Extraction Hub", [ new("Tartarus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, 0, 10, 0, -4, 3, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubCivilian =
            [
                new("Civilian Hub", [ new("Aegle") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 1, 1, -3, 0, 0, 3, 3, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubExploration =
            [
                new("Exploration Hub", [ new("Tellus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 0, 7, 0, 3, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubOutpost =
            [
                new("Outpost Hub", [ new("Io") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 1, 1, -2, 0, 0, 3, 3, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubScientific =
            [
                new("Scientific Hub", [ new("Athena"), new("Caelus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 10, 0, 0, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubMilitary =
            [
                new("Military Hub", [ new("Alala"), new("Ares") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 1, 1, 10, 0, 0, 0, 0, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubRefinery =
            [
                new("Refinery Hub", [ new("Silenus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Refinery, 1, 1, -1, 5, 3, -2, 7, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubHighTech =
            [
                new("High Tech Hub", [ new("Janus") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, -2, 3, 10, 0, 0, 5, AssetFilter.Hub)
            ];

        private static readonly List<AssetStats> HubIndustrial =
            [
                new("Industrial Hub", [ new("Eunostus"), new("Molae"), new("Tellud") ], LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Industrial, 1, 1, 0, 5, 3, -4, 3, 5, AssetFilter.Hub)
            ];

        public static List<AssetStats> GetAssestStats(ColonisationBuild build)
        {
            return build switch
            {
                ColonisationBuild.Starport_T2P or ColonisationBuild.Starport_T2 => StarportT2,
                ColonisationBuild.Starport_T3P or ColonisationBuild.Starport_T3 => StarportT3,
                ColonisationBuild.DodecP or ColonisationBuild.Dodec => Dodec,
                ColonisationBuild.Satellite_Comms => Satellite_Comms,
                ColonisationBuild.SpaceFarm => SpaceFarm,
                ColonisationBuild.PirateInstallation => PirateInstallation,
                ColonisationBuild.MiningOutpost => MiningOutpost,
                ColonisationBuild.RelayStation => RelayStation,
                ColonisationBuild.MilitaryInstallation => MilitaryInstallation,
                ColonisationBuild.SecurityStation => SecurityStation,
                ColonisationBuild.GovernmentInstallation => GovernmentInstallation,
                ColonisationBuild.MedicalInstallation => MedicalInstallation,
                ColonisationBuild.ResearchStation => ResearchStation,
                ColonisationBuild.TouristInstallation => TouristInstallation,
                ColonisationBuild.BarInstallation => BarInstallation,
                ColonisationBuild.SurfaceOutpostT1 => SurfaceOutpostT1,
                ColonisationBuild.SurfaceOutpostT3 => SurfaceOutpostT3,
                ColonisationBuild.SettlementAgriSmall => SettlementAgriSmall,
                ColonisationBuild.SettlementAgriMedium => SettlementAgriMedium,
                ColonisationBuild.SettlementAgriLarge => SettlementAgriLarge,
                ColonisationBuild.SettlementExtractionSmall => SettlementExtractionSmall,
                ColonisationBuild.SettlementExtractionMedium => SettlementExtractionMedium,
                ColonisationBuild.SettlementExtractionLarge => SettlementExtractionLarge,
                ColonisationBuild.SettlementIndustrialSmall => SettlementIndustrialSmall,
                ColonisationBuild.SettlementIndustrialMedium => SettlementIndustrialMedium,
                ColonisationBuild.SettlementIndustrialLarge => SettlementIndustrialLarge,
                ColonisationBuild.SettlementMiltarySmall => SettlementMiltarySmall,
                ColonisationBuild.SettlementMiltaryMedium => SettlementMiltaryMedium,
                ColonisationBuild.SettlementMiltaryLarge => SettlementMiltaryLarge,
                ColonisationBuild.SettlementResearchBioSmall => SettlementResearchBioSmall,
                ColonisationBuild.SettlementResearchBioMedium => SettlementResearchBioMedium,
                ColonisationBuild.SettlementResearchBioLarge => SettlementResearchBioLarge,
                ColonisationBuild.SettlementTourismSmall => SettlementTourismSmall,
                ColonisationBuild.SettlementTourismMedium => SettlementTourismMedium,
                ColonisationBuild.SettlementTourismLarge => SettlementTourismLarge,
                ColonisationBuild.HubExtraction => HubExtraction,
                ColonisationBuild.HubCivilian => HubCivilian,
                ColonisationBuild.HubExploration => HubExploration,
                ColonisationBuild.HubOutpost => HubOutpost,
                ColonisationBuild.HubScientific => HubScientific,
                ColonisationBuild.HubMilitary => HubMilitary,
                ColonisationBuild.HubRefinery => HubRefinery,
                ColonisationBuild.HubHighTech => HubHighTech,
                ColonisationBuild.HubIndustrial => HubIndustrial,
                _ => Outpost,
            };
        }

        public static List<AssetStats> GetAll()
        {
            var ret = new List<AssetStats>();

            var build = Enum.GetValues<ColonisationBuild>();

            foreach (var item in build)
            {
                switch (item)
                {
                    case ColonisationBuild.OutpostP:
                    case ColonisationBuild.Starport_T2P:
                    case ColonisationBuild.Starport_T3P:
                    case ColonisationBuild.DodecP:
                        continue;
                }
                ret.AddRange(GetAssestStats(item));
            }

            return ret;
        }
    }
}
