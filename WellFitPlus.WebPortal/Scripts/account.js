$(document).ready(function () {
    var tokenKey = 'accessToken';
    var userRole = 'userRole';
    var userID = 'userID';

    $("#messageLink").prop("disabled", true);

    //$(document).on("click", '#submitLogin', function () {
    //    var loginData = {
    //            grant_type: 'password',
    //            username: $("#Email").val(),
    //            password: $("#Password").val()
    //        };

    //    $.ajax({
    //        type: 'POST',
    //        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
              
    //            url: urlBaseAPI + '/token',
    //            data: loginData
    //        }).done(function (data) {
    //            // Cache the access token in session storage.
    //            sessionStorage.setItem(tokenKey, data.access_token);
    //            sessionStorage.setItem(userID, data.guid);
                
    //            // get the users role based on the userid
    //            $.ajax({
    //                type: 'GET',
    //                headers: { 'Authorization': 'Bearer ' + sessionStorage.getItem(tokenKey) },
    //                url: urlBaseAPI + '/api/Account/GetRole?userID=' + sessionStorage.getItem(userID)
                    
    //            }).done(function (data) {
    //                sessionStorage.setItem(userRole, data.role);
    //            }).error(function (data) {
    //                // Cache the access token in session storage.
    //                alert("Badddd");
    //            });
    //            alert("Successful Login");
    //             var url = urlBasePortal + '/video/MainPage';
    //             window.location.href = url;

    //        }).error(function (data) {
    //            // Cache the access token in session storage.
    //            alert("Username or Password is invalid");
    //        });
    //});


    $(document).on("click", '#sendEmail', function () {
        var data = {
            email: $("#Email").val(),
        };

        $.ajax({
            type: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            url: urlBaseAPI + 'api/account/forgotpassword',
            data: data
        }).done(function (data) {
            // Cache the access token in session storage.
            alert("Forgot password link sent")
        }).error(function (data) {
            // Cache the access token in session storage.
            alert("Forgot password link not sent " + data.responseText.valueOf("message"));
        });
    });

});