using System.Collections.Generic;
using UCode.Desktop.Models.Enums;

namespace UCode.Desktop.Models
{
    public class Dataset
    {
        public string DatasetId { get; set; } = string.Empty;
        public string ProblemId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DatasetKind Kind { get; set; }
        public List<TestCase> TestCases { get; set; } = new();
    }

    public class TestCase
    {
        public string TestCaseId { get; set; } = string.Empty;
        public string DatasetId { get; set; } = string.Empty;
        public string InputRef { get; set; } = string.Empty;
        public string OutputRef { get; set; } = string.Empty;
        public int IndexNo { get; set; }
        public double? Score { get; set; }
    }
}

