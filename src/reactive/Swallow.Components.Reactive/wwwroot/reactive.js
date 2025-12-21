'use strict';

(async scriptTag => {
    const reactiveFragment = scriptTag.previousElementSibling;

    await triggerInteraction(reactiveFragment, null);
    scriptTag.remove();

    async function triggerInteraction(targetElement, triggeringEvent) {
        const response = await fetchResponse(targetElement, triggeringEvent);

        if (response.redirect) {
            window.location = decodeURI(response.redirect);
        }

        if (response.content) {
            applyResponse(targetElement, response.content);
        }

        if (response.error) {
            targetElement.setAttribute("_srx-error", "");
            targetElement.querySelector("& > .srx-error").innerText = response.error;
        }
    }

    async function fetchResponse(targetElement, triggeringEvent) {
        const formData = buildForm(targetElement, triggeringEvent);
        const route = targetElement.getAttribute("_srx-route");

        try {
            const response = await fetch(route, { method: "POST", body: formData });
            if (response.headers.get("srx-response") !== "true") {
                console.error("srx request was not handled by correct endpoint");
                return { content: undefined, redirect: undefined };
            }

            switch (response.status) {
                case 200:
                    const content = await response.text();

                    return { content: content, redirect: undefined, error: undefined };

                case 204:
                    const location = response.headers.get("srx-redirect");
                    return { content: undefined, redirect: location, error: undefined };

                case 500:
                    const errorMessage = await response.text();
                    return { content: undefined, redirect: undefined, error: errorMessage };

                default:
                    console.error("srx request returned unhandled status code: " + response.status);
                    return { content: undefined, redirect: undefined };
            }
        } catch (error) {
            console.error("srx request failed: " + error);
            return { content: undefined, redirect: undefined, error: undefined };
        }
    }

    function buildForm(targetElement, triggeringEvent) {
        const antiforgeryName = targetElement.getAttribute("_srx-antiforgery-name");
        const antiforgeryToken = targetElement.getAttribute("_srx_antiforgery-token");

        const formData = new FormData();
        if (antiforgeryName && antiforgeryToken) {
            formData.append(antiforgeryName, antiforgeryToken);
        }

        if (triggeringEvent) {
            formData.append("_srx-event", "on" + triggeringEvent.event.type);
            formData.append("_srx-path", triggeringEvent.element);

            const transformer = getEventObject(triggeringEvent.event.type);
            if (transformer) {
                formData.append("_srx-event-body", JSON.stringify(transformer(triggeringEvent.event)));
            }
        }

        for (const parameterElement of [...targetElement.querySelectorAll("& > meta[itemprop='parameter']")]) {
            formData.append("_srx-parameter-" + parameterElement.getAttribute("data-key"), parameterElement.getAttribute("data-value"));
        }

        for (const stateElement of [...targetElement.querySelectorAll("& > meta[itemprop='state']")]) {
            formData.append("_srx-state-" + stateElement.getAttribute("data-key"), stateElement.getAttribute("data-value"));
        }

        return formData;
    }

    function applyResponse(targetElement, content) {
        const parser = new DOMParser();
        const document = parser.parseFromString(content, "text/html");

        placeContentIntoFragment(targetElement, document.body);

        [...document.head.querySelectorAll("meta[itemprop='event-handler']")]
            .forEach(meta => registerEventHandler(targetElement, meta.getAttribute("data-element"), meta.getAttribute("data-event")));

        [...document.head.querySelectorAll("meta[itemprop='parameter']")]
            .forEach(meta => targetElement.appendChild(meta));

        [...document.head.querySelectorAll("meta[itemprop='state']")]
            .forEach(meta => targetElement.appendChild(meta));
    }

    function placeContentIntoFragment(target, content) {
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

    function registerEventHandler(targetElement, element, event) {
        try {
            const resolvedElement = resolveElement(targetElement, element);
            resolvedElement.setAttribute("_srx-path", element);

            const eventName = event.replace(/^on/, "");
            resolvedElement.addEventListener(eventName, onReactiveElementTriggered);
        } catch {
            console.error("Can't resolve '" + element + "' in " + targetElement);
        }
    }

    function resolveElement(targetElement, path) {
        let element = targetElement.querySelector("& > .srx-content");
        for (const segment of path.split("/")) {
            if (segment === "") {
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

    function onReactiveElementTriggered(evnt) {
        const container = evnt.currentTarget.closest("[_srx-fragment]");
        const dispatchInfo = { element: evnt.currentTarget.getAttribute("_srx-path"), event: evnt };

        evnt.preventDefault();
        evnt.stopPropagation();

        triggerInteraction(container, dispatchInfo);
    }

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
})(document.currentScript);
