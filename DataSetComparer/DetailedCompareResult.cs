using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DataSetTools
{
    public class DetailedCompareResult
    {
        private ObservableCollection<CompareError> _errors;
        public ObservableCollection<CompareError> ComparisonErrors
        {
            get
            {
                return _errors;
            }
        }

        public int LeftRowCount { get; set; }
        public int RightRowCount { get; set; }

        public DetailedCompareResult()
        {
            _errors = new ObservableCollection<CompareError>();
        }

        public DetailedStats GetCalculatedStats()
        {
            var stats = new DetailedStats();

            var s = from e in _errors.Where(er => er.ValuesDoesNotMatch).SelectMany(er => er.ValueDifferences)
                    group e by e.ColumnName into g
                    select new { 
                        NonMatchingCount = g.Sum(f => 1), 
                        g.Key, 
                        Members = g.ToList() 
                    };

            foreach (var d in s)
            {
                stats.Values.Add(d.Key, new ValueStat
                {
                    NonMatchingCount = d.NonMatchingCount, 
                    Members = d.Members
                });
            }

            return stats;
        }
    }
}
