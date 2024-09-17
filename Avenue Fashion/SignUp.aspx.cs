using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using PTS_Common;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Avenue_Fashion.Pages
{
    public partial class Sign_Up : System.Web.UI.Page
    {
        private BindingList<PTSFundBrokersInfos> m_mainL = new BindingList<PTSFundBrokersInfos>();
        private Client m_Client = null;

        private ChildThreadSafeSumParames m_sumAccountInfo = null;

        private bool isOpen = false;


        SqlConnection sqlCon = new SqlConnection(@"Data Source=DESKTOP-1NPG654;Initial Catalog=CRUD;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        protected void Page_Load(object sender, EventArgs e)
        {
            errLabel.Text = "";
            scsLabel.Text = "";
            createErrLabel.Text = "";
            createScsLabel.Text = "";
        }

        protected void signInBtn_Click(object sender, EventArgs e)
        {
            //if (sqlCon.State == ConnectionState.Closed)
            //{
            //    sqlCon.Open();
            //}
            //if (userNameBox.Text == "" || passwordBox.Text == "")
            //{
            //    scsLabel.Text = "";
            //    createErrLabel.Text = "";
            //    createScsLabel.Text = "";
            //    errLabel.Text = "Please fill in all the details!";
            //}
            //else
            //{
            //    var email = userNameBox.Text;
            //    var password = passwordBox.Text;
            //    errLabel.Text = "";
            //    createErrLabel.Text = "";
            //    createScsLabel.Text = "";
            //    if (AuthenticateAdmin(email, password))
            //    {
            //        Session["email"] = email;
            //        Response.Redirect("Admin.aspx");
            //    }
            //    else
            //    {
            //        errLabel.Text = "Please enter valid information!";
            //    }
            //}

            try
            {
               
                Task.Run(() =>
                {
                    ChildAccountInfo childAccInfo = new ChildAccountInfo()
                    {
                        ChildAccountID = "00008",
                        ChildAccoutPsd = "123"
                    };

                    PTSFundBrokersInfos m_mainAccountInfo6 = new PTSFundBrokersInfos()
                    {
                        PTSBrokerName = "SimNow",
                        PTSBrokerID = "9999",
                        AccountID = "090822"
                    };

                    m_sumAccountInfo = new ChildThreadSafeSumParames()
                    {
                        ChildAccountInfo = childAccInfo,
                        PTSFundBrokersInfos = m_mainAccountInfo6
                    };

                    if (m_Client == null)
                    {
                        m_Client = new Client();

                        m_Client.OnAccount += M_Client_OnAccountInfoEvent;
                        m_Client.OnInstruments += M_Client_OnInstrumentsInfoEvent;
                        m_Client.OnOrder += M_Client_OnOrderInfoEvent;
                        m_Client.OnPosition += M_Client_OnPositionInfoEvent;
                        m_Client.OnTrade += M_Client_OnTradeInfoEvent;

                        m_Client.OnMsgInfoEvent += M_Client_OnMsgInfoEvent;
                        m_Client.OnPTSSyncData += M_Client_OnPTSSyncDataEvent;

                        m_Client.ReqConnect("8.136.208.170", 23032);
                        m_Client.ReqUserLogin(m_sumAccountInfo);
                    }
                });
            }
            catch (Exception ex)
            {
                if (m_Client != null)
                {
                    m_Client.OnMsgInfoEvent -= M_Client_OnMsgInfoEvent;

                    m_Client.OnAccount -= M_Client_OnAccountInfoEvent;
                    m_Client.OnInstruments -= M_Client_OnInstrumentsInfoEvent;
                    m_Client.OnOrder -= M_Client_OnOrderInfoEvent;
                    m_Client.OnPosition -= M_Client_OnPositionInfoEvent;
                    m_Client.OnTrade -= M_Client_OnTradeInfoEvent;

                    m_Client.OnPTSSyncData -= M_Client_OnPTSSyncDataEvent;

                    m_Client = null;
                }
                return;
            }
        }

        private void M_Client_OnTradeInfoEvent(PTSTradeField e)
        {
            Debug.WriteLine("正在查询成交明细信息...");
        }

        private void M_Client_OnPositionInfoEvent(PTSPositionField e)
        {
            Debug.WriteLine("正在查询持仓信息...");
        }

        private void M_Client_OnOrderInfoEvent(PTSOrderField e)
        {
            Debug.WriteLine("正在查询委托信息...");
        }

        private void M_Client_OnInstrumentsInfoEvent(PTSInstrumentField iL)
        {
            Debug.WriteLine("正在查询基础合约信息...");
        }

        private void M_Client_OnAccountInfoEvent(PTSTradingAccount e)
        {
            Debug.WriteLine("正在查询账户信息...");
        }

        private void M_Client_OnPTSSyncDataEvent(SyncData e)
        {
            if (e.ISSyncDataFinished)
            {
                Debug.WriteLine("交易数据同步完毕...");           
            }
        }

        private void M_Client_OnMsgInfoEvent(PTSMsgInfo serverInfo)
        {
            Debug.WriteLine(serverInfo.MsgInfos);
        }

        private bool AuthenticateAdmin(string email, string password)
        {
            if (sqlCon.State == ConnectionState.Closed)
            {
                sqlCon.Open();
            }
            var sql = "select * from Users where email = @email and Password = HashBytes('MD5', @Password)";
            var command = new SqlCommand(sql, sqlCon);
            command.Parameters.Add("@email", SqlDbType.NVarChar).Value = email;
            command.Parameters.Add("@Password", SqlDbType.VarChar).Value = password;

            var da = new SqlDataAdapter(command);
            var dt = new DataTable();
            da.Fill(dt);
            return dt.Rows.Count > 0;
        }

        protected void createAccBtn_Click(object sender, EventArgs e)
        {
            if (sqlCon.State == ConnectionState.Closed)
            {
                sqlCon.Open();
            }
            if (createUserNameBox.Text == "" || createPasswordBox.Text == "" || confirmPasswordBox.Text == "")
            {
                createScsLabel.Text = "";
                errLabel.Text = "";
                scsLabel.Text = "";
                createErrLabel.Text = "Please fill in all the details!";
            }
            else
            {
                if (createPasswordBox.Text != confirmPasswordBox.Text)
                {
                    createScsLabel.Text = "";
                    errLabel.Text = "";
                    scsLabel.Text = "";
                    createErrLabel.Text = "Password donot match!";
                }
                else
                {
                    createErrLabel.Text = "";
                    errLabel.Text = "";
                    scsLabel.Text = "";
                    SqlCommand sqlCmd = new SqlCommand("CRUDcreateOrUpdate", sqlCon);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("@UserID", (HiddenUserID.Value == "" ? 0 : Convert.ToInt32(HiddenUserID.Value)));
                    sqlCmd.Parameters.AddWithValue("@Email", createUserNameBox.Text.Trim());
                    sqlCmd.Parameters.AddWithValue("@Password", createPasswordBox.Text.Trim());
                    sqlCmd.ExecuteNonQuery();
                    sqlCon.Close();
                    string userID = HiddenUserID.Value;
                    Clear();
                    if (userID == "")
                    {
                        createScsLabel.Text = "Account created successfully";
                    }
                    else
                    {
                        createScsLabel.Text = "Account created successfully";
                    }
                }
            }
            
        }

        public void Clear()
        {
            HiddenUserID.Value = "";
            createUserNameBox.Text = createPasswordBox.Text = "";
            createScsLabel.Text = createErrLabel.Text = "";
        }
    }
}