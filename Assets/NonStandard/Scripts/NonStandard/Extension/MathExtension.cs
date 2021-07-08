namespace NonStandard.Extension {
    public static class MathExtension {
        public static float Clamp01(this float f) {
            return f < 0 ? 0 : f > 1 ? 1 : f;
		}
        public static float Clamp0(this float f, float max) {
            return f < 0 ? 0 : f > max ? max : f;
        }
        public static float Clamp(this float f, float min, float max) {
            return f < min ? min : f > max ? max : f;
        }
    }
}