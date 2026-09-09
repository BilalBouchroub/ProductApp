using Microsoft.AspNetCore.Hosting;
using ProductApp.Application.Products.Images;

namespace ProductApp.Infrastructure.Products;

public sealed class LocalProductImageStorage(IWebHostEnvironment environment) : IProductImageStorage
{
    public async Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken)
    {
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var directory = Path.Combine(webRoot, "uploads", "products");
        Directory.CreateDirectory(directory);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var path = Path.Combine(directory, fileName);
        await using var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None,
            81920, FileOptions.Asynchronous);
        await content.CopyToAsync(output, cancellationToken);
        return $"/uploads/products/{fileName}";
    }
}
