using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace liymis01
{
    public partial class frmLogin : Form
    {
        public static string CurrentUserRole { get; private set; }
        public static string CurrentUserID { get; private set; }

        public frmLogin()
        {
            InitializeComponent();
            comboBox1.Items.AddRange(new string[] { "管理员", "教师", "学生" });
            comboBox1.SelectedIndex = 0;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (txtName.Text == "" || txtPwd.Text == "")
            {
                MessageBox.Show("用户名或密码不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string role = comboBox1.SelectedItem.ToString();
            string username = txtName.Text.Trim();
            string password = txtPwd.Text.Trim();

            // 管理员验证
            if (role == "管理员" && username == "liye" && password == "liye")
            {
                CurrentUserRole = "管理员";
                CurrentUserID = "admin";
                frmMain fmain = new frmMain();
                fmain.Show();
                this.Hide();
                return;
            }

            // 教师和学生验证
            sqlConnect sqlconn = new sqlConnect();
            try
            {
                string sql = "";
                if (role == "教师")
                {
                    sql = $"SELECT ly_tno01 FROM Liy_Teachers01 WHERE ly_tno01='{username}' AND ly_tno01='{password}'";
                }
                else if (role == "学生")
                {
                    sql = $"SELECT ly_sno01 FROM Liy_Students01 WHERE ly_sno01='{username}' AND ly_sno01='{password}'";
                }

                DataSet ds = sqlconn.Getds(sql);
                if (ds.Tables[0].Rows.Count > 0)
                {
                    CurrentUserRole = role;
                    CurrentUserID = username;
                    frmMain fmain = new frmMain();
                    fmain.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("用户名或密码错误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("登录验证出错：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                sqlconn.closeConnect();
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}