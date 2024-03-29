using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeNumberScraper.ColumnMaps
{
    internal sealed class GermanMap : ClassMap<Marketpartner>
    {
        public GermanMap()
        {
            Map(m => m.Code).Name("Code");
            Map(m => m.Kind).Name("Ausgabestelle");
            Map(m => m.CompanyName).Name("Code-Inhaber");
            Map(m => m.Role).Name("Rolle");
            Map(m => m.ValidFrom).Name("Gültig ab");
            Map(m => m.InternalCode).Name("Interner Code");  
        }
    }
}
