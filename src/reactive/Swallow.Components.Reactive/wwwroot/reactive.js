const fragmentSelector = "[srx-fragment]";
const stateMarker = " srx-prerender-state ";

export function setupFragment(element, prerenderedState) {
    for (const key in prerenderedState) {
        const stateElement = document.createElement("meta");
        stateElement.setAttribute("itemprop", "state");
        stateElement.setAttribute("data-key", key);
        stateElement.setAttribute("data-value", prerenderedState[key]);

        element.appendChild(stateElement);
    }

    const fragment = new ReactiveFragment(element);
    element["fragment"] = fragment;
    fragment.triggerInteraction(undefined, true);
}

function consumePrerenderedState() {
    const trailingComments = [...document.childNodes].filter(n => n.nodeType === Node.COMMENT_NODE);

    for (const trailingComment of trailingComments) {
        if (trailingComment.textContent.startsWith(stateMarker)) {
            const prerenderedStateText = trailingComment.textContent.substring(stateMarker.length);
            trailingComment.remove();

            return JSON.parse(prerenderedStateText);
        }
    }
}

function handleInteraction(event) {
    const fragment = event.target.closest(fragmentSelector)["fragment"];
    const trigger = event.target.getAttribute("_srx-path");

    if (fragment && trigger) {
        event.preventDefault();
        event.stopPropagation();

        const interaction = new Interaction(trigger, event);
        fragment.triggerInteraction(interaction);

        return true;
    }
}

class Interaction {

    trigger;
    eventName;
    eventBody;

    constructor(trigger, event) {
        this.trigger = trigger;
        this.eventName = "on" + event.type

        const transformer = getEventObject(event.type);
        this.eventBody = transformer(event);
    }
}

class ReactiveFragment {
    #triggerTimeout;

    #element;
    #interactions;

    constructor(element) {
        this.#element = element;
        this.#interactions = [];
    }

    triggerInteraction(interaction, immediate) {
        if (interaction) {
            this.#interactions.push(interaction);
        }

        if (immediate) {
            this.#sendInteraction(true).catch(_ => {});
        } else {
            clearTimeout(this.#triggerTimeout);
            this.#triggerTimeout = setTimeout(() => this.#sendInteraction(), 100);
        }
    }

    async #sendInteraction(force) {
        clearTimeout(this.#triggerTimeout);
        this.#triggerTimeout = undefined;

        if (this.#interactions.length === 0 && !force) {
            return;
        }

        const route = this.#element.getAttribute("srx-route");
        const formData = ReactiveFragment.#constructForm(this.#element, this.#interactions);
        this.#interactions = [];

        try {
            const response = await fetch(
                route,
                {
                    method: "POST",
                    headers: { "srx-request": "true" },
                    body: formData
                });

            await this.#handleResponse(response);
        } catch (error) {
            console.error("srx request failed: " + error);
        }
    }

    async #handleResponse(response) {
        if (response.headers.get("srx-response") !== "true") {
            throw new Error("srx request not handled by correct endpoint.");
        }

        switch (response.status) {
            case 200:
                const streamingBoundary = response.headers.get("srx-streaming-marker");
                if (streamingBoundary) {
                    const iterator = iterateChunks(response.body.getReader(), streamingBoundary);
                    for await (const chunk of iterator) {
                        ReactiveFragment.#applyBody(this.#element, chunk);
                    }
                } else {
                    const content = await response.text();
                    ReactiveFragment.#applyBody(this.#element, content);
                }

                break;

            case 204:
                const location = response.headers.get("srx-redirect");
                window.location = decodeURI(location);
                break;

            case 500:
                const errorMessage = await response.text();
                throw new Error(errorMessage);

            default:
                throw new Error("Unexpected status code: " + response.status);
        }
    }

    static #constructForm(element, interactions) {
        const formData = new FormData();

        const antiforgeryElement = element.querySelector("& > meta[itemprop='antiforgery']");
        if (antiforgeryElement) {
            formData.append(antiforgeryElement.getAttribute("data-name"), antiforgeryElement.getAttribute("data-token"));
        }

        for (const parameterElement of [...element.querySelectorAll("& > meta[itemprop='parameter']")]) {
            formData.append("_srx-parameter-" + parameterElement.getAttribute("data-key"), parameterElement.getAttribute("data-value"));
        }

        for (const stateElement of [...element.querySelectorAll("& > meta[itemprop='state']")]) {
            formData.append("_srx-state-" + stateElement.getAttribute("data-key"), stateElement.getAttribute("data-value"));
        }

        for (const interaction of interactions) {
            formData.append("_srx-event", JSON.stringify(interaction));
        }

        return formData;
    }

    static #applyBody(element, content) {
        const parser = new DOMParser();
        const document = parser.parseFromString(content, "text/html");

        const headContent = document.body.querySelector(".srx-head-content");
        const mainContent = document.body.querySelector(".srx-content");

        this.#updateHeadContent(window.document.head, headContent, element);
        this.#placeContentIntoFragment(element, mainContent);

        [...document.head.querySelectorAll("meta[itemprop='event-handler']")]
            .forEach(meta => this.#registerEventHandler(element, meta.getAttribute("data-element"), meta.getAttribute("data-event")));


        [...document.head.querySelectorAll("meta[itemprop='parameter']")]
            .forEach(meta => element.appendChild(meta));

        [...document.head.querySelectorAll("meta[itemprop='state']")]
            .forEach(meta => element.appendChild(meta));

        [...document.head.querySelectorAll("meta[itemprop='antiforgery']")]
            .forEach(meta => element.appendChild(meta));
    }

    static #updateHeadContent(target, content, marker) {
        target.querySelectorAll(`[srx-fragment='${marker}']`).forEach(i => i.remove());

        for (const element of [...content.children]) {
            element.setAttribute("srx-fragment", marker);
            target.insertAdjacentElement("afterbegin", element);
        }
    }

    static #placeContentIntoFragment(target, content) {
        target.innerHTML = content.innerHTML;

        for (const script of [...target.querySelectorAll("script")]) {
            const duplicatedScript = document.createElement("script");

            for (const attribute of [...script.attributes]) {
                duplicatedScript.setAttribute(attribute.name, attribute.value);
            }

            duplicatedScript.textContent = script.textContent;
            duplicatedScript.async = false;

            script.replaceWith(duplicatedScript);
        }
    }

    static #registerEventHandler(targetElement, element, event) {
        try {
            const resolvedElement = ReactiveFragment.#resolveElement(targetElement, element);
            resolvedElement.setAttribute("_srx-path", element);

            const eventName = event.replace(/^on/, "");
            resolvedElement.addEventListener(eventName, handleInteraction);
        } catch {
            console.error("Can't resolve '" + element + "' in " + targetElement);
        }
    }

    static #resolveElement(targetElement, path) {
        let element = targetElement;

        for (const segment of path.split("/")) {
            if (segment === "") {
                continue;
            }

            if (segment.startsWith("#")) {
                element = element.querySelector(segment);
                continue;
            }

            const match = segment.match(/^(?<tag>[^\[]+)(\[(?<index>\d+)])?$/);
            const tagName = match.groups["tag"];

            if (match.groups["index"]) {
                const index = Number.parseInt(match.groups["index"]);
                element = element.querySelector(`& > ${tagName}:nth-of-type(${index + 1})`);
            } else {
                element = element.querySelector(`& > ${tagName}`);
            }
        }

        return element;
    }
}

async function* iterateChunks(reader, boundary) {
    const decoder = new TextDecoder();

    while (true) {
        const chunk = await reader.read();
        if (chunk.done) {
            return;
        }

        for (const part of decoder.decode(chunk.value).split(boundary)) {
            if (part.length !== 0 && part !== "\n")
            {
                yield part;
            }
        }
    }
}

// --------- //
// - setup - //
// --------- //

const fragments = document.querySelectorAll(fragmentSelector);
const prerenderedState = consumePrerenderedState();

for (const fragment of fragments) {
    setupFragment(fragment, prerenderedState);
}

// --------- //

// The following is taken from https://raw.githubusercontent.com/dotnet/aspnetcore/2edb8c84fa5e64ff7877f11e0f181f0419c039c9/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts
// Copyright found in original file:
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

const eventTypeRegistry = new Map();

function getEventObject(eventName) {
    return eventTypeRegistry.get(eventName);
}
function configureEvent(eventNames, transformer) {
    eventNames.forEach(eventName => eventTypeRegistry.set(eventName, transformer));
}

configureEvent(['input', 'change'], parseChangeEvent);
function parseChangeEvent(event) {
    const element = event.target;

    if (isTimeBasedInput(element)) {
        const normalizedValue = normalizeTimeBasedValue(element);

        return { value: normalizedValue };
    } else if (isMultipleSelectInput(element)) {
        const selectedValues = Array.from(element.options)
            .filter(option => option.selected)
            .map(option => option.value);

        return { value: selectedValues };
    } else {
        const targetIsCheckbox = isCheckbox(element);
        const newValue = targetIsCheckbox ? !!element['checked'] : element['value'];

        return { value: newValue };
    }

    function isCheckbox(element) {
        return !!element && element.tagName === 'INPUT' && element.getAttribute('type') === 'checkbox';
    }

    function isTimeBasedInput(element) {
        const timeBasedInputs = ['date', 'datetime-local', 'month', 'time', 'week'];
        return timeBasedInputs.indexOf(element.getAttribute('type')) !== -1;
    }

    function isMultipleSelectInput(element) {
        return element instanceof HTMLSelectElement && element.type === 'select-multiple';
    }

    function normalizeTimeBasedValue(element) {
        const value = element.value;
        const type = element.type;
        switch (type) {
            case 'date':
            case 'month':
                return value;
            case 'datetime-local':
                return value.length === 16 ? value + ':00' : value; // Convert yyyy-MM-ddTHH:mm to yyyy-MM-ddTHH:mm:00
            case 'time':
                return value.length === 5 ? value + ':00' : value; // Convert hh:mm to hh:mm:00
            case 'week':
                // For now we are not going to normalize input type week as it is not trivial
                return value;
        }

        throw new Error(`Invalid element type '${type}'.`);
    }
}

configureEvent(['copy', 'cut','paste'], parseClipboardEvent);
function parseClipboardEvent(event) {
    return { type: event.type };
}

configureEvent(['drag', 'dragend', 'dragenter', 'dragleave', 'dragover', 'dragstart', 'drop'], parseDragEvent);
function parseDragEvent(event) {
    return {
        ...parseMouseEvent(event),
        dataTransfer: event.dataTransfer ? {
            dropEffect: event.dataTransfer.dropEffect,
            effectAllowed: event.dataTransfer.effectAllowed,
            files: Array.from(event.dataTransfer.files).map(f => f.name),
            items: Array.from(event.dataTransfer.items).map(i => ({ kind: i.kind, type: i.type })),
            types: event.dataTransfer.types
        } : null
    };
}

configureEvent(['focus', 'blur', 'focusin', 'focusout'], parseFocusEvent);
function parseFocusEvent(event) {
    return { type: event.type };
}

configureEvent(['keydown', 'keyup', 'keypress'], parseKeyboardEvent);
function parseKeyboardEvent(event) {
    return {
        key: event.key,
        code: event.code,
        location: event.location,
        repeat: event.repeat,
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
        type: event.type,
        isComposing: event.isComposing,
    };
}

configureEvent(['contextmenu', 'click', 'mouseover', 'mouseout', 'mousemove', 'mousedown', 'mouseup', 'mouseleave', 'mouseenter', 'dblclick'], parseMouseEvent);
function parseMouseEvent(event) {
    return {
        detail: event.detail,
        screenX: event.screenX,
        screenY: event.screenY,
        clientX: event.clientX,
        clientY: event.clientY,
        offsetX: event.offsetX,
        offsetY: event.offsetY,
        pageX: event.pageX,
        pageY: event.pageY,
        movementX: event.movementX,
        movementY: event.movementY,
        button: event.button,
        buttons: event.buttons,
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
        type: event.type,
    };
}

configureEvent(['error'], parseErrorEvent);
function parseErrorEvent(event) {
    return {
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        type: event.type,
    };
}

configureEvent(['loadstart', 'timeout', 'abort', 'load', 'loadend', 'progress'], parseProgressEvent);
function parseProgressEvent(event) {
    return {
        lengthComputable: event.lengthComputable,
        loaded: event.loaded,
        total: event.total,
        type: event.type,
    };
}

configureEvent(['touchcancel', 'touchend', 'touchmove', 'touchenter', 'touchleave', 'touchstart'], parseTouchEvent);
function parseTouchEvent(event) {
    return {
        detail: event.detail,
        touches: parseTouch(event.touches),
        targetTouches: parseTouch(event.targetTouches),
        changedTouches: parseTouch(event.changedTouches),
        ctrlKey: event.ctrlKey,
        shiftKey: event.shiftKey,
        altKey: event.altKey,
        metaKey: event.metaKey,
        type: event.type,
    };

    function parseTouch(touchList) {
        const touches = [];

        for (let i = 0; i < touchList.length; i++) {
            const touch = touchList[i];
            touches.push({
                identifier: touch.identifier,
                clientX: touch.clientX,
                clientY: touch.clientY,
                screenX: touch.screenX,
                screenY: touch.screenY,
                pageX: touch.pageX,
                pageY: touch.pageY,
            });
        }
        return touches;
    }
}

configureEvent(['gotpointercapture', 'lostpointercapture', 'pointercancel', 'pointerdown', 'pointerenter', 'pointerleave', 'pointermove', 'pointerout', 'pointerover', 'pointerup'], parsePointerEvent);
function parsePointerEvent(event) {
    return {
        ...parseMouseEvent(event),
        pointerId: event.pointerId,
        width: event.width,
        height: event.height,
        pressure: event.pressure,
        tiltX: event.tiltX,
        tiltY: event.tiltY,
        pointerType: event.pointerType,
        isPrimary: event.isPrimary,
    };
}

configureEvent(['wheel', 'mousewheel'], parseWheelEvent);
function parseWheelEvent(event) {
    return {
        ...parseMouseEvent(event),
        deltaX: event.deltaX,
        deltaY: event.deltaY,
        deltaZ: event.deltaZ,
        deltaMode: event.deltaMode,
    };
}
