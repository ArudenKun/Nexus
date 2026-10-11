using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using OfficeIMO;
using OfficeIMO.Excel;
using OfficeIMO.Excel.Html;

namespace Nexus.Blazor.Client.Pages;

public partial class OfficeViewer
{
    private IBrowserFile? _selectedFile;
    private ExcelDocument? _selectedExcelDocument;
    private ExcelSheet? _selectedExcelSheet;
    private IReadOnlyList<ExcelSheet> ExcelSheets => _selectedExcelDocument?.Sheets ?? [];
    private IReadOnlyList<string> SheetNames => ExcelSheets.Select(x => x.Name).ToList() ?? [];
    private string _excelHtml = string.Empty;
    private bool _isLoading;

    private async Task OnInputFileChanged(InputFileChangeEventArgs e)
    {
        if (_selectedExcelDocument is not null)
        {
            await _selectedExcelDocument.DisposeAsync();
        }

        _selectedFile = e.File;
        if (_selectedFile == null)
        {
            _selectedExcelDocument = null;
            StateHasChanged();
            return;
        }

        _isLoading = true;
        StateHasChanged();

        try
        {
            // Register encoding provider required for legacy .xls files
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Read stream into memory array so we can reload without re-uploading
            await using var stream = _selectedFile.OpenReadStream(maxAllowedSize: 20_000_000); // 20 MB limit
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _selectedExcelDocument = await ExcelDocument.LoadAsync(
                memoryStream,
                new ExcelLoadOptions { AccessMode = DocumentAccessMode.ReadOnly }
            );

            // Generate initial HTML
            ConvertSelectedSheetToHtml();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    // 2. Handle Sheet Switch
    private async Task OnSheetChanged(string newSheetName)
    {
        _selectedExcelSheet = _selectedExcelDocument?.Sheets.FirstOrDefault(x =>
            x.Name == newSheetName
        );
        await ReloadExcelHtmlAsync();
    }

    // 3. Re-convert and Reload HTML Trigger
    private async Task ReloadExcelHtmlAsync()
    {
        if (_selectedExcelDocument == null || _selectedExcelSheet == null)
            return;

        _isLoading = true;
        StateHasChanged();

        // Small delay to allow UI spinner to show during larger tables
        await Task.Yield();

        ConvertSelectedSheetToHtml();

        _isLoading = false;
        StateHasChanged();
    }

    // 4. Conversion Core Engine (DataTable -> HTML Table)
    private void ConvertSelectedSheetToHtml()
    {
        if (_selectedExcelDocument == null || _selectedExcelSheet == null)
        {
            _excelHtml = string.Empty;
            return;
        }

        _excelHtml = _selectedExcelSheet.ToHtml(
            new ExcelHtmlSaveOptions { HeaderMode = ExcelHtmlHeaderMode.FirstRow }
        );
    }
}
