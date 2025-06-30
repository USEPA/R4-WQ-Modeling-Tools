// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PluginBase.cs" company="MapWindow Team">
//   Mozilla Public License Version 1.1
// </copyright>
// <summary>
//   A class that providers a simpler way for creating MapWindow Plugins.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace MapWindow.Interfaces
{
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// A class that providers a simpler way for creating MapWindow Plugins.
    /// </summary>
    public abstract class PluginBase : IPlugin
    {
        #region Constants and Fields

        private FileVersionInfo _file = null;

        #endregion

        #region Public Properties

        /// <summary>
        /// Author of the plugin.
        /// </summary>
        public virtual string Author
        {
            get
            {
                return this.ReferenceFile.CompanyName;
            }
        }

        /// <summary>
        /// Build date.
        /// </summary>
        public virtual string BuildDate
        {
            get
            {
                return File.GetLastWriteTime(this.ReferenceAssembly.Location).ToLongDateString();
            }
        }

        /// <summary>
        /// Short description of the plugin.
        /// </summary>
        public virtual string Description
        {
            get
            {
                return this.ReferenceFile.Comments;
            }
        }

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return this.ReferenceFile.ProductName;
            }
        }

        /// <summary>
        /// Plugin serial number.  NO LONGER NEEDED; kept for backward compatibility.
        /// </summary>
        public virtual string SerialNumber
        {
            get
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Plugin version.
        /// </summary>
        public virtual string Version
        {
            get
            {
                return this.ReferenceFile.FileVersion;
            }
        }

        #endregion

        #region Properties

        protected IMapWin App { get; set; }

        protected int ParentHandle { get; set; }

        private Assembly ReferenceAssembly
        {
            get
            {
                return this.GetType().Assembly;
            }
        }

        private FileVersionInfo ReferenceFile
        {
            get
            {
                return this._file ?? (this._file = FileVersionInfo.GetVersionInfo(this.ReferenceAssembly.Location));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the specified app.
        /// </summary>
        /// <param name="app">The app.</param>
        /// <param name="parentHandle">The parent handle.</param>
        public virtual void Initialize(IMapWin app, int parentHandle)
        {
            this.App = app;
            this.ParentHandle = parentHandle;
            this.Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// This method is called by the MapWindow when a toolbar or menu item is clicked.
        /// </summary>
        /// <param name="itemName">Name of the item that was clicked.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void ItemClicked(string itemName, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when a layer is removed from the map.
        /// </summary>
        /// <param name="handle">Handle of the layer that was removed.</param>
        public virtual void LayerRemoved(int handle)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when a layer is selected in code or by the legend.
        /// </summary>
        /// <param name="handle">Handle of the newly selected layer.</param>
        public virtual void LayerSelected(int handle)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when one or more layer(s) is/are added.
        /// </summary>
        /// <param name="layers">An array of <c>Layer</c> objects that were added.</param>
        public virtual void LayersAdded(Layer[] layers)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when all layers are cleared from the map.
        /// </summary>
        public virtual void LayersCleared()
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user double-clicks on the legend.
        /// </summary>
        /// <param name="handle">Handle of the layer or group that was double-clicked</param>
        /// <param name="location">Location that was clicked.  Either a layer or a group.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void LegendDoubleClick(int handle, ClickLocation location, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user presses a mouse button on the legend.
        /// </summary>
        /// <param name="handle">Handle of the layer or group that was under the cursor.</param>
        /// <param name="button">The mouse button that was pressed.  You can use the <c>vb6Buttons</c> enumeration to determine which button was pressed.</param>
        /// <param name="location">Location that was clicked.  Either a layer or a group</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void LegendMouseDown(int handle, int button, ClickLocation location, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user releases a mouse button on the legend.
        /// </summary>
        /// <param name="handle">Handle of the layer or group that was under the cursor.</param>
        /// <param name="button">The mouse button that was released.  You can use the <c>vb6Buttons</c> enumeration to determine which button it was.</param>
        /// <param name="location">Location that was clicked.  Either a layer or a group</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void LegendMouseUp(int handle, int button, ClickLocation location, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user completes a dragging operation on the map.
        /// </summary>
        /// <param name="bounds">The rectangle that was selected, in pixel coordinates.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void MapDragFinished(Rectangle bounds, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the extents of the map have changed.
        /// </summary>
        public virtual void MapExtentsChanged()
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user presses a mouse button over the map.
        /// </summary>
        /// <param name="button">The button that was pressed.  These values can be compared to the <c>vb6Butttons</c> enumerations.</param>
        /// <param name="shift">This value represents the state of the <c>ctrl</c>, <c>alt</c> and <c>shift</c> keyboard buttons.  The values use VB6 values.</param>
        /// <param name="x">The x coordinate in pixels.</param>
        /// <param name="y">The y coordinate in pixels.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void MapMouseDown(int button, int shift, int x, int y, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user moves the mouse over the map display.
        /// </summary>
        /// <param name="screenX">The x coordinate in pixels.</param>
        /// <param name="screenY">The y coordinate in pixels.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void MapMouseMove(int screenX, int screenY, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the user releases a mouse button over the map.
        /// </summary>
        /// <param name="button">The button that was released.  These values can be compared to the <c>vb6Butttons</c> enumerations.</param>
        /// <param name="shift">This value represents the state of the <c>ctrl</c>, <c>alt</c> and <c>shift</c> keyboard buttons.  The values use VB6 values.</param>
        /// <param name="x">The x coordinate in pixels.</param>
        /// <param name="y">The y coordinate in pixels.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void MapMouseUp(int button, int shift, int x, int y, ref bool handled)
        {
        }

        /// <summary>
        /// This message is relayed by the MapWindow when another plugin broadcasts a message.  Messages can be used to send messages between plugins.
        /// </summary>
        /// <param name="msg">The message being relayed.</param>
        /// <param name="handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
        public virtual void Message(string msg, ref bool handled)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when a project is being loaded.
        /// </summary>
        /// <param name="projectFile">Filename of the project file.</param>
        /// <param name="settings">Settings string for this plugin from the project file.</param>
        public virtual void ProjectLoading(string projectFile, string settings)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when a project is being saved.
        /// </summary>
        /// <param name="projectFile">Filename of the project file.</param>
        /// <param name="settings">Reference parameter.  Set this value in order to save your plugin's settings string in the project file.</param>
        public virtual void ProjectSaving(string projectFile, ref string settings)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when shapes are selected by the user.
        /// </summary>
        /// <param name="handle">Handle of the shapefile layer that was selected on.</param>
        /// <param name="selectInfo">The <c>SelectInfo</c> object containing information about the selected shapes.</param>
        public virtual void ShapesSelected(int handle, SelectInfo selectInfo)
        {
        }

        /// <summary>
        /// This method is called by the MapWindow when the plugin is unloaded.
        /// </summary>
        public abstract void Terminate();

        #endregion
    }
}