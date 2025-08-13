using System.Collections.Concurrent;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DesafioBackend.Application.Middleware;

/// <summary>
/// Provides validation filtering for endpoint requests using FluentValidation.
/// </summary>
/// <typeparam name="TRequest">The type of the request to validate.</typeparam>
public class ValidationFilter<TRequest> : IEndpointFilter
{
    private const string ValidationErrorTitle = "Validation Error";
    private const string ValidationErrorDetail = "An error occurred while validating the request.";
    private const int InitialArraySize = 4;
    private const int MaxArraySize = 16;

    private static readonly ConcurrentDictionary<string, string[]> ErrorArrayCache = new();
    private readonly IValidator<TRequest> _validator;

    /// <summary>
    /// Initializes a new instance of the ValidationFilter class.
    /// </summary>
    /// <param name="validator">The validator instance for the request type.</param>
    /// <exception cref="ArgumentNullException">Thrown when validator is null.</exception>
    public ValidationFilter(IValidator<TRequest> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Validates the request before proceeding with the endpoint execution.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The delegate to invoke the next filter or endpoint.</param>
    /// <returns>The result of the endpoint execution or a validation problem.</returns>
    /// <summary>
    /// Validates the request before proceeding with the endpoint execution.
    /// </summary>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            var request = ExtractRequest(context);
            if (request is null)
            {
                return CreateRequestNotFoundResult();
            }

            var validationResult = await ValidateRequestAsync(request, context.HttpContext.RequestAborted);
            if (!validationResult.IsValid)
            {
                return CreateValidationProblemResult(validationResult);
            }

            return await next(context);
        }
        catch (Exception)
        {
            return CreateInternalErrorResult();
        }
    }

    /// <summary>
    /// Extracts the typed request from the endpoint context.
    /// </summary>
    private static TRequest? ExtractRequest(EndpointFilterInvocationContext context)
    {
        foreach (var arg in context.Arguments)
        {
            if (arg is TRequest typedRequest)
            {
                return typedRequest;
            }
        }
        return default;
    }

    /// <summary>
    /// Validates the request using the configured validator.
    /// </summary>
    private Task<ValidationResult> ValidateRequestAsync(TRequest request, CancellationToken cancellationToken)
    {
        return _validator.ValidateAsync(request, cancellationToken);
    }

    /// <summary>
    /// Creates a validation problem result from validation errors.
    /// </summary>
    private static IResult CreateValidationProblemResult(ValidationResult validationResult)
    {
        var errors = GroupValidationErrors(validationResult);
        return TypedResults.ValidationProblem(errors);
    }

    /// <summary>
    /// Groups validation errors by property name using cached arrays to reduce allocations.
    /// </summary>
    private static Dictionary<string, string[]> GroupValidationErrors(ValidationResult validationResult)
    {
        var errors = new Dictionary<string, string[]>(validationResult.Errors.Count);
        var currentProperty = string.Empty;
        var currentArray = Array.Empty<string>();
        var currentIndex = 0;

        // Pre-sort the errors by property name to avoid allocating groups
        var sortedErrors = validationResult.Errors
            .OrderBy(e => e.PropertyName);

        foreach (var error in sortedErrors)
        {
            if (error.PropertyName != currentProperty)
            {
                // If we have collected errors for the previous property, add them to the dictionary
                if (currentIndex > 0)
                {
                    var finalArray = new string[currentIndex];
                    Array.Copy(currentArray, finalArray, currentIndex);
                    errors[currentProperty] = finalArray;
                }

                // Start collecting errors for the new property
                currentProperty = error.PropertyName;
                var arraySize = Math.Min(
                    Math.Max(InitialArraySize, MaxArraySize),
                    validationResult.Errors.Count);

                // Get or create an array from the cache
                currentArray = ErrorArrayCache.GetOrAdd(
                    arraySize.ToString(),
                    _ => new string[arraySize]);
                currentIndex = 0;
            }

            if (currentIndex < currentArray.Length)
            {
                currentArray[currentIndex++] = error.ErrorMessage;
            }
        }

        // Add the last property's errors
        if (currentIndex > 0)
        {
            var finalArray = new string[currentIndex];
            Array.Copy(currentArray, finalArray, currentIndex);
            errors[currentProperty] = finalArray;
        }

        return errors;
    }

    /// <summary>
    /// Creates a bad request result when the request type is not found.
    /// </summary>
    private static IResult CreateRequestNotFoundResult()
    {
        return TypedResults.BadRequest($"Request of type {typeof(TRequest).Name} not found in the context.");
    }

    /// <summary>
    /// Creates an internal server error result.
    /// </summary>
    private static IResult CreateInternalErrorResult()
    {
        return TypedResults.Problem(
            title: ValidationErrorTitle,
            detail: ValidationErrorDetail,
            statusCode: StatusCodes.Status500InternalServerError);
    }
}

/// <summary>
/// Extension methods for adding validation filters to route handlers.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds a validation filter for the specified request type to the route handler.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request to validate.</typeparam>
    /// <param name="builder">The route handler builder.</param>
    /// <returns>The route handler builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static RouteHandlerBuilder AddValidationFilter<TRequest>(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .AddEndpointFilter<ValidationFilter<TRequest>>()
            .ProducesValidationProblem();
    }
}