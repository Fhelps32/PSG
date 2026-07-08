using CsvHelper.Configuration;
using PSG.Application.Servicos.Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PSG.Infra.Data.Csv.Maps
{
    public class LinhaCsvMap : ClassMap<LinhaCsvDto>
    {
        public LinhaCsvMap()
        {
            Map(m => m.Curso).Name("Curso");
            Map(m => m.NumeroModulo).Name("Nº do Módulo");
            Map(m => m.NomeModulo).Name("Nome do Módulo");
            Map(m => m.Aluno).Name("Aluno(a)");
            Map(m => m.DataMatricula).Name("Data matríc.");
            Map(m => m.DataAcesso).Name("Data de acesso");
            Map(m => m.DataFim).Name("Data fim");
            Map(m => m.Nota).Name("Nota");
            Map(m => m.Aprovacao).Name("Aprovação");
            Map(m => m.Celular).Name("Celular");
            Map(m => m.Observacao).Name("Obs.");
            Map(m => m.Recuperacao).Name("Recuperação após exclusão do módulo");
        }
    }
}