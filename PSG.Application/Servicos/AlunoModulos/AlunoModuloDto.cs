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
}
