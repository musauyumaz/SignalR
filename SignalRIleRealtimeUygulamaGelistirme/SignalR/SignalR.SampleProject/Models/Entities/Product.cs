using Microsoft.AspNetCore.Identity;

namespace SignalR.SampleProject.Models.Entities;

public class Product
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }

    public IdentityUser IdentityUser { get; set; }
}