﻿const attendance = (function() {
    const baseUrl = "http://localhost:19362";

    //create new user
    const registerNewDeviceUser = function () {
        var isSend = false;
        const index = $("[id*=School_DropDownList]").get(0).selectedIndex;
        const password = $('[id*=Password_TextBox]').val();

        if (index > 0 && password !== '') {
            const users = {
                username: $('[id*=School_DropDownList]').val(),
                email: $('[id*=School_DropDownList]').val() + "@gmail.com",
                password: password,
                confirmPassword: password
            };

            $.ajax({
                url: `${baseUrl}/api/account/register`,
                method: 'POST',
                async: false,
                contentType: 'application/json',
                data: JSON.stringify(users),
                success: function () {
                    isSend = true;
                },
                error: function (err) {
                    var response = null;
                    const errors = [];
                    var errorsString = "";

                    if (err.status === 400) {
                        try {
                            response = JSON.parse(err.responseText);
                        } catch (e) { }
                    }
                    if (response != null) {
                        const modelState = response.ModelState;

                        for (let key in modelState) {
                            if (modelState.hasOwnProperty(key)) {
                                errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                                errors.push(modelState[key]); //list of error messages in an array
                            }
                        }
                    }

                    //DISPLAY THE LIST OF ERROR MESSAGES 
                    if (errorsString !== "") {
                        $("#divErrorText").html(errorsString);
                        $('#divError').show('fade');
                    }
                }
            });
        }
        return isSend;
    }

    //change device password
    const changeDevicePassword = function (user) {
        $.ajax({
            url: `${baseUrl}/token`,
            method: 'POST',
            contentType: 'application/x-www-form-urlencoded',
            data: user,
            success: token => postChangePassword(token,user),
            error: function (err) {
                console.log(err)

                const { error_description } = JSON.parse(err.responseText);
                $.notify(error_description, "error");
            }
        });
    }

    //post change password
    function postChangePassword(token,data) {
        const model = {
            OldPassword: data.password,
            NewPassword: data.newPassword,
            ConfirmPassword: data.newPassword
        }
        if (!token) return;

        $.ajax({
            url: `${baseUrl}/api/account/ChangePassword`,
            method: 'POST',
            headers: {
                Authorization: `Bearer ${token}`
            },
            contentType: 'application/json',
            data: JSON.stringify(model),
            success: function () {
                updatePassword({})
                $("#password-error").html("Password Changed Success!");
            },
            error: function (err) {
                var response = null;
                const errors = [];
                var errorsString = "";

                if (err.status === 400) {
                    try {
                        response = JSON.parse(err.responseText);
                    } catch (e) {
                    }
                }
                if (response != null) {
                    const modelState = response.ModelState;

                    for (let key in modelState) {
                        if (modelState.hasOwnProperty(key)) {
                            errorsString = (errorsString === "" ? "" : errorsString + "<br/>") + modelState[key];
                            errors.push(modelState[key]); //list of error messages in an array
                        }
                    }
                }

                //DISPLAY THE LIST OF ERROR MESSAGES 
                if (errorsString !== "") {
                    $("#password-error").html(errorsString);
                }
            }
        });
    }

    //update password in sikkhaloy db
    function updatePassword(data) {
        $.ajax({
            url: "InstitutionRegister.aspx/UpdatePassword",
            method: 'POST',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(data),
            success: function () {
                console.log("success")
            },
            error: function (err) {
                console.log(err)
            }
        });
    }



    const obj = {};
    obj.registerNewDeviceUser = registerNewDeviceUser;
    obj.changeDevicePassword = changeDevicePassword;


    return obj;
})();