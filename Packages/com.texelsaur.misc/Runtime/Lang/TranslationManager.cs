
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class TranslationManager : EventBase
    {
        public TranslationManager parentManager;
        public TranslationTable translationTable;
        public bool loadFromSystem = true;

        public Text[] textTargets;
        public string[] textKeys;

        public VRC_Pickup[] pickupTargets;
        public string[] pickupInteractKeys;
        public string[] pickupUseKeys;

        public GameObject[] behaviorTargets;
        public string[] behaviorInteractKeys;

        private TranslationManager rootManager;
        private int selectedLang = 0;

        public const int EVENT_LANG_CHANGED = 0;
        const int EVENT_COUNT = 1;

        void Start()
        {
            _EnsureInit();
        }

        protected override int EventCount => EVENT_COUNT;

        protected override void _Init()
        {
            rootManager = this;
            if (parentManager)
            {
                for (int i = 0; i < 10; i++)
                {
                    rootManager = rootManager.parentManager;
                    if (!rootManager.parentManager)
                        break;
                }

                translationTable = rootManager.translationTable;
                rootManager._Register(EVENT_LANG_CHANGED, this, nameof(_OnParentLangChanged));
            }

            int lang = 0;
            if (loadFromSystem)
            {
                int sysLang = translationTable._GetLangBySystem();
                if (sysLang > -1)
                    lang = sysLang;
            }

            _SelectLang(lang);
        }

        public void _OnParentLangChanged()
        {
            selectedLang = rootManager.selectedLang;
            _UpdateLang();
        }

        public int SelectedLang
        {
            get { return selectedLang; }
            set
            {
                _SelectLang(value);
            }
        }

        public void _SelectLang(int id)
        {
            if (!rootManager)
                return;

            rootManager._RootSelectLang(id);
        }

        public void _RootSelectLang(int id)
        {
            if (id < 0 || id >= translationTable.languages.Length)
                return;

            selectedLang = id;
            Debug.Log($"Selected lang {id}");

            _UpdateHandlers(EVENT_LANG_CHANGED);
            _UpdateLang();
        }

        void _UpdateLang()
        {
            _ApplyTextTranslations();
            _ApplyPickupTranslations();
            _ApplyBehaviorTranslations();
        }

        void _ApplyTextTranslations()
        {
            for (int i = 0; i < textTargets.Length; i++)
            {
                Text target = textTargets[i];
                if (!Utilities.IsValid(target))
                    continue;

                string value = translationTable._GetValue(selectedLang, textKeys[i]);
                target.text = value;
            }
        }

        void _ApplyPickupTranslations()
        {
            for (int i = 0; i < pickupTargets.Length; i++)
            {
                VRC_Pickup target = pickupTargets[i];
                if (!Utilities.IsValid(target))
                    continue;

                string value = translationTable._GetValue(selectedLang, pickupInteractKeys[i]);
                target.InteractionText = value;

                value = translationTable._GetValue(selectedLang, pickupUseKeys[i]);
                target.UseText = value;
            }
        }

        void _ApplyBehaviorTranslations()
        {
            for (int i = 0; i < behaviorTargets.Length; i++)
            {
                GameObject targetObj = behaviorTargets[i];
                if (!Utilities.IsValid(targetObj))
                    continue;

                UdonBehaviour target = (UdonBehaviour)targetObj.GetComponent(typeof(UdonBehaviour));
                if (!Utilities.IsValid(target))
                    continue;

                string value = translationTable._GetValue(selectedLang, behaviorInteractKeys[i]);
                target.InteractionText = value;
            }
        }
    }
}
