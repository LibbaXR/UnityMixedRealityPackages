// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class PermissionsViewModel : ViewModel
    {
        internal struct ManifestDeclaration
        {
            public string Name;
            public string ManifestPath;
        }

        private static ManifestDeclaration AllManifestDeclaration => new()
        {
            Name = "ALL_PERMISSIONS",
            ManifestPath = "",
        };
        
        private static ManifestDeclaration ZIManifestDeclaration => new()
        {
            Name = "ZI_PERMISSIONS",
            ManifestPath = Permissions.Handle.GetZIPermissionsManifest(),
        };
        
        private static ManifestDeclaration ProjectManifestDeclaration => new()
        {
            Name = "PROJECT_PERMISSIONS",
            ManifestPath = $"{Application.dataPath}/Plugins/Android/AndroidManifest.xml",
        };
        
        public event Action<string> ManifestFileUpdated;
        public event Action<List<PermissionWrapper>> PermissionsUpdated;
        public event Action<List<string>> ProfilesListUpdated;
        public event Action<List<string>> PermissionRequests;
        private static ZIBridge.ModuleWrapper<Permissions, PermissionsChanges> Permissions => Bridge.Permissions;
        private ZIBridge.ModuleWrapper<ConfigurationSettings, ConfigurationSettingsChanges> ConfigurationSettings => ZIBridge.Instance.ConfigurationSettings;

        private readonly List<PermissionDetailsWrapper> permissionsDetails = new();
        private List<ManifestDeclaration> predeterminedManifests = new();
        
        public override void Initialize()
        {
            Permissions.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            Permissions.OnTakeChanges += PermissionsChanged;
        }

        public override void UnInitialize()
        {
            Permissions.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            Permissions.OnTakeChanges -= PermissionsChanged;
        }

        protected override void SessionStarted()
        {
            base.SessionStarted();

            predeterminedManifests = new List<ManifestDeclaration>()
            {
                AllManifestDeclaration, ZIManifestDeclaration, ProjectManifestDeclaration,
            };
            
            UpdateLoadedManifest();
            UpdatePermissionDetails();
            UpdatePermissions();
            UpdateProfilesList();
        }

        private void UpdateLoadedManifest()
        {
            string manifest = Permissions.Handle.GetCurrentLoadedManifest();
            
            var profile = predeterminedManifests.Find(x => x.ManifestPath == manifest);
            if (!string.IsNullOrEmpty(profile.Name)) 
                manifest = profile.Name;

            ManifestFileUpdated?.Invoke(manifest);
        }

        private void UpdatePermissions()
        {
            List<PermissionWrapper> wrappers = new List<PermissionWrapper>();
            var permissionsStates = PermissionNameAndStateList.Alloc();
            Permissions.Handle.GetCurrentPermissionStates(permissionsStates);
            for (uint i = 0; i < permissionsStates.GetSize(); i++)
            {
                var state = permissionsStates.Get(i);

                if (!GetDetails(state.Name, out var details)) 
                    Debug.LogWarning($@"Permission {state.Name} doesn't exist in manifest.");

                wrappers.Add(new PermissionWrapper(state.Name, (PermissionStateWrapper) state.State , details.Description, details.Level));
            }

            PermissionsUpdated?.Invoke(wrappers);
        }

        private void UpdatePermissionDetails()
        {
            var details = PermissionPropertiesList.Alloc();
            Permissions.Handle.GetPermissionProperties(details);
            for (uint i = 0; i < details.GetSize(); i++)
            {
                var builder = details.Get(i);
                permissionsDetails.Add(new PermissionDetailsWrapper(builder.Name , builder.Description, builder.Level));
            }
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ConfigurationSettings.IsHandleConnected && Permissions.IsHandleConnected;
        }
        
        private void PermissionsChanged(PermissionsChanges changes)
        {
            if (changes.HasFlag(PermissionsChanges.SessionConnected))
            {
                UpdateLoadedManifest();
                UpdatePermissionDetails();
                UpdatePermissions();
            }

            if (changes.HasFlag(PermissionsChanges.LoadedManifest))
            {
                UpdateLoadedManifest();
                UpdatePermissions();
            }
            
            if (changes.HasFlag(PermissionsChanges.State))
            {
                UpdatePermissions();
            }
            
            if (changes.HasFlag(PermissionsChanges.AppRequests))
            {
                RequestPermissions();
            }
            
            if (changes.HasFlag(PermissionsChanges.ProfilesList))
            {
                UpdateProfilesList();
            }
        }

        private void RequestPermissions()
        {
            StringList sl = StringList.Alloc();
            Permissions.Handle.TakeAppRequests(sl);
            
            List<string> list = new List<string>();
            for (uint i = 0; i < sl.GetSize(); i++) 
                list.Add(sl.Get(i));

            PermissionRequests?.Invoke(list);
        }

        private void UpdateProfilesList()
        {
            StringList profiles = StringList.Alloc();
            Permissions.Handle.GetProfileNames(profiles);
            List<string> list = new List<string>();
            for (uint i = 0; i < profiles.GetSize(); i++) 
                list.Add(profiles.Get(i));

            ProfilesListUpdated?.Invoke(list);
        }

        private bool GetDetails(string name, out PermissionDetailsWrapper permission)
        {
            permission = permissionsDetails.FirstOrDefault(x => x.Name == name);
            return !string.IsNullOrEmpty(permission.Name);
        }

        public void SetManifestFile(string path)
        {
            var index = predeterminedManifests.FindIndex(x => x.Name == path);
            if (index != -1) 
                path = predeterminedManifests[index].ManifestPath;

            LoadManifestFile(path);
        }

        private void LoadManifestFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                Permissions.Handle.UnloadManifestPermissions();
            else
                Permissions.Handle.LoadManifestPermissions(path, StringList.Alloc());
        }

        public void SetPermissionState(string name, PermissionStateWrapper state)
        {
            Debug.Log($"Permission {name} state changing to {(PermissionState)state}");
            Permissions.Handle.SetPermissionState(name, (PermissionState) state);
        }

        public void SaveProfile(string profileName, List<PermissionWrapper> permissions)
        {
            var permissionNameAndStateList = PermissionNameAndStateList.Alloc();

            for (int i = 0; i < permissions.Count; i++)
            {
                PermissionWrapper permission = permissions[i];

                var builder = PermissionNameAndStateBuilder.Alloc();
                builder.Name = permission.Name;
                builder.State = (PermissionState) permission.State;

                permissionNameAndStateList.Append(builder);
            }

            Permissions.Handle.SaveProfile(profileName , permissionNameAndStateList);
        }

        public void LoadProfile(string profileName)
        {
            Permissions.Handle.LoadProfile(profileName);
        }

        public void DeleteProfile(string profileName)
        {
            Permissions.Handle.RemoveProfile(profileName);
        }

        public void SetAllPermissions(PermissionStateWrapper state , List<PermissionWrapper> permissions)
        {
            for (int i = 0; i < permissions.Count; i++) 
                Permissions.Handle.SetPermissionState(permissions[i].Name, (PermissionState) state);
        }
    }
}