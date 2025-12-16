document.addEventListener("DOMContentLoaded", function () {
    var defaultUsers = [
        { name: "minhan", email: "minhan@gmail.com", password: "123456" },
        { name: "hungvi", email: "hungvi@gmail.com", password: "123456" }
    ];
    if (!localStorage.getItem("users")) {
        localStorage.setItem("users", JSON.stringify(defaultUsers));
    }

    // ======== XỬ LÝ FORM ĐĂNG KÝ ========
    var registerForm = document.querySelector(".register-form form");
    if (registerForm) {
        registerForm.addEventListener("submit", function (e) {
            e.preventDefault();
            var name = document.getElementById("ten").value.trim();
            var email = document.getElementById("email").value.trim();
            var pw = document.getElementById("mat_khau").value;
            var pw2 = document.getElementById("xac_nhan_mat_khau").value;
            var agree = document.querySelector('input[name="dieu_khoan"]').checked;
            if (name === "" || email === "" || pw === "" || pw2 === "") {
                alert("Vui lòng điền đầy đủ thông tin!");
                return;
            }

            var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            if (!emailPattern.test(email)) {
                alert("Địa chỉ email không hợp lệ!");
                return;
            }

            if (pw.length < 6) {
                alert("Mật khẩu phải có ít nhất 6 ký tự!");
                return;
            }

            if (pw !== pw2) {
                alert("Mật khẩu xác nhận không trùng khớp!");
                return;
            }

            if (!agree) {
                alert("Vui lòng đồng ý với Điều khoản & Dịch vụ!");
                return;
            }
            var users = JSON.parse(localStorage.getItem("users")) || [];
            var existed = false;
            for (var i = 0; i < users.length; i++) {
                if (users[i].email === email) {
                    existed = true;
                    break;
                }
            }

            if (existed) {
                alert("Email này đã được đăng ký!");
                return;
            }
            users.push({ name: name, email: email, password: pw });
            localStorage.setItem("users", JSON.stringify(users));

            alert("Đăng ký thành công! Bạn sẽ được chuyển đến trang đăng nhập.");
            window.location.href = "./doan.html";
        });
    }

    // ======== XỬ LÝ FORM ĐĂNG NHẬP ========
    var loginForm = document.querySelector(".login-form form");
    if (loginForm) {
        loginForm.addEventListener("submit", function (e) {
            e.preventDefault();

            var usernameOrEmail = document.querySelector('input[type="text"]').value.trim();
            var password = document.querySelector('input[type="password"]').value;

            if (usernameOrEmail === "" || password === "") {
                alert("Vui lòng nhập đầy đủ thông tin!");
                return;
            }
            var users = JSON.parse(localStorage.getItem("users")) || [];
            var found = null;
            for (var i = 0; i < users.length; i++) {
                var u = users[i];
                if ((u.email === usernameOrEmail || u.name === usernameOrEmail) && u.password === password) {
                    found = u;
                    break;
                }
            }

            if (found) {
                alert("Đăng nhập thành công! Đang chuyển đến trang chủ...");
                localStorage.setItem("currentUser", JSON.stringify(found));
                window.location.href = "index.html";
            } else {
                alert("Sai thông tin đăng nhập! Vui lòng kiểm tra lại.");
            }
        });
    }
    var welcomeBox = document.getElementById("welcome-box");
    if (welcomeBox) {
    var currentUser = JSON.parse(localStorage.getItem("currentUser"));
        if (currentUser) {
           
            welcomeBox.innerHTML =
                "<h2>Xin chào, " + currentUser.name + "!</h2>" +
                "<p>Email: " + currentUser.email + "</p>" +
                '<button id="logoutBtn">Đăng xuất</button>';
            document.getElementById("logoutBtn").addEventListener("click", function () {
                localStorage.removeItem("currentUser");
                alert("Đăng xuất thành công!");
                window.location.href = "đồ án.html";
            });
        } else {
              window.location.href = "đồ án.html";
        }
    }

});