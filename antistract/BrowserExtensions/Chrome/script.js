var checkModex;
var websitesx = ['www.youtube.com', 'www.google.com', 'www.twitter.com'];

window.onload = function(e) {
    FetchBlacklistedWebsites();
    console.log("teststst");
}

function FetchBlacklistedWebsites() {
    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        checkModex = items.checkMode;
        websitesx = items.websites;
        console.log("read storage");
    });
    CheckCurrentWebsite();
}

function CheckCurrentWebsite() {
    console.log("checkcurrentwebsite");
    for (var i = 0; i < websitesx.length; i++) {
        if (window.location.hostname === websitesx[i].toString()) {
            console.log("shouldclosenow");
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