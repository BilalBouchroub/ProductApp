using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Application.Products;
using ProductApp.Application.ProductionSteps;
using ProductApp.Application.MarketAnalysis;
using ProductApp.Domain.Products;
using ProductApp.Application.Production;
using ProductApp.Application.Commercial;
using ProductApp.Application.Auth;
using ProductApp.Application.Administration;
using Microsoft.EntityFrameworkCore;
using ProductApp.Application.SmartProduct;

namespace ProductApp.Api.Infrastructure;

public sealed class ApiExceptionHandler(
    ILogger<ApiExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var problemDetails = exception switch
        {
            ValidationException validationException => CreateValidationProblem(validationException),
            ProductNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Product not found"),
            ProductCodeConflictException => CreateProblem(StatusCodes.Status409Conflict, "Product code conflict"),
            ProductArchivedException => CreateProblem(StatusCodes.Status409Conflict, "Product is archived"),
            ProductionStepNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Production step not found"),
            ProductionStepOrderConflictException => CreateProblem(StatusCodes.Status409Conflict, "Production step order conflict"),
            InvalidProductionStepOrderingException => CreateProblem(StatusCodes.Status400BadRequest, "Invalid production step ordering"),
            SapMaterialNotFoundException => CreateProblem(StatusCodes.Status400BadRequest, "SAP material not found"),
            MarketAnalysisPrerequisiteException => CreateProblem(StatusCodes.Status409Conflict, "Market analysis prerequisites are missing"),
            ProductVersionNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Product version not found"),
            StepResourceNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Step resource not found"),
            ProductionExperimentNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Production experiment not found"),
            ProductionWorkflowException workflowException => CreateProblem(StatusCodes.Status409Conflict, workflowException.Message),
            MarketStudyNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Market study not found"),
            CompetitorNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Competitor not found"),
            CommercialRiskNotFoundException => CreateProblem(StatusCodes.Status404NotFound, "Commercial risk not found"),
            CommercialWorkflowException commercialException => CreateProblem(StatusCodes.Status409Conflict, commercialException.Message),
            AuthenticationException authenticationException => CreateProblem(StatusCodes.Status401Unauthorized, authenticationException.Message),
            InvalidRefreshTokenException => CreateProblem(StatusCodes.Status401Unauthorized, "Invalid or expired refresh token"),
            AdministrationNotFoundException notFound => CreateProblem(StatusCodes.Status404NotFound, notFound.Message),
            AdministrationConflictException conflict => CreateProblem(StatusCodes.Status409Conflict, conflict.Message),
            InvitationDeliveryException deliveryException => CreateProblem(
                StatusCodes.Status503ServiceUnavailable, deliveryException.Message),
            AiNotFoundException notFound => CreateProblem(StatusCodes.Status404NotFound, notFound.Message),
            AiAccessDeniedException => CreateProblem(StatusCodes.Status403Forbidden, "Accès SMART PRODUCT refusé"),
            AiAttachmentValidationException attachment => CreateProblem(StatusCodes.Status400BadRequest, attachment.Message),
            AiProviderUnavailableException => CreateProblem(StatusCodes.Status503ServiceUnavailable, "SMART PRODUCT est temporairement indisponible"),
            DbUpdateConcurrencyException => CreateProblem(StatusCodes.Status409Conflict, "The resource was modified by another request."),
            DbUpdateException => CreateProblem(StatusCodes.Status409Conflict, "The requested database operation conflicts with existing data."),
            ArgumentException argument => CreateProblem(StatusCodes.Status400BadRequest, argument.Message),
            _ => CreateProblem(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (problemDetails.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred while processing the request.");
            if (environment.IsDevelopment()) problemDetails.Detail = exception.Message;
        }

        problemDetails.Instance = httpContext.Request.Path;
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, options: null,
            contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }

    private static ProblemDetails CreateProblem(int status, string title)
    {
        return new ProblemDetails { Status = status, Title = title };
    }

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed"
        };
    }
}
