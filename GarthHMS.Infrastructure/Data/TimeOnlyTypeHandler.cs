// GarthHMS.Infrastructure/Data/TimeOnlyTypeHandler.cs
using System;
using System.Data;
using Dapper;

namespace GarthHMS.Infrastructure.Data
{
    public class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value.ToTimeSpan();
        }

        public override TimeOnly Parse(object value)
        {
            return value switch
            {
                TimeOnly t => t,
                TimeSpan ts => TimeOnly.FromTimeSpan(ts),
                DateTime dt => TimeOnly.FromDateTime(dt),
                _ => TimeOnly.FromTimeSpan((TimeSpan)Convert.ChangeType(value, typeof(TimeSpan)))
            };
        }
    }
}