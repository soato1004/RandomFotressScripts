// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Je+MCCkebHLSdgcPLdIwMuzrNo9ISXSqnQXu1JZz8hZpK917idJPu1IHM67OuTyAQqIh07aamnQzk1RHccNAY3FMR0hrxwnHtkxAQEBEQULAw7QLldpGn0CuOge2ZjtXq9Mj0X2j/UYrqvzvJ65CmCe0wS/iSVRXlBhKsuD96d6TomHBnxAQa0zCnLBLOv26iJezVdyPeyVV4SyofPSk58NATkFxw0BLQ8NAQEGXRNyLWdxKjjSZn6edU2QkwxXL4Dh2D0ZwSs5WWze3WbPxKBj4S2Yii5eP87LUTD6wn0KzHwrT9moriVm0+UGqsLbyXGGM9bI4hEy6coE7TWLOhCN2mERogg4OFyUrwjSrSnvEyWsEeQpyF30eeLsBx0v9TENCQEFA");
        private static int[] order = new int[] { 5,13,3,5,5,9,12,9,13,10,11,12,12,13,14 };
        private static int key = 65;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
