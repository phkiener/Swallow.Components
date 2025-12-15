'use strict';

(async scriptTag => {
    const reactiveFragment = scriptTag.previousElementSibling;

    await triggerInteraction(reactiveFragment, null);
    scriptTag.remove();

    async function triggerInteraction(targetElement, triggeringEvent) {
        const request = buildRequest(targetElement, triggeringEvent);
        const response = await fetch(request.route, request.options);

        switch (response.status) {
            case 200:
                const content = await response.text();
                applyResponse(targetElement, content);

                break;

            default:
                console.error("srx request returned non-200 status code: " + response.status);
                break;
        }
    }

    function buildRequest(targetElement, triggeringEvent) {
        const route = targetElement.getAttribute("_srx-route");

        const antiforgeryName = targetElement.getAttribute("_srx-antiforgery-name");
        const antiforgeryToken = targetElement.getAttribute("_srx_antiforgery-token");

        const formData = new FormData();
        if (antiforgeryName && antiforgeryToken) {
            formData.append(antiforgeryName, antiforgeryToken);
        }

        if (triggeringEvent) {
            formData.append("_srx-event", triggeringEvent.event);
            formData.append("_srx-path", triggeringEvent.element);
        }

        for (const stateElement of [...targetElement.querySelectorAll("& > meta[itemprop='state']")]) {
            formData.append("_srx-state-" + stateElement.getAttribute("data-key"), stateElement.getAttribute("data-value"));
        }

        return { route: route, options: { method: "POST", body: formData }};
    }

    function applyResponse(targetElement, content) {
        const parser = new DOMParser();
        const document = parser.parseFromString(content, "text/html");

        placeContentIntoFragment(targetElement, document.body);

        [...document.head.querySelectorAll("meta[itemprop='event-handler']")]
            .forEach(meta => registerEventHandler(targetElement, meta.getAttribute("data-element"), meta.getAttribute("data-event")));

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

            target.replaceChild(script, duplicatedScript);
        }
    }

    function registerEventHandler(targetElement, element, event) {
        try {
            const resolvedElement = resolveElement(targetElement, element);
            resolvedElement.setAttribute("_srx-path", element);
            resolvedElement.setAttribute("_srx-listener", event);

            const eventName = event.replace(/^on/, "");
            resolvedElement.addEventListener(eventName, onReactiveElementTriggered);
        } catch {
            console.error("Can't resolve '" + element + "' in " + targetElement);
        }
    }

    function resolveElement(targetElement, path) {
        let element = targetElement;
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
        const dispatchInfo = { element: evnt.currentTarget.getAttribute("_srx-path"), event: evnt.currentTarget.getAttribute("_srx-listener") };

        evnt.preventDefault();
        evnt.stopPropagation();

        triggerInteraction(container, dispatchInfo);
    }
})(document.currentScript);
