namespace PayRexApplication.Enums
{
    /// <summary>
    /// Unified roles for all users (SuperAdmin at platform level, Admin/HR/Employee at company level)
    /// </summary>
  public enum UserRole
    {
      SuperAdmin = 0,
        Admin = 1,
        Hr = 2,
        Employee = 3
    }
}