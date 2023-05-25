using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeNumberScraper.DVGW
{
    internal class DvgwScraper : ScraperBase
    {
        // Testing showed that it is important to set the following headers for the request to work:
        // Content-Type: application/x-www-form-urlencoded; charset=UTF-8
        // Accept: application/json, text/javascript, */*; q=0.01
        // Accept-Encoding: gzip, deflate, br

        /*
         * How the site works:
         * First there is a POST-request to https://codevergabe.dvgw-sc.de/MarketParticipants/GetCodeCompanies
         * The body of the request is simply: draw=1&columns%5B0%5D%5Bdata%5D=Name&columns%5B0%5D%5Bname%5D=&columns%5B0%5D%5Bsearchable%5D=true&columns%5B0%5D%5Borderable%5D=true&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B1%5D%5Bdata%5D=ZipCode&columns%5B1%5D%5Bname%5D=&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5Borderable%5D=true&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5Bdata%5D=City&columns%5B2%5D%5Bname%5D=&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=true&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=&columns%5B3%5D%5Bname%5D=&columns%5B3%5D%5Bsearchable%5D=true&columns%5B3%5D%5Borderable%5D=false&columns%5B3%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=&columns%5B4%5D%5Bname%5D=&columns%5B4%5D%5Bsearchable%5D=true&columns%5B4%5D%5Borderable%5D=false&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B4%5D%5Bsearch%5D%5Bregex%5D=false&order%5B0%5D%5Bcolumn%5D=0&order%5B0%5D%5Bdir%5D=asc&start=0&length=10&search%5Bvalue%5D=&search%5Bregex%5D=false&selectedMarketFunction=
         * Which is URL-encoded and looks decoded like this:
         * draw=1&columns[0][data]=Name&columns[0][name]=&columns[0][searchable]=true&columns[0][orderable]=true&columns[0][search][value]=&columns[0][search][regex]=false&columns[1][data]=ZipCode&columns[1][name]=&columns[1][searchable]=true&columns[1][orderable]=true&columns[1][search][value]=&columns[1][search][regex]=false&columns[2][data]=City&columns[2][name]=&columns[2][searchable]=true&columns[2][orderable]=true&columns[2][search][value]=&columns[2][search][regex]=false&columns[3][data]=&columns[3][name]=&columns[3][searchable]=true&columns[3][orderable]=false&columns[3][search][value]=&columns[3][search][regex]=false&columns[4][data]=&columns[4][name]=&columns[4][searchable]=true&columns[4][orderable]=false&columns[4][search][value]=&columns[4][search][regex]=false&order[0][column]=0&order[0][dir]=asc&start=0&length=10&search[value]=&search[regex]=false&selectedMarketFunction=
         * This looks like as if you could query various columns with the request. However the default looks like the above
         * The most important bit is length=10 where you can set the length.
         * Testing showed that you can query ALL of the entries at once. So we should make one request with one entry to get the recordsTotal field and after that we'd fetch all of the rows.
         * and the result returns a json similar to this example:
         * {
            "Result":"OK",
            "draw":1,
            "recordsTotal":2085,
            "recordsFiltered":2085,
            "data":[
               {
                  "City":"Montabaur",
                  "Id":2,
                  "Location":null,
                  "Name":"1\u00261 Energy GmbH",
                  "ZipCode":"56410",
                  "HasActiveContactSheets":false
               },
               {
                  "City":"Murnau",
                  "Id":3,
                  "Location":null,
                  "Name":"17er Oberlandenergie GmbH",
                  "ZipCode":"82418",
                  "HasActiveContactSheets":false
               },
            ]
           }
        * Now a GET-request has to be made to https://codevergabe.dvgw-sc.de/MarketParticipants/GetActiveCompanyCodes?companyId=2
        * where the companyId is the Id-field from the first json
        * In our example it is 2 for 1\u00261 Energy GmbH
        * The result looks similar to this:
        * {
           "data":[
              {
                 "Id":2,
                 "MarketFunction":"Bilanzkreisverantwortlicher",
                 "CodeType":"DVGW",
                 "Code":"9800423500004",
                 "CodeTypeEnum":1,
                 "LocalizedStatus":"Aktiv",
                 "CodeState":2,
                 "DateFrom":"\/Date(1457942880000)\/",
                 "DateTo":null,
                 "StatusLocalizationKey":"DVGW_Code_Status_Active"
              },
              {
                 "Id":3,
                 "MarketFunction":"Lieferant / Transportkunde",
                 "CodeType":"DVGW",
                 "Code":"9800423600002",
                 "CodeTypeEnum":1,
                 "LocalizedStatus":"Aktiv",
                 "CodeState":2,
                 "DateFrom":"\/Date(1458228660000)\/",
                 "DateTo":null,
                 "StatusLocalizationKey":"DVGW_Code_Status_Active"
              }
           ],
           "ErrorInfo":[
              
           ],
           "HasErrors":false,
           "ResultDataModel":[
              {
                 "Id":2,
                 "MarketFunction":"Bilanzkreisverantwortlicher",
                 "CodeType":"DVGW",
                 "Code":"9800423500004",
                 "CodeTypeEnum":1,
                 "LocalizedStatus":"Aktiv",
                 "CodeState":2,
                 "DateFrom":"\/Date(1457942880000)\/",
                 "DateTo":null,
                 "StatusLocalizationKey":"DVGW_Code_Status_Active"
              },
              {
                 "Id":3,
                 "MarketFunction":"Lieferant / Transportkunde",
                 "CodeType":"DVGW",
                 "Code":"9800423600002",
                 "CodeTypeEnum":1,
                 "LocalizedStatus":"Aktiv",
                 "CodeState":2,
                 "DateFrom":"\/Date(1458228660000)\/",
                 "DateTo":null,
                 "StatusLocalizationKey":"DVGW_Code_Status_Active"
              }
           ],
           "StatusOperation":false,
           "TotalDisplayRecords":2,
           "TotalRecords":2
         }
        Now all we need to do is to create Marketpartner-instances with all the gathered data.
        */
        public DvgwScraper(ILogger logger, int maxRowCount = 8000) : base(logger)
        {
            MaxRowCount = maxRowCount;
        }

        public int MaxRowCount { get; }

        public async Task<IEnumerable<Marketpartner>> FetchMarketpartners()
        {
            Logger.LogInformation("Executing DVGW-Codenumber Scraper.");

            List<Marketpartner> outList = new List<Marketpartner>();

            using (var httpClient = CreateHttpClient())
            {
                string endpoint = "https://codevergabe.dvgw-sc.de/MarketParticipants/GetCodeCompanies";

                string requestBody = @"draw=1&columns%5B0%5D%5Bdata%5D=Name&columns%5B0%5D%5Bname%5D=&columns%5B0%5D%5Bsearchable%5D=true&columns%5B0%5D%5Borderable%5D=true&columns%5B0%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B0%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B1%5D%5Bdata%5D=ZipCode&columns%5B1%5D%5Bname%5D=&columns%5B1%5D%5Bsearchable%5D=true&columns%5B1%5D%5Borderable%5D=true&columns%5B1%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B1%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B2%5D%5Bdata%5D=City&columns%5B2%5D%5Bname%5D=&columns%5B2%5D%5Bsearchable%5D=true&columns%5B2%5D%5Borderable%5D=true&columns%5B2%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B2%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B3%5D%5Bdata%5D=&columns%5B3%5D%5Bname%5D=&columns%5B3%5D%5Bsearchable%5D=true&columns%5B3%5D%5Borderable%5D=false&columns%5B3%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B3%5D%5Bsearch%5D%5Bregex%5D=false&columns%5B4%5D%5Bdata%5D=&columns%5B4%5D%5Bname%5D=&columns%5B4%5D%5Bsearchable%5D=true&columns%5B4%5D%5Borderable%5D=false&columns%5B4%5D%5Bsearch%5D%5Bvalue%5D=&columns%5B4%5D%5Bsearch%5D%5Bregex%5D=false&order%5B0%5D%5Bcolumn%5D=0&order%5B0%5D%5Bdir%5D=asc&start=0&length=***LENGTH***&search%5Bvalue%5D=&search%5Bregex%5D=false&selectedMarketFunction=";
                requestBody = requestBody.Replace("***LENGTH***", MaxRowCount.ToString());
                var content = new StringContent(requestBody);
                content.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");

                int rowCount = 0;

                Logger.LogInformation($"Fetching header rows. Pagesize: {MaxRowCount}");

                var response = await PostAsyncWithRetry(httpClient, endpoint, content, 3);
                if (LogIfTrue(!response.IsSuccessStatusCode, $"Error fetching data from DVGW. Status code: {response.StatusCode}"))
                    return outList;

                var responseContent = response.Content;

                string jsonString = await responseContent.ReadAsStringAsync();
                var json = JsonObject.Parse(jsonString);
                if (json != null)
                {
                    var dataArray = json["data"]?.AsArray();
                    if (dataArray != null)
                    {
                        Console.WriteLine($"Got {dataArray.Count} header-rows.");

                        foreach (var dataNode in dataArray)
                        {
                            var id = dataNode["Id"]?.GetValue<int>().ToString();
                            var name = dataNode["Name"]?.GetValue<string>();

                            var detailsResponse = await GetAsyncWithRetry(httpClient, $"https://codevergabe.dvgw-sc.de/MarketParticipants/GetActiveCompanyCodes?companyId={id}", 3);
                            if (LogIfTrue(!detailsResponse.IsSuccessStatusCode, $"Error fetching data from DVGW. Status code: {detailsResponse.StatusCode} Company Id: {id} Company name: {name}"))
                                continue;

                            var detailsJsonString = await detailsResponse.Content.ReadAsStringAsync();
                            var detailsJson = JsonObject.Parse(detailsJsonString);
                            if (LogIfNull(detailsJson, $"Error Parsing JSON: {detailsJsonString}"))
                                continue;

                            var detailsDataArray = detailsJson["data"]?.AsArray();
                            if (LogIfNull(detailsDataArray, $"Missing data Element in json: {detailsJsonString}"))
                                continue;

                            foreach (var detailData in detailsDataArray)
                            {
                                rowCount++;

                                Marketpartner mp = new Marketpartner()
                                {
                                    InternalCode = id,
                                    CompanyName = name,
                                    Code = detailData["Code"]?.GetValue<string>(),
                                    Role = detailData["MarketFunction"]?.GetValue<string>(),
                                    Kind = MarketpartnerKind.Dvgw
                                };
                                // Try parse the date
                                var dateString = detailData["DateFrom"]?.GetValue<string>();
                                if (!string.IsNullOrEmpty(dateString))
                                {
                                    // Looks like this: \/Date(1458228660000)\/
                                    // We parse just the numbers:
                                    string dateValueString = Regex.Match(dateString, "(?<date>\\d+)").Groups["date"].Value;
                                    if (!string.IsNullOrEmpty(dateValueString))
                                    {
                                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(dateValueString));
                                        mp.ValidFrom = dateTimeOffset.LocalDateTime;
                                    }
                                }
                                outList.Add(mp);
                                if (rowCount % 100 == 0)
                                    Logger.LogInformation($"Processed {rowCount} marketpartners.");
                            }
                        }
                        Logger.LogInformation($"Total marketpartners added: {rowCount}");
                        return outList;
                    }
                }

                return outList;
            }
        }
    }
}