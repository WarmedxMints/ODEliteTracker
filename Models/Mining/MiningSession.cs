using EliteJournalReader.Events;
using ODMVVM.Helpers.IO;
using System.Text.Json.Serialization;

namespace ODEliteTracker.Models.Mining
{
    public sealed class MiningSession(string starSystem, string location, ulong systemAddress, long bodyID)
    {
        public string StarSystem { get; } = starSystem;
        public ulong SystemAddress { get; } = systemAddress;
        public long BodyID { get; } = bodyID;
        public string Location { get; } = location;
        public DateTime TimeStarted { get; set; } = DateTime.MaxValue;
        public DateTime TimeFinished { get; set; } = DateTime.MaxValue;
        public List<MiningItem> Items { get; set; } = [];
        public List<MiningProspector> Prospectors { get; set; } = [];
        public int AsteroidsProspected { get; set; }
        public int AsteroidsCracked { get; set; }
        public int ProspectorsFired { get; set; }
        public int CollectorsDeployed { get; set; }
        public int LowContent { get; set; }
        public int MedContent { get; set; }
        public int HighContent { get; set; }

        [JsonIgnore]
        internal bool HasData
        {
            get
            {
                return ProspectorsFired > 0
                    || CollectorsDeployed > 0
                    || AsteroidsProspected > 0
                    || Items.Where(x => x.Type == MiningItemType.Ore).Any();
            }
        }

        internal MiningSession? Clone()
        {
            var ret = Json.CloneObject(this);

            return ret;
        }


        internal void CheckStartTime(DateTime timestamp)
        {
            if (timestamp < TimeStarted)
            {
                TimeStarted = timestamp;
            }
        }

        internal void AddAsteroid(ProspectedAsteroidEvent.ProspectedAsteroidEventArgs e)
        {
            //Ignore empty rocks
            if (e.Remaining <= 0)
                return;

            AsteroidsProspected++;

            if (string.IsNullOrEmpty(e.MotherlodeMaterial) == false)
            {
                var item = ODMVVM.Helpers.EliteCommodityHelpers.GetCommodityFromPartial(e.MotherlodeMaterial, e.MotherlodeMaterial);

                var known = Items.FirstOrDefault(x => string.Equals(x.Name, item.EnglishName));

                if (known == null)
                {
                    known = new(item.EnglishName, MiningItemType.Ore);
                    Items.Add(known);
                }
                known.MotherLoad++;
                known.Prospected++;
                known.AddContent(e.Content);
            }

            foreach (var material in e.Materials)
            {
                var item = ODMVVM.Helpers.EliteCommodityHelpers.GetCommodityFromPartial(material.Name, material.Name);

                var known = Items.FirstOrDefault(x => string.Equals(x.Name, item.EnglishName));

                if (known == null)
                {
                    known = new(item.EnglishName, MiningItemType.Ore);
                    Items.Add(known);
                }

                if (known.MinPercentage == 0 || known.MinPercentage > material.Proportion)
                {
                    known.MinPercentage = material.Proportion;
                }

                known.MaxPercentage = double.Max(known.MaxPercentage, material.Proportion);
                known.Prospected++;
                known.AddContent(e.Content);
            }

            if (string.Equals(e.Content, "$AsteroidMaterialContent_Low;"))
            {
                LowContent++;
                return;
            }

            if (string.Equals(e.Content, "$AsteroidMaterialContent_Medium;"))
            {
                MedContent++;
                return;
            }

            if (string.Equals(e.Content, "$AsteroidMaterialContent_High;"))
            {
                HighContent++;
                return;
            }
        }

        internal void AddOre(MiningRefinedEvent.MiningRefinedEventArgs miningRefined)
        {
            var item = ODMVVM.Helpers.EliteCommodityHelpers.GetCommodityDetails(miningRefined.Type);

            var known = Items.FirstOrDefault(x => string.Equals(x.Name, item.EnglishName));

            if (known == null)
            {
                known = new(item.EnglishName, MiningItemType.Ore);
                Items.Add(known);
            }

            known.RefinedCount++;
        }

        internal void AddMaterial(MaterialCollectedEvent.MaterialCollectedEventArgs e)
        {
            var mat = ODMVVM.Helpers.EliteShipMaterialHelper.GetMaterial(e.Name);

            var known = Items.FirstOrDefault(x => string.Equals(x.Name, mat.EnglishName));

            if (known == null)
            {
                known = new(mat.EnglishName, MiningItemType.Material);
                Items.Add(known);
            }

            known.CollectedCount += e.Count;
        } 
        
        public void AddProspector(MiningProspector prospector)
        {
            if (Prospectors.Contains(prospector))
                return;

            Prospectors.Insert(0, prospector);
        }
    }
}
