const supportedAttributes = ["for"];

(() => processSubtree(document.body))();

function processSubtree(element) {
    for (const elegibleElement of [...element.querySelectorAll("[idref]")]) {
        for (const attribute of supportedAttributes) {
            const attributeValue = element.getAttribute(attribute);
            if (attributeValue == null) {
                return;
            }

            const target = findTargetElement(element, attributeValue);
            if (target.id == null || target.id === "") {
                target.id = "genid-" + crypto.randomUUID();
            }

            element.setAttribute(attribute, target.id);
        }

        elegibleElement.removeAttribute("idref");
    }
}

function findTargetElement(element, selector) {
    const selectorMatch = selector.match(/^(find|next|previous|closest) (.+)+$/i);
    if (selectorMatch == null) {
        return null;
    }

    const selectorType = selectorMatch[1];
    const parsedSelector = selectorMatch[2];

    switch (selectorType) {
        case "find":
            return document.querySelector(parsedSelector);

        case "closest":
            return element.closest(parsedSelector);

        case "next":
            let next = element.nextElementSibling;
            while (next) {
                if (next.matches(parsedSelector)) {
                    return next;
                }

                next = next.nextElementSibling;
            }
            break;

        case "previous":
            let previous = element.nextElementSibling;
            while (previous) {
                if (previous.matches(parsedSelector)) {
                    return previous;
                }

                previous = previous.nextElementSibling;
            }
            break;

    }

    return null;
}
