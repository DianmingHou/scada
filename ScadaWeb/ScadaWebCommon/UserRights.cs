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
 * Summary  : Application user rights
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using Scada.Data;
using System;
using System.Collections.Generic;
using System.IO;
using Utils;

namespace Scada.Web
{
    /// <summary>
    /// Application user rights
    /// <para>Права пользователя приложения</para>
    /// </summary>
    public class UserRights
    {
        /// <summary>
        /// Идентификатор роли пользователя
        /// </summary>
        protected int roleID;
        /// <summary>
        /// Права пользователя на объекты интерфейса, введённые в базу конфигурации
        /// </summary>
        protected Dictionary<string, MainData.Right> itfObjRights;
        /// <summary>
        /// Права пользователя на представления
        /// </summary>
        protected MainData.Right[] viewRights;
        /// <summary>
        /// Права пользователя на отчёты
        /// </summary>
        protected bool[] reportRights;


        /// <summary>
        /// Конструктор
        /// </summary>
        public UserRights()
        {
            roleID = BaseValues.Roles.Disabled;
            itfObjRights = null;
            viewRights = null;
            reportRights = null;
        }


        /// <summary>
        /// Инициализировать права пользователя
        /// </summary>
        public void InitUserRights(int roleID, ViewSettings viewSettings)
        {
            try
            {
                this.roleID = roleID;
                itfObjRights = AppData.MainData.GetRights(roleID);

                if (viewSettings == null)
                {
                    viewRights = null;
                    reportRights = null;
                }
                else
                {
                    // инициализация прав на представления
                    int viewCnt = viewSettings.AllViewItems.Count;
                    viewRights = new MainData.Right[viewCnt];

                    for (int viewInd = 0; viewInd < viewCnt; viewInd++)
                    {
                        string itfObjName = Path.GetFileName(viewSettings.AllViewItems[viewInd].FileName);
                        viewRights[viewInd] = GetRight(itfObjName);
                    }

                    // инициализация прав на отчёты
                    int repCnt = viewSettings.AllReports.Count;
                    reportRights = new bool[repCnt];

                    for (int repInd = 0; repInd < repCnt; repInd++)
                    {
                        string itfObjName = Path.GetFileName(viewSettings.AllReports[repInd].FileName);
                        reportRights[repInd] = GetRight(itfObjName).ViewRight;
                    }
                }
            }
            catch (Exception ex)
            {
                AppData.Log.WriteAction((Localization.UseRussian ? "Ошибка при инициализации прав пользователя: " :
                    "Error initializing user rights: ") + ex.Message, Log.ActTypes.Exception);
            }
        }

        /// <summary>
        /// Получить права пользователя на объект интерфейса
        /// </summary>
        public MainData.Right GetRight(string itfObjName)
        {
            if (roleID == BaseValues.Roles.Admin || roleID == BaseValues.Roles.Dispatcher)
            {
                return new MainData.Right(true, true);
            }
            else if (roleID == BaseValues.Roles.Guest)
            {
                return new MainData.Right(true, false);
            }
            else if (roleID == BaseValues.Roles.Disabled || roleID == BaseValues.Roles.App || 
                roleID == BaseValues.Roles.Err)
            {
                return MainData.Right.NoRights;
            }
            else // Custom
            {
                MainData.Right right;
                return itfObjRights != null && itfObjRights.TryGetValue(itfObjName, out right) ? 
                    right : MainData.Right.NoRights;
            }
        }

        /// <summary>
        /// Получить права пользователя на представление
        /// </summary>
        public MainData.Right GetViewRight(int viewIndex)
        {
            return viewRights != null && 0 <= viewIndex && viewIndex < viewRights.Length ? 
                viewRights[viewIndex] : MainData.Right.NoRights;
        }

        /// <summary>
        /// Получить права пользователя на отчёт
        /// </summary>
        public bool GetReportRight(int reportIndex)
        {
            return reportRights != null && 0 <= reportIndex && reportIndex < reportRights.Length ? 
                reportRights[reportIndex] : false;
        }
    }
}