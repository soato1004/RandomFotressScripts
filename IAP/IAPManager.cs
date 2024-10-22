using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RandomFortress;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : Singleton<IAPManager>, IDetailedStoreListener
{
    private static IStoreController storeController;
    private static IExtensionProvider storeExtensionProvider;

    // 상품 ID
    public const string removeAds = "com.just.randomfortress.removeads"; // 구글 플레이콘솔 저장된 아이디
    public const string superPass = "com.just.randomfortress.superpass";

    // 상품의 종류
    public const string productIDConsumable = "consumable"; // 소모용 
    public const string productIDNonConsumable = "nonconsumable"; // 비소모용
    public const string productIDSubscription = "subscription"; // 구독

    public void InitUnityIAP()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(superPass, ProductType.Consumable, new IDs() { { superPass, GooglePlay.Name }, });

        builder.AddProduct(removeAds, ProductType.NonConsumable, new IDs() { { removeAds, GooglePlay.Name }, });

        UnityPurchasing.Initialize(this, builder);
    }

    private bool IsInitialized()
    {
        return storeController != null && storeExtensionProvider != null;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extension)
    {
        Debug.Log("유니티 IAP 초기화 성공");
        storeController = controller;
        storeExtensionProvider = extension;
    }

    public void OnInitializeFailed(InitializationFailureReason reason, string message)
    {
        Debug.LogError($"유니티 IAP 초기화 실패 {reason.ToString()}");
    }

    public void OnInitializeFailed(InitializationFailureReason reason)
    {
        Debug.LogError($"유니티 IAP 초기화 실패 {reason.ToString()}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription reason)
    {
        Debug.LogWarning($"구매 실패 - {product.definition.id}, {reason}");
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.LogWarning($"구매 실패 - {product.definition.id}, {reason}");
    }
    
    // 구매하기 버튼 클릭시
    public void PurchaseStart(string productId)
    {
        if (!IsInitialized())
        {
            Debug.LogWarning("IAP가 초기화되지 않았습니다. 구매를 시도할 수 없습니다.");
            return;
        }

        var product = storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            Debug.Log($"구매 시도 - {product.definition.id}");
            storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning($"구매 시도 불가 - 제품이 존재하지 않거나 구매 불가능 상태입니다: {productId}");
        }
    }

    // 구매 성공시
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        HandlePurchase(args.purchasedProduct);
        return PurchaseProcessingResult.Complete;
    }

    // 인앱구매 성공 후 처리
    private async void HandlePurchase(Product product)
    {
        Debug.Log("ProcessPurchase: PASS. Product: " + product.definition.id);

        // 인앱구매 후 서버에 저장
        await Account.I.SavePurchaseToServer(product);
    }

    #region 구매복원

    public void RestorePurchases()
    {
        if (!IsInitialized())
        {
            Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            Debug.Log("RestorePurchases started ...");

            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((success, error) =>
            {
                Debug.Log($"RestorePurchases continuing: {success}. Error: {error}");
                OnTransactionsRestored(success, error);
            });
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("RestorePurchases started ...");

            var google = storeExtensionProvider.GetExtension<IGooglePlayStoreExtensions>();
            google.RestoreTransactions((success, error) =>
            {
                Debug.Log($"RestorePurchases continuing: {success}. Error: {error}");
                OnTransactionsRestored(success, error);
            });
        }
        else
        {
            Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }

    private async void OnTransactionsRestored(bool success, string error)
    {
        if (success)
        {
            Debug.Log("Transactions restored successfully");
            await ProcessRestoredPurchases();
        }
        else
        {
            Debug.LogError($"Transaction restoration failed. Error: {error}");
            // 사용자에게 오류 메시지 표시
            PopupManager.I.ShowPopup(PopupNames.CommonPopup, null, null, null, "실패",
                $"구매 항목 복원에 실패했습니다. 오류: {error}");
        }
    }

    private async Task ProcessRestoredPurchases()
    {
        foreach (var product in storeController.products.all)
        {
            if (product.hasReceipt)
            {
                Debug.Log($"Processing restored product: {product.definition.id}");
                bool isValid = await Account.I.SavePurchaseToServer(product);
                if (isValid)
                {
                    Debug.Log($"구매항목 복원 성공: {product.definition.id}");
                }
                else
                {
                    Debug.LogWarning($"Invalid restored purchase: {product.definition.id}");
                }
            }
        }

        PopupManager.I.ShowPopup(PopupNames.CommonPopup, null, null, null, "구매복원", "구매 항목 복원이 완료되었습니다.");
    }

    #endregion
}