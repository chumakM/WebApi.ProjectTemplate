using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace $ext_safeprojectname$.BL
{
    /// <summary>
    /// Базовый класс для логики
    /// </summary>
    /// <typeparam name="T">Тип наследуемого класса</typeparam>
    public class BaseLogic<T> where T : class
    {
        /// <summary>
        /// Логгер
        /// </summary>
        protected readonly ILogger<T> _logger;
        public BaseLogic(ILogger<T> logger)
        {
            _logger = logger;
        }
    }
}
