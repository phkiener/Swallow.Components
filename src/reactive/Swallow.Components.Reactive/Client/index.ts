import { ReactiveFragment } from "./fragment";
import { Transport } from "./transport";
import * as common from "./common";

class DummyTransport implements Transport {
    close(): Promise<void> {
        return Promise.resolve();
    }

    initialize(fragment: common.Fragment): Promise<void> {
        return Promise.resolve();
    }

    send(interaction: common.Interaction): Promise<void> {
        return Promise.resolve();
    }
}

const fragments = document.querySelectorAll<HTMLElement>("[srx-fragment]");
for (const element of fragments) {
    const transport: Transport = new DummyTransport();
    const fragment = new ReactiveFragment(element, transport);

    await fragment.initialize();
}
