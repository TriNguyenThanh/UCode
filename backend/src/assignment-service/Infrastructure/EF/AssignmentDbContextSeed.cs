using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.EF;

/// <summary>
/// Class để seed data mẫu vào database
/// </summary>
public static class AssignmentDbContextSeed
{
    /// <summary>
    /// Seed initial data
    /// </summary>
    public static async Task SeedAsync(AssignmentDbContext context)
    {
        if (await context.Problems.AnyAsync())
            return; // Already seeded

        // Seed Users
        // var users = new List<User>
        // {
        //     new User
        //     {
        //         UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        //         Email = "admin@ucode.io",
        //     },
        //     new User
        //     {
        //         UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
        //         Email = "teacher1@ucode.io",
        //     }
        // };
        // await context.Users.AddRangeAsync(users);
        // await context.SaveChangesAsync();

        // Seed Tags
        var tags = new List<Tag>
        {
            new Tag { TagId = Guid.NewGuid(), Name = "Array", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "String", Category = TagCategory.DIFFICULTY },
            new Tag { TagId = Guid.NewGuid(), Name = "Dynamic Programming", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Math", Category = TagCategory.OTHER}
        };
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        // Seed Problems (đã gộp ProblemVersion vào)
        var problem1 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "HYTGDFD",
            Slug = "two-sum",
            Title = "Two Sum",
            Difficulty = Difficulty.EASY,
            OwnerId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            // Properties từ ProblemVersion
            StatementMdRef = "problems/two-sum/statement.md",
            IoMode = IoMode.STDIO,
            TimeLimitMs = 1000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            Changelog = "Initial version",
            IsLocked = false
        };

        var problem2 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P002",
            Slug = "reverse-string",
            Title = "Reverse String",
            Difficulty = Difficulty.EASY,
            OwnerId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            StatementMdRef = "problems/reverse-string/statement.md",
            IoMode = IoMode.STDIO,
            TimeLimitMs = 1000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            Changelog = "Initial version",
            IsLocked = false
        };

        await context.Problems.AddRangeAsync(problem1, problem2);
        await context.SaveChangesAsync();

        // Seed ProblemTags
        var problemTags = new List<ProblemTag>
        {
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[0].TagId }, // Two Sum - Array
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[3].TagId }, // Two Sum - Math
            new ProblemTag { ProblemId = problem2.ProblemId, TagId = tags[1].TagId }  // Reverse String - String
        };
        await context.ProblemTags.AddRangeAsync(problemTags);
        await context.SaveChangesAsync();

        // Seed Datasets (sử dụng ProblemId thay vì ProblemVersionId)
        var datasets = new List<Dataset>
        {
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId, // Đã đổi từ ProblemVersionId
                Name = "Sample",
                Kind = DatasetKind.SAMPLE
            },
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Name = "Test",
                Kind = DatasetKind.PRIVATE
            },
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Name = "Sample",
                Kind = DatasetKind.SAMPLE
            }
        };
        await context.Datasets.AddRangeAsync(datasets);
        await context.SaveChangesAsync();

        // Seed TestCases
        var testCases = new List<TestCase>
        {
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[0].DatasetId,
                InputRef = "testcases/two-sum/sample/input1.txt",
                OutputRef = "testcases/two-sum/sample/output1.txt",
                Score = "100",
            },
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[1].DatasetId,
                InputRef = "testcases/two-sum/test/input1.txt",
                OutputRef = "testcases/two-sum/test/output1.txt",
                Score = "100",
            }
        };
        await context.TestCases.AddRangeAsync(testCases);
        await context.SaveChangesAsync();

        // Seed LanguageLimits (sử dụng ProblemId)
        var languageLimits = new List<LanguageLimit>
        {
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId, // Đã đổi
                Lang = "python",
                TimeFactor = 3.0m,
                MemoryKbOverride = 524288
            },
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "java",
                TimeFactor = 2.0m,
                MemoryKbOverride = null
            }
        };
        await context.LanguageLimits.AddRangeAsync(languageLimits);
        await context.SaveChangesAsync();

        // Seed CodeTemplates (sử dụng ProblemId)
        var codeTemplates = new List<CodeTemplate>
        {
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId, // Đã đổi
                Lang = "cpp",
                StarterRef = "templates/two-sum/cpp/starter.cpp"
            },
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "python",
                StarterRef = "templates/two-sum/python/starter.py"
            }
        };
        await context.CodeTemplates.AddRangeAsync(codeTemplates);
        await context.SaveChangesAsync();

        // Seed ProblemAssets
        var problemAssets = new List<ProblemAsset>
        {
            new ProblemAsset
            {
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Type = AssetType.STATEMENT_PDF,
                ObjectRef = "assets/two-sum/explanation.pdf",
                Checksum = "abc123def456"
            }
        };
        await context.ProblemAssets.AddRangeAsync(problemAssets);
        await context.SaveChangesAsync();
    }
}