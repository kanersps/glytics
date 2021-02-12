gl = (id) => {
    let returnObject = {};
    
    returnObject.send = (type) => {
        fetch("https://localhost:5001/app/web", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                Type: type,
                Id: id,
                Timezone: new Date().getTimezoneOffset(),
                Sent: `/Date(${new Date().getTime()})/`,
                Unique: document.referrer ? (new URL(document.referrer)).hostname.replace("www.", "") !== document.location.hostname : true,
                Path: window.location.pathname
            })
        }).then(r => {})
    }
    
    return returnObject;
}