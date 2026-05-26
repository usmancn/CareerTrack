document.addEventListener('DOMContentLoaded', function () {
    var sidebar = document.getElementById('sidebar');
    var toggle = document.querySelector('[data-sidebar-toggle]');
    var mobileQuery = window.matchMedia('(max-width: 768px)');

    if (sidebar && toggle) {
        toggle.addEventListener('click', function () {
            if (mobileQuery.matches) {
                sidebar.classList.toggle('mobile-open');
                sidebar.classList.remove('collapsed');
            } else {
                sidebar.classList.toggle('collapsed');
                sidebar.classList.remove('mobile-open');
            }
        });

        document.addEventListener('click', function (event) {
            if (!mobileQuery.matches || !sidebar.classList.contains('mobile-open')) return;
            if (sidebar.contains(event.target) || toggle.contains(event.target)) return;
            sidebar.classList.remove('mobile-open');
        });
    }

    document.querySelectorAll('.alert.alert-dismissible').forEach(function (alert) {
        setTimeout(function () {
            var bsAlert = bootstrap.Alert.getInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });
});
