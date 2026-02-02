namespace EquipRentApi.Models
{
    public class Order
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public int PickUpPointEquipmentId { get; set; }
        public PickUpPointEquipment PickUpPointEquipment { get; set; } = null!;
    }

}
