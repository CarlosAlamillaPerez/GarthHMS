// GarthHMS.Infrastructure/Data/DateOnlyTypeHandler.cs
using System;
using System.Data;
using Dapper;

namespace GarthHMS.Infrastructure.Data
{
    /// <summary>
    /// TypeHandler para que Dapper pueda manejar DateOnly ↔ PostgreSQL DATE.
    /// Registrar una sola vez en Program.cs con:
    ///     SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
    /// </summary>
    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
        {
            parameter.DbType = DbType.Date;
            parameter.Value = value.ToDateTime(TimeOnly.MinValue);
        }

        public override DateOnly Parse(object value)
        {
            return value switch
            {
                DateOnly d => d,
                DateTime dt => DateOnly.FromDateTime(dt),
                _ => DateOnly.FromDateTime(Convert.ToDateTime(value))
            };
        }
    }
}