using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Servicos.Shared;
using PSG.Domain;
using PSG.Domain.Enum;

namespace PSG.Application.Servicos.Alunos
{
    public class AlunoService
    {
        private readonly IPSGDbContext _context;

        public AlunoService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<Aluno> ObterAlunoPorIdAsync(int idAluno)
        {
            var aluno = await _context.Alunos.FindAsync(idAluno);
            if (aluno == null)
            {
                throw new Exception($"Aluno com ID {idAluno} não encontrado.");
            }
                return aluno;
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

        public async Task<PagedResult<Aluno>> ObterAlunosPorCursoAsync(int idCurso, int pagina)
        {
            var query = _context.Alunos
                .Where(a => a.IdCurso == idCurso)
                .AsQueryable();
            var result = await query.Paginar<Aluno>(new PaginationRequest { NumeroPagina = pagina, TamanhoPagina = 20 });
            return result;
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

        public async Task<int> ObterQuantidadeTotalAlunosAsync()
        {
            return await _context.Alunos.CountAsync();
        }

        public async Task<AlunoQuantidadeCursoDto> ObterQuantidadeAlunosPorCursoAsync(int idCurso)
        {
            var query = _context.Alunos.AsQueryable();
            var quantidadeAlunos = await query
                .Where(a => a.IdCurso == idCurso)
                .CountAsync();
            var nomeCurso = (await _context.Cursos.FindAsync(idCurso))?.Nome;
            return new AlunoQuantidadeCursoDto(quantidadeAlunos, nomeCurso!, DateTime.Now);
        }

        public async Task<List<AlunoQuantidadeCursoDto>> ObterQuantidadeAlunosPorCursoAsync()
        {
            var dados = await _context.Cursos
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Nome, Total = c.Alunos.Count() })
                .ToListAsync();

            return dados
                .Select(d => new AlunoQuantidadeCursoDto(d.Total, d.Nome, DateTime.Now))
                .ToList();
        }

        public async Task<List<AlunoQuantidadeModuloDto>> ObterQuantidadeAlunosPorModuloAsync(int idCurso)
        {
            var dados = await _context.Modulos
                .Where(m => m.IdCurso == idCurso)
                .OrderBy(m => m.Numero)
                .Select(m => new
                {
                    NomeCurso = m.Curso.Nome,
                    NomeModulo = m.Nome,
                    Total = m.Alunos.Count(am => am.Status)
                })
                .ToListAsync();

            return dados
                .Select(d => new AlunoQuantidadeModuloDto(d.Total, d.NomeCurso, d.NomeModulo))
                .ToList();
        }

        // Não existe uma "DataCancelamento" na entidade: a conclusão é o momento em que a
        // inscrição saiu do ar; quando ela não existe, o acesso é a melhor aproximação.
        public async Task<List<AlunoCanceladoDto>> ObterAlunosCanceladosAsync(int quantidade = 10)
        {
            var dados = await _context.AlunoModulos
                .Where(am => am.StatusInscricao == EnumStatus.Cancelado)
                .OrderByDescending(am => am.DataConclusao ?? am.DataAcesso)
                .Take(quantidade)
                .Select(am => new
                {
                    am.Aluno.Nome,
                    NomeCurso = am.Modulo.Curso.Nome,
                    NomeModulo = am.Modulo.Nome,
                    Data = am.DataConclusao ?? am.DataAcesso
                })
                .ToListAsync();

            return dados
                .Select(d => new AlunoCanceladoDto(d.Nome, d.NomeCurso, d.NomeModulo, d.Data))
                .ToList();
        }

        //TODO trocar o DataCadastro por DataMatricula que ainda vai ser implementado no Aluno
        public async Task<AlunoQuantidadeDto> ObterQuantidadeAlunosPorMes(DateTime mes, int? idCurso)
        {
            // Mês fechado: do dia 1 (inclusive) até o dia 1 do mês seguinte (exclusive).
            // Antes usava a data recebida como início, então a janela era do dia X ao dia X-1
            // do mês seguinte e os pontos não representavam meses de calendário.
            var dataInicio = new DateTime(mes.Year, mes.Month, 1);
            var dataFim = dataInicio.AddMonths(1);

            var query = _context.Alunos.AsQueryable();
            if (idCurso.HasValue)
            {
                query = query.Where(a => a.IdCurso == idCurso.Value);
            }
            var quantidadeAlunos = await query
                .Where(a => a.DataCadastro >= dataInicio && a.DataCadastro < dataFim)
                .CountAsync();

            return new AlunoQuantidadeDto(quantidadeAlunos, dataInicio);
        }


    }
}
