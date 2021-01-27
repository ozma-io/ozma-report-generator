using System;
using System.Linq;
using ReportGenerator.Models;

namespace ReportGenerator.Repositories
{
    public abstract class Repository : IDisposable
    {
        protected ReportGeneratorContext dbContext;
        protected readonly Instance instance = null!;

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public Repository(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new Exception("Instance name cannot be empty");
            dbContext = new ReportGeneratorContext();
            var instance = dbContext.Instances.FirstOrDefault(p => p.Name == instanceName);
            if (instance == null)
            {
                //throw new Exception("Instance " + instanceName + " not found in database");
                var newInstance = new Instance {Name = instanceName};
                dbContext.Instances.AddAsync(newInstance);
                dbContext.SaveChangesAsync();
                instance = newInstance;
            }
            this.instance = instance;
        }
    }
}
