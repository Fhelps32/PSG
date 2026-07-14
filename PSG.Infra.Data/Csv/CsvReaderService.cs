using CsvHelper.Configuration;
using PSG.Application.Interfaces;
using PSG.Application.Servicos.Csv;
using PSG.Infra.Data.Csv.Maps;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Infra.Data.Csv
{
    public class CsvReaderService : ICsvReaderService
    {
        public List<LinhaCsvDto> LerCsv(Stream stream)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                MissingFieldFound = null
            };

            using var reader = new StreamReader(stream);
            using var csv = new CsvHelper.CsvReader(reader, config);
            csv.Context.RegisterClassMap<LinhaCsvMap>();

            return csv.GetRecords<LinhaCsvDto>().ToList();
        }
    }
}
