using Newtonsoft.Json;

namespace ODEliteTracker.Models.Settings
{
    public sealed class SpanshCSVSettings
    {
        [JsonIgnore]
        public EventHandler<CsvType>? CSVChanged;

        public Dictionary<int, CsvType> SelectedCSV { get; set; } = [];
        public bool AutoCopySystemToClipboard { get; set; } = true;

        [JsonIgnore]
        public CsvType this[int index]
        {
            get
            {
                if (SelectedCSV.TryGetValue(index, out CsvType csvType))
                {
                    return csvType;
                }
                SelectedCSV.Add(index, CsvType.RoadToRiches);

                return CsvType.RoadToRiches;
            }
            set
            {
                if (SelectedCSV.ContainsKey(index))
                {
                    SelectedCSV[index] = value;
                    CSVChanged?.Invoke(this, value);
                    return;
                }

                SelectedCSV.TryAdd(index, value);
                CSVChanged?.Invoke(this, value);
            }
        }
    }
}
