# MyJobService
Example of a console app that can be used to run a set of jobs you define.

## To run
* Open the solution in Visual Studio and set __MyJobService__ as the startup project.
* Hit F5 to start running in debug mode.

## Layout
* __MyJobService__ is the console app that will look for jobs to run.  Jobs are expected to implement the IJobTask interface.
* __MyJobServiceLib__ is a DLL that contains the IJobTask interface and other shared classes.
* __TestLib__ is an example of DLL that will contain jobs that performs tests.  The first type of test implemented is a Crawler, which will crawl pages on a website and grade the content.  _The Crawler is not yet a runnable sample._

## Usage - Windows Task Scheduler
You can add the MyJobService.exe to Windows Task Scheduler to run every minute.  Set the flag to NOT create a new instance if an existing instance is already running.

Setup a SQL database to store the configuration information for jobs you want to run.  You can also define when and how frequently to run the job.  Then in Program::GetListOfJobsToRun(), change the method to pull the list of jobs to run from the DB.

Finally, create your own DLL with classes that implement _IJobTask_.  Deploy your DLL (and any other dependencies) in the same folder as the application.  Add the configuration information into your SQL database.  The application will start running your jobs.



