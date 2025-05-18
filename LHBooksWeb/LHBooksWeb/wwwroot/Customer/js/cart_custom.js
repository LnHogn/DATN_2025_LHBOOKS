document.addEventListener('DOMContentLoaded', function() {
    $.ajax({
        url: '/Cart/GetCartItemCount',
        type: 'GET',
        success: function(cartItemCount) {
            // Cập nhật số lượng giỏ hàng trong phần tử #checkout_items
            $('#checkout_items').text(cartItemCount);
        },
        error: function(err) {
            toastr.error("Lỗi khi lấy số lượng giỏ hàng");
        }
    });

    $('.btnAddToCart').click(function (e) {
        e.preventDefault();

        const button = $(this);
        const productId = parseInt(button.data('id'));
        const productName = button.data('name');
        const productImage = button.data('image');
        const price = parseFloat(button.data('price'));
        const quantity = parseInt(button.data('quantity'));
        const isSelected = false;

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: {
                productId,
                productName,
                productImage,
                price,
                quantity,
                isSelected
            },
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);

                    // Gọi API GetCartItemCount để lấy số lượng sản phẩm trong giỏ hàng
                    $.ajax({
                        url: '/Cart/GetCartItemCount',
                        type: 'GET',
                        success: function(cartItemCount) {
                            // Cập nhật số lượng giỏ hàng trong phần tử #checkout_items
                            $('#checkout_items').text(cartItemCount);
                        },
                        error: function(err) {
                            toastr.error("Lỗi khi lấy số lượng giỏ hàng");
                        }
                    });
                } else {
                    toastr.error("Thêm thất bại");
                }
            },
            error: function (xhr) {
    if (xhr.status === 401) {
        toastr.warning("Bạn cần đăng nhập để thêm sản phẩm.");
        setTimeout(() => {
            window.location.href = '/Account/Login';
        }, 1500);
    } else {
        toastr.error("Thêm thất bại");
    }
}
        });
    });

    $('.btnBuyNow').click(function (e) {
        e.preventDefault();

        const button = $(this);
        const productId = parseInt(button.data('id'));
        const productName = button.data('name');
        const productImage = button.data('image');
        const price = parseFloat(button.data('price'));
        const quantity = parseInt(button.data('quantity'));
        const isSelected = true;

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: {
                productId,
                productName,
                productImage,
                price,
                quantity,
                isSelected
            },
            success: function (res) {
                if (res.success) {
                    // Sau khi thêm vào giỏ, chuyển sang trang giỏ hàng
                    window.location.href = '/Cart/Index';
                } else {
                    toastr.error("Thêm thất bại");
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    toastr.warning("Bạn cần đăng nhập để mua sản phẩm.");
                    setTimeout(() => {
                        window.location.href = '/Account/Login';
                    }, 1500);
                } else {
                    toastr.error("Thêm thất bại");
                }
            }
        });
    });

});
