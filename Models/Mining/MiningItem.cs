namespace ODEliteTracker.Models.Mining
{
    public enum MiningItemType
    {
        Ore,
        Material
    }

    public sealed class MiningItem(string name, MiningItemType type)
    {
        public string Name { get; } = name;
        public MiningItemType Type { get; } = type;
        public int RefinedCount { get; set; }
        public int CollectedCount { get; set; }
        public int Prospected { get; set; }
        public double MinPercentage { get; set; }
        public double MaxPercentage { get; set; }
        public int MotherLoad {  get; set; }
        public int LowContent { get; set; }
        public int MedContent { get; set; }
        public int HighContent { get; set; }
        public int Count => LowContent + MedContent + HighContent;

        public void AddContent(string content)
        {
            if (string.Equals(content, "$AsteroidMaterialContent_Low;"))
            {
                LowContent++;
                return;
            }

            if(string.Equals(content, "$AsteroidMaterialContent_Medium;"))
            {
                MedContent++;
                return;
            }

            if(string.Equals(content, "$AsteroidMaterialContent_High;"))
            {
                HighContent++; 
                return;
            }
        }
    }
}
