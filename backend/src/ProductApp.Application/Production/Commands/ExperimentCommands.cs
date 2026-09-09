using FluentValidation;
using MediatR;
using ProductApp.Application.Production.DTOs;
using ProductApp.Domain.Experiments;
using ProductApp.Domain.ProductionSteps;
using ProductApp.Domain.Common;
using ProductApp.Application.Common;
using ProductApp.Application.Auth;

namespace ProductApp.Application.Production.Commands;

public sealed record CreateProductionExperimentCommand(Guid ProductId, int VersionNumber,
    string Name, string Objective, string? Hypothesis, DateTime StartDate, decimal PlannedQuantity,
    string? Observations = null, string? Conclusion = null)
    : IRequest<ProductionExperimentDto>;
public sealed class CreateProductionExperimentCommandValidator : AbstractValidator<CreateProductionExperimentCommand>
{
    public CreateProductionExperimentCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty(); RuleFor(x => x.VersionNumber).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Hypothesis).MaximumLength(2000);
        RuleFor(x => x.Observations).MaximumLength(4000);
        RuleFor(x => x.Conclusion).MaximumLength(4000);
        RuleFor(x => x.StartDate).NotEmpty(); RuleFor(x => x.PlannedQuantity).GreaterThan(0);
    }
}
public sealed class CreateProductionExperimentCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<CreateProductionExperimentCommand, ProductionExperimentDto>
{
    public async Task<ProductionExperimentDto> Handle(CreateProductionExperimentCommand request, CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(request.ProductId, request.VersionNumber, cancellationToken)
            ?? throw new ProductVersionNotFoundException(request.ProductId, request.VersionNumber);
        var experimentSteps = version.ProductionSteps
            .Where(step => step.Status is RecordStatus.Active or RecordStatus.Validated)
            .OrderBy(step => step.Order)
            .ToArray();
        if (experimentSteps.Length == 0)
            throw new ProductionWorkflowException("Activez ou validez au moins une étape de production avant de créer une expérience.");
        var chain = ProductionChainCalculator.Calculate(experimentSteps);
        var experiment = ProductionExperiment.Create(version.Id, request.Name, request.Objective,
            request.Hypothesis, request.StartDate, request.PlannedQuantity,
            chain.TotalCost, chain.TotalDurationMinutes);
        experiment.UpdateNarrative(request.Objective, request.Hypothesis,
            request.Observations, request.Conclusion);
        foreach (var step in experimentSteps)
        {
            experiment.AddStep(ExperimentStep.Create(experiment.Id, step.Id, step.Order,
                step.PlannedCost ?? step.LaborCost, step.DurationMinutes, step.PlannedOutputQuantity));
        }
        await repository.AddExperimentAsync(experiment, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return experiment.ToDto();
    }
}

public sealed record RecordExperimentStepActualsCommand(Guid ExperimentId, Guid ExperimentStepId,
    decimal ActualCost, int ActualDurationMinutes, decimal? ActualOutputQuantity, string? Observations)
    : IRequest<ProductionExperimentDto>;

public sealed record UpdateProductionExperimentNarrativeCommand(Guid ExperimentId,
    string Objective, string? Hypothesis, string? Observations, string? Conclusion)
    : IRequest<ProductionExperimentDto>;
public sealed class UpdateProductionExperimentNarrativeCommandValidator
    : AbstractValidator<UpdateProductionExperimentNarrativeCommand>
{
    public UpdateProductionExperimentNarrativeCommandValidator()
    {
        RuleFor(x => x.ExperimentId).NotEmpty();
        RuleFor(x => x.Objective).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Hypothesis).MaximumLength(2000);
        RuleFor(x => x.Observations).MaximumLength(4000);
        RuleFor(x => x.Conclusion).MaximumLength(4000);
    }
}
public sealed class UpdateProductionExperimentNarrativeCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<UpdateProductionExperimentNarrativeCommand, ProductionExperimentDto>
{
    public async Task<ProductionExperimentDto> Handle(
        UpdateProductionExperimentNarrativeCommand request, CancellationToken cancellationToken)
    {
        var experiment = await repository.GetExperimentAsync(request.ExperimentId, cancellationToken)
            ?? throw new ProductionExperimentNotFoundException(request.ExperimentId);
        experiment.UpdateNarrative(request.Objective, request.Hypothesis,
            request.Observations, request.Conclusion);
        await repository.SaveChangesAsync(cancellationToken);
        return experiment.ToDto();
    }
}

public sealed class RecordExperimentStepActualsCommandValidator : AbstractValidator<RecordExperimentStepActualsCommand>
{
    public RecordExperimentStepActualsCommandValidator()
    {
        RuleFor(x => x.ExperimentId).NotEmpty(); RuleFor(x => x.ExperimentStepId).NotEmpty();
        RuleFor(x => x.ActualCost).GreaterThanOrEqualTo(0); RuleFor(x => x.ActualDurationMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ActualOutputQuantity).GreaterThanOrEqualTo(0).When(x => x.ActualOutputQuantity.HasValue);
        RuleFor(x => x.Observations).MaximumLength(4000);
    }
}
public sealed class RecordExperimentStepActualsCommandHandler(IProductionWorkflowRepository repository)
    : IRequestHandler<RecordExperimentStepActualsCommand, ProductionExperimentDto>
{
    public async Task<ProductionExperimentDto> Handle(RecordExperimentStepActualsCommand request, CancellationToken cancellationToken)
    {
        var experiment = await repository.GetExperimentAsync(request.ExperimentId, cancellationToken)
            ?? throw new ProductionExperimentNotFoundException(request.ExperimentId);
        var step = experiment.Steps.SingleOrDefault(item => item.Id == request.ExperimentStepId)
            ?? throw new ProductionWorkflowException("Étape d’expérience introuvable.");
        step.RecordActuals(request.ActualCost, request.ActualDurationMinutes,
            request.ActualOutputQuantity, request.Observations, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return experiment.ToDto();
    }
}

public sealed record CompleteProductionExperimentCommand(Guid ExperimentId, decimal ActualQuantity,
    ExperimentResult Result, string? Observations, string? Conclusion)
    : IRequest<ExperimentSummaryDto>;
public sealed class CompleteProductionExperimentCommandValidator : AbstractValidator<CompleteProductionExperimentCommand>
{
    public CompleteProductionExperimentCommandValidator()
    {
        RuleFor(x => x.ExperimentId).NotEmpty(); RuleFor(x => x.ActualQuantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Result).IsInEnum().NotEqual(ExperimentResult.Cancelled);
        RuleFor(x => x.Observations).MaximumLength(4000); RuleFor(x => x.Conclusion).MaximumLength(4000);
    }
}
public sealed class CompleteProductionExperimentCommandHandler(IProductionWorkflowRepository repository, INotificationService? notifications = null)
    : IRequestHandler<CompleteProductionExperimentCommand, ExperimentSummaryDto>
{
    public async Task<ExperimentSummaryDto> Handle(CompleteProductionExperimentCommand request, CancellationToken cancellationToken)
    {
        var experiment = await repository.GetExperimentAsync(request.ExperimentId, cancellationToken)
            ?? throw new ProductionExperimentNotFoundException(request.ExperimentId);
        var summary = experiment.Complete(request.ActualQuantity, request.Result,
            request.Observations, request.Conclusion, DateTime.UtcNow);
        if (notifications is not null)
            await notifications.NotifyRoleAsync(AppRoles.ProductionManager, "Expérience terminée",
                $"L’expérience {experiment.Name} est terminée avec le résultat {request.Result}.",
                NotificationType.Success, nameof(ProductionExperiment), experiment.Id, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return summary.ToDto();
    }
}
