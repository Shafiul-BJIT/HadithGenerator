namespace HadithGenerator.Models.ViewModels;

public class HadithViewModel
{
    public int HadithNo { get; set; }
    public string BanglaRaw { get; set; } = string.Empty;
    public string NoteRaw { get; set; }  = string.Empty;
    public string BookNameBN { get; set; } = string.Empty;
    public string SectionBN { get; set; } = string.Empty;
    public string StatusBN { get; set; } = string.Empty;
    public string ExplanationRaw { get; set; } = string.Empty;
    public List<HadithImageDesignViewModel> ImageDesigns { get; set; } = [];
    public bool IsImageGenerationConfigured { get; set; }
    public int ImageDesignCount { get; set; } = 1;
    public string ImageGenerationError { get; set; } = string.Empty;
}
