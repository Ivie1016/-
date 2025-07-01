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
    public partial class frmSclass : Form
    {
        private sqlConnect con = new sqlConnect();
        private DataSet ds = new DataSet();
        private string sql;

        public frmSclass()
        {
            InitializeComponent();
            InitializeTimetableGridView();
            LoadStudentComboBox();
            LoadSemesterComboBox();
        }

        // 初始化课表DataGridView
        private void InitializeTimetableGridView()
        {
            // 清空现有设置
            dataGView.Columns.Clear();
            dataGView.Rows.Clear();
            dataGView.AutoGenerateColumns = false;

            // 设置基本属性
            dataGView.AllowUserToAddRows = false;
            dataGView.ReadOnly = true;
            dataGView.RowHeadersVisible = true;
            dataGView.ColumnHeadersVisible = true;
            dataGView.SelectionMode = DataGridViewSelectionMode.CellSelect;

            // 添加星期列（周一至周日）
            string[] weekDays = { "周一", "周二", "周三", "周四", "周五", "周六", "周日" };
            foreach (string day in weekDays)
            {
                dataGView.Columns.Add(new DataGridViewTextBoxColumn()
                {
                    Name = day,
                    HeaderText = day,
                    Width = 150
                });
            }

            // 设置行标题（1-12节课）
            dataGView.RowCount = 12;
            for (int i = 0; i < 12; i++)
            {
                dataGView.Rows[i].HeaderCell.Value = $"{i + 1}-{i + 2}节";
            }

            // 样式设置
            dataGView.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dataGView.RowHeadersWidth = 80;
            dataGView.BackgroundColor = Color.White;
        }

        // 加载学生下拉框数据
        private void LoadStudentComboBox()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                sql = "SELECT ly_sno01, ly_sname01 FROM liye.Liy_Students01 ORDER BY ly_sno01";
                ds = con.Getds(sql);

                comboBox1.Items.Add("所有学生");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add($"{row["ly_sno01"]}-{row["ly_sname01"]}");
                }
                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载学生数据失败！\n" + ex.Message,
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
                ClearTimetable();

                if (comboBox1.SelectedItem.ToString() == "所有学生")
                {
                    MessageBox.Show("请选择具体学生", "提示",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string studentNo = comboBox1.SelectedItem.ToString().Split('-')[0];
                string semesterCondition = comboBox2.SelectedItem.ToString() != "所有学期" ?
                    $" AND cl.ly_semester01 = '{comboBox2.SelectedItem}'" : "";

                // 查询学生课表
                sql = $@"SELECT c.ly_cname01 AS 课程名称, cl.ly_ctime01 AS 上课时间, 
                        cl.ly_cplace01 AS 上课地点, cl.ly_class01 AS 班级名称
                        FROM liye.Liy_SC01 sc
                        JOIN liye.Liy_Class01 cl ON sc.ly_cno01 = cl.ly_cno01 
                            AND sc.ly_class01 = cl.ly_class01 
                            AND sc.ly_semester01 = cl.ly_semester01
                        JOIN liye.Liy_Courses01 c ON cl.ly_cno01 = c.ly_cno01
                        WHERE sc.ly_sno01 = '{studentNo}' {semesterCondition}
                        ORDER BY cl.ly_semester01, cl.ly_ctime01";

                ds = con.Getds(sql);

                // 填充课表
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    string timeStr = row["上课时间"].ToString();
                    string courseInfo = $"{row["课程名称"]}\n{row["班级名称"]}\n{row["上课地点"]}";

                    if (ParseClassTime(timeStr, out string day, out int startPeriod, out int endPeriod))
                    {
                        int colIndex = DayToColumnIndex(day);
                        if (colIndex >= 0)
                        {
                            for (int period = startPeriod; period <= endPeriod; period++)
                            {
                                if (period - 1 < dataGView.RowCount)
                                {
                                    dataGView.Rows[period - 1].Cells[colIndex].Value = courseInfo;
                                    dataGView.Rows[period - 1].Cells[colIndex].Style.BackColor = Color.LightSkyBlue;
                                }
                            }
                        }
                    }
                }

                MessageBox.Show($"已加载 {ds.Tables[0].Rows.Count} 门课程",
                    "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("查询失败！\n" + ex.Message,
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 解析上课时间（示例格式："周一第1-2节"）
        private bool ParseClassTime(string timeStr, out string day, out int startPeriod, out int endPeriod)
        {
            day = "";
            startPeriod = endPeriod = 0;

            try
            {
                string[] parts = timeStr.Split(new[] { "第", "节" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    day = parts[0];
                    string[] periods = parts[1].Split('-');
                    if (periods.Length == 2 &&
                        int.TryParse(periods[0], out startPeriod) &&
                        int.TryParse(periods[1], out endPeriod))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        // 星期转换为列索引
        private int DayToColumnIndex(string day)
        {
            switch (day)
            {
                case "周一": return 0;
                case "周二": return 1;
                case "周三": return 2;
                case "周四": return 3;
                case "周五": return 4;
                case "周六": return 5;
                case "周日": return 6;
                default: return -1;
            }
        }

        // 清空课表
        private void ClearTimetable()
        {
            foreach (DataGridViewRow row in dataGView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = null;
                    cell.Style.BackColor = Color.White;
                }
            }
        }

        // 退出按钮
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
