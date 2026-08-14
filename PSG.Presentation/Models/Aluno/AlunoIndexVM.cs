using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Application.Servicos.Alunos;
using PSG.Domain.Enum;

namespace PSG.Presentation.Models.Aluno
{
    public class AlunoIndexVM
    {
        public List<AlunoItem> Alunos { get; set; } = new List<AlunoItem>();
        public AlunoDetalhes AlunoDetalhes { get; set; } = new AlunoDetalhes();

        // Filtros escolhidos: preenchem o form de novo e acompanham os links de
        // paginação e de seleção de aluno, para não se perderem na navegação.
        public string? Busca { get; set; }
        public int? CursoId { get; set; }
        public int? ModuloId { get; set; }

        // Opções dos dropdowns de filtro
        public List<SelectListItem> Cursos { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Modulos { get; set; } = new List<SelectListItem>();

        // Paginação (vinda do PagedResult do service)
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public bool TemPaginaAnterior { get; set; }
        public bool TemProximaPagina { get; set; }
    }

    public class AlunoItem
    {
        public int IdAluno { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Matricula { get; set; }
        public string NomeCurso { get; set; } = string.Empty;
        // Null quando o aluno não tem nenhum módulo em andamento.
        public string? ModuloAtual { get; set; }
        public EnumStatusAluno Status { get; set; }
    }

    public class AlunoDetalhes
    {
        public int IdAluno { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Matricula { get; set; }
        public string NomeCurso { get; set; } = string.Empty;
        public EnumStatusAluno Status { get; set; }

        // "Módulos 4/10": concluídos sobre o total de módulos ativos do curso.
        public int ModulosConcluidos { get; set; }
        public int TotalModulosCurso { get; set; }

        public List<ModuloAlunoItem> Modulos { get; set; } = new List<ModuloAlunoItem>();
        public List<ModuloAlunoItem> ModulosReprovados { get; set; } = new List<ModuloAlunoItem>();
    }

    public class ModuloAlunoItem
    {
        public string Nome { get; set; } = string.Empty;
        public int Numero { get; set; }
        public DateTime? DataInscricao { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal Nota { get; set; }
        public EnumStatus Status { get; set; }
    }
}
