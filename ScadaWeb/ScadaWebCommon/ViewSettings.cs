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
 * Summary  : View settings
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2011
 * Modified : 2015
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Scada.Client;

namespace Scada.Web
{
    /// <summary>
    /// View settings
    /// <para>Настройки представлений</para>
    /// </summary>
    public class ViewSettings : BaseSettings
    {
        /// <summary>
        /// Стандартные типы представлений
        /// </summary>
        public static class ViewTypes
        {
            /// <summary>
            /// Табличное представление
            /// </summary>
            public const string Table = "TableView";
            /// <summary>
            /// Мнемосхема
            /// </summary>
            public const string Scheme = "SchemeView";
            /// <summary>
            /// Лица
            /// </summary>
            public const string Faces = "FacesView";
            /// <summary>
            /// Веб-страница
            /// </summary>
            public const string WebPage = "WebPageView";
        }

        /// <summary>
        /// Элемент, описывающий представление
        /// </summary>
        public class ViewItem
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ViewItem()
                : this("", "", "", 0)
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public ViewItem(string title, string type, string fileName, int cnlNum)
            {
                Title = title;
                Type = type;
                FileName = fileName;
                CnlNum = cnlNum;
                ChildViewItems = new List<ViewItem>();
            }

            /// <summary>
            /// Получить или установить заголовок
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// Получить или установить тип
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// Получить или установить имя файла
            /// </summary>
            public string FileName { get; set; }
            /// <summary>
            /// Получить или установить номер входного канала, показывающего состояние представления
            /// </summary>
            public int CnlNum { get; set; }
            /// <summary>
            /// Получить список дочерних представлений
            /// </summary>
            public List<ViewItem> ChildViewItems { get; protected set; }

            /// <summary>
            /// Создать копию элемента
            /// </summary>
            public ViewItem Clone()
            {
                ViewItem newViewItem = new ViewItem(Title, Type, FileName, CnlNum);
                foreach (ViewItem childViewItem in ChildViewItems)
                    newViewItem.ChildViewItems.Add(childViewItem.Clone());
                return newViewItem;
            }
        }

        /// <summary>
        /// Элемент, описывающий отчёт
        /// </summary>
        public class ReportItem
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ReportItem()
                : this("", "")
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public ReportItem(string title, string fileName)
            {
                Title = title;
                FileName = fileName;
            }

            /// <summary>
            /// Получить или установить заголовок
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// Получить или установить имя файла
            /// </summary>
            public string FileName { get; set; }

            /// <summary>
            /// Создать копию элемента
            /// </summary>
            public ReportItem Clone()
            {
                return new ReportItem(Title, FileName);
            }
        }

        /// <summary>
        /// Группа отчётов
        /// </summary>
        public class ReportGroup
        {
            /// <summary>
            /// Конструктор
            /// </summary>
            public ReportGroup()
                : this("")
            {
            }
            /// <summary>
            /// Конструктор
            /// </summary>
            public ReportGroup(string title)
            {
                Title = title;
                ReportItems = new List<ReportItem>();
            }

            /// <summary>
            /// Получить или установить заголовок
            /// </summary>
            public string Title { get; set; }
            /// <summary>
            /// Получить список отчётов
            /// </summary>
            public List<ReportItem> ReportItems { get; protected set; }

            /// <summary>
            /// Создать копию группы отчётов
            /// </summary>
            public ReportGroup Clone()
            {
                ReportGroup reportGroup = new ReportGroup(Title);
                foreach (ReportItem reportItem in ReportItems)
                    reportGroup.ReportItems.Add(reportItem.Clone());
                return reportGroup;
            }
        }

        
        /// <summary>
        /// Имя файла настроек представлений по умолчанию
        /// </summary>
        public const string DefFileName = "ViewSettings.xml";


        /// <summary>
        /// Конструктор
        /// </summary>
        public ViewSettings()
            : base()
        {
            ViewItems = new List<ViewItem>();
            AllViewItems = new List<ViewItem>();
            ReportGroups = new List<ReportGroup>();
            AllReports = new List<ReportItem>();
        }


        /// <summary>
        /// Получить имя файла по умолчанию
        /// </summary>
        public override string DefaultFileName
        {
            get
            {
                return DefFileName;
            }
        }


        /// <summary>
        /// Получить список представлений
        /// </summary>
        public List<ViewItem> ViewItems { get; protected set; }

        /// <summary>
        /// Получить плоский список из всех представлений
        /// </summary>
        public List<ViewItem> AllViewItems { get; protected set; }

        /// <summary>
        /// Получить список групп отчётов
        /// </summary>
        public List<ReportGroup> ReportGroups { get; protected set; }

        /// <summary>
        /// Получить плоский список из всех отчётов
        /// </summary>
        public List<ReportItem> AllReports { get; protected set; }


        /// <summary>
        /// Установить значения настроек по умолчанию
        /// </summary>
        protected void SetToDefault()
        {
            ViewItems.Clear();
            AllViewItems.Clear();
            ReportGroups.Clear();
            AllReports.Clear();
        }

        /// <summary>
        /// Рекурсивно загрузить представления
        /// </summary>
        protected void LoadViewItems(XmlNode parentXmlNode, List<ViewItem> viewItems, List<ViewItem> allViewItems)
        {
            XmlNodeList viewNodes = parentXmlNode.SelectNodes("View");

            foreach (XmlElement viewElem in viewNodes)
            {
                string titleAttr = viewElem.GetAttribute("title");
                string title = titleAttr == "" ? viewElem.InnerText : titleAttr;
                ViewItem viewItem = new ViewItem(title, viewElem.GetAttribute("type"), 
                    viewElem.GetAttribute("fileName"), viewElem.GetAttrAsInt("cnlNum"));
                viewItems.Add(viewItem);
                allViewItems.Add(viewItem);
                LoadViewItems(viewElem, viewItem.ChildViewItems, allViewItems);
            }
        }

        /// <summary>
        /// Сохранить представления
        /// </summary>
        protected void SaveViewItems(XmlNode parentXmlNode, List<ViewItem> viewItems)
        {
            if (viewItems.Count > 0)
            {
                foreach (ViewItem viewItem in viewItems)
                {
                    XmlElement viewElem = parentXmlNode.OwnerDocument.CreateElement("View");
                    viewElem.InnerText = viewItem.Title;
                    viewElem.SetAttribute("type", viewItem.Type);
                    viewElem.SetAttribute("fileName", viewItem.FileName);
                    viewElem.SetAttribute("cnlNum", viewItem.CnlNum);
                    parentXmlNode.AppendChild(viewElem);
                    SaveViewItems(viewElem, viewItem.ChildViewItems);
                }
            }
        }

        /// <summary>
        /// Рекурсивно добавить представление в плоский список представлений
        /// </summary>
        protected void AddToAllViewItems(ViewItem viewItem)
        {
            AllViewItems.Add(viewItem);
            foreach (ViewItem childViewItem in viewItem.ChildViewItems)
                AddToAllViewItems(childViewItem);
        }


        /// <summary>
        /// Загрузить настройки представлений из файла
        /// </summary>
        public override bool LoadFromFile(string fileName, out string msg)
        {
            // установка значений по умолчанию
            SetToDefault();

            try
            {
                // вызов исключения, если файл не существует
                if (!File.Exists(fileName))
                    throw new FileNotFoundException(string.Format(CommonPhrases.NamedFileNotFound, fileName));

                // загрузка настроек
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(fileName);
                XmlElement rootElem = xmlDoc.DocumentElement;

                // загрузка представлений
                XmlNode viewsNode = rootElem.SelectSingleNode("Views");
                if (viewsNode != null)
                    LoadViewItems(viewsNode, ViewItems, AllViewItems);

                // загрузка отчётов
                XmlNode reportsNode = rootElem.SelectSingleNode("Reports");
                if (reportsNode != null)
                {
                    XmlNodeList reportGroupNodes = reportsNode.SelectNodes("ReportGroup");
                    foreach (XmlElement reportGroupElem in reportGroupNodes)
                    {
                        ReportGroup reportGroup = new ReportGroup(reportGroupElem.GetAttribute("title"));
                        ReportGroups.Add(reportGroup);

                        XmlNodeList reportNodes = reportGroupElem.SelectNodes("Report");
                        foreach (XmlElement reportElem in reportNodes)
                        {
                            ReportItem reportItem = new ReportItem(reportElem.InnerText, 
                                reportElem.GetAttribute("fileName"));
                            reportGroup.ReportItems.Add(reportItem);
                            AllReports.Add(reportItem);
                        }
                    }
                }

                msg = WebPhrases.ViewSettingsLoaded;
                return true;
            }
            catch (Exception ex)
            {
                msg = WebPhrases.LoadViewSettingsError + ": " + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Сохранить настройки представлений в файле
        /// </summary>
        public override bool SaveToFile(string fileName, out string errMsg)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();

                XmlDeclaration xmlDecl = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                xmlDoc.AppendChild(xmlDecl);

                XmlElement rootElem = xmlDoc.CreateElement("ViewSettings");
                xmlDoc.AppendChild(rootElem);

                // сохранение представлений
                XmlElement viewsElem = xmlDoc.CreateElement("Views");
                rootElem.AppendChild(viewsElem);
                SaveViewItems(viewsElem, ViewItems);

                // сохранение отчётов
                XmlElement reportsElem = xmlDoc.CreateElement("Reports");
                rootElem.AppendChild(reportsElem);

                foreach (ReportGroup reportGroup in ReportGroups)
                {
                    XmlElement reportGroupElem = xmlDoc.CreateElement("ReportGroup");
                    reportGroupElem.SetAttribute("title", reportGroup.Title);
                    reportsElem.AppendChild(reportGroupElem);

                    foreach (ReportItem reportItem in reportGroup.ReportItems)
                    {
                        XmlElement reportElem = xmlDoc.CreateElement("Report");
                        reportElem.InnerText = reportItem.Title;
                        reportElem.SetAttribute("fileName", reportItem.FileName);
                        reportGroupElem.AppendChild(reportElem);
                    }
                }

                xmlDoc.Save(fileName);
                errMsg = "";
                return true;
            }
            catch (Exception ex)
            {
                errMsg = WebPhrases.SaveViewSettingsError + ":\n" + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Создать копию настроек представлений
        /// </summary>
        public ViewSettings Clone()
        {
            ViewSettings viewSettings = new ViewSettings();

            foreach (ViewItem viewItem in ViewItems)
            {
                ViewItem newViewItem = viewItem.Clone();
                viewSettings.ViewItems.Add(newViewItem);
                viewSettings.AddToAllViewItems(newViewItem);
            }

            foreach (ReportGroup reportGroup in ReportGroups)
            {
                viewSettings.ReportGroups.Add(reportGroup.Clone());
                viewSettings.AllReports.AddRange(reportGroup.ReportItems);
            }

            return viewSettings;
        }
    }
}