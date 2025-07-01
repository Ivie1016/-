using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Data.Odbc; // 保留ODBC命名空间，移除SqlClient相关引用

namespace liymis01
{
    class sqlConnect
    {
        // 使用ODBC连接字符串（需替换为你的DSN和认证信息）
        string odbcStr = "Dsn=openGauss;Uid=liye;Pwd=ly@123456;";
        public OdbcConnection conn = null;

        public sqlConnect()// 构造函数，连接数据库
        {
            if (conn == null)
            {
                conn = new OdbcConnection(odbcStr);
                try
                {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("数据库连接失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void closeConnect()// 关闭连接
        {
            if (conn != null && conn.State == ConnectionState.Open)
                conn.Close();
        }

        // 从数据库中查询数据
        public DataSet Getds(string sql)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            DataSet ds = new DataSet();
            OdbcDataAdapter da = new OdbcDataAdapter(sql, conn);
            da.Fill(ds);
            conn.Close();
            return ds;
        }

        // 对数据库进行更新（增删改）
        public int OperateData(string sql)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            OdbcCommand sqlcom = new OdbcCommand(sql, conn);
            int x = sqlcom.ExecuteNonQuery();
            conn.Close();
            return x;
        }

        // 绑定DataGridView控件
        public DataSet BindDataGridView(DataGridView dgv, string sql)
        {
            if (conn.State == ConnectionState.Closed)
                conn.Open();

            OdbcDataAdapter da = new OdbcDataAdapter(sql, conn);
            DataSet ds = new DataSet();
            da.Fill(ds);
            dgv.DataSource = ds.Tables[0];
            conn.Close();
            return ds;
        }
    }
}