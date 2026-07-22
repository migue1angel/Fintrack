using ErrorOr;
using FluentValidation;
using MediatR;

namespace FinTrack.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr 
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
        => _validator = validator;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        if (_validator is null) return await next();

        var result = await _validator.ValidateAsync(request, ct);
        if (result.IsValid) return await next();

        var errors = result.Errors
            .ConvertAll(f => Error.Validation(
                code: f.PropertyName,
                description: f.ErrorMessage));

        return (TResponse)(dynamic)errors.First();
    }
}