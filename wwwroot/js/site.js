/**
 * CareerTrack — site.js
 * Premium interactive script layer utilizing Anime.js for transitions, 
 * side-drawers, custom toast systems, and stagger layout entries.
 */

document.addEventListener('DOMContentLoaded', function () {
    // ════════════════════════════════════════════════
    // 1. SIDEBAR & LAYOUT COLLAPSE
    // ════════════════════════════════════════════════
    const sidebar = document.getElementById('sidebar');
    const toggle = document.querySelector('[data-sidebar-toggle]');
    const mobileQuery = window.matchMedia('(max-width: 768px)');

    if (sidebar && toggle) {
        toggle.addEventListener('click', function () {
            if (mobileQuery.matches) {
                sidebar.classList.toggle('mobile-open');
                sidebar.classList.remove('collapsed');
            } else {
                sidebar.classList.toggle('collapsed');
                sidebar.classList.remove('mobile-open');
                
                // Animate content shift slightly for ultra-smoothness
                const mainContent = document.querySelector('.main-content');
                if (mainContent) {
                    anime({
                        targets: mainContent,
                        marginLeft: sidebar.classList.contains('collapsed') ? '0px' : '260px',
                        duration: 350,
                        easing: 'easeOutQuint'
                    });
                }
            }
        });

        document.addEventListener('click', function (event) {
            if (!mobileQuery.matches || !sidebar.classList.contains('mobile-open')) return;
            if (sidebar.contains(event.target) || toggle.contains(event.target)) return;
            sidebar.classList.remove('mobile-open');
        });
    }

    // ════════════════════════════════════════════════
    // 2. DASHBOARD ANIMATIONS (Staggers & Counters)
    // ════════════════════════════════════════════════
    // Staggered load for cards
    const staggerCards = document.querySelectorAll('.stagger-card, .stat-card, .list-item, .log-card');
    if (staggerCards.length > 0) {
        anime({
            targets: staggerCards,
            translateY: [20, 0],
            opacity: [0, 1],
            delay: anime.stagger(60, { start: 100 }),
            duration: 800,
            easing: 'easeOutQuart'
        });
    }

    // Counter anim for stats
    const counterElements = document.querySelectorAll('.count-anim');
    counterElements.forEach(el => {
        const targetValue = parseInt(el.textContent.trim(), 10) || 0;
        el.textContent = '0';
        
        const countObj = { value: 0 };
        anime({
            targets: countObj,
            value: targetValue,
            round: 1,
            duration: 1200,
            easing: 'easeOutExpo',
            update: function() {
                el.textContent = countObj.value;
            }
        });
    });

    // ════════════════════════════════════════════════
    // 3. TOAST SYSTEM
    // ════════════════════════════════════════════════
    window.showPremiumToast = function(type, message) {
        const container = document.getElementById('premium-toast-container');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `premium-toast toast-${type}`;
        
        const icon = type === 'success' ? 'bi-check-circle-fill' : 'bi-exclamation-triangle-fill';
        
        toast.innerHTML = `
            <div class="toast-body-content">
                <i class="bi ${icon} toast-icon"></i>
                <div class="toast-text-area">
                    <div class="toast-title">${type.toUpperCase()}</div>
                    <div class="toast-msg">${message}</div>
                </div>
                <button type="button" class="btn-toast-close"><i class="bi bi-x"></i></button>
            </div>
            <div class="toast-progress"></div>
        `;

        container.appendChild(toast);

        // Slide-in animation
        anime({
            targets: toast,
            translateX: [150, 0],
            opacity: [0, 1],
            scale: [0.9, 1],
            duration: 400,
            easing: 'easeOutBack'
        });

        // Progress bar visual countdown
        const progressBar = toast.querySelector('.toast-progress');
        anime({
            targets: progressBar,
            width: ['100%', '0%'],
            duration: 5000,
            easing: 'linear'
        });

        // Auto close after 5 seconds
        const dismissTimeout = setTimeout(() => {
            closeToast(toast);
        }, 5000);

        // Manual close action
        toast.querySelector('.btn-toast-close').addEventListener('click', () => {
            clearTimeout(dismissTimeout);
            closeToast(toast);
        });
    };

    function closeToast(toast) {
        anime({
            targets: toast,
            translateX: 150,
            opacity: 0,
            scale: 0.9,
            duration: 350,
            easing: 'easeInBack',
            complete: function() {
                toast.remove();
            }
        });
    }

    // ════════════════════════════════════════════════
    // 4. GLASS SLIDING DRAWERS
    // ════════════════════════════════════════════════
    window.openPremiumDrawer = function(drawerId) {
        const drawer = document.getElementById(drawerId);
        if (!drawer) return;

        // Ensure visible display
        drawer.style.display = 'block';

        // Blur layout/backdrop overlay animation
        const overlay = drawer.querySelector('.drawer-overlay');
        if (overlay) {
            anime({
                targets: overlay,
                opacity: [0, 1],
                duration: 300,
                easing: 'easeOutQuad'
            });
        }

        // Drawer sliding from right
        const content = drawer.querySelector('.drawer-content');
        if (content) {
            anime({
                targets: content,
                translateX: ['100%', '0%'],
                opacity: [0, 1],
                duration: 450,
                easing: 'easeOutQuint'
            });
        }
    };

    window.closePremiumDrawer = function(drawerId) {
        const drawer = document.getElementById(drawerId);
        if (!drawer) return;

        const overlay = drawer.querySelector('.drawer-overlay');
        const content = drawer.querySelector('.drawer-content');

        // Slide content back to right
        anime({
            targets: content,
            translateX: '100%',
            opacity: 0,
            duration: 350,
            easing: 'easeInQuint'
        });

        // Fade backdrop overlay out
        anime({
            targets: overlay,
            opacity: 0,
            duration: 300,
            easing: 'easeInQuad',
            complete: function() {
                drawer.style.display = 'none';
            }
        });
    };
});
