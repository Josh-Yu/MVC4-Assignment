using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Assignment2Basic.Models.AdminModels
{
    //This viewmodel will hold a list of objects of typeBillPayPayee_View,
    //This will be passed to the stop scheduled jobs so it can be displayed in a grid
    public class BillPayList_ViewModel
    {
       // public List<BillPayPayee_View> ListBillPay = new List<BillPayPayee_View>();


        //This list is required for the grid. It will contain the record/objects of the bpay record for
        //each row. each object contains properties with related information
       public List<BillPayPayee_View> ActiveBPayJobsGrid = new List<BillPayPayee_View>();



        //This list is required for the grid. It will contain the record/objects of the PAUSED bpay record for
        //each row. each object contains properties with related information
       public List<BillPayPayee_View> PausedBPayJobsGrid = new List<BillPayPayee_View>();

    }
}