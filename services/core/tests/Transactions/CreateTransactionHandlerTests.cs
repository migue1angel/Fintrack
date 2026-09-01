

using FinTrack.Common.Contracts;
using FinTrack.Modules.Transactions.Events;
using FinTrack.Modules.Transactions.Features.Create;
using FinTrack.Tests;
using MediatR;
using NSubstitute;

namespace FinTrack.Tests.Transactions;

public class CreateTransactionHandlerTests
{

    [Fact]
    public async Task Handle_NewTransaction_CreatesTransactionAndReturnsResult()
    {
        // Arrange
        var ctx = TestHelpers.CreateContext();
        var mediator = Substitute.For<IMediator>();
        var handler = new CreateTransactionHandler(ctx, mediator);
        var transactionCommand = new CreateTransactionCommand(
                UserId: Guid.NewGuid(),
                Amount: 100.0m,
                Type: TransactionType.Income,
                Category: "Salary",
                Description: "Monthly salary",
                TransactionDate: DateOnly.FromDateTime(DateTime.UtcNow),
                IdempotencyKey: null
            );

        //Act
        var result = await handler.Handle(
            transactionCommand,
            default
        );

        //Assert
        Assert.True(result.IsSuccess);
        var transaction = await ctx.Transactions.FindAsync(result.Value.TransactionId);
        Assert.NotNull(transaction);
        Assert.Equal(transactionCommand.UserId, transaction.UserId);
        Assert.Equal(transactionCommand.Amount, transaction.Amount);
        Assert.Equal(transactionCommand.Type, transaction.Type);
        Assert.Equal(transactionCommand.Category, transaction.Category);
        Assert.Equal(transactionCommand.Description, transaction.Description);
        Assert.Equal(transactionCommand.TransactionDate, transaction.TransactionDate);
        Assert.False(result.Value.WasDuplicate);

        await mediator.Received(1).Publish(Arg.Is<TransactionCreatedEvent>(
            e => e.TransactionId == transaction.Id &&
                    e.UserId == transaction.UserId &&
                    e.Amount == transaction.Amount &&
                    e.Type == transaction.Type &&
                    e.Category == transaction.Category &&
                    e.TransactionDate == transaction.TransactionDate
        ), Arg.Any<CancellationToken>());
    }

}
