// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class CombinedVector3Field
    {
        public event Action<CombinedVector3> OnValueChanged;

        private const string notValidSign = "---";

        private readonly IMGUIContainer container;

        private CombinedVector3 combinedVector = CombinedVector3.zero;

        private GUIContent labelContent = GUIContent.none;

        private string valueStringX = string.Empty;
        private string valueStringY = string.Empty;
        private string valueStringZ = string.Empty;
        private GUIContent xContent = GUIContent.none;
        private GUIContent yContent = GUIContent.none;
        private GUIContent zContent = GUIContent.none;

        public CombinedVector3Field(IMGUIContainer container, string label, CombinedVector3 combinedVector)
        {
            this.container = container;
            this.combinedVector = combinedVector;

            InitializeContents(label);
            RegisterUICallbacks();
        }

        public void SetPropertyData(PropertyData<CombinedVector3> propertyData)
        {
            container.SetEnabled(propertyData.enabled);
            combinedVector = propertyData.value;
            combinedVector.vector = combinedVector.vector.RoundToDisplay();
        }

        public void ResetField()
        {
            combinedVector = CombinedVector3.zero;
        }

        private void DrawFloatField(ref float value, ref bool isValid, ref string stringValue, GUIContent content)
        {
            if (isValid)
            {
                value = EditorGUILayout.FloatField(content, value);
            }
            else
            {
                stringValue = EditorGUILayout.TextField(content, notValidSign);
                if (float.TryParse(stringValue, out float parsedValue))
                {
                    value = parsedValue;
                    isValid = true;
                }
            }
        }

        private void InitializeContents(string label)
        {
            labelContent = new GUIContent(label);

            xContent = new GUIContent("X");
            yContent = new GUIContent("Y");
            zContent = new GUIContent("Z");
        }

        private void OnGUI()
        {
            EditorGUIUtility.fieldWidth = 50.0f;
            EditorGUIUtility.labelWidth = 10.0f;

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(labelContent);

            EditorGUILayout.BeginHorizontal();

            DrawFloatField(ref combinedVector.vector.x, ref combinedVector.isCombinedX, ref valueStringX, xContent);
            DrawFloatField(ref combinedVector.vector.y, ref combinedVector.isCombinedY, ref valueStringY, yContent);
            DrawFloatField(ref combinedVector.vector.z, ref combinedVector.isCombinedZ, ref valueStringZ, zContent);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                OnValueChanged?.Invoke(combinedVector);
            }
        }

        private void RegisterUICallbacks()
        {
            container.onGUIHandler += OnGUI;
        }
    }
}
