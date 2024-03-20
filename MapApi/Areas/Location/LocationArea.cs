using System.ComponentModel;
using NewLife;
using NewLife.Cube;

namespace MapApi.Areas.Location
{
    [DisplayName("地理编码")]
    public class LocationArea : AreaBase
    {
        public LocationArea() : base(nameof(LocationArea).TrimEnd("Area")) { }
    }
}