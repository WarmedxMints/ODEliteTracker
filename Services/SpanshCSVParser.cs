using Microsoft.VisualBasic.FileIO;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Spansh;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace ODEliteTracker.Services
{
    public class CsvParserReturn
    {
        public CsvType CsvType { get; set; }

        public required List<ExplorationTarget> Targets;
    }

    public static class SpanshCSVParser
    {
        private static readonly List<string[]> csvHeaders =
        [
            ["System Name", "Body Name", "Body Subtype", "Is Terraformable", "Distance To Arrival", "Estimated Scan Value", "Estimated Mapping Value", "Jumps"], //roadToRichesRoute
            ["System Name", "Distance", "Distance Remaining", "Tritium in tank", "Tritium in market", "Fuel Used", "Icy Ring", "Pristine", "Restock Tritium"], //fleetCarrierRoute
            ["System Name", "Distance To Arrival", "Distance Remaining", "Neutron Star", "Jumps"], //neutronRoute
            ["System Name", "Distance", "Distance Remaining", "Fuel Left", "Fuel Used", "Refuel", "Neutron Star"], //galaxyPlotterRoute
            ["System Name", "Body Name", "Distance To Arrival", "Jumps"], //worldTypeRoute
            ["System Name", "Jumps"], //touristRoute
            ["System Name","Body Name","Body Subtype","Distance To Arrival","Landmark Subtype","Value","Count","Jumps"], //ExoBiology    
            ["System Name", "Body Name", "Body Subtype", "Distance To Arrival", "Landmark Type", "Value", "Jumps"], //ExoBiologyold
            ["System Name", "Distance", "Distance Remaining"] //Colonisation Plotter
        ];

        public static CsvParserReturn? ForceParse(string filename, CsvType type)
        {
            try
            {
                using TextFieldParser parser = new(filename);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                string[]? firstLine = parser.ReadFields();

                if (firstLine is null || firstLine.Length == 0)
                    return null;

                return type switch
                {
                    CsvType.RoadToRiches => ProcessRoadToRichesRoute(parser),
                    CsvType.FleetCarrier => ProcessFleetCarrierRoute(parser),
                    CsvType.NeutronRoute => ProcessNeutronRoute(parser),
                    CsvType.GalaxyPlotter => ProcessGalaxyPlotterRoute(parser),
                    CsvType.WorldTypeRoute => ProcessWorldTypeRoute(parser),
                    CsvType.TouristRoute => ProcessTouristRoute(parser),
                    CsvType.ExobiologyOld => ProcessExoRoute(parser, CsvType.ExobiologyOld),
                    CsvType.Exobiology => ProcessExoRoute(parser, CsvType.Exobiology),
                    CsvType.Colonisation => ProcessColonisation(parser),
                    _ => null,
                };
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

                Tuple<CsvType, bool> csvType = CheckCsvType(firstLine);

                if (csvType.Item2 == false)
                {
                    return null;
                }

                return csvType.Item1 switch
                {
                    CsvType.RoadToRiches => ProcessRoadToRichesRoute(parser),
                    CsvType.FleetCarrier => ProcessFleetCarrierRoute(parser),
                    CsvType.NeutronRoute => ProcessNeutronRoute(parser),
                    CsvType.GalaxyPlotter => ProcessGalaxyPlotterRoute(parser),
                    CsvType.WorldTypeRoute => ProcessWorldTypeRoute(parser),
                    CsvType.TouristRoute => ProcessTouristRoute(parser),
                    CsvType.ExobiologyOld => ProcessExoRoute(parser, CsvType.ExobiologyOld),
                    CsvType.Exobiology => ProcessExoRoute(parser, CsvType.Exobiology),
                    CsvType.Colonisation => ProcessColonisation(parser),
                    _ => null,
                };
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
                CsvType = CsvType.Colonisation,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                ExplorationTarget? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
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

        private static CsvParserReturn ProcessExoRoute(TextFieldParser parser, CsvType csvType)
        {
            CsvParserReturn ret = new()
            {
                CsvType = csvType,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                ExplorationTarget? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = systemName.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName),
                    Distance = fields[4].ToUpperInvariant(),
                    Property1 = $"{double.Parse(fields[5], new CultureInfo("en-GB")):N0}"
                };


                target.BodiesInfo ??= [];

                target.BodiesInfo.Add(bodyInfo);
                target.BodiesInfo.Sort((x, y) =>
                {
                    if (x.Body is null)
                        return 0;
                    return x.Body.CompareTo(y.Body);
                });
            }


            return ret;
        }

        private static CsvParserReturn ProcessTouristRoute(TextFieldParser parser)
        {
            CsvParserReturn ret = new()
            {
                CsvType = CsvType.TouristRoute,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                ExplorationTarget target = new()
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
                CsvType = CsvType.WorldTypeRoute,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                ExplorationTarget? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = systemName.ToUpperInvariant(),
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName),
                    Distance = $"{double.Parse(fields[2], new CultureInfo("en-GB")):N0} ls",
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
                CsvType = CsvType.GalaxyPlotter,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                ExplorationTarget target = new()
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
                CsvType = CsvType.NeutronRoute,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                ExplorationTarget target = new()
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
                CsvType = CsvType.FleetCarrier,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                ExplorationTarget target = new()
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
                CsvType = CsvType.RoadToRiches,
                Targets = []
            };

            while (!parser.EndOfData)
            {
                //Process row
                string[]? fields = parser.ReadFields();

                if (fields is null || fields.Length == 0)
                    continue;

                string systemName = fields[0];

                ExplorationTarget? target = ret.Targets.Find(x => x.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    target = new ExplorationTarget
                    {
                        SystemName = systemName.ToUpperInvariant(),
                        Property1 = fields[7] //Jumps to System
                    };

                    ret.Targets.Add(target);
                }

                BodiesInfo bodyInfo = new()
                {
                    Body = GetBodyName(fields[1], target.SystemName),
                    Distance = $"{double.Parse(fields[4], new CultureInfo("en-GB")):N0} ls",
                    Property1 = $"{double.Parse(fields[6], new CultureInfo("en-GB")):N0}"
                };


                target.BodiesInfo ??= [];

                target.BodiesInfo.Add(bodyInfo);
            }

            return ret;
        }

        private static Tuple<CsvType, bool> CheckCsvType(string[] firstLine)
        {
            for (int i = 0; i < csvHeaders.Count; i++)
            {
                if (firstLine.SequenceEqual(csvHeaders[i]))
                {
                    return Tuple.Create((CsvType)i, true);
                }
            }

            return Tuple.Create(CsvType.RoadToRiches, false);
        }

        private static string GetBodyName(string bodyName, string systemName)
        {
            if (bodyName.Length > systemName.Length)
            {
                bodyName = bodyName.Remove(0, systemName.Length);

                return bodyName;
            }

            return bodyName.ToUpperInvariant();

        }
    }
}
