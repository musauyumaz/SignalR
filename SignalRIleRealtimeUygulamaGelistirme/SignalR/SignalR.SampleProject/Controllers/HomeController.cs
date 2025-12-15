using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SignalR.SampleProject.Models;

namespace SignalR.SampleProject.Controllers;

public class HomeController : Controller
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
    //
    // public IActionResult SignUp()
    // {
    //     return View();
    // }
    
    public IActionResult SignIn()
    {
        return View();
    }
    // public IActionResult SignIn()
    // {
    //     return View();
    // }
    
    public IActionResult ProductList()
    {
        return View();
    }
}