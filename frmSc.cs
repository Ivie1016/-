using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using liymis01;

namespace LiyMIS01
{
    public partial class frmSc : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                sql = "select ly_cno01 as 课程号, ly_class01 as 班级号, ly_semester01 as 学期, " +
                      "ly_sno01 as 学号, ly_grade01 as 成绩 from Liy_SC01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true;
                dataGView.Columns[1].ReadOnly = true;
                dataGView.Columns[2].ReadOnly = true;
                dataGView.Columns[3].ReadOnly = true;
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
                // 初始化课程号下拉框
                comboBox1.Items.Clear();
                ds = con.Getds("select distinct ly_cno01 from Liy_Class01");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add(row["ly_cno01"].ToString());
                }
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;

                // 初始化班级号下拉框(根据课程号变化)
                UpdateClassComboBox();

                // 初始化学期下拉框(根据课程号和班级号变化)
                UpdateSemesterComboBox();

                // 初始化学号下拉框
                comboBox4.Items.Clear();
                ds = con.Getds("select ly_sno01 from Liy_Students01");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox4.Items.Add(row["ly_sno01"].ToString());
                }
                if (comboBox4.Items.Count > 0)
                    comboBox4.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化下拉框失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateClassComboBox()
        {
            if (comboBox1.SelectedItem != null)
            {
                comboBox2.Items.Clear();
                sql = $"select distinct ly_class01 from Liy_Class01 where ly_cno01 = '{comboBox1.SelectedItem}'";
                ds = con.Getds(sql);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox2.Items.Add(row["ly_class01"].ToString());
                }
                if (comboBox2.Items.Count > 0)
                    comboBox2.SelectedIndex = 0;
                UpdateSemesterComboBox();
            }
        }

        private void UpdateSemesterComboBox()
        {
            if (comboBox1.SelectedItem != null && comboBox2.SelectedItem != null)
            {
                comboBox3.Items.Clear();
                sql = $"select distinct ly_semester01 from Liy_Class01 " +
                      $"where ly_cno01 = '{comboBox1.SelectedItem}' and ly_class01 = '{comboBox2.SelectedItem}'";
                ds = con.Getds(sql);
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox3.Items.Add(row["ly_semester01"].ToString());
                }
                if (comboBox3.Items.Count > 0)
                    comboBox3.SelectedIndex = 0;
            }
        }

        public frmSc()
        {
            InitializeComponent();
            InitializeComboBoxes();
            SetBind();
        }

        private void button1_Click(object sender, EventArgs e) // 插入按钮
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请选择课程号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("请选择班级号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox3.SelectedItem == null)
            {
                MessageBox.Show("请选择学期！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox4.SelectedItem == null)
            {
                MessageBox.Show("请选择学号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox1.Text, out int grade) || grade < 1 || grade > 100)
            {
                MessageBox.Show("成绩必须是1-100之间的整数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查选课记录是否已存在
                sql = $"select count(*) from Liy_SC01 where ly_cno01 = '{comboBox1.SelectedItem}' " +
                      $"and ly_class01 = '{comboBox2.SelectedItem}' and ly_semester01 = '{comboBox3.SelectedItem}' " +
                      $"and ly_sno01 = '{comboBox4.SelectedItem}'";
                int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("该选课记录已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查学生是否已选该课程(不同班级)
                sql = $"select count(*) from Liy_SC01 where ly_cno01 = '{comboBox1.SelectedItem}' " +
                      $"and ly_sno01 = '{comboBox4.SelectedItem}'";
                int courseCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (courseCount > 0)
                {
                    MessageBox.Show("该学生已选过此课程(不同班级)！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 插入选课数据
                sql = $"insert into Liy_SC01 (ly_cno01, ly_class01, ly_semester01, ly_sno01, ly_grade01) " +
                      $"values('{comboBox1.SelectedItem}', '{comboBox2.SelectedItem}', " +
                      $"'{comboBox3.SelectedItem}', '{comboBox4.SelectedItem}', {grade})";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("选课记录添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
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
                MessageBox.Show("请选择要删除的选课记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该选课记录吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();
                    string classNo = dataGView.CurrentRow.Cells["班级号"].Value.ToString();
                    string semester = dataGView.CurrentRow.Cells["学期"].Value.ToString();
                    string sno = dataGView.CurrentRow.Cells["学号"].Value.ToString();

                    sql = $"delete from Liy_SC01 where ly_cno01 = '{cno}' " +
                          $"and ly_class01 = '{classNo}' and ly_semester01 = '{semester}' " +
                          $"and ly_sno01 = '{sno}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("选课记录删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("请选择要修改的选课记录！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox1.Text, out int grade) || grade < 1 || grade > 100)
            {
                MessageBox.Show("成绩必须是1-100之间的整数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();
                string classNo = dataGView.CurrentRow.Cells["班级号"].Value.ToString();
                string semester = dataGView.CurrentRow.Cells["学期"].Value.ToString();
                string sno = dataGView.CurrentRow.Cells["学号"].Value.ToString();

                sql = $"update Liy_SC01 set ly_grade01 = {grade} " +
                      $"where ly_cno01 = '{cno}' and ly_class01 = '{classNo}' " +
                      $"and ly_semester01 = '{semester}' and ly_sno01 = '{sno}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("成绩修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                comboBox1.SelectedItem = row.Cells["课程号"].Value.ToString();
                comboBox2.SelectedItem = row.Cells["班级号"].Value.ToString();
                comboBox3.SelectedItem = row.Cells["学期"].Value.ToString();
                comboBox4.SelectedItem = row.Cells["学号"].Value.ToString();
                textBox1.Text = row.Cells["成绩"].Value.ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateClassComboBox();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSemesterComboBox();
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 学期选择变化时不需要特殊处理
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 学号选择变化时不需要特殊处理
        }
    }
}