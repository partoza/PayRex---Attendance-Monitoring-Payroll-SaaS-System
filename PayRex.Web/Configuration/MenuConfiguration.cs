namespace PayRex.Web.Configuration
{
    using PayRex.Web.Models;

    /// <summary>
    /// Static configuration for application menu items with role-based access
    /// </summary>
    public static class MenuConfiguration
    {
        /// <summary>
        /// Gets the main navigation menu items for the application
        /// </summary>
        /// <returns>The <see cref="List{MenuItem}"/></returns>
        public static List<MenuItem> GetMenuItems()
        {
            return new List<MenuItem>
            {
                // ===== Dashboards (role-specific) =====
                new MenuItem
                {
                    Title = "Admin Dashboard",
                    Url = "/Admin/Dashboard",
                    Icon = @"<svg class=""w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },
                new MenuItem
                {
                    Title = "Dashboard",
                    Url = "/Dashboard",
                    Icon = @"<svg class=""w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M10.707 2.293a1 1 0 00-1.414 0l-7 7a1 1 0 001.414 1.414L4 10.414V17a1 1 0 001 1h2a1 1 0 001-1v-2a1 1 0 011-1h2a1 1 0 011 1v2a1 1 0 001 1h2a1 1 0 001-1v-6.586l.293.293a1 1 0 001.414-1.414l-7-7z"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR", "Employee" }
                },

                // ===== SuperAdmin Section =====
                new MenuItem
                {
                    Title = "Manage Companies",
                    Url = "/Admin/Companies",
                    SectionHeader = "Platform Management",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M4 4a2 2 0 012-2h8a2 2 0 012 2v12a1 1 0 01-1 1H5a1 1 0 01-1-1V4zm3 1h6v4H7V5zm8 8v2h1v-2h-1zm-2-2v2h1v-2h-1zm2 0v2h1v-2h-1zm-2-2v2h1v-2h-1zm2 0v2h1v-2h-1zm-2-2v2h1v-2h-1zm-6 0v2h1v-2H7zm2 0v2h1v-2H9zm-2 2v2h1v-2H7zm2 0v2h1v-2H9zm-2 2v2h1v-2H7zm2 0v2h1v-2H9z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },
                new MenuItem
                {
                    Title = "Manage Users",
                    Url = "/Admin/Users",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },
                new MenuItem
                {
                    Title = "Admin Billing",
                    Url = "/Admin/Billing",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2h-6a2 2 0 01-2-2V4z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },
                new MenuItem
                {
                    Title = "System Settings",
                    Url = "/Admin/Settings",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M11.49 3.17c-.38-1.56-2.6-1.56-2.98 0a1.532 1.532 0 01-2.286.948c-1.372-.836-2.942.734-2.106 2.106.54.886.061 2.042-.947 2.287-1.561.379-1.561 2.6 0 2.978a1.532 1.532 0 01.947 2.287c-.836 1.372.734 2.942 2.106 2.106a1.532 1.532 0 012.287.947c.379 1.561 2.6 1.561 2.978 0a1.533 1.533 0 012.287-.947c1.372.836 2.942-.734 2.106-2.106a1.533 1.533 0 01.947-2.287c1.561-.379 1.561-2.6 0-2.978a1.532 1.532 0 01-.947-2.287c.836-1.372-.734-2.942-2.106-2.106a1.532 1.532 0 01-2.287-.947zM10 13a3 3 0 100-6 3 3 0 000 6z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },
                new MenuItem
                {
                    Title = "Audit Logs",
                    Url = "/AuditLogs",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "SuperAdmin" }
                },

                // ===== Admin Section =====
                new MenuItem
                {
                    Title = "HR Management",
                    Url = "/Users",
                    SectionHeader = "Organization",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M13 6a3 3 0 11-6 0 3 3 0 016 0zM18 8a2 2 0 11-4 0 2 2 0 014 0zM14 15a4 4 0 00-8 0v3h8v-3zM6 8a2 2 0 11-4 0 2 2 0 014 0zM16 18v-3a5.972 5.972 0 00-.75-2.906A3.005 3.005 0 0119 15v3h-3zM4.75 12.094A5.973 5.973 0 004 15v3H1v-3a3 3 0 013.75-2.906z"" /></svg>",
                    AllowedRoles = new[] { "Admin" }
                },
                new MenuItem
                {
                    Title = "Employee Management",
                    Url = "/Employees",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M9 6a3 3 0 11-6 0 3 3 0 016 0zM17 6a3 3 0 11-6 0 3 3 0 016 0zM12.93 17c.046-.327.07-.66.07-1a6.97 6.97 0 00-1.5-4.33A5 5 0 0119 16v1h-6.07zM6 11a5 5 0 015 5v1H1v-1a5 5 0 015-5z"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Attendance Monitoring",
                    Url = "/Attendance",
                    SectionHeader = "Payroll & Attendance",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Salary Computation",
                    Url = "/Salary",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M8.433 7.418c.155-.103.346-.196.567-.267v1.698a2.305 2.305 0 01-.567-.267C8.07 8.34 8 8.114 8 8c0-.114.07-.34.433-.582zM11 12.849v-1.698c.22.071.412.164.567.267.364.243.433.468.433.582 0 .114-.07.34-.433.582a2.305 2.305 0 01-.567.267z"" /><path fill-rule=""evenodd"" d=""M10 18a8 8 0 100-16 8 8 0 000 16zm1-13a1 1 0 10-2 0v.092a4.535 4.535 0 00-1.676.662C6.602 6.234 6 7.009 6 8c0 .99.602 1.765 1.324 2.246.48.32 1.054.545 1.676.662v1.941c-.22-.071-.412-.164-.567-.267C8.07 12.34 8 12.114 8 12a1 1 0 10-2 0c0 1.414.935 2.846 2.5 3.502V17a1 1 0 102 0v-.092a4.535 4.535 0 001.676-.662C13.398 15.766 14 14.991 14 14c0-.99-.602-1.765-1.324-2.246A4.535 4.535 0 0011 11.092V9.151c.22.071.412.164.567.267.364.243.433.468.433.582a1 1 0 102 0c0-1.414-.935-2.846-2.5-3.502V4z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Tax & Contributions",
                    Url = "/Contributions",
                    SectionHeader = "Compensation",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4z"" /><path fill-rule=""evenodd"" d=""M18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Compensation",
                    Url = "/Compensation",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Payslip",
                    Url = "/Payslips",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M7 3a1 1 0 000 2h6a1 1 0 100-2H7zM4 7a1 1 0 011-1h10a1 1 0 110 2H5a1 1 0 01-1-1zM2 11a2 2 0 012-2h12a2 2 0 012 2v4a2 2 0 01-2 2H4a2 2 0 01-2-2v-4z"" /></svg>",
                    AllowedRoles = new[] { "Admin", "HR" }
                },
                new MenuItem
                {
                    Title = "Audit Logs",
                    Url = "/AuditLogs",
                    SectionHeader = "Administration",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Admin" }
                },
                new MenuItem
                {
                    Title = "Company Settings",
                    Url = "/Settings",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M11.49 3.17c-.38-1.56-2.6-1.56-2.98 0a1.532 1.532 0 01-2.286.948c-1.372-.836-2.942.734-2.106 2.106.54.886.061 2.042-.947 2.287-1.561.379-1.561 2.6 0 2.978a1.532 1.532 0 01.947 2.287c-.836 1.372.734 2.942 2.106 2.106a1.532 1.532 0 012.287.947c.379 1.561 2.6 1.561 2.978 0a1.533 1.533 0 012.287-.947c1.372.836 2.942-.734 2.106-2.106a1.533 1.533 0 01.947-2.287c1.561-.379 1.561-2.6 0-2.978a1.532 1.532 0 01-.947-2.287c.836-1.372-.734-2.942-2.106-2.106a1.532 1.532 0 01-2.287-.947zM10 13a3 3 0 100-6 3 3 0 000 6z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Admin" }
                },

                // ===== Employee Section =====
                new MenuItem
                {
                    Title = "Attendance",
                    Url = "/Attendance",
                    SectionHeader = "My Workspace",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M10 18a8 8 0 100-16 8 8 0 000 16zm1-12a1 1 0 10-2 0v4a1 1 0 00.293.707l2.828 2.829a1 1 0 101.415-1.415L11 9.586V6z"" clip-rule=""evenodd"" /></svg>",
                    AllowedRoles = new[] { "Employee" }
                },
                new MenuItem
                {
                    Title = "Payslips",
                    Url = "/Payslips",
                    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path d=""M7 3a1 1 0 000 2h6a1 1 0 100-2H7zM4 7a1 1 0 011-1h10a1 1 0 110 2H5a1 1 0 01-1-1zM2 11a2 2 0 012-2h12a2 2 0 012 2v4a2 2 0 01-2 2H4a2 2 0 01-2-2v-4z"" /></svg>",
                    AllowedRoles = new[] { "Employee" }
                },
                //new MenuItem
                //{
                //    Title = "Profile",
                //    Url = "/Profile",
                //    Icon = @"<svg class=""shrink-0 w-5 h-5 transition duration-75 group-hover:text-fg-brand"" xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 20 20"" fill=""currentColor""><path fill-rule=""evenodd"" d=""M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z"" clip-rule=""evenodd"" /></svg>",
                //    AllowedRoles = new[] { "SuperAdmin", "Admin", "HR", "Employee" }
                //}
            };
        }

        /// <summary>
        /// Filters menu items based on user role
        /// </summary>
        /// <param name="userRole">The userRole<see cref="string?"/></param>
        /// <returns>The <see cref="List{MenuItem}"/></returns>
        public static List<MenuItem> GetMenuItemsForRole(string? userRole)
        {
            if (string.IsNullOrEmpty(userRole))
                return new List<MenuItem>();

            return GetMenuItems()
                  .Where(item => item.AllowedRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
