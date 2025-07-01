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
    public partial class frmCourse : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                sql = "select ly_cno01 as 课程号, ly_cname01 as 课程名, ly_ccredits01 as 学分, " +
                      "ly_chours01 as 学时, ly_ctype01 as 考核方式 from Liy_Courses01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true;
                dataGView.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据加载失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void InitializeComboBoxes()
        {
            try
            {
                // 学分下拉框 (1-6)
                comboBox1.Items.Clear();
                for (int i = 1; i <= 6; i++)
                {
                    comboBox1.Items.Add(i);
                }
                comboBox1.SelectedIndex = 0;

                // 学时下拉框 (16-64，以8为间隔)
                comboBox2.Items.Clear();
                for (int i = 16; i <= 64; i += 8)
                {
                    comboBox2.Items.Add(i);
                }
                comboBox2.SelectedIndex = 0;

                // 考核方式下拉框
                comboBox3.Items.Clear();
                comboBox3.Items.AddRange(new string[] { "考试", "考查" });
                comboBox3.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化下拉框失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public frmCourse()
        {
            InitializeComponent();
            InitializeComboBoxes();
            SetBind();
        }

        private void button1_Click(object sender, EventArgs e) // 插入按钮
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("课程号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("课程名不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查课程号是否已存在
                sql = $"select count(*) from Liy_Courses01 where ly_cno01 = '{textBox1.Text}'";
                int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("该课程号已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 插入课程数据
                sql = $"insert into Liy_Courses01 (ly_cno01, ly_cname01, ly_ccredits01, ly_chours01, ly_ctype01) " +
                      $"values('{textBox1.Text}', '{textBox2.Text}', {comboBox1.SelectedItem}, " +
                      $"{comboBox2.SelectedItem}, '{comboBox3.SelectedItem}')";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("课程添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
                textBox2.Clear();
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                comboBox3.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("添加失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e) // 删除按钮
        {
            if (dataGView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要删除的课程！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该课程吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();

                    // 检查是否有班级关联该课程
                    sql = $"select count(*) from Liy_Class01 where ly_cno01 = '{cno}'";
                    int classCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (classCount > 0)
                    {
                        MessageBox.Show("该课程已有班级开设，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 检查是否有选课记录
                    sql = $"select count(*) from Liy_SC01 where ly_cno01 = '{cno}'";
                    int scCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (scCount > 0)
                    {
                        MessageBox.Show("该课程已有学生选课，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sql = $"delete from Liy_Courses01 where ly_cno01 = '{cno}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("课程删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("删除失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) // 保存按钮
        {
            if (dataGView.SelectedRows.Count == 0)
            {
                MessageBox.Show("请选择要修改的课程！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();
                string cname = dataGView.CurrentRow.Cells["课程名"].Value.ToString();
                string credit = dataGView.CurrentRow.Cells["学分"].Value.ToString();
                string hours = dataGView.CurrentRow.Cells["学时"].Value.ToString();
                string ctype = dataGView.CurrentRow.Cells["考核方式"].Value.ToString();

                sql = $"update Liy_Courses01 set " +
                      $"ly_cname01 = '{cname}', " +
                      $"ly_ccredits01 = {credit}, " +
                      $"ly_chours01 = {hours}, " +
                      $"ly_ctype01 = '{ctype}' " +
                      $"where ly_cno01 = '{cno}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("课程修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("修改失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e) // 退出按钮
        {
            this.Close();
        }

        private void dataGView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGView.Rows.Count)
            {
                DataGridViewRow row = dataGView.Rows[e.RowIndex];
                textBox1.Text = row.Cells["课程号"].Value.ToString();
                textBox2.Text = row.Cells["课程名"].Value.ToString();

                // 设置学分下拉框
                string credit = row.Cells["学分"].Value.ToString();
                comboBox1.SelectedItem = Convert.ToInt32(credit);

                // 设置学时下拉框
                string hours = row.Cells["学时"].Value.ToString();
                comboBox2.SelectedItem = Convert.ToInt32(hours);

                // 设置考核方式下拉框
                string ctype = row.Cells["考核方式"].Value.ToString();
                comboBox3.SelectedItem = ctype;
            }
        }
    }
}