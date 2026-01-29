using Microsoft.EntityFrameworkCore;
using AssignmentService.Domain.Entities;
using AssignmentService.Domain.Enums;

namespace AssignmentService.Infrastructure.EF;

/// <summary>
/// Class để seed data mẫu vào database
/// Dữ liệu đồng bộ với UserDbContextSeed
/// </summary>
public static class AssignmentDbContextSeed
{
    // ===== IDs từ UserDbContextSeed =====
    private static readonly Guid Teacher1Id = Guid.Parse("e54be995-11d5-4bb1-8c2d-f8af59b91707"); // Nguyễn Văn Giáo
    private static readonly Guid Teacher2Id = Guid.Parse("1c009683-9544-4e67-bd72-d61982b67697"); // Trần Thị Hương
    private static readonly Guid Student1Id = Guid.Parse("57b45e9b-a0dc-4126-a850-adae3e71d411"); // Lê Văn An
    private static readonly Guid Student2Id = Guid.Parse("214b4663-97b9-42ef-8671-90e500816e88"); // Phạm Thị Bình
    private static readonly Guid Student3Id = Guid.Parse("eb63f6d4-1c8d-4cb9-a692-b3a107fee8ec"); // Hoàng Văn Cường
    private static readonly Guid Class1Id = Guid.Parse("3685709d-51e5-492c-bb2b-7bfbad2f1dcc"); // Lập trình C# nâng cao (Teacher1, Student1, Student2)
    private static readonly Guid Class2Id = Guid.Parse("00f99f84-0f4f-418f-a1d8-99f050f1e6bb"); // Cấu trúc dữ liệu và giải thuật (Teacher2, Student3)

    /// <summary>
    /// Seed initial data
    /// </summary>
    public static async Task SeedAsync(AssignmentDbContext context)
    {
        if (await context.Problems.AnyAsync())
        {
            Console.WriteLine("Database đã được seed. Bỏ qua thao tác seed.");
            return;
        }

        Console.WriteLine("Bắt đầu seed dữ liệu mẫu...\n");

        // ===== 1. Seed Tags =====
        var tags = await SeedTagsAsync(context);

        // ===== 2. Seed Languages =====
        var languages = await SeedLanguagesAsync(context);

        // ===== 3. Seed Problems =====
        var problems = await SeedProblemsAsync(context);

        // ===== 4. Seed ProblemTags =====
        await SeedProblemTagsAsync(context, problems, tags);

        // ===== 5. Seed Datasets & TestCases =====
        await SeedDatasetsAndTestCasesAsync(context, problems);

        // ===== 6. Seed ProblemLanguages (cho phép tất cả ngôn ngữ active) =====
        await SeedProblemLanguagesAsync(context, problems, languages);

        // ===== 7. Seed Assignments =====
        var assignments = await SeedAssignmentsAsync(context);

        // ===== 8. Seed AssignmentProblems =====
        await SeedAssignmentProblemsAsync(context, assignments, problems);

        // ===== 9. Seed AssignmentUsers =====
        await SeedAssignmentUsersAsync(context, assignments);

        Console.WriteLine("\n✓ Hoàn thành seed dữ liệu mẫu!");
    }

    private static async Task<List<Tag>> SeedTagsAsync(AssignmentDbContext context)
    {
        Console.WriteLine("Đang seed Tags...");
        
        var tags = new List<Tag>
        {
            // Chủ đề
            new Tag { TagId = Guid.NewGuid(), Name = "Mảng", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Chuỗi", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Quy hoạch động", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Toán học", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Sắp xếp", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Tìm kiếm nhị phân", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Cây", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Đồ thị", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Đệ quy", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Tham lam", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Vòng lặp", Category = TagCategory.TOPIC },
            new Tag { TagId = Guid.NewGuid(), Name = "Điều kiện", Category = TagCategory.TOPIC },
            // Độ khó
            new Tag { TagId = Guid.NewGuid(), Name = "Dễ", Category = TagCategory.DIFFICULTY },
            new Tag { TagId = Guid.NewGuid(), Name = "Trung bình", Category = TagCategory.DIFFICULTY },
            new Tag { TagId = Guid.NewGuid(), Name = "Khó", Category = TagCategory.DIFFICULTY }
        };

        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {tags.Count} tags");
        
        return tags;
    }

    private static async Task<List<Language>> SeedLanguagesAsync(AssignmentDbContext context)
    {
        Console.WriteLine("\nĐang seed Languages...");
        
        var languages = new List<Language>
        {
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "cpp", 
                DisplayName = "C++ 17", 
                DefaultTimeFactor = 1.0m,
                DefaultMemoryKb = 262144,
                DefaultHead = "#include <iostream>\n#include <vector>\n#include <algorithm>\n#include <string>\nusing namespace std;\n",
                DefaultBody = "int main() {\n    // Viết code của bạn ở đây\n    return 0;\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "python", 
                DisplayName = "Python 3.11", 
                DefaultTimeFactor = 2.5m,
                DefaultMemoryKb = 262144,
                DefaultHead = "import sys\nimport math\nfrom typing import List, Optional\n",
                DefaultBody = "def main():\n    # Viết code của bạn ở đây\n    pass\n\nif __name__ == '__main__':\n    main()",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 2,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "java", 
                DisplayName = "Java 17", 
                DefaultTimeFactor = 1.5m,
                DefaultMemoryKb = 524288,
                DefaultHead = "import java.util.*;\nimport java.io.*;\n",
                DefaultBody = "public class Main {\n    public static void main(String[] args) {\n        Scanner sc = new Scanner(System.in);\n        // Viết code của bạn ở đây\n    }\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 3,
                CreatedAt = DateTime.UtcNow
            },
            new Language 
            { 
                LanguageId = Guid.NewGuid(), 
                Code = "csharp", 
                DisplayName = "C# .NET 8", 
                DefaultTimeFactor = 1.2m,
                DefaultMemoryKb = 262144,
                DefaultHead = "using System;\nusing System.Collections.Generic;\nusing System.Linq;\n",
                DefaultBody = "class Program\n{\n    static void Main(string[] args)\n    {\n        // Viết code của bạn ở đây\n    }\n}",
                DefaultTail = null,
                IsEnabled = true,
                DisplayOrder = 4,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Languages.AddRangeAsync(languages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {languages.Count} ngôn ngữ lập trình");
        
        return languages;
    }

    private static async Task<List<Problem>> SeedProblemsAsync(AssignmentDbContext context)
    {
        Console.WriteLine("\nĐang seed Problems...");
        
        var problems = new List<Problem>
        {
            // ===== Bài 1: Tính tổng hai số (Dễ - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT001",
                Slug = "tinh-tong-hai-so",
                Title = "Tính tổng hai số",
                Description = "Cho hai số nguyên a và b. Hãy tính và in ra tổng của hai số đó.",
                Difficulty = Difficulty.EASY,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho hai số nguyên a và b. Hãy tính và in ra tổng của hai số đó.\n\n## Ví dụ\n- Đầu vào: 3 5\n- Đầu ra: 8",
                IoMode = IoMode.STDIO,
                InputFormat = "Một dòng chứa hai số nguyên a và b cách nhau bởi dấu cách (−10^9 ≤ a, b ≤ 10^9)",
                OutputFormat = "Một số nguyên duy nhất là tổng của a và b",
                Constraints = "−10^9 ≤ a, b ≤ 10^9",
                MaxScore = 100,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "3 5",
                SampleOutput = "8",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 2: Kiểm tra số chẵn lẻ (Dễ - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT002",
                Slug = "kiem-tra-so-chan-le",
                Title = "Kiểm tra số chẵn lẻ",
                Description = "Cho một số nguyên n. Hãy kiểm tra xem n là số chẵn hay số lẻ.",
                Difficulty = Difficulty.EASY,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một số nguyên n. Hãy kiểm tra xem n là số chẵn hay số lẻ.\n\nNếu n là số chẵn, in ra \"CHAN\". Ngược lại in ra \"LE\".",
                IoMode = IoMode.STDIO,
                InputFormat = "Một số nguyên n (−10^9 ≤ n ≤ 10^9)",
                OutputFormat = "In ra \"CHAN\" nếu n là số chẵn, \"LE\" nếu n là số lẻ",
                Constraints = "−10^9 ≤ n ≤ 10^9",
                MaxScore = 100,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "4",
                SampleOutput = "CHAN",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 3: Đảo ngược chuỗi (Dễ - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT003",
                Slug = "dao-nguoc-chuoi",
                Title = "Đảo ngược chuỗi",
                Description = "Cho một chuỗi ký tự s. Hãy in ra chuỗi đảo ngược của s.",
                Difficulty = Difficulty.EASY,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một chuỗi ký tự s chỉ chứa các ký tự chữ cái Latin và chữ số. Hãy in ra chuỗi đảo ngược của s.",
                IoMode = IoMode.STDIO,
                InputFormat = "Một dòng chứa chuỗi s (1 ≤ |s| ≤ 10^5)",
                OutputFormat = "Chuỗi đảo ngược của s",
                Constraints = "1 ≤ |s| ≤ 10^5\nChuỗi chỉ chứa chữ cái Latin và chữ số",
                MaxScore = 100,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "xinchaovietnam",
                SampleOutput = "manteivoacnix",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 4: Tìm số lớn nhất trong mảng (Dễ - Teacher2) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT004",
                Slug = "tim-so-lon-nhat-mang",
                Title = "Tìm số lớn nhất trong mảng",
                Description = "Cho một mảng gồm n số nguyên. Hãy tìm và in ra số lớn nhất trong mảng.",
                Difficulty = Difficulty.EASY,
                OwnerId = Teacher2Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một mảng gồm n số nguyên a[1], a[2], ..., a[n]. Hãy tìm và in ra số lớn nhất trong mảng.",
                IoMode = IoMode.STDIO,
                InputFormat = "Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 10^5)\nDòng thứ hai chứa n số nguyên a[i] cách nhau bởi dấu cách",
                OutputFormat = "Một số nguyên duy nhất là giá trị lớn nhất trong mảng",
                Constraints = "1 ≤ n ≤ 10^5\n−10^9 ≤ a[i] ≤ 10^9",
                MaxScore = 100,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "5\n3 1 4 1 5",
                SampleOutput = "5",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 5: Tính giai thừa (Trung bình - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT005",
                Slug = "tinh-giai-thua",
                Title = "Tính giai thừa",
                Description = "Cho số nguyên dương n. Hãy tính n! (n giai thừa).",
                Difficulty = Difficulty.MEDIUM,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho số nguyên dương n. Hãy tính n! = 1 × 2 × 3 × ... × n.\n\nLưu ý: 0! = 1",
                IoMode = IoMode.STDIO,
                InputFormat = "Một số nguyên n (0 ≤ n ≤ 20)",
                OutputFormat = "Giá trị của n!",
                Constraints = "0 ≤ n ≤ 20",
                MaxScore = 150,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "5",
                SampleOutput = "120",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 6: Dãy Fibonacci (Trung bình - Teacher2) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT006",
                Slug = "day-fibonacci",
                Title = "Dãy Fibonacci",
                Description = "Cho số nguyên n. Hãy tính số Fibonacci thứ n.",
                Difficulty = Difficulty.MEDIUM,
                OwnerId = Teacher2Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Dãy Fibonacci được định nghĩa như sau:\n- F(0) = 0\n- F(1) = 1\n- F(n) = F(n-1) + F(n-2) với n ≥ 2\n\nCho số nguyên n, hãy tính F(n).",
                IoMode = IoMode.STDIO,
                InputFormat = "Một số nguyên n (0 ≤ n ≤ 45)",
                OutputFormat = "Số Fibonacci thứ n",
                Constraints = "0 ≤ n ≤ 45",
                MaxScore = 150,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "10",
                SampleOutput = "55",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 7: Sắp xếp mảng tăng dần (Trung bình - Teacher2) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT007",
                Slug = "sap-xep-mang-tang-dan",
                Title = "Sắp xếp mảng tăng dần",
                Description = "Cho một mảng gồm n số nguyên. Hãy sắp xếp mảng theo thứ tự tăng dần.",
                Difficulty = Difficulty.MEDIUM,
                OwnerId = Teacher2Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một mảng gồm n số nguyên a[1], a[2], ..., a[n]. Hãy sắp xếp và in ra mảng theo thứ tự tăng dần.",
                IoMode = IoMode.STDIO,
                InputFormat = "Dòng đầu tiên chứa số nguyên n (1 ≤ n ≤ 10^5)\nDòng thứ hai chứa n số nguyên a[i] cách nhau bởi dấu cách",
                OutputFormat = "n số nguyên đã được sắp xếp tăng dần, cách nhau bởi dấu cách",
                Constraints = "1 ≤ n ≤ 10^5\n−10^9 ≤ a[i] ≤ 10^9",
                MaxScore = 150,
                TimeLimitMs = 2000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "5\n3 1 4 1 5",
                SampleOutput = "1 1 3 4 5",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 8: Tìm kiếm nhị phân (Trung bình - Teacher2) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT008",
                Slug = "tim-kiem-nhi-phan",
                Title = "Tìm kiếm nhị phân",
                Description = "Cho một mảng đã sắp xếp tăng dần và một giá trị x. Hãy tìm vị trí của x trong mảng.",
                Difficulty = Difficulty.MEDIUM,
                OwnerId = Teacher2Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một mảng gồm n số nguyên đã được sắp xếp tăng dần và một giá trị x. Hãy tìm và in ra vị trí (chỉ số bắt đầu từ 0) của x trong mảng. Nếu không tìm thấy, in ra -1.",
                IoMode = IoMode.STDIO,
                InputFormat = "Dòng đầu tiên chứa hai số nguyên n và x\nDòng thứ hai chứa n số nguyên đã sắp xếp tăng dần",
                OutputFormat = "Vị trí của x trong mảng (0-indexed) hoặc -1 nếu không tìm thấy",
                Constraints = "1 ≤ n ≤ 10^5\n−10^9 ≤ a[i], x ≤ 10^9",
                MaxScore = 150,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "6 9\n-1 0 3 5 9 12",
                SampleOutput = "4",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 9: Kiểm tra số nguyên tố (Trung bình - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT009",
                Slug = "kiem-tra-so-nguyen-to",
                Title = "Kiểm tra số nguyên tố",
                Description = "Cho số nguyên dương n. Hãy kiểm tra xem n có phải là số nguyên tố hay không.",
                Difficulty = Difficulty.MEDIUM,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Số nguyên tố là số tự nhiên lớn hơn 1, chỉ chia hết cho 1 và chính nó.\n\nCho số nguyên dương n, hãy kiểm tra xem n có phải là số nguyên tố hay không.",
                IoMode = IoMode.STDIO,
                InputFormat = "Một số nguyên n (1 ≤ n ≤ 10^9)",
                OutputFormat = "In ra \"YES\" nếu n là số nguyên tố, \"NO\" nếu ngược lại",
                Constraints = "1 ≤ n ≤ 10^9",
                MaxScore = 150,
                TimeLimitMs = 1000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "17",
                SampleOutput = "YES",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },

            // ===== Bài 10: Đếm số lần xuất hiện (Khó - Teacher1) =====
            new Problem
            {
                ProblemId = Guid.NewGuid(),
                Code = "BT010",
                Slug = "dem-so-lan-xuat-hien",
                Title = "Đếm số lần xuất hiện",
                Description = "Cho một mảng n số nguyên và q truy vấn. Mỗi truy vấn cho một số x, hãy đếm số lần x xuất hiện trong mảng.",
                Difficulty = Difficulty.HARD,
                OwnerId = Teacher1Id,
                Visibility = Visibility.PUBLIC,
                Status = ProblemStatus.PUBLISHED,
                Statement = "Cho một mảng gồm n số nguyên và q truy vấn. Với mỗi truy vấn cho một số x, hãy đếm và in ra số lần x xuất hiện trong mảng.",
                IoMode = IoMode.STDIO,
                InputFormat = "Dòng đầu tiên chứa hai số nguyên n và q\nDòng thứ hai chứa n số nguyên của mảng\nq dòng tiếp theo, mỗi dòng chứa một số x",
                OutputFormat = "Với mỗi truy vấn, in ra số lần xuất hiện của x trên một dòng",
                Constraints = "1 ≤ n, q ≤ 10^5\n−10^9 ≤ a[i], x ≤ 10^9",
                MaxScore = 200,
                TimeLimitMs = 2000,
                MemoryLimitKb = 262144,
                SourceLimitKb = 65536,
                StackLimitKb = 8192,
                SampleInput = "7 3\n1 2 3 2 1 2 4\n2\n1\n5",
                SampleOutput = "3\n2\n0",
                Changelog = "Phiên bản đầu tiên",
                IsLocked = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Problems.AddRangeAsync(problems);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {problems.Count} bài toán");
        
        return problems;
    }

    private static async Task SeedProblemTagsAsync(AssignmentDbContext context, List<Problem> problems, List<Tag> tags)
    {
        Console.WriteLine("\nĐang seed ProblemTags...");
        
        // Lấy tag theo tên để dễ sử dụng
        var tagDict = tags.ToDictionary(t => t.Name, t => t.TagId);
        
        var problemTags = new List<ProblemTag>
        {
            // BT001 - Tính tổng hai số (Toán học, Dễ)
            new ProblemTag { ProblemId = problems[0].ProblemId, TagId = tagDict["Toán học"] },
            new ProblemTag { ProblemId = problems[0].ProblemId, TagId = tagDict["Dễ"] },
            
            // BT002 - Kiểm tra số chẵn lẻ (Điều kiện, Toán học, Dễ)
            new ProblemTag { ProblemId = problems[1].ProblemId, TagId = tagDict["Điều kiện"] },
            new ProblemTag { ProblemId = problems[1].ProblemId, TagId = tagDict["Toán học"] },
            new ProblemTag { ProblemId = problems[1].ProblemId, TagId = tagDict["Dễ"] },
            
            // BT003 - Đảo ngược chuỗi (Chuỗi, Dễ)
            new ProblemTag { ProblemId = problems[2].ProblemId, TagId = tagDict["Chuỗi"] },
            new ProblemTag { ProblemId = problems[2].ProblemId, TagId = tagDict["Dễ"] },
            
            // BT004 - Tìm số lớn nhất trong mảng (Mảng, Vòng lặp, Dễ)
            new ProblemTag { ProblemId = problems[3].ProblemId, TagId = tagDict["Mảng"] },
            new ProblemTag { ProblemId = problems[3].ProblemId, TagId = tagDict["Vòng lặp"] },
            new ProblemTag { ProblemId = problems[3].ProblemId, TagId = tagDict["Dễ"] },
            
            // BT005 - Tính giai thừa (Toán học, Vòng lặp, Trung bình)
            new ProblemTag { ProblemId = problems[4].ProblemId, TagId = tagDict["Toán học"] },
            new ProblemTag { ProblemId = problems[4].ProblemId, TagId = tagDict["Vòng lặp"] },
            new ProblemTag { ProblemId = problems[4].ProblemId, TagId = tagDict["Trung bình"] },
            
            // BT006 - Dãy Fibonacci (Quy hoạch động, Đệ quy, Trung bình)
            new ProblemTag { ProblemId = problems[5].ProblemId, TagId = tagDict["Quy hoạch động"] },
            new ProblemTag { ProblemId = problems[5].ProblemId, TagId = tagDict["Đệ quy"] },
            new ProblemTag { ProblemId = problems[5].ProblemId, TagId = tagDict["Trung bình"] },
            
            // BT007 - Sắp xếp mảng tăng dần (Mảng, Sắp xếp, Trung bình)
            new ProblemTag { ProblemId = problems[6].ProblemId, TagId = tagDict["Mảng"] },
            new ProblemTag { ProblemId = problems[6].ProblemId, TagId = tagDict["Sắp xếp"] },
            new ProblemTag { ProblemId = problems[6].ProblemId, TagId = tagDict["Trung bình"] },
            
            // BT008 - Tìm kiếm nhị phân (Mảng, Tìm kiếm nhị phân, Trung bình)
            new ProblemTag { ProblemId = problems[7].ProblemId, TagId = tagDict["Mảng"] },
            new ProblemTag { ProblemId = problems[7].ProblemId, TagId = tagDict["Tìm kiếm nhị phân"] },
            new ProblemTag { ProblemId = problems[7].ProblemId, TagId = tagDict["Trung bình"] },
            
            // BT009 - Kiểm tra số nguyên tố (Toán học, Vòng lặp, Trung bình)
            new ProblemTag { ProblemId = problems[8].ProblemId, TagId = tagDict["Toán học"] },
            new ProblemTag { ProblemId = problems[8].ProblemId, TagId = tagDict["Vòng lặp"] },
            new ProblemTag { ProblemId = problems[8].ProblemId, TagId = tagDict["Trung bình"] },
            
            // BT010 - Đếm số lần xuất hiện (Mảng, Khó)
            new ProblemTag { ProblemId = problems[9].ProblemId, TagId = tagDict["Mảng"] },
            new ProblemTag { ProblemId = problems[9].ProblemId, TagId = tagDict["Khó"] }
        };

        await context.ProblemTags.AddRangeAsync(problemTags);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {problemTags.Count} liên kết problem-tag");
    }

    private static async Task SeedDatasetsAndTestCasesAsync(AssignmentDbContext context, List<Problem> problems)
    {
        Console.WriteLine("\nĐang seed Datasets và TestCases...");
        
        var allDatasets = new List<Dataset>();
        var allTestCases = new List<TestCase>();

        foreach (var problem in problems)
        {
            // Mỗi problem có 2 dataset: Sample và Official
            var sampleDataset = new Dataset 
            { 
                DatasetId = Guid.NewGuid(), 
                ProblemId = problem.ProblemId, 
                Name = "Ví dụ mẫu", 
                Kind = DatasetKind.SAMPLE 
            };
            
            var officialDataset = new Dataset 
            { 
                DatasetId = Guid.NewGuid(), 
                ProblemId = problem.ProblemId, 
                Name = "Test chính thức", 
                Kind = DatasetKind.OFFICIAL 
            };
            
            allDatasets.Add(sampleDataset);
            allDatasets.Add(officialDataset);

            // Tạo test cases dựa trên từng bài
            var testCases = CreateTestCasesForProblem(problem, sampleDataset.DatasetId, officialDataset.DatasetId);
            allTestCases.AddRange(testCases);
        }

        await context.Datasets.AddRangeAsync(allDatasets);
        await context.TestCases.AddRangeAsync(allTestCases);
        await context.SaveChangesAsync();
        
        Console.WriteLine($"✓ Đã seed {allDatasets.Count} datasets và {allTestCases.Count} test cases");
    }

    private static List<TestCase> CreateTestCasesForProblem(Problem problem, Guid sampleDatasetId, Guid officialDatasetId)
    {
        var testCases = new List<TestCase>();
        
        switch (problem.Code)
        {
            case "BT001": // Tính tổng hai số
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "3 5", OutputRef = "8", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "0 0", OutputRef = "0", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "-5 10", OutputRef = "5", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "100 200", OutputRef = "300", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "-100 -200", OutputRef = "-300", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "1000000000 1000000000", OutputRef = "2000000000", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "-1000000000 1000000000", OutputRef = "0", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 6, InputRef = "123456789 987654321", OutputRef = "1111111110", Score = "20" }
                });
                break;
                
            case "BT002": // Kiểm tra số chẵn lẻ
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "4", OutputRef = "CHAN", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "0", OutputRef = "CHAN", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "1", OutputRef = "LE", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "-2", OutputRef = "CHAN", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "-7", OutputRef = "LE", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "1000000000", OutputRef = "CHAN", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "999999999", OutputRef = "LE", Score = "25" }
                });
                break;
                
            case "BT003": // Đảo ngược chuỗi
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "xinchaovietnam", OutputRef = "manteivoacnix", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "a", OutputRef = "a", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "ab", OutputRef = "ba", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "hello", OutputRef = "olleh", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "12345", OutputRef = "54321", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "abcdefghijklmnopqrstuvwxyz", OutputRef = "zyxwvutsrqponmlkjihgfedcba", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "A1B2C3D4E5", OutputRef = "5E4D3C2B1A", Score = "25" }
                });
                break;
                
            case "BT004": // Tìm số lớn nhất trong mảng
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "5\n3 1 4 1 5", OutputRef = "5", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "1\n42", OutputRef = "42", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "3\n-1 -2 -3", OutputRef = "-1", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "5\n5 4 3 2 1", OutputRef = "5", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "5\n1 2 3 4 5", OutputRef = "5", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "7\n-100 0 50 -50 100 25 75", OutputRef = "100", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "6\n1000000000 -1000000000 0 999999999 -999999999 1", OutputRef = "1000000000", Score = "25" }
                });
                break;
                
            case "BT005": // Tính giai thừa
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "5", OutputRef = "120", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "0", OutputRef = "1", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "1", OutputRef = "1", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "3", OutputRef = "6", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "10", OutputRef = "3628800", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "15", OutputRef = "1307674368000", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "20", OutputRef = "2432902008176640000", Score = "25" }
                });
                break;
                
            case "BT006": // Dãy Fibonacci
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "10", OutputRef = "55", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "0", OutputRef = "0", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "1", OutputRef = "1", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "2", OutputRef = "1", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "15", OutputRef = "610", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "30", OutputRef = "832040", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "45", OutputRef = "1134903170", Score = "30" }
                });
                break;
                
            case "BT007": // Sắp xếp mảng tăng dần
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "5\n3 1 4 1 5", OutputRef = "1 1 3 4 5", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "1\n5", OutputRef = "5", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "3\n3 2 1", OutputRef = "1 2 3", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "5\n1 2 3 4 5", OutputRef = "1 2 3 4 5", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "5\n-5 -3 -1 -4 -2", OutputRef = "-5 -4 -3 -2 -1", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "7\n0 -1 1 -2 2 -3 3", OutputRef = "-3 -2 -1 0 1 2 3", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "6\n1000000 -1000000 500000 -500000 0 1", OutputRef = "-1000000 -500000 0 1 500000 1000000", Score = "25" }
                });
                break;
                
            case "BT008": // Tìm kiếm nhị phân
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "6 9\n-1 0 3 5 9 12", OutputRef = "4", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "1 5\n5", OutputRef = "0", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "5 1\n1 2 3 4 5", OutputRef = "0", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "5 5\n1 2 3 4 5", OutputRef = "4", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "5 6\n1 2 3 4 5", OutputRef = "-1", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "7 0\n-10 -5 0 5 10 15 20", OutputRef = "2", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "8 100\n-1000 -100 -10 0 10 100 1000 10000", OutputRef = "5", Score = "30" }
                });
                break;
                
            case "BT009": // Kiểm tra số nguyên tố
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "17", OutputRef = "YES", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "1", OutputRef = "NO", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "2", OutputRef = "YES", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "4", OutputRef = "NO", Score = "10" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "97", OutputRef = "YES", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "100", OutputRef = "NO", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 5, InputRef = "999999937", OutputRef = "YES", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 6, InputRef = "1000000000", OutputRef = "NO", Score = "20" }
                });
                break;
                
            case "BT010": // Đếm số lần xuất hiện
                testCases.AddRange(new[]
                {
                    // Sample
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = sampleDatasetId, IndexNo = 0, InputRef = "7 3\n1 2 3 2 1 2 4\n2\n1\n5", OutputRef = "3\n2\n0", Score = "100" },
                    // Official
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 0, InputRef = "5 2\n1 1 1 1 1\n1\n2", OutputRef = "5\n0", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 1, InputRef = "6 3\n1 2 3 4 5 6\n3\n7\n1", OutputRef = "1\n0\n1", Score = "15" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 2, InputRef = "8 4\n5 5 5 3 3 1 1 1\n5\n3\n1\n2", OutputRef = "3\n2\n3\n0", Score = "20" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 3, InputRef = "10 3\n-1 0 1 -1 0 1 -1 0 1 0\n0\n-1\n1", OutputRef = "4\n3\n3", Score = "25" },
                    new TestCase { TestCaseId = Guid.NewGuid(), DatasetId = officialDatasetId, IndexNo = 4, InputRef = "5 5\n1000000000 -1000000000 0 1000000000 -1000000000\n1000000000\n-1000000000\n0\n1\n999999999", OutputRef = "2\n2\n1\n0\n0", Score = "25" }
                });
                break;
        }
        
        return testCases;
    }

    private static async Task SeedProblemLanguagesAsync(AssignmentDbContext context, List<Problem> problems, List<Language> languages)
    {
        Console.WriteLine("\nĐang seed ProblemLanguages...");
        
        var problemLanguages = new List<ProblemLanguage>();
        
        // Mỗi problem cho phép TẤT CẢ ngôn ngữ đang active
        foreach (var problem in problems)
        {
            foreach (var language in languages.Where(l => l.IsEnabled))
            {
                problemLanguages.Add(new ProblemLanguage
                {
                    ProblemId = problem.ProblemId,
                    LanguageId = language.LanguageId,
                    IsAllowed = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.ProblemLanguages.AddRangeAsync(problemLanguages);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {problemLanguages.Count} liên kết problem-language");
    }

    private static async Task<List<Assignment>> SeedAssignmentsAsync(AssignmentDbContext context)
    {
        Console.WriteLine("\nĐang seed Assignments...");
        
        var assignments = new List<Assignment>
        {
            // ===== Lớp 1: Lập trình C# nâng cao (Teacher1) =====
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.HOMEWORK,
                ClassId = Class1Id,
                Title = "Bài tập tuần 1 - Cơ bản",
                Description = "Ôn tập các kiến thức cơ bản về lập trình: nhập xuất, điều kiện, vòng lặp. Hoàn thành trước thứ 7 tuần này.",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(7),
                AssignedBy = Teacher1Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 300,
                AllowLateSubmission = true,
                Status = AssignmentStatus.PUBLISHED
            },
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.HOMEWORK,
                ClassId = Class1Id,
                Title = "Bài tập tuần 2 - Mảng và chuỗi",
                Description = "Thực hành các bài toán về mảng một chiều và xử lý chuỗi ký tự.",
                StartTime = DateTime.UtcNow.AddDays(7),
                EndTime = DateTime.UtcNow.AddDays(14),
                AssignedBy = Teacher1Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 400,
                AllowLateSubmission = true,
                Status = AssignmentStatus.PUBLISHED
            },
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.PRACTICE,
                ClassId = Class1Id,
                Title = "Luyện tập - Giải thuật cơ bản",
                Description = "Các bài luyện tập về đệ quy và quy hoạch động. Không có thời hạn - làm bất cứ lúc nào.",
                StartTime = DateTime.UtcNow,
                EndTime = null,
                AssignedBy = Teacher1Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 300,
                AllowLateSubmission = true,
                Status = AssignmentStatus.PUBLISHED
            },
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.EXAMINATION,
                ClassId = Class1Id,
                Title = "Kiểm tra giữa kỳ",
                Description = "Bài kiểm tra giữa kỳ - 90 phút. Không được sử dụng tài liệu.",
                StartTime = DateTime.UtcNow.AddDays(30),
                EndTime = DateTime.UtcNow.AddDays(30).AddMinutes(90),
                AssignedBy = Teacher1Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = null,
                TotalPoints = 500,
                AllowLateSubmission = false,
                Status = AssignmentStatus.DRAFT
            },

            // ===== Lớp 2: Cấu trúc dữ liệu và giải thuật (Teacher2) =====
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.HOMEWORK,
                ClassId = Class2Id,
                Title = "Bài tập chương 1 - Mảng và tìm kiếm",
                Description = "Thực hành các bài toán cơ bản về mảng và thuật toán tìm kiếm.",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(10),
                AssignedBy = Teacher2Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 350,
                AllowLateSubmission = true,
                Status = AssignmentStatus.PUBLISHED
            },
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.HOMEWORK,
                ClassId = Class2Id,
                Title = "Bài tập chương 2 - Sắp xếp",
                Description = "Thực hành các thuật toán sắp xếp: Bubble Sort, Selection Sort, Insertion Sort, Quick Sort.",
                StartTime = DateTime.UtcNow.AddDays(10),
                EndTime = DateTime.UtcNow.AddDays(20),
                AssignedBy = Teacher2Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 300,
                AllowLateSubmission = false,
                Status = AssignmentStatus.PUBLISHED
            },
            new Assignment
            {
                AssignmentId = Guid.NewGuid(),
                AssignmentType = AssignmentType.PRACTICE,
                ClassId = Class2Id,
                Title = "Luyện tập tự do",
                Description = "Kho bài tập luyện tập cho sinh viên tự rèn luyện kỹ năng.",
                StartTime = DateTime.UtcNow,
                EndTime = null,
                AssignedBy = Teacher2Id,
                CreatedAt = DateTime.UtcNow,
                AssignedAt = DateTime.UtcNow,
                TotalPoints = 500,
                AllowLateSubmission = true,
                Status = AssignmentStatus.PUBLISHED
            }
        };

        await context.Assignments.AddRangeAsync(assignments);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {assignments.Count} bài tập (assignments)");
        
        return assignments;
    }

    private static async Task SeedAssignmentProblemsAsync(AssignmentDbContext context, List<Assignment> assignments, List<Problem> problems)
    {
        Console.WriteLine("\nĐang seed AssignmentProblems...");
        
        var assignmentProblems = new List<AssignmentProblem>
        {
            // ===== Lớp 1 - Bài tập tuần 1 (BT001, BT002, BT003) =====
            new AssignmentProblem { AssignmentId = assignments[0].AssignmentId, ProblemId = problems[0].ProblemId, Points = 100, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[0].AssignmentId, ProblemId = problems[1].ProblemId, Points = 100, OrderIndex = 2 },
            new AssignmentProblem { AssignmentId = assignments[0].AssignmentId, ProblemId = problems[2].ProblemId, Points = 100, OrderIndex = 3 },
            
            // ===== Lớp 1 - Bài tập tuần 2 (BT003, BT004, BT005, BT009) =====
            new AssignmentProblem { AssignmentId = assignments[1].AssignmentId, ProblemId = problems[2].ProblemId, Points = 100, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[1].AssignmentId, ProblemId = problems[3].ProblemId, Points = 100, OrderIndex = 2 },
            new AssignmentProblem { AssignmentId = assignments[1].AssignmentId, ProblemId = problems[4].ProblemId, Points = 100, OrderIndex = 3 },
            new AssignmentProblem { AssignmentId = assignments[1].AssignmentId, ProblemId = problems[8].ProblemId, Points = 100, OrderIndex = 4 },
            
            // ===== Lớp 1 - Luyện tập (BT005, BT006) =====
            new AssignmentProblem { AssignmentId = assignments[2].AssignmentId, ProblemId = problems[4].ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[2].AssignmentId, ProblemId = problems[5].ProblemId, Points = 150, OrderIndex = 2 },
            
            // ===== Lớp 1 - Kiểm tra giữa kỳ (BT008, BT009, BT010) =====
            new AssignmentProblem { AssignmentId = assignments[3].AssignmentId, ProblemId = problems[7].ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[3].AssignmentId, ProblemId = problems[8].ProblemId, Points = 150, OrderIndex = 2 },
            new AssignmentProblem { AssignmentId = assignments[3].AssignmentId, ProblemId = problems[9].ProblemId, Points = 200, OrderIndex = 3 },
            
            // ===== Lớp 2 - Bài tập chương 1 (BT004, BT008) =====
            new AssignmentProblem { AssignmentId = assignments[4].AssignmentId, ProblemId = problems[3].ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[4].AssignmentId, ProblemId = problems[7].ProblemId, Points = 200, OrderIndex = 2 },
            
            // ===== Lớp 2 - Bài tập chương 2 (BT007, BT010) =====
            new AssignmentProblem { AssignmentId = assignments[5].AssignmentId, ProblemId = problems[6].ProblemId, Points = 150, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[5].AssignmentId, ProblemId = problems[9].ProblemId, Points = 150, OrderIndex = 2 },
            
            // ===== Lớp 2 - Luyện tập tự do (BT001, BT002, BT005, BT006, BT009) =====
            new AssignmentProblem { AssignmentId = assignments[6].AssignmentId, ProblemId = problems[0].ProblemId, Points = 100, OrderIndex = 1 },
            new AssignmentProblem { AssignmentId = assignments[6].AssignmentId, ProblemId = problems[1].ProblemId, Points = 100, OrderIndex = 2 },
            new AssignmentProblem { AssignmentId = assignments[6].AssignmentId, ProblemId = problems[4].ProblemId, Points = 100, OrderIndex = 3 },
            new AssignmentProblem { AssignmentId = assignments[6].AssignmentId, ProblemId = problems[5].ProblemId, Points = 100, OrderIndex = 4 },
            new AssignmentProblem { AssignmentId = assignments[6].AssignmentId, ProblemId = problems[8].ProblemId, Points = 100, OrderIndex = 5 }
        };

        await context.AssignmentProblems.AddRangeAsync(assignmentProblems);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {assignmentProblems.Count} liên kết assignment-problem");
    }

    private static async Task SeedAssignmentUsersAsync(AssignmentDbContext context, List<Assignment> assignments)
    {
        Console.WriteLine("\nĐang seed AssignmentUsers...");
        
        // Lớp 1: Student1 (Lê Văn An), Student2 (Phạm Thị Bình)
        // Lớp 2: Student3 (Hoàng Văn Cường)
        
        var assignmentUsers = new List<AssignmentUser>
        {
            // ===== Lớp 1 - Bài tập tuần 1 =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[0].AssignmentId,
                UserId = Student1Id,
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddHours(1),
                Score = 200,
                MaxScore = 300,
                IsActive = true
            },
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[0].AssignmentId,
                UserId = Student2Id,
                Status = AssignmentUserStatus.GRADED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddHours(2),
                Score = 300,
                MaxScore = 300,
                IsActive = true
            },
            
            // ===== Lớp 1 - Bài tập tuần 2 =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[1].AssignmentId,
                UserId = Student1Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = 0,
                MaxScore = 400,
                IsActive = true
            },
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[1].AssignmentId,
                UserId = Student2Id,
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddDays(7).AddHours(3),
                Score = 150,
                MaxScore = 400,
                IsActive = true
            },
            
            // ===== Lớp 1 - Luyện tập =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[2].AssignmentId,
                UserId = Student1Id,
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddDays(1),
                Score = 150,
                MaxScore = 300,
                IsActive = true
            },
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[2].AssignmentId,
                UserId = Student2Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = 0,
                MaxScore = 300,
                IsActive = true
            },
            
            // ===== Lớp 2 - Bài tập chương 1 =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[4].AssignmentId,
                UserId = Student3Id,
                Status = AssignmentUserStatus.IN_PROGRESS,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddHours(5),
                Score = 150,
                MaxScore = 350,
                IsActive = true
            },
            
            // ===== Lớp 2 - Bài tập chương 2 =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[5].AssignmentId,
                UserId = Student3Id,
                Status = AssignmentUserStatus.NOT_STARTED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = null,
                Score = 0,
                MaxScore = 300,
                IsActive = true
            },
            
            // ===== Lớp 2 - Luyện tập tự do =====
            new AssignmentUser
            {
                AssignmentUserId = Guid.NewGuid(),
                AssignmentId = assignments[6].AssignmentId,
                UserId = Student3Id,
                Status = AssignmentUserStatus.GRADED,
                AssignedAt = DateTime.UtcNow,
                StartedAt = DateTime.UtcNow.AddDays(1),
                Score = 400,
                MaxScore = 500,
                IsActive = true
            }
        };

        await context.AssignmentUsers.AddRangeAsync(assignmentUsers);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Đã seed {assignmentUsers.Count} liên kết assignment-user");
    }
}
