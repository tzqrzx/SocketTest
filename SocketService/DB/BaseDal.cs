using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace SocketService.DB
{
   
    public class BaseDal
    {

        public static string ConnTaskModel = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public delegate void TextReportEventHandler(string UtcID,string context);
        public event TextReportEventHandler TextReport;
        public SqlConnection GetSqlConnection()
        {
            SqlConnection conn = new SqlConnection(ConnTaskModel);
            conn.Open();
            return conn;
        }

        public static SqlConnection CreateContext()
        {
            
            SqlConnection conn = new SqlConnection(ConnTaskModel);
            conn.Open();
            return conn;
        }

        #region 监听数据库数据添加命令
        public void Init()
        {
            //backgroundThread = new System.Threading.Thread(MonitoringRequests);
            //backgroundThread.Start();
            try
            {
                SqlDependency.Start(ConnTaskModel);
                SqlDependencyWatch();
                RefreshTable();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private  void SqlDependencyWatch()
        {

            string sSQL = "SELECT [Id],[FactoryID],[EquipmentID],[Type],[Message],[State],[CreatorTime],[CreatorUserId],[ReceiveTime],[DTUID]  FROM[dbo].[Task_Request] where [state] = 0";
            SqlConnection connection = null;
            SqlCommand command = null;
            try
            {
                using (connection = new SqlConnection(ConnTaskModel))
                {
                    using (command = new SqlCommand(sSQL, connection))
                    {
                        connection.Open();
                        command.CommandType = CommandType.Text;

                        SqlDependency dependency = new SqlDependency(command);
                        dependency.OnChange += new OnChangeEventHandler(SqlTableOnChange);
                        SqlDataReader sdr = command.ExecuteReader();
                        if (sdr.Read())
                        {
                           this. TextReport(sdr["DTUID"].ToString(), sdr["Message"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
        /* 資料表修改觸發Event事件處理 */
         void SqlTableOnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency dependency = sender as SqlDependency;
            dependency.OnChange -= SqlTableOnChange;

            //if (e.Info == SqlNotificationInfo.Insert ||
            //  e.Info == SqlNotificationInfo.Update ||
            //  e.Info == SqlNotificationInfo.Delete)
            //{
            //    UpdateData();
            //}
            SqlDependencyWatch();
            //if (e.Info != SqlNotificationInfo.Invalid)
            //{
            //    SqlDependencyWatch();//此处需重复注册<span style="font-family: Arial, Helvetica, sans-serif;">SqlDependency，每次注册只执行一次，SqlDependency.id可用用于验证注册唯一 编号  
            //}
            //SqlDependencyWatch();
            //RefreshTable();
        }
        private static void RefreshTable()
        {
            string sSQL = "SELECT [Id],[FunctionType],[OperationType],[Message],[CreatorTime],[CreatorUserId] FROM [dbo].[Task_Request] WHERE [IsReceive] = 0";

            DataTable datatable = new DataTable();
            using (SqlConnection connection = new SqlConnection(ConnTaskModel))
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(sSQL, connection))
                {
                    //using (SqlDataAdapter dr = new SqlDataAdapter(sSQL, connection))
                    //{
                    //    dr.Fill(datatable);
                    //    //this.Invoke((EventHandler)(delegate { dataGridView1.DataSource = datatable; }));
                    //}


                    using (SqlDataAdapter dr = new SqlDataAdapter(sSQL, connection))
                    {
                        dr.Fill(datatable);
                        foreach (DataRow item in datatable.Rows)
                        {
                            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  功能类型：" + item["FunctionType"].ToString() + " 操作方法：" + item["OperationType"].ToString());
                            //AddHandler handler = new AddHandler(Distribution);
                            //UpdateRequest(item["Id"].ToString());
                            //handler.BeginInvoke(item["Id"].ToString(), item["FunctionType"].ToString(), item["OperationType"].ToString(), item["Message"].ToString(), item["CreatorUserId"].ToString(), null, null);
                        }
                        //this.Invoke((EventHandler)(delegate { dataGridView1.DataSource = datatable; }));
                    }
                }
            }
        }
        #endregion



        protected Guid GetGuid()
        {
            return System.Guid.NewGuid();
        }

        public static Guid Zero = new Guid("00000000-0000-0000-0000-000000000000");
    }
}
