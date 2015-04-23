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
 * Summary  : Cache of views
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2015
 */

using Scada.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace Scada.Web
{
    /// <summary>
    /// Cache of views
    /// <para>Кэш представлений</para>
    /// </summary>
    public class ViewCache
    {
        /// <summary>
        /// Кэш представлений
        /// </summary>
        protected BaseView[] views;
        /// <summary>
        /// Имена файлов представлений
        /// </summary>
        protected string[] fileNames;


        /// <summary>
        /// Конструктор
        /// </summary>
        public ViewCache()
        {
            views = null;
            fileNames = null;
        }


        /// <summary>
        /// Инициализировать кэш представлений
        /// </summary>
        public void InitViewCache(ViewSettings viewSettings)
        {
            try
            {
                if (viewSettings != null)
                {
                    int viewCnt = viewSettings.AllViewItems.Count;
                    views = new BaseView[viewCnt];
                    fileNames = new string[viewCnt];

                    for (int viewInd = 0; viewInd < viewCnt; viewInd++)
                    {
                        views[viewInd] = null;
                        fileNames[viewInd] = viewSettings.AllViewItems[viewInd].FileName;
                    }
                }
            }
            catch (Exception ex)
            {
                AppData.Log.WriteAction((Localization.UseRussian ? "Ошибка при инициализации кэша представлений: " :
                    "Error initializing cache of views: ") + ex.Message, Log.ActTypes.Exception);
            }
        }

        /// <summary>
        /// Получить представление только из кэша
        /// </summary>
        public BaseView GetView(int viewIndex)
        {
            return views != null && 0 <= viewIndex && viewIndex < views.Length ? views[viewIndex] : null;
        }

        /// <summary>
        /// Получить представление заданного типа из кэша или от сервера
        /// </summary>
        public T GetView<T>(int viewIndex) where T : BaseView
        {
            T view = null;

            try
            {
                if (views != null && 0 <= viewIndex && viewIndex < views.Length)
                {
                    BaseView viewCache = views[viewIndex];
                    Type viewType = typeof(T);

                    if (viewCache != null || viewCache.GetType() == viewType)
                    {
                        // получение представления из кэша
                        view = (T)viewCache;
                    }
                    else
                    {
                        // получение представления от сервера
                        view = (T)Activator.CreateInstance(viewType);
                        string fileName = fileNames[viewIndex];

                        if (!view.StoredOnServer)
                            view.ItfObjName = Path.GetFileName(fileName);

                        if (!view.StoredOnServer ||
                            AppData.MainData.ServerComm.ReceiveView(fileName, view))
                        {
                            AppData.MainData.RefreshBase();
                            view.BindCnlProps(AppData.MainData.CnlPropsArr);
                            views[viewIndex] = view;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppData.Log.WriteAction((Localization.UseRussian ? 
                    "Ошибка при получении представления из кэша или от сервера: " :
                    "Error getting view from the cache or from the server: ") + ex.Message, Log.ActTypes.Exception);
            }

            return view;
        }
    }
}