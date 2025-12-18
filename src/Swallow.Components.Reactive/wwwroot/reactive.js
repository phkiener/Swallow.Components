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

                    return { content: content, redirect: undefined };

                case 204:
                    const location = response.headers.get("srx-redirect");
                    return { content: undefined, redirect: location };

                default:
                    console.error("srx request returned unhandled status code: " + response.status);
                    return { content: undefined, redirect: undefined };
            }
        } catch (error) {
            console.error("srx request failed: " + error);
            return { content: undefined, redirect: undefined };
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
            formData.append("_srx-event", triggeringEvent.event);
            formData.append("_srx-path", triggeringEvent.element);
        }

        for (const stateElement of [...targetElement.querySelectorAll("& > meta[itemprop='state']")]) {
            formData.append("_srx-state-" + stateElement.getAttribute("data-key"), stateElement.getAttribute("data-value"));
        }

        return formData;
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

        return { route: route, options: { method: "POST", body: formData, redirect: "manual" }};
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

            target.replaceChild(duplicatedScript, script);
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
        const dispatchInfo = { element: evnt.currentTarget.getAttribute("_srx-path"), event: "on" + evnt.type };

        evnt.preventDefault();
        evnt.stopPropagation();

        triggerInteraction(container, dispatchInfo);
    }
})(document.currentScript);
