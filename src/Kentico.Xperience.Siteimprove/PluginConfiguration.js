// var is used here because the plugin needs access to this variable
var _si = window._si || [];

const HIGHLIGHT_CLASS = "siteimprove-content-check-highlight";
const HIGHLIGHT_SECONDS = 60; 
const HIGHLIGHT_COLOR = "#ffe066";

let cleanupFunction = null;
let cleanupTimeout = null;

const injectHighlightStyles = () => {
    document.head.insertAdjacentHTML('beforeend', `
        <style type="text/css">
          .${HIGHLIGHT_CLASS} {
            animation-name: highlight-outline;
            animation-duration: ${HIGHLIGHT_SECONDS}s;
            animation-fill-mode: forwards;
            outline: 3px solid ${HIGHLIGHT_COLOR};
            outline-offset: -3px;
          }

          @keyframes highlight-outline {
            0% {
              outline-color: ${HIGHLIGHT_COLOR};
            }
            90% {
              outline-color: ${HIGHLIGHT_COLOR};
            }
            100% {
              outline-color: transparent;
            }
          }
        </style>`
    );
};

const onHighlight = (highlight) => {
    highlight.highlights.forEach(highlight => {
        clearCleanup();

        if (highlight.selector.startsWith('head') || highlight.selector === 'html') {
            highlight.selector = 'body';
            highlight.offset = null;
        }

        const element = document.querySelector(highlight.selector);

        if (!element) {
            return;
        }

        element.scrollIntoView({
            behavior: 'smooth',
            block: 'center',
            inline: 'center'
        });

        if (highlight.offset) {
            const originalHTML = element.innerHTML;
            const childNode = element.childNodes[highlight.offset.child];
            const start = highlight.offset.start;
            const length = highlight.offset.length;

            const range = document.createRange();
            range.setStart(childNode, start);
            range.setEnd(childNode, start + length);

            const span = document.createElement('span');
            span.className = HIGHLIGHT_CLASS;

            range.surroundContents(span);

            cleanup(() => element.innerHTML = originalHTML);

        } else {
            element.classList.add(HIGHLIGHT_CLASS);
            cleanup(() => element.classList.remove(HIGHLIGHT_CLASS));
        }
    });
};

const cleanup = (func) => {
    cleanupFunction = func;
    cleanupTimeout = setTimeout(() => {
        func();
        cleanupFunction = null;
        cleanupTimeout = null;
    }, HIGHLIGHT_SECONDS * 1000);
};

const clearCleanup = () => {
    if (cleanupTimeout && cleanupFunction) {
        clearTimeout(cleanupTimeout);
        cleanupFunction();
        cleanupFunction = null;
        cleanupTimeout = null;
    }
};

const injectContentCheck = () => {
    _si.push(['registerPrepublishCallback', getDomCallback]);

    injectHighlightStyles();
    _si.push(['onHighlight', onHighlight]);

    return true;
};

const injectContainerTitle = () => {
    const container = document.querySelector('div[aria-label="Siteimprove CMS Plugin"] > .si-boxes-container');

    if (!container) {
        return false;
    }

    container.setAttribute('title', "Siteimprove Page overview");

    return true;
};

const getDomCallback = async () => {
    const body = document.querySelector('body');
    const frameContainer = document.createElement('div');
    const tempFrame = document.createElement('iframe');

    tempFrame.src = window.location.href;
    tempFrame.style = 'height:100vh; width:100%';
    frameContainer.appendChild(tempFrame);
    body.appendChild(frameContainer);

    const getDocument = new Promise((resolve) => {
        tempFrame.addEventListener(
            'load',
            () => resolve(tempFrame.contentWindow.document),
            { once: true }
        );
    });

    return [
        await getDocument,
        () => {
            tempFrame.remove();
            frameContainer.remove();
        }
    ];
};

((token, input = null, contentCheck = false) => {
    _si.push(['clear', () => { }, token]);

    if (input !== null) {
        _si.push(['input', input, token, () => { }]);
    }

    // In case the plugin takes bit of a time to load and injects failed, try again several times.
    // It usually doesn't take more than 2 attempts.
    let attempts = 0;
    const maxAttempts = 25;
    let containerTitleInjected = false;
    let contentCheckInjected = false;
    const interval = setInterval(() => {
        if (attempts >= maxAttempts) {
            clearInterval(interval);
        }
        attempts++;
        if (!containerTitleInjected) {
            containerTitleInjected = injectContainerTitle();
        }

        if (contentCheck && !contentCheckInjected) {
            contentCheckInjected = injectContentCheck();
        }

        if (containerTitleInjected && (!contentCheck || contentCheckInjected)) {
            clearInterval(interval);
        }
    }, 800);
})