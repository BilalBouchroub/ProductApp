using FluentValidation;
using MediatR;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Commercial.Commands;

public sealed class CompetitorValuesValidator : AbstractValidator<CompetitorValues>
{
    public CompetitorValuesValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0); RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.EstimatedQualityScore).InclusiveBetween(0, 100).When(x => x.EstimatedQualityScore.HasValue);
        RuleFor(x => x.MarketShare).InclusiveBetween(0, 100).When(x => x.MarketShare.HasValue);
        RuleFor(x => x.CustomerRating).InclusiveBetween(0, 5).When(x => x.CustomerRating.HasValue);
        RuleFor(x => x.Strengths).MaximumLength(2000); RuleFor(x => x.Weaknesses).MaximumLength(2000);
        RuleFor(x => x.SalesChannels).MaximumLength(1000);
    }
}

public sealed record CreateCompetitorCommand(Guid StudyId, CompetitorValues Values) : IRequest<CompetitorDto>;
public sealed class CreateCompetitorCommandValidator : AbstractValidator<CreateCompetitorCommand>
{
    public CreateCompetitorCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new CompetitorValuesValidator()); }
}
public sealed class CreateCompetitorCommandHandler(ICommercialRepository repository)
    : IRequestHandler<CreateCompetitorCommand, CompetitorDto>
{
    public async Task<CompetitorDto> Handle(CreateCompetitorCommand request, CancellationToken cancellationToken)
    {
        var study = await repository.GetStudyAsync(request.StudyId, cancellationToken)
            ?? throw new MarketStudyNotFoundException(request.StudyId);
        CommercialCommandRules.EnsureEditable(study);
        var v = request.Values;
        var competitor = Competitor.Create(study.Id, v.Name, v.ProductName, v.Price,
            v.Quantity, v.EstimatedQualityScore, v.MarketShare, v.Strengths, v.Weaknesses,
            v.SalesChannels, v.CustomerRating);
        await repository.AddCompetitorAsync(competitor, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return competitor.ToDto();
    }
}

public sealed record UpdateCompetitorCommand(Guid StudyId, Guid CompetitorId, CompetitorValues Values) : IRequest<CompetitorDto>;
public sealed class UpdateCompetitorCommandValidator : AbstractValidator<UpdateCompetitorCommand>
{
    public UpdateCompetitorCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.CompetitorId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new CompetitorValuesValidator()); }
}
public sealed class UpdateCompetitorCommandHandler(ICommercialRepository repository)
    : IRequestHandler<UpdateCompetitorCommand, CompetitorDto>
{
    public async Task<CompetitorDto> Handle(UpdateCompetitorCommand request, CancellationToken cancellationToken)
    {
        var competitor = await repository.GetCompetitorAsync(request.CompetitorId, cancellationToken)
            ?? throw new CompetitorNotFoundException(request.CompetitorId);
        if (competitor.MarketStudyId != request.StudyId) throw new CompetitorNotFoundException(request.CompetitorId);
        CommercialCommandRules.EnsureEditable(competitor.MarketStudy);
        var v = request.Values;
        competitor.Update(v.Name, v.ProductName, v.Price, v.Quantity, v.EstimatedQualityScore,
            v.MarketShare, v.Strengths, v.Weaknesses, v.SalesChannels, v.CustomerRating, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return competitor.ToDto();
    }
}

public sealed record DeleteCompetitorCommand(Guid StudyId, Guid CompetitorId) : IRequest;
public sealed class DeleteCompetitorCommandValidator : AbstractValidator<DeleteCompetitorCommand>
{ public DeleteCompetitorCommandValidator() { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.CompetitorId).NotEmpty(); } }
public sealed class DeleteCompetitorCommandHandler(ICommercialRepository repository) : IRequestHandler<DeleteCompetitorCommand>
{
    public async Task Handle(DeleteCompetitorCommand request, CancellationToken cancellationToken)
    {
        var competitor = await repository.GetCompetitorAsync(request.CompetitorId, cancellationToken)
            ?? throw new CompetitorNotFoundException(request.CompetitorId);
        if (competitor.MarketStudyId != request.StudyId) throw new CompetitorNotFoundException(request.CompetitorId);
        CommercialCommandRules.EnsureEditable(competitor.MarketStudy); repository.RemoveCompetitor(competitor);
        await repository.SaveChangesAsync(cancellationToken);
    }
}

internal static class CommercialCommandRules
{
    public static void EnsureEditable(MarketStudy study)
    {
        if (study.Status == MarketStudyStatus.Validated)
            throw new CommercialWorkflowException("Une étude validée ne peut plus être modifiée.");
    }
}
