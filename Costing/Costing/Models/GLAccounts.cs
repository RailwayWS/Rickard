namespace Costing.Models
{
    public class GLPeriodControl
    {
        public string Company { get; set; }
        public int GlPeriod { get; set; }
        public int GlYear { get; set; }
    }

    public class GLAccount
    {
        public string Company { get; set; }
        public int GlYear { get; set; }
        public int GlPeriod { get; set; }
        public string GlCode { get; set; }
        public string Description { get; set; }
        public string GlGroup { get; set; }
        public decimal Value { get; set; }

        public decimal Annualised
        {
            get
            {
                if (GlPeriod == 0) return 0;
                return (Value / GlPeriod) * 12;
            }
        }
    }
}