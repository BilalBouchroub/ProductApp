using Moq;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Application.Tests.SmartProduct;

public sealed class SmartProductOwnershipTests
{
    [Fact]
    public async Task A_user_cannot_load_a_conversation_not_returned_for_their_identity()
    {
        var repository = new Mock<IAiConversationRepository>();
        var userId = Guid.NewGuid(); var conversationId = Guid.NewGuid();
        repository.Setup(item => item.GetOwnedAsync(conversationId, userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductApp.Domain.SmartProduct.AiConversation?)null);
        var service = new SmartProductService(repository.Object, Mock.Of<IAiProvider>(),
            Mock.Of<IAiBusinessToolCatalog>(), Mock.Of<IAiAttachmentStorage>(),
            Mock.Of<IAiKnowledgeService>(), TimeProvider.System);

        await Assert.ThrowsAsync<AiNotFoundException>(() => service.GetConversationAsync(
            new AiUserContext(userId, "ProductionManager", "Test"), conversationId, CancellationToken.None));
        repository.Verify(item => item.GetOwnedAsync(conversationId, userId, true, It.IsAny<CancellationToken>()), Times.Once);
    }
}
