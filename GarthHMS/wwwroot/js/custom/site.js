// ========================================
// THEME TOGGLE (Dark/Light Mode)
// ========================================
document.addEventListener('DOMContentLoaded', function () {

    // Cargar tema guardado
    const savedTheme = localStorage.getItem('theme') || 'light';
    document.documentElement.setAttribute('data-theme', savedTheme);
    updateThemeIcon(savedTheme);

    // Toggle tema
    const themeToggle = document.getElementById('themeToggle');
    if (themeToggle) {
        themeToggle.addEventListener('click', function () {
            const currentTheme = document.documentElement.getAttribute('data-theme');
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';

            document.documentElement.setAttribute('data-theme', newTheme);
            localStorage.setItem('theme', newTheme);
            updateThemeIcon(newTheme);
        });
    }

    function updateThemeIcon(theme) {
        const icon = document.querySelector('#themeToggle i');
        if (icon) {
            if (theme === 'light') {
                icon.className = 'fas fa-moon';
            } else {
                icon.className = 'fas fa-sun';
            }
        }
    }

    // ========================================
    // SIDEBAR TOGGLE
    // ========================================
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');

            // Guardar estado
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('sidebarCollapsed', isCollapsed);
        });

        // Cargar estado guardado
        const savedState = localStorage.getItem('sidebarCollapsed');
        if (savedState === 'true') {
            sidebar.classList.add('collapsed');
        }
    }

    // ========================================
    // USER DROPDOWN
    // ========================================
    const userMenuBtn = document.getElementById('userMenuBtn');
    const userMenu = document.getElementById('userMenu');

    if (userMenuBtn && userMenu) {
        userMenuBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            userMenu.classList.toggle('show');
        });

        // Cerrar al hacer click fuera
        document.addEventListener('click', function (e) {
            if (!userMenu.contains(e.target) && !userMenuBtn.contains(e.target)) {
                userMenu.classList.remove('show');
            }
        });
    }

    // ========================================
    // ACTIVE NAV LINK
    // ========================================
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.nav-item');

    navLinks.forEach(link => {
        if (link.getAttribute('href') === currentPath) {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });
});


/* ============================================ */
/* SUBMENU EXPANDIBLE - CONFIGURACIÓN */
/* ============================================ */

document.addEventListener('DOMContentLoaded', function () {
    initConfigSubmenu();
});

/**
 * Inicializa el submenu de configuración
 */
function initConfigSubmenu() {
    const configMenu = document.getElementById('configMenu');
    const configSubmenu = document.getElementById('configSubmenu');

    if (!configMenu || !configSubmenu) {
        console.warn('Submenu de configuración no encontrado en el DOM');
        return;
    }

    // Restaurar estado del submenu desde localStorage
    const isExpanded = localStorage.getItem('configSubmenuExpanded') === 'true';
    if (isExpanded) {
        expandSubmenu(configMenu, configSubmenu);
    }

    // Toggle submenu al hacer clic
    configMenu.addEventListener('click', function (e) {
        e.preventDefault();
        toggleSubmenu(configMenu, configSubmenu);
    });

    // Marcar item activo basado en la URL actual
    markActiveSubmenuItem();

    // Auto-expandir si estamos en una vista de configuración
    autoExpandIfActive(configMenu, configSubmenu);
}

/**
 * Toggle expand/collapse del submenu
 */
function toggleSubmenu(menuItem, submenu) {
    const isExpanded = submenu.classList.contains('expanded');

    if (isExpanded) {
        collapseSubmenu(menuItem, submenu);
    } else {
        expandSubmenu(menuItem, submenu);
    }
}

/**
 * Expandir submenu
 */
function expandSubmenu(menuItem, submenu) {
    menuItem.classList.add('expanded');
    submenu.classList.add('expanded');
    localStorage.setItem('configSubmenuExpanded', 'true');
}

/**
 * Colapsar submenu
 */
function collapseSubmenu(menuItem, submenu) {
    menuItem.classList.remove('expanded');
    submenu.classList.remove('expanded');
    localStorage.setItem('configSubmenuExpanded', 'false');
}

/**
 * Marca el item activo del submenu basado en la URL
 */
function markActiveSubmenuItem() {
    const currentPath = window.location.pathname;
    const submenuItems = document.querySelectorAll('.submenu-item');

    submenuItems.forEach(item => {
        const itemPath = item.getAttribute('href');

        // Si la URL actual coincide con el href del item
        if (currentPath.includes(itemPath) && itemPath !== '#') {
            item.classList.add('active');

            // También marcar el nav-item padre
            const parentNavItem = document.getElementById('configMenu');
            if (parentNavItem) {
                parentNavItem.classList.add('active');
            }
        } else {
            item.classList.remove('active');
        }
    });
}

/**
 * Auto-expandir el submenu si estamos en alguna vista de configuración
 */
function autoExpandIfActive(menuItem, submenu) {
    const currentPath = window.location.pathname.toLowerCase();

    // Lista de rutas que pertenecen a configuración
    const configRoutes = [
        '/hotelsettings',
        '/roomtypes',
        '/rooms',
        '/users',
        '/roles',
        '/hourpackages'
    ];

    // Si estamos en alguna ruta de configuración
    const isInConfigRoute = configRoutes.some(route => currentPath.includes(route.toLowerCase()));

    if (isInConfigRoute) {
        expandSubmenu(menuItem, submenu);
    }
}


function closeOtherSubmenus(currentSubmenu) {
    const allSubmenus = document.querySelectorAll('.submenu');
    const allMenuItems = document.querySelectorAll('.nav-item.has-submenu');
    
    allSubmenus.forEach((submenu, index) => {
        if (submenu !== currentSubmenu && submenu.classList.contains('expanded')) {
            collapseSubmenu(allMenuItems[index], submenu);
        }
    });
}
