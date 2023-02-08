const scopes = ['payments', 'username'];
let myDotNetHelper;

function onIncompletePaymentFound(payment) {
    //alert("PiNetwork.Blazor.Sdk.js: onIncompletePaymentFound trigered");
    //alert('Id: ' + payment.identifier + ' TxId: ' + payment.transaction.txid);
    myDotNetHelper.invokeMethodAsync('CreatePaymentOnIncopletePaymentFound', payment.identifier, payment.transaction.txid);
}

(function () {
    window.PiNetworkBlazorSdk = {
        Authenticate: function (dotNetHelper, redirectUri) {
            myDotNetHelper = dotNetHelper;
            Pi.authenticate(scopes, onIncompletePaymentFound).then(function (auth) {
                //alert(JSON.stringify(auth));
                dotNetHelper.invokeMethodAsync('AuthenticateOnSuccess', auth, redirectUri);
            }).catch(function (error) {
                //alert(JSON.stringify(error));
                dotNetHelper.invokeMethodAsync('AuthenticateOnError', JSON.stringify(error, redirectUri));
            });
        },
        CreatePayment: function (dotNetHelper, amountMe, memoMe, orderIdMe) {
            //alert('createPayment');
            myDotNetHelper = dotNetHelper;
            Pi.createPayment({
                amount: amountMe,
                memo: memoMe, // e.g: "Digital kitten #1234",
                metadata: { orderId: orderIdMe }, // e.g: { kittenId: 1234 }
            }, {
                onReadyForServerApproval: function (paymentId) {
                    //alert('onReadyForServerApproval paymentId ' + paymentId);
                    dotNetHelper.invokeMethodAsync('CreatePaymentOnReadyForServerApproval', paymentId);
                },
                onReadyForServerCompletion: function (paymentId, txid) {
                    //alert('onReadyForServerCompletion paymentId ' + paymentId + ' txid ' + txid);
                    dotNetHelper.invokeMethodAsync('CreatePaymentOnReadyForServerCompletion', paymentId, txid);
                },
                onCancel: function (paymentId) {
                    //alert('onCancel. paymentId ' + paymentId);
                    dotNetHelper.invokeMethodAsync('CreatePaymentOnCancel', paymentId);
                },
                onError: function (error, payment) {
                    //alert('onError. error:' + JSON.stringify(error) + ' payment id:' + payment.identifier + ' TxId:' + payment.transaction.txid);
                    if (typeof payment === 'undefined' || payment === null || payment.length === 0) {
                        dotNetHelper.invokeMethodAsync('CreatePaymentOnError', null, null);
                    }
                    else {
                        //alert('onError. payment id:' + payment.identifier + ' TxId:' + payment.transaction.txid);
                        dotNetHelper.invokeMethodAsync('CreatePaymentOnError', payment.identifier, payment.transaction.txid);
                    }
                },
            });
        },
        OpenShareDialog: function (title, message) {
            Pi.openShareDialog(title, message);
        },
        Test: function () {
            DotNet.invokeMethodAsync('PiNetwork.Blazor.Sdk', 'Test', 'Testing text from invoked')
                .then(data => {
                    alert(data);
                });

            return "Testing text from return";
        }
    };
    window.Browser = {
        IsPiNetworkBrowser: function (dotNetHelper) {
            if (navigator.userAgent.toLowerCase().includes("pibrowser")) {
                dotNetHelper.invokeMethodAsync('IsPiNetworkBrowser');
            }
        }
    };
    window.Test2 = (dotNetHelper) => {
        dotNetHelper.invokeMethodAsync('Test2', 'testas2');
    };
})();