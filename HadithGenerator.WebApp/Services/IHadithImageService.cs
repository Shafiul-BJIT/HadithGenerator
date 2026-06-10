using HadithGenerator.Models.ViewModels;

namespace HadithGenerator.Services;

public interface IHadithImageService
{
    bool IsConfigured { get; }
    int DesignCount { get; }
    Task<IReadOnlyList<HadithImageDesignViewModel>> GenerateDesigns(
        HadithViewModel hadith,
        CancellationToken cancellationToken = default);
}
