using MyJobServiceLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyJobService
{
    class SimpleJob : IJobTask
    {
        JobConfig Config;
        public void Init(JobConfig config)
        {
            this.Config = config;
        }

        public void Run()
        {
            Console.WriteLine($"I am job id {Config.JobId}");
            foreach(string key in Config.Config.Keys)
            {
                Console.WriteLine($"{key} = {Config.Config[key]}");
            }

        }
    }
}
