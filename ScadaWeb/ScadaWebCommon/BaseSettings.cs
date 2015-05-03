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
 * Summary  : The base class for settings that supports refresh
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System;

namespace Scada.Web
{
    /// <summary>
    /// The base class for settings that supports refresh
    /// <para>Базовый класс настроек, поддерживающий обновление</para>
    /// </summary>
    public abstract class BaseSettings
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public BaseSettings()
        {
            FileAge = DateTime.MinValue;
        }


        /// <summary>
        /// Получить имя файла по умолчанию
        /// </summary>
        public abstract string DefaultFileName { get; }

        /// <summary>
        /// Получить или установить время изменения файла настроек при обновлении
        /// </summary>
        public DateTime FileAge { get; set; }


        /// <summary>
        /// Загрузить настройки из файла
        /// </summary>
        public abstract bool LoadFromFile(string fileName, out string msg);

        /// <summary>
        /// Сохранить настройки в файле
        /// </summary>
        public abstract bool SaveToFile(string fileName, out string errMsg);

        /// <summary>
        /// Обновить настройки из файла, если файл изменился
        /// </summary>
        public virtual bool Refresh(string directory, out bool reloaded, out string msg)
        {
            string fileName = directory + DefaultFileName;
            DateTime newFileAge = ScadaUtils.GetLastWriteTime(fileName);

            if (FileAge == newFileAge)
            {
                reloaded = false;
                msg = "";
                return true;
            }
            else if (LoadFromFile(fileName, out msg))
            {
                reloaded = true;
                FileAge = newFileAge;
                return true;
            }
            else
            {
                reloaded = false;
                return false;
            }
        }

        /// <summary>
        /// Обновить настройки из файла, если файл изменился
        /// </summary>
        public virtual bool Refresh(string directory, out string msg)
        {
            bool reloaded;
            return Refresh(directory, out reloaded, out msg);
        }
    }
}
