using Microsoft.EntityFrameworkCore;
using Unifintech.Domain.Entities;

namespace Unifintech.Infrastructure.Data.Configurations
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(
            Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Loan> builder
        ) { }
    }
}
