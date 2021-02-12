gl = (id) => {
    let returnObject = {};

    const day = new Date().getTime() + (24 * 60 * 60 * 1000)
    
    if(localStorage.getItem("gl")) {
        if(new Date(parseInt(localStorage.getItem("gl"))) < new Date() - day) {
            localStorage.clear();
        }
    }
    
    const IsUnique = localStorage.getItem("gl") == null;
    
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
                Unique: IsUnique,
                Path: window.location.pathname
            })
        }).then(r => {})
    }
    
    if(!localStorage.getItem("gl"))
        localStorage.setItem("gl", `${new Date().getTime()}`);
    
    return returnObject;
}