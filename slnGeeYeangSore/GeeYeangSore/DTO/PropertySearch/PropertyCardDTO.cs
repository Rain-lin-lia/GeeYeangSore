using Microsoft.AspNetCore.Mvc;

namespace GeeYeangSore.DTO.PropertySearch
{
    public class PropertyCardDTO
    {
        public int PropertyId { get; set; }
        public string? Title { get; set; }
        public int? RentPrice { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? Address { get; set; }
        public int? RoomCount { get; set; }
        public int? BathroomCount { get; set; }
        public string? PropertyType { get; set; }
        public string? Image { get; set; }
        public string? BadgeType { get; set; }
    }
}
