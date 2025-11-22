using ODEliteTracker.Models;
using System.ComponentModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
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
                new ("Commercial", "Plutus", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 1, 1, -1, 3, 0, 5, 0, 3),
                new ("Industrial", "Vulcan", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 3, 0, 3, 3),
                new ("Pirate", "Dysnomia", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Service, 1, 1, -2, 3, 0, 0, 0, 3),
                new ("Civilian", "Vesta", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 1, 1, -1, 1, 0, 2, 1, 3),
                new ("Scientific", "Prometheus", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.HighTech, 1, 1, 0, 0, 3, 0, 0, 3),
                new ("Military", "Nemesis", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 2, 0, 0, 0, 0, 3)
            ];

        private static readonly List<AssetStats> StarportT2 =
            [
                new ("Coriolis", "No Truss, Dual Truss, Quad Truss", LandingPadSize.Large, 2, 3, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Colony, 1, 1, -2, 3, 2, 3, 3, 8),
                new ("Asteroid", "Asteroid", LandingPadSize.Large, 2, 3, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, -1, 5, 3, -4, 7, 8)
            ];

        private static readonly List<AssetStats> StarportT3 =
            [
                new ("Ocellus", "Ocellus", LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 5, 1, -3, 8, 7, 5, 9, 15),
                new ("Orbis", "Apollo, Artemis", LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 5, 1, -3, 8, 7, 5, 9, 15)
            ];

        private static readonly List<AssetStats> Dodec =
            [
                new("Dodec", "Dodec, Quint Truss, Dec Truss", LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 8, 4, -4, 9, 8, 7, 10, 15)
            ];

        private static readonly List<AssetStats> Satellite_Comms =
            [
                new("Satellite Installation", "Angelia, Eirene, Hermes", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.None, 0, 0, 0, 1, 0, 2, 1, 3),
                new("Communication Installation", "Alethia, Pistis, Soter", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.None, 0, 0, 1, 0, 3, 0, 0, 3)
            ];

        private static readonly List<AssetStats> SpaceFarm =
            [
                new("Space Farm Installation", "Demeter", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 0, 0, 0, 0, 0, 5, 1, 2),
            ];

        private static readonly List<AssetStats> PirateInstallation =
            [
                new("Pirate Base Installation", "Apate, Laverna", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Service, 0, 0, -4, 4, 0, 0, 0, 3),
            ];

        private static readonly List<AssetStats> MiningOutpost =
            [
                new("Mining Installation", "Euthenia, Phorcys", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 0, 0, 0, 4, 0, -2, 0, 2)
            ];

        private static readonly List<AssetStats> RelayStation =
            [
                new("Relay Installation", "Enodia, Ichnaea", LandingPadSize.None, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 0, 0, 1, 0, 0, 0, 1, 3)
            ];

        private static readonly List<AssetStats> MilitaryInstallation =
            [
                new("Military Installation", "Alastor, Vacuna", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 0, 0, 7, 0, 0, 0, 0, 4)
            ];

        private static readonly List<AssetStats> SecurityStation =
            [
                new("Security Installation", "Dicaeosyne, Eunomia, Nomos, Poena", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 0, 0, 8, 0, 0, 3, 3, 3)
            ];

        private static readonly List<AssetStats> GovernmentInstallation =
            [
                new("Government Installation", "Harmonia", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 0, 0, 2, 0, 0, 7, 3, 3)
            ];

        private static readonly List<AssetStats> MedicalInstallation =
            [
                new("Medical Installation", "Asclepius, Eupraxia", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 0, 0, 0, 0, 3, 5, 0, 4)
            ];

        private static readonly List<AssetStats> ResearchStation =
            [
                new("Research Installation", "Astraeus, Coeus, Dione, Dodona", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 0, 0, 0, 0, 8, 0, 3, 4)
            ];

        private static readonly List<AssetStats> TouristInstallation =
            [
                new("Tourist Installation", "Hedone, Opora, Pasithea", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 0, 0, -3, 6, 0, 0, 3, 3)
            ];

        private static readonly List<AssetStats> BarInstallation =
            [
                new("Space Bar Installation", "Bacchus, Dionysus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 0, 0, -2, 3, 0, 3, 0, 4)
            ];

        private static readonly List<AssetStats> SurfaceOutpostT1 =
            [
                new("Civilian Surface Outpost", "Atropos, Clotho, Decima, Hestia, Lachesis, Nona", LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Colony, 2, 1, -2, 0, 0, 3, 0, 4),
                new("Industrial Surface Outpost", "Bia, Hephaestus, Mefitis, Opis, Ponos, Tethys", LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, -1, 3, 0, 0, 0, 4),
                new("Scientific Surface Outpost", "Ananke, Antevorta, Fauna, Necessitas, Porrima, Providentia", LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.HighTech, 1, 1, -1, 0, 5, 0, 1, 4)
            ];

        private static readonly List<AssetStats> SurfaceOutpostT3 =
            [
                new("Large Planetary Port", "Aphrodite, Hera, Poseidon, Zeus", LandingPadSize.Large, 3, 6, PointTier.Tier3, 0, PointTier.None, AssetEconomy.Colony, 10, 10, -3, 5, 5, 7, 10, 15)

            ];

        private static readonly List<AssetStats> SettlementAgriSmall =
            [
                new("Agriculture Settlement [Small]", "Consus", LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 5, 0, 1)

            ];

        private static readonly List<AssetStats> SettlementAgriMedium =
            [
                new("Agriculture Settlement [Medium]", "Annona, Picumnus", LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 7, 0, 2)

            ];

        private static readonly List<AssetStats> SettlementAgriLarge =
            [
                new("Agriculture Settlement [Large]", "Ceres, Fornax", LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Agriculture, 1, 1, 0, 0, 0, 10, 0, 4)

            ];

        private static readonly List<AssetStats> SettlementExtractionSmall =
            [
                new("Mining Settlement [Small]", "Ourea", LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 1, 1, 0, 3, 0, 0, 0, 1)
            ];

        private static readonly List<AssetStats> SettlementExtractionMedium =
            [
                new("Mining Settlement [Medium]", "Mantus, Orcus", LandingPadSize.Large, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Extraction, 1, 1, 0, 5, 0, 0, 0, 2)
            ];

        private static readonly List<AssetStats> SettlementExtractionLarge =
            [
                new("Mining Settlement [Large]", "Aerecura, Erebus", LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, 0, 8, 2, -2, 0, 4)
            ];

        private static readonly List<AssetStats> SettlementIndustrialSmall =
            [
                new("Industrial Settlement [Small]", "Fontus", LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 0, 0, 3, 1)
            ];

        private static readonly List<AssetStats> SettlementIndustrialMedium =
            [
                new("Industrial Settlement [Medium]", "Meteope, Minthe, Palici", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Industrial, 1, 1, 0, 0, 0, 0, 6, 2)
            ];

        private static readonly List<AssetStats> SettlementIndustrialLarge =
            [
                new("Industrial Settlement [Large]", "Gaea", LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Industrial, 1, 1, 0, 3, 0, 0, 9, 4)
            ];

        private static readonly List<AssetStats> SettlementMiltarySmall =
            [
                new("Military Settlement [Small]", "Fontus", LandingPadSize.Small, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 2, 0, 0, 0, 0, 1)
            ];

        private static readonly List<AssetStats> SettlementMiltaryMedium =
            [
                new("Military Settlement [Medium]", "Bellona, Enyo, Polemos", LandingPadSize.Medium, 1, 0, PointTier.None, 1, PointTier.Tier2, AssetEconomy.Military, 1, 1, 4, 0, 0, 0, 0, 2)
            ];

        private static readonly List<AssetStats> SettlementMiltaryLarge =
            [
                new("Military Settlement [Large]", "Minerva", LandingPadSize.Large, 2, 1, PointTier.Tier2, 2, PointTier.Tier3, AssetEconomy.Military, 1, 1, 7, 0, 0, 0, 3, 2)
            ];

        private static readonly List<AssetStats> SettlementResearchBioSmall =
            [
                new("Research Bio Settlement [Small]", "Pheobe", LandingPadSize.Small, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 3, 0, 1, 2)
            ];

        private static readonly List<AssetStats> SettlementResearchBioMedium =
            [
                new("Research Bio Settlement [Medium]", "Asteria, Caerus", LandingPadSize.Small, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 7, 0, 1, 2)
            ];

        private static readonly List<AssetStats> SettlementResearchBioLarge =
            [
                new("Research Bio Settlement [Large]", "Chronos", LandingPadSize.Large, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 10, 0, 3, 4)
            ];

        private static readonly List<AssetStats> SettlementTourismSmall =
            [
                new("Tourist Settlement [Small]", "Aergia", LandingPadSize.Medium, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 1, 0, 0, 0, 1)
            ];

        private static readonly List<AssetStats> SettlementTourismMedium =
            [
                new("Tourist Settlement [Medium]", "Cornus, Gelos", LandingPadSize.Large, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 3, 0, 0, 0, 2)
            ];

        private static readonly List<AssetStats> SettlementTourismLarge =
            [
                new("Tourist Settlement [Large]", "Fufluns", LandingPadSize.Large, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 5, 0, 0, 0, 4)
            ];

        private static readonly List<AssetStats> HubExtraction =
            [
                new("Extraction Hub", "Tartarus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Extraction, 1, 1, 0, 10, 0, -4, 3, 5)
            ];

        private static readonly List<AssetStats> HubCivilian =
            [
                new("Civilian Hub", "Aegle", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 1, 1, -3, 0, 0, 3, 3, 5)
            ];

        private static readonly List<AssetStats> HubExploration =
            [
                new("Exploration Hub", "Tellus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Tourism, 1, 1, -1, 0, 7, 0, 3, 5)
            ];

        private static readonly List<AssetStats> HubOutpost =
            [
                new("Outpost Hub", "Io", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.None, 1, 1, -2, 0, 0, 3, 3, 5)
            ];

        private static readonly List<AssetStats> HubScientific =
            [
                new("Scientific Hub", "Athena, Caelus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, 0, 0, 10, 0, 0, 5)
            ];

        private static readonly List<AssetStats> HubMilitary =
            [
                new("Military Hub", "Alala, Ares", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Military, 1, 1, 10, 0, 0, 0, 0, 5)
            ];

        private static readonly List<AssetStats> HubRefinery =
            [
                new("Refinery Hub", "Silenus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Refinery, 1, 1, -1, 5, 3, -2, 7, 5)
            ];

        private static readonly List<AssetStats> HubHighTech =
            [
                new("High Tech Hub", "Janus", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.HighTech, 1, 1, -2, 3, 10, 0, 0, 5)
            ];

        private static readonly List<AssetStats> HubIndustrial =
            [
                new("Industrial Hub", "Eunostus, Molae, Tellud", LandingPadSize.None, 2, 1, PointTier.Tier2, 1, PointTier.Tier3, AssetEconomy.Industrial, 1, 1, 0, 5, 3, -4, 3, 5)
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
    }
}
