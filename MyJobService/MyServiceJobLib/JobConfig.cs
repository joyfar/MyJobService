using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace MyJobServiceLib
{
    /// <summary>
    /// Definition and configuration information for a job.
    /// This information is stored in sql tables.
    /// </summary>
    public class JobConfig
    {
        public JobConfig(long jobId, string assemblyName, string typeName)
        {
            this.JobId = jobId;
            this.AssemblyName = assemblyName;
            this.TypeName = typeName;
            this.Config = new NameValueCollection();
        }
        /// <summary>
        /// A job will be assigned a unique id.
        /// </summary>
        public long JobId { get; private set; }
        
        /// <summary>
        /// The assembly name of where to find the class type.
        /// </summary>
        public string AssemblyName { get; private set; }

        /// <summary>
        /// The fully qualified name of the class type.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Configuration information for the job, which are stored in sql tables.
        /// </summary>
        public NameValueCollection Config { get; private set; }

        
    }
}
