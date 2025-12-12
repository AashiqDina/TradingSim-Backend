using Newtonsoft.Json;

public class CompanyProfile
{
    public string Symbol { get; set; }
    public string Name { get; set; }
    public string Exchange { get; set; }

    [JsonProperty("mic_code")]
    public string MicCode { get; set; }

    public string Sector { get; set; }
    public string Industry { get; set; }
    public int Employees { get; set; }
    public string Website { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }

    public string CEO { get; set; }
    public string Address { get; set; }

    [JsonProperty("address2")]
    public string Address2 { get; set; }

    public string City { get; set; }
    public string Zip { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
}
