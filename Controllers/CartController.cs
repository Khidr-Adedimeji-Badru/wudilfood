using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WudilFood.Data;
using WudilFood.Models;
using WudilFood.ViewModels;
namespace WudilFood.Controllers;
public class CartController(AppDb db):Controller {
 public static List<CartEntry> Get(ISession s)=>JsonSerializer.Deserialize<List<CartEntry>>(s.GetString("cart")??"[]")??new(); public static void Put(ISession s,List<CartEntry> cart)=>s.SetString("cart",JsonSerializer.Serialize(cart));
 public async Task<IActionResult> Index(){var cart=Get(HttpContext.Session);var ids=cart.Select(x=>x.ItemId).ToList();ViewBag.Cart=cart;ViewBag.Settings=await db.Settings.FirstAsync();return View(await db.MenuItems.Include(x=>x.Vendor).Where(x=>ids.Contains(x.Id)).ToListAsync());}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Add(int id,int quantity=1){if(quantity<1||quantity>100)return BadRequest();var item=await db.MenuItems.Include(x=>x.Vendor).FirstOrDefaultAsync(x=>x.Id==id&&x.IsVisible&&x.Stock>=quantity&&x.Mode!=AvailabilityMode.Unavailable&&x.Vendor.Status==ApprovalStatus.Approved);if(item==null)return NotFound();var cart=Get(HttpContext.Session);var existing=cart.FirstOrDefault(x=>x.ItemId==id);if(existing!=null){if(existing.Quantity+quantity>item.Stock)return BadRequest("Not enough stock.");existing.Quantity+=quantity;}else {var vendorIds=await db.MenuItems.Where(x=>cart.Select(c=>c.ItemId).Contains(x.Id)).Select(x=>x.VendorId).Distinct().ToListAsync();if(vendorIds.Any(x=>x!=item.VendorId))return BadRequest("Check out one vendor at a time.");cart.Add(new CartEntry{ItemId=id,Quantity=quantity});}Put(HttpContext.Session,cart);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Update(int id,int quantity){var cart=Get(HttpContext.Session);var entry=cart.FirstOrDefault(x=>x.ItemId==id);if(entry==null)return NotFound();var item=await db.MenuItems.AsNoTracking().FirstOrDefaultAsync(x=>x.Id==id&&x.IsVisible&&x.Mode!=AvailabilityMode.Unavailable);if(item==null)return NotFound();if(quantity<=0)cart.Remove(entry);else if(quantity>item.Stock||quantity>100)return BadRequest("Requested quantity is not available.");else entry.Quantity=quantity;Put(HttpContext.Session,cart);return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Remove(int id){var cart=Get(HttpContext.Session);cart.RemoveAll(x=>x.ItemId==id);Put(HttpContext.Session,cart);return RedirectToAction(nameof(Index));}
}
