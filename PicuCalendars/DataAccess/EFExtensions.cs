using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EFExtensions
{
    public static class EFExtensions
    {
        public static EntityOp<TEntity> Upsert<TEntity>(this DbContext context, IEnumerable<TEntity> entity) where TEntity : class
        {
            return new UpsertOp<TEntity>(context, entity);
        }
    }

    public abstract class EntityOp<TEntity, TRet>
    {
        public readonly DbContext Context;
        public readonly IEnumerable<TEntity> EntityList;
        public readonly string TableName;
        public readonly string EntityPrimaryKeyName;

        private readonly List<string> keyNames = new List<string>();
        public IEnumerable<string> KeyNames { get { return keyNames; } }

        private readonly List<string> excludeProperties = new List<string>();

        private static string GetMemberName<T>(Expression<Func<TEntity, T>> selectMemberLambda)
        {
            var member = selectMemberLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException("The parameter selectMemberLambda must be a member accessing labda such as x => x.Id", "selectMemberLambda");
            }
            return member.Member.Name;
        }

        public EntityOp(DbContext context, IEnumerable<TEntity> entityList)
        {
            Context = context;
            EntityList = entityList;
            TableName = GetTableName(typeof(TEntity), context, out EntityPrimaryKeyName);
        }

        public abstract TRet Execute();
        public void Run()
        {
            Execute();
        }

        public EntityOp<TEntity, TRet> Key<TKey>(Expression<Func<TEntity, TKey>> selectKey)
        {
            keyNames.Add(GetMemberName(selectKey));
            return this;
        }

        public EntityOp<TEntity, TRet> ExcludeField<TField>(Expression<Func<TEntity, TField>> selectField)
        {
            excludeProperties.Add(GetMemberName(selectField));
            return this;
        }

        public IEnumerable<PropertyInfo> ColumnProperties
        {
            get
            {
                // Dont include virtual navigation properties
                return typeof(TEntity).GetProperties().Where(pr => !excludeProperties.Contains(pr.Name) && !pr.GetMethod.IsVirtual && pr.Name != EntityPrimaryKeyName);
            }
        }

        public static string GetTableName(Type type, DbContext context, out string EntityPrimaryKeyName)
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Get the name of the primary key for the table as we wish to exclude this from the column mapping (we are assuming Identity insert is OFF)
            EntityPrimaryKeyName = mapping.EntitySet.ElementType.KeyMembers.Select(k => k.Name).FirstOrDefault();

            // Find the storage entity set (table) that the entity is mapped
            var table = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Return the table name from the storage entity set
            return (string)table.MetadataProperties["Table"].Value ?? table.Name;
        }
    }

    public abstract class EntityOp<TEntity> : EntityOp<TEntity, int>
    {
        public EntityOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        public sealed override int Execute()
        {
            ExecuteNoRet();
            return 0;
        }

        protected abstract void ExecuteNoRet();
    }

    public class UpsertOp<TEntity> : EntityOp<TEntity>
    {
        public UpsertOp(DbContext context, IEnumerable<TEntity> entityList) : base(context, entityList) { }

        protected override void ExecuteNoRet()
        {
            StringBuilder sql = new StringBuilder();

            int notNullFields = 0;
            var valueKeyList = new List<string>();
            var columnList = new List<string>();

            var columnProperties = ColumnProperties.ToArray();
            foreach (var p in columnProperties)
            {
                columnList.Add(p.Name);
                valueKeyList.Add("{" + (notNullFields++) + "}");
            }
            var columns = columnList.ToArray();

            sql.Append("merge into ");
            sql.Append(TableName);
            sql.Append(" as T ");

            sql.Append("using (values (");
            sql.Append(string.Join(",", valueKeyList.ToArray()));
            sql.Append(")) as S (");
            sql.Append(string.Join(",", columns));
            sql.Append(") ");

            sql.Append("on (");
            var mergeCond = string.Join(" and ", KeyNames.Select(kn => "T." + kn + "=S." + kn));
            sql.Append(mergeCond);
            sql.Append(") ");

            sql.Append("when matched then update set ");
            sql.Append(string.Join(",", columns.Select(c => "T." + c + "=S." + c).ToArray()));

            sql.Append(" when not matched then insert (");
            sql.Append(string.Join(",", columns));
            sql.Append(") values (S.");
            sql.Append(string.Join(",S.", columns));
            sql.Append(");");
            var command = sql.ToString();

            foreach (var entity in EntityList)
            {
                var valueList = new List<object>();

                foreach (var p in columnProperties)
                {
                    var val = p.GetValue(entity, null);
                    valueList.Add(val ?? DBNull.Value);
                }

                Context.Database.ExecuteSqlCommand(command, valueList.ToArray());
            }
        }
    }
}