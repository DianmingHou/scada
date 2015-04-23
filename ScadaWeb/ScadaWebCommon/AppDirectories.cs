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
 * Summary  : The directories of SCADA-Web
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

namespace Scada.Web
{
    /// <summary>
    /// The directories of SCADA-Web
    /// <para>Директории SCADA-Web</para>
    /// </summary>
    public class AppDirectories
    {
        /// <summary>
        /// Директория веб-приложения по умолчанию
        /// </summary>
        public const string DefWebAppDir = @"C:\SCADA\ScadaWeb\";


        /// <summary>
        /// Конструктор
        /// </summary>
        public AppDirectories()
        {
            Init(DefWebAppDir);
        }


        /// <summary>
        /// Получить директорию веб-приложения
        /// </summary>
        public string WebAppDir { get; private set; }

        /// <summary>
        /// Получить директорию исполняемых файлов
        /// </summary>
        public string BinDir { get; private set; }

        /// <summary>
        /// Получить директорию конфигурации
        /// </summary>
        public string ConfigDir { get; private set; }

        /// <summary>
        /// Получить директорию языковых файлов
        /// </summary>
        public string LangDir { get; private set; }

        /// <summary>
        /// Получить директорию журналов
        /// </summary>
        public string LogDir { get; private set; }


        /// <summary>
        /// Инициализировать директории относительно директории веб-приложения
        /// </summary>
        public void Init(string webAppDir)
        {
            WebAppDir = ScadaUtils.NormalDir(webAppDir);
            BinDir = WebAppDir + "bin\\";
            ConfigDir = WebAppDir + "config\\";
            LangDir = WebAppDir + "lang\\";
            LogDir = WebAppDir + "log\\";
        }
    }
}
