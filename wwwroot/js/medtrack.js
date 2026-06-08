// MedTrack Jordan - Core JavaScript

$(document).ready(function() {
    // Sidebar Toggle
    $('#sidebarCollapse').on('click', function() {
        $('#sidebar').toggleClass('collapsed');
        $('#content').toggleClass('expanded');
    });

    // Initialize DataTables
    if ($.fn.DataTable) {
        $('.datatable').DataTable({
            responsive: true,
            pageLength: 25,
            language: {
                search: "Search:",
                lengthMenu: "Show _MENU_ entries"
            },
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>' +
                 '<"row"<"col-sm-12"tr>>' +
                 '<"row"<"col-sm-12 col-md-5"i><"col-sm-12 col-md-7"p>>'
        });
    }

    // Initialize Tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert-dismissible').alert('close');
    }, 5000);

    // CSV Import Preview
    $('#csvFile').on('change', function(e) {
        var file = e.target.files[0];
        if (file) {
            $('#fileName').text(file.name);
            $('#importPreview').removeClass('d-none');
        }
    });

    // Expiry Date Validation
    $('input[type="date"]').each(function() {
        var today = new Date().toISOString().split('T')[0];
        $(this).attr('min', today);
    });

    // Stock Level Indicator
    $('.stock-input').on('input', function() {
        var val = parseInt($(this).val());
        var min = parseInt($(this).data('min')) || 0;
        var indicator = $(this).closest('.form-group').find('.stock-indicator');

        if (val <= min) {
            indicator.html('<span class="text-danger"><i class="bi bi-exclamation-triangle"></i> Below minimum</span>');
        } else if (val <= min * 1.5) {
            indicator.html('<span class="text-warning"><i class="bi bi-dash-circle"></i> Running low</span>');
        } else {
            indicator.html('<span class="text-success"><i class="bi bi-check-circle"></i> Adequate</span>');
        }
    });

    // SOS Alert Confirmation
    $('.btn-sos').on('click', function(e) {
        e.preventDefault();
        var drugName = $(this).data('drug');
        mtConfirm('Send SOS Shortage Alert to MOH and nearby pharmacies for ' + drugName + '?', {
            title: 'Send SOS Alert',
            icon: 'warning',
            confirmText: 'Send Alert'
        }).then(function(ok) {
            if (ok) showToast('SOS Alert sent successfully', 'success');
        });
    });

    // Notification Mark as Read
    $(document).on('click', '.notification-item', function(e) {
        $(this).removeClass('unread').addClass('read');
        updateNotificationCount();
    });

    // Governorate Filter
    $('.gov-pill').on('click', function() {
        $(this).toggleClass('active');
        filterTableByGovernorate();
    });

    // Chart.js Defaults
    if (typeof Chart !== 'undefined') {
        applyChartThemeDefaults();
    }
});

function getAppTheme() {
    return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
}

function getChartThemeColors() {
    var isDark = getAppTheme() === 'dark';
    return {
        text: isDark ? '#9db3c9' : '#6c757d',
        grid: isDark ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.08)',
        surface: isDark ? '#111927' : '#ffffff'
    };
}

function applyChartThemeDefaults() {
    var colors = getChartThemeColors();
    Chart.defaults.font.family = "'Sora', 'Segoe UI', system-ui, sans-serif";
    Chart.defaults.color = colors.text;
}

var dashboardCharts = [];

function buildDashboardCharts() {
    dashboardCharts.forEach(function (chart) { chart.destroy(); });
    dashboardCharts = [];

    var colors = getChartThemeColors();

    var ctx1 = document.getElementById('inventoryStatusChart');
    if (ctx1) {
        dashboardCharts.push(new Chart(ctx1, {
            type: 'doughnut',
            data: {
                labels: ['Healthy', 'Expiring Soon', 'Critical', 'Expired'],
                datasets: [{
                    data: [65, 20, 10, 5],
                    backgroundColor: ['#198754', '#ffc107', '#dc3545', '#721c24'],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { color: colors.text }
                    }
                },
                cutout: '70%'
            }
        }));
    }

    var ctx2 = document.getElementById('expiryTrendChart');
    if (ctx2) {
        dashboardCharts.push(new Chart(ctx2, {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
                datasets: [{
                    label: 'Waste Value (JOD)',
                    data: [1200, 1900, 800, 1500, 1100, 900],
                    borderColor: '#dc3545',
                    backgroundColor: 'rgba(220, 53, 69, 0.1)',
                    tension: 0.4,
                    fill: true
                }, {
                    label: 'Saved via Transfers (JOD)',
                    data: [400, 600, 1200, 800, 1500, 1800],
                    borderColor: '#198754',
                    backgroundColor: 'rgba(25, 135, 84, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: { color: colors.text }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: colors.text },
                        grid: { color: colors.grid }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: colors.text },
                        grid: { color: colors.grid }
                    }
                }
            }
        }));
    }

    var ctx3 = document.getElementById('shortageChart');
    if (ctx3) {
        dashboardCharts.push(new Chart(ctx3, {
            type: 'bar',
            data: {
                labels: ['Amman', 'Irbid', 'Zarqa', 'Balqa', 'Madaba', 'Ajloun', 'Jerash', 'Mafraq', 'Karak', 'Tafilah', 'Ma\'an', 'Aqaba'],
                datasets: [{
                    label: 'Active Shortages',
                    data: [12, 8, 15, 5, 3, 2, 4, 6, 7, 2, 3, 4],
                    backgroundColor: '#dc3545'
                }, {
                    label: 'Resolved This Month',
                    data: [8, 5, 10, 3, 2, 1, 3, 4, 5, 1, 2, 3],
                    backgroundColor: '#198754'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'top',
                        labels: { color: colors.text }
                    }
                },
                scales: {
                    x: {
                        ticks: { color: colors.text },
                        grid: { color: colors.grid }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: colors.text },
                        grid: { color: colors.grid }
                    }
                }
            }
        }));
    }
}

function initDashboardCharts() {
    buildDashboardCharts();
}

window.addEventListener('mt-theme-change', function () {
    applyChartThemeDefaults();
    buildDashboardCharts();
});

// showToast, mtConfirm, mtAlert — provided by swal-helpers.js

// Initialize on page load
$(window).on('load', function() {
    initDashboardCharts();
    initShortageMap();
});

// Leaflet Map Initialization
function initShortageMap() {
    var mapContainer = document.getElementById('shortageMap');
    if (!mapContainer || typeof L === 'undefined') return;

    if (mapContainer._leafletMap) {
        mapContainer._leafletMap.remove();
        mapContainer._leafletMap = null;
    }

    var map = L.map('shortageMap').setView([31.5, 36.5], 8);
    mapContainer._leafletMap = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: 'OpenStreetMap contributors'
    }).addTo(map);

    var governorates = window.mtShortageMapData && window.mtShortageMapData.length
        ? window.mtShortageMapData
        : [
            { name: 'Amman', lat: 31.9454, lng: 35.9284, shortages: 12, severity: 'high', pharmacies: 6 },
            { name: 'Irbid', lat: 32.5568, lng: 35.8469, shortages: 8, severity: 'medium', pharmacies: 5 },
            { name: 'Zarqa', lat: 32.0602, lng: 36.0870, shortages: 15, severity: 'high', pharmacies: 8 }
        ];

    governorates.forEach(function(gov) {
        var sev = (gov.severity || 'low').toLowerCase();
        var color = sev === 'critical' || sev === 'high' ? '#dc3545'
            : sev === 'medium' ? '#ffc107' : '#198754';
        var shortages = gov.shortages || 0;
        var radius = Math.max(shortages * 500, 800);

        L.circle([gov.lat, gov.lng], {
            color: color,
            fillColor: color,
            fillOpacity: 0.35,
            radius: radius
        }).addTo(map).bindPopup(
            '<strong>' + gov.name + '</strong><br>' +
            'Active Shortages: ' + shortages + '<br>' +
            'Pharmacies Affected: ' + (gov.pharmacies || 0) + '<br>' +
            'Severity: ' + sev.toUpperCase()
        );
    });

    setTimeout(function() { map.invalidateSize(); }, 200);
}

// Filter table by governorate
function filterTableByGovernorate() {
    var activeGovs = $('.gov-pill.active').map(function() { return $(this).data('gov'); }).get();

    if (activeGovs.length === 0) {
        $('.gov-row').show();
        return;
    }

    $('.gov-row').each(function() {
        var rowGov = $(this).data('governorate');
        if (activeGovs.includes(rowGov)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

// Update notification count
function updateNotificationCount() {
    var count = $('.notification-item.unread').length;
    $('#notificationCount').text(count);
    if (count === 0) {
        $('#notificationCount').hide();
    }
}

// Bulk select all checkboxes
$('#selectAll').on('change', function() {
    $('.row-checkbox').prop('checked', $(this).prop('checked'));
});

// Confirm delete (SweetAlert2)
function confirmDelete(itemName) {
    mtConfirmDelete(itemName);
    return false;
}

$(document).on('click', '[data-mt-delete-name]', function (e) {
    e.preventDefault();
    var name = $(this).data('mt-delete-name');
    mtConfirmDelete(name).then(function (ok) {
        if (ok) showToast(name + ' deleted', 'success');
    });
});

// Export table to CSV
function exportTableToCSV(tableId, filename) {
    var csv = [];
    var rows = document.querySelectorAll('#' + tableId + ' tr');

    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        for (var j = 0; j < cols.length; j++) {
            row.push(cols[j].innerText);
        }
        csv.push(row.join(','));
    }

    var csvFile = new Blob([csv.join('\n')], { type: 'text/csv' });
    var downloadLink = document.createElement('a');
    downloadLink.download = filename;
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.click();
}
