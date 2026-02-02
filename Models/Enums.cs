namespace EquipRentApi.Models
{
    public enum UserRole
    {
        Admin = 0,
        Manager = 1,
    }

    public enum OrderStatus
    {
        Pending = 0,
        Confirmed = 1,
        Rejected = 2,
        InUse = 3,
        Done = 4
    }
}
