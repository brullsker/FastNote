using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Services.Store;

namespace FastNote.Misc
{
    public class Purchases
    {
        private StoreContext context = null;

        public async void PurchaseAddOn(string storeId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }
            StorePurchaseResult result = await context.RequestPurchaseAsync(storeId);

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                    Debug.WriteLine("The user has already purchased the product. Try again later.");
                    ConsumeAddOn(storeId);
                    break;

                case StorePurchaseStatus.Succeeded:
                    Debug.WriteLine("The purchase was successful.");
                    ConsumeAddOn(storeId);
                    break;

                case StorePurchaseStatus.NotPurchased:
                    Debug.WriteLine("The purchase did not complete. " +
                        "The user may have cancelled the purchase. ExtendedError: " + extendedError);
                    break;

                case StorePurchaseStatus.NetworkError:
                    Debug.WriteLine("The purchase was unsuccessful due to a network error. " +
                        "ExtendedError: " + extendedError);
                    break;

                case StorePurchaseStatus.ServerError:
                    Debug.WriteLine("The purchase was unsuccessful due to a server error. " +
                        "ExtendedError: " + extendedError);
                    break;

                default:
                    Debug.WriteLine("The purchase was unsuccessful due to an unknown error. " +
                        "ExtendedError: " + extendedError);
                    break;
            }
        }

        public async void ConsumeAddOn(string storeId)
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
                // If your app is a desktop app that uses the Desktop Bridge, you
                // may need additional code to configure the StoreContext object.
                // For more info, see https://aka.ms/storecontext-for-desktop.
            }

            // This is an example for a Store-managed consumable, where you specify the actual number
            // of units that you want to report as consumed so the Store can update the remaining
            // balance. For a developer-managed consumable where you maintain the balance, specify 1
            // to just report the add-on as fulfilled to the Store.
            uint quantity = 1;
            string addOnStoreId = storeId;
            Guid trackingId = Guid.NewGuid();
            StoreConsumableResult result = await context.ReportConsumableFulfillmentAsync(
                addOnStoreId, quantity, trackingId);

            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StoreConsumableStatus.Succeeded:
                    Debug.WriteLine("The fulfillment was successful. " +
                        $"Remaining balance: {result.BalanceRemaining}");
                    break;

                case StoreConsumableStatus.InsufficentQuantity:
                    Debug.WriteLine("The fulfillment was unsuccessful because the remaining " +
                        $"balance is insufficient. Remaining balance: {result.BalanceRemaining}");
                    break;

                case StoreConsumableStatus.NetworkError:
                    Debug.WriteLine("The fulfillment was unsuccessful due to a network error. " +
                        "ExtendedError: " + extendedError);
                    break;

                case StoreConsumableStatus.ServerError:
                    Debug.WriteLine("The fulfillment was unsuccessful due to a server error. " +
                        "ExtendedError: " + extendedError);
                    break;

                default:
                    Debug.WriteLine("The fulfillment was unsuccessful due to an unknown error. " +
                        "ExtendedError: " + extendedError);
                    break;
            }
        }
    }
}
