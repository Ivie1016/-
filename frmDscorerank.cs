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
    public partial class frmDscorerank : Form
    {
        private sqlConnect con = new sqlConnect();
        private DataSet ds = new DataSet();
        private string sql;

        public frmDscorerank()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadSemesterComboBox();
            LoadMajorComboBox();
        }

        // 初始化DataGridView
        private void InitializeDataGridView()
        {
            dataGView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGView.AllowUserToAddRows = false;
            dataGView.ReadOnly = true;
            dataGView.RowHeadersVisible = false;
            dataGView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGView.Columns.Add("Rank", "排名");
            dataGView.Columns.Add("Sno", "学号");
            dataGView.Columns.Add("Sname", "姓名");
            dataGView.Columns.Add("MajorNo", "专业号");
            dataGView.Columns.Add("AvgScore", "平均成绩");
            dataGView.Columns.Add("TotalCredits", "已修学分");

            dataGView.Columns["AvgScore"].DefaultCellStyle.Format = "N1";
        }

        // 加载学期下拉框数据
        private void LoadSemesterComboBox()
        {
            try
            {
                comboBox1.Items.Clear();
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

                sql = "SELECT DISTINCT ly_semester01 FROM liye.Liy_Class01 ORDER BY ly_semester01 DESC";
                ds = con.Getds(sql);

                comboBox1.Items.Add("所有学期");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox1.Items.Add(row["ly_semester01"].ToString());
                }

                comboBox1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载学期数据失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 加载专业下拉框数据（只显示专业号）
        private void LoadMajorComboBox()
        {
            try
            {
                comboBox2.Items.Clear();
                comboBox2.DropDownStyle = ComboBoxStyle.DropDownList;

                sql = "SELECT DISTINCT ly_mno01 FROM liye.Liy_Major01 ORDER BY ly_mno01";
                ds = con.Getds(sql);

                comboBox2.Items.Add("所有专业");
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    comboBox2.Items.Add(row["ly_mno01"].ToString());
                }

                comboBox2.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载专业数据失败！错误信息: " + ex.Message,
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 查询按钮点击事件
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string selectedSemester = comboBox1.SelectedItem.ToString();
                string selectedMajor = comboBox2.SelectedItem.ToString();

                string semesterCondition = selectedSemester != "所有学期" ?
                    $" AND cl.ly_semester01 = '{selectedSemester}'" : "";

                string majorCondition = selectedMajor != "所有专业" ?
                    $" AND s.ly_mno01 = '{selectedMajor}'" : "";

                sql = $@"WITH StudentScores AS (
                            SELECT 
                                s.ly_sno01,
                                s.ly_sname01,
                                s.ly_mno01,
                                AVG(sc.ly_grade01) AS avg_score,
                                s.ly_scredits01
                            FROM liye.Liy_Students01 s
                            JOIN liye.Liy_SC01 sc ON s.ly_sno01 = sc.ly_sno01
                            JOIN liye.Liy_Class01 cl ON sc.ly_cno01 = cl.ly_cno01 
                                AND sc.ly_class01 = cl.ly_class01 
                                AND sc.ly_semester01 = cl.ly_semester01
                            WHERE sc.ly_grade01 IS NOT NULL
                                {semesterCondition}
                                {majorCondition}
                            GROUP BY s.ly_sno01, s.ly_sname01, s.ly_mno01, s.ly_scredits01
                        )
                        SELECT 
                            RANK() OVER (ORDER BY avg_score DESC) AS rank,
                            ly_sno01,
                            ly_sname01,
                            ly_mno01,
                            avg_score,
                            ly_scredits01
                        FROM StudentScores
                        ORDER BY rank";

                ds = con.Getds(sql);

                dataGView.Rows.Clear();

                int rank = 1;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    dataGView.Rows.Add(
                        rank++,
                        row["ly_sno01"],
                        row["ly_sname01"],
                        row["ly_mno01"],
                        row["avg_score"],
                        row["ly_scredits01"]
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