using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using Microsoft.Extensions.Logging;
using Shared.SharedKernel;

namespace DirectoryService.Infrastructure.Database;

public class TransactionScope : ITransactionScope
{
    private readonly IDbTransaction _transaction;
    private readonly ILogger<TransactionScope> _logger;

    public TransactionScope(IDbTransaction transaction, ILogger<TransactionScope> logger)
    {
        _transaction = transaction;
        _logger = logger;
    }

    public UnitResult<Error> Commit()
    {
        try
        {
            _transaction.Commit();
            return UnitResult.Success<Error>();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return GeneralErrors.DataBase("transaction.commit");
        }
    }

    public UnitResult<Error> Rollback()
    {
        try
        {
            _transaction.Rollback();
            return UnitResult.Success<Error>();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return GeneralErrors.DataBase("transaction.rollback");
        }
    }

    public void Dispose() => _transaction.Dispose();
}