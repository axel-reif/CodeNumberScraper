using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using System.Globalization;
using System.Text;

namespace CodeNumberScraper
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            string filename = "marketpartners.csv";
            bool dvgw = true;
            bool bdew = true;

            var result = Parser.Default.ParseArguments<Options>(args);
            if (result != null)
            {
                if (result.Errors.Any())
                {
                    return -1;
                }
                if (result.Value != null)
                {
                    if (!string.IsNullOrEmpty(result.Value.Out))
                        filename = result.Value.Out;
                }
            }

            var logger = CreateLogger();

            List<Marketpartner> marketpartners = new List<Marketpartner>();
            
            if (dvgw)
            {
                DVGW.DvgwScraper scraper = new DVGW.DvgwScraper(logger);
                marketpartners.AddRange(await scraper.FetchMarketpartners());
            }
            
            if (bdew)
            {
                BDEW.BdewScraper bdewScraper = new BDEW.BdewScraper(logger);
                marketpartners.AddRange(await bdewScraper.FetchMarketpartners());
            }

            Console.WriteLine("Writing CSV");
            MarketpartnerWriter writer = new MarketpartnerWriter();

            try
            {
                await writer.Write(filename, marketpartners);
            }
            catch (Exception ex)
            {
                logger.LogCritical("Cannot write csv: " + ex.Message);
                return -1;
            }

            logger.LogInformation("Done.");
            return 0;
        }

        static ILogger CreateLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("CodeNumberScraper.Program", LogLevel.Debug)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger("CodeNumberScraper");
            return logger;
        }
    }
}