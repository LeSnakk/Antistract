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
        console.log("Tab is running");
        brokeDaRulesTrue();
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
    fetch("data.txt").then(r => r.text()).then(result => {
        writeTrue(result);
    })
}

function writeTrue(data) {
    var txtData = data;
    var txtStringData = String(txtData);

    var brokeDaRules = "true";
    chrome.storage.sync.set({ brokeDaRules });
    chrome.storage.sync.get(['brokeDaRules'], function (items) {
        console.log(items.brokeDaRules);
    });
}

function brokeDaRulesFalse() {

}






//DEBUG AREA
function LogCurrentTabs() {
    chrome.tabs.query({}, (tabs) => { console.log(tabs) });
}
