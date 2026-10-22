namespace FatturaFacile.Model;

public class CountryData(int regionId, int parentRegionId, string region, double area)
{
    public int RegionID { get; set; } = regionId;
    public int ParentRegionID { get; set; } = parentRegionId;
    public string Region { get; set; } = region;
    public double Area { get; set; } = area;
}