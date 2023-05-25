using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeNumberScraper.BDEW
{
    internal class BdewScraper : ScraperBase
    {
        public BdewScraper(ILogger logger, int maxRowCount = 8000) : base(logger)
        {
            MaxRowCount = maxRowCount;
        }

        public int MaxRowCount { get; }

        /*
         * How the site works:
         * We just need to make a POST-request to the following endpoint: https://bdew-codes.de/Codenumbers/BDEWCodes/GetCompanyList?jtStartIndex=0&jtPageSize=5000
         * There are currently ~ 4500 marketpartners. You can just query anything larger than that and it will return all of them
         * 
         * This returns a json similar to this:
         * {
            "Result": "OK",
            "Records": [
                {
                    "Id": 1500,
                    "CompanyUId": 661394,
                    "Company": " K+S Minerals and Agriculture GmbH"
                }
            ],
            "TotalRecordCount": 4477
            }
         * Next thing we need to do is query the endpoint with POST: https://bdew-codes.de/Codenumbers/BDEWCodes/GetBdewCodeListOfCompany?companyId=1500&filter=
         * Where companyId is the Id of the marketpartner from the first request.
         * That results in a json similar to this one:
         * {
            "Result": "OK",
            "Records": [
               {
                  "Id": 2801,
                  "CompanyUId": 661394,
                  "BdewCode": "9904843000006",
                  "MarketFunctionName": "Netznutzer ohne All-Inklusiv-Vertrag",
                  "ContactName": "Herber, Carsten"
               },
               {
                  "Id": 24117,
                  "CompanyUId": 661394,
                  "BdewCode": "9978375000008",
                  "MarketFunctionName": "Einsatzverantwortlicher",
                  "ContactName": "Herber, Carsten"
               }
            ]
           }
         * Now we can create instances of Marketpartner with the gathered data
         */

        public async Task<IEnumerable<Marketpartner>> FetchMarketpartners()
        {
            Logger.LogInformation("Executing BDEW-Codenumber Scraper.");

            List<Marketpartner> outList = new List<Marketpartner>();

            using (var httpClient = CreateHttpClient())
            {
                string pageSize = MaxRowCount.ToString();
                string endpoint = "https://bdew-codes.de/Codenumbers/BDEWCodes/GetCompanyList?jtStartIndex=0&jtPageSize=" + pageSize;

                Logger.LogInformation($"Fetching header rows. Pagesize: {pageSize}");

                var response = await PostAsyncWithRetry(httpClient, endpoint, new StringContent(""), 2);
                if (LogIfTrue(!response.IsSuccessStatusCode, $"Error fetching header data. Status-Code: {response.StatusCode}"))
                    return outList;
                
                var headerJsonString = await response.Content.ReadAsStringAsync();
                var headerJson = JsonObject.Parse(headerJsonString);

                if(LogIfNull(headerJson, $"Cannot parse response JSON. Json: {headerJsonString}"))
                    return outList; 

                var companyHeaderArray = headerJson["Records"]?.AsArray();
                if (LogIfNull(companyHeaderArray, $"Cannot parse response JSON. Records-Node is missing. Json: {headerJsonString}"))
                    return outList;

                Logger.LogInformation($"Got {companyHeaderArray.Count} company headers.");

                int rowCount = 0;
                foreach (var headerRow in companyHeaderArray)
                {
                    if (headerRow == null)
                        continue;

                    var id = headerRow["Id"]?.GetValue<long>();
                    var name = headerRow["Company"]?.GetValue<string>();
                    name = name?.Trim().Replace("\"", string.Empty);

                    //Now query the details
                    var detailsResponse = await PostAsyncWithRetry(
                        httpClient,
                        $"https://bdew-codes.de/Codenumbers/BDEWCodes/GetBdewCodeListOfCompany?companyId={id}&filter=",
                        new StringContent(string.Empty),
                        3);

                    if (LogIfTrue(!detailsResponse.IsSuccessStatusCode, $"Error fetching details data. Status-Code: {detailsResponse.StatusCode} Company-Id: {id} Company-Name: {name}"))
                        continue;
                    
                    var detailJsonString = await detailsResponse.Content.ReadAsStringAsync();
                    var detailJson = JsonObject.Parse(detailJsonString);

                    if (LogIfNull(detailJson, $"Cannot parse response Detail-JSON. CompanyId: {id} Company: {name} Json: {detailJsonString}"))
                        continue;

                    var detailHeaderArray = detailJson["Records"]?.AsArray();
                    if (LogIfNull(detailHeaderArray, $"Cannot parse response JSON. Records-Node is missing. CompanyId: {id} Company: {name} Json: {detailJsonString}"))
                        continue;

                    foreach (var detailRow in detailHeaderArray)
                    {
                        rowCount++;

                        if (detailRow == null)
                            continue;

                        Marketpartner mp = new Marketpartner()
                        {
                            InternalCode = id.ToString(),
                            Code = detailRow["BdewCode"]?.GetValue<string>(),
                            CompanyName = name,
                            Kind = MarketpartnerKind.Bdew,
                            Role = detailRow["MarketFunctionName"]?.GetValue<string>(),
                            ValidFrom = new DateTime(1900, 1, 1)
                        };
                        
                        outList.Add(mp);

                        if (rowCount % 100 == 0)
                            Logger.LogInformation($"Processed {rowCount} marketpartners.");
                    }
                }
                
                return outList;
            }
        }


    }
}
