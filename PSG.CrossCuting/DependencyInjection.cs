using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSG.Application.Context;
using PSG.Application.Interfaces;
using PSG.Application.Servicos.AlunoModulos;
using PSG.Application.Servicos.Alunos;
using PSG.Application.Servicos.Csv;
using PSG.Application.Servicos.Cursos;
using PSG.Application.Servicos.Modulos;
using PSG.Domain;
using PSG.Infra.Data;
using PSG.Infra.Data.Csv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PSG.CrossCuting
{
    public static class DependencyInjection
    {
        #region Application
        public static IServiceCollection AddApplication(this IServiceCollection services) 
        {
            services.AddScoped<IPSGDbContext, PSGDbContext>();
            services.AddScoped<ICsvReaderService, CsvReaderService>();
            services.AddScoped<CsvImporterService>();
            services.AddScoped<AlunoModuloService>();
            services.AddScoped<CursoService>();
            services.AddScoped<ModuloService>();
            services.AddScoped<AlunoService>();

            return services;
        }
        #endregion

        #region Infra
        public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<PSGDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") 
                    ?? throw new InvalidOperationException("Não foi possível encontrar a string de conexão 'DefaultConnection'."));
            });
            return services;
        }
        #endregion
    }   
}
