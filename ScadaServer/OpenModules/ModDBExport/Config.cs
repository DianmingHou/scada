﻿/*
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
 * Module   : ModDBExport
 * Summary  : Module configuration
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Scada.Server.Modules.DBExport
{
    /// <summary>
    /// Module configuration
    /// <para>Конфигурация модуля</para>
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Параметры экспорта
        /// </summary>
        public class ExportParams
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ExportParams()
            {
                ExportCurData = false;
                ExportCurDataQuery = "";
                ExportArcData = false;
                ExportArcDataQuery = "";
                ExportEvents = false;
                ExportEventQuery = "";
            }

            /// <summary>
            /// Получить или установить признак, экспортировать ли текущие данные
            /// </summary>
            public bool ExportCurData { get; set; }
            /// <summary>
            /// Получить или установить SQL-запрос для экспорта текущих данных
            /// </summary>
            public string ExportCurDataQuery { get; set; }
            /// <summary>
            /// Получить или установить признак, экспортировать ли архивные данные
            /// </summary>
            public bool ExportArcData { get; set; }
            /// <summary>
            /// Получить или установить SQL-запрос для экспорта архивных данных
            /// </summary>
            public string ExportArcDataQuery { get; set; }
            /// <summary>
            /// Получить или установить признак, экспортировать ли события
            /// </summary>
            public bool ExportEvents { get; set; }
            /// <summary>
            /// Получить или установить SQL-запрос для экспорта событий
            /// </summary>
            public string ExportEventQuery { get; set; }

            /// <summary>
            /// Клонировать параметры экспорта
            /// </summary>
            public ExportParams Clone()
            {
                return new ExportParams()
                    {
                        ExportCurData = this.ExportCurData,
                        ExportCurDataQuery = this.ExportCurDataQuery,
                        ExportArcData = this.ExportArcData,
                        ExportArcDataQuery = this.ExportArcDataQuery,
                        ExportEvents = this.ExportEvents,
                        ExportEventQuery = this.ExportEventQuery
                    };
            }
        }

        /// <summary>
        /// Назначение экспорта
        /// </summary>
        public class ExportDestination : IComparable<ExportDestination>
        {
            /// <summary>
            /// Конструктор, ограничивающий создание объекта без параметров
            /// </summary>
            private ExportDestination()
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public ExportDestination(DataSource dataSource, ExportParams exportParams)
            {
                if (dataSource == null)
                    throw new ArgumentNullException("dataSource");
                if (exportParams == null)
                    throw new ArgumentNullException("exportParams");
                
                this.DataSource = dataSource;
                this.ExportParams = exportParams;
            }

            /// <summary>
            /// Получить источник данных
            /// </summary>
            public DataSource DataSource { get; private set; }
            /// <summary>
            /// Получить параметры экспорта
            /// </summary>
            public ExportParams ExportParams { get; private set; }

            /// <summary>
            /// Клонировать назначение экспорта
            /// </summary>
            public ExportDestination Clone()
            {
                return new ExportDestination(DataSource.Clone(), ExportParams.Clone());
            }
            /// <summary>
            /// Сравнить текущий объект с другим объектом такого же типа
            /// </summary>
            public int CompareTo(ExportDestination other)
            {
                return DataSource.CompareTo(other.DataSource);
            }
        }


        /// <summary>
        /// Имя файла конфигурации
        /// </summary>
        private const string ConfigFileName = "ModDBExport.xml";


        /// <summary>
        /// Конструктор, ограничивающий создание объекта без параметров
        /// </summary>
        private Config()
        {
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public Config(string configDir)
        {
            FileName = ScadaUtils.NormalDir(configDir) + ConfigFileName;
            SetToDefault();
        }


        /// <summary>
        /// Получить полное имя файла конфигурации
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// Получить назначения экспорта
        /// </summary>
        public List<ExportDestination> ExportDestinations { get; private set; }

        /// <summary>
        /// Получить или установить номер канала управления для экспорта текущих данных в ручном режиме
        /// </summary>
        public int CurDataCtrlCnlNum { get; set; }

        /// <summary>
        /// Получить или установить номер канала управления для экспорта архивных данных в ручном режиме
        /// </summary>
        public int ArcDataCtrlCnlNum { get; set; }

        /// <summary>
        /// Получить или установить номер канала управления для экспорта событий в ручном режиме
        /// </summary>
        public int EventsCtrlCnlNum { get; set; }


        /// <summary>
        /// Установить значения параметров конфигурации по умолчанию
        /// </summary>
        private void SetToDefault()
        {
            if (ExportDestinations == null)
                ExportDestinations = new List<ExportDestination>();
            else
                ExportDestinations.Clear();

            CurDataCtrlCnlNum = 1;
            ArcDataCtrlCnlNum = 2;
            EventsCtrlCnlNum = 3;
        }

        /// <summary>
        /// Загрузить конфигурацию модуля
        /// </summary>
        public bool Load(out string errMsg)
        {
            SetToDefault();

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(FileName);

                // загрузка назначений экспорта
                XmlNode expDestsNode = xmlDoc.DocumentElement.SelectSingleNode("ExportDestinations");
                if (expDestsNode != null)
                {
                    XmlNodeList expDestNodeList = expDestsNode.SelectNodes("ExportDestination");
                    foreach (XmlElement expDestElem in expDestNodeList)
                    {
                        // загрузка источника данных
                        DataSource dataSource = null;
                        XmlNode dataSourceNode = expDestElem.SelectSingleNode("DataSource");

                        if (dataSourceNode != null)
                        {
                            // получение типа источника данных
                            DBType dbType;
                            if (!Enum.TryParse<DBType>(dataSourceNode.GetChildAsString("DBType"), out dbType))
                                dbType = DBType.Undefined;

                            // создание источника данных
                            switch (dbType)
                            {
                                case DBType.MSSQL:
                                    dataSource = new SqlDataSource();
                                    break;
                                case DBType.Oracle:
                                    dataSource = new OraDataSource();
                                    break;
                                case DBType.PostgreSQL:
                                    dataSource = new PgSqlDataSource();
                                    break;
                                case DBType.MySQL:
                                    dataSource = new MySqlDataSource();
                                    break;
                                case DBType.OLEDB:
                                    dataSource = new OleDbDataSource();
                                    break;
                                default:
                                    dataSource = null;
                                    break;
                            }

                            if (dataSource != null)
                            {
                                dataSource.Server = dataSourceNode.GetChildAsString("Server");
                                dataSource.Database = dataSourceNode.GetChildAsString("Database");
                                dataSource.User = dataSourceNode.GetChildAsString("User");
                                dataSource.Password = dataSourceNode.GetChildAsString("Password");
                                dataSource.ConnectionString = dataSourceNode.GetChildAsString("ConnectionString");

                                if (string.IsNullOrEmpty(dataSource.ConnectionString))
                                    dataSource.ConnectionString = dataSource.BuildConnectionString();
                            }
                        }

                        // загрузка параметров экспорта
                        ExportParams exportParams = null;
                        XmlNode exportParamsNode = expDestElem.SelectSingleNode("ExportParams");

                        if (dataSource != null && exportParamsNode != null)
                        {
                            exportParams = new ExportParams();
                            exportParams.ExportCurDataQuery = exportParamsNode.GetChildAsString("ExportCurDataQuery");
                            exportParams.ExportCurData = !string.IsNullOrEmpty(exportParams.ExportCurDataQuery) && 
                                exportParamsNode.GetChildAsBool("ExportCurData");
                            exportParams.ExportArcDataQuery = exportParamsNode.GetChildAsString("ExportArcDataQuery");
                            exportParams.ExportArcData = !string.IsNullOrEmpty(exportParams.ExportArcDataQuery) && 
                                exportParamsNode.GetChildAsBool("ExportArcData");
                            exportParams.ExportEventQuery = exportParamsNode.GetChildAsString("ExportEventQuery");
                            exportParams.ExportEvents = !string.IsNullOrEmpty(exportParams.ExportEventQuery) &&
                                exportParamsNode.GetChildAsBool("ExportEvents");
                        }

                        // создание назначения экспорта
                        if (dataSource != null && exportParams != null)
                        {
                            ExportDestination expDest = new ExportDestination(dataSource, exportParams);
                            ExportDestinations.Add(expDest);
                        }
                    }

                    // сортировка назначений экспорта
                    ExportDestinations.Sort();
                }

                // загрузка номеров каналов управления для экспорта в ручном режиме
                XmlNode manExpNode = xmlDoc.DocumentElement.SelectSingleNode("ManualExport");
                if (manExpNode != null)
                {
                    CurDataCtrlCnlNum = manExpNode.GetChildAsInt("CurDataCtrlCnlNum");
                    ArcDataCtrlCnlNum = manExpNode.GetChildAsInt("ArcDataCtrlCnlNum");
                    EventsCtrlCnlNum = manExpNode.GetChildAsInt("EventsCtrlCnlNum");
                }

                errMsg = "";
                return true;
            }
            catch (FileNotFoundException ex)
            {
                errMsg = ModPhrases.LoadModSettingsError + ": " + ex.Message + 
                    Environment.NewLine + ModPhrases.ConfigureModule;
                return false;
            }
            catch (Exception ex)
            {
                errMsg = ModPhrases.LoadModSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Сохранить конфигурацию модуля
        /// </summary>
        public bool Save(out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("ModDBExport");
                xmlDoc.AppendChild(rootElem);

                // сохранение назначений экспорта
                XmlElement expDestsElem = xmlDoc.CreateElement("ExportDestinations");
                rootElem.AppendChild(expDestsElem);

                foreach (ExportDestination expDest in ExportDestinations)
                {
                    XmlElement expDestElem = xmlDoc.CreateElement("ExportDestination");
                    expDestsElem.AppendChild(expDestElem);

                    // сохранение источника данных
                    DataSource dataSource = expDest.DataSource;
                    XmlElement dataSourceElem = xmlDoc.CreateElement("DataSource");
                    dataSourceElem.AppendElem("DBType", dataSource.DBType);
                    dataSourceElem.AppendElem("Server", dataSource.Server);
                    dataSourceElem.AppendElem("Database", dataSource.Database);
                    dataSourceElem.AppendElem("User", dataSource.User);
                    dataSourceElem.AppendElem("Password", dataSource.Password);
                    string connStr = dataSource.ConnectionString;
                    string bldConnStr = dataSource.BuildConnectionString();
                    dataSourceElem.AppendElem("ConnectionString", 
                        !string.IsNullOrEmpty(bldConnStr) && bldConnStr == connStr ? "" : connStr);
                    expDestElem.AppendChild(dataSourceElem);

                    // сохранение параметров экспорта
                    ExportParams exportParams = expDest.ExportParams;
                    XmlElement exportParamsElem = xmlDoc.CreateElement("ExportParams");
                    exportParamsElem.AppendElem("ExportCurData", exportParams.ExportCurData);
                    exportParamsElem.AppendElem("ExportCurDataQuery", exportParams.ExportCurDataQuery);
                    exportParamsElem.AppendElem("ExportArcData", exportParams.ExportArcData);
                    exportParamsElem.AppendElem("ExportArcDataQuery", exportParams.ExportArcDataQuery);
                    exportParamsElem.AppendElem("ExportEvents", exportParams.ExportEvents);
                    exportParamsElem.AppendElem("ExportEventQuery", exportParams.ExportEventQuery);
                    expDestElem.AppendChild(exportParamsElem);
                }

                // сохранение номеров каналов управления для экспорта в ручном режиме
                XmlElement manExpElem = xmlDoc.CreateElement("ManualExport");
                rootElem.AppendChild(manExpElem);
                manExpElem.AppendElem("CurDataCtrlCnlNum", CurDataCtrlCnlNum);
                manExpElem.AppendElem("ArcDataCtrlCnlNum", ArcDataCtrlCnlNum);
                manExpElem.AppendElem("EventsCtrlCnlNum", EventsCtrlCnlNum);

                xmlDoc.Save(FileName);
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ModPhrases.SaveModSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Клонировать конфигурацию модуля
        /// </summary>
        public Config Clone()
        {
            Config configCopy = new Config();
            configCopy.FileName = FileName;
            configCopy.ExportDestinations = new List<ExportDestination>();

            foreach (ExportDestination expDest in ExportDestinations)
                configCopy.ExportDestinations.Add(expDest.Clone());

            configCopy.CurDataCtrlCnlNum = CurDataCtrlCnlNum;
            configCopy.ArcDataCtrlCnlNum = ArcDataCtrlCnlNum;
            configCopy.EventsCtrlCnlNum = EventsCtrlCnlNum;

            return configCopy;
        }
    }
}
