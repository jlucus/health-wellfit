$(document).ready(function () {
    var theParent = document.querySelector("#videoParent");
    var sponsors = document.querySelector("#sponsors");
    var sort = document.querySelector("#sortVideos");
    theParent.addEventListener("click", processVideoSelection, false);
    sort.addEventListener("click", sortVideos, false);

    var clickedItem = "";

    var data = {
        ID: "null",
        Path: "Empty",
        Type: "None",
        Tags: "None",
        Title: "No Title",
        Description: "No Description",
        Active:true,
        DateModified:"",
        DateUploaded:""
    };

    function processVideoSelection(e) {
        if (e.target !== e.currentTarget) {
            clickedItem = e.target.id;

            data.ID = clickedItem;

            $.ajax({
                type: 'GET',
                url: urlBaseAPI + '/api/video/GetVideo',
                data: data
            }).done(function (data) {
                
                $("#videoTitle").val(data.title);
                $("#videoDescription").val(data.description);
                $('#selectedVideo')[0].src = data.path + "#t=00:00:12";

                var modified_at = new Date(data.dateModified);
                var day = modified_at.getDate();
                var month = modified_at.getMonth() + 1;
                var year = modified_at.getFullYear();
                var hour = modified_at.getHours();
                hour = ("0" + hour).slice(-2);
                var minutes = modified_at.getMinutes();
                minutes = ("0" + minutes).slice(-2);

                $('#dateModified').val(month + "/" + day + "/" + year + " " + hour + ":" + minutes);

                var created_at = new Date(data.dateUploaded);
                day = created_at.getDate();
                month = created_at.getMonth()+1;
                year = created_at.getFullYear();
                hour = created_at.getHours();
                hour = ("0" + hour).slice(-2);
                minutes = created_at.getMinutes();
                minutes = ("0" + minutes).slice(-2);

                $('#dateUploaded').val(month+"/"+day+"/"+year+" "+hour+":"+minutes);

                $('#selectedVideo').prop("controls", true)
                //document.getElementById('activateButtonLabel').innerText = 'Activate Video';
                if (data.active == true) {
                    //document.getElementById('activateButtonLabel').innerText = 'Deactivate Video';
                    //$('#deactivateVideo').attr("value", "Deactivate Video")
                }

            }).error(function (data) {
                alert("Video Selection failed");
            });
        }
        e.stopPropagation();
    }

    function processVideoChanges() {
        
        data.Path = $("#videoPath").attr("value");

        $.ajax({
            type: 'POST',
            url: urlBaseAPI + '/api/video/Edit',
            data: data
        }).done(function (data) {
        }).error(function (data) {
            alert("Video Edit failed");
        });
    }

    function updateProgress(evt) {
        if (evt.lengthComputable) {  //evt.loaded the bytes browser receive
            //evt.total the total bytes seted by the header
            //
            var percentComplete = Math.round((evt.loaded / evt.total) * 100);
            $('#progressBar').attr("value", percentComplete);
            $('#curr').val(percentComplete + "%");
        }
    }

    $(document).on("change", "#UploadVideo", function () {
        var selectedFile = ($("#UploadVideo"))[0].files[0];

        var xhr = new XMLHttpRequest();
        //xhr.upload.onprogress = updateProgress;
        xhr.upload.addEventListener('progress', updateProgress, false);
        xhr.addEventListener('progress', updateProgress, false);
        $('#progressBar').removeClass("hidden");
        $('#progressBarLabel').removeClass("hidden");
        $('#curr').removeClass("hidden");

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    // OK
                    alert('Successful Upload of Video');
                    
                    //window.location.href = data.Url
                    location.reload();
                    // here you can use the result (cli.responseText)
                } else {
                    // not OK
                    alert('failure!');
                }

                $('#progressBar').addClass("hidden");
                $('#progressBarLabel').addClass("hidden");
                $('#curr').addClass("hidden");
            }
        };
        var fd = new FormData();
        fd.append("fileToUpload", selectedFile);
        xhr.open("POST", urlBasePortal + "/video/LoadVideo", true);
        xhr.send(fd);
        $('#progressBarLabel').val("Uploading of file " + selectedFile.name + " in progress");


    });

    $(document).on("click", "#deleteVideo", function () {
        $.ajax({
            type: 'POST',
            //headers: { 'Content-Type': 'application/json' },
            url: urlBasePortal + '/video/DeleteVideo',
            data: data
        }).done(function (result) {
            $('#' + data.ID).parent().parent().remove();

            data = {
                ID: "null",
                Path: "Empty",
                Type: "None",
                Tags: "None",
                Title: "No Title",
                Description: "No Description",
                Active: true,
                DateModified: "",
                DateUploaded: ""
            };

            $("#videoTitle").val(data.title);
            $("#videoDescription").val(data.description);
            $('#selectedVideo')[0].src = '';

        }).error(function (data) {
            // Cache the access token in session storage.
            alert("Delete Video Failed");
        });
    });

    $(document).on("click", "#deactivateVideo", function () {

        data.Active = false;

        $.ajax({
            type: 'POST',
            //headers: { 'Content-Type': 'application/json' },
            url: urlBasePortal + '/video/ToggleVideoActive',
            data: data
        }).done(function (data) {
            //alert("Video DeActivated");
            location.reload();

        }).error(function (data) {
            // Cache the access token in session storage.
            alert("Deactivate Video Failed");
        });
    });

    $(document).on("change", "#UploadSponsor", function () {
        var selectedFile = ($("#UploadSponsor"))[0].files[0];
        
        var xhr = new XMLHttpRequest();
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    // OK
                    //alert('Successful Upload of Sponsor "' + ($("#UploadSponsor"))[0].files[0].name + '"');
                    // here you can use the result (cli.responseText)

                    var el = document.createElement('li');
                    el.innerHTML = '<div class="col-lg-5 col-lg-offset-1">'
                        + '<div>'
                        + '<img src="' + urlBasePortal + '/Content/Sponsor/' + ($("#UploadSponsor"))[0].files[0].name + '" style="width:auto;height:150px;margin:5px 5px 5px 5px;border:1px solid black" />'
                    + '</div>'
                    + '</div>';
                    sponsors.append(el);
                } else {
                    // not OK
                    alert('Failed Uploading Sponsor');
                }
            }
        };
        var fd = new FormData();
        fd.append("sponsorToUpload", selectedFile);
        xhr.open("POST", urlBasePortal + "/sponsor/LoadSponsor", true);
        xhr.send(fd);
    });

    
    function loadSponsors(e) {
            $.ajax({
                type: 'POST',
                url: urlBaseAPI + '/api/sponsor/GetSponsors',
            }).done(function (data) {
                if (data.length == 0) { return; }
                for (var i = 0; i < data.length; i++)
                {
                    var el = document.createElement('li');
                    el.innerHTML = '<div class="col-lg-5 col-lg-offset-1">'
                        + '<div>'
                        + '<img src="' + urlBasePortal + '/Content/Sponsor/' + data[i].name + '" style="width:150px;height:150px;margin:5px 5px 5px 5px;border:1px solid black" />'
                    + '</div>'
                    + '</div>';
                    sponsors.append(el);
                };
                
            }).error(function (data) {
                //alert("Loading sponsors failed");
            });
    }
    loadSponsors(null);

    // Search Videos
    $('.videoTitle').each(function () {
        $(this).attr('data-search-term', $(this).text().toLowerCase());
    });
    $('.live-search-box').on('keyup', function () {
        var searchTerm = $(this).val().toLowerCase();
        $('.videoTitle').each(function () {
            if ($(this).filter('[data-search-term *= ' + searchTerm + ']').length > 0 || searchTerm.length < 1) {
                $(this).parent('*').parent('*').show();
            } else {
                $(this).parent('*').parent('*').hide();
            }
        });
    });

    var ascending = true;
    function sortVideos() {
        ascending = !ascending;

        $("#videoParent .video").sort(function (a, b) {
            var contentA = parseInt($(a).attr('data-sort'));
            var contentB = parseInt($(b).attr('data-sort'));
                if (ascending) {
                    return (contentA < contentB) ? -1 : (contentA > contentB) ? 1 : 0;
                }
                else
                {
                    return contentB - contentA;
                }
        }).each(function () {
            var elem = $(this);
            elem.remove();
            $(elem).appendTo("#videoParent");
        });
    }

    $(document).on("change", "#videoTitle", function () {
        data.Title= $("#videoTitle").val();
        processVideoChanges();
    });

    $(document).on("change", "#videoDescription", function () {
        data.Description = $("#videoDescription").val();
        processVideoChanges();
    });

});