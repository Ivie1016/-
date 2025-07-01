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
    public partial class frmMajor : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        protected void SetBind()
        {
            try
            {
                sql = "select ly_mno01 as 专业编号, ly_mname01 as 专业名称, ly_dno01 as 所属系编号 from Liy_Major01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.Columns[0].ReadOnly = true;
                dataGView.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据加载失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public frmMajor()
        {
            InitializeComponent();
            SetBind();
        }

        private void button1_Click(object sender, EventArgs e) // 插入按钮
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("专业编号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("专业名称不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("所属系编号不能为空！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 检查专业编号是否已存在
                sql = $"select count(*) from Liy_Major01 where ly_mno01 = '{textBox1.Text}'";
                int count = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (count > 0)
                {
                    MessageBox.Show("该专业编号已存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 检查系编号是否存在
                sql = $"select count(*) from Liy_Dept01 where ly_dno01 = '{textBox3.Text}'";
                int deptCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (deptCount == 0)
                {
                    MessageBox.Show("所属系编号不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 插入专业数据
                sql = $"insert into Liy_Major01 (ly_mno01, ly_mname01, ly_dno01) " +
                      $"values('{textBox1.Text}', '{textBox2.Text}', '{textBox3.Text}')";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("专业添加成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 清空输入框
                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();
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
                MessageBox.Show("请选择要删除的专业！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("确定要删除该专业吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    string mno = dataGView.CurrentRow.Cells["专业编号"].Value.ToString();

                    // 检查是否有学生属于该专业
                    sql = $"select count(*) from Liy_Students01 where ly_mno01 = '{mno}'";
                    int studentCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                    if (studentCount > 0)
                    {
                        MessageBox.Show("该专业已有学生，不能删除！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    sql = $"delete from Liy_Major01 where ly_mno01 = '{mno}'";
                    con.OperateData(sql);
                    SetBind();
                    MessageBox.Show("专业删除成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("请选择要修改的专业！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string mno = dataGView.CurrentRow.Cells["专业编号"].Value.ToString();
                string mname = dataGView.CurrentRow.Cells["专业名称"].Value.ToString();
                string dno = dataGView.CurrentRow.Cells["所属系编号"].Value.ToString();

                // 检查系编号是否存在
                sql = $"select count(*) from Liy_Dept01 where ly_dno01 = '{dno}'";
                int deptCount = Convert.ToInt32(con.Getds(sql).Tables[0].Rows[0][0]);
                if (deptCount == 0)
                {
                    MessageBox.Show("所属系编号不存在！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                sql = $"update Liy_Major01 set " +
                      $"ly_mname01 = '{mname}', " +
                      $"ly_dno01 = '{dno}' " +
                      $"where ly_mno01 = '{mno}'";

                con.OperateData(sql);
                SetBind();
                MessageBox.Show("专业信息修改成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                textBox1.Text = row.Cells["专业编号"].Value.ToString();
                textBox2.Text = row.Cells["专业名称"].Value.ToString();
                textBox3.Text = row.Cells["所属系编号"].Value.ToString();
            }
        }
    }
}