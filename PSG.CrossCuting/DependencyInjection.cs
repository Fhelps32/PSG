using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PSG.Infra.Data;
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
        public static IServiceCollection AddAplication(this IServiceCollection services) 
        {
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
