using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WudilFood.Data;
using WudilFood.Models;
using WudilFood.ViewModels;
namespace WudilFood.Controllers;
public class AccountController(UserManager<AppUser> users,SignInManager<AppUser> signIn,AppDb db):Controller{
 [HttpGet]public IActionResult Register()=>View(new RegisterForm());
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Register(RegisterForm f){if(f.Role!=Roles.Customer&&f.Role!=Roles.Vendor)ModelState.AddModelError(nameof(f.Role),"Choose Customer or Vendor.");if(f.Role==Roles.Vendor&&(string.IsNullOrWhiteSpace(f.BusinessName)||string.IsNullOrWhiteSpace(f.Location)))ModelState.AddModelError("","Vendor name and location are required.");if(!ModelState.IsValid)return View(f);var u=new AppUser{UserName=f.Email,Email=f.Email,FullName=f.FullName,PhoneNumber=f.Phone,Address=f.Address};var result=await users.CreateAsync(u,f.Password);if(!result.Succeeded){foreach(var e in result.Errors)ModelState.AddModelError("",e.Description);return View(f);}await users.AddToRoleAsync(u,f.Role);if(f.Role==Roles.Vendor)db.Vendors.Add(new Vendor{UserId=u.Id,Name=f.BusinessName!,Location=f.Location!});await db.SaveChangesAsync();await signIn.SignInAsync(u,false);return RedirectToAction("Index",f.Role==Roles.Vendor?"Vendor":"Marketplace");}
 [HttpGet]public IActionResult Login()=>View(new LoginForm());
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Login(LoginForm f,string? returnUrl=null){if(!ModelState.IsValid)return View(f);var u=await users.FindByEmailAsync(f.Email);if(u==null||!(await signIn.PasswordSignInAsync(u,f.Password,false,true)).Succeeded){ModelState.AddModelError("","Invalid sign-in details.");return View(f);}if(Url.IsLocalUrl(returnUrl))return LocalRedirect(returnUrl);if(await users.IsInRoleAsync(u,Roles.Admin))return RedirectToAction("Index","Admin");if(await users.IsInRoleAsync(u,Roles.Vendor))return RedirectToAction("Index","Vendor");return RedirectToAction("Index","Marketplace");}
 [HttpPost,Authorize,ValidateAntiForgeryToken]public async Task<IActionResult> Logout(){await signIn.SignOutAsync();return RedirectToAction("Index","Home");}
 public IActionResult Denied()=>View();
}

