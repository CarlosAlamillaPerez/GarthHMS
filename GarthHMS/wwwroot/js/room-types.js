// ============================================================================
// ROOM TYPES - Gestión de Tipos de Habitación
// Usa Partial Views + AJAX + SweetAlert2
// ============================================================================

let roomTypesData = [];

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    loadRoomTypes();
    setupEventHandlers();
});

// ============================================================================
// EVENT HANDLERS
// ============================================================================

function setupEventHandlers() {
    // Búsqueda en tiempo real
    $('#searchInput').on('keyup', function () {
        filterTable();
    });

    // Filtro por estado
    $('#filterActive').on('change', function () {
        filterTable();
    });

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
        showTableLoading('roomTypesTable', 7);

        const response = await fetch('/RoomTypes/GetAll');
        const result = await response.json();

        if (result.success) {
            roomTypesData = result.data;
            renderTable(roomTypesData);
        } else {
            showErrorToast(result.message || 'Error al cargar los tipos de habitación');
            showTableEmpty('roomTypesTable', 7, 'Error al cargar los datos');
        }
    } catch (error) {
        console.error('Error loading room types:', error);
        showErrorToast('Error al cargar los datos');
        showTableEmpty('roomTypesTable', 7, 'Error al cargar los datos');
    }
}

// ============================================================================
// RENDERIZADO DE TABLA
// ============================================================================

function renderTable(data) {
    const tbody = $('#roomTypesTable tbody');
    tbody.empty();

    if (data.length === 0) {
        showTableEmpty('roomTypesTable', 7, 'No hay tipos de habitación registrados');
        return;
    }

    data.forEach(roomType => {
        const statusBadge = roomType.isActive
            ? '<span class="badge bg-success">Activo</span>'
            : '<span class="badge bg-secondary">Inactivo</span>';

        const row = `
            <tr>
                <td><strong>${roomType.code}</strong></td>
                <td>${roomType.name}</td>
                <td>${roomType.capacityDisplay}</td>
                <td>$${roomType.basePriceNightly.toFixed(2)}</td>
                <td>$${roomType.basePriceHourly.toFixed(2)}</td>
                <td>${statusBadge}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-info" onclick="viewDetails('${roomType.roomTypeId}')" title="Ver detalles">
                        <i class="fas fa-eye"></i>
                    </button>
                    <button class="btn btn-sm btn-warning" onclick="openEditModal('${roomType.roomTypeId}')" title="Editar">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${roomType.roomTypeId}')" title="Eliminar">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
}

// ============================================================================
// FILTRADO
// ============================================================================

function filterTable() {
    const searchText = $('#searchInput').val().toLowerCase();
    const filterActive = $('#filterActive').val();

    let filtered = roomTypesData;

    // Filtrar por búsqueda
    if (searchText) {
        filtered = filtered.filter(rt =>
            rt.name.toLowerCase().includes(searchText) ||
            rt.code.toLowerCase().includes(searchText)
        );
    }

    // Filtrar por estado
    if (filterActive !== '') {
        const isActive = filterActive === 'true';
        filtered = filtered.filter(rt => rt.isActive === isActive);
    }

    renderTable(filtered);
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
            await loadRoomTypes();
        } else {
            showErrorToast(result.message);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating room type:', error);
        showErrorToast('Error al crear el tipo de habitación');
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
            await loadRoomTypes();
        } else {
            showErrorToast(result.message);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating room type:', error);
        showErrorToast('Error al actualizar el tipo de habitación');
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
            await loadRoomTypes();
        } else {
            showErrorToast(result.message);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting room type:', error);
        showErrorToast('Error al eliminar el tipo de habitación');
    }
}