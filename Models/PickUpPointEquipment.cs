namespace EquipRentApi.Models
{
    public class PickUpPointEquipment
    {
        public int Id { get; set; }

        public int PickUpPointId { get; set; }
        public int EquipmentId { get; set; }

        public int Quantity { get; set; }

        // Navigation
        public PickUpPoint PickUpPoint { get; set; } = null!;
        public Equipment Equipment { get; set; } = null!;

        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
}
