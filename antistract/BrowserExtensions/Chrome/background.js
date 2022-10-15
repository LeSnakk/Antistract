chrome.runtime.onInstalled.addListener(() => {
    LoadBlockedWebsites();
})

chrome.tabs.onRemoved.addListener(function (tabid, removed) {
    checkAllTabs();
})

chrome.windows.onRemoved.addListener(function (windowid) {
    console.log("window closed" + windowid);
})

chrome.tabs.onUpdated.addListener(function
    (tabId, changeInfo, tab) {
    LoadBlockedWebsites();
})

chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
    if (request.msg === "tab_close_msg") {
        chrome.tabs.query({
            currentWindow: true,
            active: true
        }, function (tabs) {
            chrome.tabs.remove(sender.tab.id);
        });
    }
    if (request.msg === "reload_db_msg") {
        console.log("reloadedDB");
        LoadBlockedWebsites();
    }
    if (request.msg == "checkalltabs_msg") {
        console.log("checking for all tabs rn")
        checkAllTabs();
    }
});

function LoadBlockedWebsites() {
    fetch("data.xml").then(r => r.text()).then(result => {
        ProcessXMLData(result);
    })
}

function ProcessXMLData(data) {
    var xmlData = data;
    var xmlStringData = String(xmlData);

    var checkModeDetermineStart = "<checkMode>";
    var checkModeDetermineEnd = "</checkMode";
    var websitesDetermineStart = "<website>";
    var websitesDetermineEnd = "</website>";

    var checkMode = xmlStringData.split(checkModeDetermineStart).pop().split(checkModeDetermineEnd)[0];
    var websites = [];

    for (var i = 1; i < xmlStringData.split(websitesDetermineEnd).length; i++) {
        var temp = xmlStringData.split(/(?=<website>)/)[i].split(websitesDetermineStart).pop().split(websitesDetermineEnd)[0];
        websites.push(temp);
        if (temp.toString().substring(0, 4) == "www.") {
            websites.push(temp.slice(4, temp.length));
        } else if (!(temp.toString().substring(0, 4) == "www.")) {
            websites.push("www." + temp);
        }
    }
    chrome.storage.sync.set({ checkMode, websites });
    console.log(checkMode);
    console.log(websites);
    checkAllTabs();
}

function brokeDaRulesTrue() {
    var tempCheckMode;
    var tempWebsites;
    var checkMode;
    var websites;
    var brokeDaRules = "-cT2A;z=YzW}f4ht/H6epiW2!Md*@,";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        chrome.storage.sync.clear();
        checkMode = tempCheckMode;
        websites = tempWebsites;
        chrome.storage.sync.set({ checkMode, websites, brokeDaRules });
    });
}

function brokeDaRulesFalse() {
    var tempCheckMode;
    var tempWebsites;
    var checkMode;
    var websites;
    var brokeDaRules = "8fj*d-*c@cP}+i3f%aB*BD#63amL*i";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        chrome.storage.sync.clear();
        checkMode = tempCheckMode;
        websites = tempWebsites;
        chrome.storage.sync.set({ checkMode, websites, brokeDaRules });
    });
}

function checkAllTabs() {
    var websites = [];
    var checkmode;
    found = 0;
    chrome.storage.sync.get(['websites', 'checkMode'], function (items) {
        websites = items.websites;
        checkmode = items.checkMode;
        console.log(websites);
        chrome.tabs.query({}, function (tabs) {
            var found = 0;
            for (var i = 0; i < tabs.length; i++) {
                var tab = tabs[i];
                var url = new URL(tab.url)
                var domain = url.hostname
                for (var j = 0; j < websites.length; j++) {
                    if (domain === websites[j].toString()) {
                        if (checkmode === "pausing") {
                            console.log("found: (p) " + domain);
                            ++found;
                        } else if (checkmode === "closing") {
                            console.log("test");
                            console.log("found: (c) " + domain);
                            chrome.tabs.remove(tab.id);
                        }
                    }
                }
            }
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
