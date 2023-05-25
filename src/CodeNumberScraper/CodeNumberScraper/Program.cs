using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text;

namespace CodeNumberScraper
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<Options>(args);
         
            DVGW.DvgwScraper scraper = new DVGW.DvgwScraper();
            var marketPartners = await scraper.FetchMarketpartners();

            var csvConfig = new CsvConfiguration(CultureInfo.CurrentCulture) { Delimiter = ";", Encoding = Encoding.UTF8 };
            using (var writer = new StreamWriter(@"C:\temp\dvgw_codes.csv"))
            using (var csv = new CsvWriter(writer, csvConfig))
            {
                csv.WriteRecords(marketPartners);
            }
        }
    }
}