var checkModex;
var websites = [];

window.onload = function (e) {
    reloadDatabase();
}

function FetchBlacklistedWebsites() {
    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        checkModex = items.checkMode;
        websites = items.websites;
        CheckCurrentWebsite();
    });
}

function CheckCurrentWebsite() {
    for (var i = 0; i < websites.length; i++) {
        if (window.location.hostname === websites[i].toString()) {
            console.log("Forbidden website: " + websites[i] + " - NOW CLOSING");
            closeCurrentTab();
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
            FetchBlacklistedWebsites();
        }
    );
}