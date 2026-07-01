window.downloadQrPng = function (base64, name) {
    var img = new Image();
    img.onload = function () {
        var canvas = document.createElement('canvas');
        var size = 300;
        canvas.width = size;
        canvas.height = size + 40;
        var ctx = canvas.getContext('2d');

        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.drawImage(img, 10, 10, size - 20, size - 20);

        ctx.fillStyle = '#1e293b';
        ctx.font = 'bold 16px Inter, "Segoe UI", sans-serif';
        ctx.textAlign = 'center';
        ctx.fillText(name, size / 2, size + 28);

        var link = document.createElement('a');
        link.download = name.replace(/[^a-zA-Z0-9]/g, '_') + '_QR.png';
        link.href = canvas.toDataURL('image/png');
        link.click();
    };
    img.src = 'data:image/png;base64,' + base64;
};

window.triggerPrint = function () {
    window.print();
};
