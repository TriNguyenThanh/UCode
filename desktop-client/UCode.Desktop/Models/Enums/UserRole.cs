namespace UCode.Desktop.Models.Enums
{
    /// <summary>
    /// Enum representing user roles in the system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Student role - can submit assignments and view results
        /// </summary>
        Student,
        
        /// <summary>
        /// Teacher role - can create classes and assignments
        /// </summary>
        Teacher,
        
        /// <summary>
        /// Admin role - full system access
        /// </summary>
        Admin
    }
}
