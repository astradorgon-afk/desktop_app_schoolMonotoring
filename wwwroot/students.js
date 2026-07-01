window.students = {
    _printId: 'qr-print-container',

    printSelected: function (studentsJson) {
        var students = JSON.parse(studentsJson);
        if (!students.length) return;

        var existing = document.getElementById(this._printId);
        if (existing) existing.remove();

        var style = document.createElement('style');
        style.id = this._printId + '-style';
        style.textContent =
            '@media print {' +
            '  body > *:not(#' + this._printId + ') { display: none !important; }' +
            '  #' + this._printId + ' { display: block !important; }' +
            '  @page { size: auto; margin: 10mm; }' +
            '}' +
            '#' + this._printId + ' { display: none; }';

        var container = document.createElement('div');
        container.id = this._printId;

        var grid = document.createElement('div');
        grid.style.cssText = 'display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; padding: 20px; font-family: "Segoe UI", system-ui, sans-serif;';

        for (var i = 0; i < students.length; i++) {
            var s = students[i];
            var card = document.createElement('div');
            card.style.cssText = 'border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; text-align: center; page-break-inside: avoid;';

            var img = document.createElement('img');
            img.src = 'data:image/png;base64,' + s.qrBase64;
            img.style.cssText = 'width: 100%; max-width: 180px; height: auto; image-rendering: pixelated; display: block; margin: 0 auto;';

            var nameDiv = document.createElement('div');
            nameDiv.style.cssText = 'font-size: 11px; font-weight: 600; color: #1e293b; margin-top: 8px; padding-top: 6px; border-top: 1px solid #f1f5f9;';
            nameDiv.textContent = s.name;

            var metaDiv = document.createElement('div');
            metaDiv.style.cssText = 'font-size: 9px; color: #94a3b8; margin-top: 2px;';
            metaDiv.textContent = s.lrn + ' | ' + s.studentNumber;

            card.appendChild(img);
            card.appendChild(nameDiv);
            card.appendChild(metaDiv);
            grid.appendChild(card);
        }

        container.appendChild(grid);
        document.head.appendChild(style);
        document.body.appendChild(container);

        setTimeout(function () { window.print(); }, 100);
    },

    downloadQrAsPng: function (studentJson) {
        var s = JSON.parse(studentJson);
        var canvas = document.createElement('canvas');
        var padding = 30;
        var nameHeight = 50;
        var qrSize = 400;
        canvas.width = qrSize + padding * 2;
        canvas.height = qrSize + padding * 2 + nameHeight;

        var ctx = canvas.getContext('2d');
        ctx.fillStyle = '#ffffff';
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        var img = new Image();
        img.onload = function () {
            ctx.drawImage(img, padding, padding, qrSize, qrSize);

            ctx.fillStyle = '#1e293b';
            ctx.font = 'bold 18px "Segoe UI", system-ui, sans-serif';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'bottom';
            ctx.fillText(s.name, canvas.width / 2, canvas.height - 14);

            ctx.fillStyle = '#64748b';
            ctx.font = '13px "Segoe UI", system-ui, sans-serif';
            ctx.textBaseline = 'top';
            ctx.fillText(s.lrn + ' | ' + s.studentNumber, canvas.width / 2, canvas.height - nameHeight + 8);

            var link = document.createElement('a');
            link.download = s.fileName + '.png';
            link.href = canvas.toDataURL('image/png');
            link.click();
        };
        img.src = 'data:image/png;base64,' + s.qrBase64;
    },

    downloadSelectedQrs: function (studentsJson) {
        var students = JSON.parse(studentsJson);
        for (var i = 0; i < students.length; i++) {
            setTimeout((function (s) {
                return function () {
                    window.students.downloadQrAsPng(JSON.stringify(s));
                };
            })(students[i]), i * 500);
        }
    },

    _cleanupPrint: function () {
        var container = document.getElementById(this._printId);
        if (container) container.remove();
        var style = document.getElementById(this._printId + '-style');
        if (style) style.remove();
    }
};

window.addEventListener('afterprint', function () {
    window.students._cleanupPrint();
});
