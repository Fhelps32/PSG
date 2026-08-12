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
    }
}
