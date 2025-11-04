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
        {
            Console.WriteLine("Database already seeded. Skipping seed operation.");
            return; // Already seeded
        }

        Console.WriteLine("Starting database seeding...\n");

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
            new Tag { TagId = Guid.NewGuid(), Name = "Tree", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Graph", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Recursion", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Greedy", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Easy", Category = TagCategory.DIFFICULTY },
            new Tag { TagId = Guid.NewGuid(), Name = "Medium", Category = TagCategory.DIFFICULTY },
            new Tag { TagId = Guid.NewGuid(), Name = "Hard", Category = TagCategory.DIFFICULTY }
        };

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {tags.Count} tags");

        // ===== 2. Seed Problems =====
        Console.WriteLine("\nSeeding Problems...");
        var teacherId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var problem1 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P001",
            Slug = "two-sum",
            Title = "Two Sum",
            Description = "Given an array of integers nums and an integer target, return indices of the two numbers such that they add up to target.\n\nYou may assume that each input would have exactly one solution, and you may not use the same element twice.\n\nYou can return the answer in any order.",
            Difficulty = Difficulty.EASY,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            Statement = "problems/two-sum/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "First line: n (number of elements)\nSecond line: n space-separated integers\nThird line: target value",
            OutputFormat = "Two space-separated integers representing the indices",
            Constraints = "2 <= n <= 10^4\n-10^9 <= nums[i] <= 10^9\n-10^9 <= target <= 10^9\nOnly one valid answer exists",
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
            Description = "Write a function that reverses a string. The input string is given as an array of characters.\n\nYou must do this by modifying the input array in-place with O(1) extra memory.",
            Difficulty = Difficulty.EASY,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            Statement = "problems/reverse-string/statement.md",
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
            Description = "The Fibonacci numbers, commonly denoted F(n) form a sequence, called the Fibonacci sequence, such that each number is the sum of the two preceding ones, starting from 0 and 1.\n\nGiven n, calculate F(n).",
            Difficulty = Difficulty.MEDIUM,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            Statement = "problems/fibonacci/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "A single integer n",
            OutputFormat = "The nth Fibonacci number",
            Constraints = "0 <= n <= 30",
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

        var problem4 = new Problem
        {
            ProblemId = Guid.NewGuid(),
            Code = "P004",
            Slug = "binary-search",
            Title = "Binary Search",
            Description = "Given an array of integers nums which is sorted in ascending order, and an integer target, write a function to search target in nums. If target exists, then return its index. Otherwise, return -1.\n\nYou must write an algorithm with O(log n) runtime complexity.",
            Difficulty = Difficulty.EASY,
            OwnerId = teacherId,
            Visibility = Visibility.PUBLIC,
            Status = ProblemStatus.PUBLISHED,
            Statement = "problems/binary-search/statement.md",
            IoMode = IoMode.STDIO,
            InputFormat = "First line: n (number of elements)\nSecond line: n space-separated integers (sorted)\nThird line: target value",
            OutputFormat = "A single integer representing the index of target (-1 if not found)",
            Constraints = "1 <= n <= 10^4\n-10^4 < nums[i], target < 10^4\nAll integers in nums are unique\nnums is sorted in ascending order",
            MaxScore = 100,
            TimeLimitMs = 1000,
            MemoryLimitKb = 262144,
            SourceLimitKb = 65536,
            StackLimitKb = 8192,
            SampleInput = "6\n-1 0 3 5 9 12\n9",
            SampleOutput = "4",
            Changelog = "Initial version",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Problems.AddRangeAsync(problem1, problem2, problem3, problem4);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded 4 problems");

        // ===== 3. Seed ProblemTags (Many-to-Many) =====
        Console.WriteLine("\nSeeding ProblemTags...");
        var problemTags = new List<ProblemTag>
        {
            // Problem 1 - Two Sum (Array, Math)
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[0].TagId },
            new ProblemTag { ProblemId = problem1.ProblemId, TagId = tags[3].TagId },
            
            // Problem 2 - Reverse String (String)
            new ProblemTag { ProblemId = problem2.ProblemId, TagId = tags[1].TagId },
            
            // Problem 3 - Fibonacci (Dynamic Programming, Math, Recursion)
            new ProblemTag { ProblemId = problem3.ProblemId, TagId = tags[2].TagId },
            new ProblemTag { ProblemId = problem3.ProblemId, TagId = tags[3].TagId },
            new ProblemTag { ProblemId = problem3.ProblemId, TagId = tags[8].TagId },
            
            // Problem 4 - Binary Search (Array, Binary Search)
            new ProblemTag { ProblemId = problem4.ProblemId, TagId = tags[0].TagId },
            new ProblemTag { ProblemId = problem4.ProblemId, TagId = tags[5].TagId }
        };

        await context.ProblemTags.AddRangeAsync(problemTags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {problemTags.Count} problem tags");

        // ===== 4. Seed Datasets =====
        Console.WriteLine("\nSeeding Datasets...");
        var datasets = new List<Dataset>
        {
            // Problem 1 - Two Sum
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem1.ProblemId, Name = "Sample", Kind = DatasetKind.SAMPLE },
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem1.ProblemId, Name = "Basic Tests", Kind = DatasetKind.PUBLIC },
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem1.ProblemId, Name = "Advanced Tests", Kind = DatasetKind.PRIVATE },
            
            // Problem 2 - Reverse String
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem2.ProblemId, Name = "Sample", Kind = DatasetKind.SAMPLE },
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem2.ProblemId, Name = "Basic Tests", Kind = DatasetKind.PUBLIC },
            
            // Problem 3 - Fibonacci
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem3.ProblemId, Name = "Sample", Kind = DatasetKind.SAMPLE },
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem3.ProblemId, Name = "Official Tests", Kind = DatasetKind.OFFICIAL },
            
            // Problem 4 - Binary Search
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem4.ProblemId, Name = "Sample", Kind = DatasetKind.SAMPLE },
            new Dataset { DatasetId = Guid.NewGuid(), ProblemId = problem4.ProblemId, Name = "Full Tests", Kind = DatasetKind.OFFICIAL }
        };

        await context.Datasets.AddRangeAsync(datasets);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {datasets.Count} datasets");

        // ===== 5. Seed TestCases =====
        Console.WriteLine("\nSeeding TestCases...");
        var testCases = new List<TestCase>
        {
            // Two Sum - Sample Dataset
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[0].DatasetId, IndexNo = 0, InputRef = "4\n2 7 11 15\n9", OutputRef = "0 1", Score = "100" },
            
            // Two Sum - Basic Tests
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[1].DatasetId, IndexNo = 0, InputRef = "3\n1 2 3\n4", OutputRef = "0 2", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[1].DatasetId, IndexNo = 1, InputRef = "5\n5 10 15 20 25\n35", OutputRef = "1 3", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[1].DatasetId, IndexNo = 2, InputRef = "2\n-1 -2\n-3", OutputRef = "0 1", Score = "20" },
            
            // Two Sum - Advanced Tests
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[2].DatasetId, IndexNo = 0, InputRef = "6\n1 5 7 9 11 13\n20", OutputRef = "2 4", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[2].DatasetId, IndexNo = 1, InputRef = "4\n-5 -2 3 8\n6", OutputRef = "1 3", Score = "20" },
            
            // Reverse String - Sample
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[3].DatasetId, IndexNo = 0, InputRef = "hello", OutputRef = "olleh", Score = "100" },
            
            // Reverse String - Basic Tests
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[4].DatasetId, IndexNo = 0, InputRef = "world", OutputRef = "dlrow", Score = "33" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[4].DatasetId, IndexNo = 1, InputRef = "a", OutputRef = "a", Score = "33" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[4].DatasetId, IndexNo = 2, InputRef = "programming", OutputRef = "gnimmargorp", Score = "34" },
            
            // Fibonacci - Sample
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[5].DatasetId, IndexNo = 0, InputRef = "10", OutputRef = "55", Score = "100" },
            
            // Fibonacci - Official Tests
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[6].DatasetId, IndexNo = 0, InputRef = "0", OutputRef = "0", Score = "25" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[6].DatasetId, IndexNo = 1, InputRef = "1", OutputRef = "1", Score = "25" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[6].DatasetId, IndexNo = 2, InputRef = "15", OutputRef = "610", Score = "25" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[6].DatasetId, IndexNo = 3, InputRef = "20", OutputRef = "6765", Score = "25" },
            
            // Binary Search - Sample
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[7].DatasetId, IndexNo = 0, InputRef = "6\n-1 0 3 5 9 12\n9", OutputRef = "4", Score = "100" },
            
            // Binary Search - Full Tests
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[8].DatasetId, IndexNo = 0, InputRef = "5\n1 2 3 4 5\n3", OutputRef = "2", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[8].DatasetId, IndexNo = 1, InputRef = "6\n-1 0 3 5 9 12\n2", OutputRef = "-1", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[8].DatasetId, IndexNo = 2, InputRef = "1\n5\n5", OutputRef = "0", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[8].DatasetId, IndexNo = 3, InputRef = "10\n1 3 5 7 9 11 13 15 17 19\n13", OutputRef = "6", Score = "20" },
            new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = datasets[8].DatasetId, IndexNo = 4, InputRef = "8\n-10 -5 0 2 4 6 8 10\n-5", OutputRef = "1", Score = "20" }
        };

        await context.TestCases.AddRangeAsync(testCases);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {testCases.Count} test cases");

        // ===== 6. Seed Languages (Global Configurations) =====
        Console.WriteLine("\nSeeding Languages...");
        var languages = new List<Language>
        {
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "cpp", 
                DisplayName = "C++17", 
                DefaultTimeFactor = 1.0m,
                DefaultMemoryKb = 262144, // 256MB default
                DefaultHead = "#include <iostream>\n#include <vector>\n#include <algorithm>\nusing namespace std;\n",
                DefaultBody = "int main() {\n    // Your code here\n    return 0;\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "java", 
                DisplayName = "Java 17", 
                DefaultTimeFactor = 1.5m,
                DefaultMemoryKb = 524288, // 512MB - Java cần nhiều memory hơn
                DefaultHead = "import java.util.*;\nimport java.io.*;\n",
                DefaultBody = "public class Main {\n    public static void main(String[] args) {\n        // Your code here\n    }\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "python", 
                DisplayName = "Python 3.11", 
                DefaultTimeFactor = 2.5m, // Python chậm hơn C++
                DefaultMemoryKb = 262144, // 256MB
                DefaultHead = "import sys\nimport math\nfrom typing import *\n",
                DefaultBody = "def main():\n    # Your code here\n    pass\n\nif __name__ == '__main__':\n    main()",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "javascript", 
                DisplayName = "Node.js 20", 
                DefaultTimeFactor = 2.0m,
                DefaultMemoryKb = 262144, // 256MB
                DefaultHead = "const readline = require('readline');\nconst rl = readline.createInterface({\n    input: process.stdin,\n    output: process.stdout\n});\n",
                DefaultBody = "// Your code here\n",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "csharp", 
                DisplayName = "C# .NET 8", 
                DefaultTimeFactor = 1.2m,
                DefaultMemoryKb = 262144, // 256MB
                DefaultHead = "using System;\nusing System.Collections.Generic;\nusing System.Linq;\n",
                DefaultBody = "class Program\n{\n    static void Main(string[] args)\n    {\n        // Your code here\n    }\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 5,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Languages.AddRangeAsync(languages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {languages.Count} languages");

        // ===== 7. Seed ProblemLanguages (Problem-specific overrides - Optional) =====
        // CHỈ seed nếu cần override global config
        // Ví dụ: Problem 1 cần Python chạy nhanh hơn default
        Console.WriteLine("\nSeeding ProblemLanguages (overrides)...");
        var cppLang = languages.First(l => l.Code == "cpp");
        var javaLang = languages.First(l => l.Code == "java");
        var pythonLang = languages.First(l => l.Code == "python");
        
        var problemLanguages = new List<ProblemLanguage>
        {
            // Problem 1 - Two Sum: Allow all languages (no overrides needed, sẽ dùng default)
            // Chỉ cần specify nếu muốn customize
            
            // Problem 2 - Reverse String: Only allow C++ and Python
            new ProblemLanguage 
            { 
                ProblemId = problem2.ProblemId, 
                LanguageId = cppLang.LanguageId,
                IsAllowed = true // Explicit allow
            },
            new ProblemLanguage 
            { 
                ProblemId = problem2.ProblemId, 
                LanguageId = pythonLang.LanguageId,
                IsAllowed = true
            },
            
            // Problem 3 - Fibonacci: Java needs more memory for this specific problem
            new ProblemLanguage 
            { 
                ProblemId = problem3.ProblemId, 
                LanguageId = javaLang.LanguageId,
                MemoryKbOverride = 1048576, // 1GB for Fibonacci (override default 512MB)
                IsAllowed = true
            }
        };

        await context.ProblemLanguages.AddRangeAsync(problemLanguages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {problemLanguages.Count} problem language overrides");

        // ===== 8. Seed ProblemAssets =====
        Console.WriteLine("\nSeeding ProblemAssets...");
        var problemAssets = new List<ProblemAsset>
        {
            // Problem 1 - Two Sum
            new ProblemAsset 
            { 
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId,
                Type = AssetType.STATEMENT, 
                ObjectRef = "assets/two-sum/statement.pdf",
                Checksum = "abc123def456",
                Title = "Two Sum Problem Statement",
                Format = ContentFormat.MARKDOWN,
                OrderIndex = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProblemAsset
            { 
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem1.ProblemId, 
                Type = AssetType.IMAGE,
                ObjectRef = "assets/two-sum/diagram.png", 
                Checksum = "img123abc456",
                Title = "Array Visualization",
                Format = ContentFormat.BINARY,
                OrderIndex = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            
            // Problem 3 - Fibonacci
            new ProblemAsset
            { 
                ProblemAssetId = Guid.NewGuid(),
                ProblemId = problem3.ProblemId, 
                Type = AssetType.STATEMENT,
                ObjectRef = "assets/fibonacci/statement.pdf", 
                Checksum = "fib789xyz012",
                Title = "Fibonacci Problem Statement",
                Format = ContentFormat.MARKDOWN,
                OrderIndex = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ProblemAsset 
            {
                ProblemAssetId = Guid.NewGuid(), 
                ProblemId = problem3.ProblemId,
                Type = AssetType.TUTORIAL, 
                ObjectRef = "assets/fibonacci/explanation.pdf",
                Checksum = "fibexp456def",
                Title = "Dynamic Programming Approach",
                Format = ContentFormat.BINARY,
                OrderIndex = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.ProblemAssets.AddRangeAsync(problemAssets);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {problemAssets.Count} problem assets");

        // ===== 9. Seed Assignments =====
        Console.WriteLine("\nSeeding Assignments...");
        var classId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        
        var assignment1 = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            AssignmentType = AssignmentType.HOMEWORK,
            ClassId = classId,
            Title = "Homework Week 1 - Basic Problems",
            Description = "Practice basic programming problems including arrays and strings. Complete all problems before the deadline.",
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
            Description = "Practice session for algorithm problems. No deadline - practice at your own pace.",
            StartTime = DateTime.UtcNow,
            EndTime = null, // No deadline for practice
            AssignedBy = teacherId,
            CreatedAt = DateTime.UtcNow,
            AssignedAt = DateTime.UtcNow,
            TotalPoints = 250,
            AllowLateSubmission = true,
            Status = AssignmentStatus.PUBLISHED
        };

        var assignment3 = new Assignment
        {
            AssignmentId = Guid.NewGuid(),
            AssignmentType = AssignmentType.EXAMINATION,
            ClassId = classId,
            Title = "Mid-Term Exam",
            Description = "Mid-term programming exam. 2 hours to solve 2 problems. Good luck!",
            StartTime = DateTime.UtcNow.AddDays(14),
            EndTime = DateTime.UtcNow.AddDays(14).AddHours(2),
            AssignedBy = teacherId,
            CreatedAt = DateTime.UtcNow,
            AssignedAt = null, // Not assigned yet
            TotalPoints = 300,
            AllowLateSubmission = false,
            Status = AssignmentStatus.DRAFT
        };

        await context.Assignments.AddRangeAsync(assignment1, assignment2, assignment3);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded 3 assignments");

        // ===== 10. Seed AssignmentProblems (Many-to-Many) =====
        Console.WriteLine("\nSeeding AssignmentProblems...");
        var assignmentProblems = new List<AssignmentProblem>
        {
            // Assignment 1 - Homework (2 problems)
            new AssignmentProblem { AssignmentId = assignment1.AssignmentId, ProblemId = problem1.ProblemId, Points = 100, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignment1.AssignmentId, ProblemId = problem2.ProblemId, Points = 100, OrderIndex = 2 },
            
            // Assignment 2 - Practice (2 problems)
            new AssignmentProblem { AssignmentId = assignment2.AssignmentId, ProblemId = problem3.ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignment2.AssignmentId, ProblemId = problem4.ProblemId, Points = 100, OrderIndex = 2 },
            
            // Assignment 3 - Contest (2 problems)
            new AssignmentProblem { AssignmentId = assignment3.AssignmentId, ProblemId = problem3.ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignment3.AssignmentId, ProblemId = problem4.ProblemId, Points = 150, OrderIndex = 2 }
        };

        await context.AssignmentProblems.AddRangeAsync(assignmentProblems);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {assignmentProblems.Count} assignment problems");

        // ===== 11. Seed AssignmentUsers =====
        Console.WriteLine("\nSeeding AssignmentUsers...");
        var student1Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var student2Id = Guid.Parse("55555555-5555-5555-5555-555555555555");
        var student3Id = Guid.Parse("66666666-6666-6666-6666-666666666666");

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
                Score = 150,
                MaxScore = 200
            },
            
            // Assignment 1 - Student 2
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment1.AssignmentId,
                UserId = student2Id,
                Status = AssignmentUserStatus.SUBMITTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddHours(2),
                Score = 200,
                MaxScore = 200
            },
            
            // Assignment 1 - Student 3
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment1.AssignmentId,
                UserId = student3Id,
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
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddMinutes(30),
                Score = null,
                MaxScore = 250
            },
            
            // Assignment 2 - Student 2
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignment2.AssignmentId,
                UserId = student2Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = null,
                MaxScore = 250
            }
        };

        await context.AssignmentUsers.AddRangeAsync(assignmentUsers);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Seeded {assignmentUsers.Count} assignment users");
    }
}