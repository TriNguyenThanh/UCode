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

        Console.WriteLine("Starting database seeding...");

        // ===== 1. Seed Tags =====
        Console.WriteLine("Seeding Tags...");
        var tags = new List<Tag>
        {
            new Tag { TagId = Guid.NewGuid(), Name = "Array", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "String", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Dynamic Programming", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Math", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Sorting", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Binary Search", Category = TagCategory.TOPIC },
        };
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {tags.Count} tags");

        // ===== 2. Seed Problems =====
        Console.WriteLine("Seeding Problems...");
        var teacherId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        
        var problem1 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P001",
            Slug = "two-sum",
            Title = "Two Sum",
            Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.",
            Difficulty = Difficulty.EASY,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            StatementMdRef = "problems/two-sum/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "First line: n (number of elements)\nSecond line: n integers\nThird line: target",
            OutputFormat = "Two space-separated integers (indices)",
            Constraints = "2 <= n <= 10^4\n-10^9 <= nums[i] <= 10^9\n-10^9 <= target <= 10^9",
            MaxScore = 100,
            TimeLimitMs = 1000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            SampleInput = "4\n2 7 11 15\n9",
            SampleOutput = "0 1",
            Changelog = "Initial version",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var problem2 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P002",
            Slug = "reverse-string",
            Title = "Reverse String",
            Description = "Write a function that reverses a string. The input string is given as an array of characters.",
            Difficulty = Difficulty.EASY,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            StatementMdRef = "problems/reverse-string/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "A single line containing a string",
            OutputFormat = "The reversed string",
            Constraints = "1 <= s.length <= 10^5\ns consists of printable ASCII characters",
            MaxScore = 100,
            TimeLimitMs = 1000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            SampleInput = "hello",
            SampleOutput = "olleh",
            Changelog = "Initial version",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var problem3 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P003",
            Slug = "fibonacci-number",
            Title = "Fibonacci Number",
            Description = "Calculate the Nth Fibonacci number using recursion or dynamic programming.",
            Difficulty = Difficulty.MEDIUM,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            StatementMdRef = "problems/fibonacci/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "A single integer N",
            OutputFormat = "The Nth Fibonacci number",
            Constraints = "0 <= N <= 30",
            MaxScore = 150,
            TimeLimitMs = 2000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            SampleInput = "10",
            SampleOutput = "55",
            Changelog = "Initial version",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Problems.AddRangeAsync(problem1, problem2, problem3);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded 3 problems");

        // ===== 3. Seed ProblemTags =====
        Console.WriteLine("Seeding ProblemTags...");
        var problemTags = new List<ProblemTag>
        {
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[0].TagId }, // Two Sum - Array
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[3].TagId }, // Two Sum - Math
            new ProblemTag { ProblemId = problem2.ProblemId, TagId = tags[1].TagId }, // Reverse String - String
            new ProblemTag { ProblemId = problem3.ProblemId, TagId = tags[2].TagId }, // Fibonacci - DP
            new ProblemTag { ProblemId = problem3.ProblemId, TagId = tags[3].TagId }, // Fibonacci - Math
        };
        await context.ProblemTags.AddRangeAsync(problemTags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {problemTags.Count} problem tags");

        // ===== 4. Seed Datasets =====
        Console.WriteLine("Seeding Datasets...");
        var datasets = new List<Dataset>
        {
            // Problem 1 - Two Sum
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Name = "Sample",
                Kind = DatasetKind.SAMPLE
            },
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Name = "Test Cases",
                Kind = DatasetKind.PRIVATE
            },
            // Problem 2 - Reverse String
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Name = "Sample",
                Kind = DatasetKind.SAMPLE
            },
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Name = "Test Cases",
                Kind = DatasetKind.PRIVATE
            },
            // Problem 3 - Fibonacci
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem3.ProblemId,
                Name = "Sample",
                Kind = DatasetKind.SAMPLE
            },
            new Dataset
            {
                DatasetId = Guid.NewGuid(),
                ProblemId = problem3.ProblemId,
                Name = "Test Cases",
                Kind = DatasetKind.PRIVATE
            },
        };
        await context.Datasets.AddRangeAsync(datasets);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {datasets.Count} datasets");

        // ===== 5. Seed TestCases =====
        Console.WriteLine("Seeding TestCases...");
        var testCases = new List<TestCase>
        {
            // Problem 1 - Two Sum - Sample
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[0].DatasetId,
                IndexNo = 0,
                InputRef = "testcases/two-sum/sample/input1.txt",
                OutputRef = "testcases/two-sum/sample/output1.txt",
                Score = "50"
            },
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[0].DatasetId,
                IndexNo = 1,
                InputRef = "testcases/two-sum/sample/input2.txt",
                OutputRef = "testcases/two-sum/sample/output2.txt",
                Score = "50"
            },
            // Problem 1 - Two Sum - Private
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[1].DatasetId,
                IndexNo = 0,
                InputRef = "testcases/two-sum/test/input1.txt",
                OutputRef = "testcases/two-sum/test/output1.txt",
                Score = "33"
            },
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[1].DatasetId,
                IndexNo = 1,
                InputRef = "testcases/two-sum/test/input2.txt",
                OutputRef = "testcases/two-sum/test/output2.txt",
                Score = "34"
            },
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[1].DatasetId,
                IndexNo = 2,
                InputRef = "testcases/two-sum/test/input3.txt",
                OutputRef = "testcases/two-sum/test/output3.txt",
                Score = "33"
            },
            // Problem 2 - Reverse String - Sample
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[2].DatasetId,
                IndexNo = 0,
                InputRef = "testcases/reverse-string/sample/input1.txt",
                OutputRef = "testcases/reverse-string/sample/output1.txt",
                Score = "100"
            },
            // Problem 3 - Fibonacci - Sample
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[4].DatasetId,
                IndexNo = 0,
                InputRef = "testcases/fibonacci/sample/input1.txt",
                OutputRef = "testcases/fibonacci/sample/output1.txt",
                Score = "50"
            },
            new TestCase
            {
                TestCaseId = Guid.NewGuid(),
                DatasetId = datasets[4].DatasetId,
                IndexNo = 1,
                InputRef = "testcases/fibonacci/sample/input2.txt",
                OutputRef = "testcases/fibonacci/sample/output2.txt",
                Score = "50"
            },
        };
        await context.TestCases.AddRangeAsync(testCases);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {testCases.Count} test cases");

        // ===== 6. Seed LanguageLimits =====
        Console.WriteLine("Seeding LanguageLimits...");
        var languageLimits = new List<LanguageLimit>
        {
            // Problem 1
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
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
            },
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "cpp",
                TimeFactor = 1.0m,
                MemoryKbOverride = null
            },
            // Problem 2
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Lang = "python",
                TimeFactor = 3.0m,
                MemoryKbOverride = 524288
            },
            // Problem 3
            new LanguageLimit
            {
                LanguageLimitId = Guid.NewGuid(),
                ProblemId = problem3.ProblemId,
                Lang = "cpp",
                TimeFactor = 1.0m,
                MemoryKbOverride = null
            },
        };
        await context.LanguageLimits.AddRangeAsync(languageLimits);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {languageLimits.Count} language limits");

        // ===== 7. Seed CodeTemplates =====
        Console.WriteLine("Seeding CodeTemplates...");
        var codeTemplates = new List<CodeTemplate>
        {
            // Problem 1 - Two Sum
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "cpp",
                StarterRef = "templates/two-sum/cpp/starter.cpp"
            },
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "python",
                StarterRef = "templates/two-sum/python/starter.py"
            },
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Lang = "java",
                StarterRef = "templates/two-sum/java/starter.java"
            },
            // Problem 2 - Reverse String
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Lang = "cpp",
                StarterRef = "templates/reverse-string/cpp/starter.cpp"
            },
            new CodeTemplate
            {
                CodeTemplateId = Guid.NewGuid(),
                ProblemId = problem2.ProblemId,
                Lang = "python",
                StarterRef = "templates/reverse-string/python/starter.py"
            },
        };
        await context.CodeTemplates.AddRangeAsync(codeTemplates);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {codeTemplates.Count} code templates");

        // ===== 8. Seed ProblemAssets =====
        Console.WriteLine("Seeding ProblemAssets...");
        var problemAssets = new List<ProblemAsset>
        {
            new ProblemAsset
            {
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Type = AssetType.STATEMENT_PDF,
                ObjectRef = "assets/two-sum/statement.pdf",
                Checksum = "abc123def456"
            },
            new ProblemAsset
            {
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Type = AssetType.IMAGE,
                ObjectRef = "assets/two-sum/diagram.png",
                Checksum = "img123abc456"
            },
        };
        await context.ProblemAssets.AddRangeAsync(problemAssets);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {problemAssets.Count} problem assets");

        // ===== 9. Seed Assignments =====
        Console.WriteLine("Seeding Assignments...");
        var classId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        var assignment1 = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            AssignmentType = AssignmentType.HOMEWORK,
            ClassId = classId,
            Title = "Homework Week 1 - Basic Problems",
            Description = "Practice basic programming problems including arrays and strings",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(7),
            AssignedBy = teacherId,
            CreatedAt = DateTime.UtcNow,
            AssignedAt = DateTime.UtcNow,
            TotalPoints = 200,
            AllowLateSubmission = true,
            Status = AssignmentStatus.PUBLISHED
        };

        var assignment2 = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            AssignmentType = AssignmentType.PRACTICE,
            ClassId = classId,
            Title = "Practice Session - Algorithms",
            Description = "Practice session for algorithm problems",
            StartTime = DateTime.UtcNow,
            EndTime = null, // No deadline
            AssignedBy = teacherId,
            CreatedAt = DateTime.UtcNow,
            AssignedAt = DateTime.UtcNow,
            TotalPoints = 150,
            AllowLateSubmission = true,
            Status = AssignmentStatus.PUBLISHED
        };

        await context.Assignments.AddRangeAsync(assignment1, assignment2);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded 2 assignments");

        // ===== 10. Seed AssignmentProblems =====
        Console.WriteLine("Seeding AssignmentProblems...");
        var assignmentProblems = new List<AssignmentProblem>
        {
            // Assignment 1
            new AssignmentProblem
            {
                AssignmentId = assignment1.AssignmentId,
                ProblemId = problem1.ProblemId,
                Points = 100,
                OrderIndex = 1
            },
            new AssignmentProblem
            {
                AssignmentId = assignment1.AssignmentId,
                ProblemId = problem2.ProblemId,
                Points = 100,
                OrderIndex = 2
            },
            // Assignment 2
            new AssignmentProblem
            {
                AssignmentId = assignment2.AssignmentId,
                ProblemId = problem3.ProblemId,
                Points = 150,
                OrderIndex = 1
            },
        };
        await context.AssignmentProblems.AddRangeAsync(assignmentProblems);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {assignmentProblems.Count} assignment problems");

        // ===== 11. Seed AssignmentUsers =====
        Console.WriteLine("Seeding AssignmentUsers...");
        var student1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var student2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        
        var assignmentUsers = new List<AssignmentUser>
        {
            // Assignment 1 - Student 1
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment1.AssignmentId,
                UserId = student1Id,
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddHours(1),
                Score = null,
                MaxScore = 200
            },
            // Assignment 1 - Student 2
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment1.AssignmentId,
                UserId = student2Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = null,
                MaxScore = 200
            },
            // Assignment 2 - Student 1
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment2.AssignmentId,
                UserId = student1Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = null,
                MaxScore = 150
            },
        };
        await context.AssignmentUsers.AddRangeAsync(assignmentUsers);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {assignmentUsers.Count} assignment users");

        // ===== 12. Seed Submissions =====
        Console.WriteLine("Seeding Submissions...");
        var submissions = new List<Submission>
        {
            // Student 1 - Problem 1 - Attempt 1
            new Submission
            {
                SubmissionId = Guid.NewGuid(),
                UserId = student1Id,
                AssignmentId = assignment1.AssignmentId,
                ProblemId = problem1.ProblemId,
                DatasetId = datasets[1].DatasetId, // Private test cases
                SourceCodeRef = "submissions/student1/two-sum/attempt1.cpp",
                Language = "cpp",
                CompareResult = "AC",
                Status = "ACCEPTED",
                ErrorCode = null,
                ErrorMessage = null,
                TotalTestcase = 5,
                PassedTestcase = 5,
                TotalTime = 125,
                TotalMemory = 2048,
                SubmittedAt = DateTime.UtcNow.AddHours(-2),
                ResultFileId = null
            },
            // Student 1 - Problem 1 - Attempt 2 (Better)
            new Submission
            {
                SubmissionId = Guid.NewGuid(),
                UserId = student1Id,
                AssignmentId = assignment1.AssignmentId,
                ProblemId = problem1.ProblemId,
                DatasetId = datasets[1].DatasetId,
                SourceCodeRef = "submissions/student1/two-sum/attempt2.cpp",
                Language = "cpp",
                CompareResult = "AC",
                Status = "ACCEPTED",
                ErrorCode = null,
                ErrorMessage = null,
                TotalTestcase = 5,
                PassedTestcase = 5,
                TotalTime = 98, // Faster
                TotalMemory = 1920, // Less memory
                SubmittedAt = DateTime.UtcNow.AddHours(-1),
                ResultFileId = null
            },
            // Student 1 - Problem 2
            new Submission
            {
                SubmissionId = Guid.NewGuid(),
                UserId = student1Id,
                AssignmentId = assignment1.AssignmentId,
                ProblemId = problem2.ProblemId,
                DatasetId = datasets[3].DatasetId,
                SourceCodeRef = "submissions/student1/reverse-string/attempt1.py",
                Language = "python",
                CompareResult = "WA",
                Status = "WRONG_ANSWER",
                ErrorCode = null,
                ErrorMessage = null,
                TotalTestcase = 3,
                PassedTestcase = 2,
                TotalTime = 250,
                TotalMemory = 3072,
                SubmittedAt = DateTime.UtcNow.AddMinutes(-30),
                ResultFileId = null
            },
        };
        await context.Submissions.AddRangeAsync(submissions);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {submissions.Count} submissions");

        // ===== 13. Seed BestSubmissions =====
        Console.WriteLine("Seeding BestSubmissions...");
        var bestSubmissions = new List<BestSubmission>
        {
            // Student 1 - Assignment 1 - Problem 1 (Best = Attempt 2)
            new BestSubmission
            {
                BestSubmissionId = Guid.NewGuid(),
                AssignmentUserId = assignmentUsers[0].AssignmentUserId,
                ProblemId = problem1.ProblemId,
                SubmissionId = submissions[1].SubmissionId, // Attempt 2 is better
                Score = 100,
                MaxScore = 100,
                TotalTime = 98,
                TotalMemory = 1920,
                UpdatedAt = DateTime.UtcNow.AddHours(-1)
            },
            // Student 1 - Assignment 1 - Problem 2 (Only one attempt, not perfect)
            new BestSubmission
            {
                BestSubmissionId = Guid.NewGuid(),
                AssignmentUserId = assignmentUsers[0].AssignmentUserId,
                ProblemId = problem2.ProblemId,
                SubmissionId = submissions[2].SubmissionId,
                Score = 67, // 2/3 test cases passed
                MaxScore = 100,
                TotalTime = 250,
                TotalMemory = 3072,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
            },
        };
        await context.BestSubmissions.AddRangeAsync(bestSubmissions);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {bestSubmissions.Count} best submissions");

        Console.WriteLine("\n✅ Database seeding completed successfully!");
        Console.WriteLine($"Summary:");
        Console.WriteLine($"  - {tags.Count} Tags");
        Console.WriteLine($"  - 3 Problems");
        Console.WriteLine($"  - {problemTags.Count} ProblemTags");
        Console.WriteLine($"  - {datasets.Count} Datasets");
        Console.WriteLine($"  - {testCases.Count} TestCases");
        Console.WriteLine($"  - {languageLimits.Count} LanguageLimits");
        Console.WriteLine($"  - {codeTemplates.Count} CodeTemplates");
        Console.WriteLine($"  - {problemAssets.Count} ProblemAssets");
        Console.WriteLine($"  - 2 Assignments");
        Console.WriteLine($"  - {assignmentProblems.Count} AssignmentProblems");
        Console.WriteLine($"  - {assignmentUsers.Count} AssignmentUsers");
        Console.WriteLine($"  - {submissions.Count} Submissions");
        Console.WriteLine($"  - {bestSubmissions.Count} BestSubmissions");
    }
}