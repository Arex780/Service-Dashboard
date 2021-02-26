using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebArchive.Data
{
    public class WebDataContext : DbContext
    {
        public DbSet<Models.Project> Projects { get; set; }

        public WebDataContext(DbContextOptions<WebDataContext> options) : base(options)
        {
            Database.Migrate();
        }

        public async Task<int> SaveChangesAsync(string user)
        {
            Models.Project previousProject = new Models.Project();
            Models.Project currentProject = new Models.Project();
            ChangeTracker.DetectChanges();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (entry.State == EntityState.Added || entry.State == EntityState.Deleted || entry.State == EntityState.Modified)
                    {
                        currentProject.GetType().GetProperty(property.Metadata.Name).SetValue(currentProject, property.CurrentValue, null);
                        PropertyInfo info = previousProject.GetType().GetProperty(property.Metadata.Name);
                        var OldValue = entry.GetDatabaseValues().GetValue<object>(property.Metadata.Name);
                        if (info == null || !info.CanWrite || property.CurrentValue == OldValue)
                            continue;
                        info.SetValue(previousProject, OldValue, null);
                    }
                }
            }
            Utils.LogHandler.ProjectEditLog(currentProject, previousProject, user);
            return await base.SaveChangesAsync();
        }
    }

}