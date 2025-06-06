using System;
using System.Collections.Generic;

namespace KPI_Dashboard.Models
{
    public class DashboardViewModel
    {
        public class LineChartData
        {
            public List<string> Dates { get; set; } = new List<string>();
            public List<int> Applications { get; set; } = new List<int>();
            public List<int> AdmissionConsultations { get; set; } = new List<int>();
            public List<int> VisaConsultations { get; set; } = new List<int>();
            public List<int> Inquiries { get; set; } = new List<int>();
            public List<int> Conversions { get; set; } = new List<int>();
        }

        public class PieChartData
        {
            public List<string> Labels { get; set; } = new List<string>();
            public List<int> Values { get; set; } = new List<int>();
        }

        public class BarChartData
        {
            public List<string> Labels { get; set; } = new List<string>();
            public List<int> Applications { get; set; } = new List<int>();
            public List<int> Consultations { get; set; } = new List<int>();
            public List<int> Conversions { get; set; } = new List<int>();
        }

        public class MetricData
        {
            public int TotalApplications { get; set; }
            public int TotalConsultations { get; set; }
            public double VisaConversionRate { get; set; }
            public string TopPerformingUser { get; set; }
        }

        public LineChartData LineData { get; set; } = new LineChartData();
        public PieChartData PieData { get; set; } = new PieChartData();
        public BarChartData BarData { get; set; } = new BarChartData();
        public MetricData Metrics { get; set; } = new MetricData();
    }
}