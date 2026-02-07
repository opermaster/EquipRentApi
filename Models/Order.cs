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
    public class OrderDto {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ClientDto Client {get;set;}
        public int PickUpPointEquipmentId { get; set; }
        public Order ToOrder() {
            return new Order {
                StartDate = StartDate,
                EndDate = EndDate,
                Client = Client.ToClient(),
                PickUpPointEquipmentId = PickUpPointEquipmentId
            };
        }
    }
}
