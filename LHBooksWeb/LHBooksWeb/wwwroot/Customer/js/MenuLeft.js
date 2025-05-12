document.addEventListener('DOMContentLoaded', function () {
    const viewMoreBtn = document.getElementById('viewMoreCategories');
    const allItems = document.querySelectorAll('.limited-list > li:not(.view-more-container)');
    let isExpanded = localStorage.getItem('menuExpanded') === 'true';

    if (isExpanded) {
        expandMenu();
    } else {
        collapseMenu();
    }

    if (allItems.length <= 10) {
        viewMoreBtn.style.display = 'none';
    }

    viewMoreBtn.addEventListener('click', function (e) {
        e.preventDefault();
        if (isExpanded) {
            collapseMenu();
        } else {
            expandMenu();
        }
    });

    function expandMenu() {
        allItems.forEach(item => item.classList.remove('hidden-item'));
        viewMoreBtn.innerHTML = 'Thu Gọn <i class="fa fa-angle-up"></i>';
        isExpanded = true;
        localStorage.setItem('menuExpanded', 'true');
    }

    function collapseMenu() {
        allItems.forEach((item, index) => {
            if (index >= 10) {
                item.classList.add('hidden-item');
            } else {
                item.classList.remove('hidden-item');
            }
        });
        viewMoreBtn.innerHTML = 'Xem Thêm <i class="fa fa-angle-down"></i>';
        isExpanded = false;
        localStorage.setItem('menuExpanded', 'false');
    }
});
