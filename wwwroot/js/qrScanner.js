let html5QrCode = null;
let dotNetRef = null;

window.qrScanner = {
    start: function (dotnetRef, elementId) {
        dotNetRef = dotnetRef;

        if (html5QrCode) {
            html5QrCode.stop().then(() => {
                html5QrCode.clear();
                html5QrCode = null;
                this._startScanner(elementId);
            });
        } else {
            this._startScanner(elementId);
        }
    },

    _startScanner: function (elementId) {
        Html5Qrcode.getCameras().then(cameras => {
            if (!cameras || cameras.length === 0) {
                dotNetRef.invokeMethodAsync('OnScannerError', 'No camera found');
                return;
            }

            html5QrCode = new Html5Qrcode(elementId);

            html5QrCode.start(
                { facingMode: "environment" },
                { fps: 24, qrbox: { width: 280, height: 180 } },
                onScanSuccess,
                onScanFailure
            ).catch(err => {
                dotNetRef.invokeMethodAsync('OnScannerError', 'Camera access denied or not available');
            });
        }).catch(err => {
            dotNetRef.invokeMethodAsync('OnScannerError', 'Could not access camera');
        });
    },

    stop: function () {
        if (html5QrCode) {
            html5QrCode.stop().then(() => {
                html5QrCode.clear();
                html5QrCode = null;
            }).catch(() => { });
        }
        dotNetRef = null;
    }
};

function onScanSuccess(decodedText) {
    if (dotNetRef) {
        dotNetRef.invokeMethodAsync('OnQrCodeScanned', decodedText);
    }
}

function onScanFailure(err) {
    // ignore intermittent scan failures
}
