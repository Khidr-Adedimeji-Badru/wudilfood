using Microsoft.AspNetCore.Mvc;
using WudilFood.Models;
namespace WudilFood.Controllers;
public class HomeController:Controller { public IActionResult Index()=>User.IsInRole(Roles.Admin)?RedirectToAction("Index","Admin"):User.IsInRole(Roles.Vendor)?RedirectToAction("Index","Vendor"):RedirectToAction("Index","Marketplace"); public IActionResult Error()=>View(); }
