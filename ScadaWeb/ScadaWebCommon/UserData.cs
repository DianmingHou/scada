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
    /// <para>Данные пользователя приложения</para>
    /// </summary>
    /// <remarks>Inheritance is impossible because class is shared by different modules
    /// <para>Наследование невозможно, т.к. класс совместно используется различными модулями</para></remarks>
    public sealed class UserData
    {
        /// <summary>
        /// Конструктор, ограничивающий создание объекта без параметров
        /// </summary>
        private UserData()
        {
            Logout();
        }


        /// <summary>
        /// Получить имя пользователя
        /// </summary>
        public string UserLogin { get; private set; }

        /// <summary>
        /// Получить идентификатор пользователя в базе конфигурации
        /// </summary>
        public int UserID { get; private set; }

        /// <summary>
        /// Получить роль пользователя
        /// </summary>
        public ServerComm.Roles Role { get; private set; }

        /// <summary>
        /// Получить идентификатор роли пользователя
        /// </summary>
        public int RoleID { get; private set; }

        /// <summary>
        /// Получить наименование роли пользователя
        /// </summary>
        public string RoleName { get; private set; }

        /// <summary>
        /// Получить признак, выполнен ли вход пользователя в систему
        /// </summary>
        public bool LoggedOn { get; private set; }

        /// <summary>
        /// Получить дату и время входа пользователя в систему
        /// </summary>
        public DateTime LogOnDT { get; private set; }

        /// <summary>
        /// Получить IP-адрес пользователя
        /// </summary>
        public string IpAddress { get; private set; }


        /// <summary>
        /// Получить копию настроек веб-приложения
        /// </summary>
        public WebSettings WebSettings { get; private set; }
        
        /// <summary>
        /// Получить копию настроек представлений
        /// </summary>
        public ViewSettings ViewSettings { get; private set; }

        /// <summary>
        /// Получить права пользователя
        /// </summary>
        public UserRights UserRights { get; private set; }

        /// <summary>
        /// Получить кэш представлений
        /// </summary>
        public ViewCache ViewCache { get; private set; }

        /// <summary>
        /// Получить меню пользователя
        /// </summary>
        public List<PluginInfo.MenuItem> UserMenu { get; private set; }
        

        /// <summary>
        /// Выполнить вход пользователя в систему
        /// </summary>
        /// <remarks>Если пароль равен null, то он не проверяется</remarks>
        public bool Login(string login, string password, out string errMsg)
        {
            login = login == null ? "" : login.Trim();
            int roleID;

            if (AppData.MainData.CheckUser(login, password, password != null, out roleID, out errMsg))
            {
                // заполнение свойств пользователя
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
                        "Вход в систему без пароля: {0} ({1}). IP-адрес: {2}" : 
                        "Login without a password: {0} ({1}). IP address: {2}", login , RoleName, IpAddress), 
                        Log.ActTypes.Action);
                }
                else
                {
                    AppData.Log.WriteAction(string.Format(Localization.UseRussian ?
                        "Вход в систему: {0} ({1}). IP-адрес: {2}" :
                        "Login: {0} ({1}). IP address: {2}", login, RoleName, IpAddress), 
                        Log.ActTypes.Action);
                }

                return true;
            }
            else
            {
                // очистка свойств пользователя
                Logout();

                string err = login == "" ? errMsg : login + " - " + errMsg;
                AppData.Log.WriteAction(string.Format(Localization.UseRussian ? 
                    "Неудачная попытка входа в систему: {0}. {1}" :
                    "Unsuccessful login attempt: {0}. {1}", err, IpAddress), Log.ActTypes.Error);
                return false;
            }
        }

        /// <summary>
        /// Выполнить вход пользователя в систему без проверки пароля
        /// </summary>
        public bool Login(string login)
        {
            string errMsg;
            return Login(login, null, out errMsg);
        }

        /// <summary>
        /// Завершить работу пользователя с определёнными ранее именем
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
        /// Получить уже загруженное представление, если на него достаточно прав
        /// </summary>
        public BaseView GetView(int viewIndex, out MainData.Right right)
        {
            right = UserRights == null ? MainData.Right.NoRights : UserRights.GetViewRight(viewIndex);
            return right.ViewRight ? ViewCache.GetView(viewIndex) : null;
        }

        /// <summary>
        /// Получить представление заданного типа, если на него достаточно прав
        /// </summary>
        public T GetView<T>(int viewIndex, out MainData.Right right) where T : BaseView
        {
            right = UserRights == null ? MainData.Right.NoRights : UserRights.GetViewRight(viewIndex);
            return right.ViewRight ? ViewCache.GetView<T>(viewIndex) : null;
        }


        /// <summary>
        /// Получить данные пользователя приложения
        /// </summary>
        /// <remarks>Для веб-приложения данные пользователя сохраняются в сессии</remarks>
        public static UserData GetUserData()
        {
            HttpSessionState session = HttpContext.Current == null ? null : HttpContext.Current.Session;
            UserData userData = session == null ? null : session["UserData"] as UserData;

            if (userData == null)
            {
                // создание данных пользователя
                AppData.InitAppData();
                userData = new UserData();

                if (session != null)
                    session.Add("UserData", userData);

                // получение IP-адреса
                HttpRequest request = HttpContext.Current == null ? null : HttpContext.Current.Request;
                userData.IpAddress = request == null ? "" : request.UserHostAddress;

                // получение копий настроек
                userData.ViewSettings = AppData.GetViewSettingsCopy();
                userData.WebSettings = AppData.GetWebSettingsCopy();

                // создание меню пользователя
                userData.UserMenu = new List<PluginInfo.MenuItem>();
                AppData.MakeUserMenu(userData);
            }

            return userData;
        }
    }
}