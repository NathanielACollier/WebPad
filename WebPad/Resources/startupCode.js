
docReady(() => {
   codeToRunOnStartup(); 
});


function docReady(fn) {
    // see if DOM is already available
    if (document.readyState === "complete" || document.readyState === "interactive") {
        // call on next available tick
        setTimeout(fn, 1);
    } else {
        document.addEventListener("DOMContentLoaded", fn);
    }
}

function codeToRunOnStartup() {
    let hoverGblStyleName = 'webpadGlobalStyles_MouseOver';
    // create the class we are going to apply on mouse enter
    let hoverGlbStyle = document.createElement('style');
    hoverGlbStyle.type = 'text/css';
    hoverGlbStyle.appendChild(
        document.createTextNode(
        `
                .${hoverGblStyleName} {
                    background-color: purple;
                }
            `)
    );

    document.getElementsByTagName('head')[0].appendChild(hoverGlbStyle);

    // setup document click to send a message
    // from: https://stackoverflow.com/questions/29555044/javascript-global-onclick-listener
    document.addEventListener('click', (e) => {
        // element is e.target
        // We've got attributes set on every element with what line number and column it is
        let lineNumber = e.target.getAttribute('linenumber');
        let column = e.target.getAttribute('column');

        window.chrome.webview.postMessage({
            lineNumber: lineNumber,
            column: column,
            type: 'elementClick'
        });
    });

    // setup the hover so we can highlight the element
    document.addEventListener('mouseover', (e) => {
        if( e.target && e.target.tagName){
            console.log(`mouseenter: [tagName: ${e.target.tagName}]`);
        }
        /*
        document doesn't have classList
         */
        if( e.target && e.target != document){
            e.target.classList.add(hoverGblStyleName);
        }
        
    });

    document.addEventListener('mouseleave', (e) => {
        /*
        document doesn't have classList
         */
        if( e.target && e.target != document){
            e.target.classList.add(hoverGblStyleName);
            e.target.classList.remove(hoverGblStyleName);
        }
    });
} // end of codeToRunOnStartup
