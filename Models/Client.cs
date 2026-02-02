namespace EquipRentApi.Models
{
    public class Client
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Navigation
        public ICollection<Order> Orders { get; set; }
            = new List<Order>();
    }
    public class ClientDto
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;

        public Client ToClient() {
            return new Client {
                FirstName = FirstName,
                LastName = LastName,
                Email = Email
            };
        }
    }

}
