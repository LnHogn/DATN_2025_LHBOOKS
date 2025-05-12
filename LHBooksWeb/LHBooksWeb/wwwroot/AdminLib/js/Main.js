

//document.addEventListener("DOMContentLoaded", function () {
//    const currentPath = window.location.pathname; // Đường dẫn hiện tại không chuyển sang chữ thường

//    document.querySelectorAll(".nav-item").forEach(item => {
//        const link = item.querySelector(":scope > a"); // Lấy thẻ <a> cấp đầu tiên
//        if (!link) return;

//        const subMenuLinks = item.querySelectorAll(".collapse-item"); // Lấy menu con
//        let isActive = false;

//        // Kiểm tra nếu bất kỳ menu con nào khớp với đường dẫn hiện tại
//        subMenuLinks.forEach(subLink => {
//            const subHref = subLink.getAttribute("href");
//            if (subHref && currentPath.startsWith(subHref.replace(/\/index$/, ""))) {
//                isActive = true;
//                subLink.classList.add("active");
//            }
//        });

//        // Nếu đường dẫn hiện tại chứa đường dẫn menu chính
//        const menuHref = link.getAttribute("href");
//        if (menuHref && currentPath.startsWith(menuHref.replace(/\/index$/, ""))) {
//            isActive = true;
//        }

//        // Nếu có menu con nào trùng URL -> active menu cha + mở rộng sub-menu
//        if (isActive) {
//            item.classList.add("active"); // Đánh dấu active menu cha
//            const collapseMenu = item.querySelector(".collapse");
//            if (collapseMenu) {
//                collapseMenu.classList.add("show"); // Mở rộng menu con
//            }
//        }
//    });
//});
