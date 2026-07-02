using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Aluno
{
    public class AlunoService
    {
        private readonly IPSGDbContext _context;

        public AlunoService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<AlunoDto> ObterAlunoPorIdAsync(int idAluno)
        {
            var aluno = await _context.Alunos.FindAsync(idAluno);
            if (aluno == null)
            {
                throw new Exception($"Aluno com ID {idAluno} não encontrado.");
            }

            var alunoDto = new AlunoDto(
                aluno.IdAluno,
                aluno.IdCurso,
                aluno.Matricula,
                aluno.Nome
            );

            return alunoDto;
        }

        public async Task<AlunoDtoDetalhado> ObterAlunoDetalhadoPorIdAsync(int idAluno)
        {
            var alunoDto = await _context.Alunos.Include(a => a.Modulos)
                .Where(a => a.IdAluno == idAluno)
                .Select(a => new AlunoDtoDetalhado(
                    a.IdAluno,
                    a.IdCurso,
                    a.Matricula,
                    a.Nome,
                    a.DataCadastro,
                    a.Status,
                    a.Modulos.Select(am => new AlunoModuloDto(
                        am.IdAlunoModulo,
                        am.IdAluno,
                        am.IdModulo,
                        am.Nota,
                        am.StatusInscricao
                    ))
                ))
                .FirstOrDefaultAsync();

            if (alunoDto == null)
            {
                throw new Exception($"Aluno com ID {idAluno} não encontrado.");
            }
            return alunoDto;
        }

        public async Task<AlunoDto> CriarAlunoAsync(AlunoDtoCriar alunoDto)
        {
            var curso = await _context.Cursos.FindAsync(alunoDto.IdCurso);
            if (curso == null)
            {
                throw new Exception($"Curso com ID {alunoDto.IdCurso} não encontrado.");
            }
            var aluno = new Aluno(curso, alunoDto.Matricula, alunoDto.Nome);
            _context.Alunos.Add(aluno);
            await _context.SaveChangesAsync();
            return new AlunoDto(
                aluno.IdAluno,
                aluno.IdCurso,
                aluno.Matricula,
                aluno.Nome
            );
        }

        public async Task<IEnumerable<AlunoDto>> ObterTodosAlunosAsync()
        {
            var alunos = _context.Alunos.ToList();
            var dtos = alunos.Select(aluno => new AlunoDto(
                aluno.IdAluno,
                aluno.IdCurso,
                aluno.Matricula,
                aluno.Nome
            ));
            return await Task.FromResult(dtos);
        }
    }
}
