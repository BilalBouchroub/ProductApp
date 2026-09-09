using FluentValidation;
using MediatR;
using ProductApp.Application.Auth;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Application.Common;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Commercial;

public sealed record GetOptimizationRequestsQuery(PageRequest Page, OptimizationStatus? Status)
    : IRequest<PagedResult<OptimizationRequestDto>>;

public sealed record CreateOptimizationRequestCommand(Guid MarketStudyId, string Message,
    OptimizationPriority Priority, IReadOnlyList<string> RequestedChanges)
    : IRequest<OptimizationRequestDto>;

public sealed class CreateOptimizationRequestCommandValidator
    : AbstractValidator<CreateOptimizationRequestCommand>
{
    public CreateOptimizationRequestCommandValidator()
    {
        RuleFor(request => request.MarketStudyId).NotEmpty();
        RuleFor(request => request.Message).NotEmpty().MaximumLength(2000);
        RuleFor(request => request.RequestedChanges).NotEmpty();
        RuleForEach(request => request.RequestedChanges).NotEmpty().MaximumLength(250);
    }
}

public sealed class GetOptimizationRequestsHandler(ICommercialRepository repository)
    : IRequestHandler<GetOptimizationRequestsQuery, PagedResult<OptimizationRequestDto>>
{
    public async Task<PagedResult<OptimizationRequestDto>> Handle(
        GetOptimizationRequestsQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page.SafePage;
        var pageSize = request.Page.SafePageSize;
        var (items, total) = await repository.ListOptimizationRequestsAsync(
            page, pageSize, request.Status, cancellationToken);
        return new(items.Select(item => item.ToDto()).ToArray(), page, pageSize, total);
    }
}

public sealed class CreateOptimizationRequestHandler(ICommercialRepository repository,
    INotificationService notifications)
    : IRequestHandler<CreateOptimizationRequestCommand, OptimizationRequestDto>
{
    public async Task<OptimizationRequestDto> Handle(
        CreateOptimizationRequestCommand request, CancellationToken cancellationToken)
    {
        var study = await repository.GetStudyAsync(request.MarketStudyId, cancellationToken)
            ?? throw new MarketStudyNotFoundException(request.MarketStudyId);
        var optimizationRequest = OptimizationRequest.Create(study, request.Message,
            request.Priority, request.RequestedChanges);
        await repository.AddOptimizationRequestAsync(optimizationRequest, cancellationToken);
        await notifications.NotifyRoleAsync(AppRoles.ProductionManager, "Optimisation demandée",
            $"Une optimisation est demandée pour {study.ProductVersion.Name}.",
            NotificationType.Warning, nameof(OptimizationRequest), optimizationRequest.Id,
            cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return optimizationRequest.ToDto();
    }
}
