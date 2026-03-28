export interface Interaction {
    trigger: string;
    eventName: string;
    eventBody: object | null;
}

export interface Context {
    route: Location,
    antiforgery?: { name: string, token: string },
    parameters: { [key: string]: string },
    state: { [key: string]: string },
}

export interface Fragment {
    getContext(): Context;
    handle(command: Command): Promise<void>;
}

export type Command = RenderCommand | RedirectCommand;
export type RenderCommand = { content: string, kind: "render" };
export type RedirectCommand = { location: string, kind: "redirect" };
