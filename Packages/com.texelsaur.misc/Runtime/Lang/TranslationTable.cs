﻿using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TranslationTable : UdonSharpBehaviour
    {
        public string[] languages;
        public string[] languageCodes;
        public TextAsset[] languageJson;
        public string[] keys;
        public string[] values;

        DataDictionary[] keystores;
        DataDictionary codestore;

        bool init = false;

        private void Start()
        {
            _EnsureInit();
        }

        public void _EnsureInit()
        {
            if (init)
                return;

            init = true;
            _Init();
        }

        protected virtual void _Init()
        {
            codestore = new DataDictionary();
            keystores = new DataDictionary[languages.Length];
            for (int i = 0; i < keystores.Length; i++)
            {
                DataDictionary store = new DataDictionary();
                if (i < languageJson.Length && languageJson[i])
                    store = getLangDictionary(languageJson[i].text);

                keystores[i] = store;
                codestore.SetValue(languageCodes[i], i);

                for (int j = 0; i < keys.Length; i++)
                    _AddToDictionary(i, j);
            }
        }

        DataDictionary getLangDictionary(string text)
        {
            if (VRCJson.TryDeserializeFromJson(text, out DataToken result))
            {
                if (result.TokenType != TokenType.DataDictionary)
                    return new DataDictionary();

                return result.DataDictionary;
            }

            return new DataDictionary();
        }

        public int _GetLangBySystem()
        {
            _EnsureInit();

            string code = VRCPlayerApi.GetCurrentLanguage();
            if (codestore.TryGetValue(code, out DataToken val))
                return val.Int;

            string[] parts = code.Split('-');
            if (parts.Length > 1 && codestore.TryGetValue(parts[0], out DataToken val2))
                return val2.Int;

            return -1;
        }

        public string _GetValue(int langIndex, string key)
        {
            _EnsureInit();
            string value = "";

            DataDictionary store = keystores[langIndex];
            if (store.TryGetValue(key, out DataToken storevalue))
                value = storevalue.String;

            if (value == "" && langIndex > 0)
            {
                store = keystores[0];
                if (store.TryGetValue(key, out DataToken storevaluedefault))
                    value = storevaluedefault.String;
            }

            return value;
        }

        void _AddToDictionary(int langIndex, int keyIndex)
        {
            DataDictionary store = keystores[langIndex];
            if (keyIndex < 0 || keyIndex >= keys.Length)
                return;

            string key = keys[keyIndex];
            string value = _GetLookupValue(langIndex, keyIndex);
            store.Add(key, value);
        }

        string _GetLookupValue(int langIndex, int keyIndex)
        {
            int max = langIndex * keyIndex;
            if (max < 0 || max >= values.Length)
                return "";

            int index = keyIndex * languages.Length + langIndex;
            string lookup = values[index];

            if (lookup == "")
            {
                index = keyIndex * languages.Length;
                lookup = values[index];
            }

            return lookup;
        }
    }
}
