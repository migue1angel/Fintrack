using ErrorOr;
using FinTrack.Common.Auth;
using FinTrack.Common.Behaviors;
using FinTrack.Common.Contracts;
using FinTrack.Common.Persistence;
using FinTrack.Modules.Budgets;
using FinTrack.Modules.Budgets.Features.Create;
using FinTrack.Modules.Budgets.Features.GetSummary;
using FinTrack.Modules.Goals;
using FinTrack.Modules.Goals.Features.AddContribution;
using FinTrack.Modules.Goals.Features.Cancel;
using FinTrack.Modules.Goals.Features.Create;
using FinTrack.Modules.Goals.Features.GetProgress;
using FinTrack.Modules.Transactions;
using FinTrack.Modules.Transactions.Features.Create;
using FinTrack.Modules.Transactions.Features.GetById;
using FinTrack.Modules.Transactions.Features.GetHistory;
using FinTrack.Modules.Users;
using FinTrack.Modules.Users.Errors;
using FinTrack.Modules.Users.Features.GetMe;
using FinTrack.Modules.Users.Features.Login;
using FinTrack.Modules.Users.Features.Register;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
            builder.Configuration.GetConnectionString("Default") ??
            throw new InvalidOperationException("Connection string is required"),
            npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                npgsql.CommandTimeout(10);
            }
        )
        .UseSnakeCaseNamingConvention()
);

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


// Modules-------------
builder.Services.AddUsersModule();
builder.Services.AddTransactionsModule();
builder.Services.AddGoalsModule();
builder.Services.AddBudgetsModule();


builder.Services
    .ConfigureHttpJsonOptions(options =>                                                                                                                                                     
        options.SerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
{
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

    ctx.Response.ContentType = "application/json";

    switch (ex)
    {
        case ValidationException ve:
            ctx.Response.StatusCode = 400;
            var msg = ve.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed";
            await ctx.Response.WriteAsJsonAsync(new { Error = msg });
            break;
        default:
            ctx.Response.StatusCode = 500;
            await ctx.Response.WriteAsJsonAsync(new { Error = "Internal Server Error" });
            break;
    }
}));


// USERS ------------------------------------------------
app.MapPost("users/register", async (RegisterCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);

    return result.Match(
        success => Results.Created($"/users/{success.UserId}", success),
        errors => errors.First().Type switch
        {
            ErrorType.Conflict => Results.Conflict(new { Error = errors.First().Description }),
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapPost("/users/login", async (LoginCommand command, IMediator mediator) =>
{
    var result = await mediator.Send(command);
    return result.Match(
        success => Results.Ok(new { Token = success.Token, UserId = success.UserId }),
        errors => errors.First().Type switch
        {
            ErrorType.Unauthorized => Results.Json(new { Error = errors.First().Description }, statusCode: 401),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapGet("/users/me", async (HttpContext http, IMediator mediator, IJwtTokenService jwtTokenService) =>
{
    var userId = jwtTokenService.ExtractUserId(http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(new GetMeQuery(userId.Value));
    return result.Match(
        success => Results.Ok(success),
        errors => Results.NotFound(new { Error = errors.First().Description }));
});

//TRANSACTIONS ---------------------------------------------------

app.MapPost("/transactions", async (
    HttpContext http,
    CreateTransactionBody body,
    IMediator mediator,
    IJwtTokenService jwtTokenService) =>
{
    var userId = jwtTokenService.ExtractUserId(http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var idempotencyKey = http.Request.Headers["Idempotency-Key"]
        .FirstOrDefault();

    var command = new CreateTransactionCommand(
        userId.Value,
        body.Amount,
        body.Type,
        body.Category,
        body.Description,
        body.TransactionDate,
        idempotencyKey);

    var result = await mediator.Send(command);

    return result.Match(
        success => success.WasDuplicate
            ? Results.Ok(new { TransactionId = success.TransactionId, WasDuplicate = true })
            : Results.Created($"/transactions/{success.TransactionId}",
                new { TransactionId = success.TransactionId, WasDuplicate = false }),
        errors => errors.First().Type switch
        {
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapGet("/transactions/{id:guid}", async (
    Guid id,
    HttpContext http,
    IJwtTokenService jwtTokenService,
    IMediator mediator) =>
{
    var userId = jwtTokenService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(new GetTransactionByIdQuery(id, userId.Value));

    return result.Match(
        success => Results.Ok(success),
        errors => errors.First().Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapGet("/transactions", async (
    HttpContext http,
    IJwtTokenService jwtTokenService,
    IMediator mediator,
    int page = 1,
    int pageSize = 20,
    string? category = null,
    string? type = null) =>
{
    var userId = jwtTokenService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var transactionType = TransactionType.None;
    if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransactionType>(type, true, out var parsedType))
    {
        transactionType = parsedType;
    }

    var result = await mediator.Send(
        new GetTransactionHistoryQuery(userId.Value, page, pageSize, category, transactionType));

    return result.Match(
        success => Results.Ok(success),
        errors => Results.BadRequest(new { Error = errors.First().Description }));
});

// GOALS -------------------------------------------
app.MapPost("/goals", async (
    HttpContext http,
    CreateGoalBody body,
    IMediator mediator,
    IJwtTokenService jwtService) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var command = new CreateGoalCommand(
        userId.Value, body.Name, body.Description,
        body.TargetAmount, body.TargetDate);

    var result = await mediator.Send(command);
    return result.Match(
        success => Results.Created($"/goals/{success.GoalId}", success),
        errors => errors.First().Type switch
        {
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapPost("/goals/{id:guid}/contributions", async (
    Guid id,
    HttpContext http,
    AddContributionBody body,
    IMediator mediator,
    IJwtTokenService jwtService) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(
        new AddContributionCommand(id, userId.Value, body.Amount, body.Note));

    return result.Match(
        success => Results.Ok(success),
        errors => errors.First().Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { Error = errors.First().Description }),
            ErrorType.Conflict => Results.Conflict(new { Error = errors.First().Description }),
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapGet("/goals/{id:guid}", async (
    Guid id,
    HttpContext http,
    IMediator mediator,
    IJwtTokenService jwtService) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(new GetGoalProgressQuery(id, userId.Value));
    return result.Match(
        success => Results.Ok(success),
        errors => Results.NotFound(new { Error = errors.First().Description }));
});

app.MapDelete("/goals/{id:guid}", async (
    Guid id,
    HttpContext http,
    IMediator mediator,
    IJwtTokenService jwtService) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(new CancelGoalCommand(id, userId.Value));
    return result.Match(
        success => Results.Ok(success),
        errors => errors.First().Type switch
        {
            ErrorType.NotFound => Results.NotFound(new { Error = errors.First().Description }),
            ErrorType.Conflict => Results.Conflict(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

//Budgets -----------------------------------------------------------
app.MapPost("/budgets", async (
    HttpContext http,
    CreateBudgetCommand command,
    IMediator mediator,
    IJwtTokenService jwtService) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();


    var secureCommand = command with { UserId = userId.Value };

    var result = await mediator.Send(secureCommand);
    return result.Match(
        success => Results.Created($"/budgets/{success.BudgetId}", success),
        errors => errors.First().Type switch
        {
            ErrorType.Conflict => Results.Conflict(new { Error = errors.First().Description }),
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});

app.MapGet("/budgets/summary", async (
    HttpContext http,
    IMediator mediator,
    IJwtTokenService jwtService,
    int month,
    int year) =>
{
    var userId = jwtService.ExtractUserId(
        http.Request.Headers.Authorization.FirstOrDefault());
    if (userId is null) return Results.Unauthorized();

    var result = await mediator.Send(
        new GetBudgetSummaryQuery(userId.Value, month, year));

    return result.Match(
        success => Results.Ok(success),
        errors => errors.First().Type switch
        {
            ErrorType.Validation => Results.BadRequest(new { Error = errors.First().Description }),
            _ => Results.Problem("Unexpected error")
        });
});
app.Run();