//Check all tabs when a tab is closed
chrome.tabs.onRemoved.addListener(function (tabid, removed) {
    checkAllTabs();
})

chrome.windows.onRemoved.addListener(function (windowid) {
    console.log("window closed" + windowid);
})

//If tab updates, re-read blacklist information and check tabs accordingly
chrome.tabs.onUpdated.addListener(function
    (tabId, changeInfo, tab) {
    LoadBlockedWebsites();
})

//Manage requests from content script
chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    //Request: Close tab x
    if (request.msg === "tab_close_msg") {
        chrome.tabs.query({
            currentWindow: true,
            active: true
        }, function (tabs) {
            chrome.tabs.remove(sender.tab.id);
        });
    }
    //Request: Re-read blacklist information and check tabs accordingly
    if (request.msg === "reload_db_msg") {
        console.log("reloadedDB");
        LoadBlockedWebsites();
    }
    //Request: Just check tabs
    if (request.msg == "checkalltabs_msg") {
        console.log("checking for all tabs rn")
        checkAllTabs();
    }
});

//Read XML file containing blacklist
function LoadBlockedWebsites() {
    fetch("data.xml").then(r => r.text()).then(result => {
        ProcessXMLData(result);
    })
}

//Process XML Data (have to do it this way because service workers don't support XML parsing in Manifest 3)
function ProcessXMLData(data) {
    var xmlData = data;
    var xmlStringData = String(xmlData);

    var checkModeDetermineStart = "<checkMode>";
    var checkModeDetermineEnd = "</checkMode";
    var websitesDetermineStart = "<website>";
    var websitesDetermineEnd = "</website>";

    //Retrieve if tabs should be closed or timer paused
    var checkMode = xmlStringData.split(checkModeDetermineStart).pop().split(checkModeDetermineEnd)[0];
    var websites = [];

    //Retrieve blacklisted websites
    for (var i = 1; i < xmlStringData.split(websitesDetermineEnd).length; i++) {
        var temp = xmlStringData.split(/(?=<website>)/)[i].split(websitesDetermineStart).pop().split(websitesDetermineEnd)[0];
        websites.push(temp);
        if (temp.toString().substring(0, 4) == "www.") {
            websites.push(temp.slice(4, temp.length));
        } else if (!(temp.toString().substring(0, 4) == "www.")) {
            websites.push("www." + temp);
        }
    }
    //Store checkMode and blacklisted domains in Chrome storage
    chrome.storage.sync.set({ checkMode, websites });
    console.log(checkMode);
    console.log(websites);
    checkAllTabs();
}

//Pause timer: String is stored in Chrome storage to be read by Antistract software
function brokeDaRulesTrue() {
    var tempCheckMode;
    var tempWebsites;
    var checkMode;
    var websites;
    //String Antistract will be looking for
    var brokeDaRules = "-cT2A;z=YzW}f4ht/H6epiW2!Md*@,";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        //Has to be cleared because else the parameters would exist multiple times which makes reliable reading of the data impossible
        chrome.storage.sync.clear();
        checkMode = tempCheckMode;
        websites = tempWebsites;
        chrome.storage.sync.set({ checkMode, websites, brokeDaRules });
    });
}

//Resume timer: String is stored in Chrome storage to be read by Antistract software
function brokeDaRulesFalse() {
    var tempCheckMode;
    var tempWebsites;
    var checkMode;
    var websites;
    //String Antistract will be looking for
    var brokeDaRules = "8fj*d-*c@cP}+i3f%aB*BD#63amL*i";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        //Has to be cleared cause else the parameters would exist multiple times which makes reliable reading of the data impossible
        chrome.storage.sync.clear();
        checkMode = tempCheckMode;
        websites = tempWebsites;
        chrome.storage.sync.set({ checkMode, websites, brokeDaRules });
    });
}

//Check all tabs for violation of blacklist
function checkAllTabs() {
    var websites = [];
    var checkmode;
    found = 0;
    chrome.storage.sync.get(['websites', 'checkMode'], function (items) {
        websites = items.websites;
        checkmode = items.checkMode;
        console.log(websites);
        //Get all tab data
        chrome.tabs.query({}, function (tabs) {
            var found = 0;
            for (var i = 0; i < tabs.length; i++) {
                var tab = tabs[i];
                var url = new URL(tab.url)
                var domain = url.hostname
                for (var j = 0; j < websites.length; j++) {
                    if (domain === websites[j].toString()) {
                        //Increase variable if tab is opened and checkMode is 'Pause'
                        if (checkmode === "pausing") {
                            console.log("found: (p) " + domain);
                            ++found;
                        }
                        //Close tab if checkMode is set to 'Close'
                        else if (checkmode === "closing") {
                            console.log("found: (c) " + domain);
                            chrome.tabs.remove(tab.id);
                        }
                    }
                }
            }
            //Call function that handles pausing of the timer if checkMode 'Pause' && tab is opened
            if (found > 0) {
                brokeDaRulesTrue();
            } else {
                brokeDaRulesFalse();
            }
        })
    });
}

//DEBUG AREA
function LogCurrentTabs() {
    chrome.tabs.query({}, (tabs) => { console.log(tabs) });
}