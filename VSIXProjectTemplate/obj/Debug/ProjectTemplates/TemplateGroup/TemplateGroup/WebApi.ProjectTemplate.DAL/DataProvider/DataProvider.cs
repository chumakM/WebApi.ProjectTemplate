using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using $ext_safeprojectname$.DAL.Model;

namespace $ext_safeprojectname$.DAL.DataProvider
{
    public class DataProvider : BaseDataProvider
    {
        public DataProvider() : base(null) { }

        /// <inheritdoc />
        public DataProvider(string connectionString) : base(connectionString) { }

        /// <inheritdoc />
        protected override DbContext CreateContext(string connectionString)
        {
            return new TestEntities(connectionString);
        }
    }
}
