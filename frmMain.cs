using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LiyMIS01;

namespace liymis01
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            SetMenuPermissions();
        }

        private void SetMenuPermissions()
        {
            string role = frmLogin.CurrentUserRole;

            // 管理员拥有所有权限
            if (role == "管理员") return;

            // 教师权限
            if (role == "教师")
            {
                学生表维护ToolStripMenuItem.Enabled = false;
                教师表维护ToolStripMenuItem.Enabled = false;
                系别表维护ToolStripMenuItem.Enabled = false;
                专业表维护ToolStripMenuItem.Enabled = false;
                课程表维护ToolStripMenuItem.Enabled = false;
                班级表维护ToolStripMenuItem.Enabled = false;
                选课表维护ToolStripMenuItem.Enabled = false;
                批量插入ToolStripMenuItem.Enabled = false;
                地区学生数统计ToolStripMenuItem.Enabled = false;
                学期成绩排名ToolStripMenuItem.Enabled = false;
                学生课表查询ToolStripMenuItem.Enabled = false;
                功能实现ToolStripMenuItem.Enabled = true; // 只保留教师课表查询和班级课程开设查询
            }

            // 学生权限
            if (role == "学生")
            {
                学生表维护ToolStripMenuItem.Enabled = false;
                教师表维护ToolStripMenuItem.Enabled = false;
                系别表维护ToolStripMenuItem.Enabled = false;
                专业表维护ToolStripMenuItem.Enabled = false;
                课程表维护ToolStripMenuItem.Enabled = false;
                班级表维护ToolStripMenuItem.Enabled = false;
                选课表维护ToolStripMenuItem.Enabled = false;
                批量插入ToolStripMenuItem.Enabled = false;
                地区学生数统计ToolStripMenuItem.Enabled = false;
                教师课表查询ToolStripMenuItem.Enabled = false;
                功能实现ToolStripMenuItem.Enabled = true; // 保留学生课表查询、班级课程开设查询、学期成绩排名、课程平均成绩
            }
        }

        // 以下是原有的菜单点击事件处理代码，保持不变
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 学生表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmStudent fStudent = new frmStudent();
            fStudent.ShowDialog();
        }

        private void 教师表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmTeacher fTeacher = new frmTeacher();
            fTeacher.ShowDialog();
        }

        private void 系别表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmDept fDept = new frmDept();
            fDept.ShowDialog();
        }

        private void 专业表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmMajor fMajor = new frmMajor();
            fMajor.ShowDialog();
        }

        private void 选课表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmSc fSc = new frmSc();
            fSc.ShowDialog();
        }

        private void 课程表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCourse fCourse = new frmCourse();
            fCourse.ShowDialog();
        }

        private void 班级表维护ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmClass fClass = new frmClass();
            fClass.ShowDialog();
        }

        private void 批量插入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAin fAin = new frmAin();
            fAin.ShowDialog();
        }

        private void 功能实现ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 空实现
        }

        private void 地区学生数统计ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmArea fArea = new frmArea();
            fArea.ShowDialog();
        }

        private void 学期成绩排名ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmDscorerank fDscorerank = new frmDscorerank();
            fDscorerank.ShowDialog();
        }

        private void 课程平均成绩ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCaverage fCaverage = new frmCaverage();
            fCaverage.ShowDialog();
        }

        private void 教师课表查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmLogin.CurrentUserRole == "教师")
            {
                // 教师只能查看自己的课表
                frmTclass fTclass = new frmTclass();
                fTclass.ShowDialog();
            }
            else
            {
                frmTclass fTclass = new frmTclass();
                fTclass.ShowDialog();
            }
        }

        private void 学生课表查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (frmLogin.CurrentUserRole == "学生")
            {
              
                frmSclass fSclass = new frmSclass();
                fSclass.ShowDialog();
            }
            else
            {
                frmSclass fSclass = new frmSclass();
                fSclass.ShowDialog();
            }
        }

        private void 班级课程开设查询ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmCslook fCslook = new frmCslook();
            fCslook.ShowDialog();
        }
    }
}