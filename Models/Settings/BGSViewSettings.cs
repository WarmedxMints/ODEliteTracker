﻿namespace ODEliteTracker.Models.Settings
{
    public class BGSViewSettings
    {
        public bool HideSystemsWithoutData { get; set; } = true;
        public int SelectedTab { get; set; }
        public string TopMostBountyFaction { get; set; } = string.Empty;
    }
}
