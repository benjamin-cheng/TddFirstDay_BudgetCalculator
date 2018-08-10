﻿using System;
using System.Linq;

namespace BudgetCalculator
{
    public class Period
    {
        public Period(DateTime start, DateTime end)
        {
            if (start > end)
                throw new ArgumentException();

            Start = start;
            End = end;
        }

        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public bool IsQuerySingleMonth()
        {
            return Start.ToString("yyyyMM") == End.ToString("yyyyMM");
        }
    }

    public class BudgetCalculator
    {
        private IBudgetRepo _repo;

        public BudgetCalculator(IBudgetRepo repo)
        {
            _repo = repo;
        }

        public decimal TotalAmount(DateTime start, DateTime end)
        {
            var period = new Period(start, end);

            var budgetList = _repo.GetAll();
            if (budgetList.Count == 0)
            {
                return 0;
            }

            if (period.IsQuerySingleMonth())
            {
                var effectiveStart = period.Start;
                var effectiveEnd = period.End;
                var budget = budgetList.FirstOrDefault(a => a.YearMonth == period.Start.ToString("yyyyMM"));
                return EffectiveAmount(budget, Days(effectiveStart, effectiveEnd));
            }
            else
            {
                var totalAmount = 0m;

                var middleStart = new DateTime(period.Start.Year, period.Start.Month, 1);
                var middleEnd = new DateTime(period.End.Year, period.End.Month,
                    DateTime.DaysInMonth(period.End.Year, period.End.Month));

                while (middleStart < middleEnd)
                {
                    var budget = budgetList.FirstOrDefault(a => a.YearMonth == middleStart.ToString("yyyyMM"));
                    if (budget != null)
                    {
                        var effectiveDays = EffectiveDays(period, budget);
                        totalAmount += EffectiveAmount(budget, effectiveDays);
                    }

                    middleStart = middleStart.AddMonths(1);
                }

                return totalAmount;
            }
        }

        private static int EffectiveDays(Period period, Budget budget)
        {
            var effectiveStart = period.Start;
            var effectiveEnd = period.End;
            if (budget.YearMonth == period.Start.ToString("yyyyMM"))
            {
                effectiveEnd = budget.LastDay();
            }
            else if (budget.YearMonth == period.End.ToString("yyyyMM"))
            {
                effectiveStart = budget.FirstDay();
            }
            else
            {
                effectiveStart = budget.FirstDay();
                effectiveEnd = budget.LastDay();
            }

            var effectiveDays = Days(effectiveStart, effectiveEnd);
            return effectiveDays;
        }

        private decimal EffectiveAmount(Budget budget, int effectiveDays)
        {
            if (budget != null)
            {
                return budget.DailyAmount() * (effectiveDays);
            }
            else
                return 0;
        }

        private static int Days(DateTime start, DateTime end)
        {
            int dayDiffs = (end - start).Days + 1;
            return dayDiffs;
        }
    }
}