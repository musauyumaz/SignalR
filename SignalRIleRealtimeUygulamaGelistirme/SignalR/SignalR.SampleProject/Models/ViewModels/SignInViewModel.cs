using System.ComponentModel.DataAnnotations;

namespace SignalR.SampleProject.Models.ViewModels;

public record SignInViewModel([Required] string Email, [Required] string Password);