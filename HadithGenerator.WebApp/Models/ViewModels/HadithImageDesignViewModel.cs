namespace HadithGenerator.Models.ViewModels;

public class HadithImageDesignViewModel
{
    public string Name { get; set; } = string.Empty;
    public string MimeType { get; set; } = "image/png";
    public string Base64Data { get; set; } = string.Empty;

    public string DataUrl => $"data:{MimeType};base64,{Base64Data}";
}
