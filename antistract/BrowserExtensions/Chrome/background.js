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
});



    LogCurrentTabs();

function LoadBlockedWebsites() {
    fetch("data.xml").then(r => r.text()).then(result => {
        ProcessXMLData(result);
    })
}

function ProcessXMLData(data) {
    var xmlData = data;
    var xmlStringData = String(xmlData);
    console.log(xmlStringData);

    var checkModeDetermineStart = "<checkMode>";
    var checkModeDetermineEnd = "</checkMode";
    var websitesDetermineStart = "<website>";
    var websitesDetermineEnd = "</website>";

    var checkMode = xmlStringData.split(checkModeDetermineStart).pop().split(checkModeDetermineEnd)[0];
    var websites = [];

    for (var i = 1; i < xmlStringData.split(websitesDetermineEnd).length; i++) {
        websites.push(xmlStringData.split(/(?=<website>)/)[i].split(websitesDetermineStart).pop().split(websitesDetermineEnd)[0]);
    }

    
    console.log(checkMode);
    console.log(websites);
}






//DEBUG AREA
function LogCurrentTabs() {
    chrome.tabs.query({}, (tabs) => { console.log(tabs) });
}
