var checkMode;
var websites = [];

window.onload = function (e) {
    reloadDatabase();
}

function FetchBlacklistedWebsites() {
    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        checkMode = items.checkMode;
        websites = items.websites;
        CheckCurrentWebsite();
    });
}

function CheckCurrentWebsite() {
    for (var i = 0; i < websites.length; i++) {
        if (window.location.hostname === websites[i].toString()) {
            if (checkMode == "closing") {
                console.log("Forbidden website: " + websites[i] + " - NOW CLOSING");
                closeCurrentTab();
            }
            else if (checkMode == "pausing")
            {
                console.log("Forbidden website: " + websites[i] + " - HALT THE TIMER!");
                brokeDaRules();
            }
        }
    }
}

function closeCurrentTab() {
    chrome.runtime.sendMessage(
        {
            msg: "tab_close_msg"
        },
        function (response) {
            console.log("response from the bg", response)
        }
    );
}

function reloadDatabase() {
    chrome.runtime.sendMessage(
        {
            msg: "reload_db_msg"
        },
        function (response) {
            console.log("response from the bg", response);

            function delay(time) {
                return new Promise(resolve => setTimeout(resolve, time));
            }
            delay(1000).then(() => FetchBlacklistedWebsites());          
        }
    );
}

function brokeDaRules() {
    chrome.runtime.sendMessage(
        {
            msg: "broke_da_rules_msg"
        },
        function (response) {
            console.log("response from the bg", response)
        }
    );
}