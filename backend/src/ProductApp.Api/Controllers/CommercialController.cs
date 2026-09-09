using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Commercial.Queries;
using ProductApp.Domain.Common;

namespace ProductApp.Api.Controllers;

[ApiController, Route("api/commercial"), Authorize(Policy = SecurityPolicies.Commercial)]
public sealed class CommercialController(ISender sender) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<CommercialOverviewResponse> Overview(CancellationToken ct)
    {
        var studies = await sender.Send(new GetStudiesQuery(), ct);
        return new(studies.Count, studies.Count(x => x.Status is MarketStudyStatus.Draft or MarketStudyStatus.InProgress),
            studies.Count(x => x.Status == MarketStudyStatus.Validated), studies.Count == 0 ? 0 : Math.Round(studies.Average(x => x.GlobalScore), 2));
    }
}
public sealed record CommercialOverviewResponse(int TotalStudies, int InProgress, int Validated, decimal AverageGlobalScore);
