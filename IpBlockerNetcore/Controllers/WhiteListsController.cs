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
    public class WhiteListsController : Controller
    {
        private readonly IpBlockerNetcoreContext _context;

        public WhiteListsController(IpBlockerNetcoreContext context)
        {
            _context = context;
        }

        // GET: WhiteLists
        public async Task<IActionResult> Index()
        {
              return _context.WhiteList != null ? 
                          View(await _context.WhiteList.ToListAsync()) :
                          Problem("Entity set 'IpBlockerNetcoreContext.WhiteList'  is null.");
        }

        // GET: WhiteLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.WhiteList == null)
            {
                return NotFound();
            }

            var whiteList = await _context.WhiteList
                .FirstOrDefaultAsync(m => m.Id == id);
            if (whiteList == null)
            {
                return NotFound();
            }

            return View(whiteList);
        }

        // GET: WhiteLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: WhiteLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DangerLevel,Date")] WhiteList whiteList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(whiteList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(whiteList);
        }

        // GET: WhiteLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.WhiteList == null)
            {
                return NotFound();
            }

            var whiteList = await _context.WhiteList.FindAsync(id);
            if (whiteList == null)
            {
                return NotFound();
            }
            return View(whiteList);
        }

        // POST: WhiteLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DangerLevel,Date")] WhiteList whiteList)
        {
            if (id != whiteList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(whiteList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!WhiteListExists(whiteList.Id))
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
            return View(whiteList);
        }

        // GET: WhiteLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.WhiteList == null)
            {
                return NotFound();
            }

            var whiteList = await _context.WhiteList
                .FirstOrDefaultAsync(m => m.Id == id);
            if (whiteList == null)
            {
                return NotFound();
            }

            return View(whiteList);
        }

        // POST: WhiteLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.WhiteList == null)
            {
                return Problem("Entity set 'IpBlockerNetcoreContext.WhiteList'  is null.");
            }
            var whiteList = await _context.WhiteList.FindAsync(id);
            if (whiteList != null)
            {
                _context.WhiteList.Remove(whiteList);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool WhiteListExists(int id)
        {
          return (_context.WhiteList?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
