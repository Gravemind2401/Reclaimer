// <auto-generated>
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable DoNotCallOverridableMethodsInConstructor
// ReSharper disable EmptyNamespace
// ReSharper disable InconsistentNaming
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ReSharper disable RedundantOverridenMember
// ReSharper disable UseNameofExpression
// TargetFrameworkVersion = 4.6
#pragma warning disable 1591    //  Ignore "Missing XML Comment" warning

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Reclaimer.Entities
{
    using Adjutant.Blam.Definitions;

    public interface IDatabaseContext : System.IDisposable
    {
        System.Data.Entity.DbSet<CacheFile> CacheFiles { get; set; } // cache_file
        System.Data.Entity.DbSet<StringIndex> StringIndexes { get; set; } // string_index
        System.Data.Entity.DbSet<StringItem> StringItems { get; set; } // string_item
        System.Data.Entity.DbSet<StringValue> StringValues { get; set; } // string_value
        System.Data.Entity.DbSet<TagIndex> TagIndexes { get; set; } // tag_index
        System.Data.Entity.DbSet<TagItem> TagItems { get; set; } // tag_item
        System.Data.Entity.DbSet<TagPath> TagPaths { get; set; } // tag_path

        int SaveChanges();
        System.Threading.Tasks.Task<int> SaveChangesAsync();
        System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken);
        System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker { get; }
        System.Data.Entity.Infrastructure.DbContextConfiguration Configuration { get; }
        System.Data.Entity.Database Database { get; }
        System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity);
        System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors();
        System.Data.Entity.DbSet Set(System.Type entityType);
        System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class;
        string ToString();
    }

}
// </auto-generated>