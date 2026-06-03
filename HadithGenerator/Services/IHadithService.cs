using HadithGenerator.Models.ViewModels;

namespace HadithGenerator.Services;

public interface IHadithService
{
    Task<HadithViewModel> GenerateNew();
    Task<HadithViewModel> GetRandomHadith();
}