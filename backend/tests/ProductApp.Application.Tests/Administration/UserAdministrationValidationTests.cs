using Moq;
using ProductApp.Application.Administration;
using ProductApp.Application.Auth;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Tests.Administration;

public sealed class UserAdministrationValidationTests
{
    [Theory]
    [InlineData(UserAdministrationAction.Activate)]
    [InlineData(UserAdministrationAction.Deactivate)]
    [InlineData(UserAdministrationAction.Suspend)]
    [InlineData(UserAdministrationAction.Delete)]
    [InlineData(UserAdministrationAction.ResetPassword)]
    public void ActionValidator_AcceptsActionsWithoutRole(UserAdministrationAction action)
    {
        var command = new ExecuteUserActionCommand(Guid.NewGuid(), Guid.NewGuid(), action);
        Assert.True(new ExecuteUserActionCommandValidator().Validate(command).IsValid);
    }

    [Fact]
    public void ActionValidator_RequiresKnownRoleForRoleChange()
    {
        var validator = new ExecuteUserActionCommandValidator();
        Assert.False(validator.Validate(new ExecuteUserActionCommand(Guid.NewGuid(), Guid.NewGuid(), UserAdministrationAction.ChangeRole)).IsValid);
        Assert.False(validator.Validate(new ExecuteUserActionCommand(Guid.NewGuid(), Guid.NewGuid(), UserAdministrationAction.ChangeRole, "Unknown")).IsValid);
        Assert.True(validator.Validate(new ExecuteUserActionCommand(Guid.NewGuid(), Guid.NewGuid(), UserAdministrationAction.ChangeRole, "Administrator")).IsValid);
    }

    [Fact]
    public async Task Handler_MapsDeactivateToInactiveStatus()
    {
        var targetId = Guid.NewGuid();
        var expected = User(targetId, UserStatus.Inactive);
        var repository = new Mock<IAdministrationRepository>();
        repository.Setup(x => x.ChangeStatusAsync(targetId, UserStatus.Inactive, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new ExecuteUserActionHandler(repository.Object, Mock.Of<IAuthService>());

        var result = await handler.Handle(new(targetId, Guid.NewGuid(), UserAdministrationAction.Deactivate), CancellationToken.None);

        Assert.False(result.Deleted);
        Assert.Equal(UserStatus.Inactive, result.User!.Status);
        repository.VerifyAll();
    }

    [Fact]
    public async Task Handler_DeletesThroughOneActionCommand()
    {
        var targetId = Guid.NewGuid();
        var repository = new Mock<IAdministrationRepository>();
        repository.Setup(x => x.DeleteUserAsync(targetId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new ExecuteUserActionHandler(repository.Object, Mock.Of<IAuthService>());

        var result = await handler.Handle(new(targetId, Guid.NewGuid(), UserAdministrationAction.Delete), CancellationToken.None);

        Assert.True(result.Deleted);
        Assert.Null(result.User);
        repository.VerifyAll();
    }

    [Fact]
    public async Task Handler_RejectsDestructiveSelfAction()
    {
        var id = Guid.NewGuid();
        var handler = new ExecuteUserActionHandler(Mock.Of<IAdministrationRepository>(), Mock.Of<IAuthService>());

        await Assert.ThrowsAsync<AdministrationConflictException>(() =>
            handler.Handle(new(id, id, UserAdministrationAction.Delete), CancellationToken.None));
    }

    [Fact]
    public async Task Handler_RequestsPasswordResetForTargetEmail()
    {
        var targetId = Guid.NewGuid();
        var target = User(targetId, UserStatus.Active);
        var repository = new Mock<IAdministrationRepository>();
        repository.Setup(x => x.GetUserAsync(targetId, It.IsAny<CancellationToken>())).ReturnsAsync(target);
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.ForgotPasswordAsync(target.Email, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var handler = new ExecuteUserActionHandler(repository.Object, auth.Object);

        var result = await handler.Handle(new(targetId, Guid.NewGuid(), UserAdministrationAction.ResetPassword), CancellationToken.None);

        Assert.Equal(targetId, result.User!.Id);
        repository.VerifyAll();
        auth.VerifyAll();
    }

    private static UserDto User(Guid id, UserStatus status) => new(id, "Alice", "Martin", "Alice Martin",
        "alice@productapp.local", null, "ProductionManager", status, DateTime.UtcNow, DateTime.UtcNow, null);
}
