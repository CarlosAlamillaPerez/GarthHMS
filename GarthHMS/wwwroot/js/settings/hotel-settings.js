// ============================================================================
// HOTEL SETTINGS - GARTH HMS
// Gestión de configuración general del hotel
// Metodología: Formulario único con TABS (NO usa tabla ni modales)
// ============================================================================

// ============================================================================
// VARIABLES GLOBALES
// ============================================================================

let currentSettings = null; // Guarda la configuración actual

// ============================================================================
// INICIALIZACIÓN
// ============================================================================

$(document).ready(function () {
    console.log('Hotel Settings module loaded');

    // Cargar configuración al iniciar
    loadSettings();

    // Event Listeners
    setupEventListeners();
});

// ============================================================================
// CONFIGURACIÓN DE EVENT LISTENERS
// ============================================================================

function setupEventListeners() {
    // Submit del formulario
    $('#settingsForm').on('submit', handleUpdate);

    // Checkbox de impuestos - habilitar/deshabilitar campos
    $('#chargesTaxes').on('change', toggleTaxFields);

    // Checkbox de facturación - habilitar/deshabilitar campos SAT
    $('#canInvoice').on('change', toggleInvoiceFields);

    // Sincronizar color picker con input text
    $('#primaryColor').on('input', function () {
        $('#primaryColorText').val($(this).val().toUpperCase());
    });

    $('#primaryColorText').on('input', function () {
        const color = $(this).val();
        if (/^#[0-9A-F]{6}$/i.test(color)) {
            $('#primaryColor').val(color);
        }
    });

    $('#secondaryColor').on('input', function () {
        $('#secondaryColorText').val($(this).val().toUpperCase());
    });

    $('#secondaryColorText').on('input', function () {
        const color = $(this).val();
        if (/^#[0-9A-F]{6}$/i.test(color)) {
            $('#secondaryColor').val(color);
        }
    });

    // Preview del logo
    $('#logoUrl').on('blur', function () {
        const url = $(this).val();
        if (url) {
            showLogoPreview(url);
        } else {
            hideLogoPreview();
        }
    });
}

// ============================================================================
// CARGAR CONFIGURACIÓN INICIAL
// ============================================================================

async function loadSettings() {
    try {
        showLoading('Cargando configuración...');

        const response = await fetch('/HotelSettings/GetSettings');
        const result = await response.json();

        hideLoading();

        if (result.success && result.data) {
            currentSettings = result.data;
            populateForm(result.data);
        } else {
            showErrorToast(result.message || 'No se pudo cargar la configuración');
            console.error('Error al cargar configuración:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error loading settings:', error);
        showErrorToast('Error al cargar la configuración del hotel');
    }
}

// ============================================================================
// POBLAR FORMULARIO CON DATOS
// ============================================================================

function populateForm(data) {
    try {
        // TAB 1: GENERAL
        $('#hotelName').val(data.hotelName || '');
        $('#operationMode').val(data.operationMode || 'hotel');
        $('#address').val(data.address || '');
        $('#city').val(data.city || '');
        $('#state').val(data.state || '');
        $('#postalCode').val(data.postalCode || '');
        $('#country').val(data.country || 'México');
        $('#phone').val(data.phone || '');
        $('#email').val(data.email || '');
        $('#website').val(data.website || '');

        // TAB 2: HORARIOS
        // Convertir TimeSpan de C# a formato HH:mm
        $('#checkInTime').val(formatTimeSpan(data.checkInTime) || '15:00');
        $('#checkOutTime').val(formatTimeSpan(data.checkOutTime) || '12:00');
        $('#lateCheckoutTime').val(formatTimeSpan(data.lateCheckoutTime) || '14:00');
        $('#lateCheckoutCharge').val(data.lateCheckoutCharge || 0);

        // TAB 3: POLÍTICAS
        $('#cancellationHours').val(data.cancellationHours || 24);
        $('#cancellationPolicyText').val(data.cancellationPolicyText || '');
        $('#minDepositPercent').val(data.minDepositPercent || 50);
        $('#requireCompanionDetails').prop('checked', data.requireCompanionDetails || false);

        // TAB 4: IMPUESTOS
        $('#chargesTaxes').prop('checked', data.chargesTaxes !== false); // Default true
        $('#taxIvaPercent').val(data.taxIvaPercent || 16.00);
        $('#taxIshPercent').val(data.taxIshPercent || 3.00);
        toggleTaxFields(); // Aplicar estado inicial

        // TAB 5: FACTURACIÓN
        $('#canInvoice').prop('checked', data.canInvoice || false);
        $('#satRfc').val(data.satRfc || '');
        $('#satBusinessName').val(data.satBusinessName || '');
        $('#satTaxRegime').val(data.satTaxRegime || '');
        toggleInvoiceFields(); // Aplicar estado inicial

        // TAB 6: BRANDING
        $('#logoUrl').val(data.logoUrl || '');
        if (data.logoUrl) {
            showLogoPreview(data.logoUrl);
        }

        const primaryColor = data.primaryColor || '#2BA49A';
        const secondaryColor = data.secondaryColor || '#D9C9B6';

        $('#primaryColor').val(primaryColor);
        $('#primaryColorText').val(primaryColor.toUpperCase());
        $('#secondaryColor').val(secondaryColor);
        $('#secondaryColorText').val(secondaryColor.toUpperCase());

        console.log('Form populated successfully');
    } catch (error) {
        console.error('Error populating form:', error);
        showErrorToast('Error al llenar el formulario');
    }
}

// ============================================================================
// CONVERTIR TIMESPAN DE C# A HH:MM
// ============================================================================

function formatTimeSpan(timeSpan) {
    if (!timeSpan) return null;

    // Si viene como string "15:00:00"
    if (typeof timeSpan === 'string') {
        const parts = timeSpan.split(':');
        return `${parts[0]}:${parts[1]}`; // Retorna "15:00"
    }

    // Si viene como objeto { hours: 15, minutes: 0, seconds: 0 }
    if (typeof timeSpan === 'object' && timeSpan.hours !== undefined) {
        const hours = String(timeSpan.hours).padStart(2, '0');
        const minutes = String(timeSpan.minutes || 0).padStart(2, '0');
        return `${hours}:${minutes}`;
    }

    return null;
}

// ============================================================================
// ACTUALIZAR CONFIGURACIÓN
// ============================================================================

async function handleUpdate(e) {
    e.preventDefault();

    try {
        // Obtener datos del formulario
        const formData = getFormData('settingsForm');

        // Validaciones personalizadas
        if (!validateSettings(formData)) {
            return;
        }

        showLoading('Guardando configuración...');

        const result = await postJSON('/HotelSettings/Update', formData);

        hideLoading();

        if (result.success) {
            showSuccessToast(result.message || 'Configuración actualizada exitosamente');

            // Recargar datos
            await loadSettings();
        } else {
            showErrorToast(result.message || 'Error al actualizar la configuración');
            console.error('Error del servidor:', result);
        }
    } catch (error) {
        hideLoading();
        console.error('Error updating settings:', error);
        showErrorToast(`Error: ${error.message || 'Error al actualizar la configuración'}`);
    }
}

// ============================================================================
// VALIDACIONES
// ============================================================================

function validateSettings(data) {
    // 1. Validar nombre del hotel
    if (!data.HotelName || data.HotelName.trim() === '') {
        showErrorToast('El nombre del hotel es requerido');
        $('#general-tab').tab('show'); // Ir al tab General
        $('#hotelName').focus();
        return false;
    }

    // 2. Validar horarios
    const checkInTime = $('#checkInTime').val();
    const checkOutTime = $('#checkOutTime').val();
    const lateCheckoutTime = $('#lateCheckoutTime').val();

    if (checkInTime >= checkOutTime) {
        showErrorToast('La hora de check-in debe ser anterior a la hora de check-out');
        $('#schedule-tab').tab('show');
        $('#checkInTime').focus();
        return false;
    }

    if (lateCheckoutTime <= checkOutTime) {
        showErrorToast('La hora de check-out tardío debe ser posterior a la hora de check-out normal');
        $('#schedule-tab').tab('show');
        $('#lateCheckoutTime').focus();
        return false;
    }

    // 3. Validar impuestos
    if (data.ChargesTaxes) {
        const iva = parseFloat(data.TaxIvaPercent);
        const ish = parseFloat(data.TaxIshPercent);

        if (isNaN(iva) || iva < 0 || iva > 100) {
            showErrorToast('El IVA debe estar entre 0 y 100%');
            $('#taxes-tab').tab('show');
            $('#taxIvaPercent').focus();
            return false;
        }

        if (isNaN(ish) || ish < 0 || ish > 100) {
            showErrorToast('El ISH debe estar entre 0 y 100%');
            $('#taxes-tab').tab('show');
            $('#taxIshPercent').focus();
            return false;
        }
    }

    // 4. Validar facturación
    if (data.CanInvoice) {
        if (!data.ChargesTaxes) {
            showErrorToast('Para poder facturar, el hotel DEBE cobrar impuestos');
            $('#taxes-tab').tab('show');
            $('#chargesTaxes').focus();
            return false;
        }

        if (!data.SatRfc || data.SatRfc.trim() === '') {
            showErrorToast('El RFC es obligatorio si el hotel puede facturar');
            $('#invoicing-tab').tab('show');
            $('#satRfc').focus();
            return false;
        }

        if (!data.SatBusinessName || data.SatBusinessName.trim() === '') {
            showErrorToast('La razón social es obligatoria si el hotel puede facturar');
            $('#invoicing-tab').tab('show');
            $('#satBusinessName').focus();
            return false;
        }

        if (!data.SatTaxRegime || data.SatTaxRegime.trim() === '') {
            showErrorToast('El régimen fiscal es obligatorio si el hotel puede facturar');
            $('#invoicing-tab').tab('show');
            $('#satTaxRegime').focus();
            return false;
        }
    }

    // 5. Validar colores hexadecimales
    const primaryColor = data.PrimaryColor;
    const secondaryColor = data.SecondaryColor;

    if (!isValidHexColor(primaryColor)) {
        showErrorToast('El color primario debe estar en formato hexadecimal válido (ej: #2BA49A)');
        $('#branding-tab').tab('show');
        $('#primaryColorText').focus();
        return false;
    }

    if (!isValidHexColor(secondaryColor)) {
        showErrorToast('El color secundario debe estar en formato hexadecimal válido (ej: #D9C9B6)');
        $('#branding-tab').tab('show');
        $('#secondaryColorText').focus();
        return false;
    }

    return true;
}

function isValidHexColor(color) {
    return /^#[0-9A-F]{6}$/i.test(color);
}

// ============================================================================
// TOGGLE DE CAMPOS CONDICIONALES
// ============================================================================

function toggleTaxFields() {
    const chargesTaxes = $('#chargesTaxes').is(':checked');

    if (chargesTaxes) {
        $('#taxFieldsContainer').show();
        $('#taxIvaPercent').prop('required', true);
        $('#taxIshPercent').prop('required', true);
    } else {
        $('#taxFieldsContainer').hide();
        $('#taxIvaPercent').prop('required', false);
        $('#taxIshPercent').prop('required', false);

        // Si no cobra impuestos, NO puede facturar
        $('#canInvoice').prop('checked', false);
        toggleInvoiceFields();
    }
}

function toggleInvoiceFields() {
    const canInvoice = $('#canInvoice').is(':checked');
    const chargesTaxes = $('#chargesTaxes').is(':checked');

    if (canInvoice) {
        // Validar que cobra impuestos
        if (!chargesTaxes) {
            showWarningToast('Para facturar, primero debe habilitar el cobro de impuestos');
            $('#canInvoice').prop('checked', false);
            return;
        }

        $('#invoiceFieldsContainer').show();
        $('#satRfc').prop('required', true);
        $('#satBusinessName').prop('required', true);
        $('#satTaxRegime').prop('required', true);
    } else {
        $('#invoiceFieldsContainer').hide();
        $('#satRfc').prop('required', false);
        $('#satBusinessName').prop('required', false);
        $('#satTaxRegime').prop('required', false);
    }
}

// ============================================================================
// PREVIEW DEL LOGO
// ============================================================================

function showLogoPreview(url) {
    $('#logoPreviewImg').attr('src', url);
    $('#logoPreview').fadeIn();

    // Manejar error de carga de imagen
    $('#logoPreviewImg').on('error', function () {
        hideLogoPreview();
        showWarningToast('No se pudo cargar la imagen del logo. Verifica la URL.');
    });
}

function hideLogoPreview() {
    $('#logoPreview').fadeOut();
    $('#logoPreviewImg').attr('src', '');
}

// ============================================================================
// UTILIDADES
// ============================================================================

function showWarningToast(message) {
    Swal.fire({
        icon: 'warning',
        title: 'Advertencia',
        text: message,
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 4000,
        timerProgressBar: true
    });
}