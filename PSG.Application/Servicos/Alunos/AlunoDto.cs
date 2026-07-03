using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Alunos
{
    public sealed record AlunoDto(
        int IdAluno,
        int IdCurso,
        string Matricula,
        string Nome
    );

    public sealed record AlunoDtoDetalhado( 
        int IdAluno,
        int IdCurso,
        string Matricula,
        string Nome,
        DateTime DataCadastro,
        IEnumerable<AlunoModuloDto> Modulos
    );

    public sealed record AlunoDtoCriar(
        int IdCurso,
        string Matricula,
        string Nome
    );

    public sealed record AlunoModuloDto(    
        int IdAlunoModulo,
        int IdAluno,
        int IdModulo,
        decimal Nota,
        EnumStatus Status
    );
}
