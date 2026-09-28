using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WudilFood.Data;
using WudilFood.Models;
namespace WudilFood.Controllers;

public class MarketplaceController(AppDb db):Controller
{
    string? Uid=>User.FindFirstValue(ClaimTypes.NameIdentifier);

    public async Task<IActionResult> Index(string? q,int? category,string? location,decimal? maxPrice,DateOnly? date)
    {
        var availabilityUtc=date.HasValue?date.Value.ToDateTime(new TimeOnly(12,0),DateTimeKind.Local).ToUniversalTime():DateTime.UtcNow;
        var items=db.MenuItems.AsNoTracking().Include(x=>x.Vendor).Include(x=>x.Category).Include(x=>x.Schedules)
            .Where(x=>x.IsVisible&&x.Stock>0&&x.Mode!=AvailabilityMode.Unavailable&&x.Vendor.Status==ApprovalStatus.Approved&&!db.VendorClosures.Any(c=>c.VendorId==x.VendorId&&c.StartsUtc<=availabilityUtc&&c.EndsUtc>=availabilityUtc));
        if(!string.IsNullOrWhiteSpace(q))items=items.Where(x=>EF.Functions.ILike(x.Name,"%"+q+"%")||EF.Functions.ILike(x.Vendor.Name,"%"+q+"%"));
        if(category.HasValue)items=items.Where(x=>x.CategoryId==category);
        if(!string.IsNullOrWhiteSpace(location))items=items.Where(x=>EF.Functions.ILike(x.Vendor.Location,"%"+location+"%"));
        if(maxPrice.HasValue)items=items.Where(x=>x.Price<=maxPrice);
        if(date.HasValue)items=items.Where(x=>x.Mode==AvailabilityMode.Immediate||x.Schedules.Any(s=>s.Day==date.Value.DayOfWeek));
        ViewBag.Categories=await db.Categories.OrderBy(x=>x.Name).ToListAsync();ViewBag.Query=q;
        if(User.IsInRole(Roles.Customer)&&Uid!=null)
        {
            ViewBag.FavoriteIds=await db.Favorites.Where(x=>x.CustomerId==Uid).Select(x=>x.MenuItemId).ToListAsync();
            var recentIds=await db.OrderLines.Where(x=>x.Order.CustomerId==Uid&&x.Order.State==OrderState.Completed).OrderByDescending(x=>x.Order.CreatedUtc).Select(x=>x.MenuItemId).Distinct().Take(4).ToListAsync();
            ViewBag.RecentItems=await db.MenuItems.AsNoTracking().Include(x=>x.Vendor).Where(x=>recentIds.Contains(x.Id)&&x.IsVisible&&x.Stock>0).ToListAsync();
        }
        else{ViewBag.FavoriteIds=new List<int>();ViewBag.RecentItems=new List<MenuItem>();}
        return View(await items.OrderBy(x=>x.Name).Take(100).ToListAsync());
    }

    public async Task<IActionResult> Vendor(int id){var vendor=await db.Vendors.AsNoTracking().Include(x=>x.Items).ThenInclude(x=>x.Schedules).Include(x=>x.Items).ThenInclude(x=>x.Category).FirstOrDefaultAsync(x=>x.Id==id&&x.Status==ApprovalStatus.Approved);return vendor==null?NotFound():View(vendor);}

    [Authorize(Roles=Roles.Customer),HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Favorite(int id,string? returnUrl=null)
    {
        if(!await db.MenuItems.AnyAsync(x=>x.Id==id&&x.IsVisible))return NotFound();
        var favorite=await db.Favorites.FirstOrDefaultAsync(x=>x.CustomerId==Uid&&x.MenuItemId==id);
        if(favorite==null)db.Favorites.Add(new Favorite{CustomerId=Uid!,MenuItemId=id});else db.Favorites.Remove(favorite);
        await db.SaveChangesAsync();
        return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl:"/");
    }

    [Authorize(Roles=Roles.Customer)]
    public async Task<IActionResult> Favorites()=>View(await db.Favorites.AsNoTracking().Where(x=>x.CustomerId==Uid).Include(x=>x.MenuItem).ThenInclude(x=>x.Vendor).OrderByDescending(x=>x.CreatedUtc).Select(x=>x.MenuItem).ToListAsync());
}

