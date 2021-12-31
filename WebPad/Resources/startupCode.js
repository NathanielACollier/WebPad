
const hoverGblStyleName = 'webpadGlobalStyles_MouseOver';

docReady(() => {
   codeToRunOnStartup(); 
});


function codeToRunOnStartup() {
    
    setupStyles();

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
        /*
        document doesn't have classList
         */
        if( e.target && e.target != document){
            onNewTargetElement(e.target);
        }
    });
    
    document.addEventListener("mouseleave", (e) => {
        // if the mouse leaves the document, then don't leave anything highlighted
        onNewTargetElement(null);
    });
    
} // end of codeToRunOnStartup

let previouseTargetElement = null;

function onNewTargetElement(target) {

    if( previouseTargetElement ){
        // remove the class
        previouseTargetElement.classList.remove(hoverGblStyleName);
    }

    if( target && target.tagName){
        console.log(`onNewTargetElement: [tagName: ${target.tagName}; id: ${target?.id ?? ''}]`);
        previouseTargetElement = target;

        target.classList.add(hoverGblStyleName);
    }
}

function docReady(fn) {
    // see if DOM is already available
    if (document.readyState === "complete" || document.readyState === "interactive") {
        // call on next available tick
        setTimeout(fn, 1);
    } else {
        document.addEventListener("DOMContentLoaded", fn);
    }
}


function setupStyles(){
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
}


