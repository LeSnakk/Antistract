var checkMode;
var brokeDaRulesData;
var websites = [];

window.onload = function (e) {
    reloadDatabase();
}

function FetchBlacklistedWebsites() {
    chrome.storage.sync.get(['checkMode', 'websites', 'brokeDaRules'], function (items) {
        checkMode = items.checkMode;
        brokeDaRulesData = items.brokeDaRules;
        websites = items.websites;
        CheckCurrentWebsite();
    });
}

function CheckCurrentWebsite() {
    if (checkMode == "closing") {
        for (var i = 0; i < websites.length; i++) {
            if (window.location.hostname === websites[i].toString()) {
                console.log("Forbidden website: " + websites[i] + " - NOW CLOSING");
                closeCurrentTab();
            }
        }
    } else if (checkMode == "pausing") {
        checkAllWebsites();
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

function checkAllWebsites() {
    chrome.runtime.sendMessage(
        {
            msg: "checkalltabs_msg"
        },
        function (response) {
            console.log("checking all tabs", response)
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