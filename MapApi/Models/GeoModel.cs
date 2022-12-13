namespace MapApi.Models;

/// <summary>Geo模型</summary>
public class GeoModel
{
    public String Hash { get; set; }

    public Double Longitude { get; set; }

    public Double Latitude { get; set; }

    public String Address { get; set; }

    public Double LongitudeBd09 { get; set; }

    public Double LatitudeBd09 { get; set; }

    public Double LongitudeGcj02 { get; set; }

    public Double LatitudeGcj02 { get; set; }

    public Int32 AreaCode { get; set; }

    public String AreaName { get; set; }

    public Int32 ProvinceId { get; set; }

    public String Province { get; set; }

    public Int32 CityId { get; set; }

    public String City { get; set; }

    public Int32 DistrictId { get; set; }

    public String District { get; set; }
}