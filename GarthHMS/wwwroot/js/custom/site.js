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
    // ACTIVE NAV ITEM
    // ========================================
    const currentPath = window.location.pathname;
    const navItems = document.querySelectorAll('.nav-item');

    navItems.forEach(item => {
        const href = item.getAttribute('href');
        if (href && currentPath.includes(href)) {
            navItems.forEach(nav => nav.classList.remove('active'));
            item.classList.add('active');
        }
    });

    // ========================================
    // MOBILE SIDEBAR
    // ========================================
    if (window.innerWidth <= 768) {
        if (sidebarToggle && sidebar) {
            sidebarToggle.addEventListener('click', function () {
                sidebar.classList.toggle('show');
            });

            // Cerrar sidebar al hacer click en un link
            navItems.forEach(item => {
                item.addEventListener('click', function () {
                    sidebar.classList.remove('show');
                });
            });
        }
    }
});