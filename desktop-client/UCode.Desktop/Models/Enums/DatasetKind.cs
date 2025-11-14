namespace UCode.Desktop.Models.Enums
{
    /// <summary>
    /// Loại dataset (nhóm test cases)
    /// SAMPLE: Test mẫu (hiển thị cho student)
    /// PUBLIC: Test công khai
    /// PRIVATE: Test riêng (không hiển thị)
    /// OFFICIAL: Test chính thức để chấm điểm
    /// </summary>
    public enum DatasetKind
    {
        SAMPLE,
        PUBLIC,
        PRIVATE,
        OFFICIAL
    }
}

