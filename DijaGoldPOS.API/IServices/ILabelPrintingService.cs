using DijaGoldPOS.API.Models;

namespace DijaGoldPOS.API.Services;

/// <summary>
/// Service for generating and printing product labels (Zebra ZPL) and QR payloads
/// </summary>
public interface ILabelPrintingService
{
    /// <summary>
    /// Generate compact QR payload string for a product. This is what will be encoded in the QR code.
    /// </summary>
    /// <param name="product">Product entity</param>
    /// <returns>Encoded payload string (base64 JSON)</returns>
    string GenerateProductQrPayload(Product product);

    /// <summary>
    /// Decode a previously generated QR payload and return the corresponding product if found
    /// </summary>
    Task<Product?> DecodeQrPayloadAsync(string payload);

    /// <summary>
    /// Generate ZPL content for a jewelry small label with QR + basic info
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="copies">Number of copies in ZPL (^PQ)</param>
    /// <returns>ZPL script</returns>
    string GenerateProductLabelZpl(Product product, int copies = 1);

    /// <summary>
    /// Send raw ZPL to configured Zebra printer
    /// </summary>
    Task<bool> PrintZplAsync(string zpl);

    /// <summary>
    /// Convenience: fetch product by id, generate label ZPL and print
    /// </summary>
    Task<bool> PrintProductLabelAsync(int productId, int copies = 1);
}


