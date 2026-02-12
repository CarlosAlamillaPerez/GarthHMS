// ============================================================================
// DASHBOARD.JS - GARTH HMS
// Gestión del dashboard principal con auto-refresh y mapa de habitaciones
// ============================================================================

// ============================================================================
// VARIABLES GLOBALES
// ============================================================================

let refreshInterval = null;
const REFRESH_INTERVAL_MS = 30000; // 30 segundos

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

document.addEventListener('DOMContentLoaded', function () {
    console.log('Dashboard cargado - Inicializando...');

    // Cargar datos iniciales
    loadDashboardData();

    // Configurar auto-refresh
    startAutoRefresh();

    // Event listeners adicionales
    setupEventListeners();
});

// ============================================================================
// AUTO-REFRESH
// ============================================================================

function startAutoRefresh() {
    // Limpiar intervalo existente si hay alguno
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }

    // Configurar nuevo intervalo
    refreshInterval = setInterval(async () => {
        console.log('Auto-refresh: Actualizando dashboard...');
        await loadDashboardData(false); // false = no mostrar loading overlay
    }, REFRESH_INTERVAL_MS);

    console.log(`Auto-refresh configurado cada ${REFRESH_INTERVAL_MS / 1000} segundos`);
}

function stopAutoRefresh() {
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
        console.log('Auto-refresh detenido');
    }
}

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadDashboardData(showLoading = true) {
    try {
        if (showLoading) {
            showLoadingOverlay();
        }

        const response = await fetch('/Dashboard/GetDashboardComplete');

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}: ${response.statusText}`);
        }

        const result = await response.json();

        if (!result.success) {
            throw new Error(result.message || 'Error al cargar datos del dashboard');
        }

        // Actualizar todas las secciones
        updateKPIs(result.data.metrics);
        updateRoomsMap(result.data.roomsMap);
        updateAlerts(result.data.alerts);
        updateLastUpdateTime();

        if (showLoading) {
            hideLoadingOverlay();
        }

    } catch (error) {
        console.error('Error cargando dashboard:', error);

        if (showLoading) {
            hideLoadingOverlay();
            showErrorToast('Error al cargar el dashboard');
        }
    }
}

// ============================================================================
// ACTUALIZACIÓN DE KPIs
// ============================================================================

function updateKPIs(metrics) {
    if (!metrics) return;

    // Ocupación
    animateNumber('kpi-occupancy', metrics.occupancyPercent, '%', 0);

    // Reservas Hoy
    animateNumber('kpi-reservations', metrics.reservationsToday);

    // Check-outs
    animateNumber('kpi-checkouts', metrics.checkoutsToday);

    // Huéspedes
    animateNumber('kpi-guests', metrics.guestsActive);
}

/**
 * Anima un número desde el valor actual hasta el nuevo valor
 */
function animateNumber(elementId, targetValue, suffix = '', decimals = 0) {
    const element = document.getElementById(elementId);
    if (!element) return;

    const currentValue = parseFloat(element.textContent) || 0;
    const increment = (targetValue - currentValue) / 20; // 20 frames
    let current = currentValue;
    let frame = 0;

    const animation = setInterval(() => {
        frame++;
        current += increment;

        if (frame >= 20) {
            current = targetValue;
            clearInterval(animation);
        }

        element.textContent = current.toFixed(decimals) + suffix;
    }, 25); // 25ms entre frames = 500ms total
}

// ============================================================================
// ACTUALIZACIÓN DEL MAPA DE HABITACIONES
// ============================================================================

function updateRoomsMap(rooms) {
    if (!rooms || !Array.isArray(rooms)) {
        console.warn('No hay habitaciones para mostrar');
        return;
    }

    const container = document.getElementById('rooms-map');
    if (!container) return;

    // Limpiar contenedor
    container.innerHTML = '';

    // Ordenar por piso y número
    const sortedRooms = [...rooms].sort((a, b) => {
        if (a.floor !== b.floor) {
            return a.floor - b.floor;
        }
        return a.roomNumber.localeCompare(b.roomNumber);
    });

    // Renderizar habitaciones
    sortedRooms.forEach(room => {
        const roomCard = createRoomCard(room);
        container.appendChild(roomCard);
    });
}

function createRoomCard(room) {
    const card = document.createElement('div');
    card.className = `room-card ${room.statusCssClass}`;
    card.dataset.roomId = room.roomId;
    card.onclick = () => showRoomDetails(room.roomId);

    card.innerHTML = `
        <div class="room-number">${room.roomNumber}</div>
        <div class="room-type">${room.roomTypeCode}</div>
        <div class="room-status-text">${room.statusText}</div>
    `;

    return card;
}

// ============================================================================
// ACTUALIZACIÓN DE ALERTAS
// ============================================================================

function updateAlerts(alerts) {
    if (!alerts || !Array.isArray(alerts)) {
        console.warn('No hay alertas para mostrar');
        return;
    }

    const container = document.getElementById('alerts-list');
    if (!container) return;

    // Si no hay alertas, mostrar estado vacío
    if (alerts.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <div class="empty-state-icon">✅</div>
                <p>No hay alertas pendientes</p>
            </div>
        `;
        return;
    }

    // Limpiar y renderizar alertas (máximo 10)
    container.innerHTML = '';
    alerts.slice(0, 10).forEach(alert => {
        const alertElement = createAlertElement(alert);
        container.appendChild(alertElement);
    });
}

function createAlertElement(alert) {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert-item ${alert.alertSeverity}`;

    // Formatear hora
    const time = new Date(alert.createdAt).toLocaleTimeString('es-MX', {
        hour: '2-digit',
        minute: '2-digit'
    });

    alertDiv.innerHTML = `
        <div class="alert-icon">${alert.alertIcon}</div>
        <div class="alert-content">
            <div class="alert-title">${alert.alertTitle}</div>
            <div class="alert-message">${alert.alertMessage}</div>
            <div class="alert-time">${time}</div>
        </div>
    `;

    // Hacer clickeable si tiene entidad relacionada
    if (alert.relatedEntityId) {
        alertDiv.style.cursor = 'pointer';
        alertDiv.onclick = () => handleAlertClick(alert);
    }

    return alertDiv;
}

function handleAlertClick(alert) {
    // Navegar según el tipo de entidad
    if (alert.relatedEntityType === 'room') {
        showRoomDetails(alert.relatedEntityId);
    } else if (alert.relatedEntityType === 'stay') {
        // TODO: Implementar cuando exista el módulo de Stays
        showInfoToast('Funcionalidad disponible en FASE 3');
    } else if (alert.relatedEntityType === 'reservation') {
        // TODO: Implementar cuando exista el módulo de Reservas
        showInfoToast('Funcionalidad disponible en FASE 3');
    }
}

// ============================================================================
// MODAL DE DETALLES DE HABITACIÓN
// ============================================================================

async function showRoomDetails(roomId) {
    try {
        // Por ahora solo mostramos info básica
        // En FASE 3 se agregará info de la estancia actual

        Swal.fire({
            title: '<i class="fas fa-door-open"></i> Detalles de Habitación',
            html: `
                <div style="text-align: center; padding: 20px;">
                    <div style="font-size: 48px; margin-bottom: 16px;">🏨</div>
                    <p style="font-size: 16px; color: var(--text-secondary);">
                        Los detalles completos de la habitación estarán disponibles en FASE 3
                    </p>
                    <p style="font-size: 14px; color: var(--text-muted); margin-top: 12px;">
                        Por ahora puedes ver el estado de la habitación en el mapa
                    </p>
                </div>
            `,
            width: '500px',
            showCloseButton: true,
            showConfirmButton: false,
            customClass: {
                popup: 'swal2-popup'
            }
        });

        // TODO FASE 3: Cargar datos reales de la habitación
        // const response = await fetch(`/Rooms/GetDetails/${roomId}`);
        // const result = await response.json();
        // ... mostrar modal completo

    } catch (error) {
        console.error('Error mostrando detalles de habitación:', error);
        showErrorToast('Error al cargar detalles de la habitación');
    }
}

// ============================================================================
// UTILIDADES
// ============================================================================

function updateLastUpdateTime() {
    const element = document.getElementById('last-update-time');
    if (element) {
        const now = new Date();
        element.textContent = now.toLocaleTimeString('es-MX', {
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    }
}

function showLoadingOverlay() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.classList.add('active');
    }
}

function hideLoadingOverlay() {
    const overlay = document.getElementById('loading-overlay');
    if (overlay) {
        overlay.classList.remove('active');
    }
}

function setupEventListeners() {
    // Listener para cuando el usuario cambia de pestaña
    document.addEventListener('visibilitychange', function () {
        if (document.hidden) {
            // Usuario salió de la pestaña, detener auto-refresh
            stopAutoRefresh();
            console.log('Pestaña oculta - Auto-refresh pausado');
        } else {
            // Usuario regresó, reanudar auto-refresh
            startAutoRefresh();
            // Actualizar inmediatamente
            loadDashboardData(false);
            console.log('Pestaña visible - Auto-refresh reanudado');
        }
    });

    // Listener para errores de red
    window.addEventListener('online', function () {
        console.log('Conexión restaurada - Actualizando dashboard...');
        loadDashboardData(true);
        showSuccessToast('Conexión restaurada');
    });

    window.addEventListener('offline', function () {
        console.log('Conexión perdida');
        showWarningToast('Sin conexión a internet');
        stopAutoRefresh();
    });
}

// ============================================================================
// LIMPIEZA AL SALIR
// ============================================================================

window.addEventListener('beforeunload', function () {
    stopAutoRefresh();
});

// ============================================================================
// FUNCIONES PÚBLICAS (para uso desde la vista)
// ============================================================================

// Estas funciones están disponibles globalmente para llamarse desde onclick en el HTML
window.showRoomDetails = showRoomDetails;
window.loadDashboardData = loadDashboardData;