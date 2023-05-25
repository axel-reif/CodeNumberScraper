using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNumberScraper
{
    [Delimiter(";")]
    internal class Marketpartner
    {
        public string? Code { get; set; }
        public MarketpartnerKind Kind { get; set; }
        public string? CompanyName { get; set; }
        public string? Role { get; set; }
        public DateTime ValidFrom { get; set; }
        public string? InternalCode { get; set; }
    }
}
