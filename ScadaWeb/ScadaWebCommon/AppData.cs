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
 * Summary  : Common data of the application
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2005
 * Modified : 2015
 */

using Scada.Client;
using Scada.Web.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using Utils;

namespace Scada.Web
{
	/// <summary>
    /// Common data of the application
    /// <para>Общие данные приложения</para>
	/// </summary>
	public static class AppData
	{
        /// <summary>
        /// Имя файла журнала приложения без директории
        /// </summary>
        public const string LogFileName = "ScadaWeb.log";

        private static readonly object appDataLock;  // объект для синхронизации доступа к данным приложения
        private static WebSettings webSettings;      // настройки веб-приложения
        private static ViewSettings viewSettings;    // настройки представлений
        private static List<PluginInfo> plugins;     // подключенные плагины

        private static DateTime scadaDataDictAge;    // время изменения файла словаря ScadaData
        private static DateTime scadaWebDictAge;     // время изменения файла словаря ScadaWeb
        

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static AppData()
		{
            appDataLock = new object();
            webSettings = new WebSettings();
            viewSettings = new ViewSettings();
            plugins = new List<PluginInfo>();

            scadaDataDictAge = DateTime.MinValue;
            scadaWebDictAge = DateTime.MinValue;

            Inited = false;
            AppDirs = new AppDirectories();
            Log = new Log(Log.Formats.Full);
            MainData = new MainData();
        }


        /// <summary>
        /// Получить признак инициализации общих данных приложения
        /// </summary>
        public static bool Inited { get; private set; }

        /// <summary>
        /// Получить директории приложения
        /// </summary>
        public static AppDirectories AppDirs { get; private set; }
        
        /// <summary>
        /// Получить журнал приложения
        /// </summary>
        public static Log Log { get; private set; }

        /// <summary>
        /// Получить объект для работы с данными системы
        /// </summary>
        public static MainData MainData { get; private set; }


        /// <summary>
        /// Обновить словари веб-приложения
        /// </summary>
        private static void RefreshDictionaries()
        {
            if (!Localization.UseRussian)
            {
                // обновление словаря ScadaData
                bool updated;
                string msg;
                bool refreshOK = Localization.RefreshDictionary(AppDirs.LangDir, "ScadaData", ref scadaDataDictAge, 
                    out updated, out msg);
                Log.WriteAction(msg, refreshOK ? Log.ActTypes.Action : Log.ActTypes.Error);

                if (updated)
                    CommonPhrases.Init();

                // обновление словаря ScadaWeb
                refreshOK = Localization.RefreshDictionary(AppDirs.LangDir, "ScadaWeb", ref scadaWebDictAge, 
                    out updated, out msg);
                Log.WriteAction(msg, refreshOK ? Log.ActTypes.Action : Log.ActTypes.Error);
                
                if (updated)
                    WebPhrases.Init();
            }
        }

        /// <summary>
        /// Обновить настройки веб-приложения
        /// </summary>
        private static bool RefreshWebSettings()
        {
            bool reloaded;
            string msg;
            bool refreshOK = webSettings.Refresh(AppDirs.ConfigDir, out reloaded, out msg);
            Log.WriteAction(msg, refreshOK ? Log.ActTypes.Action : Log.ActTypes.Error);
            return reloaded;
        }

        /// <summary>
        /// Обновить настройки представлений
        /// </summary>
        private static bool RefreshViewSettings()
        {
            bool reloaded;
            string msg;
            bool refreshOK = viewSettings.Refresh(AppDirs.ConfigDir, out reloaded, out msg);
            Log.WriteAction(msg, refreshOK ? Log.ActTypes.Action : Log.ActTypes.Error);
            return reloaded;
        }

        /// <summary>
        /// Загрузить информацию о плагинах
        /// </summary>
        private static void LoadPlugins()
        {
            plugins.Clear();

            foreach (string fileName in webSettings.PluginFileNames)
            {
                string errMsg;
                PluginInfo pluginInfo = PluginInfo.CreateFromDll(AppDirs.BinDir + fileName, out errMsg);

                if (pluginInfo == null)
                    Log.WriteAction(errMsg, Log.ActTypes.Error);
                else
                    plugins.Add(pluginInfo);
            }
        }

        /// <summary>
        /// Инициализировать плагины
        /// </summary>
        private static void InitPlugins()
        {
            foreach (PluginInfo pluginInfo in plugins)
            {
                try
                {
                    pluginInfo.Init();
                }
                catch (Exception ex)
                {
                    Log.WriteAction(string.Format(Localization.UseRussian ?
                        "Ошибка при инициализации плагина \"{0}\": {1}" :
                        "Error initializing plugin \"{0}\": {1}", pluginInfo.Name, ex.Message),
                        Log.ActTypes.Exception);
                }
            }
        }

        /// <summary>
        /// Обновить настройки плагинов
        /// </summary>
        private static void RefreshPluginSettings()
        {
            foreach (PluginInfo pluginInfo in plugins)
            {
                try
                {
                    pluginInfo.RefreshSettings();
                }
                catch (Exception ex)
                {
                    Log.WriteAction(string.Format(Localization.UseRussian ? 
                        "Ошибка при обновлении настроек плагина \"{0}\": {1}" :
                        "Error refreshing plugin \"{0}\" settings: {1}", pluginInfo.Name, ex.Message), 
                        Log.ActTypes.Exception);
                }
            }
        }


        /// <summary>
        /// Инициализировать общие данные приложения
        /// </summary>
        public static void InitAppData()
        {
            lock (appDataLock)
            {
                if (!Inited)
                {
                    Inited = true;

                    // инициализация директорий приложения
                    if (HttpContext.Current != null && HttpContext.Current.Request != null)
                        AppDirs.Init(HttpContext.Current.Request.PhysicalApplicationPath);

                    // настройка журнала приложения
                    Log.FileName = AppDirs.LogDir + LogFileName;
                    Log.Encoding = Encoding.UTF8;
                    Log.WriteBreak();
                    Log.WriteAction(Localization.UseRussian ? "Инициализация общих данных приложения" :
                        "Initialize common application data", Log.ActTypes.Action);

                    // настройка объекта для работы с данными системы
                    MainData.SettingsFileName = AppDirs.ConfigDir + CommSettings.DefFileName;
                }

                // обновление словарей веб-приложения
                RefreshDictionaries();

                // обновление настроек веб-приложения
                if (RefreshWebSettings())
                {
                    // загрузка информации о плагинах и инициализация плагинов
                    LoadPlugins();
                    InitPlugins();
                }

                // обновление настроек плагинов
                RefreshPluginSettings();

                // обновление настроек представлений
                RefreshViewSettings();
            }
        }

        /// <summary>
        /// Получить копию настроек веб-приложения
        /// </summary>
        public static WebSettings GetWebSettingsCopy()
        {
            lock (appDataLock)
                return webSettings.Clone();
        }

        /// <summary>
        /// Получить копию настроек представлений
        /// </summary>
        public static ViewSettings GetViewSettingsCopy()
        {
            lock (appDataLock)
                return viewSettings.Clone();
        }

        /// <summary>
        /// Сформировать меню пользователя, используя подключенные плагины
        /// </summary>
        public static void MakeUserMenu(UserData userData)
        {
            lock (appDataLock)
            {
                if (userData != null)
                {
                    HashSet<PluginInfo.StandardMenuItem> standardMenuItems = 
                        new HashSet<PluginInfo.StandardMenuItem>();

                    // добавление пользовательских элементов меню
                    foreach (PluginInfo pluginInfo in plugins)
                    {
                        try
                        {
                            userData.UserMenu.AddRange(pluginInfo.GetMenuItems(userData));
                            standardMenuItems.UnionWith(pluginInfo.GetStandardMenuItems(userData));
                        }
                        catch (Exception ex)
                        {
                            Log.WriteAction(string.Format(Localization.UseRussian ?
                                "Ошибка при получении элементов меню плагина \"{0}\": {1}" :
                                "Error getting menu items of plugin \"{0}\": {1}", pluginInfo.Name, ex.Message),
                                Log.ActTypes.Exception);
                        }
                    }

                    // добавление стандартных элементов меню
                    if (standardMenuItems.Contains(PluginInfo.StandardMenuItem.Config))
                        userData.UserMenu.Insert(0, PluginInfo.ConvertStandardMenuItem(
                            PluginInfo.StandardMenuItem.Config));

                    if (standardMenuItems.Contains(PluginInfo.StandardMenuItem.Reports))
                        userData.UserMenu.Insert(0, PluginInfo.ConvertStandardMenuItem(
                            PluginInfo.StandardMenuItem.Reports));

                    if (standardMenuItems.Contains(PluginInfo.StandardMenuItem.Views))
                        userData.UserMenu.Insert(0, PluginInfo.ConvertStandardMenuItem(
                            PluginInfo.StandardMenuItem.Views));

                    if (standardMenuItems.Contains(PluginInfo.StandardMenuItem.About))
                        userData.UserMenu.Add(PluginInfo.ConvertStandardMenuItem(
                            PluginInfo.StandardMenuItem.About));
                }
            }
        }

        /// <summary>
        /// Получить информацию о плагине по его типу
        /// </summary>
        public static PluginInfo GetPluginInfo(Type pluginInfoType)
        {
            // безопасно ли?
            lock (appDataLock)
            {
                return null;
            }
        }
    }
}