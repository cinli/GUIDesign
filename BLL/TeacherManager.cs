﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL;
using Model;

namespace BLL
{
    public class TeacherManager
    {
        #region 常量变量定义
        TeacherService ts = new TeacherService();

        #endregion

        #region 执行教师登入检查
        /// <summary>
        /// 执行教师登入检查
        /// </summary>
        /// <param name="loginId">用户id</param>
        /// <param name="loginPwd">用户密码</param>
        /// <returns>true：登入成功  flase：反则</returns>
        public bool CheckTeacherLogin(string loginId, string loginPwd)
        {
            try
            {
                return ts.CheckTeacherLogin(loginId, loginPwd);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #endregion
    }
}