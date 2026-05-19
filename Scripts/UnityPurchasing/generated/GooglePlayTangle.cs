// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("U6LHugaYkmionbRnYHGYB79p5qN9lrRJXkCl0CdbThCggBEOk19joNCvLYG3mos8A1o8FaidbIxp0aYC9xyP79c0/3dbLXzKAZ2CvaS+lIYRap8FEq9FDkXhB72fuXHYGR4anEv5ellLdn1yUf0z/Yx2enp6fnt4N377f7C9WrkQt68F9HZtOcZQ5OLalfcIzFXzxNo3di2Uo276v0Ck263ljHmMyYwgqOeFCvXkL66HJEOvnLlB/2Fw1TDIvfBn8pCQRPg4UaODpyL+J04NRcApg0kftVq4a6Rk+aJJm7VvGOJyWzaY9oK5XDmnskD++Xp0e0v5enF5+Xp6e+8PMqLlpaacLC1Tpsf4W6cGNpQzkq2O4CLWePobXngQlaaoYnl4ent6");
        private static int[] order = new int[] { 8,13,11,4,6,8,7,10,12,12,10,12,13,13,14 };
        private static int key = 123;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
