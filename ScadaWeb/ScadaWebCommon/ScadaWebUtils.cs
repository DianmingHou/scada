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
 * Module   : ScadaData
 * Summary  : The class contains utility methods for web applications
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System;
using System.Web;
using Utils;

namespace Scada.Web
{
    /// <summary>
    /// The class contains utility methods for web applications
    /// <para>Класс, содержащий вспомогательные методы для веб-приложений</para>
    /// </summary>
    static class ScadaWebUtils
    {
        /// <summary>
        /// Делегат загрузки настроек
        /// </summary>
        public delegate bool LoadSettingsDelegate(string fileName, out string errMsg);


        /// <summary>
        /// Обновить настройки из файла, если файл изменился
        /// </summary>
        public static bool RefreshSettings(LoadSettingsDelegate loadSettingsMethod, string fileName,
            ref DateTime fileAge, out bool reloaded, out string errMsg)
        {
            if (loadSettingsMethod == null)
                throw new ArgumentNullException("loadSettingsMethod");

            DateTime newFileAge = ScadaUtils.GetLastWriteTime(fileName);

            if (fileAge == newFileAge)
            {
                reloaded = false;
                errMsg = "";
                return true;
            }
            else if (loadSettingsMethod(fileName, out errMsg))
            {
                reloaded = true;
                fileAge = newFileAge;
                return true;
            }
            else
            {
                reloaded = false;
                return false;
            }
        }

        /// <summary>
        /// Отключить кэширование страницы
        /// </summary>
        public static void DisablePageCache(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            response.AppendHeader("Pragma", "No-cache");
            response.AppendHeader("Cache-Control", "no-store, no-cache, must-revalidate, post-check=0, pre-check=0");
        }

        /// <summary>
        /// Преобразовать строку для вывода на веб-страницу, заменив "\n" на тег "br"
        /// </summary>
        public static string HtmlEncodeWithBreak(string s)
        {
            return HttpUtility.HtmlEncode(s).Replace("\n", "<br />");
        }
    }
}