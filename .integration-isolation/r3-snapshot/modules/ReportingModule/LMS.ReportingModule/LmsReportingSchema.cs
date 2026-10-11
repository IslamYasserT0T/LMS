using IY.Reporting.Schema;

namespace LMS.ReportingModule;

public static class LmsReportingSchema
{
    public const string SamplesTable = "Samples";
    public const string CreatedByColumn = "CreatedBy";

    public static ReportSchema Create()
    {
        var columns = new Dictionary<string, ReportColumn>(StringComparer.OrdinalIgnoreCase)
        {
            ["Id"] = new("Id", ColumnDataType.String, IsPrimaryKey: true),
            ["Name"] = new("Name", ColumnDataType.String),
            [CreatedByColumn] = new(CreatedByColumn, ColumnDataType.String),
            ["CreatedDate"] = new("CreatedDate", ColumnDataType.Date),
            ["ModifiedDate"] = new("ModifiedDate", ColumnDataType.Date)
        };
        var operators = new[]
        {
            new ReportOperator("eq", new[] { ColumnDataType.String, ColumnDataType.Number, ColumnDataType.Date, ColumnDataType.Boolean }),
            new ReportOperator("contains", new[] { ColumnDataType.String }),
            new ReportOperator("gt", new[] { ColumnDataType.Number, ColumnDataType.Date }),
            new ReportOperator("gte", new[] { ColumnDataType.Number, ColumnDataType.Date }),
            new ReportOperator("lt", new[] { ColumnDataType.Number, ColumnDataType.Date }),
            new ReportOperator("lte", new[] { ColumnDataType.Number, ColumnDataType.Date })
        };
        return new ReportSchema("lms-samples-v1", new[] { new ReportTable(SamplesTable, columns, Array.Empty<ReportJoin>()) }, operators);
    }
}
