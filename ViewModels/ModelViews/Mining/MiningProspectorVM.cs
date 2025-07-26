using ODEliteTracker.Models.Mining;

namespace ODEliteTracker.ViewModels.ModelViews.Mining
{
    public sealed class MiningProspectorVM
    {
        public MiningProspectorVM(MiningProspector prospector)
        {
            Prospector = prospector;

            if (prospector.Remaining <= 0)
            {
                Items = [new("Asteroid Depleted", string.Empty)];
                return;
            }

            Items = [];

            foreach (var item in prospector.Materials)
            {
                var ore = ODMVVM.Helpers.EliteCommodityHelpers.GetCommodityFromPartial(item.Name, item.Name);

                Items.Add(new(ore.EnglishName, $"{item.Proportion:N2}%"));
            }

            Items.Add(new("Material Content", prospector.Content.ToString()));

            if (string.IsNullOrEmpty(prospector.MotherlodeMaterial) == false)
            {
                var ore = ODMVVM.Helpers.EliteCommodityHelpers.GetCommodityFromPartial(prospector.MotherlodeMaterial, prospector.MotherlodeMaterial);
                MotherLoadContent = $"Core Detected : {ore.EnglishName}";
            }         
        }

        public List<ProspectorItemVM> Items { get; }
        public string MotherLoadContent { get; } = string.Empty;
        public MiningProspector Prospector { get; }
    }
}
