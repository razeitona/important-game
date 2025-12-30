using Dapper;
using System.Data;
using System.Globalization;

namespace important_game.app.Handlers;

public sealed class DateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value)
    {
        if (string.IsNullOrWhiteSpace((string)value))
            return default;

        return DateTimeOffset.ParseExact(value.ToString()!, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
    }

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value)
    {
        parameter.DbType = DbType.String;
        parameter.Value = value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}

public sealed class NullableDateTimeOffsetHandler : SqlMapper.TypeHandler<DateTimeOffset?>
{
    public override DateTimeOffset? Parse(object value)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            return null;

        return DateTimeOffset.ParseExact(
            value.ToString()!,
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal
        );
    }

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset? value)
    {
        if (value == null)
        {
            parameter.Value = DBNull.Value;
            return;
        }

        parameter.DbType = DbType.String;
        parameter.Value = value.Value.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
