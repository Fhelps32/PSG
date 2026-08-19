using Microsoft.AspNetCore.Mvc.Rendering;

namespace PSG.Presentation.Models.Relatorio
{
    public class RelatorioIndexVM
    {
        // Opções dos selects de filtro (curso e aluno específico).
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Alunos { get; set; } = new List<SelectListItem>();

        // Prévia com os filtros padrão, calculada no servidor para a página já
        // carregar com uma tabela de exemplo — depois disso quem atualiza é o fetch.
        public RelatorioResultadoVM Preview { get; set; } = new RelatorioResultadoVM();
    }
}
