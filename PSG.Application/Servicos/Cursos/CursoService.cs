using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Domain;
using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Cursos
{
    public class CursoService
    {
        private readonly IPSGDbContext _context;

        public CursoService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<List<Curso>> ObterTodosOsCursosAsync()
        {
            return await Task.FromResult(_context.Cursos.ToList());
        }

        public async Task<int> ObterQuantidadeTotalCursosAsync()
        {
            return await _context.Cursos.CountAsync();
        }

        /// <summary>
        /// Curso com a maior proporção de inscrições no status informado
        /// (ex.: Cancelado para taxa de cancelamento, Reprovado para taxa de reprovação).
        /// Retorna null quando nenhum curso tem inscrições nesse status.
        /// </summary>
        public async Task<CursoTaxaDto?> ObterCursoComMaiorTaxaAsync(EnumStatus status)
        {
            var dados = await _context.Cursos
                .Select(c => new
                {
                    c.Nome,
                    Total = c.Modulos.SelectMany(m => m.Alunos).Count(),
                    Importancia = c.Modulos.SelectMany(m => m.Alunos).Count(am => am.StatusInscricao == status)
                })
                .Where(x => x.Total > 0 && x.Importancia > 0)
                .ToListAsync();

            var maior = dados
                .OrderByDescending(x => (double)x.Importancia / x.Total)
                .ThenByDescending(x => x.Importancia)
                .FirstOrDefault();

            if (maior is null)
            {
                return null;
            }

            var taxa = (int)Math.Round(100d * maior.Importancia / maior.Total);
            return new CursoTaxaDto(maior.Nome, taxa, maior.Importancia, maior.Total);
        }

        /// <summary>
        /// Lista os cursos para a tela de Cursos: nome, total de alunos e taxa de
        /// cancelamento de cada um, ordenados por nome.
        /// </summary>
        /// <remarks>
        /// Devolve a lista inteira porque a busca da tela é feita no cliente,
        /// filtrando conforme o usuário digita.
        /// Contagens iguais às já usadas no dashboard, para os números baterem entre as telas:
        /// total de alunos = alunos vinculados ao curso (mesma regra de
        /// AlunoService.ObterQuantidadeAlunosPorCursoAsync).
        /// Cancelado = aluno com ao menos uma inscrição no status Cancelado.
        /// </remarks>
        public async Task<List<CursoListagemDto>> ObterCursosParaListagemAsync()
        {
            var dados = await _context.Cursos
                .OrderBy(c => c.Nome)
                .Select(c => new
                {
                    c.IdCurso,
                    c.Nome,
                    NomeCoordenador = c.Coordenador.Nome,
                    Total = c.Alunos.Count(),
                    Cancelados = c.Alunos.Count(a => a.Modulos.Any(am => am.StatusInscricao == EnumStatus.Cancelado))
                })
                .ToListAsync();

            return dados
                .Select(d => new CursoListagemDto(
                    d.IdCurso,
                    d.Nome,
                    // Sigla ainda não existe no Domain.
                    Sigla: null,
                    d.NomeCoordenador,
                    d.Total,
                    CalcularTaxa(d.Cancelados, d.Total)))
                .ToList();
        }

        /// <summary>
        /// Detalhes do curso selecionado na tela de Cursos: total de alunos, quantos
        /// cancelaram/reprovaram, as taxas correspondentes e a lista de módulos com a
        /// quantidade de alunos de cada um (ordenada pelo número do módulo).
        /// Retorna null quando o curso não existe.
        /// </summary>
        /// <remarks>
        /// A contagem por módulo considera só as inscrições ativas (AlunoModulo.Status),
        /// mesma regra de AlunoService.ObterQuantidadeAlunosPorModuloAsync.
        /// Atenção: a taxa aqui é sobre ALUNOS do curso, enquanto
        /// ObterCursoComMaiorTaxaAsync (usada no dashboard) é sobre INSCRIÇÕES —
        /// os percentuais das duas telas podem divergir.
        /// </remarks>
        public async Task<CursoDetalhesDto?> ObterDetalhesDoCursoAsync(int idCurso)
        {
            var dados = await _context.Cursos
                .Where(c => c.IdCurso == idCurso)
                .Select(c => new
                {
                    c.IdCurso,
                    c.Nome,
                    Total = c.Alunos.Count(),
                    Cancelados = c.Alunos.Count(a => a.Modulos.Any(am => am.StatusInscricao == EnumStatus.Cancelado)),
                    Reprovados = c.Alunos.Count(a => a.Modulos.Any(am => am.StatusInscricao == EnumStatus.Reprovado)),
                    Modulos = c.Modulos
                        // Módulos excluídos (Status = false) somem da lista.
                        .Where(m => m.Status)
                        .OrderBy(m => m.Numero)
                        .Select(m => new
                        {
                            m.IdModulo,
                            m.Nome,
                            m.Numero,
                            NomeProfessor = m.Professor.Nome,
                            Total = m.Alunos.Count(am => am.Status)
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (dados is null)
            {
                return null;
            }

            return new CursoDetalhesDto(
                dados.IdCurso,
                dados.Nome,
                // Sigla ainda não existe no Domain.
                Sigla: null,
                dados.Total,
                dados.Cancelados,
                dados.Reprovados,
                CalcularTaxa(dados.Cancelados, dados.Total),
                CalcularTaxa(dados.Reprovados, dados.Total),
                dados.Modulos
                    .Select(m => new ModuloCursoDto(
                        m.IdModulo,
                        m.Nome,
                        m.Numero,
                        m.NomeProfessor,
                        m.Total))
                    .ToList());
        }

        /// <summary>
        /// Percentual arredondado de <paramref name="parte"/> sobre <paramref name="total"/>.
        /// Curso sem aluno nenhum devolve 0 em vez de estourar a divisão.
        /// </summary>
        private static int CalcularTaxa(int parte, int total) =>
            total == 0 ? 0 : (int)Math.Round(100d * parte / total);
    }
}
