using FluentValidation;
using MediatR;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Products;
using ProductApp.Domain.Common;
using ProductApp.Application.Common;
using ProductApp.Application.Auth;

namespace ProductApp.Application.Commercial.Commands;

public sealed class MarketStudyValuesValidator : AbstractValidator<MarketStudyValues>
{
    public MarketStudyValuesValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TargetMarket).NotEmpty().MaximumLength(250);
        RuleFor(x => x.GeographicArea).NotEmpty().MaximumLength(250);
        RuleFor(x => x.CustomerSegment).NotEmpty().MaximumLength(500);
        RuleFor(x => x.StudyDate).NotEmpty(); RuleFor(x => x.EstimatedMarketSize).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AnnualGrowthRate).InclusiveBetween(-100, 1000);
        RuleFor(x => x.ProductionCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ProposedSalePrice).GreaterThan(0);
        RuleFor(x => x.AverageMarketPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MonthlySalesVolume).GreaterThanOrEqualTo(0);
    }
}

public sealed record CreateStudyCommand(Guid ProductVersionId, MarketStudyValues Values) : IRequest<MarketStudyDto>;
public sealed class CreateStudyCommandValidator : AbstractValidator<CreateStudyCommand>
{
    public CreateStudyCommandValidator()
    { RuleFor(x => x.ProductVersionId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new MarketStudyValuesValidator()); }
}
public sealed class CreateStudyCommandHandler(ICommercialRepository repository)
    : IRequestHandler<CreateStudyCommand, MarketStudyDto>
{
    public async Task<MarketStudyDto> Handle(CreateStudyCommand request, CancellationToken cancellationToken)
    {
        var version = await repository.GetProductVersionAsync(request.ProductVersionId, cancellationToken)
            ?? throw new CommercialWorkflowException("Version produit introuvable.");
        try { version.BeginMarketStudy(DateTime.UtcNow); }
        catch (InvalidOperationException exception) { throw new CommercialWorkflowException(exception.Message); }
        var v = request.Values;
        var study = MarketStudy.Create(version, v.Name, v.TargetMarket, v.GeographicArea,
            v.CustomerSegment, v.StudyDate, v.EstimatedMarketSize, v.AnnualGrowthRate,
            v.ProductionCost, v.ProposedSalePrice, v.AverageMarketPrice, v.MonthlySalesVolume);
        await repository.AddStudyAsync(study, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return study.ToDto();
    }
}

public sealed record UpdateStudyCommand(Guid StudyId, MarketStudyValues Values) : IRequest<MarketStudyDto>;
public sealed class UpdateStudyCommandValidator : AbstractValidator<UpdateStudyCommand>
{
    public UpdateStudyCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new MarketStudyValuesValidator()); }
}
public sealed class UpdateStudyCommandHandler(ICommercialRepository repository)
    : IRequestHandler<UpdateStudyCommand, MarketStudyDto>
{
    public async Task<MarketStudyDto> Handle(UpdateStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await StudyCommandHelpers.GetStudy(repository, request.StudyId, cancellationToken);
        CommercialCommandRules.EnsureEditable(study);
        var v = request.Values;
        study.Update(v.Name, v.TargetMarket, v.GeographicArea, v.CustomerSegment,
            v.StudyDate, v.EstimatedMarketSize, v.AnnualGrowthRate, v.ProductionCost,
            v.ProposedSalePrice, v.AverageMarketPrice, v.MonthlySalesVolume, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return study.ToDto();
    }
}

public sealed record SaveDraftCommand(Guid StudyId, MarketStudyValues Values) : IRequest<MarketStudyDto>;
public sealed class SaveDraftCommandValidator : AbstractValidator<SaveDraftCommand>
{
    public SaveDraftCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new MarketStudyValuesValidator()); }
}
public sealed class SaveDraftCommandHandler(ICommercialRepository repository)
    : IRequestHandler<SaveDraftCommand, MarketStudyDto>
{
    public async Task<MarketStudyDto> Handle(SaveDraftCommand request, CancellationToken cancellationToken)
    {
        var study = await StudyCommandHelpers.GetStudy(repository, request.StudyId, cancellationToken);
        CommercialCommandRules.EnsureEditable(study);
        var v = request.Values;
        study.SaveAsDraft(v.Name, v.TargetMarket, v.GeographicArea, v.CustomerSegment,
            v.StudyDate, v.EstimatedMarketSize, v.AnnualGrowthRate, v.ProductionCost,
            v.ProposedSalePrice, v.AverageMarketPrice, v.MonthlySalesVolume, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return study.ToDto();
    }
}

public sealed record DeleteStudyCommand(Guid StudyId) : IRequest;
public sealed class DeleteStudyCommandValidator : AbstractValidator<DeleteStudyCommand>
{ public DeleteStudyCommandValidator() => RuleFor(x => x.StudyId).NotEmpty(); }
public sealed class DeleteStudyCommandHandler(ICommercialRepository repository) : IRequestHandler<DeleteStudyCommand>
{
    public async Task Handle(DeleteStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await StudyCommandHelpers.GetStudy(repository, request.StudyId, cancellationToken);
        if (!study.CanBeDeleted) throw new CommercialWorkflowException("Seule une étude en brouillon peut être supprimée.");
        repository.RemoveStudy(study); await repository.SaveChangesAsync(cancellationToken);
    }
}

public sealed record ValidateStudyCommand(Guid StudyId) : IRequest<CommercialResultDto>;
public sealed class ValidateStudyCommandValidator : AbstractValidator<ValidateStudyCommand>
{ public ValidateStudyCommandValidator() => RuleFor(x => x.StudyId).NotEmpty(); }
public sealed class ValidateStudyCommandHandler(ICommercialRepository repository,
    ProductionScoreCalculator productionCalculator, MarketScoreCalculator marketCalculator,
    FinancialScoreCalculator financialCalculator, RiskScoreCalculator riskCalculator,
    GlobalScoreCalculator globalCalculator, RecommendationEngine recommendationEngine,
    INotificationService? notifications = null)
    : IRequestHandler<ValidateStudyCommand, CommercialResultDto>
{
    public async Task<CommercialResultDto> Handle(ValidateStudyCommand request, CancellationToken cancellationToken)
    {
        var study = await StudyCommandHelpers.GetStudy(repository, request.StudyId, cancellationToken);
        if (study.Status == MarketStudyStatus.Validated)
            throw new CommercialWorkflowException("Cette étude est déjà validée.");
        var production = productionCalculator.Calculate(study.ProductVersion, study.ProductionCost, study.ProposedSalePrice);
        var market = marketCalculator.Calculate(study);
        var financial = financialCalculator.Calculate(study);
        var risk = riskCalculator.Calculate(study.Risks);
        var scores = globalCalculator.Calculate(production, market, financial, risk);
        var result = recommendationEngine.Generate(study, scores);
        study.Validate(scores, result.Recommendation, DateTime.UtcNow);
        study.ProductVersion.ApplyRecommendation(result.Recommendation, DateTime.UtcNow);
        if (notifications is not null)
            await notifications.NotifyRoleAsync(AppRoles.ProductionManager, "Étude commerciale validée",
                $"L’étude {study.Name} recommande {result.Recommendation} pour {study.ProductVersion.Name}.",
                result.Recommendation == ProductApp.Domain.MarketAnalysis.MarketRecommendation.Viable
                    ? NotificationType.Success : NotificationType.Warning,
                nameof(MarketStudy), study.Id, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return new CommercialResultDto(study.Id, result.Recommendation,
            scores.ProductionScore, scores.MarketScore, scores.FinancialScore,
            scores.RiskScore, scores.GlobalScore, result.Strengths, result.Weaknesses,
            result.MainRisks, result.Improvements, result.AdvisedPrice, result.MinimumVolume);
    }
}

file static class StudyCommandHelpers
{
    public static async Task<MarketStudy> GetStudy(ICommercialRepository repository,
        Guid id, CancellationToken cancellationToken) =>
        await repository.GetStudyAsync(id, cancellationToken) ?? throw new MarketStudyNotFoundException(id);
}
