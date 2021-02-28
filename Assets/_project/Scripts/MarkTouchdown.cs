using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MarkTouchdown : MonoBehaviour {
	public GameObject prefab_marker;
	public GameObject marker;
	public bool disableAfterFirstTouchdown = false;
	public bool copyColor = true;
	public static List<GameObject> markers = new List<GameObject>();
	public static void ClearMarkers() {
		markers.ForEach(go => Destroy(go));
		markers.Clear();
	}
	private void OnCollisionEnter(Collision collision) {
		if (!enabled) return;
		if (disableAfterFirstTouchdown) { enabled = false; }
		if (marker == null) { marker = Instantiate(prefab_marker); markers.Add(marker); }
		marker.transform.position = transform.position + prefab_marker.transform.position;
		if (copyColor) {
			Renderer selfr = GetComponent<Renderer>();
			SpriteRenderer sr = marker.GetComponent<SpriteRenderer>();
			if (sr) {
				sr.color = selfr.material.color;
			} else {
				Renderer markr = marker.GetComponent<Renderer>();
				//Debug.Log(selfr.material.color);
				markr.material.color = selfr.material.color;
			}
		}
	}
}
