using CsvHelper.Configuration;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNumberScraper
{
    internal class MarketpartnerWriter
    {
        public MarketpartnerWriter()
        {
            
        }

        public async Task Write(string path, IEnumerable<Marketpartner> marketpartners)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                await csv.WriteRecordsAsync(marketpartners);
            }
        }
    }
}
