window.qrCodes = {
    _printId: 'qr-cards-print-container',

    // groupsJson: [{ title: "Section A", cards: [{ name, line1, line2, qrBase64 }] }]
    printCards: function (groupsJson) {
        var groups = JSON.parse(groupsJson);
        if (!groups.length) return;

        this._cleanupPrint();

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
        container.style.fontFamily = '"Segoe UI", system-ui, sans-serif';

        for (var g = 0; g < groups.length; g++) {
            var group = groups[g];
            var section = document.createElement('div');
            if (g > 0) section.style.pageBreakBefore = 'always';

            if (group.title) {
                var heading = document.createElement('div');
                heading.style.cssText = 'font-size: 14px; font-weight: 700; color: #1e293b; padding: 0 20px 8px; border-bottom: 2px solid #1e293b; margin: 0 20px 4px;';
                heading.textContent = group.title + ' (' + group.cards.length + ')';
                section.appendChild(heading);
            }

            var grid = document.createElement('div');
            grid.style.cssText = 'display: grid; grid-template-columns: repeat(4, 1fr); gap: 16px; padding: 16px 20px;';

            for (var i = 0; i < group.cards.length; i++) {
                var c = group.cards[i];
                var card = document.createElement('div');
                card.style.cssText = 'border: 1px dashed #94a3b8; border-radius: 8px; padding: 12px; text-align: center; page-break-inside: avoid;';

                var img = document.createElement('img');
                img.src = 'data:image/png;base64,' + c.qrBase64;
                img.style.cssText = 'width: 100%; max-width: 160px; height: auto; image-rendering: pixelated; display: block; margin: 0 auto;';

                var nameDiv = document.createElement('div');
                nameDiv.style.cssText = 'font-size: 11px; font-weight: 600; color: #1e293b; margin-top: 8px; padding-top: 6px; border-top: 1px solid #f1f5f9;';
                nameDiv.textContent = c.name;

                card.appendChild(img);
                card.appendChild(nameDiv);

                if (c.line1) {
                    var l1 = document.createElement('div');
                    l1.style.cssText = 'font-size: 9px; color: #64748b; margin-top: 2px;';
                    l1.textContent = c.line1;
                    card.appendChild(l1);
                }
                if (c.line2) {
                    var l2 = document.createElement('div');
                    l2.style.cssText = 'font-size: 9px; color: #94a3b8; margin-top: 1px;';
                    l2.textContent = c.line2;
                    card.appendChild(l2);
                }

                grid.appendChild(card);
            }

            section.appendChild(grid);
            container.appendChild(section);
        }

        document.head.appendChild(style);
        document.body.appendChild(container);

        setTimeout(function () { window.print(); }, 100);
    },

    _cleanupPrint: function () {
        var container = document.getElementById(this._printId);
        if (container) container.remove();
        var style = document.getElementById(this._printId + '-style');
        if (style) style.remove();
    }
};

window.addEventListener('afterprint', function () {
    window.qrCodes._cleanupPrint();
});
