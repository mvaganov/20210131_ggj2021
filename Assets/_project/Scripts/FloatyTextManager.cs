﻿using NonStandard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyTextManager : MonoBehaviour
{
	public FloatyText prefab_floatyText;

	public static FloatyText Create(Vector3 position, string text, Camera cam = null) {
		FloatyTextManager ftm = Global.Get<FloatyTextManager>();
		FloatyText ft = Instantiate(ftm.prefab_floatyText).GetComponent<FloatyText>();
		ft.cam = cam;
		ft.name = text;
		ft.transform.position = position;
		return ft;
	}
}
