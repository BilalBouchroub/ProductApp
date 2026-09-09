using ProductApp.Domain.SmartProduct;

namespace ProductApp.Domain.Tests.SmartProduct;

public sealed class AiConversationTests
{
    [Fact]
    public void Conversation_is_owned_and_can_hold_business_context()
    {
        var userId = Guid.NewGuid(); var productId = Guid.NewGuid(); var now = DateTime.UtcNow;
        var conversation = AiConversation.Create(userId, productId, null, now);
        Assert.Equal(userId, conversation.UserId); Assert.Equal(productId, conversation.ProductId);
        Assert.Equal("Nouvelle conversation", conversation.Title); Assert.False(conversation.IsArchived);
    }

    [Fact]
    public void User_message_cannot_be_empty()
    {
        Assert.Throws<ArgumentException>(() => AiMessage.CreateUser(Guid.NewGuid(), "  ", DateTime.UtcNow, Guid.NewGuid()));
    }

    [Fact]
    public void Assistant_status_follows_generation_lifecycle()
    {
        var userId = Guid.NewGuid(); var message = AiMessage.CreateAssistant(Guid.NewGuid(), "test-model", DateTime.UtcNow, userId);
        message.Start(DateTime.UtcNow, userId); Assert.Equal(AiMessageStatus.Streaming, message.Status);
        message.Complete("Réponse", null, 10, 5, 120, DateTime.UtcNow, userId);
        Assert.Equal(AiMessageStatus.Completed, message.Status); Assert.Equal(15, message.InputTokens + message.OutputTokens);
    }
}
