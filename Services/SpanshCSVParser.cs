using Microsoft.VisualBasic.FileIO;
using ODEliteTracker.Models.Spansh;
using System.Globalization;
using System.IO;

namespace ODEliteTracker.Services
{
    public class CsvParserReturn
    {
        public SpanshRouteType CsvType { get; set; }
        public required List<SpanshTargetSystem> Targets { get; set; }
        public DateTime FileCreationDate { get; set; }
    }

    public static class SpanshCSVParser
    {
        private static readonly List<string[]> csvHeaders =
        [
            ["System Name", "Body Name", "Body Subtype", "Is Terraformable", "Distance To Arrival", "Estimated Scan Value", "Estimated Mapping Value", "Jumps"], //roadToRichesRoute
            ["System Name", "Distance", "Distance Remaining", "Tritium in tank", "Tritium in market", "Fuel Used", "Icy Ring", "Pristine", "Restock Tritium"], //fleetCarrierRoute
            ["System Name", "Distance To Arrival", "Distance Remaining", "Neutron Star", "Jumps"], //neutronRoute
            ["System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star", "Inject"], //galaxyPlotterRoute            
            ["System Name", "Body Name", "Distance To Arrival", "Jumps"], //worldTypeRoute
            ["System Name", "Jumps"], //touristRoute
            ["System Name","Body Name","Body Subtype","Distance To Arrival","Landmark Subtype","Value","Count","Jumps"], //ExoBiology    
            ["System Name", "Body Name", "Body Subtype", "Distance To Arrival", "Landmark Type", "Value", "Jumps"], //ExoBiologyold
            ["System Name", "Distance", "Distance Remaining"], //Colonisation Plotter,
            ["System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star"], //galaxyPlotterRoute old
        ];

        public static CsvParserReturn? ForceParse(string filename, SpanshRouteType type)
        {
            try
            {
                using TextFieldParser parser = new(filename);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[]? firstLine = parser.ReadFields();

                if (firstLine is null || firstLine.Length == 0)
                    return null;

                var container = type switch
                {
                    SpanshRouteType.RoadToRiches => ProcessRoadToRichesRoute(parser),
                    SpanshRouteType.FleetCarrier => ProcessFleetCarrierRoute(parser),
                    SpanshRouteType.NeutronRoute => ProcessNeutronRoute(parser),
                    SpanshRouteType.GalaxyPlotter => ProcessGalaxyPlotterRoute(parser),
                    SpanshRouteType.WorldTypeRoute => ProcessWorldTypeRoute(parser),
                    SpanshRouteType.TouristRoute => ProcessTouristRoute(parser),
                    SpanshRouteType.ExobiologyOld => ProcessExoRoute(parser, SpanshRouteType.ExobiologyOld),
                    SpanshRouteType.Exobiology => ProcessExoRoute(parser, SpanshRouteType.Exobiology),
                    SpanshRouteType.Colonisation => ProcessColonisation(parser),
                    _ => null,
                };

                if (container == null)
                    return null;

                container.FileCreationDate = File.GetCreationTimeUtc(filename);

                return container;
            }
            catch
            {
                return null;
            }
        }

        public static CsvParserReturn? ParseCsv(string filename)
        {
            try
            {
                using TextFieldParser parser = new(filename);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[]? firstLine = parser.ReadFields();

                if (firstLine is null || firstLine.Length == 0)
                    return null;

                Tuple<SpanshRouteType, bool> csvType = CheckCsvType(firstLine);

                if (csvType.Item2 == false)
                {
                    return null;
                }

                var container = csvType.Item1 switch
                {
                    SpanshRouteType.RoadToRiches => ProcessRoadToRichesRoute(parser),
                    SpanshRouteType.FleetCarrier => ProcessFleetCarrierRoute(parser),
                    SpanshRouteType.NeutronRoute => ProcessNeutronRoute(parser),
                    SpanshRouteType.GalaxyPlotter => ProcessGalaxyPlotterRoute(parser),
                    SpanshRouteType.WorldTypeRoute => ProcessWorldTypeRoute(parser),
                    SpanshRouteType.TouristRoute => ProcessTouristRoute(parser),
                    SpanshRouteType.ExobiologyOld => ProcessExoRoute(parser, SpanshRouteType.ExobiologyOld),
                    SpanshRouteType.Exobiology => ProcessExoRoute(parser, SpanshRouteType.Exobiology),
                    SpanshRouteType.Colonisation => ProcessColonisation(parser),
                    SpanshRouteType.GalaxyPlotterOld => ProcessGalaxyPlotterRoute(parser),
                    _ => null,
                };

                if (container == null)
                    return null;

                container.FileCreationDate = File.GetCreationTimeUtc(filename);

                return container;
            }
            catch
            {
                return null;
            }
        }

        private static CsvParserReturn ProcessColonisation(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.Colonisation,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                SpanshTargetSystem? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new SpanshTargetSystem
                    {
                        SystemName = systemName.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }

                if (double.TryParse(fields[1], out var distance))
                {
                    target.Property1 = distance.ToString("N2");
                }

                if (double.TryParse(fields[2], out var remaining))
                {
                    target.Property2 = remaining.ToString("N2");
                }
            }

            return ret;
        }

        private static CsvParserReturn ProcessExoRoute(TextFieldParser parser, SpanshRouteType csvType)
        {
            CsvParserReturn ret = new()
            {
                CsvType = csvType,
                Targets = []
            };
            var lastBodyName = string.Empty;
            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                SpanshTargetSystem? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new SpanshTargetSystem
                    {
                        SystemName = systemName.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }
               
                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName, ref lastBodyName),
                    Distance = fields[4].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[5], new CultureInfo("en-GB")):N0}"
                };

                target.BodiesInfo ??= [];
                target.BodiesInfo.Add(bodyInfo);
            }
            return ret;
        }

        private static CsvParserReturn ProcessTouristRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.TouristRoute,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                SpanshTargetSystem target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //jumps
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessWorldTypeRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.WorldTypeRoute,
                Targets = []
            };
            var lastBodyname = string.Empty;
            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                SpanshTargetSystem? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new SpanshTargetSystem
                    {
                        SystemName = systemName.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }
             
                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName, ref lastBodyname),
                    Distance = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}",
                    Property1 = $"{double.Parse(fields[3], new CultureInfo("en-GB")):N0}"
                };

                target.BodiesInfo ??= [];

                target.BodiesInfo.Add(bodyInfo);
            }

            return ret;
        }

        private static CsvParserReturn ProcessGalaxyPlotterRoute(TextFieldParser parser)
        {
            //"System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star"

            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.GalaxyPlotter,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                SpanshTargetSystem target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = fields[5], //refuel 
                    Property4 = fields[6] //neutron star
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessNeutronRoute(TextFieldParser parser)
        {
            //"System Name","Distance To Arrival","Distance Remaining","Neutron Star","Jumps"

            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.NeutronRoute,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                SpanshTargetSystem target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = fields[4], //jumps 
                    Property4 = fields[3]
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static CsvParserReturn ProcessFleetCarrierRoute(TextFieldParser parser)
        {
            //    0            1             2                  3                    4                 5         6         7             8
            //"System Name","Distance","Distance Remaining","Tritium in tank","Tritium in market","Fuel Used","Icy Ring","Pristine","Restock Tritium"

            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.FleetCarrier,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                SpanshTargetSystem target = new()
                {
                    SystemName = fields[0].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[1], new CultureInfo("en-GB")):N0}", //distance
                    Property2 = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0}", //distance remaining
                    Property3 = GetRingInfo(fields[6], fields[7]),
                };

                ret.Targets.Add(target);
            }

            return ret;
        }

        private static string GetRingInfo(string v1, string v2)
        {
            bool bool1 = v1.Contains("Yes", StringComparison.OrdinalIgnoreCase);

            bool bool2 = v2.Contains("Yes", StringComparison.OrdinalIgnoreCase);

            return bool1 ? bool2 ? "Pristine" : "Yes" : "No";
        }

        private static CsvParserReturn ProcessRoadToRichesRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = SpanshRouteType.RoadToRiches,
                Targets = []
            };
            var lastBodyName = string.Empty;
            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                SpanshTargetSystem? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new SpanshTargetSystem
                    {
                        SystemName = systemName.ToUpperInvariant(),
                        Property1 = fields[7] //Jumps to System
                    };

                    ret.Targets.Add(target);
                }
               
                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName, ref lastBodyName),
                    Distance = $"{double.Parse(fields[4], new CultureInfo("en-GB")):N0} ls",
                    Property1 = $"{double.Parse(fields[6], new CultureInfo("en-GB")):N0}"
                };


                target.BodiesInfo ??= [];

                target.BodiesInfo.Add(bodyInfo);
            }

            return ret;
        }

        private static Tuple<SpanshRouteType, bool> CheckCsvType(string[] firstLine)
        {
            for (int i = 0; i < csvHeaders.Count; i++)
            {
                if (firstLine.SequenceEqual(csvHeaders[i]))
                {
                    return Tuple.Create((SpanshRouteType)i, true);
                }
            }

            return Tuple.Create(SpanshRouteType.RoadToRiches, false);
        }

        private static string GetBodyName(string bodyName, string systemName, ref string lastBodyName)
        {
            if (bodyName.Length > systemName.Length)
            {
                bodyName = bodyName.Remove(0, systemName.Length);
            }

            if (!string.Equals(bodyName, lastBodyName))
            {
                lastBodyName = bodyName;
                return bodyName;
            }

            return string.Empty;
        }
    }
}
