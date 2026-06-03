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
}