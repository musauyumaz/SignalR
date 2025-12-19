using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalR.SampleProject.Models.Contexts;
using SignalR.SampleProject.Models.Entities;
using SignalR.SampleProject.Models.ViewModels;

namespace SignalR.SampleProject.Controllers;

public class HomeController(
    UserManager<IdentityUser> userManager,
    SignInManager<IdentityUser> signInManager,
    AppDbContext dbContext) : Controller
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
    public async Task<IActionResult> SignIn(SignInViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var hasUser = await userManager.FindByEmailAsync(model.Email);
        if (hasUser == null) ModelState.AddModelError(string.Empty, "Email or Password is incorrect");

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);
        if (!result.Succeeded) ModelState.AddModelError(string.Empty, "Email or Password is incorrect");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ProductList()
    {
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Id == "fa34618b-17ce-4824-b40e-a3062a943fa0");

        if (!dbContext.Products.Any(x => x.UserId == user.Id))
        {
            var product1 = new Product()
                { Name = "Pen 1", Description = "Description 1", Price = 100, UserId = user.Id };
            var product2 = new Product()
                { Name = "Pen 2", Description = "Description 2", Price = 200, UserId = user.Id };
            var product3 = new Product()
                { Name = "Pen 3", Description = "Description 3", Price = 300, UserId = user.Id };
            var product4 = new Product()
                { Name = "Pen 4", Description = "Description 4", Price = 400, UserId = user.Id };
            var product5 = new Product()
                { Name = "Pen 5", Description = "Description 5", Price = 500, UserId = user.Id };

            await dbContext.Products.AddAsync(product1);
            await dbContext.Products.AddAsync(product2);
            await dbContext.Products.AddAsync(product3);
            await dbContext.Products.AddAsync(product4);
            await dbContext.Products.AddAsync(product5);
            await dbContext.SaveChangesAsync();
        }


        var products = await dbContext.Products.Where(x => x.UserId == user.Id).ToListAsync();
        return View(products);
    }
}