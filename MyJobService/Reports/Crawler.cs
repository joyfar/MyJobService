using MyJobService;
using MyJobServiceLib;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace TestLib
{
    /// <summary>
    /// Example implementation of IJobTask.  Crawler will crawl a set of urls for a specific domain and determine set the result as pass or fail.
    /// </summary>
    public class Crawler : IJobTask
    {
        JobConfig Config;

        /// <summary>
        /// The website to crawl.
        /// </summary>
        string EndPoint;

        /// <summary>
        /// Pages to crawl are from the database and retrieved in batches.
        /// </summary>
        int QueueSize = 200;

        /// <summary>
        /// Maximum number of crawlers to run in parallel.
        /// </summary>
        int MaxCrawlers = 3;

        //Counters
        int Processed = 0;
        int Succeeded = 0;
        
        /// <summary>
        /// Implementation of the Init() interface for IJobTask.
        /// </summary>
        /// <param name="config"></param>
        void IJobTask.Init(JobConfig config)
        {
            this.Config = config;
            if (config.Config["EndPoint"] == null || String.IsNullOrEmpty(config.Config["EndPoint"]))
                throw new ArgumentNullException("EndPoint is required.");

            this.EndPoint = config.Config["EndPoint"];
        }


        /// <summary>
        /// Implementation of the Run() interface for IJobTask.
        /// </summary>
        void IJobTask.Run()
        {

            var CreateTestTask = Task.Run(() =>
            {
                CreateTests();
            });

            //Wait for tasks to finish
            CreateTestTask.Wait();
        }


        /// <summary>
        /// Gets a list of pages to crawl from the database and runs tests on the pages.
        /// </summary>
        private void CreateTests()
        {
            BlockingCollection<CrawlTest> tests = getCrawlTests();

            do
            {
                this.Processed += tests.Count();

                Console.WriteLine($"Crawling {tests.Count()} records.");
                RunTests(tests);
                Console.WriteLine($"Saved {this.Succeeded} items out of {this.Processed}.");


            } while ((tests = getCrawlTests()).Count > 0);

            FlushDatabaseBuffer();
        }

        /// <summary>
        /// Runs tests on the input pages.
        /// </summary>
        /// <param name="tests"></param>
        private void RunTests(BlockingCollection<CrawlTest> tests)
        {
            BlockingCollection<CrawlTest> crawlResultQueue =
                new BlockingCollection<CrawlTest>(this.QueueSize);

            var saveResultTask = Task.Run(() =>
            {
                foreach (var result in crawlResultQueue.GetConsumingEnumerable())
                {
                    BulkSave(result);
                }
            });


            //Only run N number of crawlers.
            var crawlerTasks = Enumerable.Range(0, this.MaxCrawlers)
            .Select(_ => Task.Run(() =>
            {
                while (!tests.IsCompleted)
                {
                    try
                    {
                        //Take should block if it's temporarily empty
                        CrawlTest test = tests.Take();
                        CrawlAndGradeResults(this.EndPoint, test);
                        crawlResultQueue.Add(test);

                    }
                    catch (InvalidOperationException)
                    {
                        
                    }

                }

            }))
            .ToArray();


            //Wait for the task to finish filling up the queue.
            Task.WaitAll(crawlerTasks);            

            //Set the flag that queue is complete.
            crawlResultQueue.CompleteAdding();

            //Wait for the test results to be saved.
            saveResultTask.Wait();
        }

        /// <summary>
        /// Saves results in batches to the database.
        /// </summary>
        /// <param name="result"></param>
        private void BulkSave(CrawlTest result)
        {
            //TO DO:  Save results.
            throw new NotImplementedException();
        }

        //Saves the results from the db buffer to sql.
        private void FlushDatabaseBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uses an HttpClient to get content from the website, then grades the results.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="test"></param>
        static private void CrawlAndGradeResults(string endpoint, CrawlTest test)
        {
            string url = string.Format(endpoint, HttpUtility.UrlEncode(test.Name));

            try
            {

                string content = null;
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    content = response.Content.ReadAsStringAsync().Result;

                    test.qData = content;
                    GradeResults(test);
                }


            }
            catch (Exception ex)
            {
                //TO DO:  Log error
                Console.WriteLine(ex.ToString());
            }

        }

        static void GradeResults(CrawlTest test)
        {
            //TO DO:  Add logic to grade the test.
            test.qResult = "pass";
        }

        static BlockingCollection<CrawlTest> getCrawlTests(int batchSize = 100)
        {
            BlockingCollection<CrawlTest> ret = new BlockingCollection<CrawlTest>();
            //TO DO:  Get a list of test from database.
            return ret;
        }


        public class CrawlTest
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string qData { get; set; }
            public string qResult { get; set; }
        }
    }
}
