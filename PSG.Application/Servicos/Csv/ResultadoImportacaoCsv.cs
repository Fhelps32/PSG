using System.Collections.Generic;

namespace PSG.Application.Servicos.Csv
{
    public class ResultadoImportacaoCsv
    {
        public int TotalLinhas { get; set; }
        public int CursosCriados { get; set; }
        public int AlunosCriados { get; set; }
        public int ModulosCriados { get; set; }
        public int RegistrosCriados { get; set; }

        public List<LinhaCsvInvalida> LinhasInvalidas { get; set; } = new();

        public int TotalLinhasInvalidas => LinhasInvalidas.Count;
    }
}
