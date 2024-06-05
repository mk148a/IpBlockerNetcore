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
    public class BanLogsController : Controller
    {
        private readonly IpBlockerNetcoreContext _context;

        public BanLogsController(IpBlockerNetcoreContext context)
        {
            _context = context;
        }

        // GET: BanLogs
        public async Task<IActionResult> Index()
        {
            var bL = _context.BanLog.Where(x=>x.Date>=DateTime.Now.AddDays(-1)).AsEnumerable();
              return bL != null ? 
                          View(bL) :
                          Problem("Entity set 'IpBlockerNetcoreContext.BanLog'  is null.");
        }

        // GET: BanLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BanLog == null)
            {
                return NotFound();
            }

            var banLog = await _context.BanLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (banLog == null)
            {
                return NotFound();
            }

            return View(banLog);
        }

        // GET: BanLogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BanLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Data,Date")] BanLog banLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(banLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(banLog);
        }

        // GET: BanLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BanLog == null)
            {
                return NotFound();
            }

            var banLog = await _context.BanLog.FindAsync(id);
            if (banLog == null)
            {
                return NotFound();
            }
            return View(banLog);
        }

        // POST: BanLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Data,Date")] BanLog banLog)
        {
            if (id != banLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(banLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BanLogExists(banLog.Id))
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
            return View(banLog);
        }

        // GET: BanLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BanLog == null)
            {
                return NotFound();
            }

            var banLog = await _context.BanLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (banLog == null)
            {
                return NotFound();
            }

            return View(banLog);
        }

        // POST: BanLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BanLog == null)
            {
                return Problem("Entity set 'IpBlockerNetcoreContext.BanLog'  is null.");
            }
            var banLog = await _context.BanLog.FindAsync(id);
            if (banLog != null)
            {
                _context.BanLog.Remove(banLog);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BanLogExists(int id)
        {
          return (_context.BanLog?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
