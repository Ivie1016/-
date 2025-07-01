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
    public partial class frmTeacher : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                sql = "select ly_tno01 as 教师编号, ly_tname01 as 姓名, ly_tsex01 as 性别, " +
                      "ly_tage01 as 年龄, ly_title01 as 职称, ly_phone01 as 联系电话, " +
                      "ly_dno01 as 所属系编号 from Liy_Teachers01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true;
                dataGView.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据加载失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected void InitializeComboBox()
        {
            try
            {
                // 仅初始化性别下拉框
                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(new string[] { "M", "F" });
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化下拉框失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public frmTeacher()
        {
            InitializeComponent();
            InitializeComboBox();
            SetBind();
        }

        private void button1_Click(object sender, EventArgs e) // 插入按钮
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("教师编号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("姓名不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("年龄不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox4.Text))
            {
                MessageBox.Show("职称不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("联系电话不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox6.Text))
            {
                MessageBox.Show("所属系编号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查教师编号是否已存在
                sql = $"select count(*) from Liy_Teachers01 where ly_tno01 = '{textBox1.Text}'";
                int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("该教师编号已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 验证年龄
                if (!int.TryParse(textBox3.Text, out int age) || age < 25 || age > 70)
                {
                    MessageBox.Show("年龄必须在25-70岁之间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查系编号是否存在
                sql = $"select count(*) from Liy_Dept01 where ly_dno01 = '{textBox6.Text}'";
                int deptCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (deptCount == 0)
                {
                    MessageBox.Show("所属系编号不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 插入教师数据
                sql = $"insert into Liy_Teachers01 (ly_tno01, ly_tname01, ly_tsex01, ly_tage01, ly_title01, ly_phone01, ly_dno01) " +
                      $"values('{textBox1.Text}', '{textBox2.Text}', '{comboBox1.SelectedItem}', {age}, " +
                      $"'{textBox4.Text}', '{textBox5.Text}', '{textBox6.Text}')";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("教师添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
                textBox4.Clear();
                textBox5.Clear();
                textBox6.Clear();
                comboBox1.SelectedIndex = 0;
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
                MessageBox.Show("请选择要删除的教师！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该教师吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string tno = dataGView.CurrentRow.Cells["教师编号"].Value.ToString();

                    // 检查是否是系主任
                    sql = $"select count(*) from Liy_Dept01 where ly_dheader01 = '{tno}'";
                    int isHeader = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (isHeader > 0)
                    {
                        MessageBox.Show("该教师是系主任，不能删除！请先更换系主任。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 检查是否有授课班级
                    sql = $"select count(*) from Liy_Class01 where ly_tno01 = '{tno}'";
                    int classCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (classCount > 0)
                    {
                        MessageBox.Show("该教师有授课班级，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sql = $"delete from Liy_Teachers01 where ly_tno01 = '{tno}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("教师删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("请选择要修改的教师！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string tno = dataGView.CurrentRow.Cells["教师编号"].Value.ToString();
                string tname = dataGView.CurrentRow.Cells["姓名"].Value.ToString();
                string tsex = dataGView.CurrentRow.Cells["性别"].Value.ToString();
                string tage = dataGView.CurrentRow.Cells["年龄"].Value.ToString();
                string title = dataGView.CurrentRow.Cells["职称"].Value.ToString();
                string phone = dataGView.CurrentRow.Cells["联系电话"].Value.ToString();
                string dno = dataGView.CurrentRow.Cells["所属系编号"].Value.ToString();

                // 验证年龄
                if (!int.TryParse(tage, out int age) || age < 25 || age > 70)
                {
                    MessageBox.Show("年龄必须在25-70岁之间！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查系编号是否存在
                sql = $"select count(*) from Liy_Dept01 where ly_dno01 = '{dno}'";
                int deptCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (deptCount == 0)
                {
                    MessageBox.Show("所属系编号不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                sql = $"update Liy_Teachers01 set " +
                      $"ly_tname01 = '{tname}', " +
                      $"ly_tsex01 = '{tsex}', " +
                      $"ly_tage01 = {age}, " +
                      $"ly_title01 = '{title}', " +
                      $"ly_phone01 = '{phone}', " +
                      $"ly_dno01 = '{dno}' " +
                      $"where ly_tno01 = '{tno}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("教师信息修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                textBox1.Text = row.Cells["教师编号"].Value.ToString();
                textBox2.Text = row.Cells["姓名"].Value.ToString();
                comboBox1.SelectedItem = row.Cells["性别"].Value.ToString();
                textBox3.Text = row.Cells["年龄"].Value.ToString();
                textBox4.Text = row.Cells["职称"].Value.ToString();
                textBox5.Text = row.Cells["联系电话"].Value.ToString();
                textBox6.Text = row.Cells["所属系编号"].Value.ToString();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}