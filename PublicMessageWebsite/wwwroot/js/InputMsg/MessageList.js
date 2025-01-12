window.onload = function () {
    $.ajax({
        url: getMessageList_url,
        type: "GET",
        contentType: "application/json",
        setTimeout: 20000,
        success: function (response) {
            document.getElementById("msglistBox").innerHTML = marked.parse(response.markdown);
        },
    });
};