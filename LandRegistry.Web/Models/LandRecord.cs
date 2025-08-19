

namespace LandRegistry.Web.Models
{
    public class LandRecord
    {
        public int Id { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public double Area { get; set; }
    }
}