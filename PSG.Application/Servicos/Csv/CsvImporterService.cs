using PSG.Application.Context;
using PSG.Application.Interfaces;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Application.Servicos.Csv
{
    public class CsvImporterService
    {
        private readonly ICsvReaderService _csvReaderService;
        private readonly IPSGDbContext _pSGDbContext;

        public CsvImporterService(ICsvReaderService csvReaderService, IPSGDbContext pSGDbContext)
        {
            _csvReaderService = csvReaderService;
            _pSGDbContext = pSGDbContext;
        }

        
        public async Task ImportarCsv(Stream stream)
        {
            var linhasCsv = _csvReaderService.LerCsv(stream);
            foreach (var linha in linhasCsv)
            {
                var curso = _pSGDbContext.Cursos.FirstOrDefault(c => c.Nome == linha.Curso);
                if(curso == null)
                {
                    curso = new Curso(linha.Curso);
                    _pSGDbContext.Cursos.Add(curso);
                }
                
                var modulo = _pSGDbContext.Modulos.FirstOrDefault(m => m.Nome == linha.NomeModulo && m.IdCurso == curso.IdCurso);
                if(modulo == null)
                {
                    var numero = linha.NumeroModulo;
                    if (numero.Contains("invertido"))
                    {
                        numero = new string(numero.Reverse().ToArray());
                    }
                    var numeroTratado = numero[0] + numero[1];
                    var numeroInt = int.Parse(numeroTratado.ToString());

                    modulo = new Modulo(curso, linha.NomeModulo, numeroInt);
                    _pSGDbContext.Modulos.Add(modulo);
                }

                var aluno = _pSGDbContext.Alunos.FirstOrDefault(a => a.Nome == linha.Aluno && a.IdCurso == curso.IdCurso);
                if(aluno == null)
                {
                    aluno = new Aluno(curso, linha.Aluno);
                    _pSGDbContext.Alunos.Add(aluno);
                }

                var alunoModulo = _pSGDbContext.AlunoModulos.FirstOrDefault(am => am.IdAluno == aluno.IdAluno && am.IdModulo == modulo.IdModulo);
                if(alunoModulo == null)
                {
                    alunoModulo = new AlunoModulo(aluno, modulo, linha);
                    _pSGDbContext.AlunoModulos.Add(alunoModulo);
                }
            }
            await _pSGDbContext.SaveChangesAsync();
        }
    }
}
