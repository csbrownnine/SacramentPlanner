﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SacramentPlanner.Data;
using SacramentPlanner.Models;

namespace SacramentPlanner.Controllers
{
    [Authorize]
    public class SacramentMeetingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BishopbricList bl;

        public SacramentMeetingsController(ApplicationDbContext context)
        {
            _context = context;
            bl = new BishopbricList();
            bl.LeaderList = new List<Bishopbric>
            {
                new Bishopbric {LeaderName = "Jerry Smith", Position = "Bishop"},
                new Bishopbric {LeaderName = "Jack Johnson", Position = "1st Counselor"},
                new Bishopbric {LeaderName = "John Miller", Position = "2nd Counselor"},
            };
        }

        // GET: SacramentMeetings
        public async Task<IActionResult> Index(
            string sortOrder,
            DateTime currentFilter,
            DateTime startingDate,
            int? page)
        {
            DateTime emptyDate = new DateTime();

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DateSortParm"] = String.IsNullOrEmpty(sortOrder) ? "date_desc" : "";
            ViewData["NameSortParm"] = sortOrder == "Name" ? "name_desc" : "Name";

            if(startingDate != emptyDate)
            {
                page = 1;
            }
            else
            {
                startingDate = currentFilter;
            }

            ViewData["CurrentFilter"] = startingDate;

            var sacramentMeetings = from s in _context.SacramentMeetings select s;

            if (startingDate.ToString() != "")
            {
                sacramentMeetings = sacramentMeetings.Where(s => s.MeetingDate >= startingDate);
            }

            switch(sortOrder)
            {
                case "name_desc":
                    sacramentMeetings = sacramentMeetings.OrderByDescending(s => s.ConductorName);
                    break;
                case "Name":
                    sacramentMeetings = sacramentMeetings.OrderBy(s => s.ConductorName);
                    break;
                case "date_desc":
                    sacramentMeetings = sacramentMeetings.OrderByDescending(s => s.MeetingDate);
                    break;
                default:
                    sacramentMeetings = sacramentMeetings.OrderBy(s => s.MeetingDate);
                    break;
            }

            int pageSize = 9;
            return View(await PaginatedList<SacramentMeeting>.CreateAsync(sacramentMeetings.Include(s => s.Speakers).AsNoTracking(), page ?? 1, pageSize));
        }

        // GET: SacramentMeetings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sacramentMeeting = await _context.SacramentMeetings
                .Include(s => s.Speakers)
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.ID == id);
            if (sacramentMeeting == null)
            {
                return NotFound();
            }

            return View(sacramentMeeting);
        }

        // GET: SacramentMeetings/Create
        public IActionResult Create()
        {
            ViewData["Bishopbric"] = new SelectList(bl.LeaderList, "LeaderName", "LeaderName");
            return View();
        }

        // POST: SacramentMeetings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,MeetingDate,SpecialNote,ConductorName,OpeningSongNumber,OpeningSongTitle,OpeningPrayerName,SacramentSongNumber,SacramentSongTitle,IntermediateSongNumber,IntermediateSongTitle,ClosingSongNumber,ClosingSongTitle,ClosingPrayerName")] SacramentMeeting sacramentMeeting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sacramentMeeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(sacramentMeeting);
        }

        // GET: SacramentMeetings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sacramentMeeting = await _context.SacramentMeetings.SingleOrDefaultAsync(m => m.ID == id);
            if (sacramentMeeting == null)
            {
                return NotFound();
            }
            ViewData["Bishopbric"] = new SelectList(bl.LeaderList, "LeaderName", "LeaderName", sacramentMeeting.ConductorName);
            return View(sacramentMeeting);
        }

        // POST: SacramentMeetings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,MeetingDate,SpecialNote,ConductorName,OpeningSongNumber,OpeningSongTitle,OpeningPrayerName,SacramentSongNumber,SacramentSongTitle,IntermediateSongNumber,IntermediateSongTitle,ClosingSongNumber,ClosingSongTitle,ClosingPrayerName")] SacramentMeeting sacramentMeeting)
        {
            if (id != sacramentMeeting.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sacramentMeeting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SacramentMeetingExists(sacramentMeeting.ID))
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
            return View(sacramentMeeting);
        }

        // GET: SacramentMeetings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sacramentMeeting = await _context.SacramentMeetings
                .SingleOrDefaultAsync(m => m.ID == id);
            if (sacramentMeeting == null)
            {
                return NotFound();
            }

            return View(sacramentMeeting);
        }

        // POST: SacramentMeetings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sacramentMeeting = await _context.SacramentMeetings.SingleOrDefaultAsync(m => m.ID == id);
            _context.SacramentMeetings.Remove(sacramentMeeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SacramentMeetingExists(int id)
        {
            return _context.SacramentMeetings.Any(e => e.ID == id);
        }
    }
}
