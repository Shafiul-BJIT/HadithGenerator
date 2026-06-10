using System.Diagnostics;
using System.Text.Json;
using HadithGenerator.Models;
using HadithGenerator.Models.ViewModels;
using HadithGenerator.Services;
using Microsoft.AspNetCore.Mvc;

namespace HadithGenerator.Controllers;

public class HadithController : Controller
{
    private readonly IHadithService _hadithService;
    private readonly IHadithImageService _hadithImageService;

    public HadithController(IHadithService hadithService, IHadithImageService hadithImageService)
    {
        _hadithService = hadithService;
        _hadithImageService = hadithImageService;
    }

    // GET
    public async Task<IActionResult> Index()
    {
        var hadithViewModel = await _hadithService.GetRandomHadith();
        hadithViewModel.IsImageGenerationConfigured = _hadithImageService.IsConfigured;
        hadithViewModel.ImageDesignCount = _hadithImageService.DesignCount;

        return View(hadithViewModel);
    }

    public async Task<IActionResult> GenerateNewHadith()
    {
        var hadithViewModel = await _hadithService.GenerateNew();
        hadithViewModel.IsImageGenerationConfigured = _hadithImageService.IsConfigured;
        hadithViewModel.ImageDesignCount = _hadithImageService.DesignCount;
        return View("Index", hadithViewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateImages(
        HadithViewModel hadithViewModel,
        CancellationToken cancellationToken)
    {
        hadithViewModel.IsImageGenerationConfigured = _hadithImageService.IsConfigured;
        hadithViewModel.ImageDesignCount = _hadithImageService.DesignCount;

        if (!hadithViewModel.IsImageGenerationConfigured)
        {
            hadithViewModel.ImageGenerationError =
                "OpenRouter image generation is not configured. Add an OpenRouter API key first.";
            return View("Index", hadithViewModel);
        }

        try
        {
            hadithViewModel.ImageDesigns = (await _hadithImageService.GenerateDesigns(
                hadithViewModel,
                cancellationToken)).ToList();

            if (hadithViewModel.ImageDesigns.Count != _hadithImageService.DesignCount)
            {
                hadithViewModel.ImageGenerationError =
                    $"OpenRouter returned {hadithViewModel.ImageDesigns.Count} of " +
                    $"{_hadithImageService.DesignCount} requested designs.";
            }
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            hadithViewModel.ImageGenerationError =
                "OpenRouter image generation failed. Check the API key, credits, quota, and network.";
        }

        return View("Index", hadithViewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
