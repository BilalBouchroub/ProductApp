namespace ProductApp.Infrastructure.SmartProduct;

public sealed class SmartProductOptions
{
    public const string SectionName = "SmartProduct";
    public string Provider { get; set; } = "Gemini";
    public string Model { get; set; } = "gemini-3.6-flash";
    public string EmbeddingModel { get; set; } = "gemini-embedding-001";
    public int EmbeddingDimensions { get; set; } = 768;
    public string OpenAiApiBaseUrl { get; set; } = "https://api.openai.com/v1/";
    public string GeminiApiBaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaximumOutputTokens { get; set; } = 2500;
    public int MaximumFileSizeMb { get; set; } = 20;
    public int MaximumRagResults { get; set; } = 6;
    public string AttachmentRoot { get; set; } = "App_Data/ai-attachments";
    public bool StreamingEnabled { get; set; } = true;
}
