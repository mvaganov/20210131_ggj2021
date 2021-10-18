using UnityEngine;

namespace NonStandard.GameUi.Particles {
	public class TriggeredParticle : MonoBehaviour {
		[Tooltip("Optional value. If given, this particle will be found in the ExpectedParticles list.")]
		public string expectedParticleName = "";
		public ParticleSystem _particleSystem;
		public int emitCount = 10;

		public void Start() {
			if (_particleSystem) {
				ParticleSystem.MainModule mm = _particleSystem.main;
				mm.simulationSpace = ParticleSystemSimulationSpace.World;
			}
		}

		/// <summary>
		/// cache the ExpectedParticles global, since it's expensive to find
		/// </summary>
		private ExpectedParticles expP;
		public ParticleSystem GetParticles() {
			ParticleSystem ps = null;
			if (!string.IsNullOrEmpty(expectedParticleName)) {
				if (expP == null) { expP = Global.GetComponent<ExpectedParticles>(); }
				if (expP != null) { ps = expP.Get(expectedParticleName); }
			}
			if (ps == null) { ps = _particleSystem; }
			return ps;
		}

		public static void ShootParticle(ParticleSystem particle, Vector3 src, Vector3 dest, int count = 10, ParticleSystem.EmitParams? para = null) {
			Transform t = particle.transform;
			Vector3 originalPosition = t.position;
			Quaternion originalRotation = t.rotation;
			t.position = src;
			t.LookAt(dest);
			if (para.HasValue) {
				particle.Emit(para.Value, count);
			} else {
				particle.Emit(count);
			}
			t.position = originalPosition;
			t.rotation = originalRotation;
		}
		public void ShootParticleFromHereToTarget(GameObject inventoryObject) {
			Vector3 a = transform.position, b = inventoryObject != null ? inventoryObject.transform.position : a + Vector3.up;
			ShootParticle(GetParticles(), a, b, emitCount);
		}
		public void ShootParticleFromHereToTargetWithThisColor(GameObject inventoryObject) {
			Vector3 a = transform.position, b = inventoryObject != null ? inventoryObject.transform.position : a + Vector3.up;
			Renderer r = GetComponent<Renderer>();
			if (r != null) {
				ShootParticle(GetParticles(), a, b, emitCount, new ParticleSystem.EmitParams() { startColor = r.material.color }); ;
			} else {
				ShootParticle(GetParticles(), a, b, emitCount);
			}
		}
		public void ShootParticleFromTargetToHereWithThisColor(GameObject inventoryObject) {
			Vector3 a = transform.position, b = inventoryObject != null ? inventoryObject.transform.position : a + Vector3.up;
			Renderer r = GetComponent<Renderer>();
			if (r != null) {
				ShootParticle(GetParticles(), b, a, emitCount, new ParticleSystem.EmitParams() { startColor = r.material.color }); ;
			} else {
				ShootParticle(GetParticles(), b, a, emitCount);
			}
		}
	}
}