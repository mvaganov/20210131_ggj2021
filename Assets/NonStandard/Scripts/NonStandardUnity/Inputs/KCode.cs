using System.Collections.Generic;

namespace NonStandard.Inputs {
	public static class KCodeExtensionUnity {
		public static bool IsDown(this KCode kCode) {return AppInput.GetKeyDown(kCode);}
		public static bool IsUp(this KCode kCode) {return AppInput.GetKeyUp(kCode);}
		public static bool IsHeld(this KCode kCode) {return AppInput.GetKey(kCode);}

		/// <summary>
		/// checks *every* possible KCode, don't put this in an inner loop. if the KCode is pressed, it is added to the given list.
		/// </summary>
		/// <param name="out_keys"></param>
		public static void GetHeld(List<KCode> out_keys) {
			for(int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsHeld(key)) { out_keys.Add(key); } }
		}
		public static void GetDown(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsDown(key)) { out_keys.Add(key); } }
		}
		public static void GetUp(List<KCode> out_keys) {
			for (int i = 0; i < (int)KCode.LAST; ++i) { KCode key = (KCode)i; if (IsUp(key)) { out_keys.Add(key); } }
		}
		public static KState GetState(this KCode kCode) {
			// prevent two-finger-right-click on touch screens, it messes with other right-click behaviour
			if(kCode == KCode.Mouse1 && UnityEngine.Input.touches != null && UnityEngine.Input.touches.Length >= 2)
			return KState.KeyReleased;
			return AppInput.GetKeyDown(kCode) ? KState.KeyDown :
			AppInput.GetKeyUp(kCode) ? KState.KeyUp : 
			AppInput.GetKey(kCode) ? KState.KeyHeld :
			KState.KeyReleased;
		}
	}
}
