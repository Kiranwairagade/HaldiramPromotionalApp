using HaldiramPromotionalApp.Data;
using HaldiramPromotionalApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaldiramPromotionalApp.Controllers
{
    public class MaterialMasterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaterialMasterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MaterialMaster
        public async Task<IActionResult> Index()
        {
            return View(await _context.MaterialMaster.ToListAsync());
        }

        // GET: MaterialMaster/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialMaster = await _context.MaterialMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materialMaster == null)
            {
                return NotFound();
            }

            return View(materialMaster);
        }

        // GET: MaterialMaster/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MaterialMaster/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ShortName,Materialname,Unit,Category,subcategory,sequence,segementname,material3partycode,price,isactive,CratesTypes,dealerprice")] MaterialMaster materialMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(materialMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(materialMaster);
        }

        // GET: MaterialMaster/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialMaster = await _context.MaterialMaster.FindAsync(id);
            if (materialMaster == null)
            {
                return NotFound();
            }
            return View(materialMaster);
        }

        // POST: MaterialMaster/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ShortName,Materialname,Unit,Category,subcategory,sequence,segementname,material3partycode,price,isactive,CratesTypes,dealerprice")] MaterialMaster materialMaster)
        {
            if (id != materialMaster.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(materialMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialMasterExists(materialMaster.Id))
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
            return View(materialMaster);
        }

        // GET: MaterialMaster/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialMaster = await _context.MaterialMaster
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materialMaster == null)
            {
                return NotFound();
            }

            return View(materialMaster);
        }

        // POST: MaterialMaster/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materialMaster = await _context.MaterialMaster.FindAsync(id);
            if (materialMaster != null)
            {
                _context.MaterialMaster.Remove(materialMaster);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaterialMasterExists(int id)
        {
            return _context.MaterialMaster.Any(e => e.Id == id);
        }
    }
}