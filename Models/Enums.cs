﻿using System.ComponentModel;

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

    public enum LandingPadSize
    {
        Small,
        Medium,
        Large
    }
}
