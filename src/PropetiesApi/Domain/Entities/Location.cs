namespace Domain.Entities
{
    public class Location
    {
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string Country { get; set; } = "BR";
    }
}
