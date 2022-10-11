chrome.runtime.onInstalled.addListener(() => {
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
    if (request.msg == "broke_da_rules_msg") {
        console.log("Forbidden tab is running");
        brokeDaRulesTrue();
    }
    if (request.msg == "no_broke_da_rules_msg") {
        console.log("Forbidden tab not running")
        brokeDaRulesFalse();
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
}

function brokeDaRulesTrue() {
    var tempCheckMode;
    var tempWebsites;
    var brokeDaRules = "-cT2A;z=YzW}f4ht/H6epiW2!Md*@,";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        chrome.storage.sync.clear();
        chrome.storage.sync.set({ tempCheckMode, tempWebsites, brokeDaRules });
    });
}

function brokeDaRulesFalse() {
    var tempCheckMode;
    var tempWebsites;
    var brokeDaRules = "8fj*d-*c@cP}+i3f%aB*BD#63amL*i";

    chrome.storage.sync.get(['checkMode', 'websites'], function (items) {
        tempCheckMode = items.checkMode;
        tempWebsites = items.websites;
        chrome.storage.sync.clear();
        chrome.storage.sync.set({ tempCheckMode, tempWebsites, brokeDaRules });
    });
}






//DEBUG AREA
function LogCurrentTabs() {
    chrome.tabs.query({}, (tabs) => { console.log(tabs) });
}
