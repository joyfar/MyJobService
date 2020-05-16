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
        static List<Task> ThreadPool = new List<Task>();
        static int MaxThreads = 10;
        static int WaitTimeMS = 5000;
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
                IJobTask job = (IJobTask) Activator.CreateInstance(Type.GetType(config.TypeName));
                job.Init(config);
                job.Run();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        /// <summary>
        /// Get a list of jobs to run.  
        /// For example, job configuration and information are stored in sql tables.
        /// Logic of what jobs need to run next are in a stored procedure.
        /// For now, returns SimpleJob.
        /// </summary>
        /// <returns></returns>
        static IList<JobConfig> GetListOfJobsToRun()
        {
            List<JobConfig> ret = new List<JobConfig>();

            //TO DO:  Get a list of jobs to run.

            ret.Add(createSimpleJob());
            return ret;            
        }

        static JobConfig createSimpleJob()
        {
            JobConfig config = new JobConfig(DateTime.UtcNow.Ticks
            , ""
            , "MyJobService.SimpleJob");

            config.Config["input"] = "Hello World!";
            
            return config;

        }


    }
}
