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
    public partial class frmArea : Form
    {
        private sqlConnect con = new sqlConnect();
        private DataSet ds = new DataSet();
        private string sql;

        public frmArea()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadAreaComboBox();
        }

        // 初始化DataGridView
        private void InitializeDataGridView()
        {
            // 设置DataGridView属性
            dataGView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGView.AllowUserToAddRows = false;
            dataGView.ReadOnly = true;
            dataGView.RowHeadersVisible = false;
            dataGView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // 添加列
            dataGView.Columns.Add("Area", "地区");
            dataGView.Columns.Add("StudentCount", "学生人数");
            dataGView.Columns.Add("AvgAge", "平均年龄");
            dataGView.Columns.Add("TotalCredits", "总学分");
            dataGView.Columns.Add("AvgCredits", "平均学分");

            // 设置数字列格式
            dataGView.Columns["AvgAge"].DefaultCellStyle.Format = "N1";
            dataGView.Columns["AvgCredits"].DefaultCellStyle.Format = "N1";
        }

        // 加载地区下拉框数据
        private void LoadAreaComboBox()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                // 从学生表中获取所有不重复的生源地区
                sql = "SELECT DISTINCT ly_place01 FROM liye.Liy_Students01 ORDER BY ly_place01";
                ds = con.Getds(sql);

                // 添加"所有地区"选项
                comboBox1.Items.Add("所有地区");

                // 添加实际地区数据
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add(row["ly_place01"].ToString());
                }

                comboBox1.SelectedIndex = 0; // 默认选择"所有地区"
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载地区数据失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 查询按钮点击事件
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedArea = comboBox1.SelectedItem.ToString();
                string condition = "";

                if (selectedArea != "所有地区")
                {
                    condition = $" WHERE s.ly_place01 = '{selectedArea}'";
                }

                // 查询地区学生统计信息
                sql = $@"SELECT s.ly_place01 AS 地区, 
                         COUNT(*) AS 学生人数,
                         AVG(s.ly_sage01) AS 平均年龄,
                         SUM(s.ly_scredits01) AS 总学分,
                         AVG(s.ly_scredits01) AS 平均学分
                         FROM liye.Liy_Students01 s
                         {condition}
                         GROUP BY s.ly_place01
                         ORDER BY 学生人数 DESC";

                ds = con.Getds(sql);

                // 清空现有数据
                dataGView.Rows.Clear();

                // 填充DataGridView
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    dataGView.Rows.Add(
                        row["地区"],
                        row["学生人数"],
                        row["平均年龄"],
                        row["总学分"],
                        row["平均学分"]
                    );
                }

                // 在label2中显示结果数量
                label2.Text = $"查询结果共: {ds.Tables[0].Rows.Count}个地区";
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 退出按钮点击事件
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 可以在这里添加选择变化时的逻辑，如果需要的话
        }
    }
}