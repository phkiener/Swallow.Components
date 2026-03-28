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
    getRoute(): string;
    handle(command: Response): Promise<void>;
}

export type Response = RenderCommand | RedirectCommand | Exception;
export type RenderCommand = { content: string, kind: "render" };
export type RedirectCommand = { location: string, kind: "redirect" };
export type Exception = { message: Error, kind: "error" };
