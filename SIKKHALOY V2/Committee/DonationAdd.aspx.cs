﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Committee
{
    public partial class DonationAdd : Page
    {
        readonly SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
      
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Session["SchoolID"] != null)
            {
                SqlCommand AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());

                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            if(HiddenCommitteeMemberId.Value != null)
            {
                AddDonationSQL.Insert();

                //clear donar info from local storage
                ScriptManager.RegisterStartupScript(this, GetType(), "clear-donar", "clearDonarInfo()", true);

                double.TryParse(PaidAmountTextBox.Text, out var paidAmount);

                if (paidAmount > 0) {
                    ReceiptSQL.Insert();

                    PaymentRecordSQL.InsertParameters["CommitteeDonationId"].DefaultValue = ViewState["CommitteeDonationId"].ToString();
                    PaymentRecordSQL.InsertParameters["CommitteeMoneyReceiptId"].DefaultValue = ViewState["CommitteeMoneyReceiptId"].ToString();
                    PaymentRecordSQL.Insert();

                    //if paid amount return to receipt
                    Response.Redirect($"./DonationReceipt.aspx?id={ViewState["CommitteeMoneyReceiptId"]}");
                }

                DonationGridView.DataBind();

                
                //clear input value
                CategoryDownList.SelectedIndex = 0;
                DonationAmountTextBox.Text = "";
                DescriptionsTextBox.Text = "";
                PromisedDateTextBox.Text = "";
                PaidAmountTextBox.Text = "";
                PaidDateTextBox.Text = "";
            }
        }


        protected void AddDonationSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["CommitteeDonationId"] = e.Command.Parameters["@CommitteeDonationId"].Value;
        }


        protected void ReceiptSQL_Inserted(object sender, SqlDataSourceStatusEventArgs e)
        {
            ViewState["CommitteeMoneyReceiptId"] = e.Command.Parameters["@CommitteeMoneyReceiptId"].Value;
        }
    }
}