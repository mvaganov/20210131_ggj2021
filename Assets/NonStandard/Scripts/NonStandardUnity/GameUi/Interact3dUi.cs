using NonStandard.Process;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi {
    public class Interact3dUi : MonoBehaviour {
        public RectTransform prefab_interactButton;
        protected List<Interact3dItem> items = new List<Interact3dItem>();
        public RectTransform uiArea;
        [ContextMenuItem("blink","Blink")]
        public Collider triggerArea;
        public Camera cam;
        private static Interact3dUi _instance;
        public static Interact3dUi Instance {
            get { return _instance ? _instance : _instance = FindObjectOfType<Interact3dUi>(); }
        }
        public class TriggerArea : MonoBehaviour {
            public Interact3dUi ui;
            private void OnTriggerEnter(Collider other) {
                Interact3dItem item = other.GetComponentInChildren<Interact3dItem>();
                if (item) { ui.Add(item); }
            }
            private void OnTriggerExit(Collider other) {
                Interact3dItem item = other.GetComponentInChildren<Interact3dItem>();
                if (item && !item.internalState.alwaysOn) { ui.Remove(item); }
            }
            public void Blink() {
                ui.Clear();
                ui.triggerArea.enabled = false;
                Proc.Delay(16, () => ui.triggerArea.enabled = true);
            }
        }
        public void EnsureUi(Interact3dItem item) {
            if (item.screenUi == null) {
                item.screenUi = Instantiate(prefab_interactButton).GetComponent<RectTransform>();
                item.screenUi.SetParent(uiArea);
                item.screenUi.transform.localScale = prefab_interactButton.transform.localScale * item.internalState.size;
                item.fontSize = item.fontSize * item.internalState.fontCoefficient;
                item.onInteractVisible?.Invoke();
            }
        }
        public void Add(Interact3dItem item) {
            EnsureUi(item);
            if (!items.Contains(item)) { items.Add(item); }
            UpdateItems();
        }
        public void Clear() {
            for(int i = items.Count-1; i >= 0; --i) {
                Remove(items[i]);
			}
		}
        public void Remove(Interact3dItem item) {
            if (item.internalState.alwaysOn) return;
            if (item.screenUi != null) {
                Destroy(item.screenUi.gameObject);
            }
            if (items.Remove(item)) {
                item.onInteractHidden?.Invoke();
            }
            UpdateItems();
        }
        void Start() {
            TriggerArea ta = triggerArea.gameObject.AddComponent<TriggerArea>();
            ta.ui = this;
            if(cam == null) { cam = Camera.main; }
        }
        public bool UpdateItem(Interact3dItem item) {
            if (item.screenUi == null) {
                Remove(item);
                Add(item);
                return false;
            }
            if (item.showing) {
                item.screenUi.gameObject.SetActive(true);
                Vector3 screenP = cam.WorldToScreenPoint(item.transform.position + item.internalState.worldOffset);
                item.screenUi.position = screenP;
            } else {
                item.screenUi.gameObject.SetActive(false);
            }
            return true;
        }
        public void UpdateItems() {
            for (int i = 0; i < items.Count; ++i) {
                Interact3dItem item = items[i];
                if (item == null) { items.RemoveAt(i--); continue; }
				if (!UpdateItem(item)) { --i; }
            }
        }
        Vector3 cPos;
        Quaternion cDir;
        // Update is called once per frame
        void Update() {
            Transform t = cam.transform;
            bool dirty = t.position != cPos || t.rotation != cDir;
            if (dirty) {
                UpdateItems();
                cPos = t.position;
                cDir = t.rotation;
            }
        }
    }
}