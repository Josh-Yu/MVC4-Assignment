using Assignment2Basic.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quartz;
using Quartz.Impl;
using System.Diagnostics;
using Quartz.Core;
using Assignment2Basic.Models;
using System.Globalization;
using Quartz.Impl.Matchers;


/*Note: THe methods have to be multiple purpose in such way that:
 * It can be called on when the customer or admin needs to modify or add a job.
 * Also, needs to be able to read from the database on the intial start because billpay jobs may exist in the 
 * database already
 */
namespace Assignment2Basic.Business_Rules
{
    public class Billpay
    {
        private ISchedulerFactory schedulerFactory;
        private IScheduler sched;
        ITrigger triggerDetail;

        //#### Constants to define what type of period is the billpay job
        public const char ONCEOFF_PERIOD = 'S';
        public const char MONTHLY_PERIOD = 'M';
        public const char QUATER_PERIOD = 'Q';
        public const char ANNUAL_PERIOD = 'A';

        public const string dateFormat = "dd/MM/yyyy";

        public Billpay(ISchedulerFactory schedulerFactory)
        {
            this.schedulerFactory = schedulerFactory;
            //this fetches the schduler from the passed in schedulerFactory from global.asax.cs
            this.sched = schedulerFactory.GetScheduler();
           // Debug.WriteLine("Inside BillPay 1st construtor - passed in factory" + DateTime.Now.ToString());
        }

        //No params constructor for when the scheduler is already running
        //Used for opertions such as remove, edit triggers/jobs and manual adding of jobs 
        public Billpay()
        {
            //This fetches the shared scheduler that is currently active
            sched = new StdSchedulerFactory().GetScheduler();
          //  Debug.WriteLine("Inside Billpay.cs 2nd constructor" + DateTime.Now.ToString());
        }

        // AS OF 12 Oct 2013 - Spec change: New spec states the Billpay table will now be used to store a history of
        //Billpay records.

        //Method will loop through every record in the BillPays table like select *
        //Each record will be read and create a job and add it to the schduler
        public void InitialReadDb()
        {
            //LINQ statement to loop each record in BillPay
            //also call the method to create jobs and trigger

            /*
             NWBAEntities db = new NWBAEntities();
            var query = (from x in db.BillPays
                         select x);
          Debug.WriteLine("Looping through result set retrieved from dbo.BillPay ");
          foreach(var i in query)
          {
              Debug.WriteLine("Current Record in iteration: BillpayID is: " + i.BillPayID);

              int billPayID = i.BillPayID;
              char period = i.Period[0]; //gets the char value from 1 char of string
              int accountNumber = i.AccountNumber;
              int payeeID = i.PayeeID;
              DateTime scheduledDate = i.ScheduleDate;
              decimal amount = i.Amount;

              createScheduledJobService(billPayID, accountNumber, payeeID, amount, period, scheduledDate);
           }
            Debug.WriteLine("Completed reading database for any scheduled jobs " + DateTime.Now.ToString());  
             */
        }
        /*
        This will manually create a job to add to the job service
         * Used when the user creates a billpay job using the web site
         * */
        public void manualCreateBPayJob(int accountNumber, int payeeID, decimal amount, string period, DateTime schedDate)
        {
            //create a database record for the newly created job then  retrive the database geneted billpay id and pass 
            //it to the createScheduledJobService so it can add the job to the job service
            Debug.WriteLine("Creating and Adding a Bew Bill Pay Record to the Billpay table");
            using (NWBAEntities db = new NWBAEntities())
            {
                BillPay newBillPayRecord = new BillPay();
                newBillPayRecord.AccountNumber = accountNumber;
                newBillPayRecord.PayeeID = payeeID;
                newBillPayRecord.Amount = amount;
                newBillPayRecord.Period = period;
                newBillPayRecord.ScheduleDate = schedDate;
                newBillPayRecord.ModifyDate = DateTime.Now;

                db.BillPays.Add(newBillPayRecord);
                db.SaveChanges();
                int billPayID = newBillPayRecord.BillPayID; //EF will update the entity object with the database generated identity

                //Now we can create a job and add it to the schduler since we have a BillpayID to identify the job
                createScheduledJobService(billPayID, accountNumber, payeeID, amount, period[0], schedDate);

            }
        }


        //needed by ADMIN and modifyJob, used to /cancel remove the job from the database
        //Removes the job from  the schedule service
        //FYI - jobname is the billpayid
        public Boolean removeJob(string jobname)
        {
            Debug.WriteLine("Job name passed in was: " + jobname);
            //Deletejob api requires a jobkey to delete the job, below is to find it
            JobKey JobKeyToDelete = new JobKey(jobname, "JobGroup1");

            //This will delete the job and unsubscribe it before from the scheduler before deletion
            if (sched.DeleteJob(JobKeyToDelete))
            {
                Debug.WriteLine("Succcessfuly deleted job from db:" + jobname);
                return true;
            }
            else
            {
                Debug.WriteLine("Cannot delete job:" + jobname);
                return false;
            }
        }

        //Used to modify the job and time if a user modifys it
        //Used to update the db and schedule service
        public void ModifyJob(BillPay UpdatedBillPayObject)
        {
            Debug.WriteLine("Inside Modify Job Method");
            //Remove old job from the schduler
            removeJob(UpdatedBillPayObject.BillPayID.ToString());

            //Add new job to schduler
            createScheduledJobService(UpdatedBillPayObject.BillPayID, UpdatedBillPayObject.AccountNumber,
                UpdatedBillPayObject.PayeeID, UpdatedBillPayObject.Amount, UpdatedBillPayObject.Period[0], UpdatedBillPayObject.ScheduleDate);

        }

        //This will create the time trigger of when to fire off the job
        //Also adds to the job and associated trigger to the scheduler service
        //Typically used, when already have a billpayID 
        //USED BY MODIFY JOB
        public void createScheduledJobService(int billPayID, int accountNumber, int payeeID, decimal amount, char period, DateTime schedDate)
        {
            Debug.WriteLine("\n\n######### Creating Scheduled Job and trigger");
            //Type changes are needed because the job dictionary cannot accept decimal
            double convertedAmount = (double)amount;
            string convertedStringDate = schedDate.ToString(dateFormat, CultureInfo.InvariantCulture);


            DateTimeOffset startTime = DateTimeOffset.Now.AddMinutes(1); // schedule for 1 minute in the future to start fireing
            Debug.WriteLine("###### Curent time: " + DateTime.Now.ToString());
            Debug.WriteLine("###### Job Set to Fire at: " + startTime);

            //If statements in charged of increasing the date depending on the type
            if (period == 'S')
            {
                //This creates the trrigger, eg scheduling information
                triggerDetail = TriggerBuilder
                   .Create()
                   .WithIdentity(billPayID.ToString(), "TriggerGrp1")
                   .StartAt(startTime) //Specify when to start first fire
                    //Specify when the trigger should start to be active. Eg telling when a job should start to take effect
                   .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForTotalCount(1))
                   .Build();
            }
            else
            {
                //Set the repeat per minute indefintely here
                //This creates the trrigger, eg scheduling information
                triggerDetail = TriggerBuilder
                   .Create()
                   .WithIdentity(billPayID.ToString(), "TriggerGrp1")
                   .StartAt(startTime) //Specify when to start first fire
                    //Specify when the trigger should start to be active. Eg telling when a job should start to take effect
                   .WithSchedule(SimpleScheduleBuilder.RepeatMinutelyForever())
                    //.WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever())
                   .Build();
            }


            //Below Creates a job aswell as provide the details and information to the job as a map
            //Create Job with the name "MyJob", Group: "MyJobGroup"
            IJobDetail jobDetail = JobBuilder
                .Create(typeof(BillPayJob))
                .WithIdentity(billPayID.ToString(), "JobGroup1")
                .UsingJobData("Account_Number", accountNumber)
                .UsingJobData("Payee_ID", payeeID)
                .UsingJobData("Amount", convertedAmount)
                .UsingJobData("Date", convertedStringDate) //< can only support string as a date format, no datetime
                .UsingJobData("Period", period)
                .Build();
            //Associate trigger with job and add to schedule
            sched.ScheduleJob(jobDetail, triggerDetail);
        }

        //This method will suspend all triggers associated with the job
        public void SuspendJob(string JobName)
        {
            //pauseJob api requires a jobkey to pause the job, below is to find it
            JobKey JobKeyToPause = new JobKey(JobName, "JobGroup1");

            //Call the schduler and pause all triggers associated matching the jobkey 
            sched.PauseJob(JobKeyToPause);
        }

        public void ResumeJob(string JobName)
        {
            //pauseJob api requires a jobkey to resume the job, below is to find it
            JobKey JobKeyToResume = new JobKey(JobName, "JobGroup1");

            sched.ResumeJob(JobKeyToResume);
        }


        //This method fetches a list of all scheduled jobs in the system
        //Because of the nature of multiple uses of this method.
        //It will return a list of billpay ID's of jobs that have been schduled
        public List<int> GetAllJobs()
        {
            List<int> BillPayIDJobList = new List<int>();

            //Returns a list of jobs associated with the job group "JobGroup1"
            var groupMatcher = GroupMatcher<JobKey>.GroupContains("JobGroup1");

            //Get the keys of all of the Job s that have the given group name.
            var jobKeys = sched.GetJobKeys(groupMatcher);

            //Adding each job jey to the list which will be passed to the view
            foreach (var x in jobKeys)
            {
                BillPayIDJobList.Add(Convert.ToInt32(x.Name));
            }
            return BillPayIDJobList;
        }

        //Return a list of billpay id's of jobs which have been suspended in the system
        public List<int> GetAllPausedJobs()
        {
            //This will contain the BillPayID's of Jobs which are paused state in the system
            List<int> ListOfPausedJobs = new List<int>();

            //Returns a list of job keys associated with the job group "JobGroup1"
            //var groupMatcher = GroupMatcher<JobKey>.GroupContains("JobGroup1");

            //Get the keys of all of the Job s that have the given group name.
            //var jobKeys = sched.GetJobKeys(groupMatcher);

            var SetOfTriggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupContains("TriggerGrp1"));

            //Examine every job trigger to check what is the state of it
            //Note: Each trigger has the billpayID as its name
            foreach (var trigger in SetOfTriggerKeys)
            {

                if (sched.GetTriggerState(trigger) == TriggerState.Paused)
                {
                    Debug.WriteLine("Trigger Name: " + trigger.Name + " STATE: Is PAUSED");
                    ListOfPausedJobs.Add(Convert.ToInt32(trigger.Name));
                }
            }

            // JobKey JobKeyToCheck= new JobKey(x.ToString(), "JobGroup1");
            //sched.GetTriggersOfJob(JobKeyToCheck);
            //sched.GetTriggerKeys(groupMatcher);

            //if (sched.GetTriggerState("ddd", "TriggerGrp1") == TriggerState.Paused)

            //Boolean = sched.GetTriggerState(TriggerKey("123", "TriggerGrp1")) == TriggerState.Paused;

            //sched.GetTriggerState("wwww","www");


            // this can be used to check for a non paused job
            //TriggerState.Normal


            return ListOfPausedJobs;
        }

        /* This method returns a list of billpayID's of jobs which have the 
         * state of normal and exclude the jobs which are in a paused state
         * 
         */
        public List<int> GetAllActiveJobs()
        {

            List<int> ListOfActiveJobs = new List<int>();


            //Returns a list of job keys associated with the job group "JobGroup1"
            //var groupMatcher = GroupMatcher<JobKey>.GroupContains("JobGroup1");

            //Get the keys of all of the Job s that have the given group name.
            //var jobKeys = sched.GetJobKeys(groupMatcher);

            var SetOfTriggerKeys = sched.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupContains("TriggerGrp1"));

            //Examine every job trigger to check what is the state of it
            //Note: Each trigger has the billpayID as its name
            foreach (var trigger in SetOfTriggerKeys)
            {

                if (sched.GetTriggerState(trigger) == TriggerState.Normal)
                {
                    Debug.WriteLine("Trigger Name: " + trigger.Name + " State is NORMAL");
                    ListOfActiveJobs.Add(Convert.ToInt32(trigger.Name));
                }
            }


            return ListOfActiveJobs;
        }

    }
}