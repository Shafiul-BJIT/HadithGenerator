using HadithGenerator.Models.ViewModels;
using HadithGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace HadithGenerator.Controllers;

public class HadithController : Controller
{
    private readonly IHadithService _hadithService;

    public HadithController(IHadithService hadithService)
    {
        _hadithService = hadithService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var hadithViewModel = await _hadithService.GetRandomHadith();
        
        return View(hadithViewModel);
    }

    public async Task<IActionResult> GenerateNewHadith()
    {
        var hadithViewModel = await _hadithService.GenerateNew();
        return View("Index", hadithViewModel);
    }
}