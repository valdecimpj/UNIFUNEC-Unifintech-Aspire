namespace Unifintech.Application.Common.Interfaces;

public interface ICustomerCreditService
{
    Task<decimal?> GetCustomerCreditScoreAsync(
        string customerId,
        CancellationToken cancellationToken = default
    );
}
