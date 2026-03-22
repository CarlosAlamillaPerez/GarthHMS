// ============================================================================
// GUESTS.JS - Módulo de Gestión de Huéspedes
// Basado en: template-crud.js
// Sistema: GarthHMS - Property Management System
// ============================================================================

let $table;

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    $table = $('#guestsTable');
    loadGuests();
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

async function loadGuests() {
    try {
        // Obtener filtros
        const isVip = $('#filterIsVip').val();
        const isBlacklisted = $('#filterBlacklist').val();
        const source = $('#filterSource').val();

        // Construir URL con filtros
        let url = '/Guests/GetAll?';
        if (isVip) url += `isVip=${isVip}&`;
        if (isBlacklisted) url += `isBlacklisted=${isBlacklisted}&`;
        if (source) url += `source=${source}&`;

        const response = await fetch(url);

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            // Cargar datos en Bootstrap Table
            $table.bootstrapTable('load', result.data || []);
        } else {
            showErrorToast(result.message || 'Error al cargar los datos');
            console.error('Error loading guests:', result);
            // Cargar tabla vacía en caso de error
            $table.bootstrapTable('load', []);
        }
    } catch (error) {
        console.error('Error loading guests:', error);
        showErrorToast('Error al cargar los huéspedes: ' + error.message);
        // Cargar tabla vacía en caso de error
        $table.bootstrapTable('load', []);
    }
}

// Aplicar filtros
function applyFilters() {
    loadGuests();
}

// Limpiar filtros
function clearFilters() {
    $('#filterIsVip').val('');
    $('#filterBlacklist').val('');
    $('#filterSource').val('');
    loadGuests();
}

// ============================================================================
// MODAL CREAR
// ============================================================================

async function openCreateModal() {
    try {
        const html = await fetchHTML('/Guests/GetCreateForm');
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-user-plus"></i> Nuevo Huésped',
            html: html,
            width: '800px',
            showConfirmButton: false,
            showCloseButton: true,
            didOpen: () => {
                // Inicializar tabs de Bootstrap
                const triggerTabList = [].slice.call(document.querySelectorAll('#createGuestTabs button'));
                triggerTabList.forEach(function (triggerEl) {
                    new bootstrap.Tab(triggerEl);
                });
            }
        });
    } catch (error) {
        console.error('Error opening create modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleCreate() {
    try {
        // Usar la función global getFormData() como en room-types.js y rooms.js
        const formData = getFormData('createForm');

        // Validaciones básicas
        if (!formData.FirstName || !formData.LastName || !formData.Phone) {
            showErrorToast('Por favor complete los campos requeridos: Nombre, Apellido y Teléfono');
            return;
        }

        // Si está marcado como blacklist, validar que tenga razón
        if (formData.IsBlacklisted && !formData.BlacklistReason) {
            showErrorToast('Debe proporcionar una razón para agregar a la lista negra');
            return;
        }

        // Si no está en blacklist, asegurar que la razón sea null
        if (!formData.IsBlacklisted) {
            formData.BlacklistReason = null;
        }

        showLoading('Guardando huésped...');

        // Usar postJSON como en el patrón estándar
        const result = await postJSON('/Guests/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadGuests();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating guest:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear el huésped'}`);
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(guestId) {
    try {
        const html = await fetchHTML(`/Guests/GetEditForm/${guestId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-user-edit"></i> Editar Huésped',
            html: html,
            width: '800px',
            showConfirmButton: false,
            showCloseButton: true,
            didOpen: () => {
                // Inicializar tabs de Bootstrap
                const triggerTabList = [].slice.call(document.querySelectorAll('#editGuestTabs button'));
                triggerTabList.forEach(function (triggerEl) {
                    new bootstrap.Tab(triggerEl);
                });
            }
        });
    } catch (error) {
        console.error('Error opening edit modal:', error);
        showErrorToast('Error al abrir el formulario');
    }
}

async function handleUpdate() {
    try {
        // Usar la función global getFormData() como en room-types.js y rooms.js
        const formData = getFormData('editForm');

        // Validaciones básicas
        if (!formData.FirstName || !formData.LastName || !formData.Phone) {
            showErrorToast('Por favor complete los campos requeridos: Nombre, Apellido y Teléfono');
            return;
        }

        // Si está marcado como blacklist, validar que tenga razón
        if (formData.IsBlacklisted && !formData.BlacklistReason) {
            showErrorToast('Debe proporcionar una razón para agregar a la lista negra');
            return;
        }

        // Si no está en blacklist, asegurar que la razón sea null
        if (!formData.IsBlacklisted) {
            formData.BlacklistReason = null;
        }

        showLoading('Actualizando huésped...');

        // Usar postJSON como en el patrón estándar
        const result = await postJSON('/Guests/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadGuests();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating guest:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar el huésped'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(guestId) {
    try {
        const html = await fetchHTML(`/Guests/GetDetails/${guestId}`);
        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle"></i> Detalles del Huésped',
            html: html,
            width: '900px',
            showConfirmButton: false,
            showCloseButton: true,
            didOpen: () => {
                // Inicializar tabs de Bootstrap si existen
                const triggerTabList = [].slice.call(document.querySelectorAll('#detailsGuestTabs button'));
                triggerTabList.forEach(function (triggerEl) {
                    new bootstrap.Tab(triggerEl);
                });
            }
        });
    } catch (error) {
        console.error('Error viewing details:', error);
        showErrorToast('Error al cargar los detalles');
    }
}

// ============================================================================
// MODAL ELIMINAR
// ============================================================================

async function openDeleteModal(guestId) {
    try {
        const html = await fetchHTML(`/Guests/GetDeleteConfirmation/${guestId}`);
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

async function confirmDeleteAction(guestId) {
    try {
        showLoading('Eliminando huésped...');

        const response = await fetch(`/Guests/Delete/${guestId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadGuests();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting guest:', error);
        showErrorToast(`Error: ${error.message || 'Error al eliminar el huésped'}`);
    }
}

// ============================================================================
// TOGGLE BLACKLIST
// ============================================================================

async function toggleBlacklist(guestId, isBlacklisted) {
    try {
        let reason = null;

        // Si se va a marcar como blacklist, pedir razón
        if (isBlacklisted) {
            const { value: inputReason } = await Swal.fire({
                title: '<i class="fas fa-ban text-danger"></i> Agregar a Lista Negra',
                html: `
                    <div class="text-start">
                        <label class="form-label">Razón (obligatoria)</label>
                        <textarea id="blacklistReason" class="form-control" rows="3" 
                                  placeholder="Ej: No pagó, comportamiento inapropiado, daños a la propiedad..." 
                                  maxlength="500"></textarea>
                    </div>
                `,
                showCancelButton: true,
                confirmButtonText: '<i class="fas fa-ban"></i> Agregar',
                cancelButtonText: '<i class="fas fa-times"></i> Cancelar',
                confirmButtonColor: '#dc3545',
                preConfirm: () => {
                    const reason = document.getElementById('blacklistReason').value.trim();
                    if (!reason) {
                        Swal.showValidationMessage('Debes proporcionar una razón');
                        return false;
                    }
                    return reason;
                }
            });

            if (!inputReason) return;
            reason = inputReason;
        }

        showLoading(isBlacklisted ? 'Agregando a blacklist...' : 'Quitando de blacklist...');

        const result = await postJSON('/Guests/ToggleBlacklist', {
            GuestId: guestId,
            IsBlacklisted: isBlacklisted,
            Reason: reason
        });

        hideLoading();

        if (result.success) {
            showSuccessToast(result.message);
            await loadGuests();
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error toggling blacklist:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar el estado'}`);
    }
}

// ============================================================================
// FORMATTERS PARA BOOTSTRAP TABLE
// ============================================================================

function staysFormatter(value, row) {
    if (value === 0) {
        return '<span class="badge bg-secondary">Primera vez</span>';
    } else if (value >= 5) {
        return `<span class="badge bg-success"><i class="fas fa-trophy"></i> ${value}</span>`;
    } else {
        return `<span class="badge bg-info">${value}</span>`;
    }
}

function dateFormatter(value, row) {
    if (!value) {
        return '<span class="text-muted">Nunca</span>';
    }
    const date = new Date(value);
    return date.toLocaleDateString('es-MX', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

function sourceFormatter(value, row) {
    const sourceIcons = {
        'direct': '<i class="fas fa-desktop"></i>',
        'whatsapp': '<i class="fab fa-whatsapp"></i>',
        'booking': '<i class="fas fa-globe"></i>',
        'airbnb': '<i class="fab fa-airbnb"></i>',
        'expedia': '<i class="fas fa-plane"></i>',
        'walkin': '<i class="fas fa-walking"></i>',
        'other': '<i class="fas fa-question-circle"></i>'
    };

    const icon = sourceIcons[value] || '<i class="fas fa-question"></i>';
    return `<span title="${row.sourceText}">${icon}</span>`;
}

function vipFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-warning"><i class="fas fa-star"></i> VIP</span>';
    }
    return '<span class="text-muted">—</span>';
}

function blacklistFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-danger"><i class="fas fa-ban"></i> Blacklist</span>';
    }
    return '<span class="badge bg-success"><i class="fas fa-check"></i> OK</span>';
}

function actionsFormatter(value, row) {
    let buttons = `
        <div class="btn-group btn-group-sm" role="group">
            <button type="button" class="btn btn-info" onclick="viewDetails('${row.guestId}')" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
    `;

    // Solo mostrar editar si no está en blacklist
    if (!row.isBlacklisted) {
        buttons += `
            <button type="button" class="btn btn-warning" onclick="openEditModal('${row.guestId}')" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
        `;
    }

    // Botón de toggle blacklist
    if (row.isBlacklisted) {
        buttons += `
            <button type="button" class="btn btn-success" onclick="toggleBlacklist('${row.guestId}', false)" title="Quitar de Blacklist">
                <i class="fas fa-check-circle"></i>
            </button>
        `;
    } else {
        buttons += `
            <button type="button" class="btn btn-danger" onclick="toggleBlacklist('${row.guestId}', true)" title="Agregar a Blacklist">
                <i class="fas fa-ban"></i>
            </button>
        `;
    }

    // Botón eliminar (solo si no tiene estancias)
    if (row.totalStays === 0) {
        buttons += `
            <button type="button" class="btn btn-danger" onclick="openDeleteModal('${row.guestId}')" title="Eliminar">
                <i class="fas fa-trash"></i>
            </button>
        `;
    }

    buttons += '</div>';
    return buttons;
}