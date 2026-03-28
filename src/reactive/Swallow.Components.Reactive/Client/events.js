// The following is taken from https://raw.githubusercontent.com/dotnet/aspnetcore/2edb8c84fa5e64ff7877f11e0f181f0419c039c9/src/Components/Web.JS/src/Rendering/Events/EventTypes.ts
// Copyright found in original file:
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

const eventTypeRegistry = new Map();

export function getEventObject(eventName) {
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
