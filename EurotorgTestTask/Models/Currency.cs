using System;
using System.Text.Json.Serialization;

namespace EurotorgTestTask.Models
{
    public class Currency
    {
        [JsonPropertyName("Cur_Abbreviation")]
        public string CurAbbreviation { get; set; }

        [JsonPropertyName("Cur_DateEnd")]
        public DateTime CurDateEnd { get; set; }

        [JsonPropertyName("Cur_ID")]
        public int CurId { get; set; }

        [JsonPropertyName("Cur_Name")]
        public string CurName { get; set; }
    }
}