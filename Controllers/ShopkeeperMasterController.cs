using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using HaldiramPromotionalApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class ShopkeeperMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ShopkeeperMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ShopkeeperMaster
        public async Task<IActionResult> Index()
        {
            return View(await _context.ShopkeeperMasters.ToListAsync());
        }

        // GET: ShopkeeperMaster/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopkeeperMaster = await _context.ShopkeeperMasters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shopkeeperMaster == null)
            {
                return NotFound();
            }

            return View(shopkeeperMaster);
        }

        // GET: ShopkeeperMaster/Create
        public IActionResult Create()
        {
            var viewModel = new ShopkeeperMasterViewModel();
            return View(viewModel);
        }

        // POST: ShopkeeperMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ShopkeeperMasterViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var shopkeeperMaster = new ShopkeeperMaster
                {
                    Name = viewModel.Name,
                    StoreLocation = viewModel.StoreLocation,
                    StoreType = viewModel.StoreType,
                    Username = viewModel.Username,
                    Password = viewModel.Password, // In a real application, you should hash the password
                    PhoneNumber = viewModel.PhoneNumber
                };

                _context.Add(shopkeeperMaster);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Shopkeeper created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: ShopkeeperMaster/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopkeeperMaster = await _context.ShopkeeperMasters.FindAsync(id);
            if (shopkeeperMaster == null)
            {
                return NotFound();
            }

            var viewModel = new ShopkeeperMasterViewModel
            {
                Id = shopkeeperMaster.Id,
                Name = shopkeeperMaster.Name,
                StoreLocation = shopkeeperMaster.StoreLocation,
                StoreType = shopkeeperMaster.StoreType,
                Username = shopkeeperMaster.Username,
                Password = shopkeeperMaster.Password,
                PhoneNumber = shopkeeperMaster.PhoneNumber
            };

            return View(viewModel);
        }

        // POST: ShopkeeperMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ShopkeeperMasterViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var shopkeeperMaster = await _context.ShopkeeperMasters.FindAsync(id);
                    if (shopkeeperMaster == null)
                    {
                        return NotFound();
                    }

                    shopkeeperMaster.Name = viewModel.Name;
                    shopkeeperMaster.StoreLocation = viewModel.StoreLocation;
                    shopkeeperMaster.StoreType = viewModel.StoreType;
                    shopkeeperMaster.Username = viewModel.Username;
                    shopkeeperMaster.Password = viewModel.Password; // In a real application, you should hash the password
                    shopkeeperMaster.PhoneNumber = viewModel.PhoneNumber;

                    _context.Update(shopkeeperMaster);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Shopkeeper updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShopkeeperMasterExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: ShopkeeperMaster/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shopkeeperMaster = await _context.ShopkeeperMasters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (shopkeeperMaster == null)
            {
                return NotFound();
            }

            return View(shopkeeperMaster);
        }

        // POST: ShopkeeperMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shopkeeperMaster = await _context.ShopkeeperMasters.FindAsync(id);
            if (shopkeeperMaster != null)
            {
                _context.ShopkeeperMasters.Remove(shopkeeperMaster);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Shopkeeper deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool ShopkeeperMasterExists(int id)
        {
            return _context.ShopkeeperMasters.Any(e => e.Id == id);
        }
    }
}