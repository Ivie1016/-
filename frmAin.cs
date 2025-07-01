using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Data.Odbc;
using ExcelDataReader;
using liymis01;

namespace LiyMIS01
{
    public partial class frmAin : Form
    {
        sqlConnect con = new sqlConnect();
        public DataSet ds = new DataSet();
        private string sql;

        public frmAin()
        {
            // 注册编码提供程序
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();
        }

        // 显示学生表数据
        private void ShowStudentData()
        {
            try
            {
                sql = "select s.ly_sno01 as 学号, s.ly_sname01 as 姓名, m.ly_mno01 as 专业编号, m.ly_mname01 as 专业名称, " +
                      "s.ly_ssex01 as 性别, s.ly_sage01 as 年龄, s.ly_place01 as 生源地, " +
                      "s.ly_year01 as 入学年份, s.ly_scredits01 as 已修学分 " +
                      "from Liy_Students01 s " +
                      "join Liy_Major01 m on s.ly_mno01 = m.ly_mno01";
                ds = con.BindDataGridView(dataGView, sql);
                dataGView.AllowUserToAddRows = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载学生数据失败: " + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 批量插入按钮点击事件
        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请输入Excel文件完整路径", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = textBox1.Text.Trim();

            if (!File.Exists(filePath))
            {
                MessageBox.Show("指定的文件不存在，请检查路径是否正确", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("只支持.xlsx格式的Excel文件", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        if (result.Tables.Count == 0)
                        {
                            MessageBox.Show("Excel文件中没有数据表", "错误",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        DataTable dt = result.Tables[0];
                        int successCount = 0;
                        int failCount = 0;
                        StringBuilder errorMessages = new StringBuilder();

                        using (OdbcConnection conn = new OdbcConnection(con.conn.ConnectionString))
                        {
                            conn.Open();
                            OdbcTransaction transaction = conn.BeginTransaction();

                            try
                            {
                                foreach (DataRow row in dt.Rows)
                                {
                                    try
                                    {
                                        // 验证必填字段（修改为专业编号）
                                        if (row["学号"] == null || string.IsNullOrWhiteSpace(row["学号"].ToString()) ||
                                            row["姓名"] == null || string.IsNullOrWhiteSpace(row["姓名"].ToString()) ||
                                            row["专业编号"] == null || string.IsNullOrWhiteSpace(row["专业编号"].ToString()) ||
                                            row["性别"] == null || string.IsNullOrWhiteSpace(row["性别"].ToString()) ||
                                            row["年龄"] == null || string.IsNullOrWhiteSpace(row["年龄"].ToString()) ||
                                            row["生源地"] == null || string.IsNullOrWhiteSpace(row["生源地"].ToString()) ||
                                            row["入学年份"] == null || string.IsNullOrWhiteSpace(row["入学年份"].ToString()))
                                        {
                                            throw new Exception("必填字段不能为空");
                                        }

                                        // 直接使用专业编号（不再需要查询专业表）
                                        string mno = row["专业编号"].ToString();

                                        // 验证专业编号是否存在
                                        string checkMajorSql = "SELECT COUNT(*) FROM Liy_Major01 WHERE ly_mno01 = '" + mno + "'";
                                        using (OdbcCommand checkCmd = new OdbcCommand(checkMajorSql, conn, transaction))
                                        {
                                            int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                                            if (count == 0)
                                            {
                                                throw new Exception($"专业编号'{mno}'不存在");
                                            }
                                        }

                                        // 构建插入SQL
                                        sql = "INSERT INTO Liy_Students01 (ly_sno01, ly_sname01, ly_ssex01, ly_sage01, " +
                                              "ly_place01, ly_scredits01, ly_year01, ly_mno01) VALUES (" +
                                              "'" + row["学号"].ToString() + "', " +
                                              "'" + row["姓名"].ToString() + "', " +
                                              "'" + row["性别"].ToString() + "', " +
                                              row["年龄"].ToString() + ", " +
                                              "'" + row["生源地"].ToString() + "', " +
                                              "0, " + // 已修学分默认为0
                                              row["入学年份"].ToString() + ", " +
                                              "'" + mno + "')";

                                        using (OdbcCommand cmd = new OdbcCommand(sql, conn, transaction))
                                        {
                                            cmd.ExecuteNonQuery();
                                        }

                                        successCount++;
                                    }
                                    catch (Exception ex)
                                    {
                                        failCount++;
                                        errorMessages.AppendLine($"学号 {row["学号"]} 插入失败: {ex.Message}");
                                    }
                                }

                                transaction.Commit();

                                string resultMessage = $"批量插入完成:\n成功: {successCount} 条\n失败: {failCount} 条";
                                if (failCount > 0)
                                {
                                    resultMessage += "\n\n错误详情:\n" + errorMessages.ToString();
                                }

                                MessageBox.Show(resultMessage, "批量插入结果",
                                    MessageBoxButtons.OK,
                                    failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

                                ShowStudentData();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                MessageBox.Show("批量插入过程中发生错误，已回滚所有更改: " + ex.Message, "错误",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入Excel文件失败: " + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void frmAin_Load(object sender, EventArgs e)
        {
            ShowStudentData();
        }
    }
}