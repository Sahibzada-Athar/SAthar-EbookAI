using EBookDashboard.Interfaces;
using EBookDashboard.Models;
using EBookDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace EBookDashboard.Controllers
{
    [Route("Admin/[controller]")]
    public class PlanFeaturesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPlanFeaturesService _planFeaturesService;

        public PlanFeaturesController(ApplicationDbContext context, IPlanFeaturesService planFeaturesService)
        {
            _planFeaturesService = planFeaturesService;
            _context = context;
        }
        // ✅ This action serves the Razor View
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            var pf = await _planFeaturesService.GetAllFeaturesAsync();
            return View("~/Views/Admin/PlanFeatures.cshtml", pf);
        }

        // ✅ Optional: JSON API for Vue.js
        [HttpGet("/api/planfeatures")]
        public async Task<IActionResult> GetAll()
        {
            var pf = await _planFeaturesService.GetAllFeaturesAsync();
            return Ok(pf);
        }
        [HttpPost("/api/planfeatures/add")]
        public async Task<IActionResult> AddFeature([FromBody] PlanFeatures model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _planFeaturesService.AddFeatureAsync(model);
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteFeature(int id)
        {
            var feature = await _context.PlanFeatures.FindAsync(id);
            if (feature == null)
                return NotFound(new { message = "Feature not found" });

            _context.PlanFeatures.Remove(feature);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Feature deleted successfully" });
        }

        // API route variant for SPA usage
        [HttpDelete("/api/planfeatures/{id}")]
        public async Task<ActionResult> ApiDeleteFeature(int id)
        {
            var feature = await _context.PlanFeatures.FindAsync(id);
            if (feature == null)
                return NotFound(new { message = "Feature not found" });

            _context.PlanFeatures.Remove(feature);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Feature deleted successfully" });
        }
    }

}
