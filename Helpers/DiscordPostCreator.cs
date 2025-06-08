using ODEliteTracker.ViewModels.ModelViews.BGS;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.PowerPlay;
using System.Data;
using System.Text;

namespace ODEliteTracker.Helpers
{
    public enum ColonisationPostType
    {
        DiscordTable,
        Table,
        List
    }

    internal class DiscordPostCreator
    {
        #region BGS
        public static bool CreateBGSPost(IEnumerable<BGSTickSystemVM> systems, TickDataVM data)
        {
            systems = systems.Where(x => x.HasData).OrderBy(x => x.Name);

            if (systems is null || systems.Any() == false)
                return false;

            var builder = new StringBuilder();

            builder.AppendLine($"__**BGS Report - Tick : {DiscordTimeConvertor(data.TickTime)}**__");
            builder.AppendLine();

            foreach (var system in systems)
            {
                builder.AppendLine($"> **{system.NonUpperName}**");

                foreach(var faction in system.Factions)
                {
                    if (faction == null || faction.HasData() == false)
                        continue;

                    builder.AppendLine(FactionDataString(faction));
                }
            }

            var result = builder.ToString().TrimEnd('\r', '\n');

            return ODMVVM.Helpers.OperatingSystem.SetStringToClipboard(result);
        }

        private static string FactionDataString(FactionVM data)
        {
            StringBuilder builder = new();

            builder.Append(">    ");

            builder.Append($"{data.Name} : ");

            //Inf
            if (data.InfPlus != 0)
            {
                builder.Append($"{data.InfPlus:+#;-#;0} Inf, ");
            }

            //Trade Spend
            if (data.Purchases?.Count > 0)
            {
                builder.Append("Spend ");
                builder.Append($"{data.Purchases.ToString()}");
                builder.Append(", ");
            }
            //Trade sales
            if (data.Sales?.Count > 0)
            {
                builder.Append("Sales ");
                builder.Append($"{data.Sales.ToString()}");
                builder.Append(", ");
            }

            //S&R sales
            if (data.SearchAndRescue?.Total > 0)
            {
                builder.Append($"S&R {data.SearchAndRescue.Total} unit{(data.SearchAndRescue.Total > 1 ? "s, " : ", ")}");
            }
            //Carto
            if (data.CartoDataValue > 0)
            {
                builder.Append($"{data.CartoData} Carto, ");
            }
            //Bounties
            if (data.Bounties > 0)
            {
                builder.Append($"{data.BountyVouchers} BVs, ");
            }
            //Bonds
            if (data.Bonds > 0)
            {
                builder.Append($"{data.BondVouchers} CBs, ");
            }
            //Failed missions
            if (data.Failed > 0)
            {
                builder.Append($"{data.MissionsFailed}x Failed, ");
            }
            //Murdered
            if (data.TotalMurders > 0)
            {
                if (data.ShipMurders > 0)
                {
                    builder.Append($"{data.ShipMurders}x Ship Murder{(data.ShipMurders > 1 ? "s," : ",")} ");
                }
                if (data.FootMurders > 0)
                {
                    builder.Append($"{data.FootMurders}x Foot Murder{(data.FootMurders > 1 ? "s," : ",")} ");
                }
            }

            if (data.Wars is not null && data.Wars.Total > 0)
            {
                if (data.Wars.LowSpaceCZ > 0)
                {
                    builder.Append($"{data.Wars.LowSpaceCZ}x LSCZ, ");
                }
                if (data.Wars.MediumSpaceCZ > 0)
                {
                    builder.Append($"{data.Wars.MediumSpaceCZ}x MSCZ, ");
                }
                if (data.Wars.HighSpaceCZ > 0)
                {
                    builder.Append($"{data.Wars.HighSpaceCZ}x HSCZ, ");
                }
                if (data.Wars.LowGroundCZ > 0)
                {
                    builder.Append($"{data.Wars.LowGroundCZ}x LGCZ, ");
                }
                if (data.Wars.MediumGroundCZ > 0)
                {
                    builder.Append($"{data.Wars.MediumGroundCZ}x MGCZ, ");
                }
                if (data.Wars.HighGroundCZ > 0)
                {
                    builder.Append($"{data.Wars.HighGroundCZ}x HGCZ, ");
                }
            }

            if (data.Wars is not null && data.Wars.Settlements.Count > 0)
            {
                var ordered = data.Wars.Settlements.OrderBy(x => x.Key);
                builder = new StringBuilder(builder.ToString().TrimEnd('\r', '\n').TrimEnd(',', ' '));
                builder.AppendLine();
                foreach (var item in ordered)
                {
                    builder.AppendLine($">           {item.Value}x {item.Key}");
                }
            }

            return builder.ToString().TrimEnd('\r', '\n').TrimEnd(',', ' ');
        }
        #endregion

        #region PowerPlay
        internal static bool CreatePowerPlayPost(IEnumerable<PowerPlaySystemVM> cycleData, string cycle, DateTime cycleDate)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"__**Powerplay report for {cycle}**__");
            builder.AppendLine();

            foreach(var system in cycleData)
            {
                if (system.Data.TryGetValue(cycleDate, out var data))
                {
                    builder.AppendLine($"> **{system.NonUpperName}**");
                    builder.AppendLine($">    Merits Earned : {data.MeritsEarnedValue:N0}");

                    if(data.GoodsCollected.Count > 0)
                    {
                        builder.AppendLine(">    Items Collected :");
                        foreach (var item in data.GoodsCollected)
                        {
                            builder.AppendLine($">        {item.Name} | {item.Count:N0}");
                        }
                    }

                    if (data.GoodsDelivered.Count > 0)
                    {
                        builder.AppendLine(">    Items Delivered :");
                        foreach (var item in data.GoodsDelivered)
                        {
                            builder.AppendLine($">        {item.Name} | {item.Count:N0}");
                        }
                    }
                }
            }

            var result = builder.ToString().TrimEnd('\r', '\n');

            return ODMVVM.Helpers.OperatingSystem.SetStringToClipboard(result);
        }
        #endregion

        #region Colonisation
        internal static bool CreateColonisationPost(ConstructionDepotVM depot, IEnumerable<ConstructionResourceVM>? selectedDepotResources, ColonisationPostType type)
        {
            //If we have no resources require, no need to make a post
            if (depot.Resources.Any(x => x.RemainingCount > 0) == false || selectedDepotResources == null)
                return false;

            var sb = new StringBuilder();

            if (type == ColonisationPostType.DiscordTable)
            {
                sb.AppendLine("```");

            }

            sb.AppendLine($"{depot.SystemNameText} | {depot.StationName}");
            sb.AppendLine($"Progress {depot.Progress}");
            sb.AppendLine();

            switch (type)
            {
                case ColonisationPostType.List:
                    sb.AppendLine(CreateDepotResourceList(selectedDepotResources));
                    break;
                default:
                    sb.AppendLine(CreateDepotResourceTable(selectedDepotResources));
                    break;
            }

            if (type == ColonisationPostType.DiscordTable)
            {
                sb.AppendLine("```");

            }

            var result = sb.ToString().TrimEnd('\r', '\n');

            return ODMVVM.Helpers.OperatingSystem.SetStringToClipboard(result);
        }

        private static string CreateDepotResourceTable(IEnumerable<ConstructionResourceVM> items)
        {
            var table = new DataTable();

            table.Columns.Add("Name");
            table.Columns.Add("Category");
            table.Columns.Add("Required");

            foreach (var item in items)
            {
                table.Rows.Add(item.LocalName, item.Category, item.Remaining);
            }

            return ODMVVM.Helpers.AsciiTableGenerator.CreateAsciiTableFromDataTable(table, [2]).ToString().TrimEnd('\r', '\n');
        }

        private static string CreateDepotResourceList(IEnumerable<ConstructionResourceVM> items)
        {
            var sb = new StringBuilder();

            foreach (var item in items)
            {
                sb.AppendLine($"{item.LocalName} | {item.Category} | {item.Remaining}");
            }

            return sb.ToString().TrimEnd('\r', '\n');
        }
        #endregion
        public static string DiscordTimeConvertor(DateTime time)
        {
            TimeSpan t = time.ToUniversalTime() - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return $"<t:{secondsSinceEpoch}>";
        }
    }
}
