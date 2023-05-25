using CommandLine;

namespace CodeNumberScraper
{
    
    internal class Options
    {
        [Option('o', "out", HelpText = "Specifies the output path. Example: /marketpartners.csv The value is Optional and the default-value ist marketpartners.csv", Required = false)]
        public string? Out { get; set; }
    }
}