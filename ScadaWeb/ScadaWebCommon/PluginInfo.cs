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
 * Summary  : The base class for information about a plugin of web application
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Scada.Web.Plugins
{
    /// <summary>
    /// The base class for information about a plugin of web application
    /// <para>Родительский класс информации о плагине веб-приложения</para>
    /// </summary>
    public abstract class PluginInfo
    {
        /// <summary>
        /// Элемент меню, добавляемый плагином
        /// </summary>
        public class MenuItem
        {
            /// <summary>
            /// Конструктор, ограничивающий создание объекта без параметров
            /// </summary>
            protected MenuItem()
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public MenuItem(string icon, string text, string url)
            {
                Icon = icon;
                Text = text;
                Url = url;
                Subitems = new List<MenuItem>();
            }

            /// <summary>
            /// Получить иконку
            /// </summary>
            public string Icon { get; protected set; }
            /// <summary>
            /// Получить текст
            /// </summary>
            public string Text { get; protected set; }
            /// <summary>
            /// Получить ссылку
            /// </summary>
            public string Url { get; protected set; }
            /// <summary>
            /// Получить или установить подпункты меню
            /// </summary>
            public List<MenuItem> Subitems { get; protected set; }
        }

        /// <summary>
        /// Стандартные элементы меню
        /// </summary>
        public enum StandardMenuItem
        {
            /// <summary>
            /// Представления
            /// </summary>
            Views,
            /// <summary>
            /// Отчёты
            /// </summary>
            Reports,
            /// <summary>
            ///  Конфигурация
            /// </summary>
            Config,
            /// <summary>
            ///  О программе
            /// </summary>
            About
        }


        /// <summary>
        /// Конструктор
        /// </summary>
        public PluginInfo()
        {
        }


        /// <summary>
        /// Получить наименование плагина
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Получить описание плагина
        /// </summary>
        public abstract string Descr { get; }


        /// <summary>
        /// Инициализировать плагин
        /// </summary>
        public virtual void Init()
        {
        }

        /// <summary>
        /// Обновить настройки плагина
        /// </summary>
        public virtual void RefreshSettings()
        {
        }


        /// <summary>
        /// Получить список элементов меню для пользователя
        /// </summary>
        /// <remarks>Поддерживается не более 2 уровней вложенности меню</remarks>
        public virtual List<MenuItem> GetMenuItems(object userData)
        {
            return null;
        }

        /// <summary>
        /// Получить требуемые стандартные элементы меню для пользователя
        /// </summary>
        public virtual HashSet<StandardMenuItem> GetStandardMenuItems(object userData)
        {
            return new HashSet<StandardMenuItem>() { StandardMenuItem.About };
        }


        /// <summary>
        /// Создать плагин, загрузив его из библиотеки
        /// </summary>
        public static PluginInfo CreateFromDll(string fullName, out string errMsg)
        {
            try
            {
                string fileName = Path.GetFileName(fullName);

                // загрузка сборки
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.LoadFile(fullName);
                }
                catch (Exception ex)
                {
                    errMsg = string.Format(Localization.UseRussian ?
                        "Ошибка при загрузке библиотеки плагина {0}:\r\n{1}" :
                        "Error loading the plugin assembly {0}:\r\n{1}", fullName, ex.Message);
                    return null;
                }

                // получение типа из загруженной сборки
                Type type = null;
                string typeName = string.Format("Scada.Web.Plugins.{0}Info", 
                    Path.GetFileNameWithoutExtension(fileName));

                try
                {
                    type = assembly.GetType(typeName, true);
                }
                catch (Exception ex)
                {
                    errMsg = string.Format(Localization.UseRussian ?
                        "Не удалось получить тип плагина {0} из библиотеки {1}:\r\n{2}" :
                        "Unable to get the plugin type {0} from the assembly {1}:\r\n{2}",
                        typeName, fullName, ex.Message);
                    return null;
                }

                try
                {
                    // создание экземпляра класса
                    PluginInfo pluginInfo = (PluginInfo)Activator.CreateInstance(type);
                    errMsg = "";
                    return pluginInfo;
                }
                catch (Exception ex)
                {
                    errMsg = string.Format(Localization.UseRussian ?
                        "Ошибка при создании экземпляра класса плагина {0} из библиотеки {1}:\r\n{2}" :
                        "Error creating plugin class instance {0} from the assembly {1}:\r\n{2}", 
                        type, fullName, ex.Message);
                    return null;
                }           
            }
            catch (Exception ex)
            {
                errMsg = string.Format(Localization.UseRussian ?
                    "Ошибка при создании плагина из библиотеки {0}:\r\n{1}" :
                    "Error creating plugin from the assembly {0}:\r\n{1}", fullName, ex.Message);
                return null;
            }       
        }

        /// <summary>
        /// Преобразовать стандартный элемент меню в пользовательский
        /// </summary>
        public static MenuItem ConvertStandardMenuItem(StandardMenuItem standardMenuItem)
        {
            switch (standardMenuItem)
            {
                case StandardMenuItem.Views:
                    return new MenuItem("", WebPhrases.ViewsMenuItemText, "~/Views.aspx");
                case StandardMenuItem.Reports:
                    return new MenuItem("", WebPhrases.ReportsMenuItemText, "~/Reports.aspx");
                case StandardMenuItem.Config:
                    return new MenuItem("", WebPhrases.ConfigMenuItemText, "~/Config.aspx");
                default: // StandardMenuItem.About
                    return new MenuItem("", WebPhrases.AboutMenuItemText, "~/About.aspx");
            }
        }
    }
}