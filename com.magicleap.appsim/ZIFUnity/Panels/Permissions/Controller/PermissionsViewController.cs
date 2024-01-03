// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using ml.zi;
using UnityEditor;

namespace MagicLeap.ZI
{
    public enum PermissionStateWrapper
    {
        Pending = PermissionState.Pending,
        Allowed = PermissionState.Allowed,
        Denied = PermissionState.Denied,
    }

    internal readonly struct PermissionDetailsWrapper
    {
        public string Name { get; }
        public string Description { get; }
        public PermissionProtectionLevel Level { get; }
        
        public PermissionDetailsWrapper(string name, string description, PermissionProtectionLevel level)
        {
            Name = name;
            Description = description;
            Level = level;
        }
    }
    
    internal class PermissionWrapper
    {
        public string Name { get; }
        public string Description { get; }
        public PermissionProtectionLevel Level { get; }
        public PermissionStateWrapper State { get; }

        public PermissionWrapper(string name, PermissionStateWrapper state, string description, PermissionProtectionLevel level)
        {
            Name = name;
            State = state;
            Description = description;
            Level = level;
        }
    }

    internal class PermissionsViewController : ViewController<PermissionsViewModel, PermissionsViewPresenter>
    {
        private static readonly string windowName = "App Sim Permissions";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Permissions #F12", false, MenuItemPriority_Permission)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Permissions", isValidateFunction: false, priority: MenuItemPriority_Permission)]
#endif
        public static void ShowWindow()
        {
            GetWindow<PermissionsViewController>(windowName);
        }
        
        protected override void Initialize()
        {
            Presenter.OnManifestFileChanged += OnManifestFileChanged;
            Presenter.OnPermissionStateChanged += OnPermissionStateChanged;
            Presenter.OnProfileSaved += OnProfileSaved;
            Presenter.OnProfileLoaded += OnProfileLoaded;
            Presenter.OnProfileDeleted += OnProfileDeleted;
            Presenter.OnChangeAllPermissions += OnChangeAllPermissions;
            Presenter.OnPermissionGranted += PresenterOnOnPermissionGranted;
            Presenter.OnEnable(rootVisualElement);
            
            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;
            Model.ManifestFileUpdated += OnManifestFileUpdated;
            Model.PermissionsUpdated += OnPermissionsUpdated;
            Model.ProfilesListUpdated += ModelOnProfilesListUpdated;
            Model.PermissionRequests += ModelOnPermissionRequests;

            Presenter.SetEnabled(Model.IsSessionRunning);
            
            base.Initialize();
        }

        private void OnDisable()
        {
            Presenter.OnManifestFileChanged -= OnManifestFileChanged;
            Presenter.OnPermissionStateChanged -= OnPermissionStateChanged;
            Presenter.OnProfileSaved -= OnProfileSaved;
            Presenter.OnProfileLoaded -= OnProfileLoaded;
            Presenter.OnProfileDeleted -= OnProfileDeleted;
            Presenter.OnChangeAllPermissions -= OnChangeAllPermissions;
            Presenter.OnChangeAllPermissions -= OnChangeAllPermissions;
            Presenter.OnPermissionGranted -= PresenterOnOnPermissionGranted;
            Presenter.OnDisable();
            
            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.ManifestFileUpdated -= OnManifestFileUpdated;
            Model.PermissionsUpdated -= OnPermissionsUpdated;
            Model.ProfilesListUpdated -= ModelOnProfilesListUpdated;
            Model.PermissionRequests -= ModelOnPermissionRequests;
            Model.UnInitialize();
        }

        private void PresenterOnOnPermissionGranted(bool granted, string permission)
        {
            Model.SetPermissionState(permission, granted ? PermissionStateWrapper.Allowed : PermissionStateWrapper.Denied);
        }
        
        private void ModelOnPermissionRequests(List<string> obj)
        {
            Presenter.OnPermissionRequests(obj);
        }
        
        private void ModelOnProfilesListUpdated(List<string> obj)
        {
            Presenter.OnProfileListUpdated(obj);
        }

        private void OnChangeAllPermissions(PermissionStateWrapper state, List<PermissionWrapper> permissions)
        {
            Model.SetAllPermissions(state, permissions);
        }

        private void OnProfileDeleted(string profileName)
        {
            Model.DeleteProfile(profileName);
        }
        
        private void OnProfileLoaded(string profileName)
        {
            Model.LoadProfile(profileName);
        }
        
        private void OnProfileSaved(string profileName, List<PermissionWrapper> permissions)
        {
            Model.SaveProfile(profileName, permissions);
        }

        private void OnPermissionStateChanged(string name, PermissionStateWrapper state)
        {
            Model.SetPermissionState(name, state);
        }

        private void OnManifestFileChanged(string obj)
        {
            Model.SetManifestFile(obj);
        }

        private void OnManifestFileUpdated(string obj)
        {
            Presenter.OnManifestFileUpdated(obj);
        }      
        
        private void OnPermissionsUpdated(List<PermissionWrapper> permissions)
        {
            Presenter.OnPermissionsUpdated(permissions);
        }
        
        private void OnSessionStarted()
        {
            Presenter.SetEnabled(!Model.IsSessionRunning);
        }

        private void OnSessionStopped()
        {
            Presenter.SetEnabled(false);
        }
    }
}
