﻿using System;
using MapWinGIS;
using MWLite.Core;
using MWLite.Core.UI;
using MWLite.Databases;
using MWLite.Databases.Forms;
using MWLite.GUI.Forms;
using MWLite.ShapeEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MWLite.Core.Exts;
using MWLite.Symbology.Forms;

namespace MWLite.GUI.Helpers
{
    internal static class LayerHelper
    {
        public static void ShowLayerProperties(int layerHandle)
        {
            var sf = App.Map.get_Shapefile(layerHandle);
            if (sf != null)
            {
                using (var form = new frmSymbologyMain(App.Legend, layerHandle))
                {
                    if (form.ShowDialog(MainForm.Instance) == DialogResult.OK)
                    {
                        // do something
                    }
                }
            }
            else
            {
                using (var imageForm = new ImagePropertiesForm(layerHandle))
                {
                    if (imageForm.ShowDialog(MainForm.Instance) == DialogResult.OK)
                    {
                        
                    }
                }
            }
        }
        
        public static void AddLayer(object layer)
        {
            if (layer == null) return;
            App.Legend.Layers.Add(layer, true);
        }

        public static void AddLayer(LayerType layerType)
        {
            var map = App.Map;

            var dlg = new OpenFileDialog { Filter = map.GetLayerFilter(layerType), Multiselect = true };

            if (dlg.ShowDialog() != DialogResult.OK) return;
            
            var legend = App.Legend;
                
            legend.Lock();
            map.LockWindow(tkLockMode.lmLock);

            string layerName = "";
            try
            {
                var fm = new FileManager();
                foreach (var name in dlg.FileNames.ToList())
                {
                    layerName = name;
                    var layer = fm.Open(name);
                    if (layer == null)
                    {
                        string msg = string.Format("Failed to open datasource: {0} \n {1}", name, fm.ErrorMsg[fm.LastErrorCode]);
                        MessageHelper.Warn(msg);
                    }
                    else if (layer is OgrDatasource)
                    {
                        var ds = layer as OgrDatasource;
                        for (int i = 0; i < ds.LayerCount; i++)
                        {
                            var l = ds.GetLayer(i, false);
                            AddLayer(l);
                        }
                        map.ZoomToMaxExtents();
                    }
                    else
                    { 
                        AddLayer(layer);
                    }
                }
            }
            catch
            {
                MessageHelper.Warn("There was a problem opening layer: " + layerName);
            }
            finally
            {
                legend.Unlock();
                map.LockWindow(tkLockMode.lmUnlock);
            }
        }

        public static void RemoveLayer()
        {
            int layerHandle = App.Legend.SelectedLayer;
            if (layerHandle != -1)
            {
                if (Editor.StopLayerEditing(layerHandle))
                {
                    if (MessageHelper.Ask("Do you want to remove layer?") == DialogResult.Yes)
                    {
                        App.Legend.Layers.Remove(layerHandle);
                    }
                    App.Legend.Refresh();
                }
            }
        }

        public static void ZoomToLayer()
        {
            int handle = App.Legend.SelectedLayer;
            App.Map.ZoomToLayer(handle);
        }

        public static void ZoomToSelected()
        {
            int handle = App.Legend.SelectedLayer;
            App.Map.ZoomToSelected(handle);
        }

        public static void ClearSelection()
        {
            int handle = App.Legend.SelectedLayer;
            var sf = App.Map.get_Shapefile(handle);
            if (sf != null)
            {
                sf.SelectNone();
                MainForm.Instance.RefreshUI();
                App.Map.Redraw();
            }
        }

        public static void OpenOgrLayer()
        {
            using (var form = new OgrLayerForm())
            {
                form.LayerAdded += (s, e) =>
                {
                    if (e.Layer == null) return;
                    AddLayer(e.Layer);
                    App.Map.Refresh();
                    App.Legend.Refresh();
                };
                form.ShowDialog(MainForm.Instance);
            }
        }

        public static void ImportOgrLayer()
        {
            int layerHandle = App.Legend.SelectedLayer;
            if (layerHandle == -1) return;

            var sf = App.Map.get_Shapefile(layerHandle);
            if (sf == null)
            {
                MessageHelper.Info("Selected layer is not a vector layer.");
                return;
            }

            using (var form = new OgrConnectionForm())
            {
                if (form.ShowDialog(MainForm.Instance) == DialogResult.OK)
                {
                    var ds = new OgrDatasource();
                    if (!OgrHelper.OpenDatasource(ds, form.ConnectionParams))
                        return;

                    string layerName = App.Map.get_LayerName(layerHandle);
                    layerName = layerName.Replace(".", "_");

                    using (var importForm = new OgrImportShapefile(layerName))
                    {
                        if (importForm.ShowDialog(MainForm.Instance) == DialogResult.OK)
                        {
                            layerName = importForm.LayerName;
                            if (!ds.ImportShapefile(sf, layerName, "", tkShapeValidationMode.NoValidation))
                            {
                                MessageHelper.Warn("Failed to import shapefile: " + ds.GdalLastErrorMsg);
                            }
                            else
                            {
                                MessageHelper.Info("Layer was imported: " + layerName);
                            }
                        }
                    }
                }
            }
        }

        public static void SaveCurrentStyle()
        {
            var legend = App.Legend;
            int layerHandle = legend.SelectedLayer;
            if (layerHandle != -1)
            {
                MessageHelper.Info(App.Map.SaveLayerOptions(layerHandle, "", true, "")
                    ? "Layer options are saved."
                    : "Failed to save layer options.");
            }
        }

        public static void LoadLayerStyle()
        {
            var legend = App.Legend;
            int layerHandle = legend.SelectedLayer;
            if (layerHandle != -1)
            {
                string description = "";
                bool result = App.Map.LoadLayerOptions(layerHandle, "", ref description);
                if (result)
                {
                    App.Map.Redraw();
                    App.Legend.Refresh();
                    MessageHelper.Info("Options are loaded successfully.");
                }
                else
                {
                    string msg = "No options are loaded: " + App.Map.get_ErrorMsg(App.Map.LastErrorCode);
                    var layer = App.Map.get_OgrLayer(layerHandle);
                    if (layer != null)
                    {
                        msg += Environment.NewLine + "Last GDAL error message: " + layer.GdalLastErrorMsg;
                    }
                    MessageHelper.Info(msg);
                }
            }
            else
            {
                MessageHelper.Info("No layer is selected.");
            }
        }
    }
}
