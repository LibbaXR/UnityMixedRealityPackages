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
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class PermissionsViewPresenter : ViewPresenter<PermissionsViewState>
    {
        private const string TemplatePath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/Permissions/Views/ZIPermissionState.uxml";
        
        private const string LOAD_PROFILE_PATH = "Load From Profile";
        private const string DELETE_PROFILE_PATH = "Delete Profile";
        private const string SET_ALL_PATH = "Set All";
        private const string PERMISSION = "PERMISSION";
        private const string LEVEL = "LEVEL";
        private const string STATE = "STATE";

        public event Action<string, List<PermissionWrapper>> OnProfileSaved;
        public event Action<string> OnProfileLoaded;
        public event Action<string> OnProfileDeleted;
        public event Action<PermissionStateWrapper, List<PermissionWrapper>> OnChangeAllPermissions;
        public event Action<string> OnManifestFileChanged;
        public event Action<string, PermissionStateWrapper> OnPermissionStateChanged;
        public event Action<bool, string> OnPermissionGranted;
        public event Action<string> OnReceivePermissionGranted;
        
        private Button manifestFilePickerButton;
        private MultiColumnListView listView;
        private Button optionsButton;
        private DropdownField pathDropdownField;

        private List<string> predeterminedNames = new() {"ALL_PERMISSIONS", "ZI_PERMISSIONS", "PROJECT_PERMISSIONS"};
        private List<string> promptList = new();
        private Dictionary<string, PermissionRequestPrompt> activePrompts = new();

        // Expresses data as a list, needed for MultiColumnListView.
        private List<PermissionWrapper> Permissions { get; set; } = new();
        private List<string> Profiles { get; set; } = new();

        public override void OnEnable(VisualElement root)
        {
            string visualTreePath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/Permissions/Views/ZIPermissionsView.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);

            visualTree.CloneTree(root);
            listView = root.Q<MultiColumnListView>();

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Permissions/Views/ZIPermissionDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Permissions/Views/ZIPermissionLightStyle.uss");

            listView.sortingEnabled = true;
        }

        public void SetEnabled(bool state)
        {
            manifestFilePickerButton.SetEnabled(state);
            listView.SetEnabled(state);
            optionsButton.SetEnabled(state);
            pathDropdownField.SetEnabled(state);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(manifestFilePickerButton, nameof(manifestFilePickerButton));
            Assert.IsNotNull(optionsButton, nameof(optionsButton));
            Assert.IsNotNull(pathDropdownField, nameof(pathDropdownField));
        }

        protected override void BindUIElements()
        {
            manifestFilePickerButton = Root.Q<Button>("manifest-button");
            optionsButton = Root.Q<Button>("options-button");
            pathDropdownField = Root.Q<DropdownField>("path-dropdown");
            pathDropdownField.choices = predeterminedNames;
        }

        protected override void RegisterUICallbacks()
        {
            manifestFilePickerButton.clicked += OnManifestFilePickerButtonClicked;
            optionsButton.clicked += OnOptionsButtonButtonClicked;
            listView.columnSortingChanged += OnColumnSortingChanged;
            pathDropdownField.RegisterValueChangedCallback(ManifestPathDropdownChanged);
        }

        protected override void UnregisterUICallbacks()
        {
            manifestFilePickerButton.clicked -= OnManifestFilePickerButtonClicked;
            optionsButton.clicked -= OnOptionsButtonButtonClicked;
            listView.columnSortingChanged -= OnColumnSortingChanged;
            pathDropdownField.UnregisterValueChangedCallback(ManifestPathDropdownChanged);
        }
        
        private void ManifestPathDropdownChanged(ChangeEvent<string> evt)
        {
            OnManifestFileChanged?.Invoke(evt.newValue);
        }
        
        private void OnOptionsButtonButtonClicked()
        {
            ShowContextMenu();
        }

        private void ShowContextMenu()
        {
            var contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Save as Profile"), false, ShowProfileSavePrompt);

            if (!Profiles.Any())
            {
                contextMenu.AddDisabledItem(new GUIContent($"{LOAD_PROFILE_PATH}/No Saved Profiles"), false);
            }
            else
            {
                foreach (var profile in Profiles)
                {
                    contextMenu.AddItem(new GUIContent($"{LOAD_PROFILE_PATH}/{profile}"), false, () =>
                    {
                        OnProfileLoaded?.Invoke(profile);
                    });
                }
            }

            if (!Profiles.Any())
            {
                contextMenu.AddDisabledItem(new GUIContent($"{DELETE_PROFILE_PATH}/No Saved Profiles"), false);
            }
            else
            {
                foreach (var profile in Profiles)
                {
                    contextMenu.AddItem(new GUIContent($"{DELETE_PROFILE_PATH}/{profile}"), false, () =>
                    {
                        OnProfileDeleted?.Invoke(profile);
                    });
                }
            }

            foreach (PermissionStateWrapper state in Enum.GetValues(typeof(PermissionStateWrapper)))
            {
                contextMenu.AddItem(new GUIContent($"{SET_ALL_PATH}/{state}"), false, () =>
                {
                    OnChangeAllPermissions?.Invoke(state, Permissions);
                });
            }

            contextMenu.ShowAsContext();
        }

        private void ShowProfileSavePrompt()
        {
            SavingPermissionProfilePrompt.ShowPrompt((result) => {
                if (!string.IsNullOrEmpty(result))
                {
                    OnProfileSaved?.Invoke(result, Permissions);
                }
            });
        }

        private void OnManifestFilePickerButtonClicked()
        {
            string savedLoggingPath = pathDropdownField.value;
            if (File.Exists(savedLoggingPath))
                savedLoggingPath = Application.dataPath + "/Plugins/Android/";

            string selectedPath = EditorUtility.OpenFilePanel("Manifest File Path", savedLoggingPath, "xml");
            if (!string.IsNullOrEmpty(selectedPath) && selectedPath != savedLoggingPath)
            {
                predeterminedNames.Add(selectedPath.Replace(@"/", @"\"));
                OnManifestFileChanged?.Invoke(selectedPath);
            }
        }

        public void OnManifestFileUpdated(string s)
        {
            pathDropdownField.SetValueWithoutNotify(s);
        }

        public void OnPermissionsUpdated(List<PermissionWrapper> permissions)
        {
            Permissions = permissions;

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TemplatePath);

            // Set MultiColumnListView.itemsSource to populate the data in the list.
            listView.itemsSource = Permissions;

            // For each column, set Column.makeCell to initialize each cell in the column.
            // You can index the columns array with names or numerical indices.
            listView.columns[PERMISSION].makeCell = () => new Label();
            listView.columns[LEVEL].makeCell = () => new Label();
            listView.columns[STATE].makeCell = () => template.Instantiate();

            // For each column, set Column.bindCell to bind an initialized cell to a data item.
            listView.columns[PERMISSION].bindCell = (VisualElement element, int index) =>
            {
                (element as Label).text = Strip(Permissions[index].Name);
                (element as Label).tooltip = Permissions[index].Name;
            };
            listView.columns[LEVEL].bindCell = (VisualElement element, int index) =>
                (element as Label).text = Permissions[index].Level.ToString();
            listView.columns[STATE].bindCell = (VisualElement element, int index) =>
            {
                EnumField enumField = element.Q<EnumField>();
                enumField.Init(Permissions[index].State);
                element.ClearClassList();
                element.AddToClassList($"permissionState__{Permissions[index].State}");
                enumField.UnregisterCallback<ChangeEvent<Enum>, int>(OnEnumFieldChangedCallback);
                enumField.RegisterCallback<ChangeEvent<Enum>, int>(OnEnumFieldChangedCallback, index);
            };
			
            Permissions.ForEach(x =>
            {
                bool granted = x.State == PermissionStateWrapper.Allowed;
                OnReceivePermissionGranted?.Invoke(x.Name);
            });
            
            listView.Rebuild();
        }

        private void OnEnumFieldChangedCallback(ChangeEvent<Enum> evt, int index)
        {
            StateChangeCallback(evt, Permissions[index].Name);
        }

        private void StateChangeCallback(ChangeEvent<Enum> evt,string name )
        {
            OnPermissionStateChanged?.Invoke(name, (PermissionStateWrapper)evt.newValue);
        }

        public void OnProfileListUpdated(List<string> list)
        {
            Profiles = list;
        }

        public void OnPermissionRequests(List<string> list)
        {
            promptList.AddRange(list);
            ShowNextPrompt();
        }

        private void ShowNextPrompt()
        {
            if (promptList?.Count != 0)
            {
                string prompt = promptList.First();
                ShowPrompt(prompt);
                promptList.Remove(prompt);
            }
        }

        private void OnColumnSortingChanged()
        {
            var description = listView.sortedColumns.FirstOrDefault();
            string sortedColumn = description.columnName;
            bool isAscending = description.direction == SortDirection.Ascending;
            Func<PermissionWrapper, string> orderFunc = sortedColumn switch
            {
                PERMISSION => x => Strip(x.Name),
                LEVEL => x => x.Level.ToString(),
                STATE => x => x.State.ToString(),
                _ => default
            };
            Permissions = isAscending ? Permissions.OrderBy(orderFunc).ToList() : Permissions.OrderByDescending(orderFunc).ToList();
            listView.Rebuild();
        }

        private string Strip(string s)
        {
            int i = s.LastIndexOf('.');
            if (i >= 0)
                s = s.Substring(i + 1);

            return s;
        }

        private void ShowPrompt(string permission)
        {
            OnReceivePermissionGranted = (perm) =>
            {
                activePrompts.Remove(perm);
                ShowNextPrompt();
            };
            activePrompts.TryAdd(permission, new PermissionRequestPrompt(permission, ref OnPermissionGranted, ref OnReceivePermissionGranted));
        }
    }
}
