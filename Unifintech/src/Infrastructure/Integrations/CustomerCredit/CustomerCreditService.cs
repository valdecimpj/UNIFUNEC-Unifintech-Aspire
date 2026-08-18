using System.Net.Http.Json;
using Unifintech.Application.Common.Interfaces;
using Unifintech.Infrastructure.Integrations.CustomerCredit;

namespace Unifintech.Infrastructure.Integrations;

public class CustomerCreditService(HttpClient client) : ICustomerCreditService
{
    public async Task<decimal?> GetCustomerCreditScoreAsync(
        string customerId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await client.GetFromJsonAsync<IEnumerable<CustomerCreditDto>>(
                $"/credit-scores?cpf={customerId}",
                cancellationToken
            );

            return response?.FirstOrDefault()?.Score;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
