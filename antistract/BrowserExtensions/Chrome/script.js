switch (window.location.hostname) {
    case "www.youtube.com":
        console.log("closing website...");
        closeCurrentTab();
        break;
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