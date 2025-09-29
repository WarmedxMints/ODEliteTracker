using System.ComponentModel;

namespace ODEliteTracker.Models
{
    public enum JournalLogAge
    {
        [Description("Load All")]
        All = 0,
        [Description("< 7 Days")]
        SevenDays,
        [Description("< 30 Days")]
        ThirtyDays,
        [Description("< 60 Days")]
        SixtyDays,
        [Description("< 180 Days")]
        OneHundredEightyDays,
        [Description("< One Year")]
        OneYear,
        [Description("< Two Years")]
        TwoYears,
        [Description("< Three Years")]
        ThreeYears,
        [Description("< Four Years")]
        FourYears,
        [Description("< Five Years")]
        FiveYears,
        [Description("< Six Years")]
        SixYears,
        [Description("< Seven Years")]
        SevenYears,
        [Description("< Eight Years")]
        EightYears,
    }

    public enum CommoditySorting
    {
        ShowAll,
        Name,
        Category,
        Remaining
    }

    public enum CarrierCommoditySorting
    {
        Name,
        Category,
    }

    public enum TradeMissionType
    {
        SourceAndReturn,
        Mining,
        Delivery
    }

    public enum VoucherType
    {
        Unknown,
        Bounty,
        Codex,
        CombatBond,
        Scannable,
        Settlement,
        Trade,
    }

    public enum TransactionType
    {
        Purchase,
        Sale
    }

    public enum  ConflictStatus
    {
        Active,
        Pending,
        Concluded
    }

    public enum FactionConflictStatus
    {
        Winning,
        Losing,
        Draw
    }

    public enum ConflictType
    {
        None,
        LowSpaceCZ,
        MediumSpaceCZ,
        HighSpaceCZ,
        LowGroundCZ,
        MediumGroundCZ,
        HighGroundCZ            
    }

    public enum MissionSorting
    {
        [Description("Accepted Order")]
        AcceptedOrder,
        [Description("Origin System")]
        System, 
        [Description("Origin Station")]
        Station,
        [Description("Issuing Faction")]
        Faction,
        [Description("Target Faction")]
        Target,
        [Description("Total Kills")]
        Kills,
        [Description("Reward")]
        Reward,
        [Description("Expiry Time")]
        Expiry,
        [Description("Wing Mission")]
        Wing
    }

    public enum MassacreStackSorting
    {
        [Description("Issuing Faction")]
        IssuingFaction,
        [Description("Target Faction")]
        TargetFaction,
        [Description("Reward")]
        Reward,
        [Description("Total Kills")]
        Kills,
        [Description("Remaining Kills")]
        Remaining,
        [Description("Kills Difference")]
        Difference,
        [Description("Mission Count")]
        MissionCount
    }

    public enum LandingPadSize
    {
        Small,
        Medium,
        Large
    }

    public enum CsvType
    {
        None = -1,
        [Description("Road To Riches")]
        RoadToRiches = 0,
        [Description("Fleet Carrier Route")]
        FleetCarrier,
        [Description("Neutron Plotter")]
        NeutronRoute,
        [Description("Galaxy Plotter")]
        GalaxyPlotter,
        [Description("World Type Route")]
        WorldTypeRoute,
        [Description("Tourist Route")]
        TouristRoute,
        [Description("Exobiology Route")]
        Exobiology,
        [Description("Exobiology Route (Old Format)")]
        ExobiologyOld,
        [Description("Colonisation Plotter")]
        Colonisation
    }

    public enum CsvTypeComboBox
    {
        [Description("Road To Riches")]
        RoadToRiches = 0,
        [Description("Fleet Carrier Route")]
        FleetCarrier,
        [Description("Neutron Plotter")]
        NeutronRoute,
        [Description("Galaxy Plotter")]
        GalaxyPlotter,
        [Description("World Type Route")]
        WorldTypeRoute,
        [Description("Tourist Route")]
        TouristRoute,
        [Description("Exobiology Route")]
        Exobiology,
        [Description("Colonisation Plotter")]
        Colonisation
    }

    public enum SystemWebSite
    {
        Inara,
        Spansh
    }

    public enum  ShipyardWebSite
    {
        EDSY,
        Coriolis
    }
}
