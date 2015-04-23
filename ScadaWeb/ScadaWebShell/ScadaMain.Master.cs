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
 * Summary  : Main page template
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System;

namespace Scada.Web
{
    /// <summary>
    /// Main page template
    /// <para>Основной шаблон страниц</para>
    /// </summary>
    public partial class MpScadaMain : System.Web.UI.MasterPage
    {
        protected string absPath; // адрес текущей страницы без параметров


        protected void Page_Load(object sender, EventArgs e)
        {
            // войти в систему при необходимости
            UserData userData = UserData.GetUserData();
            userData.LoginIfNeeded(Context);

            // получение адреса текущей страницы без параметров
            absPath = Request.Url.AbsolutePath;

            if (!IsPostBack)
            {
                // отображение меню пользователя
                if (userData.UserMenu.Count > 0)
                {
                    repUserMenu.DataSource = userData.UserMenu;
                    repUserMenu.DataBind();
                }
                else
                {
                    repUserMenu.Visible = false;
                }
            }
        }
    }
}