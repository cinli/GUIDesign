﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Model;
using BLL;

namespace GUI.UI
{
    public partial class CheckStudentsVolunteer : Form
    {
        //教师选志愿介绍：只能一次登录完成，暂时不能存储选到一半的数据，
        //如果选到一半，退出系统重进的话，只能重新选择，且提交后不能再改动
        //变量定义
        TeacherManager tm = new TeacherManager();
        public string teaNo = LoginInterface.loginid;// 当前登录用户的账号  非常重要
        public static string restuno = null;//存储点击单元格返回的单元格内的数据
        public static string  regroupid =null;//存储点击单元格返回的单元格内的数据
        public DataTable dt = new DataTable();//暂存教师志愿的datatable


        private static CheckStudentsVolunteer formInstance;
        public static CheckStudentsVolunteer GetIntance
        {
            get
            {
                if (formInstance != null)
                {
                    return formInstance;
                }
                else
                {
                    formInstance = new CheckStudentsVolunteer();
                    return formInstance;
                }
            }
        }
        public CheckStudentsVolunteer()
        {
            InitializeComponent();
        }

        private void Form29_Load(object sender, EventArgs e)
        {
            dataGridView1.Enabled = true;
            //DataTable dt = tm.selectStuVol();
            dtStuvol = tm.selectStuVol();//页面载入时从数据库获取当前数据库中的所有学生志愿数据并显示在datagridview1中
            dataGridView1.DataSource = dtStuvol;
            dt = tm.dtTeaVol();//页面载入时从数据库获取当前数据库中的所有教师志愿数据
            //设置dt的主键，才能把dt的更新提交到数据库
            dt.PrimaryKey = new DataColumn[2] { dt.Columns["Teano"], dt.Columns["Groupid"]};
            //以下是针对datagridview的显示设置
            dataGridView1.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;//标题居中
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
        }

        #region 双击学生单元格，查看学生详细信息
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                int CIndex = e.ColumnIndex;
                if (CIndex >= 8 && CIndex <= 12)
                {
                    //re=点击datagridview里组员1-5单元格，获取单元格其中的内容
                    restuno = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    //获取单元格内容中的数字
                    restuno = System.Text.RegularExpressions.Regex.Replace(restuno, @"[^0-9]+", "");
                    //要用这个带参数的构造函数，把教师查看学生这边获取的学生id传到学生信息页面
                    XPersonalInformationPreview formStu = new XPersonalInformationPreview(restuno);
                    formStu.ShowDialog();
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        #endregion
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        #region 点击单元格数据 选择或取消志愿
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //单击任意单元格选中该行一整行
                this.dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                int CIndex = e.ColumnIndex;
                if (CIndex == 6)//点中的单元格第7个 --操作1 内容“选择”把该行数据插入teavol表（datatable暂存）
                {
                    //查询当前教师工号的可带组数
                    int teagrnum = tm.GetGroupNum(teaNo);
                    //要查询当前datatable中当前教师工号已经选了多少组
                    DataRow[] dtlin = dt.Select("Teano=" + teaNo);
                    int i = dtlin.Length;
                    if (i < teagrnum)
                    {
                        //获取这条记录中的groupid  第0个单元格 查看通知  （获取通知id)  作为输入数据库的参数 参考教师查看修改个人信息  
                        //单击任意单元格，选中该行，获取该行中的序号列的内容
                        int re = Convert.ToInt32(this.dataGridView1.Rows[e.RowIndex].Cells["组号"].Value);
                        DataRow dr = dt.NewRow();
                        dr[0] = teaNo;
                        dr[1] = re;//groupid 从点击的行索引获取
                        dt.Rows.Add(dr);//到此为在datatable中保存新的数据
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;//点击后该行颜色改变
                        i = i + 1;
                    }
                    else
                    {
                        MessageBox.Show("您可带学生组数为:" + teagrnum + "组，如要选择该组，请先取消其他组");
                    }

                }
                if (CIndex == 7)//取消选择  从datatable中删除数据
                {
                    if (dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor != Color.Yellow)//该行颜色没改变说明没被选择
                    {
                        MessageBox.Show("该组学生未被选择，不可取消选择");
                    }
                    else
                    {
                        int re = Convert.ToInt32(this.dataGridView1.Rows[e.RowIndex].Cells["组号"].Value);
                        DataRow[] dr;
                        dr = dt.Select("Teano=" + teaNo + "and Groupid=" + re);
                        foreach (DataRow i in dr)
                        {
                            dt.Rows.Remove(i);
                        }//至此为在datatable dtteavol中删除特定行
                        dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.White;
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion

        #region 提交志愿按钮  把最终选择提交到数据库
        private void button1_Click(object sender, EventArgs e)
        {
            int i = tm.updateTV(dt);
            if (i > 0)//此方法为最终提交到数据库 并判断返回的数据即影响的行数是否大于0
            {
                MessageBox.Show("提交选择成功");
            }
            dataGridView1.Enabled = false;//整个datagridview变灰 无法触发点击事件
        } 
        #endregion


        #region 以下代码暂时不要
        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {
        }

        private void dataGridView1_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
        }
        //datagridview载入时判断哪些组是被当下教师账号已经选择了的，选择了的要显示成黄颜色才不会多次重复选择同一组学生
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {//暂时不要  会影响到提交功能
            //if (e.RowIndex > -1)
            //{
            //    int re = Convert.ToInt32(this.dataGridView1.Rows[e.RowIndex].Cells["组号"].Value);
            //    DataRow[] dr = dt.Select();
            //    foreach (DataRow d in dr)
            //        if (Convert.ToInt32(d[1]) == re && d[0].ToString() == teaNo)
            //        {
            //            dataGridView1.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Yellow;
            //        }
            //}
        } 
        #endregion
    }
}
