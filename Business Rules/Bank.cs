using Assignment2Basic.Models;
using Assignment2Basic.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Assignment2Basic.Business_Rules
{
    public class Bank
    {
        const int SAVINGS_MINIMAL_BALANCE = 0;
        const int CHECKING_MINIMAL_BALANCE = 200;
        const int MAX_FREE_TRANSACTIONS = 4;

        //Connection to the Repsoitry to call operations
        CentralRepo Repo = new CentralRepo();

        /*PayBillsGet
         *CREATES VIEWMODEL FOR USER TO FILL IN DETAILS OF THE SCHEDULED JOB

         * This is triggered when the user chooses 'Pay Bills' from the dropdown list on the ATM page
         *  The purpose of this method is to create a viewmodel For the view that the user  fills in the details to 
         *   schedule an automated billpay Job
         *  Eg) a prepopulated viewmodel so the user can choose from dropdown lists of payee and accounts
         */
        public ATMPayBills_ViewModel PayBills(int sessionUserID)
        {
            //Find the customer object from EF
            Customer customerQuery = Repo.GetCustomerSingle(sessionUserID);

            //Search the Accounts table and return all the accounts objects that match the customerID of the current user
            IQueryable<Account> accountQuery = Repo.GetCustomerAccountQueryable(sessionUserID);

            IQueryable<Payee> allPayeeQuery = Repo.GetAllPayeeQueryable();

            //Creates an list of accounts owned by the user for them to select to conduct automatic billpay job
            IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            //Creates an list of Payees for the viewModel so the user can choose the payee from the dropdownlist
            IEnumerable<SelectListItem> AllPayee = allPayeeQuery.OrderBy(p => p.PayeeID).ToList().Select(p => new SelectListItem
            {
                Value = Convert.ToString(p.PayeeID),
                Text = (p.PayeeID + " - " + p.PayeeName)
            });
            //create a prepoluated viewmodel with list information from payee and accounts
            var newModel = new ATMPayBills_ViewModel()
            {
                CustomerName = customerQuery.CustomerName,
                AccountList = accounts,
                AllPayeeList = AllPayee,
                ScheduledDate = System.DateTime.Now
            };
            return newModel;
        }


        /*PayBillsPost
         *Triggered when the user submits the form for processing once they have filled in the details 
         * about the scheduled billpay job they want to schedule.
         */
        public ATMPayBills_ViewModel PayBillsPost(int sessionUserID, ATMPayBills_ViewModel models)
        {
            //Now adding the job to the schuduler to be fired off in 1minute
            Billpay BPayReference = new Billpay();
            BPayReference.manualCreateBPayJob(models.FromAccountNumber, models.Payee, models.Amount, models.Period, models.ScheduledDate);

            Customer customerQuery = Repo.GetCustomerSingle(sessionUserID);
            var accountQuery = Repo.GetCustomerAccountQueryable(sessionUserID);
            var allPayeeQuery = Repo.GetAllPayeeQueryable();

            IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            IEnumerable<SelectListItem> allPayee = allPayeeQuery.OrderBy(p => p.PayeeID).ToList().Select(p => new SelectListItem
            {
                Value = Convert.ToString(p.PayeeID),
                Text = (p.PayeeID + " - " + p.PayeeName)
            });

            models.CustomerName = customerQuery.CustomerName;
            models.AccountList = accounts;
            models.AllPayeeList = allPayee;
            models.Message = "Bill Pay Schedule Recorded/Scheduled SUCCESSFULLY";

            return models;
        }


        /* PayBillUpdateGet
         * THis method is triggered when the user clicks 'Edit' inside the grid of 
         * Scheduled billpay job aligned with the associated job they wish to modify
         * 
         * This method will generate the viewmodel to displays the view where the 
         * user would like to update the scheduled job 
         */
        public ATMPayBillsSchedule_ViewModel billUpdateGet(int sessionID, string id)
        {
            int billID = 0;
            if (id != null)
            {
                billID = int.Parse(id);
            }
            var customerQuery = Repo.GetCustomerSingle(sessionID);
            var accountQuery = Repo.GetCustomerAccountQueryable(sessionID);
            var allPayeeQuery = Repo.GetAllPayeeQueryable();
            var billPayIDQuery = Repo.GetBillPaySingle(billID);

            IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            IEnumerable<SelectListItem> allPayee = allPayeeQuery.OrderBy(p => p.PayeeID).ToList().Select(p => new SelectListItem
            {
                Value = Convert.ToString(p.PayeeID),
                Text = (p.PayeeID + " - " + p.PayeeName)
            });

            var newModel = new ATMPayBillsSchedule_ViewModel()
            {
                CustomerName = customerQuery.CustomerName,
                accountList = accounts,
                AllPayeeList = allPayee,
                AccountNumber = billPayIDQuery.AccountNumber,
                PayeeID = billPayIDQuery.PayeeID,
                Amount = billPayIDQuery.Amount,
                ScheduleDate = billPayIDQuery.ScheduleDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                Period = billPayIDQuery.Period,
            };
            return newModel;
        }


        /*PayBillUpdatePost
         *This method actually triggered when the user submit the changes they have made to the schdulked job
         * Method will modify the database and also modify the active billpay job in the schduler
         */
        public ATMPayBillsSchedule_ViewModel billUpdatePost(int sessionID, ATMPayBillsSchedule_ViewModel models, string id)
        {
            int billID = 0;
            if (id != null)
            {
                billID = int.Parse(id);
            }

            //Create a billPayObject instance with newly updated properties
            //and send it to repositry to handle the update to DB.
            BillPay UpdatedBillPayObject = new BillPay();
            DateTime ScheduleDateFormat = DateTime.ParseExact(models.ScheduleDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            UpdatedBillPayObject.BillPayID = billID;
            UpdatedBillPayObject.AccountNumber = models.AccountNumber;
            UpdatedBillPayObject.PayeeID = models.PayeeID;
            UpdatedBillPayObject.Amount = models.Amount;
            UpdatedBillPayObject.ScheduleDate = ScheduleDateFormat;
            UpdatedBillPayObject.Period = models.Period;
            UpdatedBillPayObject.ModifyDate = System.DateTime.Now;

            Repo.UpdateExistingBillPayRecord(UpdatedBillPayObject);

            //Tell the schduler to remove the job 
            //and now manually create a new job with the newly updated information
            Billpay BillPayBusinessObj = new Billpay();
            BillPayBusinessObj.ModifyJob(UpdatedBillPayObject);

            //######### Repopulating the viewmodel Below ####

            var customerQuery = Repo.GetCustomerSingle(sessionID);
            var AccountListQuery = Repo.GetCustomerAccountQueryable(sessionID);
            var allPayeeQuery = Repo.GetAllPayeeQueryable();

            IEnumerable<SelectListItem> accounts = AccountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            IEnumerable<SelectListItem> allPayee = allPayeeQuery.OrderBy(p => p.PayeeID).ToList().Select(p => new SelectListItem
            {
                Value = Convert.ToString(p.PayeeID),
                Text = (p.PayeeID + " - " + p.PayeeName)
            });

            models.CustomerName = customerQuery.CustomerName;
            models.accountList = accounts;
            models.AllPayeeList = allPayee;

            return models;
        }

        /*PayBillScheduleGet
         *This creates the viewmodel to displays the the grid of schdueled jobs for the user
         */
        public ATMBillSchedule_ViewModel billSchedule(int sessionID)
        {
            NWBAEntities db = new NWBAEntities();

            var customerQuery = Repo.GetCustomerSingle(sessionID);
            var AccountListQuery = Repo.GetCustomerAccountQueryable(sessionID);


            IEnumerable<SelectListItem> account = AccountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() : "Checkings - " + a.AccountNumber.ToString()
            });

            var newModel = new ATMBillSchedule_ViewModel
            {
                CustomerName = customerQuery.CustomerName,
                accountList = account,
                scheduleBillList = new List<BillingList>()
            };
            return newModel;
        }

        /*PayBillSchedulePost
         *User submits the view to view the pending billpay jobs
         *This post also retrieves the current jobs for the particular account and creates a list for the view to display
         */
        public ATMBillSchedule_ViewModel billSchedulePost(int sessionID, ATMBillSchedule_ViewModel models)
        {
            NWBAEntities db = new NWBAEntities();
            int accountID = models.AccountNumber;

            if (accountID != 0)
            {
                var accountQuery = Repo.GetAccount(accountID);
                var customerQuery = Repo.GetCustomerAccountQueryable(sessionID);
                var singleCustomerQuery = Repo.GetCustomerSingle(sessionID);

                IEnumerable<SelectListItem> accounts = customerQuery.OrderBy(c => c.AccountNumber).ToList().
                         Select(c => new SelectListItem
                         {
                             Value = Convert.ToString(c.AccountNumber),
                             Text = (c.AccountType.Equals("S")) ? "Saving " + c.AccountNumber.ToString()
                             : "Checkings " + " " + c.AccountNumber.ToString()
                         });

                //This fetches a list of current billpay jobs scheduled #########

                var currentJobsList = Repo.GetCurrentBillPayJobs();

                //Get all Billpay records in the system that are currently being schedueld and for the particular account
                var billList = (from a in db.BillPays
                                where a.AccountNumber.Equals(accountID) && currentJobsList.Contains(a.BillPayID)
                                select a);

                var payeeList = Repo.GetAllPayeeQueryable();


                //If the account has at least 1 schecduled billpay job
                //enter this if, Else display empty record string
                if (billList.Count() > 0)
                {
                    //select [WDT].[dbo].[Payee].PayeeName, [WDT].[dbo].[BillPay].PayeeID, 
                    //       [WDT].[dbo].[BillPay].Amount, [WDT].[dbo].[BillPay].ScheduleDate, [WDT].[dbo].[BillPay].Period
                    //from [WDT].[dbo].[Payee] 
                    //join [WDT].[dbo].[BillPay]
                    //on [WDT].[dbo].[Payee].PayeeID=[WDT].[dbo].[BillPay].PayeeID


                    //Creates a list of billing list objects which is needed by the grid to display the current scheduled jobs for the account
                    List<BillingList> BillPayGridList = new List<BillingList>();

                    // Contains an annomous object with properties from billpay and payee
                    //A list of current billpay jobs with information from payee table
                    var CustomScheduledList = (from x in db.BillPays
                                               join y in db.Payees on x.PayeeID equals y.PayeeID
                                               where x.AccountNumber.Equals(accountID) && currentJobsList.Contains(x.BillPayID)
                                               select new
                                               {
                                                   x.BillPayID,
                                                   y.PayeeName,
                                                   x.Amount,
                                                   x.ScheduleDate,
                                                   x.Period
                                               });

                    foreach (var z in CustomScheduledList)
                    {
                        string scheduleDateFormat = z.ScheduleDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

                        BillingList bl = new BillingList();
                        bl.BillPayID = z.BillPayID;
                        bl.PayeeName = z.PayeeName;
                        bl.Amount = (double)z.Amount;
                        bl.ScheduleDate = scheduleDateFormat;
                        bl.Period = z.Period;

                        BillPayGridList.Add(bl);
                    }

                    models.CustomerName = singleCustomerQuery.CustomerName;
                    models.AccountNumber = accountID;
                    models.accountList = accounts;
                    models.scheduleBillList = BillPayGridList;
                }
                else
                {
                    //This part basically creates a viewmodel that tells the user 
                    //they dont have any schduled bill pay jobs
                    List<BillingList> bList = new List<BillingList>();
                    models.CustomerName = singleCustomerQuery.CustomerName;
                    models.AccountNumber = accountID;
                    models.accountList = accounts;
                    models.scheduleBillList = bList;
                    models.Message = "Empty Record";
                }
            }
            else
            {
                billSchedule(sessionID);
            }
            return models;
        }



        //################# Background BillPay Operation

        //CREATED: BY JOSH
        /* Method is in charged of performing the scheduled bill pay operation
         * It will be called by the jobs in the schduler.
         * It will create a billpay transaction.
         */
        public void processBillPay(int _accountNumber, int _payeeID, double _amount, string _date, char _period)
        {
            int senderAccountNumber = _accountNumber;
            decimal decimalAmount = (decimal)_amount;
            decimal balanceThreshold;
            decimal newBalanceFrom;
            string billPayComment = "Automated BillPay Withdraw";
            DateTime currentDate = System.DateTime.Now;


            //Rretrieve the account object from the Account table matching the account Number
            Account FoundAccount = Repo.GetAccount(senderAccountNumber);

            //Checks if if the user transaction count is less than 4. If more than 4, then apply 30cent billpay fee
            if (calculateTransactionCount(senderAccountNumber) >= MAX_FREE_TRANSACTIONS)
            {
                newBalanceFrom = FoundAccount.Balance - decimalAmount - (decimal)0.30;
            }
            else
            {
                newBalanceFrom = FoundAccount.Balance - decimalAmount;
            }

            //Determines the minimal balance threshold depending on the account
            if (FoundAccount.AccountType.Equals("S"))
            {
                balanceThreshold = SAVINGS_MINIMAL_BALANCE;
            }
            else
            {
                balanceThreshold = CHECKING_MINIMAL_BALANCE;
            }

            //Actual Billpay withdraw operation
            if (newBalanceFrom >= balanceThreshold)
            {

                FoundAccount.Balance = newBalanceFrom;
                FoundAccount.ModifyDate = currentDate;

                Repo.UpdateExistingAccount(FoundAccount);
                // Debug.WriteLine(db.Entry(FoundAccount).State);
                //db.SaveChanges(); //<-----------------------NOT SURE IF IT WILL SAVE CHANGES

                Transaction newTransactionBillPay = new Transaction
                {
                    TransactionType = "B",
                    AccountNumber = senderAccountNumber,
                    DestAccount = _payeeID,
                    Amount = decimalAmount,
                    Comment = billPayComment,
                    ModifyDate = currentDate
                };

                Repo.AddTransaction(newTransactionBillPay);

                if (calculateTransactionCount(senderAccountNumber) >= MAX_FREE_TRANSACTIONS)
                {
                    Transaction serviceTransaction = new Transaction
                    {
                        TransactionType = "S",
                        AccountNumber = senderAccountNumber,
                        Amount = (decimal)0.30,
                        Comment = "Bill Pay Service Charge",
                        ModifyDate = currentDate
                    };
                    Repo.AddTransaction(serviceTransaction);
                }
                Debug.WriteLine("Bill Pay SUCCESSFUL.");
            }
            else
            {
                Transaction newTransactionBillPay = new Transaction
                {
                    TransactionType = "F",
                    AccountNumber = senderAccountNumber,
                    DestAccount = _payeeID,
                    Amount = decimalAmount,
                    Comment = "Bill Pay Failed",
                    ModifyDate = currentDate
                };
                Repo.AddTransaction(newTransactionBillPay);
                Debug.WriteLine("Bill Pay UNSUCCESSFUL.");
            }
        }



        //########################## DONT TOUCH BELOW UNTIL REPO FIXED
        //TransferGet
        //Transfer Page display
        public ATMTransfer_ViewModel Transfer(int sessionUserID)
        {
            NWBAEntities db = new NWBAEntities();

            var customerQuery = Repo.GetCustomerSingle(sessionUserID);
            var accountQuery = Repo.GetCustomerAccountQueryable(sessionUserID);


            if (HttpContext.Current.Session["fromAccountNumber"] != null)
            {
                int fromAccountNumber = (int)HttpContext.Current.Session["fromAccountNumber"];

                var allAccountQuery = (from x in db.Accounts
                                       where x.AccountNumber != fromAccountNumber
                                       select x);

                IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                           : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                });

                IEnumerable<SelectListItem> allAccounts = allAccountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() : "Checkings - " + a.AccountNumber.ToString()
                });


                var newModel = new ATMTransfer_ViewModel()
                {
                    CustomerName = customerQuery.CustomerName,
                    AccountList = accounts,
                    AllAccountList = allAccounts
                };
                return newModel;
            }
            else
            {
                //get all accounts EXCEPT the currently logged in user
                var allAccountQuery = Repo.GetAllAccountsExceptUser(sessionUserID);

                IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                           : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                });

                IEnumerable<SelectListItem> allAccounts = allAccountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() : "Checkings - " + a.AccountNumber.ToString()
                });


                var newModel = new ATMTransfer_ViewModel()
                {
                    CustomerName = customerQuery.CustomerName,
                    AccountList = accounts,
                    AllAccountList = allAccounts
                };
                return newModel;
            }
        }

        //TransferPost
        public ATMTransfer_ViewModel TransferPost(int sessionUserID, ATMTransfer_ViewModel models)
        {
            decimal newBalanceFrom;
            decimal balanceThreshold;
            NWBAEntities db = new NWBAEntities();
            int fromAccountNumber = models.FromAccountNumber;

            if (fromAccountNumber != 0)
            {
                int toAccountNumber = models.ToAccountNumber;

                var fromAccountQuery = Repo.GetAccount(fromAccountNumber);

                int transactionCountCheck = calculateTransactionCount(fromAccountNumber);

                decimal amount = models.Amount;


                if (transactionCountCheck >= MAX_FREE_TRANSACTIONS)
                {
                    newBalanceFrom = fromAccountQuery.Balance - amount - (decimal)0.20;
                }
                else
                {
                    newBalanceFrom = fromAccountQuery.Balance - amount;
                }

                if (fromAccountQuery.AccountType.Equals("S"))
                {
                    balanceThreshold = SAVINGS_MINIMAL_BALANCE;
                }
                else
                {
                    balanceThreshold = CHECKING_MINIMAL_BALANCE;
                }

                if (newBalanceFrom >= balanceThreshold)
                {
                    DateTime modifiedDateFrom = System.DateTime.Now;

                    Account updateAccountFrom = db.Accounts.First(i => i.AccountNumber.Equals(fromAccountNumber));
                    updateAccountFrom.Balance = newBalanceFrom;
                    updateAccountFrom.ModifyDate = modifiedDateFrom;

                    Repo.UpdateExistingAccount(updateAccountFrom);


                    Transaction newTransactionWithdraw = new Transaction
                    {
                        TransactionType = "T",
                        AccountNumber = fromAccountNumber,
                        DestAccount = toAccountNumber,
                        Amount = amount,
                        Comment = models.Comment,
                        ModifyDate = modifiedDateFrom
                    };
                    Repo.AddTransaction(newTransactionWithdraw);


                    if (transactionCountCheck >= MAX_FREE_TRANSACTIONS)
                    {
                        Transaction serviceTransaction = new Transaction
                        {
                            TransactionType = "S",
                            AccountNumber = fromAccountNumber,
                            Amount = (decimal)0.20,
                            Comment = "Service Charge",
                            ModifyDate = modifiedDateFrom
                        };
                        Repo.AddTransaction(serviceTransaction);
                    }
                    else
                    {
                        Transaction serviceTransaction = new Transaction
                        {
                            TransactionType = "S",
                            AccountNumber = fromAccountNumber,
                            Amount = (decimal)0.00,
                            Comment = "Free Service Charge",
                            ModifyDate = modifiedDateFrom
                        };
                        Repo.AddTransaction(serviceTransaction);
                    }

                    if (toAccountNumber != 0)
                    {
                        var toAccountQuery = Repo.GetAccount(toAccountNumber);

                        decimal newBalanceTo = toAccountQuery.Balance + amount;
                        DateTime modifiedDateTo = System.DateTime.Now;

                        Account updateAccountTo = db.Accounts.First(i => i.AccountNumber.Equals(toAccountNumber));
                        updateAccountTo.Balance = newBalanceTo;
                        updateAccountTo.ModifyDate = modifiedDateTo;
                        Repo.UpdateExistingAccount(updateAccountTo);

                        Transaction newTransactionDeposit = new Transaction
                        {
                            TransactionType = "D",
                            AccountNumber = toAccountNumber,
                            DestAccount = toAccountNumber,
                            Amount = amount,
                            Comment = models.Comment,
                            ModifyDate = modifiedDateTo
                        };
                        Repo.AddTransaction(newTransactionDeposit);
                    }
                    models.Message = "Transfer SUCCESSFUL.";
                }
                else
                {
                    models.Message = "Transfer UNSUCCESSFUL.";
                }


                var accountListQuery = (from x in db.Accounts
                                        where x.CustomerID.Equals(sessionUserID)
                                        select x);

                var allAccountQuery = (from x in db.Accounts
                                       where x.CustomerID.Equals(sessionUserID)
                                       select x);


                IEnumerable<SelectListItem> allAccounts = allAccountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() : "Checkings - " + a.AccountNumber.ToString()
                });

                IEnumerable<SelectListItem> accounts = accountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                });

                var customerQuery = Repo.GetCustomerSingle(sessionUserID);

                decimal formatAccountBalance = Convert.ToDecimal(string.Format("{0:0.00}", fromAccountQuery.Balance));

                models.AccountList = accounts;
                models.AllAccountList = allAccounts;
                models.CustomerName = customerQuery.CustomerName;
                models.AccountBalanceMessage = "Account Balance: " + formatAccountBalance;
            }
            return models;
        }

        //DepositGet
        public ATMDeposit_ViewModel Deposit(int sessionUserID)
        {
            var customerQuery = Repo.GetCustomerSingle(sessionUserID);
            var accountQuery = Repo.GetCustomerAccountQueryable(sessionUserID);


            IEnumerable<SelectListItem> accounts = accountQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            //Create a viewmodel and populate it with data
            var newModel = new ATMDeposit_ViewModel()
            {
                CustomerName = customerQuery.CustomerName,
                AccountList = accounts,
            };
            return newModel;
        }

        //DepositPost
        public ATMDeposit_ViewModel DepositPost(int sessionID, ATMDeposit_ViewModel models)
        {
            NWBAEntities db = new NWBAEntities();

            int accountNumber = models.ToAccountNumber;

            if (accountNumber != 0)
            {
                var accountQuery = Repo.GetAccount(accountNumber);
                decimal amount = models.Amount;

                decimal newBalance = accountQuery.Balance + amount;
                DateTime modifiedDate = System.DateTime.Now;

                Account updateAccount = Repo.GetAccount(accountNumber);
                updateAccount.Balance = newBalance;
                updateAccount.ModifyDate = modifiedDate;

                Repo.UpdateExistingAccount(updateAccount);

                Transaction newTransaction = new Transaction
                {
                    TransactionType = "D",
                    AccountNumber = accountNumber,
                    DestAccount = accountNumber,
                    Amount = amount,
                    Comment = models.Comment,
                    ModifyDate = modifiedDate
                };
                Repo.AddTransaction(newTransaction);

                models.Message = "Deposit SUCCESSFUL.";

                var accountListQuery = (from x in db.Accounts
                                        where x.CustomerID.Equals(sessionID)
                                        select x);

                IEnumerable<SelectListItem> accounts = accountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
                {
                    Value = Convert.ToString(a.AccountNumber),
                    Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                });

                var customerQuery = Repo.GetCustomerSingle(sessionID);

                decimal formatAccountBalance = Convert.ToDecimal(string.Format("{0:0.00}", accountQuery.Balance));

                models.AccountList = accounts;
                models.CustomerName = customerQuery.CustomerName;
                models.AccountBalanceMessage = "Account Balance: " + formatAccountBalance;
            }
            return models;
        }

        //WithdrawalGet
        public ATMWithdraw_ViewModel Withdrawal(int sessionUserID)
        {
            var customerQuery = Repo.GetCustomerSingle(sessionUserID);
            var customerAccountListQuery = Repo.GetCustomerAccountQueryable(sessionUserID);

            IEnumerable<SelectListItem> accounts = customerAccountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                       : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            var newModel = new ATMWithdraw_ViewModel()
            {
                CustomerName = customerQuery.CustomerName,
                AccountList = accounts,
            };

            return newModel;
        }

        //WithdrawalPost
        public ATMWithdraw_ViewModel WithdrawalPost(int sessionUserID, ATMWithdraw_ViewModel models)
        {
            NWBAEntities db = new NWBAEntities();

            int accountNumber = models.FromAccountNumber;
            var accountQuery = Repo.GetAccount(accountNumber);

            if (accountNumber != 0)
            {
                decimal amount = models.Amount;
                decimal newBalance;
                decimal balanceThreshold;
                int transactionCountCheck = calculateTransactionCount(accountNumber);

                if (transactionCountCheck >= MAX_FREE_TRANSACTIONS)
                {
                    newBalance = accountQuery.Balance - amount - (decimal)0.20;
                }
                else
                {
                    newBalance = accountQuery.Balance - amount;
                }

                if (accountQuery.AccountType.Equals("S"))
                {
                    balanceThreshold = SAVINGS_MINIMAL_BALANCE;
                }
                else
                {
                    balanceThreshold = CHECKING_MINIMAL_BALANCE;
                }

                if (newBalance >= balanceThreshold)
                {
                    DateTime modifiedDate = System.DateTime.Now;

                    Account updateAccount = db.Accounts.First(i => i.AccountNumber.Equals(accountNumber));
                    updateAccount.Balance = newBalance;
                    updateAccount.ModifyDate = modifiedDate;

                    Repo.UpdateExistingAccount(updateAccount);

                    Transaction newTransaction = new Transaction
                    {
                        TransactionType = "W",
                        AccountNumber = accountNumber,
                        Amount = amount,
                        Comment = models.Comment,
                        ModifyDate = modifiedDate
                    };
                    Repo.AddTransaction(newTransaction);


                    if (transactionCountCheck >= MAX_FREE_TRANSACTIONS)
                    {
                        Transaction serviceTransaction = new Transaction
                        {
                            TransactionType = "S",
                            AccountNumber = accountNumber,
                            Amount = (decimal)0.20,
                            Comment = "Service Charge",
                            ModifyDate = modifiedDate
                        };
                        Repo.AddTransaction(serviceTransaction);
                    }
                    else
                    {
                        Transaction serviceTransaction = new Transaction
                        {
                            TransactionType = "S",
                            AccountNumber = accountNumber,
                            Amount = (decimal)0.00,
                            Comment = "Free Service Charge",
                            ModifyDate = modifiedDate
                        };
                        Repo.AddTransaction(serviceTransaction);
                    }

                    models.Message = "Withdrawal SUCCESSFUL.";
                }
                else
                {
                    models.Message = "Withdrawal UNSUCCESSFUL.";
                }
            }
            var accountListQuery = Repo.GetCustomerAccountQueryable(sessionUserID);

            IEnumerable<SelectListItem> accounts = accountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() + " - $" + a.Balance
                                                        : "Checkings - " + a.AccountNumber.ToString() + " - $" + a.Balance
            });

            Customer customerQuery = Repo.GetCustomerSingle(sessionUserID);


            decimal formatAccountBalance = Convert.ToDecimal(string.Format("{0:0.00}", accountQuery.Balance));

            models.AccountList = accounts;
            models.CustomerName = customerQuery.CustomerName;
            models.AccountBalanceMessage = "Account Balance: " + formatAccountBalance;

            return models;
        }

        //StatementGet
        public Statement_ViewModel getStatement(int sessionID)
        {
            NWBAEntities db = new NWBAEntities();

            var AccountListQuery = Repo.GetCustomerAccountQueryable(sessionID);


            IEnumerable<SelectListItem> accounts = AccountListQuery.OrderBy(a => a.AccountNumber).ToList().Select(a => new SelectListItem
            {
                Value = Convert.ToString(a.AccountNumber),
                Text = (a.AccountType.Equals("S")) ? "Savings - " + a.AccountNumber.ToString() : "Checkings - " + a.AccountNumber.ToString()
            });

            if (HttpContext.Current.Session["accountNumber"] != null)
            {
                int accountNumber = (int)HttpContext.Current.Session["accountNumber"];
                var transList = (from a in db.Transactions
                                 where a.AccountNumber.Equals(accountNumber)
                                 select a);

                var accountQuery = (from x in db.Accounts
                                    where x.CustomerID.Equals(sessionID) && x.AccountNumber.Equals(accountNumber)
                                    select x).Single();

                var model = new Statement_ViewModel()
                {
                    accountList = accounts,
                    tranList = transList.ToList(),
                    Balance = Convert.ToDecimal(string.Format("{0:0.00}", accountQuery.Balance))
                };
                return model;
            }
            else
            {
                var transList = (from a in db.Transactions
                                 where a.AccountNumber.Equals(0)
                                 select a);

                var model = new Statement_ViewModel()
                {
                    accountList = accounts,
                    tranList = transList.ToList(),
                };
                return model;
            }
        }

        //StatementPost
        public Statement_ViewModel postStatement(int sessionID, Statement_ViewModel models)
        {
            using (NWBAEntities db = new NWBAEntities())
            {
                int accountID = models.AccountNumber;

                if (accountID != 0)
                {
                    var accountQuery = Repo.GetAccount(accountID);
                    var customerAccountsQuery = Repo.GetCustomerAccountQueryable(sessionID);

                    IEnumerable<SelectListItem> accounts = customerAccountsQuery.OrderBy(c => c.AccountNumber).ToList().
                         Select(c => new SelectListItem
                         {
                             Value = Convert.ToString(c.AccountNumber),
                             Text = (c.AccountType.Equals("S")) ? "Saving " + c.AccountNumber.ToString()
                             : "Checkings " + " " + c.AccountNumber.ToString()
                         });

                    decimal balance = 0;
                    decimal minSbalance = 0.20M;
                    decimal minCbalance = 200.20M;
                    decimal transactionFee = 0.20M;
                    decimal balanceThreshold;
                    Boolean check = false;

                    if (accountQuery.AccountType.Equals("S"))
                    {
                        if (accountQuery.Balance >= minSbalance)
                        {
                            balance = accountQuery.Balance - transactionFee;
                            check = true;
                        }
                        else
                        {
                            balance = accountQuery.Balance;
                        }
                    }
                    else if (accountQuery.AccountType.Equals("C"))
                    {
                        if (accountQuery.Balance >= minCbalance)
                        {
                            balance = accountQuery.Balance - transactionFee;
                            check = true;
                        }
                        else
                        {
                            balance = accountQuery.Balance;
                        }
                    }

                    if (accountQuery.AccountType.Equals("S"))
                    {
                        balanceThreshold = SAVINGS_MINIMAL_BALANCE;
                    }
                    else
                    {
                        balanceThreshold = CHECKING_MINIMAL_BALANCE;
                    }

                    if (balance >= balanceThreshold && check)
                    {
                        DateTime date = System.DateTime.Now;
                        Account updateAccount = db.Accounts.First(u => u.AccountNumber.Equals(accountID));
                        updateAccount.Balance = Convert.ToDecimal(string.Format("{0:0.00}", balance));
                        updateAccount.ModifyDate = date;
                        Repo.UpdateExistingAccount(updateAccount);

                        Transaction serviceTransaction = new Transaction
                        {
                            TransactionType = "S",
                            AccountNumber = accountID,
                            Amount = Convert.ToDecimal(string.Format("{0:0.00}", transactionFee)),
                            Comment = "View Statement Charge",
                            ModifyDate = date
                        };
                        Repo.AddTransaction(serviceTransaction);

                        models.retrieveMessage = "Transaction History Retrieved SUCCESSFULLY";

                        var sList = (from a in db.Transactions
                                     where a.AccountNumber.Equals(accountID)
                                     select a);

                        models.AccountNumber = accountID;
                        models.tranList = sList.ToList();
                        models.accountList = accounts;
                        models.Balance = Convert.ToDecimal(string.Format("{0:0.00}", balance));
                    }
                    else
                    {
                        models.retrieveMessage = "Transaction History Was Unable To Retrieve Due To INSUFFICIENT Amount";
                        var aList = (from a in db.Transactions
                                     where a.AccountNumber.Equals(0)
                                     select a);
                        models.AccountNumber = accountID;
                        models.accountList = accounts;
                        models.Balance = Convert.ToDecimal(string.Format("{0:0.00}", balance));
                        models.tranList = aList.ToList();
                    }
                }
                else
                {
                    return getStatement(sessionID);
                }
                return models;
            }
        }
        //######################################### HELPER METHODS ##########################################

        //Helper Methood, calculates the number of transactions they currently have in the system.
        //Needed to determine whether to apply a fee
        private int calculateTransactionCount(int accountNumber)
        {
            NWBAEntities db = new NWBAEntities();
            var transactionQuery = (from x in db.Transactions
                                    where x.AccountNumber.Equals(accountNumber) &&
                                    (x.TransactionType.Equals("W") || x.TransactionType.Equals("T") ||
                                    x.TransactionType.Equals("B"))
                                    select x);
            return transactionQuery.ToArray().Length;
        }
    }
}