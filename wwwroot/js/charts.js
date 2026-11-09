window.AttendanceCharts = {
    _instances: {},

    _themeColors: function () {
        var isDark = document.documentElement.getAttribute('data-theme') === 'dark';
        return {
            grid: isDark ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.15)',
            tick: isDark ? 'rgba(255,255,255,0.4)' : 'rgba(0,0,0,0.55)',
            tooltipBg: isDark ? 'rgba(15, 12, 41, 0.9)' : 'rgba(30, 30, 40, 0.95)',
            tooltipTitle: isDark ? 'rgba(255,255,255,0.7)' : 'rgba(255,255,255,0.85)',
            tooltipBorder: isDark ? 'rgba(255,255,255,0.1)' : 'rgba(0,0,0,0.1)'
        };
    },

    initLineChart: function (canvasId, labels, data) {
        var existing = this._instances[canvasId];
        if (existing) {
            existing.destroy();
        }

        var ctx = document.getElementById(canvasId);
        if (!ctx) return;

        var tc = this._themeColors();

        this._instances[canvasId] = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Attendance Rate',
                    data: data,
                    borderColor: '#6366f1',
                    backgroundColor: function (context) {
                        var chart = context.chart;
                        var ctx = chart.ctx;
                        var gradient = ctx.createLinearGradient(0, 0, 0, chart.height);
                        gradient.addColorStop(0, 'rgba(99, 102, 241, 0.25)');
                        gradient.addColorStop(1, 'rgba(99, 102, 241, 0.0)');
                        return gradient;
                    },
                    fill: true,
                    tension: 0.3,
                    pointRadius: 4,
                    pointBackgroundColor: '#6366f1',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointHoverRadius: 6,
                    borderWidth: 2.5
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tc.tooltipBg,
                        titleColor: tc.tooltipTitle,
                        bodyColor: '#fff',
                        borderColor: tc.tooltipBorder,
                        borderWidth: 1,
                        cornerRadius: 8,
                        padding: 12,
                        callbacks: {
                            label: function (context) {
                                return context.parsed.y + '%';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: {
                            display: false
                        },
                        ticks: {
                            color: tc.tick,
                            font: { size: 11 }
                        }
                    },
                    y: {
                        min: 0,
                        max: 100,
                        grid: {
                            color: tc.grid,
                            drawBorder: false
                        },
                        ticks: {
                            color: tc.tick,
                            font: { size: 11 },
                            callback: function (value) {
                                return value + '%';
                            }
                        }
                    }
                },
                interaction: {
                    intersect: false,
                    mode: 'index'
                }
            }
        });

        this._store('line', canvasId, labels, data);
    },

    initBarChart: function (canvasId, labels, data, colors) {
        var existing = this._instances[canvasId];
        if (existing) {
            existing.destroy();
        }

        var ctx = document.getElementById(canvasId);
        if (!ctx) return;

        var bgColors = colors || data.map(function () { return '#6366f1'; });
        var tc = this._themeColors();

        this._instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Attendance Rate',
                    data: data,
                    backgroundColor: bgColors,
                    borderRadius: 4,
                    borderSkipped: false,
                    barThickness: 20
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: tc.tooltipBg,
                        titleColor: tc.tooltipTitle,
                        bodyColor: '#fff',
                        borderColor: tc.tooltipBorder,
                        borderWidth: 1,
                        cornerRadius: 8,
                        padding: 12,
                        callbacks: {
                            label: function (context) {
                                return context.parsed.y + '%';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: {
                            color: tc.tick,
                            font: { size: 11 }
                        }
                    },
                    y: {
                        min: 0,
                        max: 100,
                        grid: {
                            color: tc.grid,
                            drawBorder: false
                        },
                        ticks: {
                            color: tc.tick,
                            font: { size: 11 },
                            callback: function (value) {
                                return value + '%';
                            }
                        }
                    }
                }
            }
        });

        this._store('bar', canvasId, labels, data, colors);
    },

    // Last config per canvas id, so a theme switch can redraw every live chart once.
    // Keyed by id: re-initialising a chart (filter change, refresh) replaces its entry.
    _stored: {},

    _store: function (type, id, labels, data, colors) {
        this._stored[id] = { type: type, id: id, labels: labels, data: data, colors: colors };
    },

    _rebuild: function () {
        var self = this;
        var stored = this._stored;
        this._stored = {};
        for (var key in this._instances) {
            if (this._instances[key]) {
                this._instances[key].destroy();
            }
        }
        this._instances = {};
        Object.keys(stored).forEach(function (id) {
            var cfg = stored[id];
            if (!document.getElementById(id)) return; // page was left
            if (cfg.type === 'line') {
                self.initLineChart(cfg.id, cfg.labels, cfg.data);
            } else if (cfg.type === 'bar') {
                self.initBarChart(cfg.id, cfg.labels, cfg.data, cfg.colors);
            } else if (cfg.type === 'stacked') {
                self.initStackedBarChart(cfg.id, cfg.labels, cfg.data, cfg.colors);
            }
        });
    },

    /** Destroys the charts on the given canvases (call when a page is left or a chart is hidden). */
    destroy: function (ids) {
        var self = this;
        (ids || []).forEach(function (id) {
            if (self._instances[id]) {
                self._instances[id].destroy();
                delete self._instances[id];
            }
            delete self._stored[id];
        });
    },

    /**
     * Stacked bars of counts per category, e.g. present / late / absent per section.
     * series: [{ label, data: [n...], color }]; options: { horizontal: bool, percent: bool }.
     * With percent, each bar is scaled to 100% and the tooltip shows count and share.
     */
    initStackedBarChart: function (canvasId, labels, series, options) {
        var existing = this._instances[canvasId];
        if (existing) {
            existing.destroy();
        }

        var ctx = document.getElementById(canvasId);
        if (!ctx) return;

        options = options || {};
        var tc = this._themeColors();
        var horizontal = !!options.horizontal;
        var percent = !!options.percent;

        var totals = labels.map(function (_, i) {
            return series.reduce(function (sum, s) { return sum + (s.data[i] || 0); }, 0);
        });

        var datasets = series.map(function (s) {
            return {
                label: s.label,
                data: percent
                    ? s.data.map(function (v, i) { return totals[i] ? Math.round(v / totals[i] * 1000) / 10 : 0; })
                    : s.data,
                raw: s.data,
                backgroundColor: s.color,
                borderRadius: 3,
                borderSkipped: false,
                maxBarThickness: 28
            };
        });

        var valueAxis = {
            stacked: true,
            beginAtZero: true,
            max: percent ? 100 : undefined,
            grid: { color: tc.grid, drawBorder: false },
            ticks: {
                color: tc.tick,
                font: { size: 11 },
                precision: 0,
                callback: function (value) { return percent ? value + '%' : value; }
            }
        };
        var categoryAxis = {
            stacked: true,
            grid: { display: false },
            ticks: { color: tc.tick, font: { size: 11 }, autoSkip: true, maxRotation: 0 }
        };

        this._instances[canvasId] = new Chart(ctx, {
            type: 'bar',
            data: { labels: labels, datasets: datasets },
            options: {
                indexAxis: horizontal ? 'y' : 'x',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'bottom',
                        labels: { color: tc.tick, boxWidth: 10, boxHeight: 10, usePointStyle: true, pointStyle: 'rectRounded', font: { size: 11 } }
                    },
                    tooltip: {
                        backgroundColor: tc.tooltipBg,
                        titleColor: tc.tooltipTitle,
                        bodyColor: '#fff',
                        borderColor: tc.tooltipBorder,
                        borderWidth: 1,
                        cornerRadius: 8,
                        padding: 12,
                        callbacks: {
                            label: function (context) {
                                var raw = context.dataset.raw[context.dataIndex] || 0;
                                var total = totals[context.dataIndex] || 0;
                                var share = total ? Math.round(raw / total * 1000) / 10 : 0;
                                return ' ' + context.dataset.label + ': ' + raw + ' (' + share + '%)';
                            }
                        }
                    }
                },
                scales: horizontal ? { x: valueAxis, y: categoryAxis } : { x: categoryAxis, y: valueAxis },
                interaction: { intersect: false, mode: 'index' }
            }
        });

        this._store('stacked', canvasId, labels, series, options);
    },

    resize: function () {
        for (var key in this._instances) {
            if (this._instances[key]) {
                this._instances[key].resize();
            }
        }
    }
};

(function () {
    var html = document.documentElement;
    var observer = new MutationObserver(function () {
        if (window.AttendanceCharts) {
            window.AttendanceCharts._rebuild();
        }
    });
    observer.observe(html, { attributes: true, attributeFilter: ['data-theme'] });
})();
