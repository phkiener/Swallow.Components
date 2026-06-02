import tabs from "./layout/tabs";

export function register(element: HTMLElement) {
    tabs(element);
}

document.addEventListener("DOMContentLoaded", () => register(document.body));
