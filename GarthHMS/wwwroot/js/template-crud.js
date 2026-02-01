// ============================================================================
// TEMPLATE-CRUD.JS - Plantilla para crear nuevos módulos CRUD
// 
// INSTRUCCIONES:
// 1. Copia este archivo y renómbralo (ej: users.js, roles.js, etc.)
// 2. Reemplaza TODOS los "Template" con el nombre de tu módulo (ej: User, Role)
// 3. Ajusta los campos del formulario según tu entidad
// 4. Actualiza los formatters de la tabla según tus necesidades
// 5. Verifica que los endpoints del controller coincidan
// 
// REQUISITOS PREVIOS:
// - Controller con métodos: Index, GetAll, Create, Update, Delete
// - Controller con métodos de vistas parciales: GetCreateForm, GetEditForm, GetDetails, GetDeleteConfirmation
// - Views parciales: _CreateModal.cshtml, _EditModal.cshtml, _ViewDetailsModal.cshtml, _DeleteConfirmation.cshtml
// - Stored Procedures en la base de datos
// - DTOs en la capa Core
// - Repository y Service implementados
// ============================================================================

let $table;

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    $table = $('#templatesTable'); // 👈 CAMBIAR: ID de tu tabla
    loadTemplates(); // 👈 CAMBIAR: nombre de tu función
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

async function loadTemplates() {
    try {
        // 👈 CAMBIAR: endpoint de tu controller
        const response = await fetch('/Templates/GetAll');
        const result = await response.json();

        if (result.success) {
            // Cargar datos en Bootstrap Table
            $table.bootstrapTable('load', result.data);
        } else {
            showErrorToast(result.message || 'Error al cargar los datos');
            console.error('Error del servidor:', result);
            $table.bootstrapTable('load', []);
        }
    } catch (error) {
        console.error('Error loading templates:', error);
        showErrorToast('Error al cargar los datos');
        $table.bootstrapTable('load', []);
    }
}

// ============================================================================
// FORMATTERS (Bootstrap Table)
// ============================================================================

// 👈 CAMBIAR: Ajusta estos formatters según tus campos

function nameFormatter(value, row) {
    return `<strong>${value}</strong>`;
}

function statusFormatter(value, row) {
    if (value) {
        return '<span class="badge bg-success"><i class="fas fa-check-circle"></i> Activo</span>';
    } else {
        return '<span class="badge bg-secondary"><i class="fas fa-times-circle"></i> Inactivo</span>';
    }
}

function actionsFormatter(value, row) {
    // 👈 CAMBIAR: templateId por el ID de tu entidad
    return `
        <div class="btn-group" role="group">
            <button class="btn btn-sm btn-info" onclick="viewDetails('${row.templateId}')" title="Ver detalles">
                <i class="fas fa-eye"></i>
            </button>
            <button class="btn btn-sm btn-warning" onclick="openEditModal('${row.templateId}')" title="Editar">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-danger" onclick="openDeleteModal('${row.templateId}')" title="Eliminar">
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
        // 👈 CAMBIAR: endpoint de tu controller
        const html = await fetchHTML('/Templates/GetCreateForm');

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-plus-circle"></i> Crear Nuevo Template', // 👈 CAMBIAR: título
            html: html,
            width: '900px', // 👈 CAMBIAR: ajusta el ancho si es necesario
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

        // 👈 CAMBIAR: Validaciones específicas de tu entidad
        // Ejemplo de validaciones básicas
        if (!formData.Name) {
            showErrorToast('El nombre es requerido');
            return;
        }

        // Ejemplo de validación personalizada
        // if (parseInt(formData.SomeField) < 0) {
        //     showErrorToast('El campo debe ser positivo');
        //     return;
        // }

        showLoading('Creando...'); // 👈 CAMBIAR: mensaje de loading

        // 👈 CAMBIAR: endpoint de tu controller
        const result = await postJSON('/Templates/Create', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadTemplates(); // 👈 CAMBIAR: recargar tu tabla
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error creating template:', error);
        showErrorToast(`Error: ${error.message || 'Error al crear'}`);
    }
}

// ============================================================================
// MODAL EDITAR
// ============================================================================

async function openEditModal(templateId) { // 👈 CAMBIAR: nombre del parámetro
    try {
        // 👈 CAMBIAR: endpoint de tu controller
        const html = await fetchHTML(`/Templates/GetEditForm/${templateId}`);

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-edit"></i> Editar Template', // 👈 CAMBIAR: título
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

        // 👈 CAMBIAR: mismas validaciones que en handleCreate
        if (!formData.Name) {
            showErrorToast('El nombre es requerido');
            return;
        }

        showLoading('Actualizando...'); // 👈 CAMBIAR: mensaje

        // 👈 CAMBIAR: endpoint de tu controller
        const result = await postJSON('/Templates/Update', formData);

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadTemplates(); // 👈 CAMBIAR: recargar tu tabla
        } else {
            showErrorToast(result.message || 'Error desconocido');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating template:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar'}`);
    }
}

// ============================================================================
// MODAL VER DETALLES
// ============================================================================

async function viewDetails(templateId) { // 👈 CAMBIAR: nombre del parámetro
    try {
        // 👈 CAMBIAR: endpoint de tu controller
        const html = await fetchHTML(`/Templates/GetDetails/${templateId}`);

        if (!html) return;

        Swal.fire({
            title: '<i class="fas fa-info-circle"></i> Detalles del Template', // 👈 CAMBIAR: título
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

async function openDeleteModal(templateId) { // 👈 CAMBIAR: nombre del parámetro
    try {
        // 👈 CAMBIAR: endpoint de tu controller
        const html = await fetchHTML(`/Templates/GetDeleteConfirmation/${templateId}`);

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

async function confirmDeleteAction(templateId) { // 👈 CAMBIAR: nombre del parámetro
    try {
        showLoading('Eliminando...'); // 👈 CAMBIAR: mensaje

        // 👈 CAMBIAR: endpoint de tu controller
        const response = await fetch(`/Templates/Delete/${templateId}`, {
            method: 'POST'
        });

        const result = await response.json();

        hideLoading();

        if (result.success) {
            Swal.close();
            showSuccessToast(result.message);
            await loadTemplates(); // 👈 CAMBIAR: recargar tu tabla
        } else {
            showErrorToast(result.message);
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error deleting template:', error);
        showErrorToast('Error al eliminar');
    }
}

// ============================================================================
// FUNCIONES AUXILIARES ESPECÍFICAS (OPCIONAL)
// ============================================================================

// Aquí puedes agregar funciones específicas de tu módulo
// Por ejemplo: cargar opciones de un dropdown, calcular totales, etc.

// Ejemplo:
// async function loadRelatedData() {
//     const response = await fetch('/Templates/GetRelatedData');
//     const result = await response.json();
//     return result.data;
// }