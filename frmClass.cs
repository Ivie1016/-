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
    public partial class frmClass : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                sql = "select ly_cno01 as 课程号, ly_class01 as 班级号, ly_semester01 as 学期, " +
                      "ly_ctime01 as 上课时间, ly_cplace01 as 上课地点, ly_count01 as 班级人数, " +
                      "ly_tno01 as 教师编号 from Liy_Class01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true;
                dataGView.Columns[1].ReadOnly = true;
                dataGView.Columns[2].ReadOnly = true;
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
                ds = con.Getds("select ly_cno01 from Liy_Courses01");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add(row["ly_cno01"].ToString());
                }
                if (comboBox1.Items.Count > 0)
                    comboBox1.SelectedIndex = 0;

                // 初始化教师编号下拉框
                comboBox2.Items.Clear();
                ds = con.Getds("select ly_tno01 from Liy_Teachers01");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox2.Items.Add(row["ly_tno01"].ToString());
                }
                if (comboBox2.Items.Count > 0)
                    comboBox2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化下拉框失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public frmClass()
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

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("班级号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("学期不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("上课时间不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("上课地点不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox5.Text, out int count) || count < 1 || count > 50)
            {
                MessageBox.Show("班级人数必须是1-50之间的整数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("请选择教师编号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查班级是否已存在
                sql = $"select count(*) from Liy_Class01 where ly_cno01 = '{comboBox1.SelectedItem}' " +
                      $"and ly_class01 = '{textBox1.Text}' and ly_semester01 = '{textBox2.Text}'";
                int classCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (classCount > 0)
                {
                    MessageBox.Show("该班级已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 插入班级数据
                sql = $"insert into Liy_Class01 (ly_cno01, ly_class01, ly_semester01, ly_ctime01, " +
                      $"ly_cplace01, ly_count01, ly_tno01) values('{comboBox1.SelectedItem}', " +
                      $"'{textBox1.Text}', '{textBox2.Text}', '{textBox3.Text}', " +
                      $"'{textBox4.Text}', {count}, '{comboBox2.SelectedItem}')";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("班级添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
                textBox5.Clear();
                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
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
                MessageBox.Show("请选择要删除的班级！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该班级吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();
                    string classNo = dataGView.CurrentRow.Cells["班级号"].Value.ToString();
                    string semester = dataGView.CurrentRow.Cells["学期"].Value.ToString();

                    // 检查是否有选课记录
                    sql = $"select count(*) from Liy_SC01 where ly_cno01 = '{cno}' " +
                          $"and ly_class01 = '{classNo}' and ly_semester01 = '{semester}'";
                    int scCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (scCount > 0)
                    {
                        MessageBox.Show("该班级已有学生选课，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sql = $"delete from Liy_Class01 where ly_cno01 = '{cno}' " +
                          $"and ly_class01 = '{classNo}' and ly_semester01 = '{semester}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("班级删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("请选择要修改的班级！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("上课时间不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("上课地点不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(textBox5.Text, out int count) || count < 1 || count > 50)
            {
                MessageBox.Show("班级人数必须是1-50之间的整数！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (comboBox2.SelectedItem == null)
            {
                MessageBox.Show("请选择教师编号！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string cno = dataGView.CurrentRow.Cells["课程号"].Value.ToString();
                string classNo = dataGView.CurrentRow.Cells["班级号"].Value.ToString();
                string semester = dataGView.CurrentRow.Cells["学期"].Value.ToString();

                sql = $"update Liy_Class01 set " +
                      $"ly_ctime01 = '{textBox3.Text}', " +
                      $"ly_cplace01 = '{textBox4.Text}', " +
                      $"ly_count01 = {count}, " +
                      $"ly_tno01 = '{comboBox2.SelectedItem}' " +
                      $"where ly_cno01 = '{cno}' and ly_class01 = '{classNo}' and ly_semester01 = '{semester}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("班级信息修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                textBox1.Text = row.Cells["班级号"].Value.ToString();
                textBox2.Text = row.Cells["学期"].Value.ToString();
                textBox3.Text = row.Cells["上课时间"].Value.ToString();
                textBox4.Text = row.Cells["上课地点"].Value.ToString();
                textBox5.Text = row.Cells["班级人数"].Value.ToString();
                comboBox2.SelectedItem = row.Cells["教师编号"].Value.ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 课程号选择变化时不需要特殊处理
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 教师编号选择变化时不需要特殊处理
        }
    }
}