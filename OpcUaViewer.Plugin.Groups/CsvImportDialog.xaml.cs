using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OpcUaViewer.Core.Contracts;
using OpcUaViewer.Core.Settings;

namespace OpcUaViewer.Plugin.Groups;

public partial class CsvImportDialog : Window
{
    public string GroupName   { get; private set; } = "";
    public string CsvFilePath { get; private set; } = "";

    private sealed class MappingRow : INotifyPropertyChanged
    {
        private string _column = "", _regex = "";

        public string Label     { get; init; } = "";
        public string ColumnTip { get; init; } = "";
        public string RegexTip  { get; init; } = "";

        public string Column { get => _column; set { _column = value; PropertyChanged?.Invoke(this, new(nameof(Column))); } }
        public string Regex  { get => _regex;  set { _regex  = value; PropertyChanged?.Invoke(this, new(nameof(Regex)));  } }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    private readonly List<MappingRow> _rows;

    public CsvImportDialog()
    {
        InitializeComponent();

        var s = AppSettings.Current;
        _rows =
        [
            new MappingRow { Label = "Part Name:",     Column = s.CsvPartNameColumn,     Regex = s.CsvPartNameRegex,
                RegexTip = "Applied to Part Name. Result becomes the List ID.\nExample: ^([A-Z]+-\\d+)" },
            new MappingRow { Label = "Template Name:", Column = s.CsvTemplateFileColumn, Regex = s.CsvTemplateFileRegex,
                ColumnTip = "Identifies the template/product file.\nLeave blank to use Part Name as Product ID.",
                RegexTip  = "Applied to Template column. Result is used to look up the product zip.\nExample: ([^\\\\/]+)(?:\\.zip)?$" },
            new MappingRow { Label = "Quantity:",      Column = s.CsvQtyColumn,          Regex = s.CsvQtyRegex,
                RegexTip = "Extracts a number from the Quantity column.\nExample: (\\d+)" },
            new MappingRow { Label = "Material:",      Column = s.CsvMaterialColumn,     Regex = s.CsvMaterialRegex,
                RegexTip = "Applied to Material column.\nExample: ^(\\S+) takes the first word." },
            new MappingRow { Label = "Thickness:",     Column = s.CsvThicknessColumn,    Regex = s.CsvThicknessRegex,
                RegexTip = "Extracts a number (inches) from the Thickness column.\nExample: (\\d+\\.?\\d*)" },
            new MappingRow { Label = "Length:",        Column = s.CsvLengthColumn,       Regex = s.CsvLengthRegex,
                RegexTip = "Extracts a number (inches) from the Length column.\nExample: L=(\\d+\\.?\\d*)" },
            new MappingRow { Label = "Width:",         Column = s.CsvWidthColumn,        Regex = s.CsvWidthRegex,
                RegexTip = "Extracts a number (inches) from the Width column.\nExample: W=(\\d+\\.?\\d*)" },
        ];

        MappingRows.ItemsSource = _rows;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Title = "Select CSV file", Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
        if (dlg.ShowDialog(this) == true) CsvPathBox.Text = dlg.FileName;
    }

    private void Import_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CsvPathBox.Text) || !File.Exists(CsvPathBox.Text))
        { DialogService.Current.Warn("Please select a CSV file.", "Import CSV"); return; }

        if (string.IsNullOrWhiteSpace(_rows[0].Column))
        { DialogService.Current.Warn("Part Name column is required.", "Import CSV"); return; }

        if (string.IsNullOrWhiteSpace(GroupNameBox.Text))
        { DialogService.Current.Warn("Please enter a group name.", "Import CSV"); return; }

        string[] regexLabels = ["Part Name RegEx", "Template RegEx", "Quantity RegEx",
                                 "Material RegEx", "Thickness RegEx", "Length RegEx", "Width RegEx"];
        for (int i = 0; i < _rows.Count; i++)
        {
            string pat = _rows[i].Regex.Trim();
            if (string.IsNullOrEmpty(pat)) continue;
            try { _ = new Regex(pat); }
            catch (ArgumentException ex)
            { DialogService.Current.Warn($"Invalid {regexLabels[i]}:\n\n{ex.Message}", "Import CSV"); return; }
        }

        var s = AppSettings.Current;
        s.CsvPartNameColumn     = _rows[0].Column.Trim(); s.CsvPartNameRegex      = _rows[0].Regex.Trim();
        s.CsvTemplateFileColumn = _rows[1].Column.Trim(); s.CsvTemplateFileRegex  = _rows[1].Regex.Trim();
        s.CsvQtyColumn          = _rows[2].Column.Trim(); s.CsvQtyRegex           = _rows[2].Regex.Trim();
        s.CsvMaterialColumn     = _rows[3].Column.Trim(); s.CsvMaterialRegex      = _rows[3].Regex.Trim();
        s.CsvThicknessColumn    = _rows[4].Column.Trim(); s.CsvThicknessRegex     = _rows[4].Regex.Trim();
        s.CsvLengthColumn       = _rows[5].Column.Trim(); s.CsvLengthRegex        = _rows[5].Regex.Trim();
        s.CsvWidthColumn        = _rows[6].Column.Trim(); s.CsvWidthRegex         = _rows[6].Regex.Trim();
        AppSettings.Save();

        GroupName   = GroupNameBox.Text.Trim();
        CsvFilePath = CsvPathBox.Text.Trim();
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private void TitleBar_Drag(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed) DragMove();
    }
}
