using MediatR;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Commercial.Queries;

public sealed record GetStudyQuery(Guid StudyId) : IRequest<MarketStudyDto>;
public sealed class GetStudyQueryHandler(ICommercialRepository repository)
    : IRequestHandler<GetStudyQuery, MarketStudyDto>
{
    public async Task<MarketStudyDto> Handle(GetStudyQuery request, CancellationToken cancellationToken) =>
        (await repository.GetStudyAsync(request.StudyId, cancellationToken)
            ?? throw new MarketStudyNotFoundException(request.StudyId)).ToDto();
}

public sealed record GetStudiesQuery(Guid? ProductVersionId = null) : IRequest<IReadOnlyList<MarketStudyDto>>;
public sealed class GetStudiesQueryHandler(ICommercialRepository repository)
    : IRequestHandler<GetStudiesQuery, IReadOnlyList<MarketStudyDto>>
{
    public async Task<IReadOnlyList<MarketStudyDto>> Handle(GetStudiesQuery request, CancellationToken cancellationToken) =>
        (await repository.ListStudiesAsync(request.ProductVersionId, cancellationToken))
            .Select(study => study.ToDto()).ToArray();
}

public sealed record GetStudyResultQuery(Guid StudyId) : IRequest<CommercialResultDto>;
public sealed class GetStudyResultQueryHandler(ICommercialRepository repository,
    RecommendationEngine recommendationEngine)
    : IRequestHandler<GetStudyResultQuery, CommercialResultDto>
{
    public async Task<CommercialResultDto> Handle(GetStudyResultQuery request, CancellationToken cancellationToken)
    {
        var study = await repository.GetStudyAsync(request.StudyId, cancellationToken)
            ?? throw new MarketStudyNotFoundException(request.StudyId);
        if (study.Status != MarketStudyStatus.Validated || !study.Recommendation.HasValue)
            throw new CommercialWorkflowException("L’étude doit être validée avant de consulter son résultat.");
        var scores = new CommercialScoreResult(study.ProductionScore, study.MarketScore,
            study.FinancialScore, study.RiskScore, study.GlobalScore);
        var result = recommendationEngine.Generate(study, scores);
        return new CommercialResultDto(study.Id, result.Recommendation,
            scores.ProductionScore, scores.MarketScore, scores.FinancialScore,
            scores.RiskScore, scores.GlobalScore, result.Strengths, result.Weaknesses,
            result.MainRisks, result.Improvements, result.AdvisedPrice, result.MinimumVolume);
    }
}
