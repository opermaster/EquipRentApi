namespace EquipRentApi.Models
{
    public class PickUpPoint
    {
        public int Id { get; set; }
        public string Addres { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public ICollection<User> Users { get; set; }
            = new List<User>();
        public ICollection<PickUpPointEquipment> Equipments { get; set; }
            = new List<PickUpPointEquipment>();
    }
    public class PickUpPointDto
    {
        public int? Id { get; set; }
        public string Addres { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public PickUpPoint ToPickUpPoint() {
            return new PickUpPoint {
                Addres = Addres, 
                PhoneNumber = PhoneNumber,
            };
        }
    }
    public class UpdatePointDto
    {
        public string Address { get; set; }
        public string Phone { get; set; }
    }
}
