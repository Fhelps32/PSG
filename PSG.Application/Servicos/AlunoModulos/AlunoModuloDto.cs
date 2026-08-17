using PSG.Domain;
using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.AlunoModulos
{
    public record AlunoModuloDto
    {

    }

    public sealed record AlunoModuloDtoCriar
    (
        Aluno Aluno,
        Modulo Modulo,
        EnumStatus EnumStatus,
        DateTime DataInicio,
        DateTime? DataFim
    );

    /// <summary>
    /// Campos editáveis de uma inscrição (usado pela modal de edição da tela de
    /// Inscrições). Aluno, curso e módulo não entram: trocá-los seria outra
    /// inscrição, não uma edição desta.
    /// </summary>
    public sealed record AlunoModuloDtoAtualizar
    (
        DateTime DataInicio,
        DateTime? DataFim,
        DateTime? DataMatricula,
        decimal Nota,
        EnumStatus EnumStatus,
        string? ObsGeral
    );

    /// <summary>
    /// Dados que a modal de edição precisa exibir: os campos editáveis mais o
    /// aluno, o curso e o módulo, que aparecem como contexto.
    /// </summary>
    public sealed record InscricaoEdicaoDto
    (
        int IdAlunoModulo,
        string NomeAluno,
        string NomeCurso,
        string NomeModulo,
        int NumeroModulo,
        DateTime DataInicio,
        DateTime? DataFim,
        DateTime? DataMatricula,
        decimal Nota,
        EnumStatus EnumStatus,
        string? ObsGeral
    );
}
