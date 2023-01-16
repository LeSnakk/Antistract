var checkMode;
var brokeDaRulesData;
var websites = [];

window.onload = function (e) {
    reloadDatabase();
}

//Load data saved by service worker into local variables
function FetchBlacklistedWebsites() {
    chrome.storage.sync.get(['checkMode', 'websites', 'brokeDaRules'], function (items) {
        checkMode = items.checkMode;
        brokeDaRulesData = items.brokeDaRules;
        websites = items.websites;
        CheckCurrentWebsite();
    });
}

//Check currently active website and perform according action
function CheckCurrentWebsite() {
    //Close current tab...
    if (checkMode == "closing") {
        for (var i = 0; i < websites.length; i++) {
            if (window.location.hostname === websites[i].toString()) {
                console.log("Forbidden website: " + websites[i] + " - NOW CLOSING");
                closeCurrentTab();
            }
        }
    }
    //...or check all tabs if timer has to be paused
    else if (checkMode == "pausing") {
        checkAllWebsites();
    }
}

//Send request to service worker to close said tab
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

//Send request to service worker to check all tabs
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

//Send request to service worker to read blacklist database
function reloadDatabase() {
    chrome.runtime.sendMessage(
        {
            msg: "reload_db_msg"
        },
        function (response) {
            console.log("response from the bg", response);
            //Wait longer after response, then call function to fetch data stored by service worker
            function delay(time) {
                return new Promise(resolve => setTimeout(resolve, time));
            }
            delay(1000).then(() => FetchBlacklistedWebsites());
        }
    );
}