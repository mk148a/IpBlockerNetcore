using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IpBlockerNetcore.Data;
using IpBlockerNetcore.Models.Domain;

namespace IpBlockerNetcore.Controllers
{
    public class ScanIpsController : Controller
    {
        private readonly IpBlockerNetcoreContext _context;

        public ScanIpsController(IpBlockerNetcoreContext context)
        {
            _context = context;
        }

        // GET: ScanIps
        public async Task<IActionResult> Index()
        {
              return _context.ScanIp != null ? 
                          View(await _context.ScanIp.ToListAsync()) :
                          Problem("Entity set 'IpBlockerNetcoreContext.ScanIp'  is null.");
        }

        // GET: ScanIps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ScanIp == null)
            {
                return NotFound();
            }

            var scanIp = await _context.ScanIp
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scanIp == null)
            {
                return NotFound();
            }

            return View(scanIp);
        }

        // GET: ScanIps/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ScanIps/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date")] ScanIp scanIp)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scanIp);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(scanIp);
        }

        // GET: ScanIps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ScanIp == null)
            {
                return NotFound();
            }

            var scanIp = await _context.ScanIp.FindAsync(id);
            if (scanIp == null)
            {
                return NotFound();
            }
            return View(scanIp);
        }

        // POST: ScanIps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Date")] ScanIp scanIp)
        {
            if (id != scanIp.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scanIp);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScanIpExists(scanIp.Id))
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
            return View(scanIp);
        }

        // GET: ScanIps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ScanIp == null)
            {
                return NotFound();
            }

            var scanIp = await _context.ScanIp
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scanIp == null)
            {
                return NotFound();
            }

            return View(scanIp);
        }

        // POST: ScanIps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ScanIp == null)
            {
                return Problem("Entity set 'IpBlockerNetcoreContext.ScanIp'  is null.");
            }
            var scanIp = await _context.ScanIp.FindAsync(id);
            if (scanIp != null)
            {
                _context.ScanIp.Remove(scanIp);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScanIpExists(int id)
        {
          return (_context.ScanIp?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
