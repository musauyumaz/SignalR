using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignalR.SampleProject.Models;
using SignalR.SampleProject.Models.ViewModels;

namespace SignalR.SampleProject.Controllers;

public class HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager) : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult SignUp()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> SignUp(SignUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userToCreate = new IdentityUser()
        {
            UserName = model.Email,
            Email = model.Email
        };

        var result = await userManager.CreateAsync(userToCreate, model.Password);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", string.Join("\n", result.Errors.Select(e => e.Description)));
        }

        return RedirectToAction(nameof(SignIn));
    }

    public IActionResult SignIn()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> SignIn(SignUpViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        
        var hasUser = await userManager.FindByEmailAsync(model.Email);
        if (hasUser == null) ModelState.AddModelError(string.Empty, "Email or Password is incorrect") ;
        
        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
        if (!result.Succeeded) ModelState.AddModelError(string.Empty, "Email or Password is incorrect") ;
        
        
        
        return RedirectToAction(nameof(Index));
    }

    public IActionResult ProductList()
    {
        return View();
    }
}