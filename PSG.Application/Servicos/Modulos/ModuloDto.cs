using PSG.Application.Servicos.Alunos;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Modulos
{
    public sealed record ModuloDto(
        int IdModulo,
        int IdCurso,
        string Nome,
        int Numero
    );

    public sealed record ModuloDtoDetalhado(
        int IdModulo,
        int IdCurso,
        string Nome,
        int Numero,
        DateTime DataCadastro,
        Curso Curso,
        IEnumerable<AlunoModuloDto> Alunos
    );

    public sealed record ModuloDtoCriar(
        Curso Curso,
        string Nome,
        int Numero
    );
}
