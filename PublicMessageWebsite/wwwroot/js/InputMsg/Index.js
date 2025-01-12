$(function () {
    document.getElementById('bottomBox').innerHTML = marked.parse(bottomText);
});
function sendButton_Click() {
    setCookie('userName', document.getElementById("nameBox").value);

    function outputToast(id) {
        let sendMsg;
        switch (id) {
            case 0:
                sendMsg="发送成功";
                break;
            case 1:
                sendMsg = "发送失败，已达到留言发送上限";
                break;
            case 2:
                sendMsg = "发送失败，留言不能为空";
                break;
            case 3:
                sendMsg = "发送失败，署名不能为空";
                break;
        }
        showToast(sendMsg, undefined, "sendMsgToast");
    }
    const inputValue = document.getElementById("inputBox").value;
    const nameValue = document.getElementById("nameBox").value


    if (inputValue == "") 
        outputToast(2);
    else if (nameValue == "") 
        outputToast(3);
    else {
        $.ajax({
            url: submitMsg_url,
            type: "POST",
            contentType: "application/json",
            setTimeout: 10000,
            data: JSON.stringify(
                {
                    InputBoxValue: inputValue,
                    NameBoxValue: nameValue
                }),
            success: function (response) {
                outputToast(response.value);
                if (response.value == 0) {
                    document.getElementById("inputBox").value = "";
                }
            },
        });
    }
}
document.addEventListener('DOMContentLoaded', function () {
    {
        document.getElementById("inputBox").addEventListener('keydown', function (event) {
            if (event.key === 'Enter')
                sendButton_Click();
        });

        var unValue = getCookie('userName');
        if (unValue != null)
            document.getElementById("nameBox").value = unValue;
    }
});
function setCookie(name, value, days = 36500) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}