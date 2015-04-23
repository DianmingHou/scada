/*
 * Copyright 2015 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaWebCommon
 * Summary  : Application user data
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2007
 * Modified : 2015
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Scada.Client;
using Utils;
using Scada.Web.Plugins;

namespace Scada.Web
{
    /// <summary>
    /// Application user data
    /// <para>������ ������������ ����������</para>
    /// </summary>
    /// <remarks>Inheritance is impossible because class is shared by different modules
    /// <para>������������ ����������, �.�. ����� ��������� ������������ ���������� ��������</para></remarks>
    public sealed class UserData
    {
        /// <summary>
        /// �����������, �������������� �������� ������� ��� ����������
        /// </summary>
        private UserData()
        {
            Logout();
        }


        /// <summary>
        /// �������� ��� ������������
        /// </summary>
        public string UserLogin { get; private set; }

        /// <summary>
        /// �������� ������������� ������������ � ���� ������������
        /// </summary>
        public int UserID { get; private set; }

        /// <summary>
        /// �������� ���� ������������
        /// </summary>
        public ServerComm.Roles Role { get; private set; }

        /// <summary>
        /// �������� ������������� ���� ������������
        /// </summary>
        public int RoleID { get; private set; }

        /// <summary>
        /// �������� ������������ ���� ������������
        /// </summary>
        public string RoleName { get; private set; }

        /// <summary>
        /// �������� �������, �������� �� ���� ������������ � �������
        /// </summary>
        public bool LoggedOn { get; private set; }

        /// <summary>
        /// �������� ���� � ����� ����� ������������ � �������
        /// </summary>
        public DateTime LogOnDT { get; private set; }

        /// <summary>
        /// �������� IP-����� ������������
        /// </summary>
        public string IpAddress { get; private set; }


        /// <summary>
        /// �������� ����� �������� ���-����������
        /// </summary>
        public WebSettings WebSettings { get; private set; }
        
        /// <summary>
        /// �������� ����� �������� �������������
        /// </summary>
        public ViewSettings ViewSettings { get; private set; }

        /// <summary>
        /// �������� ����� ������������
        /// </summary>
        public UserRights UserRights { get; private set; }

        /// <summary>
        /// �������� ��� �������������
        /// </summary>
        public ViewCache ViewCache { get; private set; }

        /// <summary>
        /// �������� ���� ������������
        /// </summary>
        public List<PluginInfo.MenuItem> UserMenu { get; private set; }
        

        /// <summary>
        /// ��������� ���� ������������ � �������
        /// </summary>
        /// <remarks>���� ������ ����� null, �� �� �� �����������</remarks>
        public bool Login(string login, string password, out string errMsg)
        {
            login = login == null ? "" : login.Trim();
            int roleID;

            if (AppData.MainData.CheckUser(login, password, password != null, out roleID, out errMsg))
            {
                // ���������� ������� ������������
                UserLogin = login;
                UserID = AppData.MainData.GetUserID(login);
                Role = ServerComm.GetRole(roleID);
                RoleID = roleID;
                RoleName = AppData.MainData.GetRoleName(RoleID);
                LoggedOn = true;
                LogOnDT = DateTime.Now;

                UserRights = new UserRights();
                UserRights.InitUserRights(roleID, ViewSettings);

                ViewCache = new Web.ViewCache();
                ViewCache.InitViewCache(ViewSettings);

                if (password == null)
                {
                    AppData.Log.WriteAction(string.Format(Localization.UseRussian ? 
                        "���� � ������� ��� ������: {0} ({1}). IP-�����: {2}" : 
                        "Login without a password: {0} ({1}). IP address: {2}", login , RoleName, IpAddress), 
                        Log.ActTypes.Action);
                }
                else
                {
                    AppData.Log.WriteAction(string.Format(Localization.UseRussian ?
                        "���� � �������: {0} ({1}). IP-�����: {2}" :
                        "Login: {0} ({1}). IP address: {2}", login, RoleName, IpAddress), 
                        Log.ActTypes.Action);
                }

                return true;
            }
            else
            {
                // ������� ������� ������������
                Logout();

                string err = login == "" ? errMsg : login + " - " + errMsg;
                AppData.Log.WriteAction(string.Format(Localization.UseRussian ? 
                    "��������� ������� ����� � �������: {0}. {1}" :
                    "Unsuccessful login attempt: {0}. {1}", err, IpAddress), Log.ActTypes.Error);
                return false;
            }
        }

        /// <summary>
        /// ��������� ���� ������������ � ������� ��� �������� ������
        /// </summary>
        public bool Login(string login)
        {
            string errMsg;
            return Login(login, null, out errMsg);
        }

        /// <summary>
        /// ��������� ������ ������������ � ������������ ����� ������
        /// </summary>
        public void Logout()
        {
            UserLogin = "";
            UserID = 0;
            Role = ServerComm.Roles.Disabled;
            RoleID = (int)Role;
            RoleName = "";
            LoggedOn = false;
            LogOnDT = DateTime.MinValue;
            IpAddress = "";

            WebSettings = null;
            ViewSettings = null;
            UserRights = null;
            ViewCache = null;
            UserMenu = null;
        }

        /// <summary>
        /// �������� ��� ����������� �������������, ���� �� ���� ���������� ����
        /// </summary>
        public BaseView GetView(int viewIndex, out MainData.Right right)
        {
            right = UserRights == null ? MainData.Right.NoRights : UserRights.GetViewRight(viewIndex);
            return right.ViewRight ? ViewCache.GetView(viewIndex) : null;
        }

        /// <summary>
        /// �������� ������������� ��������� ����, ���� �� ���� ���������� ����
        /// </summary>
        public T GetView<T>(int viewIndex, out MainData.Right right) where T : BaseView
        {
            right = UserRights == null ? MainData.Right.NoRights : UserRights.GetViewRight(viewIndex);
            return right.ViewRight ? ViewCache.GetView<T>(viewIndex) : null;
        }


        /// <summary>
        /// �������� ������ ������������ ����������
        /// </summary>
        /// <remarks>��� ���-���������� ������ ������������ ����������� � ������</remarks>
        public static UserData GetUserData()
        {
            HttpSessionState session = HttpContext.Current == null ? null : HttpContext.Current.Session;
            UserData userData = session == null ? null : session["UserData"] as UserData;

            if (userData == null)
            {
                // �������� ������ ������������
                AppData.InitAppData();
                userData = new UserData();

                if (session != null)
                    session.Add("UserData", userData);

                // ��������� IP-������
                HttpRequest request = HttpContext.Current == null ? null : HttpContext.Current.Request;
                userData.IpAddress = request == null ? "" : request.UserHostAddress;

                // ��������� ����� ��������
                userData.ViewSettings = AppData.GetViewSettingsCopy();
                userData.WebSettings = AppData.GetWebSettingsCopy();

                // �������� ���� ������������
                userData.UserMenu = new List<PluginInfo.MenuItem>();
                AppData.MakeUserMenu(userData);
            }

            return userData;
        }
    }
}