//<![CDATA[


function ChangeText() {
    if (Content0.style.display === "none") {
        ButtonCollapse1.value = "Visa text";
        SetFocus.innerHTML = "IB0";
        $("#IB0").focus();
    } else {
        ButtonCollapse.value = "Dölj text";
        ButtonCollapse1.value = "Dölj text";
        SetFocus.innerHTML = "";
    }
};

function CheckIB(p) {
    var GetVal = jQuery.fn.V = function () { return $("#" + p).val(); };
    var re = new RegExp("^([0]|[1-9][0-9]{0,4}|EoS)$");
    if (!re.test(GetVal())) {
        alert("Installed Base måste vara mellan 0 och 99999!");
        return false;
    } else {
        return true;
    }
};

function CheckFR(p) {
    var GetVal = jQuery.fn.V = function () { return $("#" + p).val(); };
    if ($("#rbMTBF_0").is(":checked")) {
        var re = new RegExp("^([1-9]|[1-9][0-9]|[1-9][0-9][0-9]|[1-9][0-9][0-9][0-9]|[1-9][0-9][0-9][0-9][0-9]|100000|[0-9][.,][0-9][1-9])$");
        if (!re.test(GetVal())) {
            alert("MTBF måste vara mellan 0.01 och 100000!");
            return false;
        } else {
            return true;
        }
    }
    else
    {
        var re = new RegExp("^([1-9]|[1-9][0-9]|100|[0-9][.,][0-9]{0,4}[1-9])$");
        if (!re.test(GetVal())) {
            alert("Failure Rate måste vara mellan 0.00001 och 100!");
            return false;
        } else {
            return true;
        }
    }
};

function CheckRS(p) {
    var GetVal = jQuery.fn.V = function () { return $("#" + p).val(); };
    var re = new RegExp("^([0]|[1-9][0-9]{0,4}|EoS)$");
    if (!re.test(GetVal())) {
        alert("Antal Regionlager måste vara mellan 0 och 9999!");
        return false;
    } else {
        return true;
    }
};

function CheckRL(p) {
    var GetVal = jQuery.fn.V = function () { return $("#" + p).val(); };
    var re = new RegExp("^([0]|[1-9]|[1-9][0-9]|100)$");
    if (!re.test(GetVal())) {
        alert("Repair Loss måste vara mellan 0 och 100%!");
        return false;
    } else {
        return true;
    }
};

function Collapse() {
    $(Content0).slideToggle("slow", function () { ChangeText(); });
};

function CheckHide() {
    if ($("#SetFocus").text() === "IB0") {
        Content0.style.display = "none";
    } else {
        Content0.style.display = "";
    }
    ChangeText();
};

function GetResult() {
    if (!CheckIB("IB0")) {
        return false;
    }
    if (!CheckFR("FR0")) {
        return false;
    }
    if (!CheckRL("RL0")) {
        return false;
    }
    ProgressOn();
    $("#divStock").empty();
    $("#divSafety").empty();
    $("#divRepaired").empty();
    $("#divLost").empty();
    $("#divFailed").empty();
    $("#divInfoText").empty();

    var handle = $.post(
        "/Ltb/Calculate",
        { data: $("#Lang").text() + "!" + GetViewState() });

    handle.done(function (result) {
        ProgressOff();
        $("#divResult").html(result);
        ErrorAlert();
        $("#IB0").focus();
        return true;
    });

    handle.fail(function (data) {
        ProgressOff();
        informUser(data.responseText);
        return false;
    });
}


function informUser(message) {
    alert(message);
}

function GetViewState() {
    return $("#ConfidenceLevels").val() + "!"
        + $("#RepairLeadDays").val() + "!"
        + $("#LTBDate").val() + "!"
        + $("#EOSDate").val() + "!_"
        + $("#IB0").val() + "!_"
        + $("#IB1").val() + "!_"
        + $("#IB2").val() + "!_"
        + $("#IB3").val() + "!_"
        + $("#IB4").val() + "!_"
        + $("#IB5").val() + "!_"
        + $("#IB6").val() + "!_"
        + $("#IB7").val() + "!_"
        + $("#IB8").val() + "!_"
        + $("#IB9").val() + "!_"
        + $("#IB10").val() + "!_"
        + $("#RS0").val() + "!_"
        + $("#RS1").val() + "!_"
        + $("#RS2").val() + "!_"
        + $("#RS3").val() + "!_"
        + $("#RS4").val() + "!_"
        + $("#RS5").val() + "!_"
        + $("#RS6").val() + "!_"
        + $("#RS7").val() + "!_"
        + $("#RS8").val() + "!_"
        + $("#RS9").val() + "!_"
        + $("#FR0").val() + "!_"
        + $("#FR1").val() + "!_"
        + $("#FR2").val() + "!_"
        + $("#FR3").val() + "!_"
        + $("#FR4").val() + "!_"
        + $("#FR5").val() + "!_"
        + $("#FR6").val() + "!_"
        + $("#FR7").val() + "!_"
        + $("#FR8").val() + "!_"
        + $("#FR9").val() + "!_"
        + $("#RL0").val() + "!_"
        + $("#RL1").val() + "!_"
        + $("#RL2").val() + "!_"
        + $("#RL3").val() + "!_"
        + $("#RL4").val() + "!_"
        + $("#RL5").val() + "!_"
        + $("#RL6").val() + "!_"
        + $("#RL7").val() + "!_"
        + $("#RL8").val() + "!_"
        + $("#RL9").val() + "!_"
        + $("#rbRepair_0").is(":checked") + "!_"
        + $("#rbMTBF_0").is(":checked");
};

var Exit = true;

function ProgressOn() {
    divInfoBox.style.display = "";
    Exit = false;
    blink("#divInfoBox");
}

function ProgressOff() { Exit = true; }

function ErrorAlert() {
    if ($("#divErrorAlert").text() === "ErrorAlert") {
        alert($("#divInfoText").text())
    }
}

function blink(selector) {
    if (Exit === true) {
        selector.style.display = "none";
        return false;
    }
    $(selector).fadeOut("slow", function () {
        if (Exit === false) {
            $(this).fadeIn("slow", function () { blink(this); });
        } else {
            $(selector).fadeOut("slow");
        }
    });
    return true;
}


function GetViewStateLTB() {
    var handle = $.post(
        "/Ltb/LtbDate",
        { data: $("#Lang").text() + "!" + GetViewState() });

    handle.done(function (result) {
        $("#ltb2").html(result);
        CheckHide();
    });

    handle.fail(function (data) {
        informUser(data.responseText);
    });
};

function GetViewStateEOS() {
    var handle = $.post(
        "/Ltb/EosDate",
        { data: $("#Lang").text() + "!" + GetViewState() });

    handle.done(function (result) {
        $("#ltb2").html(result);
        CheckHide();
    });

    handle.fail(function (data) {
        informUser(data.responseText);
    });
};

function SetRepair() {
    var handle = $.post(
        "/Ltb/Repair",
        { data: $("#Lang").text() + "!" + GetViewState() });

    handle.done(function (result) {
        $("#ltb2").html(result);
        CheckHide();
    });

    handle.fail(function (data) {
        informUser(data.responseText);
    });
};

function SetNoRepair() {
    var handle = $.post(
        "/Ltb/NoRepair",
        { data: $("#Lang").text() + "!" + GetViewState() });

    handle.done(function (result) {
        $("#ltb2").html(result);
        CheckHide();
    });

    handle.fail(function (data) {
        informUser(data.responseText);
    });
};

function SetMtbf() {
    $("#divFR").addClass("hidden");
    $("#divMTBF").removeClass("hidden");
    $("#FR0").attr("placeholder","MTBF0");
    if ($("#FR1").attr("placeholder")) { $("#FR1").attr("placeholder", "MTBF1"); }
    if ($("#FR2").attr("placeholder")) { $("#FR2").attr("placeholder", "MTBF2"); }
    if ($("#FR3").attr("placeholder")) { $("#FR3").attr("placeholder", "MTBF3"); }
    if ($("#FR4").attr("placeholder")) { $("#FR4").attr("placeholder", "MTBF4"); }
    if ($("#FR5").attr("placeholder")) { $("#FR5").attr("placeholder", "MTBF5"); }
    if ($("#FR6").attr("placeholder")) { $("#FR6").attr("placeholder", "MTBF6"); }
    if ($("#FR7").attr("placeholder")) { $("#FR7").attr("placeholder", "MTBF7"); }
    if ($("#FR8").attr("placeholder")) { $("#FR8").attr("placeholder", "MTBF8"); }
    if ($("#FR9").attr("placeholder")) { $("#FR9").attr("placeholder", "MTBF9"); }
};

function SetNoMtbf() {
    $("#divFR").removeClass("hidden");
    $("#divMTBF").addClass("hidden");
    $("#FR0").attr("placeholder", "FR0");
    if ($("#FR1").attr("placeholder")) { $("#FR1").attr("placeholder", "FR1"); }
    if ($("#FR2").attr("placeholder")) { $("#FR2").attr("placeholder", "FR2"); }
    if ($("#FR3").attr("placeholder")) { $("#FR3").attr("placeholder", "FR3"); }
    if ($("#FR4").attr("placeholder")) { $("#FR4").attr("placeholder", "FR4"); }
    if ($("#FR5").attr("placeholder")) { $("#FR5").attr("placeholder", "FR5"); }
    if ($("#FR6").attr("placeholder")) { $("#FR6").attr("placeholder", "FR6"); }
    if ($("#FR7").attr("placeholder")) { $("#FR7").attr("placeholder", "FR7"); }
    if ($("#FR8").attr("placeholder")) { $("#FR8").attr("placeholder", "FR8"); }
    if ($("#FR9").attr("placeholder")) { $("#FR9").attr("placeholder", "FR9"); }
};

var _gaq = _gaq || [];
_gaq.push(["_setAccount", "UA-31410005-1"]);
_gaq.push(["_trackPageview"]);

(function () {
    var ga = document.createElement("script");
    ga.type = "text/javascript";
    ga.async = true;
    ga.src = ("https:" === document.location.protocol ? "https://ssl" : "http://www") + ".google-analytics.com/ga.js";
    var s = document.getElementsByTagName("script")[0];
    s.parentNode.insertBefore(ga, s);
})();
$(document).ready(function () {

    if ($("#rbMTBF_0").is(":checked")) {
        SetMtbf();
    }
    else
    {
        SetNoMtbf();
    }
});
//]]>