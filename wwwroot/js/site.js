(function () {
    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function ready(callback) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', callback);
        } else {
            callback();
        }
    }

    function initMotionField() {
        if (reduceMotion) return;

        const shell = document.querySelector('[data-motion-shell]');
        if (!shell) return;

        let rafId = 0;
        let pointerX = window.innerWidth / 2;
        let pointerY = window.innerHeight / 2;

        function applyMotion() {
            const x = ((pointerX / window.innerWidth) - 0.5) * 22;
            const y = ((pointerY / window.innerHeight) - 0.5) * 18;
            shell.style.setProperty('--motion-x', `${x.toFixed(2)}px`);
            shell.style.setProperty('--motion-y', `${y.toFixed(2)}px`);
            rafId = 0;
        }

        window.addEventListener('pointermove', (event) => {
            pointerX = event.clientX;
            pointerY = event.clientY;
            if (!rafId) rafId = window.requestAnimationFrame(applyMotion);
        }, { passive: true });
    }

    function initSidebar() {
        const sidebar = document.querySelector('[data-app-sidebar]');
        const toggle = document.querySelector('[data-sidebar-toggle]');
        const backdrop = document.querySelector('[data-sidebar-backdrop]');
        if (!sidebar || !toggle) return;

        const isMobile = () => window.innerWidth < 992;

        const closeMobileSidebar = () => {
            if (isMobile()) {
                sidebar.classList.remove('mobile-open');
                backdrop?.classList.remove('is-visible');
            }
        };

        toggle.addEventListener('click', () => {
            if (isMobile()) {
                const isOpen = sidebar.classList.toggle('mobile-open');
                backdrop?.classList.toggle('is-visible', isOpen);
            } else {
                document.body.classList.toggle('sidebar-collapsed');
            }
        });

        backdrop?.addEventListener('click', closeMobileSidebar);

        sidebar.querySelectorAll('a.nav-link').forEach((link) => {
            link.addEventListener('click', closeMobileSidebar);
        });
    }

    function initReveal() {
        if (reduceMotion) return;

        const candidates = Array.from(document.querySelectorAll(
            '.page-toolbar, .welcome-banner, .stat-card, .chart-card, .card-section, .table-card, .form-card, .detail-card, .log-card, .posting-card, .empty-state-full, .stagger-item, .stagger-card'
        ));

        candidates.forEach((el, index) => {
            el.classList.add('reveal-on-load');
            el.style.transitionDelay = `${Math.min(index * 42, 360)}ms`;
        });

        if (!('IntersectionObserver' in window)) {
            requestAnimationFrame(() => candidates.forEach((el) => el.classList.add('is-visible')));
            return;
        }

        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (!entry.isIntersecting) return;
                entry.target.classList.add('is-visible');
                observer.unobserve(entry.target);
            });
        }, { threshold: 0.08, rootMargin: '0px 0px -20px 0px' });

        candidates.forEach((el) => observer.observe(el));
    }

    function initCounters() {
        if (reduceMotion) return;

        document.querySelectorAll('.count-anim').forEach((counter) => {
            const target = parseInt(counter.textContent.trim(), 10);
            if (!Number.isFinite(target) || target <= 0) return;

            const duration = Math.min(1100, 520 + target * 16);
            const startTime = performance.now();

            function frame(now) {
                const progress = Math.min((now - startTime) / duration, 1);
                const eased = 1 - Math.pow(1 - progress, 3);
                counter.textContent = Math.round(target * eased).toString();

                if (progress < 1) requestAnimationFrame(frame);
            }

            counter.textContent = '0';
            requestAnimationFrame(frame);
        });
    }

    function initCardMotion() {
        if (reduceMotion || !window.matchMedia('(hover: hover)').matches) return;

        const surfaces = document.querySelectorAll(
            '.stat-card, .chart-card, .card-section, .table-card, .form-card, .detail-card, .log-card, .posting-card, .content-area > .card'
        );

        surfaces.forEach((surface) => {
            surface.addEventListener('pointermove', (event) => {
                const bounds = surface.getBoundingClientRect();
                const x = (event.clientX - bounds.left) / bounds.width;
                const y = (event.clientY - bounds.top) / bounds.height;
                const tiltY = (x - 0.5) * 7;
                const tiltX = (0.5 - y) * 6;

                surface.style.setProperty('--tilt-x', `${tiltX.toFixed(2)}deg`);
                surface.style.setProperty('--tilt-y', `${tiltY.toFixed(2)}deg`);
                surface.classList.add('is-tilting');
            });

            surface.addEventListener('pointerleave', () => {
                surface.classList.remove('is-tilting');
                surface.style.setProperty('--tilt-x', '0deg');
                surface.style.setProperty('--tilt-y', '0deg');
            });
        });
    }

    function initTodoFeedback() {
        if (reduceMotion) return;

        document.querySelectorAll('.btn-toggle, .btn-toggle-done').forEach((button) => {
            button.addEventListener('click', () => {
                const item = button.closest('.todo-list-item');
                if (!item) return;
                item.classList.add('is-completing');
                window.setTimeout(() => item.classList.remove('is-completing'), 460);
            });
        });
    }

    function iconForToast(type) {
        if (type === 'success') return 'bi-check-circle-fill';
        if (type === 'warning') return 'bi-exclamation-circle-fill';
        return 'bi-exclamation-triangle-fill';
    }

    window.showPremiumToast = function (type, message) {
        const container = document.getElementById('premium-toast-container');
        if (!container || !message) return;

        const toast = document.createElement('div');
        toast.className = `premium-toast toast-${type || 'success'}`;
        toast.setAttribute('role', 'status');

        const icon = document.createElement('i');
        icon.className = `bi ${iconForToast(type)} toast-icon`;

        const textArea = document.createElement('div');
        textArea.className = 'toast-text-area';

        const title = document.createElement('div');
        title.className = 'toast-title';
        title.textContent = type === 'error' ? 'Hata' : type === 'warning' ? 'Uyarı' : 'Başarılı';

        const body = document.createElement('div');
        body.className = 'toast-msg';
        body.textContent = message;

        const close = document.createElement('button');
        close.type = 'button';
        close.className = 'btn-toast-close';
        close.setAttribute('aria-label', 'Bildirimi kapat');
        close.innerHTML = '<i class="bi bi-x-lg"></i>';

        textArea.append(title, body);
        toast.append(icon, textArea, close);
        container.appendChild(toast);

        const dismiss = window.setTimeout(() => toast.remove(), 5400);
        close.addEventListener('click', () => {
            window.clearTimeout(dismiss);
            toast.remove();
        });
    };

    window.openPremiumDrawer = function (drawerId) {
        const drawer = document.getElementById(drawerId);
        if (!drawer) return;

        drawer.style.display = 'block';
        requestAnimationFrame(() => drawer.classList.add('is-open'));
        document.body.style.overflow = 'hidden';
    };

    window.closePremiumDrawer = function (drawerId) {
        const drawer = document.getElementById(drawerId);
        if (!drawer) return;

        drawer.classList.remove('is-open');
        document.body.style.overflow = '';

        window.setTimeout(() => {
            if (!drawer.classList.contains('is-open')) {
                drawer.style.display = 'none';
            }
        }, reduceMotion ? 0 : 320);
    };

    function initDrawers() {
        document.addEventListener('keydown', (event) => {
            if (event.key !== 'Escape') return;
            document.querySelectorAll('.premium-drawer.is-open').forEach((drawer) => {
                window.closePremiumDrawer(drawer.id);
            });
        });
    }

    window.togglePassword = function () {
        const input = document.getElementById('passwordInput');
        const icon = document.getElementById('eyeIcon');
        if (!input || !icon) return;

        const show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        icon.classList.toggle('bi-eye', !show);
        icon.classList.toggle('bi-eye-slash', show);
    };

    function initBootstrapWidgets() {
        if (!window.bootstrap) return;

        document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach((el) => {
            window.bootstrap.Tooltip.getOrCreateInstance(el);
        });

        document.querySelectorAll('[data-bs-toggle="pill"]').forEach((el) => {
            window.bootstrap.Tab.getOrCreateInstance(el);
        });
    }

    function roleMeta(role) {
        const map = {
            admin: {
                icon: 'bi-shield-lock-fill',
                caption: 'Admin kontrol akışı hazır'
            },
            school: {
                icon: 'bi-bank2',
                caption: 'Okul onay akışı hazır'
            },
            employer: {
                icon: 'bi-building-fill',
                caption: 'İşveren süreç akışı hazır'
            },
            student: {
                icon: 'bi-mortarboard-fill',
                caption: 'Öğrenci akışı hazır'
            }
        };

        return map[role] || map.student;
    }

    function applyAuthRole(role) {
        const shell = document.querySelector('.login-orchestra');
        if (!shell) return;

        shell.dataset.authRole = role;
        shell.classList.remove('auth-burst');
        requestAnimationFrame(() => shell.classList.add('auth-burst'));

        const meta = roleMeta(role);
        const icon = document.querySelector('#loginRoleIcon i');
        const caption = document.getElementById('loginRoleCaption');

        if (icon) {
            icon.className = `bi ${meta.icon} glow-icon`;
        }
        if (caption) {
            caption.textContent = meta.caption;
        }

        window.setTimeout(() => shell.classList.remove('auth-burst'), reduceMotion ? 0 : 700);
    }

    function initAuthRolePicker() {
        const triggers = document.querySelectorAll('[data-auth-role-trigger]');
        if (!triggers.length) return;

        triggers.forEach((trigger) => {
            trigger.addEventListener('shown.bs.tab', () => {
                applyAuthRole(trigger.dataset.role || 'student');
            });

            trigger.addEventListener('mouseenter', () => {
                if (reduceMotion) return;
                applyAuthRole(trigger.dataset.role || 'student');
            });
        });

        const active = document.querySelector('[data-auth-role-trigger].active');
        if (active) applyAuthRole(active.dataset.role || 'student');
    }

    function initDemoLogin() {
        document.querySelectorAll('.btn-demo[data-email][data-pass]').forEach((button) => {
            button.addEventListener('click', () => {
                const email = document.getElementById('emailInput');
                const password = document.getElementById('passwordInput');
                const form = document.getElementById('loginForm');
                if (!email || !password || !form) return;

                const role = button.dataset.role || 'student';
                applyAuthRole(role);
                button.classList.add('is-pressed');
                email.value = button.dataset.email || '';
                password.value = button.dataset.pass || '';

                if (window.bootstrap && button.matches('[data-bs-toggle="pill"]')) {
                    window.bootstrap.Tab.getOrCreateInstance(button).show();
                }

                const submitDelay = reduceMotion ? 0 : 780;
                window.setTimeout(() => form.submit(), submitDelay);
            });
        });
    }

    function animateNumber(el, target, duration) {
        if (reduceMotion) {
            el.textContent = Math.round(target).toString();
            return;
        }

        const start = performance.now();
        function frame(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            el.textContent = Math.round(target * eased).toString();
            if (progress < 1) requestAnimationFrame(frame);
        }
        el.textContent = '0';
        requestAnimationFrame(frame);
    }

    function initDashboardCharts() {
        const barGroups = document.querySelectorAll('[data-chart-bars]');
        if (barGroups.length || document.querySelector('[data-chart-donut]')) {
            document.documentElement.dataset.chartsReady = 'true';
        }

        barGroups.forEach((group) => {
            const rows = Array.from(group.querySelectorAll('[data-chart-value]'));
            const values = rows.map((row) => Number(row.dataset.chartValue || 0));
            const max = Math.max(1, ...values);

            rows.forEach((row, index) => {
                const value = Number(row.dataset.chartValue || 0);
                const percent = value <= 0 ? 2 : Math.max(8, (value / max) * 100);
                row.style.setProperty('--chart-percent', `${percent.toFixed(2)}%`);
                row.style.setProperty('--chart-delay', `${index * 90}ms`);

                const number = row.querySelector('[data-chart-number]');
                if (number) animateNumber(number, value, 720 + index * 90);
            });

            requestAnimationFrame(() => group.classList.add('chart-ready'));
        });

        document.querySelectorAll('[data-chart-donut]').forEach((donut) => {
            const value = Math.max(0, Number(donut.dataset.chartValue || 0));
            const total = Math.max(1, Number(donut.dataset.chartTotal || 1));
            const targetAngle = Math.min(360, (value / total) * 360);

            if (reduceMotion) {
                donut.style.setProperty('--donut-angle', `${targetAngle.toFixed(2)}deg`);
                return;
            }

            const start = performance.now();
            function frame(now) {
                const progress = Math.min((now - start) / 920, 1);
                const eased = 1 - Math.pow(1 - progress, 3);
                donut.style.setProperty('--donut-angle', `${(targetAngle * eased).toFixed(2)}deg`);
                if (progress < 1) requestAnimationFrame(frame);
            }
            requestAnimationFrame(frame);
        });
    }

    function initInlineActionForms() {
        document.querySelectorAll('.application-status-form').forEach((form) => {
            const select = form.querySelector('select');
            select?.addEventListener('change', () => {
                form.classList.add('is-dirty');
            });

            form.addEventListener('submit', () => {
                form.classList.add('is-submitting');
                const button = form.querySelector('button[type="submit"]');
                if (button) {
                    button.innerHTML = '<i class="bi bi-arrow-repeat me-1"></i>Kaydediliyor';
                }
            });
        });

        document.querySelectorAll('.table-input[form^="company-edit-"]').forEach((input) => {
            input.addEventListener('input', () => {
                input.closest('tr')?.classList.add('is-edited');
            });
        });
    }

    function initRolePicker() {
        const radios = document.querySelectorAll('input[name="Role"]');
        const employerNote = document.getElementById('employerNote');
        const departmentField = document.getElementById('departmentField');
        if (!radios.length || !departmentField) return;

        function applyRole(value) {
            const input = departmentField.querySelector('input');
            const label = departmentField.querySelector('label');
            const icon = departmentField.querySelector('i');

            employerNote?.classList.toggle('d-none', value !== 'Employer');
            departmentField.classList.remove('role-shift');
            requestAnimationFrame(() => departmentField.classList.add('role-shift'));

            if (input) input.placeholder = value === 'Employer' ? 'İnsan Kaynakları / Departman' : 'Bilgisayar Mühendisliği';
            if (label) label.textContent = value === 'Employer' ? 'Departman' : 'Bölüm';
            if (icon) {
                icon.classList.toggle('bi-briefcase', value === 'Employer');
                icon.classList.toggle('bi-mortarboard', value !== 'Employer');
            }
        }

        radios.forEach((radio) => {
            radio.addEventListener('change', () => applyRole(radio.value));
            if (radio.checked) applyRole(radio.value);
        });
    }

    function initFormFocus() {
        document.querySelectorAll('.input-glass-wrapper input, .input-glass-wrapper select, .input-glass-wrapper textarea').forEach((field) => {
            const wrapper = field.closest('.input-glass-wrapper');
            if (!wrapper) return;

            field.addEventListener('focus', () => wrapper.classList.add('is-focused'));
            field.addEventListener('blur', () => wrapper.classList.remove('is-focused'));
        });
    }

    ready(() => {
        initMotionField();
        initSidebar();
        initReveal();
        initCounters();
        initCardMotion();
        initTodoFeedback();
        initDrawers();
        initBootstrapWidgets();
        initAuthRolePicker();
        initDemoLogin();
        initDashboardCharts();
        initInlineActionForms();
        initRolePicker();
        initFormFocus();
    });
})();
