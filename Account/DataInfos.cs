using System;
using UnityEngine.Purchasing;

namespace RandomFortress
{
    
    [Serializable]
    public class PurchaseInfo
    {
        public string productId;
        public string purchaseDate;
        public string expirationDate;
        public ProductType type;
    }
}