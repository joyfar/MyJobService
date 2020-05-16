using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MyJobServiceLib;

namespace MyJobService
{
    class Program
    {
        static List<Task> ThreadPool;
        static int MaxThreads = 10;
        static int WaitTimeMS = 1000;
        static AutoResetEvent ShutdownEvent;

        /// <summary>
        /// Looks in the db for jobs to run.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            ShutdownEvent = new AutoResetEvent(false);
            Console.CancelKeyPress += (sender, args) =>
            {
                ShutdownEvent.Set();
            };


            do
            {
                ThreadPool.RemoveAll(t =>
                       t.Status == TaskStatus.RanToCompletion
                    || t.Status == TaskStatus.Canceled
                    || t.Status == TaskStatus.Faulted);


                if (ThreadPool.Count < MaxThreads)
                {
                    try
                    {
                        IList<JobConfig> configs = GetListOfJobsToRun();

                        if (configs != null)
                        {
                            foreach (JobConfig config in configs)
                            {
                                Task curTask = new Task(RunJob, config);
                                curTask.Start();
                                ThreadPool.Add(curTask);

                            }
                        }
                    }
                    catch (AggregateException)
                    {
                        //TO DO: Log and handle error
                        break;

                    }
                    catch (Exception)
                    {
                        //TO DO: Log and handle error
                        break;
                    }
                }

              
            } while (!ShutdownEvent.WaitOne(WaitTimeMS));

            //Wait for the thread to finish
            Task.WaitAll(ThreadPool.ToArray(), 60000);

        }

        static void RunJob(Object state)
        {
            try
            {
                JobConfig config = (JobConfig)state;
                IJobTask job = (IJobTask) Activator.CreateInstance(config.AssemblyName, config.TypeName);
                job.Init(config);
                job.Run();
            }
            catch(Exception)
            {
                //TO DO: Log error
            }

        }

        /// <summary>
        /// Get list of jobs to run.  Job configuration and information are stored in sql tables.
        /// Logic of what jobs need to run next are in a stored procedure.
        /// </summary>
        /// <returns></returns>
        static IList<JobConfig> GetListOfJobsToRun()
        {
            throw new NotImplementedException();
        }
    }
}
