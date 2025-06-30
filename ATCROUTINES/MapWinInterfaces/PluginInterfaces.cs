using System;

namespace MapWindow
{
	namespace PluginInterfaces
	{
		/// <summary>
		/// Provides details such as Author and Description for a plug-in. Required for any
		/// plug-in that's loaded by MapWindow 5. (MapWindow 4 will also load plugins
		/// implementing IPlugin).
		/// NOTE: GUID is 55e3f056-1f5f-11dc-8314-0800200c9a66
		/// </summary>
		[System.Runtime.InteropServices.Guid("55e3f056-1f5f-11dc-8314-0800200c9a66")]
		public interface IBasePlugin
		{
			/// <summary>
			/// Author of the plugin. 
			/// </summary>
			string Author {get;}

			/// <summary>
			/// Short description of the plugin.
			/// </summary>
			string Description {get;}

			/// <summary>
			/// Name of the plugin. 
			/// </summary>
			string Name {get;}

			/// <summary>
			/// Build date. 
			/// </summary>
			string BuildDate {get;}

			/// <summary>
			/// Plugin version.
			/// </summary>
			string Version {get;}

			/// <summary>
			/// This method is called by the MapWindow when the plugin is loaded.
			/// </summary>
			void Initialize();

			/// <summary>
			/// This method is called by the MapWindow when the plugin is unloaded.
			/// </summary>
			void Terminate(); 
		}

		/// <summary>
		/// Provides the ability to add menus, toolbar icons and dockable panels, and alter the status bar (using the Statusbar argument).
		/// NOTE: GUID is 334b9e9e-1f5f-11dc-8314-0800200c9a66
		/// </summary>
		[System.Runtime.InteropServices.Guid("334b9e9e-1f5f-11dc-8314-0800200c9a66")]
		public interface IMapWinGUI
		{
			/// <summary>
			/// This method is called by the MapWindow when the plugin is loaded.
			/// </summary>
			void Initialize(MapWindow.Interfaces.Menus Menus, MapWindow.Interfaces.Toolbar Toolbar, MapWindow.Interfaces.StatusBar StatusBar, MapWindow.Interfaces.UIPanel DockableToolbars, int MapWindowFormHandle);

			/// <summary>
			/// This method is called by the MapWindow when the plugin is unloaded.
			/// </summary>
			void Terminate();

			/// <summary>
			/// This method is called by the MapWindow when a toolbar or menu item is clicked.
			/// </summary>
			/// <param name="ItemName">Name of the item that was clicked.</param>
			/// <param name="Handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
			void ItemClicked(string ItemName, ref bool Handled);
		}

		/// <summary>
		/// Allows a plug-in to interact with other plug-ins (send/receive messages; turn on/off; list)
		/// NOTE: GUID is 334b9ea2-1f5f-11dc-8314-0800200c9a66
		/// </summary>
		[System.Runtime.InteropServices.Guid("334b9ea2-1f5f-11dc-8314-0800200c9a66")]
		public interface IPluginInteraction
		{
			/// <summary>
			/// This method is called by the MapWindow when the plugin is loaded.
			/// </summary>
			void Initialize(PluginTracker Plugins);

			/// <summary>
			/// This method is called by the MapWindow when the plugin is unloaded.
			/// </summary>
			void Terminate();

			/// <summary>
			/// This message is relayed by the MapWindow when another plugin broadcasts a message.  Messages can be used to send messages between plugins.
			/// </summary>
			/// <param name="msg">The message being relayed.</param>
			/// <param name="Handled">Set this parameter to true if your plugin handles this so that no other plugins recieve this message.</param>
			void Message(string msg, ref bool Handled);
		}

		/// <summary>
		/// Interface for manipulating plugins which have a base IBasePlugin type.
		/// This differs from "Plugins" in that the Item ([]) property returns IBasePlugin
		/// instead of IPlugin. The original "Plugins" cannot be changed without breaking the
		/// backward compatibility of the interface.
		/// </summary>
		// (No GUID necessary - not directly a plugin type)
		public interface PluginTracker
		{
			/// <summary>
			/// clears all plugins from the list of available plugins, but doesn't unload loaded plugins
			/// </summary>
			void Clear();
			/// <summary>
			/// Add a plugin from a file
			/// </summary>
			/// <param name="Path">path to the plugin</param>
			/// <returns>true on success, false on failure</returns>
			bool AddFromFile(string Path);
			/// <summary>
			/// Adds any compatible plugins from a directory(recursive into subdirs)
			/// </summary>
			/// <param name="Path">path to the directory</param>
			/// <returns>true on success, false otherwise</returns>
			bool AddFromDir(string Path);
			/// <summary>
			/// Loads a plugin from an instance of an object
			/// </summary>
			/// <param name="Plugin">the Plugin object to load</param>
			/// <param name="PluginKey">The Key by which this plugin can be identified at a later time</param>
			/// <param name="SettingsString">A string that contains any settings that should be passed to the plugin after it is loaded into the system</param>
			/// <returns>true on success, false otherwise</returns>
			bool LoadFromObject(IBasePlugin Plugin, string PluginKey, string SettingsString);
			/// <summary>
			/// Loads a plugin from an instance of an object
			/// </summary>
			/// <param name="Plugin">the Plugin object to load</param>
			/// <param name="PluginKey">The Key by which this plugin can be identified at a later time</param>
			/// <returns>true on success, false otherwise</returns>
			bool LoadFromObject(IBasePlugin Plugin, string PluginKey);
			/// <summary>
			/// Starts (loads) a specified plugin
			/// </summary>
			/// <param name="Key">Identifying key for the plugin to start</param>
			/// <returns>true on success, false otherwise</returns>
			bool StartPlugin(string Key);
			/// <summary>
			/// Stops (unloads) a specified plugin
			/// </summary>
			/// <param name="Key">Identifying key for the plugin to stop</param>
			void StopPlugin(string Key);
			/// <summary>
			/// number of available plugins
			/// </summary>
			int Count{get;}

			/// <summary>
			/// Gets an IPlugin object from the list of all loaded plugins
			/// <param name="Index">0-based index into the list of plugins</param>
			/// </summary>
			IBasePlugin this[int Index]{get;}

			/// <summary>
			/// Removes a plugin from the list of available plugins and unloads the plugin if loaded
			/// </summary>
			/// <param name="IndexOrKey">0-based integer index or string key for the plugin to remove</param>
			void Remove(object IndexOrKey);

			/// <summary>
			/// Gets or Sets the default folder where plugins are loaded from 
			/// </summary>
			string PluginFolder{get;set;}

			/// <summary>
			/// Checks to see if a plugin is currently loaded (running)
			/// </summary>
			/// <param name="Key">Unique key identifying the plugin</param>
			/// <returns>true if loaded, false otherwise</returns>
			bool PluginIsLoaded(string Key);

			/// <summary>
			/// Shows the dialog for loading/starting/stopping plugins
			/// </summary>
			void ShowPluginDialog();

			/// <summary>
			/// Sends a broadcast message to all loaded plugins
			/// </summary>
			/// <param name="Message">The message that should be sent</param>
			void BroadcastMessage(string Message);

			/// <summary>
			/// Returns the key belonging to a plugin with the given name. An empty string is returned if the name is not found.
			/// </summary>
			/// <param name="PluginName">The name of the plugin</param>
			string GetPluginKey(string PluginName);
		}

        /// <summary>
        /// Represents the currently loaded project.
        /// </summary>
		[System.Runtime.InteropServices.Guid("334b9ea4-1f5f-11dc-8314-0800200c9a66")]
		public interface IProject
		{
			/// <summary>
			/// This method is called by the MapWindow when the plugin is loaded.
			/// </summary>
			void Initialize(MapWindow.Interfaces.Project Project);

			/// <summary>
			/// This method is called by the MapWindow when the plugin is unloaded.
			/// </summary>
			void Terminate();
		}

        /// <summary>
        /// Represents events that occur on/with project files.
        /// </summary>
		[System.Runtime.InteropServices.Guid("334b9eaa-1f5f-11dc-8314-0800200c9a66")]
		public interface IProjectEvents
		{
			/// <summary>
			/// This method is called by the MapWindow when the plugin is loaded.
			/// </summary>
			void Initialize();

			/// <summary>
			/// This method is called by the MapWindow when the plugin is unloaded.
			/// </summary>
			void Terminate();

			/// <summary>
			/// This method is called by the MapWindow when a project is being loaded.
			/// </summary>
			/// <param name="ProjectFile">Filename of the project file.</param>
			/// <param name="SettingsString">Settings string for this plugin from the project file.</param>
			void ProjectLoading(string ProjectFile, string SettingsString);
			
			/// <summary>
			/// This method is called by the MapWindow when a project is being saved.
			/// </summary>
			/// <param name="ProjectFile">Filename of the project file.</param>
			/// <param name="SettingsString">Reference parameter.  Set this value in order to save your plugin's settings string in the project file.</param>
			void ProjectSaving(string ProjectFile, ref string SettingsString);
		}
	}
}
