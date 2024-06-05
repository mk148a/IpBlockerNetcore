using Newtonsoft.Json;

namespace IpBlockerNetcore.Models.Domain
{
   
    public class Data
    {
        [JsonProperty("ipAddress")]
        public string IpAddress;

        [JsonProperty("isPublic")]
        public bool? IsPublic;

        [JsonProperty("ipVersion")]
        public int? IpVersion;

        [JsonProperty("isWhitelisted")]
        public string IsWhitelisted;

        [JsonProperty("abuseConfidenceScore")]
        public int? AbuseConfidenceScore;

        [JsonProperty("countryCode")]
        public string CountryCode;

        [JsonProperty("countryName")]
        public string CountryName;

        [JsonProperty("usageType")]
        public string UsageType;

        [JsonProperty("isp")]
        public string Isp;

        [JsonProperty("domain")]
        public string Domain;

        [JsonProperty("hostnames")]
        public List<string> Hostnames;

        [JsonProperty("isTor")]
        public bool? IsTor;

        [JsonProperty("totalReports")]
        public int? TotalReports;

        [JsonProperty("numDistinctUsers")]
        public int? NumDistinctUsers;

        [JsonProperty("lastReportedAt")]
        public string LastReportedAt;
    }

    public class AbuseIpDb
    {
        [JsonProperty("data")]
        public Data Data;
    }

}
