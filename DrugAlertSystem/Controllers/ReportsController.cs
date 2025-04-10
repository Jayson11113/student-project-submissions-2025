using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DrugAlertSystem.Data;
using DrugAlertSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using DrugAlertSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace DrugAlertSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly DrugsDbContext _context;
        private readonly UserManager<DrugsUser> _userManager;

        public ReportsController(DrugsDbContext context, UserManager<DrugsUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reports
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Check if user is in an admin or law enforcement role
            bool isAdminOrLawEnforcement = User.IsInRole("Admin") || User.IsInRole("LawEnforcement");

            var reports = isAdminOrLawEnforcement
                ? _context.Reports.Include(r => r.User) // Return all reports for Admin/Law Enforcement
                : _context.Reports.Where(r => r.UserId == userId).Include(r => r.User); // Return only user's reports

            return View(await reports.ToListAsync());
        }


        // GET: Reports/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(d=>d.DrugHotspotData)
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET: Reports/Create
       
        public async Task<IActionResult> Create()
        {

            var user = (await _userManager.GetUserAsync(User));
           

            ViewData["UserId"] = user.Id;
            return View(new Report(){UserId = user.Id});
        }

        // POST: Reports/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
       
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([ValidateNever] Report report)
        {
            var user = (await _userManager.GetUserAsync(User));
             
             report.UserId = user.Id;

            if (ModelState.IsValid)
            {
                report.Id = Guid.NewGuid();
                
                _context.Add(report);
                await _context.SaveChangesAsync();
                return RedirectToAction("Predict", "DrugHotspot", new {id  = report.Id});
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", report.UserId);
            return View(report);
        }

        // GET: Reports/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", report.UserId);
            return View(report);
        }

        // POST: Reports/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,  Report report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(report);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReportExists(report.Id))
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", report.UserId);
            return View(report);
        }

        // GET: Reports/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.Reports
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // POST: Reports/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(Guid id)
        {
            return _context.Reports.Any(e => e.Id == id);
        }
    }
}
