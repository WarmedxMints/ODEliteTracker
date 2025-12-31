using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(Id))]
    public class BGSTickData(string id, DateTime time, DateTime updated_At, bool manualTick = false, bool hidden = false)
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = id;
        public DateTime Time { get; set; } = time;
        public DateTime Updated_At { get; set; } = updated_At;
        public bool ManualTick { get; set; } = manualTick;
        public bool Hidden { get; set; } = hidden;

        public override bool Equals(object? obj)
        {
            if (obj is BGSTickData otherData)
            {
                return string.Equals(otherData.Id, Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (Id is null)
            {
                return base.GetHashCode();
            }

            return Id.GetHashCode();
        }
    }
}
