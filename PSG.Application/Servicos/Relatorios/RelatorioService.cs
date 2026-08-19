using Microsoft.EntityFrameworkCore;
using PSG.Application.Context;
using PSG.Application.Servicos.Alunos;
using PSG.Domain.Enum;

namespace PSG.Application.Servicos.Relatorios
{
    public class RelatorioService
    {
        private readonly IPSGDbContext _context;

        public RelatorioService(IPSGDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Ponto de entrada único: monta o relatório de acordo com o Tipo do filtro.
        /// Usado tanto pela prévia quanto pela exportação — as duas chamam esta mesma
        /// função, então nunca divergem sobre quais linhas o relatório tem.
        /// </summary>
        public async Task<RelatorioResultadoDto> GerarRelatorioAsync(RelatorioFiltroDto filtro)
        {
            var (periodoInicio, periodoFim) = ResolverPeriodo(filtro);

            var resultado = filtro.Tipo switch
            {
                EnumRelatorioTipo.Alunos => filtro.EscopoAluno == EnumRelatorioEscopoAluno.Especifico
                    ? await MontarRelatorioAlunoEspecificoAsync(filtro.IdAlunoEspecifico, periodoInicio, periodoFim)
                    : await MontarRelatorioAlunosFiltradosAsync(filtro, periodoInicio, periodoFim),
                EnumRelatorioTipo.Modulos => await MontarRelatorioModulosAsync(filtro.IdCurso, filtro.MostrarAlunosNosModulos, periodoInicio, periodoFim),
                EnumRelatorioTipo.Cursos => await MontarRelatorioCursosAsync(filtro.MostrarAlunosNosModulos, periodoInicio, periodoFim),
                _ => throw new ArgumentOutOfRangeException(nameof(filtro))
            };

            return resultado with { PeriodoInicio = periodoInicio, PeriodoFim = periodoFim };
        }

        /// <summary>
        /// Converte o preset de período (última semana/mês/semestre) ou a data
        /// personalizada em um intervalo concreto. SemFiltro devolve (null, null) —
        /// os montadores de relatório tratam isso como "sem filtro de data".
        /// </summary>
        private static (DateTime? Inicio, DateTime? Fim) ResolverPeriodo(RelatorioFiltroDto filtro)
        {
            var hoje = DateTime.Today;

            return filtro.Periodo switch
            {
                EnumRelatorioPeriodo.UltimaSemana => (hoje.AddDays(-7), hoje),
                EnumRelatorioPeriodo.UltimoMes => (hoje.AddMonths(-1), hoje),
                EnumRelatorioPeriodo.UltimoSemestre => (hoje.AddMonths(-6), hoje),
                EnumRelatorioPeriodo.Personalizado when filtro.DataInicioPersonalizada.HasValue =>
                    (filtro.DataInicioPersonalizada, filtro.DataFimPersonalizada ?? hoje),
                _ => (null, null)
            };
        }

        /// <summary>
        /// Relatório de UM aluno: todas as inscrições dele (módulo, status, nota,
        /// datas), com a situação geral e quantos módulos já concluiu sobre o total
        /// do curso — o mesmo cálculo usado na tela de Alunos.
        /// </summary>
        /// <remarks>
        /// O período, quando informado, filtra pela data de início da inscrição
        /// (DataAcesso); inscrições fora do intervalo não entram nas linhas, mas o
        /// contador de módulos concluídos considera TODAS as inscrições do aluno,
        /// não só as do período — senão o número pareceria "errado" comparado à tela
        /// de Alunos.
        /// </remarks>
        private async Task<RelatorioResultadoDto> MontarRelatorioAlunoEspecificoAsync(
            int? idAluno, DateTime? periodoInicio, DateTime? periodoFim)
        {
            if (!idAluno.HasValue)
            {
                return RelatorioVazio("Relatório de aluno", "Nenhum aluno selecionado.");
            }

            var dados = await _context.Alunos
                .Where(a => a.IdAluno == idAluno.Value)
                .Select(a => new
                {
                    a.Nome,
                    a.Matricula,
                    NomeCurso = a.Curso.Nome,
                    TotalModulosCurso = a.Curso.Modulos.Count(m => m.Status),
                    Aprovados = a.Modulos.Count(am => am.StatusInscricao == EnumStatus.Aprovado),
                    EmAndamento = a.Modulos.Count(am => am.StatusInscricao == EnumStatus.EmAndamento),
                    Modulos = a.Modulos
                        .OrderBy(am => am.Modulo.Numero)
                        .Select(am => new
                        {
                            am.Modulo.Numero,
                            am.Modulo.Nome,
                            am.StatusInscricao,
                            am.Nota,
                            am.DataAcesso,
                            am.DataConclusao,
                            am.DataMatricula
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (dados is null)
            {
                return RelatorioVazio("Relatório de aluno", "Aluno não encontrado.");
            }

            var status = AlunoService.CalcularStatusAluno(dados.EmAndamento, dados.Aprovados, dados.TotalModulosCurso);

            var linhas = dados.Modulos
                .Where(m => NoPeriodo(m.DataAcesso, periodoInicio, periodoFim))
                .Select(m => new List<string>
                {
                    $"{m.Numero:00} - {m.Nome}",
                    StatusTexto(m.StatusInscricao),
                    FormatarNota(m.StatusInscricao, m.Nota),
                    FormatarData(m.DataAcesso),
                    FormatarData(m.DataConclusao),
                    FormatarData(m.DataMatricula)
                })
                .ToList();

            var subtitulo = $"{dados.NomeCurso} · Matrícula {dados.Matricula ?? "não informada"} · " +
                             $"{StatusAlunoTexto(status)} · {dados.Aprovados}/{dados.TotalModulosCurso} módulos concluídos";

            return new RelatorioResultadoDto(
                $"Relatório do aluno: {dados.Nome}",
                subtitulo,
                DateTime.Now,
                null, null, // preenchido pelo chamador (GerarRelatorioAsync)
                new List<string> { "Módulo", "Status", "Nota", "Data Início", "Data Fim", "Data Matrícula" },
                linhas);
        }

        /// <summary>
        /// Relatório de VÁRIOS alunos por filtro. Tem duas formas, dependendo se o
        /// usuário filtrou por status de inscrição:
        /// - Com status: uma linha por INSCRIÇÃO que bate os filtros (curso, status,
        ///   e nota quando o status é Aprovado/Reprovado). O mesmo aluno pode aparecer
        ///   mais de uma vez, uma por módulo.
        /// - Sem status: uma linha por ALUNO (do curso filtrado, ou todos), com a
        ///   situação geral dele — sem repetir aluno por módulo.
        /// </summary>
        /// <remarks>
        /// Período: no modo "com status" filtra pela data de início da inscrição
        /// (DataAcesso); no modo "sem status" filtra pela data de cadastro do aluno
        /// (DataCadastro), já que não há uma inscrição específica em jogo.
        /// </remarks>
        private async Task<RelatorioResultadoDto> MontarRelatorioAlunosFiltradosAsync(
            RelatorioFiltroDto filtro, DateTime? periodoInicio, DateTime? periodoFim)
        {
            if (filtro.Status.HasValue)
            {
                var aplicaFiltroNota = filtro.Nota.HasValue &&
                    filtro.Status is EnumStatus.Aprovado or EnumStatus.Reprovado;

                var query = _context.AlunoModulos
                    .Where(am => am.Status && am.StatusInscricao == filtro.Status.Value);

                if (filtro.IdCurso.HasValue)
                {
                    query = query.Where(am => am.Modulo.IdCurso == filtro.IdCurso.Value);
                }

                if (aplicaFiltroNota)
                {
                    query = filtro.OperadorNota switch
                    {
                        EnumOperadorNota.MaiorOuIgual => query.Where(am => am.Nota >= filtro.Nota!.Value),
                        EnumOperadorNota.MenorOuIgual => query.Where(am => am.Nota <= filtro.Nota!.Value),
                        _ => query.Where(am => am.Nota == filtro.Nota!.Value)
                    };
                }

                var inscricoes = await query
                    .OrderBy(am => am.Aluno.Nome)
                    .Select(am => new
                    {
                        NomeAluno = am.Aluno.Nome,
                        am.Aluno.Matricula,
                        NomeCurso = am.Modulo.Curso.Nome,
                        NumeroModulo = am.Modulo.Numero,
                        NomeModulo = am.Modulo.Nome,
                        am.StatusInscricao,
                        am.Nota,
                        am.DataAcesso,
                        am.DataConclusao
                    })
                    .ToListAsync();

                var linhasInscricoes = inscricoes
                    .Where(i => NoPeriodo(i.DataAcesso, periodoInicio, periodoFim))
                    .Select(i => new List<string>
                    {
                        i.NomeAluno,
                        i.Matricula ?? "—",
                        i.NomeCurso,
                        $"{i.NumeroModulo:00} - {i.NomeModulo}",
                        StatusTexto(i.StatusInscricao),
                        FormatarNota(i.StatusInscricao, i.Nota),
                        FormatarData(i.DataAcesso),
                        FormatarData(i.DataConclusao)
                    })
                    .ToList();

                return new RelatorioResultadoDto(
                    "Relatório de alunos",
                    $"Inscrições com status {StatusTexto(filtro.Status.Value)}" +
                        (aplicaFiltroNota ? $", nota {OperadorNotaTexto(filtro.OperadorNota)} {filtro.Nota:0.0}" : ""),
                    DateTime.Now, null, null,
                    new List<string> { "Aluno", "Matrícula", "Curso", "Módulo", "Status", "Nota", "Data Início", "Data Fim" },
                    linhasInscricoes);
            }

            var queryAlunos = _context.Alunos.AsQueryable();
            if (filtro.IdCurso.HasValue)
            {
                queryAlunos = queryAlunos.Where(a => a.IdCurso == filtro.IdCurso.Value);
            }

            var alunos = await queryAlunos
                .OrderBy(a => a.Nome)
                .Select(a => new
                {
                    a.Nome,
                    a.Matricula,
                    a.DataCadastro,
                    NomeCurso = a.Curso.Nome,
                    TotalModulosCurso = a.Curso.Modulos.Count(m => m.Status),
                    Aprovados = a.Modulos.Count(am => am.StatusInscricao == EnumStatus.Aprovado),
                    EmAndamento = a.Modulos.Count(am => am.StatusInscricao == EnumStatus.EmAndamento),
                    ModuloAtual = a.Modulos
                        .Where(am => am.StatusInscricao == EnumStatus.EmAndamento)
                        .OrderByDescending(am => am.Modulo.Numero)
                        .Select(am => am.Modulo.Nome)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var linhasAlunos = alunos
                .Where(a => NoPeriodo(a.DataCadastro, periodoInicio, periodoFim))
                .Select(a => new List<string>
                {
                    a.Nome,
                    a.Matricula ?? "—",
                    a.NomeCurso,
                    StatusAlunoTexto(AlunoService.CalcularStatusAluno(a.EmAndamento, a.Aprovados, a.TotalModulosCurso)),
                    a.ModuloAtual ?? "nenhum em andamento"
                })
                .ToList();

            return new RelatorioResultadoDto(
                "Relatório de alunos",
                filtro.IdCurso.HasValue ? null : "Todos os cursos",
                DateTime.Now, null, null,
                new List<string> { "Aluno", "Matrícula", "Curso", "Situação", "Módulo atual" },
                linhasAlunos);
        }

        /// <summary>
        /// Relatório dos módulos de um curso (ou de todos, sem <paramref name="idCurso"/>).
        /// Sem <paramref name="mostrarAlunos"/>, uma linha por módulo com a quantidade de
        /// inscritos; com, uma linha por inscrição (o módulo se repete a cada aluno).
        /// </summary>
        /// <remarks>
        /// O período só se aplica quando mostrarAlunos está ligado (filtra pela data de
        /// início da inscrição) — sem alunos na tabela não há data de inscrição para
        /// filtrar, então a lista de módulos sai inteira independente do período.
        /// </remarks>
        private async Task<RelatorioResultadoDto> MontarRelatorioModulosAsync(
            int? idCurso, bool mostrarAlunos, DateTime? periodoInicio, DateTime? periodoFim)
        {
            var query = _context.Modulos.Where(m => m.Status);
            if (idCurso.HasValue)
            {
                query = query.Where(m => m.IdCurso == idCurso.Value);
            }

            string? nomeCursoFiltro = idCurso.HasValue
                ? await _context.Cursos.Where(c => c.IdCurso == idCurso.Value).Select(c => c.Nome).FirstOrDefaultAsync()
                : null;
            var titulo = nomeCursoFiltro is null ? "Relatório de módulos — todos os cursos" : $"Relatório de módulos — {nomeCursoFiltro}";

            if (!mostrarAlunos)
            {
                // Projeta pra um tipo anônimo e materializa ANTES de montar a List<string> de
                // cada linha: um Select(m => new List<string>{...}) direto na IQueryable não
                // traduz pra SQL (EF Core não sabe converter um inicializador de coleção em
                // uma projeção) e derrubaria a consulta em runtime.
                var modulos = await query
                    .OrderBy(m => m.Curso.Nome).ThenBy(m => m.Numero)
                    .Select(m => new
                    {
                        NomeCurso = m.Curso.Nome,
                        m.Numero,
                        m.Nome,
                        NomeProfessor = m.Professor.Nome,
                        QuantidadeAlunos = m.Alunos.Count(am => am.Status)
                    })
                    .ToListAsync();

                var linhas = modulos
                    .Select(m => new List<string>
                    {
                        m.NomeCurso, m.Numero.ToString("00"), m.Nome, m.NomeProfessor, m.QuantidadeAlunos.ToString()
                    })
                    .ToList();

                return new RelatorioResultadoDto(titulo, null, DateTime.Now, null, null,
                    new List<string> { "Curso", "Número", "Módulo", "Professor", "Quantidade de Alunos" },
                    linhas);
            }

            var inscricoes = await query
                .OrderBy(m => m.Curso.Nome).ThenBy(m => m.Numero)
                .SelectMany(m => m.Alunos
                    .Where(am => am.Status)
                    .Select(am => new
                    {
                        NomeCurso = m.Curso.Nome,
                        m.Numero,
                        NomeModulo = m.Nome,
                        NomeProfessor = m.Professor.Nome,
                        NomeAluno = am.Aluno.Nome,
                        am.Aluno.Matricula,
                        am.StatusInscricao,
                        am.Nota,
                        am.DataAcesso
                    }))
                .ToListAsync();

            var linhas = inscricoes
                .Where(i => NoPeriodo(i.DataAcesso, periodoInicio, periodoFim))
                .Select(i => new List<string>
                {
                    i.NomeCurso,
                    i.Numero.ToString("00"),
                    i.NomeModulo,
                    i.NomeProfessor,
                    i.NomeAluno,
                    i.Matricula ?? "—",
                    StatusTexto(i.StatusInscricao),
                    FormatarNota(i.StatusInscricao, i.Nota)
                })
                .ToList();

            return new RelatorioResultadoDto(titulo, null, DateTime.Now, null, null,
                new List<string> { "Curso", "Número", "Módulo", "Professor", "Aluno", "Matrícula", "Status", "Nota" },
                linhas);
        }

        /// <summary>
        /// Relatório de TODOS os cursos com seus módulos. Curso sem módulo ativo ainda
        /// assim aparece, numa linha só com "—" nas colunas de módulo — diferente do
        /// relatório de Módulos, aqui a lista de cursos é sempre completa.
        /// </summary>
        private async Task<RelatorioResultadoDto> MontarRelatorioCursosAsync(
            bool mostrarAlunos, DateTime? periodoInicio, DateTime? periodoFim)
        {
            // Três consultas achatadas (curso / módulo / inscrição), em vez de uma só com
            // três níveis de coleção aninhada: o EF Core traduz projeção aninhada de um
            // nível com folga (usado em CursoService), mas em três níveis o risco de cair
            // numa consulta não traduzível é real e eu não tenho como testar aqui. A junção
            // dos três conjuntos é feita em memória logo abaixo.
            var cursos = await _context.Cursos
                .Where(c => c.Status)
                .OrderBy(c => c.Nome)
                .Select(c => new { c.IdCurso, c.Nome, NomeCoordenador = c.Coordenador.Nome })
                .ToListAsync();

            var modulos = await _context.Modulos
                .Where(m => m.Status)
                .OrderBy(m => m.Numero)
                .Select(m => new
                {
                    m.IdModulo,
                    m.IdCurso,
                    m.Numero,
                    m.Nome,
                    NomeProfessor = m.Professor.Nome,
                    QuantidadeAlunos = m.Alunos.Count(am => am.Status)
                })
                .ToListAsync();

            var modulosPorCurso = modulos.ToLookup(m => m.IdCurso);

            // Só busca inscrições quando o switch está ligado — sem elas ninguém usa.
            var inscricoesPorModulo = mostrarAlunos
                ? (await _context.AlunoModulos
                    .Where(am => am.Status)
                    .Select(am => new
                    {
                        am.IdModulo,
                        NomeAluno = am.Aluno.Nome,
                        am.Aluno.Matricula,
                        am.StatusInscricao,
                        am.Nota,
                        am.DataAcesso
                    })
                    .ToListAsync())
                    .Where(i => NoPeriodo(i.DataAcesso, periodoInicio, periodoFim))
                    .ToLookup(i => i.IdModulo)
                : null;

            var linhas = new List<List<string>>();

            foreach (var curso in cursos)
            {
                var modulosDoCurso = modulosPorCurso[curso.IdCurso].ToList();

                if (modulosDoCurso.Count == 0)
                {
                    // Curso sem módulo: entra com uma linha "vazia" pra não sumir do relatório.
                    linhas.Add(mostrarAlunos
                        ? new List<string> { curso.Nome, curso.NomeCoordenador, "—", "Sem módulos cadastrados", "—", "—", "—", "—", "—" }
                        : new List<string> { curso.Nome, curso.NomeCoordenador, "—", "Sem módulos cadastrados", "—", "0" });
                    continue;
                }

                foreach (var modulo in modulosDoCurso)
                {
                    if (!mostrarAlunos)
                    {
                        linhas.Add(new List<string>
                        {
                            curso.Nome, curso.NomeCoordenador, modulo.Numero.ToString("00"), modulo.Nome,
                            modulo.NomeProfessor, modulo.QuantidadeAlunos.ToString()
                        });
                        continue;
                    }

                    var alunosNoPeriodo = inscricoesPorModulo![modulo.IdModulo].ToList();
                    if (alunosNoPeriodo.Count == 0)
                    {
                        // Sem inscrito (ou nenhum no período): módulo ainda aparece, sem aluno.
                        linhas.Add(new List<string>
                        {
                            curso.Nome, curso.NomeCoordenador, modulo.Numero.ToString("00"), modulo.Nome,
                            modulo.NomeProfessor, "—", "—", "—", "—"
                        });
                        continue;
                    }

                    foreach (var aluno in alunosNoPeriodo)
                    {
                        linhas.Add(new List<string>
                        {
                            curso.Nome, curso.NomeCoordenador, modulo.Numero.ToString("00"), modulo.Nome,
                            modulo.NomeProfessor, aluno.NomeAluno, aluno.Matricula ?? "—",
                            StatusTexto(aluno.StatusInscricao), FormatarNota(aluno.StatusInscricao, aluno.Nota)
                        });
                    }
                }
            }

            var colunas = mostrarAlunos
                ? new List<string> { "Curso", "Coordenador", "Número", "Módulo", "Professor", "Aluno", "Matrícula", "Status", "Nota" }
                : new List<string> { "Curso", "Coordenador", "Número", "Módulo", "Professor", "Quantidade de Alunos" };

            return new RelatorioResultadoDto("Relatório de cursos", null, DateTime.Now, null, null, colunas, linhas);
        }

        private static RelatorioResultadoDto RelatorioVazio(string titulo, string motivo) =>
            new(titulo, motivo, DateTime.Now, null, null, new List<string>(), new List<List<string>>());

        private static bool NoPeriodo(DateTime? data, DateTime? inicio, DateTime? fim)
        {
            if (inicio is null)
            {
                return true; // sem filtro de período: tudo entra
            }
            if (data is null)
            {
                return false; // tem filtro, mas a linha não tem data pra comparar
            }
            return data.Value.Date >= inicio.Value.Date && data.Value.Date <= (fim ?? DateTime.Today).Date;
        }

        private static string FormatarData(DateTime? data) => data?.ToString("dd/MM/yyyy") ?? "—";

        // Nota só é significativa depois que a inscrição termina (aprovado/reprovado);
        // em andamento ou cancelado ela fica 0.0 no banco, mas mostrar "0.0" confundiria.
        private static string FormatarNota(EnumStatus status, decimal nota) =>
            status is EnumStatus.Aprovado or EnumStatus.Reprovado ? nota.ToString("0.0") : "—";

        private static string StatusTexto(EnumStatus status) => status switch
        {
            EnumStatus.Aprovado => "Aprovado",
            EnumStatus.Reprovado => "Reprovado",
            EnumStatus.EmAndamento => "Em andamento",
            EnumStatus.Cancelado => "Cancelado",
            _ => status.ToString()
        };

        private static string StatusAlunoTexto(EnumStatusAluno status) => status switch
        {
            EnumStatusAluno.Cursando => "Cursando",
            EnumStatusAluno.Finalizado => "Finalizado",
            EnumStatusAluno.EmEspera => "Em espera",
            _ => status.ToString()
        };

        private static string OperadorNotaTexto(EnumOperadorNota op) => op switch
        {
            EnumOperadorNota.MaiorOuIgual => "maior ou igual a",
            EnumOperadorNota.MenorOuIgual => "menor ou igual a",
            _ => "igual a"
        };
    }
}
