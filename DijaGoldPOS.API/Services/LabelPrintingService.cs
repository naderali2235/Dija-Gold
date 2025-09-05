using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DijaGoldPOS.API.Data;
using DijaGoldPOS.API.IServices;
using DijaGoldPOS.API.Models;
using DijaGoldPOS.API.Models.ProductModels;
using Microsoft.EntityFrameworkCore;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Zebra label printing service using raw ZPL over TCP (default port 9100) or Windows print share
/// </summary>
public class LabelPrintingService : ILabelPrintingService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<LabelPrintingService> _logger;
    private readonly IConfiguration _configuration;

    public LabelPrintingService(ApplicationDbContext db,
        ILogger<LabelPrintingService> logger,
        IConfiguration configuration)
    {
        _db = db;
        _logger = logger;
        _configuration = configuration;
    }

    public string GenerateProductQrPayload(Product product)
    {
        var payload = new
        {
            v = 1,
            // minimal fields used by POS to look up product quickly
            id = product.Id,
            code = product.ProductCode
        };
        var json = JsonSerializer.Serialize(payload);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    public async Task<Product?> DecodeQrPayloadAsync(string payload)
    {
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            int? id = doc.RootElement.TryGetProperty("id", out var idEl) ? idEl.GetInt32() : null;
            string? code = doc.RootElement.TryGetProperty("code", out var cEl) ? cEl.GetString() : null;

            if (id.HasValue)
            {
                var byId = await _db.Products.FirstOrDefaultAsync(p => p.Id == id.Value);
                if (byId != null) return byId;
            }
            if (!string.IsNullOrWhiteSpace(code))
            {
                var byCode = await _db.Products.FirstOrDefaultAsync(p => p.ProductCode == code);
                if (byCode != null) return byCode;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to decode QR payload");
            return null;
        }
    }

    public string GenerateProductLabelZpl(Product product, int copies = 1)
    {
        // Config
        var companyName = _configuration["CompanyInfo:Name"] ?? "Dija Gold";
        var labelWidthDots = _configuration.GetValue<int?>("LabelPrinter:WidthDots") ?? 600; // ~3in @203dpi
        var density = _configuration.GetValue<int?>("LabelPrinter:Density") ?? 8; // print darkness

        // QR payload
        var payload = GenerateProductQrPayload(product);

        // Compose ZPL for small jewelry tag similar to sample images
        var sb = new StringBuilder();
        sb.AppendLine("^XA");
        sb.AppendLine($"^MD{density}");
        sb.AppendLine("^PW" + labelWidthDots);
        sb.AppendLine("^LH0,0");
        // Back-side style per sample: QR on left, Kxx and W:<ProductCode> on right
        // QR code block (left)
        sb.AppendLine("^FO20,20^BQN,2,5");
        sb.AppendLine("^FDLA," + EscapeZpl(payload) + "^FS");
        // Karat (right-top)
        sb.AppendLine("^FO230,20^A0N,32,32^FDK" + product.KaratTypeId.ToString() + "^FS");
        // W is used in your tag to indicate product code
        sb.AppendLine("^FO230,60^A0N,28,28^FDW:^FS");
        sb.AppendLine("^FO270,60^A0N,28,28^FD" + EscapeZpl(product.ProductCode) + "^FS");
        // Optional: brand/name small under code
        sb.AppendLine("^FO230,100^A0N,22,22^FD" + EscapeZpl(Shorten(product.Name, 18)) + "^FS");
        // copies
        sb.AppendLine($"^PQ{Math.Max(1, copies)}");
        sb.AppendLine("^XZ");
        return sb.ToString();
    }

    public async Task<bool> PrintZplAsync(string zpl)
    {
        var host = _configuration["LabelPrinter:Host"];
        var port = _configuration.GetValue<int?>("LabelPrinter:Port") ?? 9100;
        var windowsPrinterShare = _configuration["LabelPrinter:WindowsPrinterName"]; // optional

        try
        {
            if (!string.IsNullOrWhiteSpace(host))
            {
                using var client = new TcpClient();
                await client.ConnectAsync(host, port);
                var buffer = Encoding.UTF8.GetBytes(zpl);
                using var stream = client.GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
                await stream.FlushAsync();
                _logger.LogInformation("Sent ZPL to {Host}:{Port}", host, port);
                return true;
            }
            if (OperatingSystem.IsWindows() && !string.IsNullOrWhiteSpace(windowsPrinterShare))
            {
                // Fallback: send via RAW print to Windows printer using PrintDocument
                // Note: Zebra expects raw text; PrintDocument can be used to send via graphics-less method
#pragma warning disable CA1416 // Validate platform compatibility
                using var raw = new System.Drawing.Printing.PrintDocument();
                raw.PrinterSettings.PrinterName = windowsPrinterShare;
                raw.PrintPage += (s, e) =>
                {
                    var font = new System.Drawing.Font("Consolas", 8);
                    e.Graphics?.DrawString(zpl, font, System.Drawing.Brushes.Black, 0, 0);
                };
                raw.Print();
#pragma warning restore CA1416 // Validate platform compatibility
                _logger.LogInformation("Sent ZPL to Windows printer {Printer}", windowsPrinterShare);
                return true;
            }

            _logger.LogError("No label printer configured. Set LabelPrinter:Host or LabelPrinter:WindowsPrinterName");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error printing ZPL");
            return false;
        }
    }

    public async Task<bool> PrintProductLabelAsync(int productId, int copies = 1)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return false;
        var zpl = GenerateProductLabelZpl(product, copies);
        return await PrintZplAsync(zpl);
    }

    private static string EscapeZpl(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("^", " ").Replace("~", "-");
    }

    private static string Shorten(string input, int max)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Length <= max ? input : input.Substring(0, max);
    }
}


