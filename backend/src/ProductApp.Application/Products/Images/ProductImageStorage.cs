using MediatR;

namespace ProductApp.Application.Products.Images;

public interface IProductImageStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken);
}

public sealed record UploadProductImageCommand(Stream Content, string FileName, string ContentType, long Length)
    : IRequest<string>;

public sealed class UploadProductImageCommandHandler(IProductImageStorage storage)
    : IRequestHandler<UploadProductImageCommand, string>
{
    private static readonly IReadOnlyDictionary<string, string> Extensions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp"
        };

    public Task<string> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        if (request.Length is <= 0 or > 5 * 1024 * 1024)
            throw new ArgumentException("L'image doit avoir une taille maximale de 5 Mo.");
        if (!Extensions.TryGetValue(request.ContentType, out var extension))
            throw new ArgumentException("Seules les images JPEG, PNG et WebP sont acceptees.");
        return storage.SaveAsync(request.Content, extension, cancellationToken);
    }
}
