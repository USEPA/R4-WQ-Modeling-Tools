//********************************************************************************************************
//The contents of this file are subject to the Mozilla Public License Version 1.1 (the "License");
//you may not use this file except in compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS" basis, WITHOUT WARRANTY OF
//ANY KIND, either express or implied. See the License for the specificlanguage governing rights and
//limitations under the License.
//
//The Original Code is MapWindow Open Source.
//
//The Initial Developer of this version of the Original Code is Daniel P. Ames using portions created by
//Utah State University and the Idaho National Engineering and Environmental Lab that were released as
//public domain in March 2004.
//
//Contributor(s): (Open source contributors should list themselves and their modifications here).
//
//********************************************************************************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MapWindow.Interfaces;

namespace LegendControl
{
    /// <summary>
    /// One layer within the legend
    /// </summary>
    public class Layer
    {
        #region "Member Variables"

        private bool m_Expanded;
        private Legend m_Legend;
        private int m_height;

        /// <summary>
        /// Color Scheme information for this layer
        /// </summary>
        protected internal ArrayList ColorLegend;
        private object m_Icon;
        private eLayerType m_LayerType;
        private bool m_UseDynamicVisibility;
        //private SymbologySettings _symbologySettings;

        internal List<LayerElement> Elements;  // size and positions of elements
        internal bool m_smallIconWasDrawn;

        /// <summary>
        /// Top of this Layer
        /// </summary>
        protected internal int Top;

        /// <summary>
        /// Allows you to force the expansion box option to be shown, e.g. you're planning to use ExpansionBoxCustomRenderFunction.
        /// </summary>
        public bool ExpansionBoxForceAllowed = false;

        /// <summary>
        /// Allows you to render the expanded region of a layer yourself. Useful with ExpansionBoxForceAllowed=true.
        /// If you use this, you must also set ExpansionBoxCustomHeightFunction.
        /// </summary>
        public ExpansionBoxCustomRenderer ExpansionBoxCustomRenderFunction = null;

        /// <summary>
        /// Tells the legend how high your custom rendered legend will be, so that it can
        /// arrange items around it.
        /// </summary>
        public ExpansionBoxCustomHeight ExpansionBoxCustomHeightFunction = null;

        /// <summary>
        /// Provides the legend with the hatching scheme information if you wish the layer
        /// to display hatching information as well as color scheme information.
        /// </summary>
        public ShapefileFillStippleScheme HatchingScheme = null;

        /// <summary>
        /// Provides the legend with the point image scheme information if you wish the layer
        /// to display point image information as well as color scheme information.
        /// </summary>
        public ShapefilePointImageScheme PointImageScheme = null;

        /// <summary>
        /// Handle to this Layer (within the MapWinGIS.Map)
        /// </summary>
        protected internal int m_Handle;//handle of the associate layer from the Map Object

        /// <summary>
        /// Stores custom objects associated with layer
        /// </summary>
        public Hashtable m_CustomObjects = null;

        /// <summary>
        /// Returns custom object for specified key
        /// </summary>
        public object GetCustomObject(string key)
        {
            return m_CustomObjects[key];
        }

        /// <summary>
        /// Sets custom object associated with layer
        /// </summary>
        public void SetCustomObject(object obj, string key)
        {
            m_CustomObjects[key] = obj;
        }

        /// <summary>
        /// If an image layer, this tells us if the layer contains transparency
        /// </summary>
        protected internal bool HasTransparency;

        /// <summary>
        /// Indicates whether to hide this layer when drawing the legend.
        /// Added by Chris M in May 2006.
        /// </summary>
        private bool m_HideFromLegend;

        /// <summary>
        /// Indicates what field index should be used for displaying map tooltips.
        /// </summary>
        public int MapTooltipFieldIndex = -1;

        /// <summary>
        /// Indicates whether map tooltips should be shown for this layer.
        /// </summary>
        public bool MapTooltipsEnabled = false;

        /// <summary>
        /// (Doesn't apply to line shapefiles)
        /// Indicates whether the vertices of a line or polygon are visible.
        /// </summary>
        public bool VerticesVisible = false;

        /// <summary>
        /// If you wish to display a caption (e.g. "State Name") above the legend items for a coloring scheme, set this.
        /// Set to "" to disable.
        /// </summary>
        public string StippleSchemeFieldCaption = "";

        /// <summary>
        /// If you wish to display a caption (e.g. "Region") above the legend items for a stipple scheme, set this.
        /// Set to "" to disable.
        /// </summary>
        public string ColorSchemeFieldCaption = "";

        /// <summary>
        /// If you wish to display a caption (e.g. "State Name") above the legend items for a point image scheme, set this.
        /// Set to "" to disable.
        /// </summary>
        public string PointImageFieldCaption = "";

        #endregion "Member Variables"

        /// <summary>
        /// Constructor
        /// </summary>
        public Layer(Legend leg)
        {
            //The next line MUST GO FIRST in the constructor
            m_Legend = leg;
            //The previous line MUST GO FIRST in the constructor

            Expanded = (m_Legend.m_Map.ShapeDrawingMethod == MapWinGIS.tkShapeDrawingMethod.dmNewSymbology);
            //Expanded = false;

            ColorLegend = new ArrayList();
            m_Handle = -1;
            m_Icon = null;
            m_LayerType = eLayerType.Invalid;
            m_UseDynamicVisibility = false;
            HasTransparency = false;

            Elements = new List<LayerElement>();

            //_symbologySettings = new SymbologySettings();
            m_CustomObjects = new Hashtable();
            m_smallIconWasDrawn = false;
            //m_height = 0;
        }

        /// <summary>
        /// Gets a snapshot (bitmap) of the layer
        /// </summary>
        /// <returns>Bitmap if successful, null (nothing) otherwise</returns>
        public System.Drawing.Bitmap Snapshot()
        {
            return m_Legend.LayerSnapshot(this.Handle);
        }

        /// <summary>
        /// Gets a snapshot (bitmap) of the layer
        /// </summary>
        /// <param name="imgWidth">Desired width in pixels of the snapshot</param>
        /// <returns>Bitmap if successful, null (nothing) otherwise</returns>
        public System.Drawing.Bitmap Snapshot(int imgWidth)
        {
            return m_Legend.LayerSnapshot(this.Handle, imgWidth);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Layer()
        {
        }

        /// <summary>
        /// Gets the Handle for this layer
        /// </summary>
        public int Handle
        {
            get
            {
                return m_Handle;
            }
        }

        /// <summary>
        /// Gets or sets the data type of the layer.
        /// Note:  This property should only be set when specifying a
        /// grid layer.  Shapefile layers and image layers are automatically
        /// set to the correct value
        /// </summary>
        public eLayerType Type
        {
            get
            {
                return m_LayerType;
            }
            set
            {
                m_LayerType = value;
            }
        }

        /// <summary>
        /// Specifies whether or not the layer should be using dynamic visibility.  The legend will draw the check box grey if the layer is using dynamic visibility.
        /// </summary>
        public bool UseDynamicVisibility
        {
            get
            {
                if (m_Legend.m_Map.ShapeDrawingMethod != MapWinGIS.tkShapeDrawingMethod.dmNewSymbology)
                    return m_UseDynamicVisibility;
                else
                    return m_Legend.m_Map.get_LayerDynamicVisibility(this.Handle);
            }
            set
            {
                if (m_Legend.m_Map.ShapeDrawingMethod != MapWinGIS.tkShapeDrawingMethod.dmNewSymbology)
                    m_UseDynamicVisibility = value;
                else
                    m_Legend.m_Map.set_LayerDynamicVisibility(this.Handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the maximium scale at which the layer is still visible when dynamic visiblity is used
        /// </summary>
        public double MaxVisibleScale
        {
            get
            {
                return m_Legend.m_Map.get_LayerMaxVisibleScale(this.m_Handle);
            }
            set
            {
                m_Legend.m_Map.set_LayerMaxVisibleScale(this.m_Handle, value);
            }
        }

        /// <summary>
        /// Gets or sets the minimum scale at which the layer is still visible when dynamic visiblity is used
        /// </summary>
        public double MinVisibleScale
        {
            get
            {
                return m_Legend.m_Map.get_LayerMinVisibleScale(this.m_Handle);
            }
            set
            {
                m_Legend.m_Map.set_LayerMinVisibleScale(this.m_Handle, value);
            }
        }

        /// <summary>
        /// Regenerates the Color Scheme associate with this layer and
        /// causes the control to redraw itself.
        /// </summary>
        public void Refresh()
        {
            NewColorLegend = m_Legend.m_Map.GetColorScheme(this.Handle);
            m_Legend.Redraw();
        }

        private object NewColorLegend
        {
            set
            {
                object ColorScheme = value;

                ColorLegend.Clear();

                Color startColor, endColor;
                string startVal, endVal;
                string Caption;
                int NumBreaks;
                ColorInfo ci;

                this.HasTransparency = m_Legend.HasTransparency(m_Legend.Map.get_GetObject(this.Handle));

                if (ColorScheme != null)
                {
                    MapWinGIS.ShapefileColorScheme sfcs = ColorScheme as MapWinGIS.ShapefileColorScheme;
                    if (sfcs != null)
                    {
                        MapWinGIS.ShapefileColorBreak Break = null;
                        NumBreaks = sfcs.NumBreaks();
                        for (int i = 0; i < NumBreaks; i++)
                        {
                            //get the break
                            Break = sfcs.get_ColorBreak(i);

                            //get the start and end colors
                            startColor = globals.UintToColor(Break.StartColor);
                            endColor = globals.UintToColor(Break.EndColor);

                            Caption = Break.Caption;
                            if (Caption.Length < 1)
                            {
                                //get the values for the caption
                                startVal = Break.StartValue.ToString();

                                endVal = Break.EndValue.ToString();

                                if (startVal.CompareTo(endVal) == 0)
                                    Caption = startVal;
                                else
                                    Caption = startVal + " - " + endVal;
                            }

                            //add to the list
                            ci = new ColorInfo(startColor, endColor, Caption);

                            ColorLegend.Add(ci);
                        }
                    }
                    else
                    {
                        MapWinGIS.GridColorScheme gcs = ColorScheme as MapWinGIS.GridColorScheme;
                        if (gcs != null)
                        {
                            MapWinGIS.GridColorBreak gcBreak;
                            NumBreaks = gcs.NumBreaks;
                            for (int i = 0; i < NumBreaks; i++)
                            {
                                //get the break
                                gcBreak = gcs.get_Break(i);

                                //get the start and end colors
                                startColor = globals.UintToColor(gcBreak.LowColor);
                                endColor = globals.UintToColor(gcBreak.HighColor);

                                Caption = gcBreak.Caption;
                                if (Caption.Length < 1)
                                {
                                    //get the values for the caption
                                    startVal = Math.Round(gcBreak.LowValue, 3).ToString();
                                    endVal = Math.Round(gcBreak.HighValue, 3).ToString();
                                    Caption = startVal + " - " + endVal;//generate a caption
                                }

                                //add to the list
                                ci = new ColorInfo(startColor, endColor, Caption);
                                ColorLegend.Add(ci);
                            }

                            //now add the NoDataColor information
                            ci = new ColorInfo(globals.UintToColor(gcs.NoDataColor), globals.UintToColor(gcs.NoDataColor), "No Data", HasTransparency);
                            ColorLegend.Add(ci);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon that appears next to this layer in the legend.
        /// Setting this value to null(nothing) removes the icon from the legend
        /// and sets it back to the default icon.
        /// </summary>
        public object Icon
        {
            get
            {
                return m_Icon;
            }
            set
            {
                if (globals.IsSupportedPicture(value))
                {
                    m_Icon = value;
                }
                else
                {
                    throw new System.Exception("Legend Error: Invalid Group Icon type");
                }
            }
        }

        /// <summary>
        /// Calculates the height of the layer
        /// </summary>
        /// <param name="UseExpandedHeight">If True, the height returned is the expanded height. Otherwise, the height is the displayed height of the layer</param>
        /// <returns>Height of layer(depends on 'Expanded' state of the layer)</returns>
        protected internal int CalcHeight(bool UseExpandedHeight)
        {
            // to affect drawing of the expansion box externally
            if (m_Expanded && ExpansionBoxCustomHeightFunction != null)
            {
                int ht = Constants.ITEM_HEIGHT;
                bool Handled = false;
                ExpansionBoxCustomHeightFunction(m_Handle, m_Legend.Width, ref ht, ref Handled);
                if (Handled)
                    return ht + Constants.ITEM_HEIGHT + Constants.EXPAND_BOX_TOP_PAD * 2;
                else
                    return Constants.ITEM_HEIGHT;
            }

            int ret = 0;

            if (m_Legend.m_Map.ShapeDrawingMethod != MapWinGIS.tkShapeDrawingMethod.dmNewSymbology ||
               (this.Type == eLayerType.Grid || this.Type == eLayerType.Image))
            {
                // Our own calculation
                if (UseExpandedHeight == false && (m_Expanded == false || ColorLegend.Count == 0)) //|| (this.Type == eLayerType.Image))
                    ret = Constants.ITEM_HEIGHT;
                else
                    ret = Constants.ITEM_HEIGHT + (ColorLegend.Count * Constants.CS_ITEM_HEIGHT) + 2;

                // Add in caption space
                if (UseExpandedHeight || m_Expanded)
                    ret += (ColorSchemeFieldCaption.Trim() != "" ? Constants.CS_ITEM_HEIGHT : 0) + (StippleSchemeFieldCaption.Trim() != "" ? Constants.CS_ITEM_HEIGHT : 0);

                // Add in extra for hatching
                if ((UseExpandedHeight || m_Expanded == true) && (HatchingScheme != null && HatchingScheme.NumHatches() > 0))
                    ret += (HatchingScheme.NumHatches() * Constants.CS_ITEM_HEIGHT);

                // Add in extra for point image scheme (5/2/2010 DK)
                if ((UseExpandedHeight || m_Expanded == true) && (PointImageScheme != null && PointImageScheme.NumberItems > 0))
                {
                    if (PointImageFieldCaption == "")
                    {
                        ret += (PointImageScheme.Items.Count * Constants.CS_ITEM_HEIGHT) + 2;
                    }
                    else
                    {
                        ret += ((PointImageScheme.Items.Count + 1) * Constants.CS_ITEM_HEIGHT) + 2;
                    }
                }
            }

            // --------------------------------------------------------
            //      new drawing procedure
            // --------------------------------------------------------
            else if (m_Legend.m_Map.ShapeDrawingMethod == MapWinGIS.tkShapeDrawingMethod.dmNewSymbology)
            {
                MapWinGIS.Shapefile sf = m_Legend.m_Map.get_GetObject(this.Handle) as MapWinGIS.Shapefile;

                if ((UseExpandedHeight || m_Expanded))
                {
                    ret = Constants.ITEM_HEIGHT + 2;                            // layer name

                    // height of symbology or label
                    int val1, val2;
                    val1 = (get_CategoryHeight(sf.DefaultDrawingOptions) + 2);  // default symbology

                    if (sf.Labels.Count == 0 || sf.Labels.Visible == false || true) // labels aren't drawn currently
                    {
                        ret += val1;
                    }
                    else
                    {
                        // label preview is present
                        LabelStyle style = new LabelStyle(sf.Labels.Options);
                        System.Drawing.Bitmap img = new System.Drawing.Bitmap(500, 200);
                        Graphics g = Graphics.FromImage(img);
                        Size size = style.MeasureString(g, "String", 30);
                        val2 = size.Height + 2;
                        ret += val1 > val2 ? val1 : val2;
                    }

                    if (sf.Categories.Count > 0)
                    {
                        ret += Constants.CS_ITEM_HEIGHT + 2;   // caption

                        MapWinGIS.ShapefileCategories categories = sf.Categories;
                        if (this.Type == eLayerType.LineShapefile || this.Type == eLayerType.PolygonShapefile)
                        {
                            ret += sf.Categories.Count * (Constants.CS_ITEM_HEIGHT + 2);
                        }
                        else
                        {
                            for (int i = 0; i < sf.Categories.Count; i++)
                            {
                                ret += get_CategoryHeight(categories.get_Item(i).DrawingOptions);
                            }
                        }
                        ret += 2;
                    }

                    if (sf.Charts.Count > 0 && sf.Charts.NumFields > 0 && sf.Charts.Visible)
                    {
                        ret += (Constants.CS_ITEM_HEIGHT + 2);   // caption
                        ret += sf.Charts.IconHeight;
                        ret += 2;

                        ret += (sf.Charts.NumFields * (Constants.CS_ITEM_HEIGHT + 2));
                    }
                }
                else
                    ret = Constants.ITEM_HEIGHT;

                // TODO: Add caption space
                //if (UseExpandedHeight || m_Expanded)
                //    ret += (ColorSchemeFieldCaption.Trim() != "" ? Constants.CS_ITEM_HEIGHT : 0) + (StippleSchemeFieldCaption.Trim() != "" ? Constants.CS_ITEM_HEIGHT : 0);
            }

            m_height = ret;  // cahching height here to get ri of recalculation when there are lots of categories

            return ret;
        }

        /// <summary>
        /// Calculates the height of the given category
        /// </summary>
        /// <param name="options"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int get_CategoryHeight(MapWinGIS.ShapeDrawingOptions options)
        {
            if (this.Type == eLayerType.PolygonShapefile || this.Type == eLayerType.LineShapefile)
            {
                return Constants.CS_ITEM_HEIGHT + 2;
            }
            else if (this.Type == eLayerType.PointShapefile && options.Picture != null && options.PointType == MapWinGIS.tkPointSymbolType.ptSymbolPicture)
            {
                if (options.Picture.Height * options.PictureScaleY + 2 <= Constants.CS_ITEM_HEIGHT)
                {
                    return Constants.CS_ITEM_HEIGHT + 2;
                }
                else
                    return (int)(options.Picture.Height * options.PictureScaleY + 2);
            }
            else if (this.Type == eLayerType.PointShapefile)
            {
                if (options.PointSize + 2 <= Constants.CS_ITEM_HEIGHT)
                {
                    return Constants.CS_ITEM_HEIGHT + 2;
                }
                else
                    return (int)options.PointSize + 2;
            }
            else
                return 0;
        }

        /// <summary>
        /// Returns the width of icon for specified set of options
        /// </summary>
        public int get_CategoryWidth(MapWinGIS.ShapeDrawingOptions options)
        {
            int maxWidth = 100;
            if (this.Type == eLayerType.PolygonShapefile || this.Type == eLayerType.LineShapefile)
            {
                return Constants.ICON_WIDTH;
            }
            else if (this.Type == eLayerType.PointShapefile && options.Picture != null && options.PointType == MapWinGIS.tkPointSymbolType.ptSymbolPicture)
            {
                if (options.Picture.Width * options.PictureScaleX <= Constants.ICON_WIDTH)
                {
                    return Constants.ICON_WIDTH;
                }
                else
                {
                    int width = (int)(options.Picture.Width * options.PictureScaleX);
                    if (width <= maxWidth)
                        return width;
                    else
                        return maxWidth;
                }
            }
            else if (this.Type == eLayerType.PointShapefile)
            {
                if (options.PointSize <= Constants.ICON_WIDTH)
                {
                    return Constants.ICON_WIDTH;
                }
                else
                {
                    int width = (int)options.PointSize;
                    if (width <= maxWidth)
                        return width;
                    else
                        return maxWidth;
                }
            }
            else
                return 0;
        }

        /// <summary>
        /// Calculates the maximium width of the icon for the layer going through all categories
        /// </summary>
        /// <returns></returns>
        public int get_MaxIconWidth(MapWinGIS.Shapefile sf)
        {
            if (sf == null)
                return 0;
            int maxWidth = get_CategoryWidth(sf.DefaultDrawingOptions);
            for (int i = 0; i < sf.Categories.Count; i++)
            {
                int width = get_CategoryWidth(sf.Categories.get_Item(i).DrawingOptions);
                if (width > maxWidth)
                    maxWidth = width;
            }
            return maxWidth;
        }

        /// <summary>
        /// Calculates the height of a layer
        /// </summary>
        /// <returns>Height of layer(depends on Expanded state of the layer)</returns>
        protected internal int CalcHeight()
        {
            return CalcHeight(this.Expanded);
        }

        public int Height
        {
            get
            {
                return CalcHeight();
            }
        }

        /// <summary>
        /// Gets or sets whether or not the Layer is expanded.  This shows or hides the
        /// layer's Color Scheme (if one exists).
        /// </summary>
        public bool Expanded
        {
            get
            {
                return m_Expanded;
            }
            set
            {
                m_Expanded = value;
                m_Legend.Redraw();
            }
        }

        /// <summary>
        /// Gets or Sets the visibility of the layer
        /// </summary>
        public bool Visible
        {
            get
            {
                return m_Legend.m_Map.get_LayerVisible(m_Handle);
            }
            set
            {
                bool oldVal = m_Legend.m_Map.get_LayerVisible(m_Handle);
                if (oldVal != value)
                {
                    m_Legend.m_Map.set_LayerVisible(m_Handle, value);
                    m_Legend.Redraw();
                }
            }
        }

        /// <summary>
        /// Indicates whether to skip over the layer when saving a project.
        /// </summary>
        public bool SkipOverDuringSave
        {
            get
            {
                return m_Legend.m_Map.get_LayerSkipOnSaving(this.Handle);
                //return m_SkipDuringSave;
            }
            set
            {
                m_Legend.m_Map.set_LayerSkipOnSaving(this.Handle, value);
                //m_SkipDuringSave = value;
            }
        }

        /// <summary>
        /// Indicates whether to skip over the layer when drawing the legend.
        /// </summary>
        public bool HideFromLegend
        {
            get
            {
                return m_HideFromLegend;
            }
            set
            {
                m_HideFromLegend = value;
            }
        }
    }
}