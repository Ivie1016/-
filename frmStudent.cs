using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace liymis01
{
    public partial class frmStudent : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                // 查询学生信息并关联专业名称
                sql = "SELECT s.ly_sno01 AS 学号, s.ly_sname01 AS 姓名, " +
                      "s.ly_ssex01 AS 性别, s.ly_sage01 AS 年龄, " +
                      "s.ly_place01 AS 地区, s.ly_year01 AS 入学年份, " +
                      "s.ly_mno01 AS 专业编号, m.ly_mname01 AS 专业名称 " +
                      "FROM Liy_Students01 s " +
                      "JOIN Liy_Major01 m ON s.ly_mno01 = m.ly_mno01 " +
                      "ORDER BY s.ly_sno01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true; // 学号设为只读
                dataGView.AllowUserToAddRows = false; // 禁止直接添加行
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据加载失败: {ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        protected void ComboBoxBind()
        {
            try
            {
                // 绑定性别下拉框
                comboBox2.Items.AddRange(new object[] { "M", "F" });
                comboBox2.SelectedIndex = 0;

                // 绑定专业编号下拉框（直接显示编号）
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.Items.Clear();
                ds = con.Getds("SELECT ly_mno01 FROM Liy_Major01");
                comboBox1.DataSource = ds.Tables[0];
                comboBox1.DisplayMember = "ly_mno01";
                comboBox1.ValueMember = "ly_mno01";
                comboBox1.SelectedIndex = 0;

                // 绑定入学年份下拉框
                comboBox3.Items.Clear();
                int currentYear = DateTime.Now.Year;
                for (int year = currentYear - 10; year <= currentYear + 5; year++)
                {
                    comboBox3.Items.Add(year);
                }
                comboBox3.SelectedItem = currentYear;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化下拉框失败: {ex.Message}", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public frmStudent()
        {
            InitializeComponent();
            ComboBoxBind();
            SetBind();
        }

        private void button1_Click(object sender, EventArgs e) // 插入按钮
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)) // 学号
            {
                MessageBox.Show("学号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查学号是否已存在
                sql = $"SELECT COUNT(*) FROM Liy_Students01 WHERE ly_sno01 = '{textBox1.Text}'";
                int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("该学号已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 获取专业编号并验证长度
                string mno = comboBox1.SelectedValue.ToString();
                if (mno.Length > 6)
                {
                    // 截断到6位并提示
                    mno = mno.Substring(0, 6);
                    MessageBox.Show("专业编号超过6位，已自动截断", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // 插入学生数据
                sql = $"INSERT INTO Liy_Students01 (ly_sno01, ly_sname01, ly_ssex01, ly_sage01, ly_place01, ly_year01, ly_mno01) " +
                $"VALUES ('{textBox1.Text}', '{textBox2.Text}', '{comboBox2.SelectedItem}', {textBox3.Text}, " +
                $"'{textBox4.Text}', {comboBox3.SelectedItem}, '{mno}')";

                con.OperateData(sql);
                SetBind(); // 刷新显示
                MessageBox.Show("添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e) // 删除按钮
        {
            if (dataGView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要删除的行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该条信息吗？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string sno = dataGView.CurrentRow.Cells["学号"].Value.ToString();

                    // 检查是否有选课记录
                    sql = $"SELECT COUNT(*) FROM Liy_SC01 WHERE ly_sno01 = '{sno}'";
                    int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (count > 0)
                    {
                        MessageBox.Show("该学生有选课记录，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sql = $"DELETE FROM Liy_Students01 WHERE ly_sno01 = '{sno}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // 保存按钮
        {
            if (dataGView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要修改的行！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string sno = dataGView.CurrentRow.Cells["学号"].Value.ToString();
                string name = dataGView.CurrentRow.Cells["姓名"].Value.ToString();
                string sex = dataGView.CurrentRow.Cells["性别"].Value.ToString();
                string age = dataGView.CurrentRow.Cells["年龄"].Value.ToString();
                string place = dataGView.CurrentRow.Cells["地区"].Value.ToString();
                string year = dataGView.CurrentRow.Cells["入学年份"].Value.ToString();

                // 获取专业编号并验证长度
                string mno = comboBox1.SelectedValue.ToString();
                if (mno.Length > 6)
                {
                    // 截断到6位并提示
                    mno = mno.Substring(0, 6);
                    MessageBox.Show("专业编号超过6位，已自动截断", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                sql = $"UPDATE Liy_Students01 SET " +
                      $"ly_sname01 = '{name}', " +
                      $"ly_ssex01 = '{sex}', " +
                      $"ly_sage01 = {age}, " +
                      $"ly_place01 = '{place}', " +
                      $"ly_year01 = {year}, " +
                      $"ly_mno01 = '{mno}' " +
                      $"WHERE ly_sno01 = '{sno}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修改失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e) // 退出按钮
        {
            this.Close();
        }

        private void dataGView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGView.Rows[e.RowIndex];
                textBox1.Text = row.Cells["学号"].Value.ToString();
                textBox2.Text = row.Cells["姓名"].Value.ToString();
                comboBox2.SelectedItem = row.Cells["性别"].Value.ToString();
                textBox3.Text = row.Cells["年龄"].Value.ToString();
                textBox4.Text = row.Cells["地区"].Value.ToString();
                comboBox3.SelectedItem = row.Cells["入学年份"].Value.ToString();

                // 设置专业编号下拉框
                string mno = row.Cells["专业编号"].Value.ToString();
                for (int i = 0; i < comboBox1.Items.Count; i++)
                {
                    DataRowView drv = (DataRowView)comboBox1.Items[i];
                    if (drv["ly_mno01"].ToString() == mno)
                    {
                        comboBox1.SelectedIndex = i;
                        break;
                    }
                }
            }
        }
    }
}