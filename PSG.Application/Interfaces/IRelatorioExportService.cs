using PSG.Application.Servicos.Relatorios;

namespace PSG.Application.Interfaces
{
    /// <summary>
    /// Serializa um RelatorioResultadoDto já pronto (colunas + linhas) para um
    /// formato de arquivo. Fica atrás de interface para o Application não depender
    /// de bibliotecas de planilha — a implementação mora na Infra, junto de
    /// ICsvReaderService/CsvReaderService (mesmo motivo).
    /// </summary>
    public interface IRelatorioExportService
    {
        byte[] GerarCsv(RelatorioResultadoDto resultado);
        byte[] GerarXlsx(RelatorioResultadoDto resultado);
    }
}
