using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ReportGenerator.Models;

namespace ReportGenerator.Repositories
{
    public abstract class Repository : IDisposable
    {
        protected ReportGeneratorContext dbContext;
        protected Instance instance { get; private set; }= null!;

        public void Dispose()
        {
            dbContext.Dispose();
        }

        public Repository(IConfiguration configuration, string instanceName, bool createInstanceIfNotExists = false)
        {
            if (string.IsNullOrEmpty(instanceName))
                throw new Exception("Instance name cannot be empty");
            dbContext = new ReportGeneratorContext(configuration);
            var instance = dbContext.Instances.FirstOrDefault(p => p.Name == instanceName);
            if (instance == null)
            {
                if (createInstanceIfNotExists)
                {
                    var newInstance = new Instance { Name = instanceName };
                    dbContext.Instances.Add(newInstance);
                    dbContext.SaveChanges();
                    instance = newInstance;
                }
                else 
                    throw new Exception("Instance " + instanceName + " not found in database");
            }
            this.instance = instance;
        }
    }
}
