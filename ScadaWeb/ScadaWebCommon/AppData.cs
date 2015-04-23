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
    /// <para>����� ������ ����������</para>
	/// </summary>
	public static class AppData
	{
        /// <summary>
        /// ��� ����� ������� ���������� ��� ����������
        /// </summary>
        public const string LogFileName = "ScadaWeb.log";

        private static readonly object appDataLock;  // ������ ��� ������������� ������� � ������ ����������
        private static WebSettings webSettings;      // ��������� ���-����������
        private static ViewSettings viewSettings;    // ��������� �������������
        private static List<PluginInfo> plugins;     // ������������ �������

        private static DateTime scadaDataDictAge;    // ����� ��������� ����� ������� ScadaData
        private static DateTime scadaWebDictAge;     // ����� ��������� ����� ������� ScadaWeb
        private static DateTime webSettingsFileAge;  // ����� ��������� ����� �������� ���-����������
        private static DateTime viewSettingsFileAge; // ����� ��������� ����� �������� �������������
        

        /// <summary>
        /// �����������
        /// </summary>
        static AppData()
		{
            appDataLock = new object();
            webSettings = new WebSettings();
            viewSettings = new ViewSettings();
            plugins = new List<PluginInfo>();

            scadaDataDictAge = DateTime.MinValue;
            scadaWebDictAge = DateTime.MinValue;
            webSettingsFileAge = DateTime.MinValue;
            viewSettingsFileAge = DateTime.MinValue;

            Inited = false;
            AppDirs = new AppDirectories();
            Log = new Log(Log.Formats.Full);
            MainData = new MainData();
        }


        /// <summary>
        /// �������� ������� ������������� ����� ������ ����������
        /// </summary>
        public static bool Inited { get; private set; }

        /// <summary>
        /// �������� ���������� ����������
        /// </summary>
        public static AppDirectories AppDirs { get; private set; }
        
        /// <summary>
        /// �������� ������ ����������
        /// </summary>
        public static Log Log { get; private set; }

        /// <summary>
        /// �������� ������ ��� ������ � ������� �������
        /// </summary>
        public static MainData MainData { get; private set; }


        /// <summary>
        /// �������� ������� ���-����������
        /// </summary>
        private static void RefreshDictionaries()
        {
            if (!Localization.UseRussian)
            {
                // ���������� ������� ScadaData
                bool updated;
                string errMsg;
                Localization.RefreshDictionary(AppDirs.LangDir, "ScadaData", ref scadaDataDictAge, 
                    out updated, out errMsg);

                if (updated)
                    CommonPhrases.Init();
                else if (errMsg != "")
                    Log.WriteAction(errMsg, Log.ActTypes.Error);

                // ���������� ������� ScadaWeb
                Localization.RefreshDictionary(AppDirs.LangDir, "ScadaWeb", ref scadaWebDictAge, 
                    out updated, out errMsg);
                
                if (updated)
                    WebPhrases.Init();
                else if (errMsg != "")
                    Log.WriteAction(errMsg, Log.ActTypes.Error);
            }
        }

        /// <summary>
        /// �������� ��������� ���-����������
        /// </summary>
        private static bool RefreshWebSettings()
        {
            bool reloaded;
            string errMsg;
            if (!ScadaWebUtils.RefreshSettings(webSettings.LoadFromFile, AppDirs.ConfigDir + WebSettings.DefFileName,
                ref webSettingsFileAge, out reloaded, out errMsg))
            {
                Log.WriteAction(errMsg, Log.ActTypes.Error);
            }
            return reloaded;
        }

        /// <summary>
        /// �������� ��������� �������������
        /// </summary>
        private static bool RefreshViewSettings()
        {
            bool reloaded;
            string errMsg;
            if (!ScadaWebUtils.RefreshSettings(viewSettings.LoadFromFile, AppDirs.ConfigDir + ViewSettings.DefFileName,
                ref viewSettingsFileAge, out reloaded, out errMsg))
            {
                Log.WriteAction(errMsg, Log.ActTypes.Error);
            }
            return reloaded;
        }

        /// <summary>
        /// ��������� ���������� � ��������
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
        /// ���������������� �������
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
                        "������ ��� ������������� ������� \"{0}\": {1}" :
                        "Error initializing plugin \"{0}\": {1}", pluginInfo.Name, ex.Message),
                        Log.ActTypes.Exception);
                }
            }
        }

        /// <summary>
        /// �������� ��������� ��������
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
                        "������ ��� ���������� �������� ������� \"{0}\": {1}" :
                        "Error refreshing plugin \"{0}\" settings: {1}", pluginInfo.Name, ex.Message), 
                        Log.ActTypes.Exception);
                }
            }
        }


        /// <summary>
        /// ���������������� ����� ������ ����������
        /// </summary>
        public static void InitAppData()
        {
            lock (appDataLock)
            {
                if (!Inited)
                {
                    Inited = true;

                    // ������������� ���������� ����������
                    if (HttpContext.Current != null && HttpContext.Current.Request != null)
                        AppDirs.Init(HttpContext.Current.Request.PhysicalApplicationPath);

                    // ��������� ������� ����������
                    Log.FileName = AppDirs.LogDir + LogFileName;
                    Log.Encoding = Encoding.UTF8;
                    Log.WriteBreak();
                    Log.WriteAction(Localization.UseRussian ? "������������� ����� ������ ����������" :
                        "Initialize common application data", Log.ActTypes.Action);

                    // ��������� ������� ��� ������ � ������� �������
                    MainData.SettingsFileName = AppDirs.ConfigDir + CommSettings.DefFileName;
                }

                // ���������� �������� ���-����������
                RefreshDictionaries();

                // ���������� �������� ���-����������
                if (RefreshWebSettings())
                {
                    // �������� ���������� � �������� � ������������� ��������
                    LoadPlugins();
                    InitPlugins();
                }

                // ���������� �������� ��������
                RefreshPluginSettings();

                // ���������� �������� �������������
                RefreshViewSettings();
            }
        }

        /// <summary>
        /// �������� ����� �������� ���-����������
        /// </summary>
        public static WebSettings GetWebSettingsCopy()
        {
            lock (appDataLock)
                return webSettings.Clone();
        }

        /// <summary>
        /// �������� ����� �������� �������������
        /// </summary>
        public static ViewSettings GetViewSettingsCopy()
        {
            lock (appDataLock)
                return viewSettings.Clone();
        }

        /// <summary>
        /// ������������ ���� ������������, ��������� ������������ �������
        /// </summary>
        public static void MakeUserMenu(UserData userData)
        {
            lock (appDataLock)
            {
                if (userData != null)
                {
                    HashSet<PluginInfo.StandardMenuItem> standardMenuItems = 
                        new HashSet<PluginInfo.StandardMenuItem>();

                    // ���������� ���������������� ��������� ����
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
                                "������ ��� ��������� ��������� ���� ������� \"{0}\": {1}" :
                                "Error getting menu items of plugin \"{0}\": {1}", pluginInfo.Name, ex.Message),
                                Log.ActTypes.Exception);
                        }
                    }

                    // ���������� ����������� ��������� ����
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
        /// �������� ���������� � ������� �� ��� ����
        /// </summary>
        public static PluginInfo GetPluginInfo(Type pluginInfoType)
        {
            // ��������� ��?
            lock (appDataLock)
            {
                return null;
            }
        }
    }
}