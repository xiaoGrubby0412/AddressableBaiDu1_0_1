﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using static UnityEngine.AddressableAssets.Addressables;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.AddressableAssets
{
    public static class InternalAddressable
    {
        internal static bool reinitializeInternalAddressables = true;
        internal static AddressablesImpl m_AddressablesInternalInstance = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10), true);
        static AddressablesImpl m_AddressablesInternal
        {
            get
            {
#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
                if (EditorSettings.enterPlayModeOptionsEnabled && reinitializeInternalAddressables)
                {
                    reinitializeInternalAddressables = false;
                    m_AddressablesInternalInstance.ReleaseSceneManagerOperation();
                    m_AddressablesInternalInstance = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));
                }
#endif
                return m_AddressablesInternalInstance;
            }
        }
        /// <summary>
        /// Stores the ResourceManager associated with this Addressables instance.
        /// </summary>
        public static ResourceManager ResourceManager { get { return m_AddressablesInternal.ResourceManager; } }
        internal static AddressablesImpl Instance { get { return m_AddressablesInternal; } }

#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
        [InitializeOnLoadMethod]
        static void RegisterPlayModeStateChange()
        {
            EditorApplication.playModeStateChanged += SetAddressablesReInitFlagOnExitPlayMode;
        }

        static void SetAddressablesReInitFlagOnExitPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
                reinitializeInternalAddressables = true;
        }

#endif

        /// <summary>
        /// The Instance Provider used by the Addressables System.
        /// </summary>
        public static IInstanceProvider InstanceProvider { get { return m_AddressablesInternal.InstanceProvider; } }

        /// <summary>
        /// Used to resolve a string using addressables config values
        /// </summary>
        /// <param name="id">The internal id to resolve.</param>
        /// <returns>Returns the string that the internal id represents.</returns>
        public static string ResolveInternalId(string id)
        {
            return m_AddressablesInternal.ResolveInternalId(id);
        }

        /// <summary>
        /// Functor to transform internal ids before being used by the providers.
        /// See the [TransformInternalId](xref:addressables-api-transform-internal-id) documentation for more details.
        /// </summary>
        static public Func<IResourceLocation, string> InternalIdTransformFunc
        {
            get { return m_AddressablesInternal.InternalIdTransformFunc; }
            set { m_AddressablesInternal.InternalIdTransformFunc = value; }
        }

        /// <summary>
        /// Delegate that can be used to override the web request options before being sent.
        /// </summary>
        /// <remarks>
        /// The web request passed to this delegate has already been preconfigured internally. Override at your own risk.
        /// </remarks>
        internal static Action<UnityWebRequest> WebRequestOverride
        {
            get { return m_AddressablesInternal.WebRequestOverride; }
            set { m_AddressablesInternal.WebRequestOverride = value; }
        }


        /// <summary>
        /// The name of the PlayerPrefs value used to set the path to load the addressables runtime data file.
        /// </summary>
        public const string kAddressablesRuntimeDataPath = "AddressablesRuntimeDataPath";
        const string k_AddressablesLogConditional = "ADDRESSABLES_LOG_ALL";

        /// <summary>
        /// The name of the PlayerPrefs value used to set the path to check for build logs that need to be shown in the runtime.
        /// </summary>
        public const string kAddressablesRuntimeBuildLogPath = "AddressablesRuntimeBuildLog";

        /// <summary>
        /// The subfolder used by the Addressables system for its initialization data.
        /// </summary>
        public static string StreamingAssetsSubFolder { get { return m_AddressablesInternal.StreamingAssetsSubFolder; } }

        /// <summary>
        /// The path to the Addressables Library subfolder
        /// </summary>
        public static string LibraryPath = "Library/com.unity.addressables/";

        /// <summary>
        /// The path used by the Addressables system for its initialization data.
        /// </summary>
        public static string BuildPath { get { return Addressables.LibraryPath + "/update/" + PlatformMappingService.GetPlatformPathSubFolder(); } }

        /// <summary>
        /// The path that addressables player data gets copied to during a player build.
        /// </summary>
        public static string PlayerBuildDataPath { get { return m_AddressablesInternal.PlayerBuildDataPath; } }

        /// <summary>
        /// The path that addressables player data gets from update folder.
        /// </summary>
        public static string UpdatePath { get { return m_AddressablesInternal.UpdatePath; } }

        /// <summary>
        /// The path that addressables player data gets from update folder.
        /// </summary>
        public static string InternalRuntimePath { get { return m_AddressablesInternal.InternalRuntimePath; } }

        /// <summary>
        /// The path used by the Addressables system to load initialization data.
        /// </summary>
        public static string RuntimePath { get { return m_AddressablesInternal.RuntimePath; } }


        /// <summary>
        /// Gets the collection of configured <see cref="IResourceLocator"/> objects. Resource Locators are used to find <see cref="IResourceLocation"/> objects from user-defined typed keys.
        /// </summary>
        /// <value>The resource locators collection.</value>
        public static IEnumerable<IResourceLocator> ResourceLocators { get { return m_AddressablesInternal.ResourceLocators; } }

        [Conditional(k_AddressablesLogConditional)]
        internal static void InternalSafeSerializationLog(string msg, LogType logType = LogType.Log)
        {
            if (m_AddressablesInstance == null)
                return;
            switch (logType)
            {
                case LogType.Warning:
                    m_AddressablesInstance.LogWarning(msg);
                    break;
                case LogType.Error:
                    m_AddressablesInstance.LogError(msg);
                    break;
                case LogType.Log:
                    m_AddressablesInstance.Log(msg);
                    break;
            }
        }

        [Conditional(k_AddressablesLogConditional)]
        internal static void InternalSafeSerializationLogFormat(string format, LogType logType = LogType.Log, params object[] args)
        {
            if (m_AddressablesInstance == null)
                return;
            switch (logType)
            {
                case LogType.Warning:
                    m_AddressablesInstance.LogWarningFormat(format, args);
                    break;
                case LogType.Error:
                    m_AddressablesInstance.LogErrorFormat(format, args);
                    break;
                case LogType.Log:
                    m_AddressablesInstance.LogFormat(format, args);
                    break;
            }
        }

        /// <summary>
        /// Debug.Log wrapper method that is contional on the ADDRESSABLES_LOG_ALL symbol definition.  This can be set in the Player preferences in the 'Scripting Define Symbols'.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        [Conditional(k_AddressablesLogConditional)]
        public static void Log(string msg)
        {
            m_AddressablesInternal.Log(msg);
        }

        /// <summary>
        /// Debug.LogFormat wrapper method that is contional on the ADDRESSABLES_LOG_ALL symbol definition.  This can be set in the Player preferences in the 'Scripting Define Symbols'.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        [Conditional(k_AddressablesLogConditional)]
        public static void LogFormat(string format, params object[] args)
        {
            m_AddressablesInternal.LogFormat(format, args);
        }

        /// <summary>
        /// Debug.LogWarning wrapper method.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        public static void LogWarning(string msg)
        {
            m_AddressablesInternal.LogWarning(msg);
        }

        /// <summary>
        /// Debug.LogWarningFormat wrapper method.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        public static void LogWarningFormat(string format, params object[] args)
        {
            m_AddressablesInternal.LogWarningFormat(format, args);
        }

        /// <summary>
        /// Debug.LogError wrapper method.
        /// </summary>
        /// <param name="msg">The msg to log</param>
        public static void LogError(string msg)
        {
            m_AddressablesInternal.LogError(msg);
        }

        /// <summary>
        /// Debug.LogException wrapper method.
        /// </summary>
        /// <param name="op">The operation handle.</param>
        /// <param name="ex">The exception.</param>
        public static void LogException(AsyncOperationHandle op, Exception ex)
        {
            m_AddressablesInternal.LogException(op, ex);
        }

        /// <summary>
        /// Debug.LogErrorFormat wrapper method.
        /// </summary>
        /// <param name="format">The string with format tags.</param>
        /// <param name="args">The args used to fill in the format tags.</param>
        public static void LogErrorFormat(string format, params object[] args)
        {
            m_AddressablesInternal.LogErrorFormat(format, args);
        }
        public static void SetDeveloper(string relativeFolder, bool local = false)
        {
            m_AddressablesInternal.SetDeveloper(relativeFolder, local);

            //reinitializeAddressables = false;
            //m_AddressablesInstance.ReleaseSceneManagerOperation();
            //m_AddressablesInstance = new AddressablesImpl(new LRUCacheAllocationStrategy(1000, 1000, 100, 10));

        }
        /// <summary>
        /// Initialize Addressables system.  Addressables will be initialized on the first API call if this is not called explicitly.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InitializeAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> Initialize()
        {
            return InitializeAsync();
        }

        /// <summary>
        /// Initialize Addressables system.  Addressables will be initialized on the first API call if this is not called explicitly.
        /// See the [InitializeAsync](xref:addressables-api-initialize-async) documentation for more details.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> InitializeAsync()
        {
            return m_AddressablesInternal.InitializeAsync();
        }

        /// <summary>
        /// Additively load catalogs from runtime data.  The settings are not used.
        /// </summary>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadContentCatalogAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalog(string catalogPath, string providerSuffix = null)
        {
            return LoadContentCatalogAsync(catalogPath, providerSuffix);
        }

        /// <summary>
        /// Additively load catalogs from runtime data.  In order for content catalog caching to work properly the catalog json file
        /// should have a .hash file associated with the catalog.  This hash file will be used to determine if the catalog
        /// needs to be updated or not.  If no .hash file is provided, the catalog will be loaded from the specified path every time.
        /// See the [LoadContentCatalogAsync](xref:addressables-api-load-content-catalog-async) documentation for more details.
        /// </summary>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, string providerSuffix = null)
        {
            return m_AddressablesInternal.LoadContentCatalogAsync(catalogPath, false, providerSuffix);
        }

        /// <summary>
        /// Additively load catalogs from runtime data.  In order for content catalog caching to work properly the catalog json file
        /// should have a .hash file associated with the catalog.  This hash file will be used to determine if the catalog
        /// needs to be updated or not.  If no .hash file is provided, the catalog will be loaded from the specified path every time.
        /// See the [LoadContentCatalogAsync](xref:addressables-api-load-content-catalog-async) documentation for more details.
        /// </summary>
        /// <param name="catalogPath">The path to the runtime data.</param>
        /// <param name="autoReleaseHandle">If true, the async operation handle will be automatically released on completion.</param>
        /// <param name="providerSuffix">This value, if not null or empty, will be appended to all provider ids loaded from this data.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IResourceLocator> LoadContentCatalogAsync(string catalogPath, bool autoReleaseHandle, string providerSuffix = null)
        {
            return m_AddressablesInternal.LoadContentCatalogAsync(catalogPath, autoReleaseHandle, providerSuffix);
        }

        /// <summary>
        /// Initialization operation.  You can register a callback with this if you need to run code after Addressables is ready.  Any requests made before this operaton completes will automatically cahin to its result.
        /// </summary>
        [Obsolete]
        public static AsyncOperationHandle<IResourceLocator> InitializationOperation => default;

        /// <summary>
        /// Load a single asset
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="location">The location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<TObject> LoadAsset<TObject>(IResourceLocation location)
        {
            return LoadAssetAsync<TObject>(location);
        }

        /// <summary>
        /// Load a single asset
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadAssetAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<TObject> LoadAsset<TObject>(object key)
        {
            return LoadAssetAsync<TObject>(key);
        }

        /// <summary>
        /// Load a single asset
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="location">The location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        public static AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(IResourceLocation location)
        {
            return m_AddressablesInternal.LoadAssetAsync<TObject>(location);
        }

        /// <summary>
        /// Load a single asset
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the asset.</typeparam>
        /// <param name="key">The key of the location of the asset.</param>
        /// <returns>Returns the load operation.</returns>
        public static AsyncOperationHandle<TObject> LoadAssetAsync<TObject>(object key)
        {
            return m_AddressablesInternal.LoadAssetAsync<TObject>(key);
        }

        /// <summary>
        /// Loads the resource locations specified by the keys.
        /// The method will always return success, with a valid IList of results. If nothing matches keys, IList will be empty
        /// </summary>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadResourceLocationsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocations(IList<object> keys, MergeMode mode, Type type = null)
        {
            return LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Loads the resource locations specified by the keys.
        /// The method will always return success, with a valid IList of results. If nothing matches keys, IList will be empty
        /// See the [LoadResourceLocations](xref:addressables-api-load-resource-locations-async) documentation for more details.
        /// </summary>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IList<object> keys, MergeMode mode, Type type = null)
        {
            return m_AddressablesInternal.LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Loads the resource locations specified by the keys.
        /// The method will always return success, with a valid IList of results. If nothing matches keys, IList will be empty
        /// See the [LoadResourceLocations](xref:addressables-api-load-resource-locations-async) documentation for more details.
        /// </summary>
        /// <param name="keys">The set of keys to use.</param>
        /// <param name="mode">The mode for merging the results of the found locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(IEnumerable keys, MergeMode mode, Type type = null)
        {
            return m_AddressablesInternal.LoadResourceLocationsAsync(keys, mode, type);
        }

        /// <summary>
        /// Request the locations for a given key.
        /// The method will always return success, with a valid IList of results. If nothing matches key, IList will be empty
        /// </summary>
        /// <param name="key">The key for the locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadResourceLocationsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocations(object key, Type type = null)
        {
            return LoadResourceLocationsAsync(key, type);
        }

        /// <summary>
        /// Request the locations for a given key.
        /// The method will always return success, with a valid IList of results. If nothing matches key, IList will be empty
        /// See the [LoadResourceLocations](xref:addressables-api-load-resource-locations-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key for the locations.</param>
        /// <param name="type">A type restriction for the lookup.  Only locations of the provided type (or derived type) will be returned.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(object key, Type type = null)
        {
            return m_AddressablesInternal.LoadResourceLocationsAsync(key, type);
        }

        /// <summary>
        /// Load multiple assets
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(IList<IResourceLocation> locations, Action<TObject> callback)
        {
            return LoadAssetsAsync(locations, callback);
        }

        /// <summary>
        /// Load multiple assets, based on list of locations provided.
        /// If any fail, all successful loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback)
        {
            return m_AddressablesInternal.LoadAssetsAsync(locations, callback, true);
        }

        /// <summary>
        /// Load multiple assets, based on list of locations provided.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<IResourceLocation> locations, Action<TObject> callback, bool releaseDependenciesOnFailure)
        {
            return m_AddressablesInternal.LoadAssetsAsync(locations, callback, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load mutliple assets
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode)
        {
            return LoadAssetsAsync(keys, callback, mode);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// If any locations from the final list fail, all successful loads and dependencies will be released.  The returned
        /// .Result will be null, and .Status will be Failed.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode)
        {
            return m_AddressablesInternal.LoadAssetsAsync(keys, callback, mode, true);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// If any locations from the final list fail, all successful loads and dependencies will be released.  The returned
        /// .Result will be null, and .Status will be Failed.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode)
        {
            return m_AddressablesInternal.LoadAssetsAsync(keys, callback, mode, true);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IList<object> keys, Action<TObject> callback, MergeMode mode, bool releaseDependenciesOnFailure)
        {
            return m_AddressablesInternal.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load multiple assets.
        /// Each key in the provided list will be translated into a list of locations.  Those many lists will be combined
        /// down to one based on the provided MergeMode.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="keys">IEnumerable set of keys for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(IEnumerable keys, Action<TObject> callback, MergeMode mode, bool releaseDependenciesOnFailure)
        {
            return m_AddressablesInternal.LoadAssetsAsync(keys, callback, mode, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Load mutliple assets
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all async methods (UnityUpgradable) -> LoadAssetsAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<IList<TObject>> LoadAssets<TObject>(object key, Action<TObject> callback)
        {
            return LoadAssetsAsync(key, callback);
        }

        /// <summary>
        /// Load all assets that match the provided key.
        /// If any fail, all successful loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation (per loaded asset).</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback)
        {
            return m_AddressablesInternal.LoadAssetsAsync(key, callback, true);
        }

        /// <summary>
        /// Load all assets that match the provided key.
        /// See the [Loading Addressable Assets](xref:addressables-api-load-asset-async) documentation for more details.
        /// </summary>
        /// <typeparam name="TObject">The type of the assets.</typeparam>
        /// <param name="key">Key for the locations.</param>
        /// <param name="callback">Callback Action that is called per load operation (per loaded asset).</param>
        /// <param name="releaseDependenciesOnFailure">
        /// If all matching locations succeed, this parameter is ignored.
        /// When true, if any matching location fails, all loads and dependencies will be released.  The returned .Result will be null, and .Status will be Failed.
        /// When false, if any matching location fails, the returned .Result will be an IList of size equal to the number of locations attempted.  Any failed location will
        /// correlate to a null in the IList, while successful loads will correlate to a TObject in the list. The .Status will still be Failed.
        /// When true, op does not need to be released if anything fails, when false, it must always be released.
        /// </param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(object key, Action<TObject> callback, bool releaseDependenciesOnFailure)
        {
            return m_AddressablesInternal.LoadAssetsAsync(key, callback, releaseDependenciesOnFailure);
        }

        /// <summary>
        /// Release asset.
        /// </summary>
        /// <typeparam name="TObject">The type of the object being released</typeparam>
        /// <param name="obj">The asset to release.</param>
        public static void Release<TObject>(TObject obj)
        {
            m_AddressablesInternal.Release(obj);
        }

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <typeparam name="TObject">The type of the AsyncOperationHandle being released</typeparam>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release<TObject>(AsyncOperationHandle<TObject> handle)
        {
            m_AddressablesInternal.Release(handle);
        }

        /// <summary>
        /// Release the operation and its associated resources.
        /// </summary>
        /// <param name="handle">The operation handle to release.</param>
        public static void Release(AsyncOperationHandle handle)
        {
            m_AddressablesInternal.Release(handle);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="instance">The GameObject instance to be released and destroyed.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(GameObject instance)
        {
            return m_AddressablesInternal.ReleaseInstance(instance);
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(AsyncOperationHandle handle)
        {
            m_AddressablesInternal.Release(handle);
            return true;
        }

        /// <summary>
        /// Releases and destroys an object that was created via Addressables.InstantiateAsync.
        /// </summary>
        /// <param name="handle">The handle to the game object to destroy, that was returned by InstantiateAsync.</param>
        /// <returns>Returns true if the instance was successfully released.</returns>
        public static bool ReleaseInstance(AsyncOperationHandle<GameObject> handle)
        {
            m_AddressablesInternal.Release(handle);
            return true;
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> GetDownloadSizeAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<long> GetDownloadSize(object key)
        {
            return GetDownloadSizeAsync(key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(object key)
        {
            return m_AddressablesInternal.GetDownloadSizeAsync(key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="key"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="key">The key of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(string key)
        {
            return m_AddressablesInternal.GetDownloadSizeAsync((object)key);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="keys"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="keys">The keys of the asset(s) to get the download size of.</param>
        [Obsolete]
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(IList<object> keys)
        {
            return m_AddressablesInternal.GetDownloadSizeAsync(keys);
        }

        /// <summary>
        /// Determines the required download size, dependencies included, for the specified <paramref name="keys"/>.
        /// Cached assets require no download and thus their download size will be 0.  The Result of the operation
        /// is the download size in bytes.
        /// </summary>
        /// <returns>The operation handle for the request.</returns>
        /// <param name="keys">The keys of the asset(s) to get the download size of.</param>
        public static AsyncOperationHandle<long> GetDownloadSizeAsync(IEnumerable keys)
        {
            return m_AddressablesInternal.GetDownloadSizeAsync(keys);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified label or address.
        /// </summary>
        /// <param name="key">The key of the asset(s) to load dependencies for.</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> DownloadDependenciesAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle DownloadDependencies(object key)
        {
            return DownloadDependenciesAsync(key);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified label or address.
        /// See the [DownloadDependenciesAsync](xref:addressables-api-download-dependencies-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the asset(s) to load dependencies for.</param>
        /// <param name="autoReleaseHandle">Automatically releases the handle on completion</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(object key, bool autoReleaseHandle = false)
        {
            return m_AddressablesInternal.DownloadDependenciesAsync(key, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets at given locations.
        /// See the [DownloadDependenciesAsync](xref:addressables-api-download-dependencies-async) documentation for more details.
        /// </summary>
        /// <param name="locations">The locations of the assets.</param>
        /// <param name="autoReleaseHandle">Automatically releases the handle on completion</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(IList<IResourceLocation> locations, bool autoReleaseHandle = false)
        {
            return m_AddressablesInternal.DownloadDependenciesAsync(locations, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified labels or addresses.
        /// See the [DownloadDependenciesAsync](xref:addressables-api-download-dependencies-async) documentation for more details.
        /// </summary>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="autoReleaseHandle">Automatically releases the handle on completion</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        [Obsolete]
        public static AsyncOperationHandle DownloadDependenciesAsync(IList<object> keys, MergeMode mode, bool autoReleaseHandle = false)
        {
            return m_AddressablesInternal.DownloadDependenciesAsync(keys, mode, autoReleaseHandle);
        }

        /// <summary>
        /// Downloads dependencies of assets marked with the specified labels or addresses.
        /// See the [DownloadDependenciesAsync](xref:addressables-api-download-dependencies-async) documentation for more details.
        /// </summary>
        /// <param name="keys">List of keys for the locations.</param>
        /// <param name="mode">Method for merging the results of key matches.  See <see cref="MergeMode"/> for specifics</param>
        /// <param name="autoReleaseHandle">Automatically releases the handle on completion</param>
        /// <returns>The AsyncOperationHandle for the dependency load.</returns>
        public static AsyncOperationHandle DownloadDependenciesAsync(IEnumerable keys, MergeMode mode, bool autoReleaseHandle = false)
        {
            return m_AddressablesInternal.DownloadDependenciesAsync(keys, mode, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a given key.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="key">The key to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(object key)
        {
            m_AddressablesInternal.ClearDependencyCacheAsync(key, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable locations.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="locations">The locations to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(IList<IResourceLocation> locations)
        {
            m_AddressablesInternal.ClearDependencyCacheAsync(locations, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="keys">The keys to clear the cache for.</param>
        [Obsolete]
        public static void ClearDependencyCacheAsync(IList<object> keys)
        {
            m_AddressablesInternal.ClearDependencyCacheAsync(keys, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="keys">The keys to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(IEnumerable keys)
        {
            m_AddressablesInternal.ClearDependencyCacheAsync(keys, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="key">The key to clear the cache for.</param>
        public static void ClearDependencyCacheAsync(string key)
        {
            m_AddressablesInternal.ClearDependencyCacheAsync((object)key, true);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a given key.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="key">The key to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(object key, bool autoReleaseHandle)
        {
            return m_AddressablesInternal.ClearDependencyCacheAsync(key, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable locations.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="locations">The locations to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IList<IResourceLocation> locations, bool autoReleaseHandle)
        {
            return m_AddressablesInternal.ClearDependencyCacheAsync(locations, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="keys">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        [Obsolete]
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IList<object> keys, bool autoReleaseHandle)
        {
            return m_AddressablesInternal.ClearDependencyCacheAsync(keys, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="keys">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(IEnumerable keys, bool autoReleaseHandle)
        {
            return m_AddressablesInternal.ClearDependencyCacheAsync(keys, autoReleaseHandle);
        }

        /// <summary>
        /// Clear the cached AssetBundles for a list of Addressable keys.  Operation may be performed async if Addressables
        /// is initializing or updating.
        /// </summary>
        /// <param name="key">The keys to clear the cache for.</param>
        /// <param name="autoReleaseHandle">If true, the returned AsyncOperationHandle will be released on completion.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<bool> ClearDependencyCacheAsync(string key, bool autoReleaseHandle)
        {
            return m_AddressablesInternal.ClearDependencyCacheAsync((object)key, autoReleaseHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return InstantiateAsync(location, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return InstantiateAsync(location, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return InstantiateAsync(key, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return InstantiateAsync(key, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> InstantiateAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<GameObject> Instantiate(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return InstantiateAsync(location, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(location, new InstantiationParameters(parent, instantiateInWorldSpace), trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(location, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="instantiateInWorldSpace">Option to retain world space when instantiated with a parent.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, Transform parent = null, bool instantiateInWorldSpace = false, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(key, parent, instantiateInWorldSpace, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="position">The position of the instantiated object.</param>
        /// <param name="rotation">The rotation of the instantiated object.</param>
        /// <param name="parent">Parent transform for instantiated object.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, Vector3 position, Quaternion rotation, Transform parent = null, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(key, position, rotation, parent, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(object key, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(key, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Instantiate a single object. Note that the dependency loading is done asynchronously, but generally the actual instantiate is synchronous.
        /// See the [InstantiateAsync](xref:addressables-api-instantiate-async) documentation for more details.
        /// </summary>
        /// <param name="location">The location of the Object to instantiate.</param>
        /// <param name="instantiateParameters">Parameters for instantiation.</param>
        /// <param name="trackHandle">If true, Addressables will track this request to allow it to be released via the result object.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<GameObject> InstantiateAsync(IResourceLocation location, InstantiationParameters instantiateParameters, bool trackHandle = true)
        {
            return m_AddressablesInternal.InstantiateAsync(location, instantiateParameters, trackHandle);
        }

        /// <summary>
        /// Load scene.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> LoadScene(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Load scene.
        /// </summary>
        /// <param name="location">The location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> LoadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> LoadScene(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Load scene.
        /// See the [LoadSceneAsync](xref:addressables-api-load-scene-async) documentation for more details.
        /// </summary>
        /// <param name="key">The key of the location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(object key, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_AddressablesInternal.LoadSceneAsync(key, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Load scene.
        /// See the [LoadSceneAsync](xref:addressables-api-load-scene-async) documentation for more details.
        /// </summary>
        /// <param name="location">The location of the scene to load.</param>
        /// <param name="loadMode">Scene load mode.</param>
        /// <param name="activateOnLoad">If false, the scene will load but not activate (for background loading).  The SceneInstance returned has an Activate() method that can be called to do this at a later point.</param>
        /// <param name="priority">Async operation priority for scene loading.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(IResourceLocation location, LoadSceneMode loadMode = LoadSceneMode.Single, bool activateOnLoad = true, int priority = 100)
        {
            return m_AddressablesInternal.LoadSceneAsync(location, loadMode, activateOnLoad, priority);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(scene, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        //[Obsolete("We have added Async to the name of all asycn methods (UnityUpgradable) -> UnloadSceneAsync(*)", true)]
        [Obsolete]
        public static AsyncOperationHandle<SceneInstance> UnloadScene(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="scene">The SceneInstance to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(SceneInstance scene, bool autoReleaseHandle = true)
        {
            return m_AddressablesInternal.UnloadSceneAsync(scene, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle handle, bool autoReleaseHandle = true)
        {
            return m_AddressablesInternal.UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Release scene
        /// </summary>
        /// <param name="handle">The handle returned by LoadSceneAsync for the scene to release.</param>
        /// <param name="autoReleaseHandle">If true, the handle will be released automatically when complete.</param>
        /// <returns>The operation handle for the request.</returns>
        public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> handle, bool autoReleaseHandle = true)
        {
            return m_AddressablesInternal.UnloadSceneAsync(handle, autoReleaseHandle);
        }

        /// <summary>
        /// Checks all updatable content catalogs for a new version.
        /// </summary>
        /// <param name="autoReleaseHandle">If true, the handle will automatically be released when the operation completes.</param>
        /// <returns>The operation containing the list of catalog ids that have an available update.  This can be used to filter which catalogs to update with the UpdateContent.</returns>
        public static AsyncOperationHandle<List<string>> CheckForCatalogUpdates(bool autoReleaseHandle = true)
        {
            return m_AddressablesInternal.CheckForCatalogUpdates(autoReleaseHandle);
        }

        /// <summary>
        /// Update the specified catalogs.
        /// See the [UpdateCatalogs](xref:addressables-api-update-catalogs) documentation for more details.
        /// </summary>
        /// <param name="catalogs">The set of catalogs to update.  If null, all catalogs that have an available update will be updated.</param>
        /// <param name="autoReleaseHandle">If true, the handle will automatically be released when the operation completes.</param>
        /// <returns>The operation with the list of updated content catalog data.</returns>
        public static AsyncOperationHandle<List<IResourceLocator>> UpdateCatalogs(IEnumerable<string> catalogs = null, bool autoReleaseHandle = true)
        {
            return m_AddressablesInternal.UpdateCatalogs(catalogs, autoReleaseHandle);
        }

        /// <summary>
        /// Add a resource locator.
        /// </summary>
        /// <param name="locator">The locator object.</param>
        /// <param name="localCatalogHash">The hash of the local catalog. This can be null if the catalog cannot be updated.</param>
        /// <param name="remoteCatalogLocation">The location of the remote catalog. This can be null if the catalog cannot be updated.</param>
        public static void AddResourceLocator(IResourceLocator locator, string localCatalogHash = null, IResourceLocation remoteCatalogLocation = null)
        {
            m_AddressablesInternal.AddResourceLocator(locator, localCatalogHash, remoteCatalogLocation);
        }

        /// <summary>
        /// Remove a locator;
        /// </summary>
        /// <param name="locator">The locator to remove.</param>
        public static void RemoveResourceLocator(IResourceLocator locator)
        {
            m_AddressablesInternal.RemoveResourceLocator(locator);
        }

        /// <summary>
        /// Remove all locators.
        /// </summary>
        public static void ClearResourceLocators()
        {
            m_AddressablesInternal.ClearResourceLocators();
        }
    }
}