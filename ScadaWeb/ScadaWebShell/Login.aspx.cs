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
 * Module   : ScadaWebShell
 * Summary  : Login web form
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2005
 * Modified : 2015
 */

using Scada.Web.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Scada.Web
{
    /// <summary>
    /// Login web form
    /// <para>Веб-форма входа в систему</para>
    /// </summary>
    public partial class WFrmLogin : System.Web.UI.Page
    {
        /// <summary>
        /// Получить первую непустую ссылку меню
        /// </summary>
        /// <remarks>Обрабатывается 2 уровня вложенности меню</remarks>
        private static string GetFirstMenuUrl(List<PluginInfo.MenuItem> menuItems)
        {
            foreach (PluginInfo.MenuItem menuItem in menuItems)
            {
                if (string.IsNullOrEmpty(menuItem.Url))
                {
                    foreach (PluginInfo.MenuItem subitem in menuItem.Subitems)
                        if (!string.IsNullOrEmpty(subitem.Url))
                            return subitem.Url;
                }
                else
                {
                    return menuItem.Url;
                }
            }

            return "";
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            // очистка сообщения об ошибке
            lblErrMsg.Visible = false;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            // получение данных пользователя
            UserData userData = UserData.GetUserData();

            // вход в систему
            string errMsg = "";
            if (userData.Login(txtLogin.Text, txtPassword.Text, out errMsg))
            {
                // переход на стартовую страницу
                userData.GoToStartPage(Context, GetFirstMenuUrl(userData.UserMenu));
            }
            else
            {
                lblErrMsg.Text = HttpUtility.HtmlEncode(string.Format(WebPhrases.UnableLogin, errMsg));
                lblErrMsg.Visible = true;
            }
        }
    }
}