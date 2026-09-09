namespace ProductApp.Domain.SmartProduct;

public enum AiMessageRole
{
    User = 1,
    Assistant = 2,
    System = 3,
    Tool = 4
}

public enum AiMessageStatus
{
    Pending = 1,
    Streaming = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5
}

public enum AiAttachmentStatus
{
    Uploaded = 1,
    Indexing = 2,
    Indexed = 3,
    Failed = 4
}

public enum AiFeedbackRating
{
    Helpful = 1,
    NotHelpful = 2
}

public enum AiToolInvocationStatus
{
    Started = 1,
    Completed = 2,
    Denied = 3,
    Failed = 4
}

public enum AiSourceKind
{
    Product = 1,
    ProductVersion = 2,
    ProductionStep = 3,
    Experiment = 4,
    Resource = 5,
    MarketStudy = 6,
    Document = 7,
    Calculation = 8,
    UserContext = 9
}
