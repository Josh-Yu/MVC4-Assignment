using Assignment2Basic.Business_Rules;
using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Jobs
{
    [PersistJobDataAfterExecution] //<- Quartz to update the stored copy of the JobDetail's JobDataMap after the execute() method completes successfully
    [DisallowConcurrentExecution] //<- Prevents other jobs with the same jobkey to execute while this current job is running
    public class BillPayJob : IJob
    {
        private int accountNumber;
        private int payeeID;
        private double amount;
        private string date;
        private DateTime convertedDate;
        private DateTime newDate;
        private string DateFormat = "dd/MM/yyyy";
        private char period;

        public void Execute(IJobExecutionContext context)
        {

            //creating a reference to the map that contains the data for the job
            //JobDataMap datamap = context.JobDetail.JobDataMap;

            //Creates a reference to the map that is shared by the same job
            JobDataMap datamap = context.MergedJobDataMap; //<-- creates a shared datamap for this job, need for date persitance
            //Assign values from the job
            accountNumber = datamap.GetInt("Account_Number");
            payeeID = datamap.GetInt("Payee_ID");
            amount = datamap.GetDouble("Amount");

            date = datamap.GetString("Date");
            period = datamap.GetChar("Period");

            Debug.WriteLine("\n\n*** Job Fired Off for Account: " + accountNumber + " " + DateTime.Now.ToString());


            //Debug.WriteLine("###### The  Current datamap is ##### " + date);
            //NEED to convert the date string format to an actual date object inorder to perform operations on it 

            convertedDate = DateTime.ParseExact(date, DateFormat, CultureInfo.InvariantCulture);

            //This should call the billpay function to basically withdraw (B) from the bank account
            //Billpay b1 = new Billpay(accountNumber,payeeID,amount,convertedDate);

            //      Debug.WriteLine("Converted datetime object is " + convertedDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            // Debug.WriteLine("current amount " + amount);

            //Debug.WriteLine("Incrementing date and storing it in shared datamap: ");

            switch (period)
            {
                case Billpay.ONCEOFF_PERIOD:
                    Debug.WriteLine("No need to increment month for once off billpay");
                    newDate = convertedDate;
                    break;

                case Billpay.MONTHLY_PERIOD:
                    Debug.WriteLine("#### Adding 1 Month ####");
                    newDate = convertedDate.AddMonths(1);
                    //Debug.WriteLine("Result of calling addMonths()" + newDate.ToString(DateFormat, CultureInfo.InvariantCulture));
                    //newDate = convertedDate.;
                    break;

                case Billpay.QUATER_PERIOD:
                    Debug.WriteLine("#### Adding 3 months #### ");
                    newDate = convertedDate.AddMonths(3);
                    break;

                case Billpay.ANNUAL_PERIOD:
                    Debug.WriteLine("#### Adding 1 Year #####");
                    newDate = convertedDate.AddYears(1);
                    break;
            }

            /* The DATA TYPES CURRENTLY:
             * int accountNumber
             * int payeeID
             * double amount  <--You will need cast to decimal inside billpaytransaction method
             * string date     <--You will need cast to datetime object inside billpaytransaction method
             * char period
        
            */
            //Call a method in the bank business object to perform a billpay debt on the account
            Bank b1 = new Bank();
            b1.processBillPay(accountNumber, payeeID, amount, date, period);

            //Call another method to update update the BillPay table of the current Date 

            Debug.WriteLine("===== SAVING New Date For Next Iteration: " + newDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            //string s = newDate.ToString(DateFormat, CultureInfo.InvariantCulture);
            //Debug.WriteLine("Adding new amount to Map: " + amount);

            //Need to convert the date back to a string so it can be placed back into the job datamap        
            //Increments the date for the next job so it can be saved in the db
            //Changing the shared job datamap so date can value can persist with each iteration
            context.JobDetail.JobDataMap.Put("Date", newDate.ToString(DateFormat, CultureInfo.InvariantCulture));
            //context.JobDetail.JobDataMap.Put("Amount", amount);

            testJob();
            Debug.WriteLine("########## Finished running Job ##############");
        }

        public void testJob()
        {
            Debug.WriteLine("Account Number: " + accountNumber);
            Debug.WriteLine("Payee ID: " + payeeID);
            Debug.WriteLine("Amount: " + amount);
            Debug.WriteLine("Date To show on transaction: " + date);
            Debug.WriteLine("Period: " + period);
        }
    }
}