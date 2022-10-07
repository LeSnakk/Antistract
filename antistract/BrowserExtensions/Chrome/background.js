chrome.runtime.onInstalled.addListener(() => {
    console.log("This alert is coming from the backgroundService!");
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

function LogCurrentTabs() {
    chrome.tabs.query({}, (tabs) => { console.log(tabs) });
}
