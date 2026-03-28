import { getEventObject } from "./events.js"
import { Transport } from "./transport";
import * as common from "./common";

function triggerInteraction(event: Event): Promise<void> {
    if (!event.target || !(event.target instanceof HTMLElement)) {
        return Promise.resolve();
    }

    const fragmentElement = event.target.closest("[srx-fragment]");
    const fragment = fragmentElement ? (fragmentElement as any)["fragment"] as ReactiveFragment : null;
    const trigger = event.target.getAttribute("_srx-path");

    if (fragment && trigger) {
        event.stopPropagation();
        event.preventDefault();

        return fragment.trigger(trigger, event);
    }

    return Promise.resolve();
}

export class ReactiveFragment implements common.Fragment {
    private readonly element: HTMLElement;
    private readonly transport: Transport;

    constructor(element: HTMLElement, transport: Transport) {
        this.element = element;
        this.transport = transport;

        (this.element as any)["fragment"] = this;
    }

    public async initialize(): Promise<void> {
        return this.transport.initialize(this);
    }

    public trigger(name: string, event: Event): Promise<void> {
        const transformer = getEventObject(event.type);
        const interaction = {
            trigger: name,
            eventName: `on${event.type}`,
            eventBody: transformer ? transformer(event) : null
        };

        return this.transport.send(interaction);
    }

    public getRoute(): string {
        const route = this.element.getAttribute("srx-route");
        if (!route) {
            throw new Error(`Fragment ${this.element.id} is missing the 'srx-route' attribute`);
        }

        return route;
    }

    public getContext(): common.Context {
        const context: common.Context = {
            route: document.location,
            antiforgery: undefined,
            parameters: {},
            state: {}
        };

        const antiforgeryElement = this.element.querySelector("& > meta[itemprop='antiforgery'][data-name][data-token]");
        if (antiforgeryElement) {
            context.antiforgery = {
                name: antiforgeryElement.getAttribute("data-name")!,
                token: antiforgeryElement.getAttribute("data-token")!
            };
        }

        for (const parameterElement of [...this.element.querySelectorAll("& > meta[itemprop='parameter'][data-key][data-value]")]) {
            const key: string = parameterElement.getAttribute("data-key")!;
            context.parameters[key] = parameterElement.getAttribute("data-value")!;
        }

        for (const stateElement of [...this.element.querySelectorAll("& > meta[itemprop='state'][data-key][data-value]")]) {
            const key: string = stateElement.getAttribute("data-key")!;
            context.parameters[key] = stateElement.getAttribute("data-value")!;
        }

        return context;
    }

    public async handle(command: common.Response): Promise<void> {
        if (command.kind === "render") {
            this.swapContent(command.content);
            return;
        }

        if (command.kind === "redirect") {
            await this.transport.close();
            window.location.href = decodeURI(command.location);
        }
    }

    private swapContent(content: string): void {
        const parser = new DOMParser();
        const document = parser.parseFromString(content, "text/html");

        const headContent = document.body.querySelector<HTMLElement>(".srx-head-content");
        const mainContent = document.body.querySelector<HTMLElement>(".srx-content");

        this.updateHeadContent(window.document.head, headContent);
        this.placeContentIntoFragment(mainContent);

        [...document.head.querySelectorAll("meta[itemprop='event-handler'][data-element][data-event]")]
            .forEach(meta => this.registerEventHandler(meta.getAttribute("data-element")!, meta.getAttribute("data-event")!));

        [...document.head.querySelectorAll("meta[itemprop='parameter']")]
            .forEach(meta => this.element.appendChild(meta));

        [...document.head.querySelectorAll("meta[itemprop='state']")]
            .forEach(meta => this.element.appendChild(meta));

        [...document.head.querySelectorAll("meta[itemprop='antiforgery']")]
            .forEach(meta => this.element.appendChild(meta));
    }

    private updateHeadContent(target: HTMLElement, content: HTMLElement | null): void {
        const marker = this.element.id;
        target.querySelectorAll(`[srx-fragment='${this.element.id}']`).forEach(i => i.remove());

        if (!content) {
            return;
        }

        for (const element of [...content.children]) {
            element.setAttribute("srx-fragment", marker);
            target.insertAdjacentElement("afterbegin", element);
        }
    }

    private placeContentIntoFragment(content: HTMLElement | null): void {
        if (!content) {
            this.element.innerHTML = "";
            return;
        }

        // TODO: morph instead of swap
        this.element.innerHTML = content.innerHTML;

        for (const script of [...this.element.querySelectorAll<HTMLScriptElement>("script")]) {
            const duplicatedScript = document.createElement("script");

            for (const attribute of [...script.attributes]) {
                duplicatedScript.setAttribute(attribute.name, attribute.value);
            }

            duplicatedScript.textContent = script.textContent;
            duplicatedScript.async = false;

            script.replaceWith(duplicatedScript);
        }
    }

    private registerEventHandler(path: string, eventName: string) {
        try {
            const resolvedElement = this.resolveElement(path);
            if (!resolvedElement) {
                console.error("Can't resolve '" + path + "' in " + this.element.id);
                return;
            }

            resolvedElement.setAttribute("_srx-path", path);
            resolvedElement.addEventListener(eventName.replace(/^on/, ""), triggerInteraction);
        } catch {
            console.error("Can't resolve '" + path + "' in " + this.element.id);
        }
    }

    private resolveElement(path: string): HTMLElement | null {
        let element: HTMLElement | null | undefined = this.element;

        for (const segment of path.split("/")) {
            if (!element) {
                return null;
            }

            if (segment === "") {
                continue;
            }

            if (segment.startsWith("#")) {
                element = element.querySelector<HTMLElement>(segment);
                continue;
            }

            const match = segment.match(/^(?<tag>[^\[]+)(\[(?<index>\d+)])?$/);
            if (!match || !match.groups) {
                return null;
            }

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
