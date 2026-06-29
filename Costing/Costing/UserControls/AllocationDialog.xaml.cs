using Costing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Costing.UserControls
{
    public partial class AllocationDialog : Window
    {


        public List<WorkCentre> WorkCentres { get; set; } = new List<WorkCentre>();
        public List<AllocationRow> ResultRows { get; private set; } = new List<AllocationRow>();


        private readonly string _employeeCode;
        private readonly string _employeeName;
        private ObservableCollection<AllocationRow> _rows;

        private const decimal MAX_TOTAL = 1.0m;
        private bool _suppressTextChanged = false;


        public AllocationDialog(string employeeCode, string employeeName, List<Allocation> existingAllocations)
        {
            InitializeComponent();

            _employeeCode = employeeCode;
            _employeeName = employeeName;

            DataContext = this;
            tbEmployeeName.Text = employeeName;

            if (existingAllocations != null && existingAllocations.Any(a => !string.IsNullOrEmpty(a.WorkCentre)))
            {
                _rows = new ObservableCollection<AllocationRow>(
                    existingAllocations
                        .Where(a => !string.IsNullOrEmpty(a.WorkCentre))
                        .Select(a => new AllocationRow
                        {
                            WorkCentre = a.WorkCentre,
                            Portion = a.Portion ?? 0m
                        }));
            }
            else
            {
                _rows = new ObservableCollection<AllocationRow>();
                _rows.Add(new AllocationRow());
            }

            icRows.ItemsSource = _rows;
            RefreshTotal();
        }

        #region Helpers

        private decimal GetTotal() => _rows.Sum(r => r.Portion);

        private void RefreshTotal(decimal? liveTotalOverride = null)
        {
            decimal total = liveTotalOverride ?? GetTotal();
            tbTotal.Text = $"{total:0.##} / 1.00";

            if (total > MAX_TOTAL)
            {
                tbTotal.Foreground = Brush("#DC2626");
                tbTotalWarning.Text = "Total exceeds 1.00";
                btnSave.IsEnabled = false;
            }
            else if (total == MAX_TOTAL)
            {
                tbTotal.Foreground = Brush("#16A34A");
                tbTotalWarning.Text = "Fully allocated";
                btnSave.IsEnabled = true;
            }
            else
            {
                tbTotal.Foreground = Brush("#1E293B");
                tbTotalWarning.Text = "";
                btnSave.IsEnabled = true;
            }
        }

        private static System.Windows.Media.SolidColorBrush Brush(string hex)
        {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            return new System.Windows.Media.SolidColorBrush(c);
        }

        private void AddRow()
        {
            if (GetTotal() >= MAX_TOTAL) return;
            if (!_rows.Any()) return;
            if (_rows.Last().Portion > 0)
                _rows.Add(new AllocationRow());
        }

        #endregion

        #region Events
        private void TbPortion_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var tb = (TextBox)sender;
            string prospective = tb.Text.Remove(tb.SelectionStart, tb.SelectionLength).Insert(tb.SelectionStart, e.Text);

            // Allow dots OR commas to support regional numpad settings
            e.Handled = !Regex.IsMatch(prospective, @"^\d*[.,]?\d*$");
        }

        private void TbPortion_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = (TextBox)sender;
            if (tb.Tag is AllocationRow row)
            {
                // converting commas to dots for C#
                string cleanText = tb.Text.Replace(",", ".");
                decimal.TryParse(cleanText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal activeVal);

                decimal liveTotal = 0;
                foreach (var r in _rows)
                {
                    liveTotal += (r == row) ? activeVal : r.Portion;
                }

                RefreshTotal(liveTotal);

                // Auto-add new row
                if (liveTotal < MAX_TOTAL && activeVal > 0 && _rows.Last() == row)
                {
                    _rows.Add(new AllocationRow());
                }
            }
        }

        private void BtnRemoveRow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is AllocationRow row)
            {
                if (_rows.Count <= 1)
                {
                    row.WorkCentre = null;
                    row.Portion = 0;
                    RefreshTotal();
                    return;
                }

                _rows.Remove(row);
                RefreshTotal();
                AddRow();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            decimal total = GetTotal();

            if (total > MAX_TOTAL)
            {
                MessageBox.Show(
                    $"The total allocation ({total:0.##}) exceeds 1.00.\nPlease adjust the portions before saving.",
                    "Invalid Total", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var validRows = _rows
                .Where(r => !string.IsNullOrEmpty(r.WorkCentre) && r.Portion > 0)
                .ToList();

            if (!validRows.Any())
            {
                MessageBox.Show(
                    "Please add at least one work centre with a valid allocation portion.",
                    "No Allocations", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var duplicates = validRows
                .GroupBy(r => r.WorkCentre)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicates.Any())
            {
                MessageBox.Show(
                    $"The following work centres appear more than once:\n{string.Join(", ", duplicates)}\n\nPlease remove duplicates.",
                    "Duplicate Work Centres", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultRows = validRows;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }
}