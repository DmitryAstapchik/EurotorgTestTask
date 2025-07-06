using System;
using System.Text.Json.Serialization;

namespace EurotorgTestTask.Models
{
    public class RateShort
    {
        [JsonPropertyName("Cur_ID")]
        public int CurId { get; set; }

        [JsonPropertyName("Cur_OfficialRate")]
        public decimal? CurOfficialRate { get; set; }

        public DateTime Date { get; set; }
    }
}