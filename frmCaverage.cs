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
    public partial class frmCaverage : Form
    {
        private sqlConnect con = new sqlConnect();
        private DataSet ds = new DataSet();
        private string sql;

        public frmCaverage()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadCourseComboBox();
            LoadSemesterComboBox();
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
            dataGView.Columns.Add("CourseNo", "课程号");
            dataGView.Columns.Add("CourseName", "课程名称");
            dataGView.Columns.Add("Semester", "学期");
            dataGView.Columns.Add("AvgScore", "平均成绩");
            dataGView.Columns.Add("MaxScore", "最高分");
            dataGView.Columns.Add("MinScore", "最低分");
            dataGView.Columns.Add("PassRate", "及格率(%)");

            // 设置数字列格式
            dataGView.Columns["AvgScore"].DefaultCellStyle.Format = "N1";
            dataGView.Columns["PassRate"].DefaultCellStyle.Format = "N1";
        }

        // 加载课程下拉框数据
        private void LoadCourseComboBox()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                // 从课程表中获取所有课程号
                sql = "SELECT DISTINCT ly_cno01, ly_cname01 FROM liye.Liy_Courses01 ORDER BY ly_cno01";
                ds = con.Getds(sql);

                // 添加"所有课程"选项
                comboBox1.Items.Add("所有课程");

                // 添加实际课程数据
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add(row["ly_cno01"].ToString());
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载课程数据失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 加载学期下拉框数据
        private void LoadSemesterComboBox()
        {
            try
            {
                comboBox2.Items.Clear();
                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

                // 从班级表中获取所有不重复的学期
                sql = "SELECT DISTINCT ly_semester01 FROM liye.Liy_Class01 ORDER BY ly_semester01 DESC";
                ds = con.Getds(sql);

                // 添加"所有学期"选项
                comboBox2.Items.Add("所有学期");

                // 添加实际学期数据
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox2.Items.Add(row["ly_semester01"].ToString());
                }

                comboBox2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载学期数据失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 查询按钮点击事件
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedCourse = comboBox1.SelectedItem.ToString();
                string selectedSemester = comboBox2.SelectedItem.ToString();

                // 构建查询条件
                string courseCondition = selectedCourse != "所有课程" ?
                    $" AND c.ly_cno01 = '{selectedCourse}'" : "";

                string semesterCondition = selectedSemester != "所有学期" ?
                    $" AND cl.ly_semester01 = '{selectedSemester}'" : "";

                // 查询课程平均成绩统计
                sql = $@"SELECT 
                            c.ly_cno01 AS 课程号,
                            c.ly_cname01 AS 课程名称,
                            cl.ly_semester01 AS 学期,
                            AVG(sc.ly_grade01) AS 平均成绩,
                            MAX(sc.ly_grade01) AS 最高分,
                            MIN(sc.ly_grade01) AS 最低分,
                            (COUNT(CASE WHEN sc.ly_grade01 >= 60 THEN 1 END) * 100.0 / COUNT(*)) AS 及格率
                         FROM liye.Liy_Courses01 c
                         JOIN liye.Liy_Class01 cl ON c.ly_cno01 = cl.ly_cno01
                         JOIN liye.Liy_SC01 sc ON cl.ly_cno01 = sc.ly_cno01 
                            AND cl.ly_class01 = sc.ly_class01 
                            AND cl.ly_semester01 = sc.ly_semester01
                         WHERE sc.ly_grade01 IS NOT NULL
                            {courseCondition}
                            {semesterCondition}
                         GROUP BY c.ly_cno01, c.ly_cname01, cl.ly_semester01
                         ORDER BY cl.ly_semester01 DESC, 平均成绩 DESC";

                ds = con.Getds(sql);

                // 清空现有数据
                dataGView.Rows.Clear();

                // 填充DataGridView
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    dataGView.Rows.Add(
                        row["课程号"],
                        row["课程名称"],
                        row["学期"],
                        row["平均成绩"],
                        row["最高分"],
                        row["最低分"],
                        row["及格率"]
                    );
                }

                MessageBox.Show($"查询完成，共找到 {ds.Tables[0].Rows.Count} 条记录",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}