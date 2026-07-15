// Models/Incricao/InscricaoCreateViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using PSG.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace PSG.Presentation.Models.Incricao
{
    public class InscricaoCreateViewModel
    {
        [Required(ErrorMessage = "Selecione o aluno.")]
        [Display(Name = "Aluno")]
        public int AlunoId { get; set; }

        [Required(ErrorMessage = "Selecione o curso.")]
        [Display(Name = "Curso")]
        public int CursoId { get; set; }

        [Required(ErrorMessage = "Selecione o módulo.")]
        [Display(Name = "Módulo")]
        public int ModuloId { get; set; }

        [Required(ErrorMessage = "Informe a data de início.")]
        [Display(Name = "Data de início")]
        [DataType(DataType.Date)]
        public DateTime DataAcesso { get; set; } = DateTime.Today;

        [Display(Name = "Data de matrícula")]
        [DataType(DataType.Date)]
        public DateTime? DataMatricula { get; set; } = DateTime.Today;

        [Display(Name = "Status")]
        public EnumStatus StatusInscricao { get; set; } = EnumStatus.EmAndamento;

        // Dropdowns
        public List<SelectListItem> Alunos { get; set; } = new();
        public List<SelectListItem> Cursos { get; set; } = new();

        // Módulos NÃO vem populado aqui — é carregado via AJAX quando o Curso é selecionado
        public List<SelectListItem> Modulos { get; set; } = new();
    }
}
