using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Budget.HelperExtensions
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
    }

    public static class DbExtensions
    {

        public static void SoftDelete<TEntity, TKey>(this IDbSet<TEntity> Set, TKey key) where TEntity : class, ISoftDeletable
        {
            var item = Set.Find(key);
            item.IsDeleted = true;
        }

        public static IQueryable NotDeleted<TEntity>(this IQueryable<TEntity> Query) where TEntity : class, ISoftDeletable
        {
            return Query.Where(t => t.IsDeleted == false);
        }
    }
}