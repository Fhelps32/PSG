using PSG.Application.Context;
using PSG.Application.Interfaces;
using PSG.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
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
                #region curso
                var curso = _pSGDbContext.Cursos.FirstOrDefault(c => c.Nome == linha.Curso);
                if(curso == null)
                {
                    curso = new Curso(linha.Curso);
                    _pSGDbContext.Cursos.Add(curso);
                }
                #endregion
                
                #region modulo
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
                #endregion

                #region aluno
                var aluno = _pSGDbContext.Alunos.FirstOrDefault(a => a.Nome == linha.Aluno && a.IdCurso == curso.IdCurso);
                if(aluno == null)
                {
                    aluno = new Aluno(curso, linha.Aluno);
                    _pSGDbContext.Alunos.Add(aluno);
                }
                #endregion

                #region alunoModulo
                var alunoModulo = _pSGDbContext.AlunoModulos.FirstOrDefault(am => am.IdAluno == aluno.IdAluno && am.IdModulo == modulo.IdModulo);
                if(alunoModulo == null)
                {
                    try 
                    {
                        var format = "dd/MM/yyyy";
                        var dataAcesso = DateTime.ParseExact(linha.DataAcesso, format, CultureInfo.InvariantCulture);
                        var dataConclusao = DateTime.ParseExact(linha.DataFim, format, CultureInfo.InvariantCulture);

                        var alunoModuloNovo = new AlunoModulo(aluno, modulo, dataAcesso, , dataConclusao);
                    }
                    catch 
                    {

                    }
                }
                #endregion
            }
            await _pSGDbContext.SaveChangesAsync();
        }
    }
}
