using Unifintech.Domain.Entities;

namespace Unifintech.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Loan> Loans { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
