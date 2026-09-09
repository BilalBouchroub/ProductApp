using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductApp.Api.Controllers;
using ProductApp.Application.Administration;

namespace ProductApp.Api.Tests.Rest;

public sealed class UsersControllerActionTests
{
    [Fact]
    public async Task ExecuteAction_ForwardsTargetActorAndActionToApplication()
    {
        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(x => x.Send(
                It.Is<ExecuteUserActionCommand>(command => command.Id == targetId
                    && command.ActorId == actorId
                    && command.Action == UserAdministrationAction.Deactivate),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserActionResultDto(false, null));
        var controller = new UsersController(sender.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, actorId.ToString())], "test"))
                }
            }
        };

        await controller.ExecuteAction(targetId, new(UserAdministrationAction.Deactivate), CancellationToken.None);

        sender.VerifyAll();
    }
}
