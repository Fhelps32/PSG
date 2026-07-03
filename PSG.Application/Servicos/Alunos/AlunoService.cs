using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Domain;

namespace PSG.Application.Servicos.Alunos
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

        public async Task<AlunoDto> ObterAlunoPorMatriculaAsync(string matricula)
        {
            var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Matricula == matricula);
            if (aluno == null)
            {
                throw new Exception($"Aluno com matrícula {matricula} não encontrado.");
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

        public async Task<IEnumerable<AlunoDto>> ObterAlunosPorCursoAsync(int idCurso)
        {
            var alunos = await _context.Alunos
                .Where(a => a.IdCurso == idCurso)
                .Select(a => new AlunoDto(
                    a.IdAluno,
                    a.IdCurso,
                    a.Matricula,
                    a.Nome
                ))
                .ToListAsync();
            return alunos;
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

        public async Task AtualizarAlunoAsync(int idAluno, AlunoDtoCriar alunoDto)
        {
            var aluno = await _context.Alunos.FindAsync(idAluno);
            if (aluno == null)
            {
                throw new Exception($"Aluno com ID {idAluno} não encontrado.");
            }
            var curso = await _context.Cursos.FindAsync(alunoDto.IdCurso);
            if (curso == null)
            {
                throw new Exception($"Curso com ID {alunoDto.IdCurso} não encontrado.");
            }
            aluno.IdCurso = alunoDto.IdCurso;
            aluno.Matricula = alunoDto.Matricula;
            aluno.Nome = alunoDto.Nome;
            _context.Alunos.Update(aluno);
            await _context.SaveChangesAsync();
        }

        public async Task ExcluirAlunoAsync(int idAluno)
        {
            var aluno = await _context.Alunos.FindAsync(idAluno);
            if (aluno == null)
            {
                throw new Exception($"Aluno com ID {idAluno} não encontrado.");
            }
            if (!aluno.Status)
            {
                throw new Exception($"O Aluno com ID {idAluno} já está inativo.");
            }

            aluno.SwitchStatus();
            _context.Alunos.Update(aluno);
            await _context.SaveChangesAsync();
        }
}}
