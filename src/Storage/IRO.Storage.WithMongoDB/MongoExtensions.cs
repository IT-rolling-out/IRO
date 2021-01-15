using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IRO.Storage.WithMongoDB
{
    internal static class MongoExtensions
    {
        public static async Task UpsertAsync<T>(this IMongoCollection<T> col, Expression<Func<T, bool>> filter, T value)
        {
            var documents = await col.FindAsync(filter);
            if (documents.Any())
            {
                await col.ReplaceOneAsync(filter, value);
            }
            else
            {
                await col.InsertOneAsync(value);
            }
        }

        public static async Task<T> FindOneOrDefaultAsync<T>(this IMongoCollection<T> col, Expression<Func<T, bool>> filter)
        {
            var documents = await col.FindAsync(filter);
            return await documents.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Throw exception if not found.
        /// </summary>
        public static async Task<T> FindOneAsync<T>(this IMongoCollection<T> col, Expression<Func<T, bool>> filter)
        {
            var documents = await col.FindAsync(filter);
            var res = await documents.FirstOrDefaultAsync();
            if (res == null)
                throw new Exception($"Entity of type {typeof(T).Name} not found in db.");
            return res;
        }

        public static async Task EnsureIndex<T>(this IMongoCollection<T> col, Expression<Func<T, object>> expr)
        {
            string name = null;
            if (expr.Body is MemberExpression memberExpr)
            {
                name = memberExpr.Member.Name;
            }
            else if (expr.Body is UnaryExpression unaryExpr)
            {
                var memberExpr2 = (MemberExpression)unaryExpr.Operand;
                name = memberExpr2.Member.Name;
            }
            else
            {
                throw new Exception("Can't resolve member name from expression.");
            }
            await EnsureIndex(col, name);
        }

        public static async Task EnsureIndex<T>(this IMongoCollection<T> col, string fieldName)
        {
            var indexBuilder = Builders<T>.IndexKeys;
            var keys = indexBuilder.Descending(fieldName);
            var indexModel = new CreateIndexModel<T>(keys);
            await col.Indexes.CreateOneAsync(indexModel);
        }
    }
}