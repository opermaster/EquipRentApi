namespace EquipRentApi.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Img { get; set; } = null!;

        
        // Navigation
        public ICollection<PickUpPointEquipment> PickUpPoints { get; set; }
            = new List<PickUpPointEquipment>();
    }
    public class EquipmentAvailabilityDto
    {
        public int Id { get; set; }              
        public int PickUpPointId { get; set; }  
        public string Address { get; set; } = null!;
        public int Quantity { get; set; }
    }
    public class EquipmentDto
    {
        public int? Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string Img { get; set; } = null!;
        public List<EquipmentAvailabilityDto> PickUpPoints { get; set; }
        = new();
        public Equipment ToEquipment() {
            return new Equipment {
                Name = Name,
                Price = Price,
                Img = Img,
            };
        }
    }
    public class EquipmentCreateDto
    {
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public IFormFile Image { get; set; } = null!;
    }

}
    
