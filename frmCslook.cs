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
    public partial class frmCslook : Form
    {
        private sqlConnect con = new sqlConnect();
        private DataSet ds = new DataSet();
        private string sql;

        public frmCslook()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadCourseComboBox();
            LoadSemesterComboBox();
        }

        // 初始化DataGridView
        private void InitializeDataGridView()
        {
            dataGView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGView.AllowUserToAddRows = false;
            dataGView.ReadOnly = true;
            dataGView.RowHeadersVisible = false;

            // 清空现有列
            dataGView.Columns.Clear();

            // 添加列
            dataGView.Columns.Add("CourseNo", "课程号");
            dataGView.Columns.Add("CourseName", "课程名称");
            dataGView.Columns.Add("ClassNo", "班级号");
            dataGView.Columns.Add("Semester", "学期");
            dataGView.Columns.Add("Teacher", "授课教师");
            dataGView.Columns.Add("Time", "上课时间");
            dataGView.Columns.Add("Place", "上课地点");
            dataGView.Columns.Add("StudentCount", "学生人数");

            // 设置列宽
            dataGView.Columns["CourseNo"].Width = 80;
            dataGView.Columns["CourseName"].Width = 150;
            dataGView.Columns["ClassNo"].Width = 80;
            dataGView.Columns["Semester"].Width = 100;
            dataGView.Columns["Teacher"].Width = 100;
            dataGView.Columns["Time"].Width = 120;
            dataGView.Columns["Place"].Width = 120;
            dataGView.Columns["StudentCount"].Width = 80;
        }

        // 加载课程下拉框数据
        private void LoadCourseComboBox()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                sql = "SELECT ly_cno01, ly_cname01 FROM liye.Liy_Courses01 ORDER BY ly_cno01";
                ds = con.Getds(sql);

                comboBox1.Items.Add("所有课程");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add($"{row["ly_cno01"]}-{row["ly_cname01"]}");
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载课程数据失败！\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 加载学期下拉框数据
        private void LoadSemesterComboBox()
        {
            try
            {
                comboBox2.Items.Clear();
                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

                sql = "SELECT DISTINCT ly_semester01 FROM liye.Liy_Class01 ORDER BY ly_semester01 DESC";
                ds = con.Getds(sql);

                comboBox2.Items.Add("所有学期");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox2.Items.Add(row["ly_semester01"].ToString());
                }

                comboBox2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载学期数据失败！\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    $" AND c.ly_cno01 = '{selectedCourse.Split('-')[0]}'" : "";

                string semesterCondition = selectedSemester != "所有学期" ?
                    $" AND cl.ly_semester01 = '{selectedSemester}'" : "";

                // 查询课程班级信息
                sql = $@"SELECT 
                            c.ly_cno01 AS 课程号,
                            c.ly_cname01 AS 课程名称,
                            cl.ly_class01 AS 班级号,
                            cl.ly_semester01 AS 学期,
                            t.ly_tname01 AS 授课教师,
                            cl.ly_ctime01 AS 上课时间,
                            cl.ly_cplace01 AS 上课地点,
                            cl.ly_count01 AS 学生人数
                         FROM liye.Liy_Class01 cl
                         JOIN liye.Liy_Courses01 c ON cl.ly_cno01 = c.ly_cno01
                         JOIN liye.Liy_Teachers01 t ON cl.ly_tno01 = t.ly_tno01
                         WHERE 1=1 {courseCondition} {semesterCondition}
                         ORDER BY cl.ly_semester01 DESC, c.ly_cno01, cl.ly_class01";

                ds = con.Getds(sql);

                // 绑定数据到DataGridView
                dataGView.Rows.Clear();
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    dataGView.Rows.Add(
                        row["课程号"],
                        row["课程名称"],
                        row["班级号"],
                        row["学期"],
                        row["授课教师"],
                        row["上课时间"],
                        row["上课地点"],
                        row["学生人数"]
                    );
                }

                MessageBox.Show($"查询完成，共找到 {ds.Tables[0].Rows.Count} 个班级",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败！\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 退出按钮点击事件
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
