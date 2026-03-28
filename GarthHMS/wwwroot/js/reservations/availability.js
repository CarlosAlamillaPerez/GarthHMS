// ============================================================================
// AVAILABILITY.JS — Motor de Disponibilidad / Calendario de Reservas
// GarthHMS · Fase 3 · Componente 2
//
// Conecta la vista Index.cshtml con los endpoints de AvailabilityController.
// Replica fielmente la maqueta MÓDULO_2_V-2_1_CALENDARIO_RESERVAS.HTML
// ============================================================================

// ============================================================================
// ESTADO GLOBAL DEL MÓDULO
// ============================================================================
const AvailabilityModule = {
    // Fecha mostrada en el calendario (primer día del mes visible)
    currentYear: new Date().getFullYear(),
    currentMonth: new Date().getMonth() + 1, // 1-based

    // Día seleccionado en el calendario
    selectedDate: null, // 'yyyy-MM-dd' string

    // Tab activa: 'today' | 'upcoming' | 'bydate'
    currentTab: 'today',

    // Caché de datos del calendario (clave: 'yyyy-MM' → array de CalendarDayDto)
    calendarCache: {},

    // Reservas actualmente en pantalla (antes de filtros)
    allReservations: [],

    // Valores actuales de los filtros
    filters: {
        channel: '',
        status: ''
    },
    // Barra de Busqueda
    searchDebounceTimer: null,
    isSearchMode: false
};

// ============================================================================
// NOMBRES DE LOS MESES (ES-MX)
// ============================================================================
const MONTH_NAMES = [
    '', 'Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
    'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'
];

const WEEKDAY_NAMES = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

// ============================================================================
// INICIALIZACIÓN
// ============================================================================
$(document).ready(function () {
    // Establecer día de hoy como seleccionado al inicio
    AvailabilityModule.selectedDate = todayStr();

    // Listeners de filtros
    $('#filterChannel').on('change', function () {
        AvailabilityModule.filters.channel = this.value;
        renderReservationList(AvailabilityModule.allReservations);
    });

    $('#filterStatus').on('change', function () {
        AvailabilityModule.filters.status = this.value;
        renderReservationList(AvailabilityModule.allReservations);
    });

    // Renderizar calendario (carga datos del mes actual)
    renderCalendar();

    // Cargar pestaña inicial: Hoy
    loadTodayReservations();

    // Búsqueda global — dispara con debounce
    $('#globalSearchInput').on('input', function () {
        const q = this.value.trim();

        clearTimeout(AvailabilityModule.searchDebounceTimer);

        // Mostrar botón X siempre que haya texto
        $('#searchClearBtn').toggle(q.length > 0);

        if (q.length === 0) {
            exitSearchMode();
            return;
        }

        // Activar modo búsqueda INMEDIATO (UX más rápida)
        if (!AvailabilityModule.isSearchMode) {
            enterSearchMode(q);
        }

        // Evitar búsquedas muy cortas
        if (q.length < 2) return;

        AvailabilityModule.searchDebounceTimer = setTimeout(() => {
            searchReservations(q);
        }, 300);
    });

    // Botón X para limpiar
    $('#searchClearBtn').on('click', function () {
        $('#globalSearchInput').val('');
        exitSearchMode();
        $('#globalSearchInput').focus();
    });

    // Limpiar búsqueda al cambiar tab manualmente
    $('.tab-btn').on('click', function () {
        if (AvailabilityModule.isSearchMode) {
            $('#globalSearchInput').val('');
            exitSearchMode(false); // false = no recargar, changeTab ya lo hace
        }
    });
});

// ============================================================================
// UTILIDADES DE FECHA
// ============================================================================

/** Devuelve la fecha de hoy como 'yyyy-MM-dd' */
function todayStr() {
    return new Date().toISOString().split('T')[0];
}

/** Devuelve el último día del mes dado */
function daysInMonth(year, month) {
    return new Date(year, month, 0).getDate();
}

/** Devuelve el primer día de la semana (0=Dom) del mes dado */
function firstDayOfMonth(year, month) {
    return new Date(year, month - 1, 1).getDay();
}

/** Formatea 'yyyy-MM-dd' a 'dd Mmm' */
function formatShortDate(dateStr) {
    if (!dateStr) return '—';
    const [y, m, d] = dateStr.split('-');
    const months = ['', 'Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
        'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'];
    return `${parseInt(d)} ${months[parseInt(m)]}`;
}

/** Formatea 'yyyy-MM-dd' a texto largo en español */
function formatLongDate(dateStr) {
    if (!dateStr) return '';
    const d = new Date(dateStr + 'T00:00:00');
    return d.toLocaleDateString('es-MX', {
        weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
    });
}

/** Capitaliza primera letra */
function capitalize(str) {
    if (!str) return '';
    return str.charAt(0).toUpperCase() + str.slice(1);
}

// ============================================================================
// NAVEGACIÓN DEL CALENDARIO
// ============================================================================

function previousMonth() {
    AvailabilityModule.currentMonth--;
    if (AvailabilityModule.currentMonth < 1) {
        AvailabilityModule.currentMonth = 12;
        AvailabilityModule.currentYear--;
    }
    renderCalendar();
}

function nextMonth() {
    AvailabilityModule.currentMonth++;
    if (AvailabilityModule.currentMonth > 12) {
        AvailabilityModule.currentMonth = 1;
        AvailabilityModule.currentYear++;
    }
    renderCalendar();
}

function goToToday() {
    const now = new Date();
    AvailabilityModule.currentYear = now.getFullYear();
    AvailabilityModule.currentMonth = now.getMonth() + 1;
    AvailabilityModule.selectedDate = todayStr();
    renderCalendar();

    // Volver a tab Hoy y recargar
    changeTab('today');
}

// ============================================================================
// RENDERIZADO DEL CALENDARIO
// ============================================================================

async function renderCalendar() {
    const { currentYear, currentMonth } = AvailabilityModule;

    // Actualizar título del mes
    $('#calendarMonthTitle').text(`${MONTH_NAMES[currentMonth]} ${currentYear}`);

    // Limpiar grid
    const $grid = $('#calendarDays');
    $grid.empty();

    // Obtener datos del mes (con caché)
    const cacheKey = `${currentYear}-${String(currentMonth).padStart(2, '0')}`;
    let dayDataMap = {};

    try {
        if (!AvailabilityModule.calendarCache[cacheKey]) {
            const result = await fetchJSON(
                `/Availability/GetCalendarData?year=${currentYear}&month=${currentMonth}`
            );
            if (result.success) {
                AvailabilityModule.calendarCache[cacheKey] = result.data;
            }
        }
        // Convertir a mapa { 'yyyy-MM-dd': {...} }
        (AvailabilityModule.calendarCache[cacheKey] || []).forEach(d => {
            dayDataMap[d.date] = d;
        });
    } catch (err) {
        console.error('Error al cargar datos del calendario:', err);
    }

    // Días del mes anterior (relleno)
    const firstDay = firstDayOfMonth(currentYear, currentMonth);
    const prevMonthDays = daysInMonth(
        currentMonth === 1 ? currentYear - 1 : currentYear,
        currentMonth === 1 ? 12 : currentMonth - 1
    );
    for (let i = firstDay - 1; i >= 0; i--) {
        $grid.append(buildDayCell(prevMonthDays - i, 'other-month'));
    }

    // Días del mes actual
    const total = daysInMonth(currentYear, currentMonth);
    const today = todayStr();

    for (let day = 1; day <= total; day++) {
        const dateStr = `${currentYear}-${String(currentMonth).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
        const isToday = dateStr === today;
        const isSelected = dateStr === AvailabilityModule.selectedDate;
        const data = dayDataMap[dateStr] || null;

        $grid.append(buildDayCell(day, '', dateStr, data, isToday, isSelected));
    }

    // Días del mes siguiente (relleno para completar 42 celdas)
    const filled = $grid.children().length;
    const remaining = 42 - filled;
    for (let day = 1; day <= remaining; day++) {
        $grid.append(buildDayCell(day, 'other-month'));
    }

    // Actualizar resumen del día si ya hay uno seleccionado
    if (AvailabilityModule.selectedDate) {
        loadDaySummary(AvailabilityModule.selectedDate);
    }
}

/**
 * Construye una celda del calendario.
 * @param {number}  day        Número del día
 * @param {string}  extraClass Clases adicionales (ej: 'other-month')
 * @param {string}  dateStr    'yyyy-MM-dd' (solo días del mes actual)
 * @param {object}  data       Datos de ocupación del día (nullable)
 * @param {boolean} isToday    Si es el día de hoy
 * @param {boolean} isSelected Si es el día seleccionado
 */
function buildDayCell(day, extraClass = '', dateStr = '', data = null,
    isToday = false, isSelected = false) {
    const $cell = $('<div>').addClass('calendar-day');

    if (extraClass) $cell.addClass(extraClass);
    $cell.text(day);

    if (!extraClass.includes('other-month') && dateStr) {

        // Color-coding de ocupación
        if (data) {
            const lvl = data.occupancyLevel;
            if (lvl === 'low') $cell.addClass('occ-low');
            else if (lvl === 'medium') $cell.addClass('occ-medium');
            else if (lvl === 'high') $cell.addClass('occ-high');
            else if (lvl === 'full') $cell.addClass('occ-full');

            // Badge con número de reservas
            if (data.reservationCount > 0) {
                const $badge = $('<span>').addClass('calendar-day-badge')
                    .text(data.reservationCount);
                $cell.append($badge);
            }

            // Tooltip
            $cell.attr('title',
                `${data.occupiedRooms}/${data.totalRooms} hab. ocupadas · ` +
                `${data.reservationCount} reserva${data.reservationCount !== 1 ? 's' : ''}`
            );
        }

        if (isToday) $cell.addClass('today');
        if (isSelected) $cell.addClass('selected');

        // Click para seleccionar día
        $cell.on('click', function () { selectDate(dateStr); });
    }

    return $cell;
}

// ============================================================================
// SELECCIÓN DE DÍA EN EL CALENDARIO
// ============================================================================

function selectDate(dateStr) {
    AvailabilityModule.selectedDate = dateStr;
    renderCalendar(); // re-renderiza para reflejar selección
    loadDaySummary(dateStr);

    // Si el tab activo es "por fecha", cargar las reservas de ese día
    if (AvailabilityModule.currentTab === 'bydate') {
        loadReservationsByDate(dateStr);
    }
}

// ============================================================================
// PANEL DE RESUMEN DEL DÍA (inferior del calendario)
// ============================================================================

async function loadDaySummary(dateStr) {
    $('#daySummaryTitle').text(capitalize(formatLongDate(dateStr)));
    $('#statTotal, #statOccupied, #statAvailable, #statPercent').text('…');

    try {
        const result = await fetchJSON(`/Availability/GetDaySummary?date=${dateStr}`);
        if (result.success && result.data) {
            const d = result.data;
            $('#statTotal').text(d.totalRooms);
            $('#statOccupied').text(d.occupiedRooms);
            $('#statAvailable').text(d.availableRooms);
            $('#statPercent').text(`${Math.round(d.occupancyPercent)}%`);
        } else {
            $('#statTotal, #statOccupied, #statAvailable, #statPercent').text('—');
        }
    } catch (err) {
        console.error('Error al cargar resumen del día:', err);
        $('#statTotal, #statOccupied, #statAvailable, #statPercent').text('—');
    }
}

// ============================================================================
// CAMBIO DE TAB
// ============================================================================

function changeTab(tab) {
    AvailabilityModule.currentTab = tab;

    // Actualizar estilos de tabs
    $('.tab-btn').removeClass('active');
    $(`.tab-btn[data-tab="${tab}"]`).addClass('active');

    // Limpiar filtros al cambiar tab
    clearFilters(false); // false = no recargar (ya lo hace la función de carga)

    if (tab === 'today') loadTodayReservations();
    if (tab === 'upcoming') loadUpcomingReservations();
    if (tab === 'bydate') loadReservationsByDate(AvailabilityModule.selectedDate);
}

// ============================================================================
// CARGAR RESERVAS SEGÚN EL TAB
// ============================================================================

async function loadTodayReservations() {
    updateReservationsHeader(
        'Reservas de hoy',
        `${capitalize(formatLongDate(todayStr()))}`
    );
    showLoadingList();

    try {
        const result = await fetchJSON('/Availability/GetTodayReservations');
        if (result.success) {
            AvailabilityModule.allReservations = result.data;
            updateTabCount('today', result.data.length);
            renderReservationList(result.data);
        } else {
            showEmptyState('Sin reservas hoy', 'No hay check-ins programados para hoy.');
        }
    } catch (err) {
        console.error('Error al cargar reservas de hoy:', err);
        showEmptyState('Error al cargar', 'No se pudo conectar con el servidor.');
    }
}

async function loadUpcomingReservations() {
    updateReservationsHeader(
        'Próximas 7 días',
        'Reservas con check-in en los próximos 7 días'
    );
    showLoadingList();

    try {
        const result = await fetchJSON('/Availability/GetUpcomingReservations');
        if (result.success) {
            AvailabilityModule.allReservations = result.data;
            updateTabCount('upcoming', result.data.length);
            renderReservationList(result.data);
        } else {
            showEmptyState('Sin reservas próximas', 'No hay reservas en los próximos 7 días.');
        }
    } catch (err) {
        console.error('Error al cargar próximas reservas:', err);
        showEmptyState('Error al cargar', 'No se pudo conectar con el servidor.');
    }
}

async function loadReservationsByDate(dateStr) {
    if (!dateStr) {
        showEmptyState('Selecciona un día', 'Haz clic en un día del calendario para ver sus reservas.');
        return;
    }

    updateReservationsHeader(
        `Reservas del ${formatShortDate(dateStr)}`,
        capitalize(formatLongDate(dateStr))
    );
    showLoadingList();

    try {
        const result = await fetchJSON(`/Availability/GetReservationsByDate?date=${dateStr}`);
        if (result.success) {
            AvailabilityModule.allReservations = result.data;
            updateTabCount('bydate', result.data.length);
            renderReservationList(result.data);
        } else {
            showEmptyState(
                'Sin reservas',
                `No hay reservas activas para el ${formatLongDate(dateStr)}.`
            );
        }
    } catch (err) {
        console.error('Error al cargar reservas por fecha:', err);
        showEmptyState('Error al cargar', 'No se pudo conectar con el servidor.');
    }
}

// ============================================================================
// FILTROS
// ============================================================================

function clearFilters(reload = true) {
    $('#filterChannel').val('');
    $('#filterStatus').val('');
    AvailabilityModule.filters = { channel: '', status: '' };
    if (reload) renderReservationList(AvailabilityModule.allReservations);
}

function applyFilters(reservations) {
    const { channel, status } = AvailabilityModule.filters;
    return reservations.filter(r => {
        const matchChannel = !channel || (r.source || '').toLowerCase() === channel.toLowerCase();
        const matchStatus = !status || (r.status || '').toLowerCase() === status.toLowerCase();
        return matchChannel && matchStatus;
    });
}

// ============================================================================
// RENDERIZADO DE LA LISTA DE RESERVAS
// ============================================================================

function renderReservationList(reservations) {
    const filtered = applyFilters(reservations);

    // Actualizar subtítulo con el conteo real tras filtros
    updateReservationsSubtitle(filtered.length);

    const $list = $('#reservationsList');
    $list.empty();

    if (!filtered || filtered.length === 0) {
        $list.append(buildEmptyStateHTML(
            'fa-calendar-times',
            'Sin reservas',
            'No se encontraron reservas para los filtros seleccionados.'
        ));
        return;
    }

    // Ordenar: en validación (pendiente de anticipo) primero, luego check-in hoy, luego por fecha
    const today = todayStr();
    filtered.sort((a, b) => {
        const aUrgent = (a.status === 'pending' && !a.hasDeposit) ? 0 : 1;
        const bUrgent = (b.status === 'pending' && !b.hasDeposit) ? 0 : 1;
        if (aUrgent !== bUrgent) return aUrgent - bUrgent;

        const aToday = a.checkInDate === today ? 0 : 1;
        const bToday = b.checkInDate === today ? 0 : 1;
        if (aToday !== bToday) return aToday - bToday;

        return (a.checkInDate || '').localeCompare(b.checkInDate || '');
    });

    filtered.forEach(r => {
        $list.append(buildReservationCard(r));
    });
}

// ============================================================================
// TARJETA DE RESERVA
// ============================================================================

function buildReservationCard(r) {
    // Badge de estado
    const statusClasses = {
        'pending': 'status-pending',
        'confirmed': 'status-confirmed',
        'checked_in': 'status-checked_in',
        'checked_out': 'status-checked_out',
        'cancelled': 'status-cancelled',
        'no_show': 'status-no_show'
    };
    const statusClass = statusClasses[(r.status || '').toLowerCase()] || 'status-pending';

    // Botón primario: depende del estado y de si es check-in hoy
    const isCheckinToday = r.checkInDate === todayStr();
    let primaryBtnText = 'Ver detalles';
    let primaryBtnClass = '';

    if (r.status === 'pending' && !r.hasDeposit) {
        primaryBtnText = '<i class="fas fa-check-circle me-1"></i>Validar Anticipo';
        primaryBtnClass = 'warning';
    } else if (r.status === 'confirmed' && isCheckinToday) {
        primaryBtnText = '<i class="fas fa-key me-1"></i>Check-in';
    } else if (r.status === 'checked_in') {
        primaryBtnText = '<i class="fas fa-sign-out-alt me-1"></i>Check-out';
    }

    // Noches
    const nightsText = r.numNights
        ? `${r.numNights} noche${r.numNights !== 1 ? 's' : ''}`
        : '—';

    // Pago
    const paymentHTML = r.hasDeposit
        ? `<span class="payment-tag paid">
               <i class="fas fa-check-circle fa-xs"></i>
               Anticipo $${(r.depositAmount || 0).toLocaleString('es-MX')}
           </span>`
        : `<span class="payment-tag pending">
               <i class="fas fa-clock fa-xs"></i>
               Anticipo pendiente
           </span>`;

    // VIP badge
    const vipHTML = r.isVip
        ? `<span class="vip-badge"><i class="fas fa-star fa-xs"></i>VIP</span>`
        : '';

    return `
    <div class="reservation-card" onclick="viewReservationDetails('${r.reservationId}')">

        <!-- CABECERA: estado + folio + canal -->
        <div class="card-top-row">
            <div class="card-status-group">
                <span class="status-badge ${statusClass}">${r.statusLabel || r.status}</span>
                <span class="folio-text">${r.folio || ''}</span>
                ${vipHTML}
            </div>
            <span class="channel-badge">${r.sourceLabel || r.source || 'Directo'}</span>
        </div>

        <!-- NOMBRE DEL HUÉSPED -->
        <div class="guest-name-row">
            <i class="fas fa-user-circle text-primary" style="font-size:1rem;opacity:.7"></i>
            ${r.guestFullName || 'Huésped'}
        </div>

        <!-- DETALLES -->
        <div class="reservation-info-list">
            <div class="info-item">
                <i class="fas fa-calendar-check"></i>
                <span>
                    ${formatShortDate(r.checkInDate)} → ${formatShortDate(r.checkOutDate)}
                    &nbsp;·&nbsp;<strong>${nightsText}</strong>
                </span>
            </div>
            ${r.roomsSummary ? `
            <div class="info-item">
                <i class="fas fa-bed"></i>
                <span>${r.roomsSummary}</span>
            </div>` : ''}
            <div class="info-item">
                <i class="fas fa-dollar-sign"></i>
                <span>$${(r.total || 0).toLocaleString('es-MX')} MXN</span>
            </div>
        </div>

        <!-- ESTADO DE PAGO -->
        ${paymentHTML}

        <!-- ACCIONES -->
        <div class="card-actions">
            <button class="btn-action-main ${primaryBtnClass}"
                    onclick="event.stopPropagation(); handlePrimaryAction('${r.reservationId}', '${r.status}', ${r.isCheckInToday})">
                ${primaryBtnText}
            </button>
            <button class="btn-action-icon"
                    onclick="event.stopPropagation(); callGuest('${r.guestPhone || ''}', '${r.guestFullName || ''}')"
                    title="Llamar huésped">
                <i class="fas fa-phone"></i>
            </button>
            <div class="btn-group">
                <button type="button"
                        class="btn-action-icon dropdown-toggle-no-caret"
                        data-bs-toggle="dropdown"
                        data-bs-boundary="viewport"
                        onclick="event.stopPropagation()"
                        title="Más opciones">
                    <i class="fas fa-ellipsis-v"></i>
                </button>
                <ul class="dropdown-menu dropdown-menu-end">
                    <li>
                        <a class="dropdown-item" href="javascript:void(0)"
                           onclick="viewReservationDetails('${r.reservationId}')">
                            <i class="fas fa-eye text-info me-2"></i>Ver detalle completo
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item" href="javascript:void(0)"
                           onclick="editReservation('${r.reservationId}')">
                            <i class="fas fa-edit text-warning me-2"></i>Editar reserva
                        </a>
                    </li>
                    <li>
                        <a class="dropdown-item" href="javascript:void(0)"
                           onclick="copyFolio('${r.folio}')">
                            <i class="fas fa-copy text-secondary me-2"></i>Copiar folio
                        </a>
                    </li>
                    <li><hr class="dropdown-divider"></li>
                    <li>
                        <a class="dropdown-item text-danger" href="javascript:void(0)"
                           onclick="cancelReservation('${r.reservationId}', '${r.folio}', '${r.guestFullName}')">
                            <i class="fas fa-times-circle me-2"></i>Cancelar reserva
                        </a>
                    </li>
                </ul>
            </div>
        </div>
    </div>`;
}

// ============================================================================
// ACCIONES DE LAS TARJETAS
// ============================================================================

/** Ver detalle completo de la reserva */
async function viewReservationDetails(reservationId) {
    try {
        const html = await fetchHTML(`/Availability/GetReservationDetails/${reservationId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-clipboard-list text-primary me-2"></i>Detalle de Reserva',
            html: html,
            width: '600px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (err) {
        console.error('Error al abrir detalle de reserva:', err);
        showErrorToast('No se pudo cargar el detalle de la reserva');
    }
}

/** Acción principal dinámica según el estado de la reserva */
function handlePrimaryAction(reservationId, status, isCheckInToday) {
    const stat = (status || '').toLowerCase();

    if (stat === 'pending') {
        // Placeholder — se completará en Componente 5 (Depósitos)
        Swal.fire({
            title: '<i class="fas fa-check-circle text-warning me-2"></i>Validar Anticipo',
            html: `<p>La validación de anticipos estará disponible en el módulo de <strong>Depósitos</strong> (Componente 5).</p>`,
            icon: 'info',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#2BA49A'
        });

    } else if (stat === 'confirmed' && isCheckInToday) {
        // Placeholder — se completará en Módulo 3 (Check-in)
        Swal.fire({
            title: '<i class="fas fa-key text-primary me-2"></i>Check-in',
            html: `<p>El proceso de check-in estará disponible en el <strong>Módulo 3: Check-in Digital</strong>.</p>`,
            icon: 'info',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#2BA49A'
        });

    } else if (stat === 'checked_in') {
        // Placeholder — se completará en Módulo 5 (Check-out)
        Swal.fire({
            title: '<i class="fas fa-sign-out-alt text-primary me-2"></i>Check-out',
            html: `<p>El proceso de check-out estará disponible en el <strong>Módulo 5: Check-out y Cierre Financiero</strong>.</p>`,
            icon: 'info',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#2BA49A'
        });

    } else {
        viewReservationDetails(reservationId);
    }
}

/** Crear nueva reserva — placeholder para Componente 3 */
function createReservation() {
    const operationMode = window.OperationMode || 'hotel';

    if (operationMode === 'motel') {
        Swal.fire({
            icon: 'info',
            title: 'Modo Motel',
            text: 'Las estancias por horas se implementarán en el Componente 4.',
            confirmButtonColor: 'var(--primary)'
        });
        return;
    }

    if (operationMode === 'hybrid') {
        Swal.fire({
            title: '¿Qué tipo de reserva?',
            html: `
                <div class="d-flex gap-3 justify-content-center mt-2">
                    <button class="btn btn-primary" onclick="window.location.href='/Reservations/Create'; Swal.close();">
                        <i class="fas fa-moon me-2"></i>Por Noches
                    </button>
                    <button class="btn btn-outline-primary" onclick="Swal.close()">
                        <i class="fas fa-clock me-2"></i>Por Horas (próximamente)
                    </button>
                </div>
            `,
            showConfirmButton: false,
            showCloseButton: true,
        });
        return;
    }

    // Modo hotel — directo al formulario
    window.location.href = '/Reservations/Create';
}

/** Llamar al huésped */
function callGuest(phone, name) {
    if (!phone) {
        showInfoToast('Este huésped no tiene teléfono registrado');
        return;
    }
    Swal.fire({
        title: `<i class="fas fa-phone text-success me-2"></i>Llamar a ${name}`,
        html: `<p class="fs-4 fw-bold text-primary">${phone}</p>
               <p class="text-muted small">Haz clic en "Llamar" para marcar el número.</p>`,
        confirmButtonText: `<i class="fas fa-phone me-1"></i>Llamar`,
        confirmButtonColor: '#38A169',
        showCancelButton: true,
        cancelButtonText: 'Cancelar',
        cancelButtonColor: '#6c757d'
    }).then(result => {
        if (result.isConfirmed) {
            window.location.href = `tel:${phone}`;
        }
    });
}

/** Editar reserva — placeholder Componente 3 */
function editReservation(reservationId) {
    Swal.fire({
        icon: 'info',
        title: 'Próximamente',
        text: 'La edición de reservas estará disponible en una próxima actualización.',
        confirmButtonText: 'Entendido',
        confirmButtonColor: '#2BA49A'
    });
}

async function copyFolio(folio) {
    try {
        await navigator.clipboard.writeText(folio);
        showSuccessToast(`Folio ${folio} copiado al portapapeles`);
    } catch (err) {
        console.error('Error al copiar folio:', err);
        showErrorToast('No se pudo copiar el folio');
    }
}

/** Cancelar reserva — placeholder Componente 3 */
function cancelReservation(reservationId, folio, guestName) {
    Swal.fire({
        title: '<i class="fas fa-times-circle text-danger me-2"></i>¿Cancelar reserva?',
        html: `
            <p>¿Estás seguro de cancelar la reserva <strong>${folio}</strong>?</p>
            <p class="text-muted small mt-1">Huésped: ${guestName}</p>
            <p class="text-danger small mt-2">Esta acción no se puede deshacer y se aplicará la política de cancelación del hotel.</p>`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, cancelar reserva',
        cancelButtonText: 'No, mantener',
        confirmButtonColor: '#E53E3E',
        cancelButtonColor: '#6c757d'
    }).then(result => {
        if (result.isConfirmed) {
            // Placeholder: cuando Componente 3 esté listo, aquí irá el postJSON
            showInfoToast('Cancelación de reservas disponible en Componente 3');
        }
    });
}

// ============================================================================
// HELPERS DE UI
// ============================================================================

function updateReservationsHeader(title, subtitle) {
    $('#reservationsTitle').text(title);
    $('#reservationsSubtitle').text(subtitle);
}

function updateReservationsSubtitle(count) {
    const current = $('#reservationsSubtitle').text();
    // Solo reemplaza si hay algo que decir sobre el conteo
    if (AvailabilityModule.filters.channel || AvailabilityModule.filters.status) {
        $('#reservationsSubtitle').text(`${count} resultado${count !== 1 ? 's' : ''} con filtros aplicados`);
    }
}

function updateTabCount(tab, count) {
    const ids = { today: '#countToday', upcoming: '#countUpcoming', bydate: '#countByDate' };
    if (ids[tab]) $(ids[tab]).text(count);
}

function showLoadingList() {
    $('#reservationsList').html(`
        <div class="loading-state">
            <i class="fas fa-circle-notch fa-spin text-primary"></i>
            <span>Cargando reservas…</span>
        </div>`);
}

function showEmptyState(title, message) {
    $('#reservationsList').html(
        buildEmptyStateHTML('fa-calendar-times', title, message)
    );
}

function buildEmptyStateHTML(icon, title, message) {
    return `
    <div class="empty-state">
        <div class="empty-state-icon">
            <i class="fas ${icon}"></i>
        </div>
        <h3>${title}</h3>
        <p>${message}</p>
    </div>`;
}

// ============================================================================
// HELPERS AJAX (usa las funciones de common.js cuando es posible)
// ============================================================================

/** GET que devuelve JSON — wrapping de fetch para este módulo */
async function fetchJSON(url) {
    const response = await fetch(url);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return await response.json();
}

// ============================================================================
// BÚSQUEDA GLOBAL
// ============================================================================

async function searchReservations(q) {
    enterSearchMode(q);
    showLoadingList();

    try {
        const result = await fetchJSON(`/Availability/SearchReservations?q=${encodeURIComponent(q)}`);

        if (!result.success) {
            showEmptyState('Error al buscar', 'No se pudo completar la búsqueda.');
            return;
        }

        const count = result.data?.length ?? 0;
        $('#searchModeText').text(
            count === 0
                ? `Sin resultados para "${q}"`
                : `${count} resultado${count !== 1 ? 's' : ''} para "${q}"`
        );

        if (count === 0) {
            showEmptyState(
                'Sin resultados',
                `No se encontraron reservas para "${q}".`
            );
            return;
        }

        // Reutilizamos el mismo renderizador — las tarjetas son idénticas
        AvailabilityModule.allReservations = result.data;
        renderReservationList(result.data);

    } catch (err) {
        console.error('Error en búsqueda global:', err);
        showEmptyState('Error al buscar', 'No se pudo conectar con el servidor.');
    }
}

function enterSearchMode(q) {
    AvailabilityModule.isSearchMode = true;

    // Mostrar X y banner
    $('#searchClearBtn').show();
    $('#searchModeBanner').show();
    $('#searchModeText').text(`Buscando "${q}"…`);

    // Deshabilitar tabs visualmente
    $('#reservationsTabs').addClass('tabs-disabled');

    // Ocultar header dinámico (no aplica en modo búsqueda)
    $('.reservations-header-box').hide();
}

function exitSearchMode(reload = true) {
    AvailabilityModule.isSearchMode = false;

    // Ocultar X y banner
    $('#searchClearBtn').hide();
    $('#searchModeBanner').hide();

    // Rehabilitar tabs
    $('#reservationsTabs').removeClass('tabs-disabled');

    // Restaurar header
    $('.reservations-header-box').show();

    // Volver al tab activo
    if (reload) {
        changeTab(AvailabilityModule.currentTab);
    }
}