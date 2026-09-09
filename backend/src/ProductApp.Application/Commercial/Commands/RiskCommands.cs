using FluentValidation;
using MediatR;
using ProductApp.Application.Commercial.DTOs;
using ProductApp.Domain.Commercial;
using ProductApp.Domain.Common;

namespace ProductApp.Application.Commercial.Commands;

public sealed class RiskValuesValidator : AbstractValidator<RiskValues>
{
    public RiskValuesValidator()
    {
        RuleFor(x => x.RiskType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Probability).InclusiveBetween(1, 5); RuleFor(x => x.Impact).InclusiveBetween(1, 5);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.MitigationAction).MaximumLength(2000);
    }
}

public sealed record CreateRiskCommand(Guid StudyId, RiskValues Values) : IRequest<RiskDto>;
public sealed class CreateRiskCommandValidator : AbstractValidator<CreateRiskCommand>
{
    public CreateRiskCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new RiskValuesValidator()); }
}
public sealed class CreateRiskCommandHandler(ICommercialRepository repository) : IRequestHandler<CreateRiskCommand, RiskDto>
{
    public async Task<RiskDto> Handle(CreateRiskCommand request, CancellationToken cancellationToken)
    {
        var study = await repository.GetStudyAsync(request.StudyId, cancellationToken)
            ?? throw new MarketStudyNotFoundException(request.StudyId);
        CommercialCommandRules.EnsureEditable(study);
        var v = request.Values;
        var risk = Risk.Create(study.Id, v.RiskType, v.Probability, v.Impact,
            v.Description, v.MitigationAction);
        await repository.AddRiskAsync(risk, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);
        return risk.ToDto();
    }
}

public sealed record UpdateRiskCommand(Guid StudyId, Guid RiskId, RiskValues Values) : IRequest<RiskDto>;
public sealed class UpdateRiskCommandValidator : AbstractValidator<UpdateRiskCommand>
{
    public UpdateRiskCommandValidator()
    { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.RiskId).NotEmpty(); RuleFor(x => x.Values).SetValidator(new RiskValuesValidator()); }
}
public sealed class UpdateRiskCommandHandler(ICommercialRepository repository) : IRequestHandler<UpdateRiskCommand, RiskDto>
{
    public async Task<RiskDto> Handle(UpdateRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = await repository.GetRiskAsync(request.RiskId, cancellationToken)
            ?? throw new CommercialRiskNotFoundException(request.RiskId);
        if (risk.MarketStudyId != request.StudyId) throw new CommercialRiskNotFoundException(request.RiskId);
        CommercialCommandRules.EnsureEditable(risk.MarketStudy);
        var v = request.Values;
        risk.Update(v.RiskType, v.Probability, v.Impact, v.Description, v.MitigationAction, DateTime.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
        return risk.ToDto();
    }
}

public sealed record DeleteRiskCommand(Guid StudyId, Guid RiskId) : IRequest;
public sealed class DeleteRiskCommandValidator : AbstractValidator<DeleteRiskCommand>
{ public DeleteRiskCommandValidator() { RuleFor(x => x.StudyId).NotEmpty(); RuleFor(x => x.RiskId).NotEmpty(); } }
public sealed class DeleteRiskCommandHandler(ICommercialRepository repository) : IRequestHandler<DeleteRiskCommand>
{
    public async Task Handle(DeleteRiskCommand request, CancellationToken cancellationToken)
    {
        var risk = await repository.GetRiskAsync(request.RiskId, cancellationToken)
            ?? throw new CommercialRiskNotFoundException(request.RiskId);
        if (risk.MarketStudyId != request.StudyId) throw new CommercialRiskNotFoundException(request.RiskId);
        CommercialCommandRules.EnsureEditable(risk.MarketStudy); repository.RemoveRisk(risk);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
