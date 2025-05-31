using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.DTO.PropertySearch
{
    public class PropertyFilterDTO
    {
        public string? Keyword { get; set; }
        public string? City { get; set; }
        public string? District { get; set; } 
        public string? Type { get; set; } 
        public string? BuildingType { get; set; }
        public int? RentMin { get; set; }
        public int? RentMax { get; set; }
        public int? AreaMin { get; set; }
        public int? AreaMax { get; set; }
        public List<string>? Features { get; set; }
    }
}
