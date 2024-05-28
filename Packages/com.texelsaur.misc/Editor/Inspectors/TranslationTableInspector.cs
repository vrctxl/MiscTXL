﻿using UnityEngine;

using UnityEditor;
using UdonSharpEditor;

namespace Texel
{
    [CustomEditor(typeof(TranslationTable))]
    public class TranslationTableInspector : Editor
    {
        static bool _showLangFoldout;
        static bool _showStringFoldout;

        SerializedProperty languagesProperty;
        SerializedProperty languageCodesProperty;
        SerializedProperty languagesJsonProperty;
        SerializedProperty keysProperty;
        SerializedProperty valuesProperty;

        private void OnEnable()
        {
            languagesProperty = serializedObject.FindProperty(nameof(TranslationTable.languages));
            languageCodesProperty = serializedObject.FindProperty(nameof(TranslationTable.languageCodes));
            languagesJsonProperty = serializedObject.FindProperty(nameof(TranslationTable.languageJson));
            keysProperty = serializedObject.FindProperty(nameof(TranslationTable.keys));
            valuesProperty = serializedObject.FindProperty(nameof(TranslationTable.values));
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
                return;

            LangFoldout();

            EditorGUILayout.Space();
            StringFoldout();

            serializedObject.ApplyModifiedProperties();
        }

        private void LangFoldout()
        {
            _showLangFoldout = EditorGUILayout.Foldout(_showLangFoldout, "Languages");
            if (_showLangFoldout)
            {
                int oldCount = languagesProperty.arraySize;
                int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("Size", languagesProperty.arraySize));
                if (newCount != oldCount)
                    resizeLang(newCount);

                for (int i = 0; i < newCount; i++)
                {
                    SerializedProperty langProp = languagesProperty.GetArrayElementAtIndex(i);
                    if (i >= oldCount)
                        langProp.stringValue = "";

                    SerializedProperty langCode = languageCodesProperty.GetArrayElementAtIndex(i);
                    SerializedProperty langJsonProp = languagesJsonProperty.GetArrayElementAtIndex(i);

                    EditorGUILayout.PropertyField(langProp, new GUIContent("Language"));
                    EditorGUILayout.PropertyField(langCode, new GUIContent("Language Code"));
                    EditorGUILayout.PropertyField(langJsonProp, new GUIContent("JSON"));
                    EditorGUILayout.Space();
                }
            }
        }

        private void StringFoldout()
        {
            _showStringFoldout = EditorGUILayout.Foldout(_showStringFoldout, "Strings");
            if (_showStringFoldout)
            {
                EditorGUI.indentLevel++;

                int oldCount = keysProperty.arraySize;
                int newCount = Mathf.Max(0, EditorGUILayout.DelayedIntField("Size", keysProperty.arraySize));
                if (newCount != oldCount)
                {
                    keysProperty.arraySize = newCount;
                    valuesProperty.arraySize = languagesProperty.arraySize * keysProperty.arraySize;
                }

                for (int i = 0; i < keysProperty.arraySize; i++)
                {
                    EditorGUILayout.Space();

                    SerializedProperty keyProp = keysProperty.GetArrayElementAtIndex(i);
                    if (i >= oldCount)
                        keyProp.stringValue = "";

                    EditorGUILayout.PropertyField(keyProp, new GUIContent("Translation Key"));

                    EditorGUI.indentLevel++;

                    for (int j = 0; j < languagesProperty.arraySize; j++)
                    {
                        int index = languagesProperty.arraySize * i + j;
                        SerializedProperty valProp = valuesProperty.GetArrayElementAtIndex(index);
                        SerializedProperty langProp = languagesProperty.GetArrayElementAtIndex(j);

                        string label = langProp.stringValue != "" ? langProp.stringValue : " ";
                        EditorGUILayout.PropertyField(valProp, new GUIContent(label));
                    }

                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
        }

        private void resizeLang(int newSize)
        {
            int oldSize = languagesProperty.arraySize;

            string[,] cur = new string[keysProperty.arraySize, valuesProperty.arraySize];
            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                for (int j = 0; j < languagesProperty.arraySize; j++)
                {
                    int index = languagesProperty.arraySize * i + j;
                    cur[i, j] = valuesProperty.GetArrayElementAtIndex(index).stringValue;
                }
            }

            languagesProperty.arraySize = newSize;
            languageCodesProperty.arraySize = newSize;
            languagesJsonProperty.arraySize = newSize;
            valuesProperty.arraySize = languagesProperty.arraySize * keysProperty.arraySize;

            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                for (int j = 0; j < newSize; j++)
                {
                    int index = languagesProperty.arraySize * i + j;
                    valuesProperty.GetArrayElementAtIndex(index).stringValue = j < oldSize ? cur[i, j] : "";
                }
            }
        }
    }
}
