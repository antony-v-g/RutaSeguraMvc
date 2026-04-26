using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RutaSeguraMvc.Data;
using RutaSeguraMvc.Models;

namespace RutaSeguraMvc.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Reportes.OrderByDescending(r => r.Fecha).ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reporte reporte)
    {
        if (ModelState.IsValid)
        {
            reporte.Fecha = DateTime.Now;
            reporte.Estado = "Pendiente";
            _context.Reportes.Add(reporte);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(reporte);
    }
}