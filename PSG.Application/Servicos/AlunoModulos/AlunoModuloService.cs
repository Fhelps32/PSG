using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Servicos.Shared;
using PSG.Domain;
using PSG.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.AlunoModulos
{
    public class AlunoModuloService
    {
        private readonly IPSGDbContext _context;

        public AlunoModuloService(IPSGDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AlunoModulo>> ObterInscricoesFiltradasPaginadoAsync(int pagina,
            string? nomeAluno = null,
            string? nomeModulo = null,
            int? idCurso = null,
            EnumStatus? status = null,
            int? idModulo = null
            )
        {
            // Include das navegações: sem lazy loading, Aluno/Modulo/Curso viriam nulos
            // e a projeção na Presentation (NomeAluno, NomeCurso, etc.) quebraria.
            var query = _context.AlunoModulos
                .Include(am => am.Aluno)
                .Include(am => am.Modulo)
                    .ThenInclude(m => m.Curso)
                .AsQueryable();
            query = query.Where(am => am.Status == true);
            if (idCurso.HasValue)
            {
                query = query.Where(am => am.Modulo.IdCurso == idCurso.Value);
            }
            if (idModulo.HasValue)
            {
                query = query.Where(am => am.IdModulo == idModulo.Value);
            }
            if (status.HasValue)
            {
                query = query.Where(am => am.StatusInscricao == status.Value);
            }
            if (!string.IsNullOrEmpty(nomeAluno))
            {
                query = query.Where(am => am.Aluno.Nome.Contains(nomeAluno));
            }
            var result = await query.Paginar<AlunoModulo>(new PaginationRequest { NumeroPagina = pagina, TamanhoPagina = 20 });
            return result;
        }

        public async Task<List<AlunoModulo>> GetAllAlunoModuloAsync()
        {
            var result = await _context.AlunoModulos
                .Include(am => am.Aluno)
                .Include(am => am.Modulo)
                .ThenInclude(m => m.Curso)
                .Where(am => am.Status == true)
                .ToListAsync();
            return result;
        }

        /// <summary>
        /// Dados de uma inscrição para a modal de edição, com aluno, curso e módulo
        /// para exibir como contexto. Retorna null quando a inscrição não existe ou
        /// já foi excluída.
        /// </summary>
        public async Task<InscricaoEdicaoDto?> ObterInscricaoParaEdicaoAsync(int idAlunoModulo)
        {
            return await _context.AlunoModulos
                .Where(am => am.IdAlunoModulo == idAlunoModulo && am.Status)
                .Select(am => new InscricaoEdicaoDto(
                    am.IdAlunoModulo,
                    am.Aluno.Nome,
                    am.Modulo.Curso.Nome,
                    am.Modulo.Nome,
                    am.Modulo.Numero,
                    am.DataAcesso,
                    am.DataConclusao,
                    am.DataMatricula,
                    am.Nota,
                    am.StatusInscricao,
                    am.ObsGeral))
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Atualiza os campos editáveis da inscrição (datas, nota, status e
        /// observação). Lança exceção quando a inscrição não existe.
        /// </summary>
        public async Task AtualizarInscricaoAsync(int idAlunoModulo, AlunoModuloDtoAtualizar dto)
        {
            var alunoModulo = await _context.AlunoModulos.FindAsync(idAlunoModulo);
            if (alunoModulo == null)
            {
                throw new Exception($"Inscrição com ID {idAlunoModulo} não encontrada.");
            }

            alunoModulo.DataAcesso = dto.DataInicio;
            alunoModulo.DataConclusao = dto.DataFim;
            alunoModulo.DataMatricula = dto.DataMatricula;
            alunoModulo.Nota = dto.Nota;
            alunoModulo.StatusInscricao = dto.EnumStatus;
            alunoModulo.ObsGeral = dto.ObsGeral;

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Exclusão lógica da inscrição (Status = false), mesma abordagem usada em
        /// aluno e módulo. As listagens já filtram por Status, então a linha some das
        /// telas sem o histórico ser apagado do banco.
        /// </summary>
        public async Task ExcluirInscricaoAsync(int idAlunoModulo)
        {
            var alunoModulo = await _context.AlunoModulos.FindAsync(idAlunoModulo);
            if (alunoModulo == null)
            {
                throw new Exception($"Inscrição com ID {idAlunoModulo} não encontrada.");
            }
            if (!alunoModulo.Status)
            {
                throw new Exception($"A inscrição com ID {idAlunoModulo} já está inativa.");
            }

            alunoModulo.Status = false;
            await _context.SaveChangesAsync();
        }

        public async Task<AlunoModulo> CreateAlunoModuloAsync(AlunoModuloDtoCriar dto)
        {
            try
            {
                var alunoModulo = new AlunoModulo(dto.Aluno, dto.Modulo, dto.DataInicio, dto.EnumStatus, dto.DataFim);
                await _context.AlunoModulos.AddAsync(alunoModulo);
                await _context.SaveChangesAsync();
                return alunoModulo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível criar a inscrição. Exepcion: {ex}");
            }
        }
    }
}
