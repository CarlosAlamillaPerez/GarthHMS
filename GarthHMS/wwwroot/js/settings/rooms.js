// ============================================================================
// GARTH HMS - ROOMS MODULE
// Metodología: Partial Views + AJAX + SweetAlert2
// ============================================================================

// ============================================================================
// VARIABLES GLOBALES
// ============================================================================

let $table;
let allRooms = [];

// Mapeo de estados (número a string y viceversa)
const ROOM_STATUS = {
    // Número a texto en español
    NAMES: {
        1: 'Disponible',
        2: 'Ocupada',
        3: 'Sucia',
        4: 'En Limpieza',
        5: 'Mantenimiento',
        7: 'Reservada'
    },
    // String enum a número (para enviar al backend)
    TO_NUMBER: {
        'Available': 1,
        'Occupied': 2,
        'Dirty': 3,
        'Cleaning': 4,
        'Maintenance': 5,
        'Reserved': 7
    },
    // Configuración de badges
    CONFIG: {
        1: { class: 'badge bg-success', icon: 'fa-check-circle' },
        2: { class: 'badge bg-danger', icon: 'fa-user' },
        3: { class: 'badge bg-warning', icon: 'fa-broom' },
        4: { class: 'badge bg-info', icon: 'fa-spray-can' },
        5: { class: 'badge bg-secondary', icon: 'fa-wrench' },
        7: { class: 'badge bg-primary', icon: 'fa-calendar-check' }
    }
};

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    console.log('Rooms module initialized');
    $table = $('#roomsTable');
    loadRooms();
});

$(document).on('submit', '#createForm', function (e) {
    e.preventDefault();
    handleCreate();
});

$(document).on('submit', '#editForm', function (e) {
    e.preventDefault();
    handleUpdate();
});

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadRooms() {
    try {
        showLoading('Cargando habitaciones...');

        const response = await fetch('/Rooms/GetAll');
        const result = await response.json();

        hideLoading();

        if (result.success) {
            allRooms = result.data;
            $table.bootstrapTable('load', allRooms);
            populateFilters(allRooms);
            console.log(`${allRooms.length} habitaciones cargadas`);
        } else {
            showErrorToast(result.message || 'Error al cargar las habitaciones');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error loading rooms:', error);
        showErrorToast('Error al cargar las habitaciones');
    }
}

// ============================================================================
// MODAL CREAR
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/Rooms/GetCreateForm');
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-plus-circle"></i> Crear Nueva Habitación',
            html: html,
            width: '700px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening create modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleCreate() {
    try {
        const formData = getFormData('createForm');

        // Validaciones básicas
        if (!formData.RoomTypeId) {
            showErrorToast('Debe seleccionar un tipo de habitación');
            return;
        }

        const floor = Number(formData.Floor);

        if (isNaN(floor)) {
            showErrorToast('El piso es requerido');
            return;
        }

        if (floor < 0) {
            showErrorToast('El piso debe ser mayor o igual a 0');
            return;
        }

        showLoading('Creando habitación...');

        const result = await postJSON('/Rooms/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating room:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear la habitación'}`);
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(roomId) {
    try {
        const html = await fetchHTML(`/Rooms/GetEditForm/${roomId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit"></i> Editar Habitación',
            html: html,
            width: '700px',
            showConfirmButton: false,
            showCloseButton: true,
            allowOutsideClick: false
        });
    } catch (error) {
        console.error('Error opening edit modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleUpdate() {
    try {
        const formData = getFormData('editForm');

        // Validaciones básicas
        if (!formData.RoomNumber || formData.RoomNumber.trim() === '') {
            showErrorToast('El número de habitación es requerido');
            return;
        }

        if (!formData.Floor || formData.Floor < 0) {
            showErrorToast('El piso debe ser mayor o igual a 0');
            return;
        }

        showLoading('Actualizando habitación...');

        const result = await postJSON('/Rooms/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating room:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar la habitación'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(roomId) {
    try {
        const html = await fetchHTML(`/Rooms/GetDetails/${roomId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle"></i> Detalles de la Habitación',
            html: html,
            width: '800px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error viewing details:', error);
        showErrorToast('Error al cargar los detalles');
    }
}

// ============================================================================
// MODAL ELIMINAR
// ============================================================================

async function openDeleteModal(roomId) {
    try {
        const html = await fetchHTML(`/Rooms/GetDeleteConfirmation/${roomId}`);
        if (!html) return;

        Swal.fire({
            html: html,
            width: '600px',
            showConfirmButton: false,
            showCloseButton: true
        });
    } catch (error) {
        console.error('Error opening delete modal:', error);
        showErrorToast('Error al abrir la confirmación');
    }
}

async function confirmDeleteAction(roomId) {
    try {
        showLoading('Eliminando habitación...');

        const response = await fetch(`/Rooms/Delete/${roomId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting room:', error);
        showErrorToast('Error al eliminar la habitación');
    }
}

// ============================================================================
// CAMBIO DE ESTADO
// ============================================================================

async function changeRoomStatus(roomId, newStatus) {
    try {
        // Obtener número y nombre del estado desde la constante
        const statusNumber = ROOM_STATUS.TO_NUMBER[newStatus];
        const statusName = ROOM_STATUS.NAMES[statusNumber];

        const confirmed = await Swal.fire({
            title: '¿Cambiar estado?',
            text: `¿Desea cambiar el estado a "${statusName}"?`,
            icon: 'question',
            showCancelButton: true,
            reverseButtons: true,
            confirmButtonText: 'Sí, cambiar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: 'var(--primary)',
            cancelButtonColor: '#6C757D'
        });

        if (!confirmed.isConfirmed) return;

        showLoading('Cambiando estado...');

        const response = await fetch(`/Rooms/UpdateStatus/${roomId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ StatusString: newStatus })
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error changing status:', error);
        showErrorToast('Error al cambiar el estado');
    }
}

async function setMaintenance(roomId) {
    try {
        const { value: notes } = await Swal.fire({
            title: 'Poner en Mantenimiento',
            input: 'textarea',
            inputLabel: 'Motivo del mantenimiento (opcional)',
            inputPlaceholder: 'Describa el motivo del mantenimiento...',
            showCancelButton: true,
            reverseButtons: true,
            confirmButtonText: 'Confirmar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: 'var(--danger)',
            cancelButtonColor: '#6C757D'
        });

        if (notes === undefined) return; // User cancelled

        showLoading('Poniendo en mantenimiento...');

        const response = await fetch(`/Rooms/SetMaintenance/${roomId}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Notes: notes || null })
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error setting maintenance:', error);
        showErrorToast('Error al poner en mantenimiento');
    }
}

async function setAvailable(roomId) {
    try {
        const confirmed = await Swal.fire({
            title: '¿Marcar como disponible?',
            text: 'La habitación estará lista para ser ocupada',
            icon: 'question',
            showCancelButton: true,
            reverseButtons: true,
            confirmButtonText: 'Sí, marcar',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: 'var(--success)',
            cancelButtonColor: '#6C757D'
        });

        if (!confirmed.isConfirmed) return;

        showLoading('Marcando como disponible...');

        const response = await fetch(`/Rooms/SetAvailable/${roomId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            showSuccessToast(result.message);
            await loadRooms();
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error setting available:', error);
        showErrorToast('Error al marcar como disponible');
    }
}

// ============================================================================
// FORMATTERS (Para Bootstrap Table)
// ============================================================================

function floorFormatter(value, row) {
    if (value === 0 || value === '0') {
        return '<span class="badge bg-secondary">PB</span>';
    }
    return `<span class="badge bg-info">${value}</span>`;
}

function statusFormatter(value, row) {
    const name = ROOM_STATUS.NAMES[value] || `Estado ${value}`;
    const config = ROOM_STATUS.CONFIG[value] || {
        class: 'badge bg-light',
        icon: 'fa-question-circle'
    };

    return `<span class="${config.class}">
                <i class="fas ${config.icon}"></i> ${name}
            </span>`;
}

function occupancyFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-danger"><i class="fas fa-user"></i> Ocupada</span>';
    }
    return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Libre</span>';
}

function notesFormatter(value, row) {
    if (!value || value.trim() === '') {
        return '<span class="text-muted">Sin notas</span>';
    }

    const truncated = value.length > 30 ? value.substring(0, 30) + '...' : value;
    return `<span class="text-truncate" title="${value}">${truncated}</span>`;
}

function activeFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activa</span>';
    }
    return '<span class="badge bg-secondary"><i class="fas fa-ban"></i> Inactiva</span>';
}

function actionsFormatter(value, row) {
    const isOccupied = row.isOccupied;
    const status = row.status;

    let html = `
        <div class="btn-group" role="group">
            <button class="btn btn-sm btn-info" onclick="viewDetails('${row.roomId}')" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-warning" onclick="openEditModal('${row.roomId}')" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
    `;

    // Botón de cambio de estado - Dropdown
    html += `
            <div class="btn-group" role="group">
                <button type="button" class="btn btn-sm btn-secondary dropdown-toggle" data-bs-toggle="dropdown" title="Cambiar estado">
                    <i class="fas fa-exchange-alt"></i>
                </button>
                <ul class="dropdown-menu">
                    <li><a class="dropdown-item" href="javascript:void(0)" onclick="setAvailable('${row.roomId}')">
                        <i class="fas fa-check-circle text-success"></i> Disponible
                    </a></li>
                    <li><a class="dropdown-item" href="javascript:void(0)" onclick="changeRoomStatus('${row.roomId}', 'Dirty')">
                        <i class="fas fa-broom text-warning"></i> Sucia
                    </a></li>
                    <li><a class="dropdown-item" href="javascript:void(0)" onclick="changeRoomStatus('${row.roomId}', 'Cleaning')">
                        <i class="fas fa-spray-can text-info"></i> En Limpieza
                    </a></li>
                    <li><hr class="dropdown-divider"></li>
                    <li><a class="dropdown-item" href="javascript:void(0)" onclick="setMaintenance('${row.roomId}')">
                        <i class="fas fa-wrench text-secondary"></i> Mantenimiento
                    </a></li>
                </ul>
            </div>
    `;

    // Botón eliminar (solo si no está ocupada, está activa, y no es Reserved)
    if (!isOccupied && row.isActive && status !== 7) {  // 7 = Reserved
        html += `
            <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${row.roomId}')" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        `;
    }

    html += '</div>';
    return html;
}

// ============================================================================
// FILTROS
// ============================================================================

function populateFilters(rooms) {
    // Poblar filtro de pisos
    const floors = [...new Set(rooms.map(r => r.floor))].sort((a, b) => a - b);
    const floorSelect = $('#filterFloor');
    floorSelect.find('option:not(:first)').remove();
    floors.forEach(floor => {
        const displayFloor = floor === 0 ? 'PB' : floor;
        floorSelect.append(`<option value="${floor}">Piso ${displayFloor}</option>`);
    });

    // Poblar filtro de tipos
    const types = [...new Set(rooms.map(r => ({
        id: r.roomTypeId,
        name: r.roomTypeName
    })).map(t => JSON.stringify(t)))].map(t => JSON.parse(t));

    const typeSelect = $('#filterType');
    typeSelect.find('option:not(:first)').remove();
    types.forEach(type => {
        typeSelect.append(`<option value="${type.id}">${type.name}</option>`);
    });
}

function filterByStatus() {
    const selectedStatus = $('#filterStatus').val();

    if (!selectedStatus) {
        $table.bootstrapTable('load', allRooms);
        return;
    }

    const statusNumber = ROOM_STATUS.TO_NUMBER[selectedStatus];

    const filtered = allRooms.filter(room =>
        room.status === statusNumber
    );

    $table.bootstrapTable('load', filtered);
}

function filterByFloor() {
    const selectedFloor = $('#filterFloor').val();

    if (!selectedFloor) {
        $table.bootstrapTable('load', allRooms);
        return;
    }

    const filtered = allRooms.filter(room =>
        room.floor.toString() === selectedFloor
    );

    $table.bootstrapTable('load', filtered);
}

function filterByType() {
    const selectedType = $('#filterType').val();

    if (!selectedType) {
        $table.bootstrapTable('load', allRooms);
        return;
    }

    const filtered = allRooms.filter(room =>
        room.roomTypeId === selectedType
    );

    $table.bootstrapTable('load', filtered);
}

function clearFilters() {
    $('#filterStatus').val('');
    $('#filterFloor').val('');
    $('#filterType').val('');
    $table.bootstrapTable('load', allRooms);
    showInfoToast('Filtros limpiados');
}

// ============================================================================
// HELPERS
// ============================================================================

function getRoomStatusBadge(status) {
    return statusFormatter(status, {});
}

console.log('Rooms module loaded successfully');