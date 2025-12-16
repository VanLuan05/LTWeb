document.addEventListener('DOMContentLoaded', function() {
    loadCart();
});

// Hàm định dạng số thành tiền tệ (VD: 600000 -> 600.000₫)
function formatCurrency(number) {
    return number.toLocaleString('vi-VN') + '₫';
}

// Hàm chính để tải và hiển thị giỏ hàng
function loadCart() {
    var cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    var tableBody = document.querySelector('.cart-table tbody');
    var subtotalValueElements = document.querySelectorAll('.subtotal-value');
    var totalValueElements = document.querySelectorAll('.total-value');
    
    tableBody.innerHTML = ''; // Xóa nội dung cũ
    var overallSubtotal = 0;

    if (cart.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 20px;">Giỏ hàng của bạn đang trống!</td></tr>';
    }

    cart.forEach(function(item) {
        var tempTotal = item.price * item.qty;
        overallSubtotal += tempTotal;

        var row = tableBody.insertRow();
        row.dataset.productId = item.id; // Lưu ID sản phẩm vào hàng
        
        // Cột 1: Sản phẩm (Tên)
        row.insertCell().textContent = item.name;
        
        // Cột 2: Giá
        row.insertCell().textContent = formatCurrency(item.price);
        
        // Cột 3: Số lượng (có thể thêm input để chỉnh sửa sau)
        var qtyCell = row.insertCell();
        qtyCell.innerHTML = `<input type="number" value="${item.qty}" min="1" style="width: 60px; text-align: center;" 
        onchange="updateQuantity('${item.id}', this.value)">`;
        
        // Cột 4: Tạm tính
        var tempTotalCell = row.insertCell();
        tempTotalCell.textContent = formatCurrency(tempTotal);
        tempTotalCell.classList.add('temp-total');

        // Cột 5: Xóa
        var deleteCell = row.insertCell();
        deleteCell.innerHTML = `<button onclick="removeItem('${item.id}')" style="background: none; border: none; color: red; cursor: pointer;">Xóa</button>`;
    });

    // Cập nhật tổng tiền (Shipping = 0đ cho ví dụ này)
    var total = overallSubtotal; 
    
    subtotalValueElements.forEach(el => el.textContent = formatCurrency(overallSubtotal));
    totalValueElements.forEach(el => el.textContent = formatCurrency(total));
}

// Hàm cập nhật số lượng
function updateQuantity(id, newQty) {
    var qty = parseInt(newQty);
    if (qty < 1) return; // Không cho phép số lượng < 1

    var cart = JSON.parse(localStorage.getItem('shoppingCart'));
    var itemIndex = cart.findIndex(item => item.id === id);

    if (itemIndex > -1) {
        cart[itemIndex].qty = qty;
        localStorage.setItem('shoppingCart', JSON.stringify(cart));
        loadCart(); // Tải lại giỏ hàng để cập nhật bảng và tổng tiền
    }
}

// Hàm xóa sản phẩm
function removeItem(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        var cart = JSON.parse(localStorage.getItem('shoppingCart')).filter(item => item.id !== id);
        localStorage.setItem('shoppingCart', JSON.stringify(cart));
        loadCart(); // Tải lại giỏ hàng
    }
}