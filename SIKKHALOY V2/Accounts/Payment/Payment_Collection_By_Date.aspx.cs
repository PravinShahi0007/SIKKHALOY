﻿using EDUCATION.COM.PaymentDataSetTableAdapters;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EDUCATION.COM.Accounts.Payment
{
    public partial class Payment_Collection_By_Date : System.Web.UI.Page
    {
        SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["EducationConnectionString"].ToString());
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                SelectedAccount();
            }
        }
        private string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        protected void MSNLinkButton_Command(object sender, CommandEventArgs e)
        {
            PaidRecordsSQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ReceivedBySQL.SelectParameters["MoneyReceiptID"].DefaultValue = e.CommandArgument.ToString();
            ScriptManager.RegisterStartupScript(this, this.GetType(), "Pop", "openModal();", true);
        }

        //Instant Payorder
        protected void OthersPaymentButton_Click(object sender, EventArgs e)
        {
            OthersPaymentSQL.InsertParameters["StudentID"].DefaultValue = StudentInfoFormView.DataKey["StudentID"].ToString();
            OthersPaymentSQL.InsertParameters["ClassID"].DefaultValue = StudentInfoFormView.DataKey["ClassID"].ToString();
            OthersPaymentSQL.InsertParameters["StudentClassID"].DefaultValue = StudentInfoFormView.DataKey["StudentClassID"].ToString();
            OthersPaymentSQL.InsertParameters["EducationYearID"].DefaultValue = StudentInfoFormView.DataKey["EducationYearID"].ToString();
            OthersPaymentSQL.Insert();

            PayRoleDropDownList.SelectedIndex = 0;
            OPayforTextBox.Text = "";
            OAmountTextBox.Text = "";
            OConcessiontBox.Text = "";
            DueGridView.DataBind();
            ScriptManager.RegisterStartupScript(this, GetType(), "Msg", "Success();", true);
        }
        protected void DueGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DateTime Startdate = Convert.ToDateTime(DueGridView.DataKeys[e.Row.DataItemIndex]["StartDate"]);
                DateTime Endtdate = Convert.ToDateTime(e.Row.Cells[5].Text);

                if (Endtdate < DateTime.Today)
                {
                    e.Row.CssClass = "PresentDue";
                }
                else
                {
                    if (Startdate == Endtdate && Startdate == DateTime.Today)
                        e.Row.CssClass = "OthersPamnt";
                }
            }
        }

        protected void OtherSessionGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DateTime Startdate = Convert.ToDateTime(OtherSessionGridView.DataKeys[e.Row.DataItemIndex]["StartDate"]);
                DateTime Endtdate = Convert.ToDateTime(e.Row.Cells[5].Text);

                if (Endtdate < DateTime.Today)
                {
                    e.Row.CssClass = "PresentDue";
                }
            }
        }

        protected void SelectedAccount()
        {
            try
            {
                SqlCommand AccountCmd = new SqlCommand("Select AccountID from Account where SchoolID = @SchoolID AND Default_Status = 'True'", con);
                AccountCmd.Parameters.AddWithValue("@SchoolID", Session["SchoolID"].ToString());
                con.Open();
                object AccountID = AccountCmd.ExecuteScalar();
                con.Close();

                if (AccountID != null)
                    AccountDropDownList.SelectedValue = AccountID.ToString();
            }
            catch { Response.Redirect("~/Login.aspx"); }
        }

        //Payment button
        protected void PayButton_Click(object sender, EventArgs e)
        {
            OrdersTableAdapter Payment_DataSet = new OrdersTableAdapter();

            double TotalPaid = 0;
            int MoneyReceiptID = 0;
            int StudentClassID = 0;

            StudentClassID = Convert.ToInt32(StudentInfoFormView.DataKey["StudentClassID"]);
            int StudentID = Convert.ToInt32(StudentInfoFormView.DataKey["StudentID"]);

            int Crrent_EduYearID = Convert.ToInt32(Session["Edu_Year"].ToString());
            int SchoolID = Convert.ToInt32(Session["SchoolID"].ToString());
            int RegistrationID = Convert.ToInt32(Session["RegistrationID"].ToString());

            DateTime Paid_Date = Convert.ToDateTime(Paid_Date_TextBox.Text);

            bool Is_Paid = false;
            bool MoneyReceipt_InsertChack = true;

            //Current Session GV
            foreach (GridViewRow row in DueGridView.Rows)
            {
                CheckBox DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                TextBox DueAmountTextBox = (TextBox)row.FindControl("DueAmountTextBox");

                int PayOrderID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["PayOrderID"]);

                double PaidAmount;
                double DueByPayOrder = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                if (DueCheckBox.Checked && double.TryParse(DueAmountTextBox.Text.Trim(), out PaidAmount))
                {
                    if (PaidAmount > DueByPayOrder)
                    {
                        MoneyReceipt_InsertChack = false;
                    }
                }
            }

            //Others Current Session GV
            foreach (GridViewRow row in OtherSessionGridView.Rows)
            {
                CheckBox DueCheckBox = (CheckBox)row.FindControl("Other_Session_CheckBox");
                TextBox DueAmountTextBox = (TextBox)row.FindControl("Other_Session_AmountTextBox");

                int PayOrderID = Convert.ToInt32(OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"]);

                double PaidAmount;
                double DueByPayOrder = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));


                if (DueCheckBox.Checked && double.TryParse(DueAmountTextBox.Text.Trim(), out PaidAmount))
                {
                    if (PaidAmount > DueByPayOrder)
                    {
                        MoneyReceipt_InsertChack = false;
                    }
                }
            }


            if (MoneyReceipt_InsertChack)
            {
                MoneyReceiptID = Convert.ToInt32(Payment_DataSet.Insert_MoneyReceipt(StudentID, RegistrationID, StudentClassID, Crrent_EduYearID, "Institution", Paid_Date, SchoolID));
                MoneyReceipt_InsertChack = false;

                //Current Session GV
                foreach (GridViewRow row in DueGridView.Rows)
                {
                    CheckBox DueCheckBox = (CheckBox)row.FindControl("DueCheckBox");
                    TextBox DueAmountTextBox = (TextBox)row.FindControl("DueAmountTextBox");

                    StudentClassID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["StudentClassID"]);
                    int PayOrderID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["PayOrderID"]);
                    int RoleID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["RoleID"]);
                    int P_Order_EduYearID = Convert.ToInt32(DueGridView.DataKeys[row.RowIndex]["EducationYearID"]);

                    double PaidAmount;
                    double DueByPayOrder = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                    if (DueCheckBox.Checked && double.TryParse(DueAmountTextBox.Text.Trim(), out PaidAmount))
                    {
                        if (PaidAmount <= DueByPayOrder)
                        {
                            Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, DueGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), Paid_Date, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, Convert.ToInt32(AccountDropDownList.SelectedValue));
                            Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);

                            TotalPaid += PaidAmount;
                            Is_Paid = true;
                            DueCheckBox.Checked = false;
                        }
                    }
                }


                //Others Current Session GV
                foreach (GridViewRow row in OtherSessionGridView.Rows)
                {
                    CheckBox DueCheckBox = (CheckBox)row.FindControl("Other_Session_CheckBox");
                    TextBox DueAmountTextBox = (TextBox)row.FindControl("Other_Session_AmountTextBox");

                    StudentClassID = Convert.ToInt32(OtherSessionGridView.DataKeys[row.RowIndex]["StudentClassID"]);
                    int PayOrderID = Convert.ToInt32(OtherSessionGridView.DataKeys[row.RowIndex]["PayOrderID"]);
                    int RoleID = Convert.ToInt32(OtherSessionGridView.DataKeys[row.RowIndex]["RoleID"]);
                    int P_Order_EduYearID = Convert.ToInt32(OtherSessionGridView.DataKeys[row.RowIndex]["EducationYearID"]);

                    double PaidAmount;
                    double DueByPayOrder = Convert.ToDouble(Payment_DataSet.DueByPayOrderID(PayOrderID));

                    if (DueCheckBox.Checked && double.TryParse(DueAmountTextBox.Text.Trim(), out PaidAmount))
                    {
                        if (PaidAmount <= DueByPayOrder)
                        {
                            Payment_DataSet.Insert_Payment_Record(StudentID, RegistrationID, RoleID, PayOrderID, PaidAmount, OtherSessionGridView.DataKeys[row.RowIndex]["PayFor"].ToString(), Paid_Date, MoneyReceiptID, StudentClassID, P_Order_EduYearID, SchoolID, Convert.ToInt32(AccountDropDownList.SelectedValue));
                            Payment_DataSet.Update_payOrder(PaidAmount, PayOrderID);

                            TotalPaid += PaidAmount;
                            Is_Paid = true;
                            DueCheckBox.Checked = false;
                        }
                    }
                }
            }

            Payment_DataSet.Update_MoneyReceipt(TotalPaid, MoneyReceiptID);

            if (Is_Paid)
            {
                string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(MoneyReceiptID)));
                string Sid = HttpUtility.UrlEncode(Encrypt(SearchIDTextBox.Text.Trim()));
                Response.Redirect(string.Format("Money_Receipt_By_Date.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
            }
        }

        protected void Print_LinkButton_Command(object sender, CommandEventArgs e)
        {
            string MRid = HttpUtility.UrlEncode(Encrypt(Convert.ToString(e.CommandArgument)));
            string Sid = HttpUtility.UrlEncode(Encrypt(StudentInfoFormView.DataKey["ID"].ToString()));
            Response.Redirect(string.Format("Money_Receipt_By_Date.aspx?mN_R={0}&s_icD={1}", MRid, Sid));
        }
    }
}