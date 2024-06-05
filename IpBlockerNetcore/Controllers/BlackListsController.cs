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
    public class BlackListsController : Controller
    {
        private readonly IpBlockerNetcoreContext _context;

        public BlackListsController(IpBlockerNetcoreContext context)
        {
            _context = context;
        }

        // GET: BlackLists
        public async Task<IActionResult> Index()
        {
              return _context.BlackList != null ? 
                          View(await _context.BlackList.ToListAsync()) :
                          Problem("Entity set 'IpBlockerNetcoreContext.BlackList'  is null.");
        }

        // GET: BlackLists/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.BlackList == null)
            {
                return NotFound();
            }

            var blackList = await _context.BlackList
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blackList == null)
            {
                return NotFound();
            }

            return View(blackList);
        }

        // GET: BlackLists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BlackLists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DangerLevel,Date")] BlackList blackList)
        {
            if (ModelState.IsValid)
            {
                _context.Add(blackList);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blackList);
        }

        // GET: BlackLists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.BlackList == null)
            {
                return NotFound();
            }

            var blackList = await _context.BlackList.FindAsync(id);
            if (blackList == null)
            {
                return NotFound();
            }
            return View(blackList);
        }

        // POST: BlackLists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DangerLevel,Date")] BlackList blackList)
        {
            if (id != blackList.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blackList);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlackListExists(blackList.Id))
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
            return View(blackList);
        }

        // GET: BlackLists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.BlackList == null)
            {
                return NotFound();
            }

            var blackList = await _context.BlackList
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blackList == null)
            {
                return NotFound();
            }

            return View(blackList);
        }

        // POST: BlackLists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.BlackList == null)
            {
                return Problem("Entity set 'IpBlockerNetcoreContext.BlackList'  is null.");
            }
            var blackList = await _context.BlackList.FindAsync(id);
            if (blackList != null)
            {
                _context.BlackList.Remove(blackList);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlackListExists(int id)
        {
          return (_context.BlackList?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
