using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
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

        public Repository(IConfiguration configuration, string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new Exception("Instance name cannot be empty");
            dbContext = new ReportGeneratorContext(configuration);
            var instance = dbContext.Instances.FirstOrDefault(p => p.Name == instanceName);
            if (instance == null)
            {
                //throw new Exception("Instance " + instanceName + " not found in database");
                var newInstance = new Instance {Name = instanceName};
                dbContext.Instances.Add(newInstance);
                dbContext.SaveChanges();
                instance = newInstance;
            }

            this.instance = instance;
        }
    }
}
