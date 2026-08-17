using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PSG.Presentation.Models.Curso
{
    /// <summary>
    /// Formulário da modal de edição de módulo, aberta pela tela de Cursos.
    /// </summary>
    public class ModuloEditVM
    {
        public int IdModulo { get; set; }

        // Volta no POST para o redirect/refresh saber qual curso recarregar.
        public int IdCurso { get; set; }

        // Só contexto na modal (o curso do módulo não é editável aqui).
        public string NomeCurso { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o nome do módulo.")]
        [StringLength(100, ErrorMessage = "O nome pode ter no máximo 100 caracteres.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o número do módulo.")]
        [Range(1, 999, ErrorMessage = "O número deve estar entre 1 e 999.")]
        [Display(Name = "Número")]
        public int Numero { get; set; }

        // Professor que ministra o módulo. Obrigatório porque a FK é obrigatória:
        // Range a partir de 1 rejeita o "Selecione..." (que chega como 0).
        [Range(1, int.MaxValue, ErrorMessage = "Selecione o professor do módulo.")]
        [Display(Name = "Professor")]
        public int IdProfessor { get; set; }

        // Opções do dropdown de professor. Precisa ser repovoado quando a modal é
        // devolvida com erro de validação, senão o select volta vazio.
        public List<SelectListItem> Professores { get; set; } = new List<SelectListItem>();
    }
}
