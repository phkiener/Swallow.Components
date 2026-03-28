import { Transport } from "./index";
import * as common from "../common";

export class RequestBasedTransport implements Transport {
    private fragment: common.Fragment | null = null;
    private queuedInteractions: common.Interaction[] | null = null;

    initialize(fragment: common.Fragment): Promise<void> {
        this.fragment = fragment;

        return this.sendInteractions([]);
    }

    async send(interaction: common.Interaction): Promise<void> {
        if (this.queuedInteractions) {
            this.queuedInteractions.push(interaction);
            return;
        }

        this.queuedInteractions = [];
        this.queuedInteractions.push(interaction);

        while (this.queuedInteractions.length > 0) {
            const currentBatch = [...this.queuedInteractions];
            this.queuedInteractions = [];

            try {
                await this.sendInteractions(currentBatch);
            } catch (error) {
                if (error instanceof Error) {
                    await this.fragment?.handle({ kind: "error", message: error });
                } else {
                    await this.fragment?.handle({ kind: "error", message: new Error(`Unhandled error ${error}`) });
                }
            }
        }

        this.queuedInteractions = null;
    }

    close(): Promise<void> {
        // Nothing to do.

        return Promise.resolve();
    }

    async sendInteractions(interactions: common.Interaction[]): Promise<void> {
        if (!this.fragment) {
            return;
        }

        const context = this.fragment.getContext();
        const formData = RequestBasedTransport.buildRequestBody(context, interactions);

        const response = await fetch(
            this.fragment.getRoute(),
            { method: "POST", body: formData, headers: { "srx-request": "true" } });

        if (response.headers.has("srx-response")) {
            throw new Error("srx-request not handled by matching endpoint.");
        }

        if (response.status === 200 && response.body) {
            const streamingBoundary = response.headers.get("srx-streaming-marker");
            if (streamingBoundary) {
                const iterator = iterateChunks(response.body, streamingBoundary);
                for await (const chunk of iterator) {
                    await this.fragment.handle({ kind: "render", content: chunk });
                }
            } else {
                const content = await response.text();
                await this.fragment.handle({ kind: "render", content: content });
            }
        }

        if (response.status === 204) {
            const location = response.headers.get("srx-redirect");
            if (location) {
                await this.fragment.handle({ kind: "redirect", location: location });
                return;
            }
        }

        if (response.status === 500) {
            const errorMessage = await response.text();
            throw new Error(errorMessage);
        }

        throw new Error(`Unexpected status code: ${response.status}`);
    }

    private static buildRequestBody(context: common.Context, interactions: common.Interaction[]): FormData {
        const formData = new FormData();

        if (context.antiforgery) {
            formData.append(context.antiforgery.name, context.antiforgery.token);
        }

        for (const key in context.parameters) {
            formData.append(`_srx-parameter-${key}`, context.parameters[key]);
        }

        for (const key in context.state) {
            formData.append(`_srx-state-${key}`, context.parameters[key]);
        }

        for (const interaction of interactions) {
            formData.append("_srx-event", JSON.stringify(interaction));
        }

        return formData;
    }
}

async function* iterateChunks(stream: ReadableStream, boundary: string) {
    const reader = stream.pipeThrough(new TextDecoderStream()).getReader();

    while (true) {
        const chunk = await reader.read();
        if (chunk.done) {
            return;
        }

        for (const part of chunk.value.split(boundary)) {
            if (part.length !== 0 && part !== "\n")
            {
                yield part;
            }
        }
    }
}
