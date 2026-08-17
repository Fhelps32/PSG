using PSG.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace PSG.Presentation.Models.Incricao
{
    /// <summary>
    /// Formulário da modal de edição de inscrição, aberta pela tela de Inscrições.
    /// Aluno, curso e módulo vêm só como contexto: trocá-los seria outra inscrição.
    /// </summary>
    public class InscricaoEditViewModel : IValidatableObject
    {
        public int IdAlunoModulo { get; set; }

        // Contexto exibido na modal (não editável).
        public string NomeAluno { get; set; } = string.Empty;
        public string NomeCurso { get; set; } = string.Empty;
        public string NomeModulo { get; set; } = string.Empty;
        public int NumeroModulo { get; set; }

        [Required(ErrorMessage = "Informe a data de início.")]
        [Display(Name = "Data de início")]
        [DataType(DataType.Date)]
        public DateTime DataInicio { get; set; }

        [Display(Name = "Data de fim")]
        [DataType(DataType.Date)]
        public DateTime? DataFim { get; set; }

        [Display(Name = "Data de matrícula")]
        [DataType(DataType.Date)]
        public DateTime? DataMatricula { get; set; }

        [Range(0, 10, ErrorMessage = "A nota deve estar entre 0 e 10.")]
        [Display(Name = "Nota")]
        public decimal Nota { get; set; }

        [Display(Name = "Status")]
        public EnumStatus StatusInscricao { get; set; }

        [StringLength(500, ErrorMessage = "A observação pode ter no máximo 500 caracteres.")]
        [Display(Name = "Observação")]
        public string? ObsGeral { get; set; }

        /// <summary>
        /// Regras que dependem de mais de um campo. Ficam aqui para valerem tanto no
        /// POST de edição quanto em qualquer outro lugar que use esta VM.
        /// </summary>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (DataFim.HasValue && DataFim.Value.Date < DataInicio.Date)
            {
                yield return new ValidationResult(
                    "A data de fim não pode ser anterior à data de início.",
                    new[] { nameof(DataFim) });
            }

            if (DataMatricula.HasValue && DataMatricula.Value.Date > DataInicio.Date)
            {
                yield return new ValidationResult(
                    "A data de matrícula não pode ser posterior à data de início.",
                    new[] { nameof(DataMatricula) });
            }

            // Status concluído sem data de fim deixa a listagem com "—" na coluna
            // Data Fim, o que confunde na hora de conferir a turma.
            var concluido = StatusInscricao is EnumStatus.Aprovado or EnumStatus.Reprovado;
            if (concluido && !DataFim.HasValue)
            {
                yield return new ValidationResult(
                    "Inscrição aprovada ou reprovada precisa de data de fim.",
                    new[] { nameof(DataFim) });
            }
        }
    }
}
