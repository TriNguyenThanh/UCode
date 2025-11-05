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
        
        // Map tên cột từ PascalCase trong entity sang PascalCase trong database
        builder.Property(bs => bs.BestSubmissionId).HasColumnName("BestSubmissionId");
        builder.Property(bs => bs.AssignmentUserId).HasColumnName("AssignmentUserId");
        builder.Property(bs => bs.ProblemId).HasColumnName("ProblemId");
        builder.Property(bs => bs.SubmissionId).HasColumnName("SubmissionId");
        builder.Property(bs => bs.Score).HasColumnName("Score");
        builder.Property(bs => bs.MaxScore).HasColumnName("MaxScore");
        builder.Property(bs => bs.TotalTime).HasColumnName("TotalTime");
        builder.Property(bs => bs.TotalMemory).HasColumnName("TotalMemory");
        builder.Property(bs => bs.UpdatedAt).HasColumnName("UpdatedAt");
        
        // VIEW không có relationships, indexes, hoặc constraints
        // Đây là read-only computed view
    }
}
