using ErrorOr;

namespace FinTrack.Modules.Transactions.Errors;

public static class TransactionErrors
{
    public static Error NotFound = Error.NotFound(
        code: "Transaction.NotFound",
        description: "Transaction not found");
}