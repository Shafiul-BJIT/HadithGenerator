using HadithGenerator.Models.ViewModels;

namespace HadithGenerator.Services;

public class HadithService : IHadithService
{
    public Task<HadithViewModel> GenerateNew()
    {
        return GetRandomHadith();
    }

    public Task<HadithViewModel> GetRandomHadith()
    {
        var hadith = new HadithViewModel
        {
            HadithNo = 1,
            BanglaRaw = "আবূ হুরায়রা (রাঃ) থেকে বর্ণিত...",
            NoteRaw = "এই হাদীসটি মুসলিম শরীফে বর্ণিত।",
            BookNameBN = "মুসলিম শরীফ",
            SectionBN = "রোজা ও নামাজ",
            StatusBN = "সাহিহ",
            ExplanationRaw = "এই হাদীস থেকে বোঝা যায় যে ঈমান ও আনুগত্য একে অপরের সাথে সম্পর্কিত।"
        };

        return Task.FromResult(hadith);
    }
}