﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact3dUi : MonoBehaviour
{
    public RectTransform prefab_interactButton;
    protected List<Interact3dItem> items = new List<Interact3dItem>();
    public RectTransform uiArea;
    public Collider triggerArea;
    public Camera cam;
    public class TriggerArea : MonoBehaviour {
        public Interact3dUi ui;
        private void OnTriggerEnter(Collider other) {
            Interact3dItem item = other.GetComponentInChildren<Interact3dItem>();
			if (item && item.enabled) { ui.Add(item); }
        }
        private void OnTriggerExit(Collider other) {
            Interact3dItem item = other.GetComponentInChildren<Interact3dItem>();
            if (item && !item.alwaysOn) { ui.Remove(item); }
        }
    }
    public void EnsureUi(Interact3dItem item) {
        if (item.interactUi == null) {
            item.interactUi = Instantiate(prefab_interactButton).GetComponent<RectTransform>();
            item.interactUi.SetParent(uiArea);
            item.onInteractVisible?.Invoke();
            if (item.onInteract != null) {
                item.OnButton.AddListener(item.onInteract.Invoke);
            }
        }
        item.Text = item.interactText;
    }
    public void Add(Interact3dItem item) {
        EnsureUi(item);
        items.Add(item);
        UpdateItems();
    }
    public void Remove(Interact3dItem item) {
        if (item.interactUi != null) {
            Destroy(item.interactUi.gameObject);
        }
		if (items.Remove(item)) {
            item.onInteractHidden?.Invoke();
        }
        UpdateItems();
    }
    void Start()
    {
        TriggerArea ta = triggerArea.gameObject.AddComponent<TriggerArea>();
        ta.ui = this;
    }
    public void UpdateItems() {
        for(int i = 0; i < items.Count; ++i) {
            Interact3dItem item = items[i];
            if (item == null) { items.RemoveAt(i--); continue; }
            if (item.interactUi == null) {
                Remove(item);
                --i;
                Add(item);
                continue;
            }
            Vector3 p = cam.WorldToScreenPoint(item.transform.position + item.worldOffset);
            item.interactUi.position = p;
		}
	}
    Vector3 cPos;
    Quaternion cDir;
    // Update is called once per frame
    void Update()
    {
        Transform t = cam.transform;
        bool dirty = t.position != cPos || t.rotation != cDir;
		if (dirty) {
            UpdateItems();
            cPos = t.position;
            cDir = t.rotation;
        }
    }
}
