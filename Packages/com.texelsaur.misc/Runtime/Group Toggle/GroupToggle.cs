
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Texel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class GroupToggle : EventBase
    {
        [Header("Access Control")]
        [Tooltip("Optional.  Enables default-on states for local user only if they have access via the referenced ACL at world load.")]
        [SerializeField] internal AccessControl accessControl;
        [Tooltip("Optional.  Enables default-on states when an ACL updates to give local user access that they did not have at world load.")]
        [SerializeField] internal bool initOnAccessUpadte = true;
        [Tooltip("Optional.  Disables the ability to toggle group to 'on' state if local user does not have access via the referenced ACL.")]
        [SerializeField] internal bool enforceOnToggle = true;

        [Header("Default State")]
        [SerializeField] internal bool defaultVR = true;
        [SerializeField] internal bool defaultDesktop = true;
        [SerializeField] internal bool defaultQuest = true;

        [Header("Objects")]
        [SerializeField] internal GameObject[] onStateObjects;
        [SerializeField] internal GameObject[] offStateObjects;

        [Header("Toggle Attributes")]
        [SerializeField] internal bool toggleGameObject = true;
        [SerializeField] internal bool toggleColliders = false;
        [SerializeField] internal bool toggleRenderers = false;

        [Header("Options")]
        [SerializeField] internal bool searchChildren = false;

        private bool state = false;
        private bool inDefault = false;

        private Collider[] onColliders;
        private Collider[] offColliders;

        private MeshRenderer[] onRenderers;
        private MeshRenderer[] offRenderers;

        public const int EVENT_TOGGLED = 0;
        const int EVENT_COUNT = 1;

        private void Start()
        {
            _EnsureInit();
        }

        protected override int EventCount => EVENT_COUNT;

        protected override void _Init()
        {
            base._Init();

            if (toggleColliders)
            {
                onColliders = _BuildColliderList(onStateObjects);
                offColliders = _BuildColliderList(offStateObjects);
            }

            if (toggleRenderers)
            {
                onRenderers = _BuildRendererList(onStateObjects);
                offRenderers = _BuildRendererList(offStateObjects);
            }

            state = _DefaultState();

            if (accessControl)
            {
                if (initOnAccessUpadte)
                    accessControl._Register(AccessControl.EVENT_VALIDATE, this, nameof(_OnValidate));

                if (!accessControl._LocalHasAccess())
                    state = false;
            }

            _ToggleInternal(state);

            inDefault = true;
        }

        public void _OnValidate()
        {
            if (inDefault && accessControl._LocalHasAccess())
            {
                _ToggleInternal(_DefaultState());
                inDefault = true;
            }
        }

        bool _DefaultState()
        {
            bool defaultState = defaultDesktop;
            if (Networking.LocalPlayer.IsUserInVR())
                defaultState = defaultVR;

#if UNITY_ANDROID
            defaultState = defaultQuest;
#endif

            return defaultState;
        }

        Collider[] _BuildColliderList(GameObject[] objects)
        {
            Collider[][] colliderLists = new Collider[objects.Length][];
            int count = 0;

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];
                if (!obj)
                    continue;

                if (searchChildren)
                {
                    colliderLists[i] = obj.GetComponentsInChildren<Collider>(true);
                    count += colliderLists[i].Length;
                    continue;
                }

                Collider collider = obj.GetComponent<Collider>();
                if (collider)
                {
                    colliderLists[i] = new Collider[1];
                    colliderLists[i][0] = collider;
                    count += 1;
                }
            }

            int index = 0;
            Collider[] colliders = new Collider[count];
            for (int i = 0; i < colliderLists.Length; i++)
            {
                Collider[] list = colliderLists[i];
                if (list == null)
                    continue;

                for (int j = 0; j < list.Length; j++)
                {
                    colliders[index] = list[j];
                    index += 1;
                }
            }

            return colliders;
        }

        MeshRenderer[] _BuildRendererList(GameObject[] objects)
        {
            MeshRenderer[][] meshLists = new MeshRenderer[objects.Length][];
            int count = 0;

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject obj = objects[i];
                if (!obj)
                    continue;

                if (searchChildren)
                {
                    meshLists[i] = obj.GetComponentsInChildren<MeshRenderer>(true);
                    count += meshLists[i].Length;
                    continue;
                }

                MeshRenderer render = obj.GetComponent<MeshRenderer>();
                if (render)
                {
                    meshLists[i] = new MeshRenderer[1];
                    meshLists[i][0] = render;
                    count += 1;
                }
            }

            int index = 0;
            MeshRenderer[] renderers = new MeshRenderer[count];
            for (int i = 0; i < meshLists.Length; i++)
            {
                MeshRenderer[] list = meshLists[i];
                if (list == null)
                    continue;

                for (int j = 0; j < list.Length; j++)
                {
                    renderers[index] = list[j];
                    index += 1;
                }
            }

            return renderers;
        }

        public bool State
        {
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    _ToggleInternal(value);
                }
            }
        }

        public void _Toggle()
        {
            if (enforceOnToggle && accessControl && !accessControl._LocalHasAccess())
                return;

            State = !State;
        }

        public void _ToggleOn()
        {
            if (enforceOnToggle && accessControl && !accessControl._LocalHasAccess())
                return;

            State = true;
        }

        public void _ToggleOff()
        {
            if (enforceOnToggle && accessControl && !accessControl._LocalHasAccess())
                return;

            State = false;
        }

        void _ToggleInternal(bool val)
        {
            state = val;
            inDefault = false;

            if (toggleColliders)
            {
                foreach (var collider in onColliders)
                    collider.enabled = state;
                foreach (var collider in offColliders)
                    collider.enabled = !state;
            }

            if (toggleRenderers)
            {
                foreach (var renderer in onRenderers)
                    renderer.enabled = state;
                foreach (var renderer in offRenderers)
                    renderer.enabled = !state;
            }

            if (toggleGameObject)
            {
                if (Utilities.IsValid(onStateObjects))
                {
                    foreach (var obj in onStateObjects)
                    {
                        if (Utilities.IsValid(obj))
                            obj.SetActive(state);
                    }
                }

                if (Utilities.IsValid(offStateObjects))
                {
                    foreach (var obj in offStateObjects)
                    {
                        if (Utilities.IsValid(obj))
                            obj.SetActive(!state);
                    }
                }
            }

            _UpdateHandlers(EVENT_TOGGLED);
        }
    }
}
