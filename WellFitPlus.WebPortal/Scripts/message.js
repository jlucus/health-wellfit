$(document).ready(function () {
    var theParent = document.querySelector("#messageParent");
    theParent.addEventListener("click", processMessageSelection, false);
    var clickedItem = "";

    var data = {
        ID: "null",
        Description: "No Description"
    };

    //function processMessageSelection(e) {
    //    if (e.target !== e.currentTarget) {
    //        clickedItem = e.target.id;

    //        data.ID = clickedItem;

    //        $.ajax({
    //            type: 'POST',
    //            url: urlBaseAPI + '/WellFitPlus.WebAPI/api/message/GetMessage',
    //            data: data
    //        }).done(function (data) {
    //            alert("Success");
    //        }).error(function (data) {
    //            // Cache the access token in session storage.
    //            alert("Rats");
    //        });
    //    }
    //    e.stopPropagation();
    //}

    function processMessageSelection(e) {
        if (e.target !== e.currentTarget) {
            clickedItem = e.target.id;

            data.ID = clickedItem;
            $.ajax({
                type: 'POST',
                url: urlBasePortal + '/message/DeleteMessage',
                data: data
            }).done(function (data) {
                //alert("Message Deleted");
                window.location.href = data.Url

            }).error(function (data) {
                // Cache the access token in session storage.
                alert("Message Deletion Failed");
            });
        }
        e.stopPropagation();
    }

    $(document).on("click", "#addMessage", function () {

        data.Description = $("#newMessage").val();

        $.ajax({
            type: 'POST',
            cache:false,
            url: urlBasePortal + '/message/AddMessage',
            data: data
        }).done(function (data) {
            //alert("Message Added");
            window.location.href = data.Url
            
        }).error(function (data) {
            // Cache the access token in session storage.
            alert("Adding of Message Failed");
        });
    });

});