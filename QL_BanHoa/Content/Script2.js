// /////////////////////////////////////////
// 1. Khai báo biến
// /////////////////////////////////////////
var btnAddToCart = document.getElementById("cart"); // Nút "THÊM VÀO GIỎ"
var quantityInput = document.getElementById("quantityInput"); // Input số lượng

// Lấy các nút +/- và input số lượng (Tùy chọn)
var plusBtn = document.getElementById("plus-btn");
var minusBtn = document.getElementById("minus-btn");


// /////////////////////////////////////////
// 2. Logic chính: Thêm vào Giỏ hàng và Chuyển trang
// /////////////////////////////////////////

if (btnAddToCart) {
    btnAddToCart.addEventListener('click', function() {
        var productId = this.getAttribute('data-product-id');
        var productName = this.getAttribute('data-product-name');
        var productPriceStr = this.getAttribute('data-product-price');
        var quantity = parseInt(quantityInput.value); 
        
        // Chuyển giá từ string sang number (ví dụ: "600000" -> 600000)
        var productPrice = parseInt(productPriceStr); 

        // Gọi hàm thêm vào giỏ hàng và lưu vào LocalStorage
        saveToCart(productId, productName, productPrice, quantity);
        
        // Chuyển hướng người dùng đến trang Giỏ Hàng
        window.location.href = "GioHang.html"; 
    });
}

// Hàm lưu dữ liệu vào Local Storage
function saveToCart(id, name, price, qty) {
    // 1. Lấy dữ liệu giỏ hàng hiện tại (nếu có)
    var cart = JSON.parse(localStorage.getItem('shoppingCart')) || [];
    
    // 2. Kiểm tra xem sản phẩm đã có trong giỏ chưa
    var existingItemIndex = cart.findIndex(item => item.id === id);
    
    if (existingItemIndex > -1) {
        // Nếu đã có, CẬP NHẬT số lượng
        cart[existingItemIndex].qty += qty;
    } else {
        // Nếu chưa có, THÊM mới
        var newItem = {
            id: id,
            name: name,
            price: price,
            qty: qty
            // Bạn có thể thêm hình ảnh, kích cỡ, v.v. vào đây
        };
        cart.push(newItem);
    }
    
    // 3. Lưu lại vào Local Storage
    localStorage.setItem('shoppingCart', JSON.stringify(cart));
    
    console.log("Đã lưu vào Local Storage và sẵn sàng chuyển trang.");
}


// /////////////////////////////////////////
// 3. Bổ sung: Logic cho nút +/- số lượng (Tùy chọn)
// /////////////////////////////////////////
// Giữ lại hoặc thêm logic này để người dùng thay đổi số lượng trước khi thêm
if (plusBtn) {
    plusBtn.onclick = function() {
        var currentValue = parseInt(quantityInput.value);
        quantityInput.value = currentValue + 1;
    }
}

if (minusBtn) {
    minusBtn.onclick = function() {
        var currentValue = parseInt(quantityInput.value);
        if (currentValue > 1) { 
            quantityInput.value = currentValue - 1;
        }
    }
}