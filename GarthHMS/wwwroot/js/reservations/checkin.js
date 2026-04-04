// GarthHMS/wwwroot/js/reservations/checkin.js
// Componente 7 — Check-In
'use strict';

// ============================================================================
// TOGGLE SECCIÓN COLAPSABLE
// ============================================================================

function toggleSection(sectionId, chevronId) {
    const section = document.getElementById(sectionId);
    const chevron = document.getElementById(chevronId);
    if (!section) return;

    const isOpen = section.style.display !== 'none';
    section.style.display = isOpen ? 'none' : '';
    if (chevron) chevron.style.transform = isOpen ? 'rotate(-90deg)' : '';
}

// ============================================================================
// GESTIONAR PAGOS — reutiliza el modal del Componente 5
// ============================================================================

async function openPaymentModal(reservationId) {
    try {
        const response = await fetch(`/Reservations/GetPaymentModal/${reservationId}`);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        const html = await response.text();

        await Swal.fire({
            html,
            width: 520,
            padding: '1.25rem',
            showConfirmButton: false,
            showCloseButton: true,
            didOpen: () => {
                if (typeof pmUpdateForm === 'function') pmUpdateForm();
            }
        });

        // Recargar la página para reflejar el nuevo pago
        window.location.reload();

    } catch (err) {
        console.error('Error al abrir modal de pagos:', err);
        showErrorToast('Error al abrir el gestor de pagos');
    }
}

// ============================================================================
// CONFIRMAR CHECK-IN
// ============================================================================

async function confirmCheckin() {
    const { reservationId, hasEmail, requireCompanions,
        companionsOptional, folio, guestName } = window.CheckInData;

    // ── Validar email si no tiene ──────────────────────────────────────────
    let guestEmail = null;
    if (!hasEmail) {
        const emailInput = document.getElementById('guestEmail');
        const emailVal = emailInput?.value?.trim() ?? '';

        if (!emailVal) {
            emailInput?.classList.add('is-invalid');
            showErrorToast('El correo electrónico es requerido para el check-in');
            emailInput?.focus();
            return;
        }

        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(emailVal)) {
            emailInput?.classList.add('is-invalid');
            showErrorToast('El formato del correo electrónico no es válido');
            emailInput?.focus();
            return;
        }

        emailInput?.classList.remove('is-invalid');
        guestEmail = emailVal;
    }

    // ── Validar acompañantes si son obligatorios ───────────────────────────
    if (requireCompanions && !companionsOptional) {
        const groups = document.querySelectorAll('.companion-group');
        let missingName = false;

        groups.forEach(group => {
            group.querySelectorAll('.companion-row').forEach(row => {
                const nameInput = row.querySelector('.companion-name');
                if (nameInput && !nameInput.value.trim()) {
                    nameInput.classList.add('is-invalid');
                    missingName = true;
                } else {
                    nameInput?.classList.remove('is-invalid');
                }
            });
        });

        if (missingName) {
            showErrorToast('Completa el nombre de todos los acompañantes');
            return;
        }
    }

    // ── Recopilar placas ───────────────────────────────────────────────────
    const vehiclePlates = [];
    document.querySelectorAll('.vehicle-plate-input').forEach(input => {
        const roomId = input.dataset.roomId;
        const plate = input.value.trim();
        const desc = document.querySelector(
            `.vehicle-desc-input[data-room-id="${roomId}"]`)?.value.trim() || null;
        if (roomId) {
            vehiclePlates.push({ roomId, plate: plate || null, description: desc });
        }
    });

    // ── Recopilar acompañantes ─────────────────────────────────────────────
    const companions = [];
    document.querySelectorAll('.companion-group').forEach(group => {
        const rrId = group.dataset.reservationRoomId;
        const rows = [];

        group.querySelectorAll('.companion-row').forEach(row => {
            const fullName = row.querySelector('.companion-name')?.value.trim() ?? '';
            if (!fullName) return;
            rows.push({
                fullName,
                age: row.querySelector('.companion-age')?.value.trim() ?? '',
                phone: row.querySelector('.companion-phone')?.value.trim() ?? '',
                relationship: row.querySelector('.companion-relationship')?.value.trim() ?? ''
            });
        });

        if (rows.length > 0) {
            companions.push({ reservationRoomId: rrId, companions: rows });
        }
    });

    // ── Confirmación SweetAlert2 ───────────────────────────────────────────
    const balanceEl = document.querySelector('.text-danger.fw-bold.fs-5');
    const hasBalance = !!balanceEl;

    const confirm = await Swal.fire({
        title: '¿Confirmar Check-In?',
        html: `
            <div class="text-start">
                <div class="d-flex flex-column gap-2">
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Folio</span>
                        <strong>${folio}</strong>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Huésped</span>
                        <strong>${guestName}</strong>
                    </div>
                    ${guestEmail ? `
                    <div class="d-flex justify-content-between">
                        <span class="text-muted">Correo</span>
                        <strong>${guestEmail}</strong>
                    </div>` : ''}
                </div>
                ${hasBalance ? `
                <div class="alert alert-warning mt-3 py-2 small mb-0">
                    <i class="fas fa-exclamation-triangle me-1"></i>
                    El huésped tiene saldo pendiente. El check-in quedará registrado con saldo por cobrar.
                </div>` : ''}
            </div>`,
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: '<i class="fas fa-key me-1"></i>Sí, confirmar Check-In',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: 'var(--success)',
        cancelButtonColor: 'var(--text-muted)',
        reverseButtons: true
    });

    if (!confirm.isConfirmed) return;

    // ── Enviar ─────────────────────────────────────────────────────────────
    const btn = document.getElementById('btnConfirmCheckin');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-circle-notch fa-spin me-2"></i>Procesando…';
    }

    try {
        const res = await fetch('/Reservations/CheckIn', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                reservationId: reservationId,
                guestEmail: guestEmail,
                vehiclePlates: vehiclePlates,
                companions: companions
            })
        });
        const data = await res.json();

        if (!data.success) {
            console.error('Error en check-in:', data.message);
            showErrorToast(data.message || 'Error al realizar el check-in');
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-key me-2"></i>Confirmar Check-In';
            }
            return;
        }

        // ── Éxito ──────────────────────────────────────────────────────────
        await Swal.fire({
            title: '¡Check-In Completado!',
            html: `
                <div class="text-center py-2">
                    <div class="mb-3" style="font-size:3.5rem;color:var(--success);">
                        <i class="fas fa-check-circle"></i>
                    </div>
                    <p class="mb-1 fs-5 fw-semibold">${guestName}</p>
                    <p class="text-muted mb-0">Folio <strong>${folio}</strong> — Bienvenido al hotel</p>
                </div>`,
            icon: null,
            confirmButtonText: '¡Listo!',
            confirmButtonColor: 'var(--success)'
        });

        window.location.href = '/Availability';

    } catch (error) {
        console.error('Error de conexión en check-in:', error);
        showErrorToast('Error de conexión. Intenta nuevamente.');
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-key me-2"></i>Confirmar Check-In';
        }
    }
}

// ============================================================================
// FORMULARIO DE PAGOS — lógica del modal (réplica de pmUpdateForm)
// Necesaria porque availability.js no se carga en esta página
// ============================================================================

function pmUpdateForm() {
    const typeEl = document.getElementById('pm-type');
    const methodEl = document.getElementById('pm-method');
    if (!typeEl || !methodEl) return;

    const type = typeEl.value;
    const method = methodEl.value;

    const amountWrap = document.getElementById('pm-amount-wrap');
    const amountInput = document.getElementById('pm-amount');
    const refWrap = document.getElementById('pm-reference-wrap');
    const refInput = document.getElementById('pm-reference');
    const hint = document.getElementById('pm-amount-hint');

    const maxBalance = parseFloat(amountInput?.getAttribute('max') || '0');

    // ── Monto ──────────────────────────────────────────────────────
    if (type === 'full') {
        if (amountWrap) amountWrap.style.display = 'none';
        if (amountInput) amountInput.value = maxBalance.toFixed(2);
    } else if (type === 'refund') {
        if (amountWrap) amountWrap.style.display = 'block';
        if (amountInput) { amountInput.removeAttribute('max'); amountInput.value = ''; }
        if (hint) hint.textContent = 'Ingresa el monto a devolver';
    } else {
        if (amountWrap) amountWrap.style.display = 'block';
        if (amountInput) { amountInput.setAttribute('max', maxBalance); amountInput.value = ''; }
        if (hint) hint.textContent = `Máximo: $${maxBalance.toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN`;
    }

    // ── Referencia ─────────────────────────────────────────────────
    if (method !== 'cash') {
        if (refWrap) refWrap.style.display = 'block';
    } else {
        if (refWrap) refWrap.style.display = 'none';
        if (refInput) refInput.value = '';
    }
}

async function submitPayment(reservationId) {
    const type = document.getElementById('pm-type')?.value;
    const method = document.getElementById('pm-method')?.value;
    const amountRaw = document.getElementById('pm-amount')?.value;
    const reference = document.getElementById('pm-reference')?.value?.trim();

    const amount = parseFloat(amountRaw);
    if (!amountRaw || isNaN(amount) || amount <= 0) {
        Swal.showValidationMessage('Ingresa un monto válido mayor a cero');
        return;
    }

    const maxInput = document.getElementById('pm-amount')?.getAttribute('max');
    if (maxInput && type !== 'refund' && amount > parseFloat(maxInput)) {
        Swal.showValidationMessage(
            `El monto excede el saldo pendiente ($${parseFloat(maxInput)
                .toLocaleString('es-MX', { minimumFractionDigits: 2 })} MXN)`);
        return;
    }

    if (method !== 'cash' && !reference) {
        Swal.showValidationMessage('La referencia es obligatoria para transferencia y tarjeta');
        return;
    }

    const btn = document.getElementById('pm-submit-btn');
    if (btn) {
        btn.disabled = true;
        btn.innerHTML = '<i class="fas fa-circle-notch fa-spin me-1"></i>Registrando…';
    }

    try {
        const res = await fetch('/Reservations/AddPayment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                reservationId,
                amount,
                paymentMethod: method,
                paymentType: type,
                reference: reference || null
            })
        });
        const data = await res.json();

        if (!data.success) {
            Swal.showValidationMessage(data.message || 'Error al registrar el pago');
            if (btn) {
                btn.disabled = false;
                btn.innerHTML = '<i class="fas fa-check me-1"></i>Registrar pago';
            }
            return;
        }

        Swal.close();
        window.location.reload();

    } catch (err) {
        console.error('Error al registrar pago:', err);
        Swal.showValidationMessage('Error de conexión. Intenta nuevamente.');
        if (btn) {
            btn.disabled = false;
            btn.innerHTML = '<i class="fas fa-check me-1"></i>Registrar pago';
        }
    }
}