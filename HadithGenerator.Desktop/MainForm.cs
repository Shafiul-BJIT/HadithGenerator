using System.Net;
using System.Text.RegularExpressions;
using HadithGenerator.Models.ViewModels;
using HadithGenerator.Services;

namespace HadithGenerator.Desktop;

public sealed class MainForm : Form
{
    private readonly IHadithService _hadithService;
    private readonly Button _newHadithButton = new();
    private readonly Label _statusLabel = new();
    private readonly WebBrowser _reader = new();
    private readonly ProgressBar _progress = new();
    private HadithViewModel _hadith = new();

    public MainForm(IHadithService hadithService)
    {
        _hadithService = hadithService;

        InitializeComponent();

        Text = "Hadith Generator";
        MinimumSize = new Size(900, 650);
        Size = new Size(1180, 800);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(244, 247, 244);
        Font = new Font("Segoe UI", 10);

        BuildLayout();
        Shown += async (_, _) => await LoadHadithAsync();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(20)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(root);

        var header = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0, 0, 0, 12)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var title = new Label
        {
            AutoSize = true,
            Text = "Random Sahih Hadith",
            Font = new Font("Segoe UI Semibold", 20),
            ForeColor = Color.FromArgb(23, 59, 36),
            Anchor = AnchorStyles.Left
        };
        ConfigureButton(_newHadithButton, "Generate New", Color.FromArgb(39, 105, 65));
        _newHadithButton.Click += async (_, _) => await LoadHadithAsync();
        header.Controls.Add(title, 0, 0);
        header.Controls.Add(_newHadithButton, 1, 0);
        root.Controls.Add(header, 0, 0);

        _reader.Dock = DockStyle.Fill;
        _reader.AllowWebBrowserDrop = false;
        _reader.IsWebBrowserContextMenuEnabled = true;
        _reader.ScriptErrorsSuppressed = true;
        root.Controls.Add(_reader, 0, 1);

        var footer = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Margin = new Padding(0, 10, 0, 0)
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        _statusLabel.AutoSize = true;
        _statusLabel.Anchor = AnchorStyles.Left;
        _statusLabel.ForeColor = Color.FromArgb(82, 103, 90);
        _statusLabel.Text = "Ready";
        _progress.Style = ProgressBarStyle.Marquee;
        _progress.MarqueeAnimationSpeed = 30;
        _progress.Width = 150;
        _progress.Visible = false;

        footer.Controls.Add(_statusLabel, 0, 0);
        footer.Controls.Add(_progress, 1, 0);
        root.Controls.Add(footer, 0, 2);
    }

    private async Task LoadHadithAsync()
    {
        SetBusy(true, "Loading hadith...");

        try
        {
            _hadith = await _hadithService.GenerateNew();
            RenderHadith();
            _statusLabel.Text = _hadith.HadithNo == 0
                ? "No Sahih hadith found. Try again."
                : "Hadith loaded.";
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _statusLabel.Text = "Could not load a hadith. Check the internet connection.";
            RenderMessage("Could not load a hadith.");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void RenderHadith()
    {
        if (_hadith.HadithNo == 0)
        {
            RenderMessage("No Sahih hadith found. Please generate again.");
            return;
        }

        var html = $$"""
            <!doctype html>
            <html>
            <head>
              <meta charset="utf-8">
              <style>
                body { font-family: "Nirmala UI", "Segoe UI", sans-serif; margin: 0; padding: 24px;
                       color: #17231b; background: #fff; line-height: 1.75; font-size: 17px; }
                h1 { margin: 0 0 4px; color: #173b24; font-size: 28px; }
                h2 { margin: 24px 0 8px; font-size: 20px; color: #276941; }
                .meta { color: #52675a; padding-bottom: 16px; border-bottom: 1px solid #dce7de; }
                .status { display: inline-block; margin-left: 8px; padding: 2px 9px;
                          border-radius: 12px; background: #e5f2e8; color: #276941; }
                .content { margin-top: 20px; }
                .panel { margin-top: 20px; padding: 14px 18px; background: #f5f8f5;
                         border-left: 4px solid #6b9678; }
              </style>
            </head>
            <body>
              <h1>{{Encode(_hadith.BookNameBN)}}</h1>
              <div class="meta">
                Hadith {{_hadith.HadithNo}} | {{Encode(_hadith.SectionBN)}}
                <span class="status">{{Encode(_hadith.StatusBN)}}</span>
              </div>
              <div class="content">{{SanitizeHtml(_hadith.BanglaRaw)}}</div>
              {{BuildSection("Note", _hadith.NoteRaw)}}
              {{BuildSection("Explanation", _hadith.ExplanationRaw)}}
            </body>
            </html>
            """;

        _reader.DocumentText = html;
    }

    private void RenderMessage(string message)
    {
        _reader.DocumentText = $"""
            <!doctype html><html><head><meta charset="utf-8"></head>
            <body style="font-family:'Segoe UI';padding:24px;color:#52675a">
            <h2>{Encode(message)}</h2></body></html>
            """;
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _newHadithButton.Enabled = !busy;
        _progress.Visible = busy;
        UseWaitCursor = busy;

        if (message is not null)
        {
            _statusLabel.Text = message;
        }
    }

    private static void ConfigureButton(Button button, string text, Color color)
    {
        button.AutoSize = true;
        button.Text = text;
        button.Padding = new Padding(10, 5, 10, 5);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = color;
        button.ForeColor = Color.White;
        button.Cursor = Cursors.Hand;
    }

    private static string BuildSection(string title, string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        return $"<section class=\"panel\"><h2>{Encode(title)}</h2>{SanitizeHtml(content)}</section>";
    }

    private static string SanitizeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var withoutScripts = Regex.Replace(
            html,
            @"<(script|style)\b[^>]*>.*?</\1>",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var withoutEventHandlers = Regex.Replace(
            withoutScripts,
            @"\s+on\w+\s*=\s*(['""]).*?\1",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
        return Regex.Replace(
            withoutEventHandlers,
            @"\s+(href|src)\s*=\s*(['""])\s*(javascript|data):.*?\2",
            string.Empty,
            RegexOptions.IgnoreCase | RegexOptions.Singleline);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        SuspendLayout();
        // 
        // MainForm
        // 
        ClientSize = new Size(284, 261);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        ResumeLayout(false);

    }

    private static string Encode(string value) => WebUtility.HtmlEncode(value);
}
