import { ReactiveFragment } from "./fragment";
import { RequestBasedTransport } from "./transport/request";

const fragments = document.querySelectorAll<HTMLElement>("[srx-fragment]");
for (const element of fragments) {
    const transport = new RequestBasedTransport();
    const fragment = new ReactiveFragment(element, transport);

    await fragment.initialize();
}
