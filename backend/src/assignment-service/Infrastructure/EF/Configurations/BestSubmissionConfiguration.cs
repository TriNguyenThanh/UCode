using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AssignmentService.Domain.Entities;

namespace AssignmentService.Infrastructure.EF.Configurations;

public class BestSubmissionConfiguration : IEntityTypeConfiguration<BestSubmission>
{
    public void Configure(EntityTypeBuilder<BestSubmission> builder)
    {
        // Map tới VIEW thay vì TABLE
        builder.ToView("BestSubmissions");
        
        // Đánh dấu là keyless vì NEWID() trong view tạo ID mới mỗi lần query
        builder.HasNoKey();
        
        // Properties (không cần IsRequired cho view)
        builder.Property(bs => bs.AssignmentUserId);
        builder.Property(bs => bs.ProblemId);
        builder.Property(bs => bs.SubmissionId);
        builder.Property(bs => bs.Score);
        builder.Property(bs => bs.MaxScore);
        builder.Property(bs => bs.TotalTime);
        builder.Property(bs => bs.TotalMemory);
        builder.Property(bs => bs.UpdatedAt);
        
        // VIEW không có relationships, indexes, hoặc constraints
        // Đây là read-only computed view
    }
}
