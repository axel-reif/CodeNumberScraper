using CommandLine;

namespace CodeNumberScraper
{
    internal class Options
    {
        [Option]
        public string? Out { get; set; }
    }
}