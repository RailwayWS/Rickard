using Costing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Costing.Helpers
{
    public class CalculationEngine
    {
        public static List<CalculatedStaff> ProcessCalculations(List<CalculatedStaff> rawRecords, List<StaffCost> liveDbCosts, IProgress<int> progress = null)
        {
            List<CalculatedStaff> processedList = new List<CalculatedStaff>();

            int totalRecords = rawRecords.Count;
            int currentRecord = 0;

            foreach (var raw in rawRecords)
            {
                raw.Rate = raw.RatePerHour * raw.Allocation;

                // every emp's calcs are stored in a dictionary for easy use
                var computedValues = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Base Rate", raw.Rate },
                    { "RatePerHour", raw.RatePerHour },
                    { "Allocation", raw.Allocation }
                };

                // function guarantees everything is calculated in the right order
                decimal ResolveCategory(string categoryName)
                {
                    string cleanName = categoryName.Trim();

                    if (computedValues.ContainsKey(cleanName))
                        return computedValues[cleanName];

                    // Find the rule the user created in the Database
                    var rule = liveDbCosts.FirstOrDefault(c => c.Category.Equals(cleanName, StringComparison.OrdinalIgnoreCase));
                    if (rule == null) return 0m;

                    decimal finalValue = 0m;

                    if (rule.Type == "Fixed")
                    {
                        finalValue = rule.Value;
                    }
                    else if (rule.Type == "Percentage" || rule.Type == "%")
                    {
                        decimal baseSum = 0m;

                        // SPLIT THE COMMAS
                        if (!string.IsNullOrWhiteSpace(rule.BaseCategory))
                        {
                            var baseCategories = rule.BaseCategory.Split(',');
                            foreach (var baseCat in baseCategories)
                            {
                                // calculate the required base category first
                                baseSum += ResolveCategory(baseCat);
                            }
                        }

                        finalValue = baseSum * rule.Value;
                    }

                    if (cleanName.Equals("UIF", StringComparison.OrdinalIgnoreCase))
                    {
                        decimal maxAnnualUif = 17712m;
                        if (finalValue > maxAnnualUif) finalValue = maxAnnualUif;
                    }

                    // Save it so we never calculate it twice for this employee
                    computedValues[cleanName] = finalValue;
                    return finalValue;
                }

                // Process every rule in the database
                decimal runningTotal = 0m;
                foreach (var dbCost in liveDbCosts)
                {
                    decimal amount = ResolveCategory(dbCost.Category);

                    raw.DynamicCosts.Add(new CalculatedStaffCost
                    {
                        CategoryName = dbCost.Category,
                        Amount = amount
                    });

                    runningTotal += amount;
                }

                raw.Total = runningTotal;
                processedList.Add(raw);

                currentRecord++;

                // for progress bar
                if (progress != null)
                {
                    // Calculate percentage and send it back
                    int percentComplete = (int)((double)currentRecord / totalRecords * 100);
                    progress.Report(percentComplete);
                }

                System.Threading.Thread.Sleep(1);//REMOVE
            }

            return processedList;
        }
    }
}