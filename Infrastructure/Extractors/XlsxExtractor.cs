using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentMcpServer.Core.Interfaces;
using System.Text;

namespace DocumentMcpServer.Infrastructure.Extractors;

/// <summary>
/// Extracts content from Microsoft Excel spreadsheets (.xlsx).
/// </summary>
public class XlsxExtractor : IDocumentExtractor
{
    public bool CanExtract(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    public string ExtractContent(string filePath)
    {
        try
        {
            using var doc = SpreadsheetDocument.Open(filePath, false);
            var workbookPart = doc.WorkbookPart;
            if (workbookPart == null)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var sheet in workbookPart.Workbook.Descendants<Sheet>())
            {
                var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id!);
                var sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();

                if (sheetData != null)
                {
                    foreach (var row in sheetData.Elements<Row>())
                    {
                        foreach (var cell in row.Elements<Cell>())
                        {
                            var cellValue = GetCellValue(cell, workbookPart);
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                sb.Append(cellValue).Append(' ');
                            }
                        }
                    }
                }
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"[Unable to read Excel file: {ex.Message}]";
        }
    }

    private static string GetCellValue(Cell cell, WorkbookPart workbookPart)
    {
        var value = cell.InnerText;
        if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
        {
            var sharedStringTablePart = workbookPart.SharedStringTablePart;
            if (sharedStringTablePart != null)
            {
                return sharedStringTablePart.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }
        }
        return value;
    }
}
