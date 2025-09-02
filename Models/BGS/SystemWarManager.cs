namespace ODEliteTracker.Models.BGS
{
    public record SystemWarZone(DateTime TimeCompleted, ConflictType Type, string SupportingFaction, string SettlementName);

    public sealed class SystemWarManager
    {
        private ConflictType conflictType;
        private bool earnedBonds;
        private string supportingFaction = string.Empty;
        private string settlementName = string.Empty;

        public bool HasConflict()
        {
            if(conflictType == ConflictType.None)
                return false;

            if(conflictType >= ConflictType.LowGroundCZ)
            {
                return earnedBonds && !string.IsNullOrEmpty(supportingFaction)
                    && !string.IsNullOrEmpty(settlementName);
            }

            return earnedBonds && !string.IsNullOrEmpty(supportingFaction);
        }

        public void OnDestinationDrop(string destinationType)
        {
            if (destinationType.StartsWith("$Warzone_PointRace") == false)
                return;

            if (destinationType.StartsWith("$Warzone_PointRace_Low"))
            {
                conflictType = ConflictType.LowSpaceCZ;
                return;
            }

            if (destinationType.StartsWith("$Warzone_PointRace_Med"))
            {
                conflictType = ConflictType.MediumSpaceCZ;
                return;
            }

            conflictType = ConflictType.HighSpaceCZ;
        }

        public void OnApproachSettlement(string settlementName)
        {
            this.settlementName = settlementName;
        }

        public void OnEarnedBonds(int value, string awardingFaction)
        {
            earnedBonds = true;
            supportingFaction = awardingFaction;
            if (string.IsNullOrEmpty(settlementName) == false)
            {
                var bond = GroundBondToConflictType(value);

                if(bond > conflictType)                    
                { 
                    conflictType = bond; 
                }
            }
        }

        internal static ConflictType GroundBondToConflictType(int bondValue)
        {
            if (bondValue > 35000)
            {
                return ConflictType.HighGroundCZ;
            }

            if (bondValue > 10000)
            {
                return ConflictType.MediumGroundCZ;

            }

            return ConflictType.LowGroundCZ;
        }

        public SystemWarZone GetConflictZone(DateTime timeCompleted)
        {
            var ret = new SystemWarZone(timeCompleted, conflictType, supportingFaction, settlementName);
            Reset();
            return ret;
        }

        public void Reset()
        {
            conflictType = ConflictType.None;
            supportingFaction = string.Empty;
            settlementName = string.Empty;
            earnedBonds = false;
        }
    }
}
