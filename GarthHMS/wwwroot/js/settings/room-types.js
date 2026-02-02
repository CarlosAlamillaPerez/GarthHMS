// ============================================================================
// ROOM TYPES - Gestión de Tipos de Habitación
// Usa Bootstrap Table (estilo local) + Partial Views + AJAX + SweetAlert2
// ============================================================================

let $table;

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    $table = $('#roomTypesTable');
    loadRoomTypes();
    setupEventHandlers();
});

// ============================================================================
// EVENT HANDLERS
// ============================================================================

function setupEventHandlers() {
    // Submit formulario crear
    $(document).on('submit', '#createForm', async function (e) {
        e.preventDefault();
        await handleCreate();
    });

    // Submit formulario editar
    $(document).on('submit', '#editForm', async function (e) {
        e.preventDefault();
        await handleUpdate();
    });
}

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadRoomTypes() {
    try {
        const response = await fetch('/RoomTypes/GetAll');
        const result = await response.json();

        if (result.success) {
            // Cargar datos en Bootstrap Table
            $table.bootstrapTable('load', result.data);
        } else {
            showErrorToast(result.message || 'Error al cargar los tipos de habitación');
            $table.bootstrapTable('load', []);
        }
    } catch (error) {
        console.error('Error loading room types:', error);
        showErrorToast('Error al cargar los datos');
        $table.bootstrapTable('load', []);
    }
}

// ============================================================================
// FORMATTERS (Bootstrap Table)
// ============================================================================

function priceFormatter(value, row) {
    if (value === null || value === undefined || value === 0) {
        return '<span class="text-muted">$0.00</span>';
    }
    return '<strong>$' + parseFloat(value).toFixed(2) + '</strong>';
}

function statusFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activo</span>';
    } else {
        return '<span class="badge bg-secondary"><i class="fas fa-times-circle"></i> Inactivo</span>';
    }
}

function actionsFormatter(value, row) {
    return `
        <div class="btn-group" role="group">
            <button class="btn btn-sm btn-info" onclick="viewDetails('${row.roomTypeId}')" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-warning" onclick="openEditModal('${row.roomTypeId}')" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${row.roomTypeId}')" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;
}

// ============================================================================
// MODAL CREAR
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/RoomTypes/GetCreateForm');

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-plus-circle"></i> Crear Nuevo Tipo de Habitación',
            html: html,
            width: '900px',
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
        if (!formData.Name || !formData.Code) {
            showErrorToast('Por favor complete los campos requeridos');
            return;
        }

        if (parseInt(formData.MaxCapacity) < parseInt(formData.BaseCapacity)) {
            showErrorToast('La capacidad máxima debe ser mayor o igual a la base');
            return;
        }

        showLoading('Creando tipo de habitación...');

        const result = await postJSON('/RoomTypes/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRoomTypes(); // Recargar tabla
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating room type:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear el tipo de habitación'}`);
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(roomTypeId) {
    try {
        const html = await fetchHTML(`/RoomTypes/GetEditForm/${roomTypeId}`);

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit"></i> Editar Tipo de Habitación',
            html: html,
            width: '900px',
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
        if (!formData.Name || !formData.Code) {
            showErrorToast('Por favor complete los campos requeridos');
            return;
        }

        if (parseInt(formData.MaxCapacity) < parseInt(formData.BaseCapacity)) {
            showErrorToast('La capacidad máxima debe ser mayor o igual a la base');
            return;
        }

        showLoading('Actualizando tipo de habitación...');

        const result = await postJSON('/RoomTypes/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRoomTypes(); // Recargar tabla
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating room type:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar el tipo de habitación'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(roomTypeId) {
    try {
        const html = await fetchHTML(`/RoomTypes/GetDetails/${roomTypeId}`);

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle"></i> Detalles del Tipo de Habitación',
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

async function openDeleteModal(roomTypeId) {
    try {
        const html = await fetchHTML(`/RoomTypes/GetDeleteConfirmation/${roomTypeId}`);

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

async function confirmDeleteAction(roomTypeId) {
    try {
        showLoading('Eliminando tipo de habitación...');

        const response = await fetch(`/RoomTypes/Delete/${roomTypeId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadRoomTypes(); // Recargar tabla
        } else {
            showErrorToast(result.message);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting room type:', error);
        showErrorToast('Error al eliminar el tipo de habitación');
    }
}
