using PSG.Application.Servicos.Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Interfaces
{
    public interface ICsvReaderService
    {
        List<LinhaCsvDto> LerCsv(Stream stream);
    }
}
