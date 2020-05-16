using MyJobServiceLib;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace MyJobService
{
    /// <summary>
    /// Interface for jobs.
    /// </summary>
    public interface IJobTask
    {
        /// <summary>
        /// Initialization work can be done here.  The service will call Init() before calling Run().
        /// </summary>
        /// <param name="config">The configuration information for the job.  
        /// Job configuration information are stored in sql tables.  The service will get this information and pass this information in this parameter.
        /// </param>
        void Init(JobConfig config);

        /// <summary>
        /// The service will call Run() after Init().  This method is where the bulk of the job logic goes.
        /// </summary>
        void Run();

    }
}
