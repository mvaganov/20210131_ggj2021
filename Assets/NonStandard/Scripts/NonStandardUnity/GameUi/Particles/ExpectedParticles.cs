using NonStandard.Extension;
using System.Collections.Generic;
using UnityEngine;

namespace NonStandard.GameUi.Particles {
	public class ExpectedParticles : MonoBehaviour {
		protected List<ParticleSystem> ps = new List<ParticleSystem>();
		protected ParticleSystem current;
		public void AbsorbChildParticles() {
			ParticleSystem[] kids = GetComponentsInChildren<ParticleSystem>(true);
			for (int i = 0; i < kids.Length; ++i) {
				if (kids[i] != null && ps.IndexOf(kids[i]) < 0) {
					ParticleSystem.MainModule mm = kids[i].main;
					mm.simulationSpace = ParticleSystemSimulationSpace.World;
					ps.Add(kids[i]);
				}
			}
		}
		void Start() {
			AbsorbChildParticles();
		}
		public void SetCurrent(string name) { current = Get(name); }
		public void SetCurrentPosition(Vector3 position) { current.transform.position = position; }
		public void SetCurrentPosition(Transform other) { SetCurrentPosition(other.position); }
		public void EmitCurrent(int count) { current.Emit(count); }
		public int GetId(string particleSystemName) { return ps.FindIndex(p => p.name == particleSystemName); }
		public ParticleSystem Get(string particleSystemName) {
			ParticleSystem pSys = ps.Find(p => p.name == particleSystemName);
			if (pSys == null) {
				Show.Warning(transform.HierarchyPath() + " could not find particle \"" + particleSystemName + "\", try: " +
					ps.JoinToString(", ", p => p.name) + " (" + ps.Count + ")");
			}
			return pSys;
		}
		public void Emit(int particleSystemId, Vector3 pos, int count) {
			ParticleSystem p = ps[particleSystemId];
			p.transform.position = pos;
			p.Emit(count);
		}
	}
}