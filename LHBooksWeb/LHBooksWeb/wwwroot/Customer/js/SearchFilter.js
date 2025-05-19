$(document).ready(function () {
    const $sortText = $('.type_sorting_text');
    const $sortButtons = $('.type_sorting_btn');
    const $publisherText = $('.publisher_filter_text');
    const $publisherButtons = $('.publisher_filter_btn');
    const currentUrl = window.location.pathname;

    // 1. Gán sự kiện click để lọc theo kiểu sắp xếp
    $sortButtons.on('click', function () {
        const sortBy = $(this).data('sort');
        const selectedText = $(this).find('span').text();
        // Cập nhật tiêu đề
        $sortText.text(selectedText);
        // Lưu vào localStorage
        localStorage.setItem('selectedSort', sortBy);
        localStorage.setItem('selectedSortText', selectedText);
        // Gọi hàm filter để áp dụng bộ lọc
        applyFilters();
    });

    // 2. Gán sự kiện click để lọc theo nhà xuất bản
    $publisherButtons.on('click', function () {
        const publisherId = $(this).data('publisher');
        const selectedText = $(this).find('span').text();
        // Cập nhật tiêu đề
        $publisherText.text(selectedText);
        // Lưu vào localStorage
        localStorage.setItem('selectedPublisher', publisherId);
        localStorage.setItem('selectedPublisherText', selectedText);
        // Gọi hàm filter để áp dụng bộ lọc
        applyFilters();
    });

    // 3. Hàm chung để áp dụng tất cả bộ lọc
    function applyFilters() {
        const sortBy = localStorage.getItem('selectedSort') || 'default';
        const publisherId = localStorage.getItem('selectedPublisher') || '0';

        $.ajax({
            url: currentUrl,
            type: 'GET',
            data: {
                sort: sortBy,
                publisherId: publisherId
            },
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (res) {
                $('#product-list').html(res);
                bindAddToCart();
            },
            error: function () {
                alert("Không thể áp dụng bộ lọc.");
            }
        });
    }

    function bindAddToCart() {
        $('.btnAddToCart').off('click').on('click', function (e) {
            e.preventDefault();

            const button = $(this);
            const productId = parseInt(button.data('id'));
            const productName = button.data('name');
            const productImage = button.data('image');
            const price = parseFloat(button.data('price'));
            const quantity = parseInt(button.data('quantity'));

            $.ajax({
                url: '/Cart/AddToCart',
                type: 'POST',
                data: {
                    productId,
                    productName,
                    productImage,
                    price,
                    quantity
                },
                success: function (res) {
                    if (res.success) {
                        toastr.success(res.message);

                        // Gọi API GetCartItemCount để lấy số lượng sản phẩm trong giỏ hàng
                        $.ajax({
                            url: '/Cart/GetCartItemCount',
                            type: 'GET',
                            success: function (cartItemCount) {
                                // Cập nhật số lượng giỏ hàng trong phần tử #checkout_items
                                $('#checkout_items').text(cartItemCount);
                            },
                            error: function (err) {
                                toastr.error("Lỗi khi lấy số lượng giỏ hàng");
                            }
                        });
                    } else {
                        toastr.error(res.message);
                    }
                },
                error: function (xhr) {
                    if (xhr.status === 401) {
                        toastr.warning("Bạn cần đăng nhập để thêm sản phẩm.");
                        setTimeout(() => {
                            window.location.href = '/Account/Login';
                        }, 1500);
                    } else {
                        let errorMsg = "Đã xảy ra lỗi khi gửi yêu cầu.";
                        if (xhr.responseJSON && xhr.responseJSON.message) {
                            errorMsg = xhr.responseJSON.message;
                        }
                        toastr.error(errorMsg);
                    }
                }
            });
        });
    }

    // 4. Khi trang được tải lại hoặc quay lại từ trang khác
    const savedSort = localStorage.getItem('selectedSort');
    const savedSortText = localStorage.getItem('selectedSortText');
    const savedPublisher = localStorage.getItem('selectedPublisher');
    const savedPublisherText = localStorage.getItem('selectedPublisherText');

    // Khôi phục trạng thái sắp xếp
    if (savedSort && savedSortText) {
        $sortText.text(savedSortText);
    }

    // Khôi phục trạng thái nhà xuất bản
    if (savedPublisher && savedPublisherText) {
        $publisherText.text(savedPublisherText);
    }

    // Áp dụng lại bộ lọc khi tải trang
    if ((savedSort && savedSortText) || (savedPublisher && savedPublisherText)) {
        applyFilters();
    }

    bindAddToCart();
});