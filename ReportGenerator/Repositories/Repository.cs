using System;
using ReportGenerator.Models;

namespace ReportGenerator.Repositories
{
    public abstract class Repository : IDisposable
    {
        protected ReportGeneratorContext dbContext;

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public Repository()
        {
            dbContext = new ReportGeneratorContext();
        }
    }
}
