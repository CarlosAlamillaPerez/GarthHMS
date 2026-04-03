// GarthHMS/wwwroot/js/reservations/payments-verification.js
// Componente 6 — Verificación de Anticipos
'use strict';

// ── Estado del módulo ────────────────────────────────────────────────────────
const PvModule = {
    currentTab: 'pending',
    pendingPayments: [],
    verifiedPayments: [],
    verifiedLoaded: false
};

// ── Init ─────────────────────────────────────────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
    loadPendingPayments();
    loadVerifiedPayments();
    loadAutoVerifySettings();
});

// ============================================================================
// TABS
// ============================================================================

function changeTab(tab) {
    PvModule.currentTab = tab;

    document.querySelectorAll('.tab-btn').forEach(btn => btn.classList.remove('active'));
    document.querySelector(`.tab-btn[data-tab="${tab}"]`).classList.add('active');

    document.getElementById('tabPending').style.display = tab === 'pending' ? '' : 'none';
    document.getElementById('tabVerified').style.display = tab === 'verified' ? '' : 'none';

    if (tab === 'verified') {
        if (!PvModule.verifiedLoaded) {
            loadVerifiedPayments();
        } else {
            // Ya cargados en background — solo llenar la tabla
            $('#verifiedPaymentsTable').bootstrapTable('load', PvModule.verifiedPayments);
        }
    }
}

// ============================================================================
// CARGA DE DATOS
// ============================================================================

async function loadPendingPayments() {
    try {
        const res = await fetch('/Payments/GetPendingVerifications');
        const data = await res.json();

        if (!data.success) {
            console.error('Error al cargar pagos pendientes:', data.message);
            showErrorToast('Error al cargar los pagos pendientes');
            return;
        }

        PvModule.pendingPayments = data.data ?? [];
        const count = PvModule.pendingPayments.length;

        updateKpis(PvModule.pendingPayments);
        updateTabCount('Pending', count);

        document.getElementById('pendingSubtitle').textContent =
            `${count} pago${count !== 1 ? 's' : ''} pendiente${count !== 1 ? 's' : ''} de verificación`;

        $('#pendingPaymentsTable').bootstrapTable('load', PvModule.pendingPayments);

    } catch (error) {
        console.error('Error de conexión al cargar pagos pendientes:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

async function loadVerifiedPayments() {
    try {
        const res = await fetch('/Payments/GetVerified');
        const data = await res.json();

        if (!data.success) {
            console.error('Error al cargar pagos verificados:', data.message);
            showErrorToast('Error al cargar los pagos verificados');
            return;
        }

        PvModule.verifiedPayments = data.data ?? [];
        PvModule.verifiedLoaded = true;
        const count = PvModule.verifiedPayments.length;

        updateTabCount('Verified', count);

        document.getElementById('verifiedSubtitle').textContent =
            `${count} pago${count !== 1 ? 's' : ''} verificado${count !== 1 ? 's' : ''}`;

        // Solo cargar en la tabla si el tab ya está visible
        if (PvModule.currentTab === 'verified') {
            $('#verifiedPaymentsTable').bootstrapTable('load', PvModule.verifiedPayments);
        }

    } catch (error) {
        console.error('Error de conexión al cargar pagos verificados:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

// ============================================================================
// KPIs
// ============================================================================

function updateKpis(payments) {
    const count = payments.length;
    const total = payments.reduce((sum, p) => sum + (p.amount ?? 0), 0);
    const oldest = payments.length > 0 ? payments[0].paymentDate : null;

    document.getElementById('kpi-count').textContent = count;
    document.getElementById('kpi-amount').textContent = formatCurrency(total);
    document.getElementById('kpi-oldest').textContent = oldest
        ? new Date(oldest).toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
        : '—';
}

function updateTabCount(tabPascal, count) {
    const el = document.getElementById(`count${tabPascal}`);
    if (el) el.textContent = count;
}

// ============================================================================
// VERIFICACIÓN INDIVIDUAL
// ============================================================================

async function verifyPayment(paymentId, folio, guestName, amount, method) {
    const confirm = await Swal.fire({
        title: '¿Confirmar recepción?',
        html: `
            <div class="text-start">
                <p class="mb-3 text-muted">Estás confirmando que el siguiente pago fue recibido en el banco:</p>
                <div class="d-flex flex-column gap-2">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Reserva</span>
                        <strong>${folio}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Huésped</span>
                        <strong>${guestName}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Método</span>
                        <strong>${method}</strong>
                    </div>
                    <hr class="my-1">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Monto</span>
                        <strong class="text-success fs-5">${formatCurrency(amount)}</strong>
                    </div>
                </div>
            </div>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check me-1"></i>Sí, confirmar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--success)',
        cancelButtonColor: 'var(--text-muted)',
        reverseButtons: true
    });

    if (!confirm.isConfirmed) return;

    try {
        const res = await fetch('/Payments/Verify', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ paymentId })
        });
        const data = await res.json();

        if (!data.success) {
            console.error('Error al verificar pago:', data.message);
            showErrorToast(data.message || 'Error al verificar el pago');
            return;
        }

        showSuccessToast('Pago verificado correctamente');
        PvModule.verifiedLoaded = false;
        loadPendingPayments();

    } catch (error) {
        console.error('Error de conexión al verificar pago:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

// ============================================================================
// VERIFICACIÓN MASIVA
// ============================================================================

async function openBulkVerifyModal() {
    const transfers = PvModule.pendingPayments.filter(p => p.paymentMethod === 'transfer');
    const cards = PvModule.pendingPayments.filter(p => p.paymentMethod === 'card');
    const totalTrans = transfers.reduce((s, p) => s + (p.amount ?? 0), 0);
    const totalCard = cards.reduce((s, p) => s + (p.amount ?? 0), 0);
    const countBoth = transfers.length + cards.length;

    if (countBoth === 0) {
        showInfoToast('No hay pagos pendientes de verificación');
        return;
    }

    const { value: selectedMethod } = await Swal.fire({
        title: '<i class="fas fa-check-double me-2"></i>Verificar en masa',
        width: 520,
        padding: '1.5rem',
        html: `
            <div class="text-start">
                <p class="text-muted mb-3">Selecciona el método que deseas verificar. Se marcarán todos los pagos pendientes del método elegido como recibidos.</p>

                <div class="d-flex flex-column gap-2">

                    <label class="d-flex align-items-center gap-3 p-3 rounded border ${transfers.length === 0 ? 'opacity-50' : ''}"
                           style="cursor:${transfers.length === 0 ? 'not-allowed' : 'pointer'};background:rgba(66,153,225,0.05);">
                        <input type="radio" name="bulkMethod" value="transfer"
                               ${transfers.length === 0 ? 'disabled' : ''}
                               onchange="pvUpdateBulkPreview()"
                               style="width:1.1rem;height:1.1rem;cursor:pointer;">
                        <div class="flex-grow-1">
                            <div class="fw-semibold" style="color:var(--info);">
                                <i class="fas fa-building-columns me-1"></i>Transferencias
                            </div>
                            <div class="text-muted small">${transfers.length} pago${transfers.length !== 1 ? 's' : ''} — ${formatCurrency(totalTrans)}</div>
                        </div>
                        ${transfers.length === 0 ? '<span class="badge bg-secondary">Sin pendientes</span>' : ''}
                    </label>

                    <label class="d-flex align-items-center gap-3 p-3 rounded border ${cards.length === 0 ? 'opacity-50' : ''}"
                           style="cursor:${cards.length === 0 ? 'not-allowed' : 'pointer'};background:rgba(81,98,181,0.05);">
                        <input type="radio" name="bulkMethod" value="card"
                               ${cards.length === 0 ? 'disabled' : ''}
                               onchange="pvUpdateBulkPreview()"
                               style="width:1.1rem;height:1.1rem;cursor:pointer;">
                        <div class="flex-grow-1">
                            <div class="fw-semibold" style="color:var(--tertiary);">
                                <i class="fas fa-credit-card me-1"></i>Tarjeta
                            </div>
                            <div class="text-muted small">${cards.length} pago${cards.length !== 1 ? 's' : ''} — ${formatCurrency(totalCard)}</div>
                        </div>
                        ${cards.length === 0 ? '<span class="badge bg-secondary">Sin pendientes</span>' : ''}
                    </label>

                    <label class="d-flex align-items-center gap-3 p-3 rounded border"
                           style="cursor:pointer;background:rgba(56,161,105,0.05);">
                        <input type="radio" name="bulkMethod" value="both"
                               onchange="pvUpdateBulkPreview()"
                               style="width:1.1rem;height:1.1rem;cursor:pointer;">
                        <div class="flex-grow-1">
                            <div class="fw-semibold" style="color:var(--success);">
                                <i class="fas fa-check-double me-1"></i>Ambos métodos
                            </div>
                            <div class="text-muted small">${countBoth} pago${countBoth !== 1 ? 's' : ''} — ${formatCurrency(totalTrans + totalCard)}</div>
                        </div>
                    </label>

                </div>

                <div id="bulkPreview" class="mt-3 p-3 rounded" style="display:none;
                     background:rgba(56,161,105,0.08);border:1px solid rgba(56,161,105,0.3);">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Se verificarán</span>
                        <strong id="previewCount">—</strong>
                    </div>
                    <div class="d-flex justify-content-between mt-1">
                        <span class="text-muted">Monto total</span>
                        <strong class="text-success" id="previewAmount">—</strong>
                    </div>
                </div>
            </div>`,
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-check me-1"></i>Verificar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--success)',
        reverseButtons: true,
        preConfirm: () => {
            const selected = document.querySelector('input[name="bulkMethod"]:checked');
            if (!selected) {
                Swal.showValidationMessage('Selecciona un método para continuar');
                return false;
            }
            return selected.value;
        }
    });

    if (!selectedMethod) return;
    await executeBulkVerify(selectedMethod);
}

function pvUpdateBulkPreview() {
    const selected = document.querySelector('input[name="bulkMethod"]:checked');
    const preview = document.getElementById('bulkPreview');
    if (!selected || !preview) return;

    const transfers = PvModule.pendingPayments.filter(p => p.paymentMethod === 'transfer');
    const cards = PvModule.pendingPayments.filter(p => p.paymentMethod === 'card');

    let count = 0, amount = 0;

    if (selected.value === 'transfer') {
        count = transfers.length;
        amount = transfers.reduce((s, p) => s + (p.amount ?? 0), 0);
    } else if (selected.value === 'card') {
        count = cards.length;
        amount = cards.reduce((s, p) => s + (p.amount ?? 0), 0);
    } else {
        count = transfers.length + cards.length;
        amount = transfers.reduce((s, p) => s + (p.amount ?? 0), 0)
            + cards.reduce((s, p) => s + (p.amount ?? 0), 0);
    }

    preview.style.display = '';
    document.getElementById('previewCount').textContent = `${count} pago${count !== 1 ? 's' : ''}`;
    document.getElementById('previewAmount').textContent = formatCurrency(amount);
}

async function executeBulkVerify(method) {
    try {
        showLoading('Verificando pagos…');

        const methods = method === 'both' ? ['transfer', 'card'] : [method];
        let totalVerified = 0, totalAmount = 0;

        for (const m of methods) {
            const res = await fetch('/Payments/VerifyBulk', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ method: m })
            });
            const data = await res.json();

            if (!data.success) {
                hideLoading();
                console.error('Error en verificación masiva:', data.message);
                showErrorToast(data.message || 'Error al verificar los pagos');
                return;
            }

            totalVerified += data.verifiedCount ?? 0;
            totalAmount += data.totalAmount ?? 0;
        }

        hideLoading();

        await Swal.fire({
            title: '¡Verificación completada!',
            html: `
                <div class="text-center py-2">
                    <div class="mb-3" style="font-size:3rem;color:var(--success);">
                        <i class="fas fa-check-circle"></i>
                    </div>
                    <div class="d-flex justify-content-center gap-5">
                        <div>
                            <div class="fs-2 fw-bold" style="color:var(--success);">${totalVerified}</div>
                            <div class="text-muted small">pagos verificados</div>
                        </div>
                        <div>
                            <div class="fs-2 fw-bold" style="color:var(--success);">${formatCurrency(totalAmount)}</div>
                            <div class="text-muted small">monto total</div>
                        </div>
                    </div>
                </div>`,
            icon: null,
            confirmButtonText: 'Aceptar',
            confirmButtonColor: 'var(--success)'
        });

        PvModule.verifiedLoaded = false;
        loadPendingPayments();

    } catch (error) {
        hideLoading();
        console.error('Error de conexión en verificación masiva:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

// ============================================================================
// AUTO-VERIFICACIÓN
// ============================================================================

async function loadAutoVerifySettings() {
    try {
        const res = await fetch('/Payments/GetAutoVerifySettings');
        const data = await res.json();

        if (!data.success) {
            console.error('Error al cargar configuración de auto-verificación:', data.message);
            return;
        }

        document.getElementById('autoVerifyCard').checked = data.autoVerifyCard;
        document.getElementById('autoVerifyTransfer').checked = data.autoVerifyTransfer;

    } catch (error) {
        console.error('Error al cargar configuración de auto-verificación:', error);
    }
}

async function saveAutoVerifySettings() {
    const autoVerifyCard = document.getElementById('autoVerifyCard').checked;
    const autoVerifyTransfer = document.getElementById('autoVerifyTransfer').checked;

    // Advertencia especial si activan transferencia
    if (autoVerifyTransfer) {
        const confirm = await Swal.fire({
            title: '⚠️ Advertencia importante',
            html: `
                <div class="text-start">
                    <p>Estás activando la <strong>verificación automática de transferencias</strong>.</p>
                    <div class="alert alert-warning mb-3">
                        <i class="fas fa-exclamation-triangle me-1"></i>
                        Los pagos por transferencia <strong>no se revisarán contra el estado de cuenta</strong>.
                        Podrías confirmar reservas con pagos que aún no llegaron al banco.
                    </div>
                    <p class="mb-0 text-muted small">¿Confirmas que entiendes el riesgo y deseas activar esta opción?</p>
                </div>`,
            icon: null,
            showCancelButton: true,
            confirmButtonText: 'Sí, entiendo el riesgo',
            cancelButtonText: 'Cancelar',
            confirmButtonColor: 'var(--warning)',
            reverseButtons: true
        });

        if (!confirm.isConfirmed) {
            document.getElementById('autoVerifyTransfer').checked = false;
            return;
        }
    }

    try {
        const res = await fetch('/Payments/UpdateAutoVerify', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ autoVerifyCard, autoVerifyTransfer })
        });
        const data = await res.json();

        if (!data.success) {
            console.error('Error al guardar configuración:', data.message);
            showErrorToast(data.message || 'Error al guardar la configuración');
            return;
        }

        showSuccessToast('Configuración guardada correctamente');

    } catch (error) {
        console.error('Error al guardar configuración de auto-verificación:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
    }
}

function toggleAutoVerifyPanel() {
    const panel = document.getElementById('autoVerifyPanel');
    const chevron = document.getElementById('autoVerifyChevron');
    const isOpen = panel.style.display !== 'none';

    panel.style.display = isOpen ? 'none' : '';
    chevron.style.transform = isOpen ? '' : 'rotate(180deg)';
}

// ============================================================================
// FORMATTERS
// ============================================================================

function pv_checkInFormatter(value) {
    if (!value) return '—';
    return new Date(value).toLocaleDateString('es-MX', {
        day: '2-digit', month: 'short', year: 'numeric'
    });
}

function pv_dateFormatter(value) {
    if (!value) return '—';
    const d = new Date(value);
    return d.toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })
        + ' ' + d.toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
}

function pv_amountFormatter(value) {
    return `<strong class="text-success">${formatCurrency(value ?? 0)}</strong>`;
}

function pv_methodFormatter(value) {
    const cfg = {
        'Transferencia': { icon: 'fa-building-columns', color: 'var(--info)' },
        'Tarjeta': { icon: 'fa-credit-card', color: 'var(--tertiary)' },
        'Efectivo': { icon: 'fa-money-bill-wave', color: 'var(--success)' },
    };
    const c = cfg[value] ?? { icon: 'fa-circle-question', color: 'var(--text-muted)' };
    return `<span style="color:${c.color};">
                <i class="fas ${c.icon} me-1"></i>${value ?? '—'}
            </span>`;
}

function pv_referenceFormatter(value) {
    if (!value) return '<span class="text-muted fst-italic">Sin referencia</span>';
    return `<code class="small">${value}</code>`;
}

function pv_actionsFormatter(value, row) {
    return `
        <button class="btn btn-sm btn-success"
                onclick="verifyPayment(
                    '${row.paymentId}',
                    '${row.folio}',
                    '${(row.guestFullName ?? '').replace(/'/g, "\\'")}',
                    ${row.amount ?? 0},
                    '${row.methodLabel ?? ''}'
                )"
                title="Confirmar recepción del pago">
            <i class="fas fa-check me-1"></i>Confirmar
        </button>`;
}