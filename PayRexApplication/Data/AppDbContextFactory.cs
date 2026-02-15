using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace PayRexApplication.Data
{
    /// <summary>
    /// Design time factory for AppDbContext so EF tools can create the context without requiring the startup project's DI.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
   var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            string? configBasePath = FindConfigBasePath(environment);

  if (string.IsNullOrEmpty(configBasePath))
      {
       configBasePath = AppContext.BaseDirectory;
       }

            var builder = new ConfigurationBuilder()
.SetBasePath(configBasePath)
   .AddJsonFile("appsettings.json", optional: true)
         .AddJsonFile($"appsettings.{environment}.json", optional: true)
      .AddEnvironmentVariables();

          var configuration = builder.Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
             throw new InvalidOperationException("Could not find a connection string named 'DefaultConnection' in appsettings.json or environment variables. Looked in: " + configBasePath);
            }

  var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
  optionsBuilder.UseSqlServer(connectionString);

 return new AppDbContext(optionsBuilder.Options);
        }

        private static string? FindConfigBasePath(string environment)
        {
  bool TryDir(string dir, out string? result)
            {
       result = null;
   try
    {
      var candidate = Path.Combine(dir, "appsettings.json");
     if (!File.Exists(candidate)) return false;

       var tempConfig = new ConfigurationBuilder()
       .SetBasePath(dir)
         .AddJsonFile("appsettings.json", optional: true)
      .AddJsonFile($"appsettings.{environment}.json", optional: true)
      .AddEnvironmentVariables()
       .Build();

          var cs = tempConfig.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(cs))
     {
     result = dir;
    return true;
               }
 }
          catch
        {
        // ignore and continue
              }
      return false;
   }

         // FIRST: Try the known API project path relative to solution
    try
   {
  // Look for PayRexApplication folder by walking up from current directory
 var current = new DirectoryInfo(Directory.GetCurrentDirectory());
           while (current != null)
 {
     // Check if this is the solution root (contains PayRexApplication folder)
         var apiProjectDir = Path.Combine(current.FullName, "PayRexApplication");
      if (Directory.Exists(apiProjectDir) && TryDir(apiProjectDir, out var found))
   {
    return found;
      }
 current = current.Parent;
  }
       }
   catch
            {
     // ignore
            }

            // Then start from the assembly location and walk up
  try
   {
      var assemblyPath = typeof(AppDbContextFactory).Assembly.Location;
         if (!string.IsNullOrEmpty(assemblyPath))
      {
       var asmDir = Path.GetDirectoryName(assemblyPath);
        var current = new DirectoryInfo(asmDir ?? AppContext.BaseDirectory!);
        while (current != null)
    {
         if (TryDir(current.FullName, out var found)) return found;

      // Also check for PayRexApplication subfolder
       var apiDir = Path.Combine(current.FullName, "PayRexApplication");
       if (Directory.Exists(apiDir) && TryDir(apiDir, out var apiFound))
            {
            return apiFound;
             }

       current = current.Parent;
       }
    }
            }
      catch
      {
          // ignore
   }

         // Finally try current directory walk
        try
    {
var current = new DirectoryInfo(Directory.GetCurrentDirectory());
      while (current != null)
      {
             if (TryDir(current.FullName, out var found)) return found;
   current = current.Parent;
  }
            }
   catch
            {
   // ignore
        }

         return null;
    }
    }
}
